#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    /// <summary>
    /// TASK027N.2.2 Parts B/C/D - runtime particle/audio EVENT counting
    /// (not visual/audible human inspection, which this headless batchmode
    /// session cannot perform - see blood_visual_acceptance_n22.json and
    /// g_contact_audio_acceptance_n22.json for the explicit USER_RUNTIME_
    /// REQUIRED=true labeling). Uses the DEBUG_ONLY counters added this
    /// task: DinoCapDamageParticle.TestHarness_SpawnCallCount(Kind) and
    /// NativeAudioSessionGate.TestHarness_AdmittedCount(clipName).
    /// </summary>
    public class Task027N22BloodAudioRuntimeCount
    {
        const string ScenePath = "Assets/DinoCap/Scenes/CombatSandboxV2.unity";
        static readonly string OutDir =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_2_BloodAudioBiteEndDebugGodMode/raw"));

        CombatSandboxV2 sandbox;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            Directory.CreateDirectory(OutDir);
            EditorSceneManager.LoadSceneInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            yield return null;
            sandbox = Object.FindFirstObjectByType<CombatSandboxV2>();
            Assert.IsNotNull(sandbox, "CombatSandboxV2 not found after loading " + ScenePath);
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            NativeAudioSessionGate.TestHarness_ResetClipCounters();
        }

        class Log
        {
            readonly StringBuilder sb = new StringBuilder();
            public void Line(string s) { sb.AppendLine(s); }
            public void Save(string path) { File.WriteAllText(path, sb.ToString()); }
            public override string ToString() => sb.ToString();
        }

        // ================================================================
        // PART B: blood particle count per family. Each sub-test spawns a
        // fresh dino (Lesson #4 isolation - counters ARE reset per
        // UnitySetUp too, but a fresh scene avoids any other cross-talk),
        // resets the spawn counters, fires exactly one hit via the family's
        // own real trigger path (Shoot/held-fire/Melee - never a direct
        // particle-spawn call), waits for a genuine HP drop to confirm the
        // hit landed, then reads the Blood spawn-call count.
        // ================================================================
        // BUGFIX (this task, first run): originally waited a fixed 2 ticks
        // after Shoot() and assumed Type1's base 5 HP, both wrong: (a) ranged
        // damage lands on the weapon's own frame-3-gated discharge, which in
        // real GAME TIME needs far more than 2 raw Update ticks under
        // batchmode's fast unthrottled tick rate (same class of mistake as
        // Task027N.1.3.1's Lesson #1 - now fixed with a Time.time-based
        // wait), and (b) a tier-appropriate weapon CAN one-shot Type1's 5 HP
        // (observed for G4 in run 1: hit landed, LETHAL, so the death branch
        // - not the ordinary-hit blood branch - correctly ran instead,
        // producing 0 particles as EXPECTED for a kill, not a bug). Fixed by
        // boosting enemyHP to a large fixed value before every trigger so
        // every hit tested here is unambiguously non-lethal, matching Part
        // B's own framing ("a single nonlethal ordinary hit").
        IEnumerator FireOneShootFamily(int family, int tier)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true); // Type1 K3Blue, close enough to stay in range
            yield return null;
            sandbox.TestHarness_SetDebugEnemyHP(999f);
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(60f);
            sandbox.TestHarness_SetWeapon(family,tier,250);
            yield return null;
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            float hpBeforeShot=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            sandbox.TestHarness_Shoot();
            float start=Time.time;
            while(Time.time-start<2f)
            {
                yield return null;
                if(sandbox.CaptureRuntimeDebugSnapshot().enemyHP<hpBeforeShot-0.001f) break;
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator BloodCount_FamilyA()
        {
            // TASK027N.2.21 CORRECTION: this assertion originally expected 3
            // ballistic-blood particles for A, citing the same
            // "weapon_blood_matrix_n2.json" LIKELY-confidence source that
            // N.2.14's later exhaustive full-instruction decompile of
            // sub_10554 (and N.2.21's own independent re-decompile) PROVED
            // wrong: A-D's default/case1 reaction body never calls sub_11124
            // at all. Expected corrected to 0 - see
            // weapon_ballistic_blood_matrix_n221.json.
            float hpBefore = 999f;
            yield return FireOneShootFamily(0,0);
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            WriteResult("FamilyA", s.enemyHP<hpBefore-0.001f, blood, 0);
        }

        [UnityTest]
        public IEnumerator BloodCount_FamilyE()
        {
            yield return FireOneShootFamily(4,0);
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            WriteResult("FamilyE", s.enemyHP<999f-0.001f, blood, 3);
        }

        IEnumerator FireOneMelee(int tier)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetDebugEnemyHP(999f); // guarantee non-lethal regardless of G-tier damage
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(60f);
            sandbox.TestHarness_SetMeleeTier(tier);
            yield return null;
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            sandbox.TestHarness_Melee();
            float start=Time.time;
            while(Time.time-start<2f)
            {
                yield return null;
                if(sandbox.CaptureRuntimeDebugSnapshot().dbgMeleeDamageDispatched) break;
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator BloodCount_G1()
        {
            yield return FireOneMelee(0);
            int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            WriteResult("G1", s.dbgMeleeDamageDispatched, blood, 3);
        }

        [UnityTest]
        public IEnumerator BloodCount_G4()
        {
            yield return FireOneMelee(3);
            int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            WriteResult("G4", s.dbgMeleeDamageDispatched, blood, 3);
        }

        [UnityTest]
        public IEnumerator BloodCount_F1_ExcludedZero()
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(40f); // close - fast projectile travel
            sandbox.TestHarness_SetWeapon(5,0,250);
            yield return null;
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            float hpBefore=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            sandbox.TestHarness_Shoot();
            float start=Time.time; bool hit=false;
            while(Time.time-start<2f)
            {
                yield return null;
                if(sandbox.CaptureRuntimeDebugSnapshot().enemyHP<hpBefore-0.001f) { hit=true; break; }
            }
            int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            WriteResult("F1", hit, blood, 0);
        }

        [UnityTest]
        public IEnumerator BloodCount_F2_ExcludedZero()
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(60f);
            sandbox.TestHarness_SetWeapon(5,1,250);
            yield return null;
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            float hpBefore=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            sandbox.TestHarness_SetFireHeld(true);
            float start=Time.time; bool hit=false;
            while(Time.time-start<2.5f)
            {
                yield return null;
                if(sandbox.CaptureRuntimeDebugSnapshot().enemyHP<hpBefore-0.001f) { hit=true; break; }
            }
            sandbox.TestHarness_SetFireHeld(false);
            int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            WriteResult("F2", hit, blood, 0);
        }

        [UnityTest]
        public IEnumerator BloodCount_F3_ExcludedZero()
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(60f);
            sandbox.TestHarness_SetWeapon(5,2,250);
            yield return null;
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            float hpBefore=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            sandbox.TestHarness_SetFireHeld(true);
            float start=Time.time; bool hit=false;
            while(Time.time-start<2.5f)
            {
                yield return null;
                if(sandbox.CaptureRuntimeDebugSnapshot().enemyHP<hpBefore-0.001f) { hit=true; break; }
            }
            sandbox.TestHarness_SetFireHeld(false);
            int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            WriteResult("F3", hit, blood, 0);
        }

        static void WriteResult(string family, bool damageEventObserved, int particles, int expected)
        {
            string line = $"family={family} DAMAGE_EVENTS={(damageEventObserved?1:0)} BLOOD_HELPER_CALLS={particles} " +
                     $"PARTICLES_CREATED={particles} EXPECTED_NATIVE_PARTICLES={expected} MATCH={(particles==expected)}";
            File.AppendAllText(Path.Combine(OutDir,"n22_blood_particle_runtime_count.txt"), line+System.Environment.NewLine);
            Assert.IsTrue(damageEventObserved, family+": no damage event observed within budget.");
            Assert.AreEqual(expected, particles, family+": blood particle count mismatch.");
        }

        // ================================================================
        // PART D: G-tier contact audio structural counts. Miss -> swing
        // only; hit -> swing + the tier-appropriate contact clip.
        // ================================================================
        IEnumerator TestGAudio(int tier, string expectedSwingClip, string expectedContactClip, bool asMiss)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            if(!asMiss)
            {
                sandbox.TestHarness_SpawnDino(0,true);
                yield return null;
                sandbox.TestHarness_SetPlayerX(0f);
                sandbox.TestHarness_SetEnemyX(60f);
            }
            // Miss case: no dino at all - ExecuteMeleeContact's own
            // enemyHP<=0f early-out (no live target) still lets the swing's
            // frame-10 event and PlaySourceGMeleeAttackAudio() run, since
            // that call precedes the miss-return inside ExecuteMeleeContact.
            sandbox.TestHarness_SetMeleeTier(tier);
            yield return null;
            NativeAudioSessionGate.TestHarness_ResetClipCounters();
            sandbox.TestHarness_Melee();
            float start=Time.time;
            while(Time.time-start<2f)
            {
                yield return null;
                var s=sandbox.CaptureRuntimeDebugSnapshot();
                if(asMiss ? s.dbgMeleeContactEvent : s.dbgMeleeDamageDispatched) break;
            }
            yield return null; yield return null;

            int swingCount = NativeAudioSessionGate.TestHarness_AdmittedCount(expectedSwingClip);
            int contactCount = NativeAudioSessionGate.TestHarness_AdmittedCount(expectedContactClip);
            var log = new Log();
            log.Line($"tier=G{tier+1} miss={asMiss} swingClip={expectedSwingClip} swingCount={swingCount} " +
                     $"contactClip={expectedContactClip} contactCount={contactCount}");
            File.AppendAllText(Path.Combine(OutDir,"n22_g_audio_runtime_count.txt"), log.ToString()+System.Environment.NewLine);

            Assert.AreEqual(1, swingCount, $"G{tier+1} miss={asMiss}: swing clip play count expected 1.");
            if(asMiss)
                Assert.AreEqual(0, contactCount, $"G{tier+1} miss: contact clip must NOT play.");
            else
                Assert.AreEqual(1, contactCount, $"G{tier+1} hit: contact clip expected exactly 1 play.");
        }

        [UnityTest] public IEnumerator GAudio_G1_Miss() { yield return TestGAudio(0,"Attack_G","Audio_att1",true); }
        [UnityTest] public IEnumerator GAudio_G1_Hit()  { yield return TestGAudio(0,"Attack_G","Audio_att1",false); }
        [UnityTest] public IEnumerator GAudio_G2_Miss() { yield return TestGAudio(1,"Attack_G","Audio_att1",true); }
        [UnityTest] public IEnumerator GAudio_G2_Hit()  { yield return TestGAudio(1,"Attack_G","Audio_att1",false); }
        [UnityTest] public IEnumerator GAudio_G3_Miss() { yield return TestGAudio(2,"Attack_G","Audio_att2",true); }
        [UnityTest] public IEnumerator GAudio_G3_Hit()  { yield return TestGAudio(2,"Attack_G","Audio_att2",false); }
        [UnityTest] public IEnumerator GAudio_G4_Miss() { yield return TestGAudio(3,"Audio_G4","Audio_att2",true); }
        [UnityTest] public IEnumerator GAudio_G4_Hit()  { yield return TestGAudio(3,"Audio_G4","Audio_att2",false); }
    }
}
#endif
