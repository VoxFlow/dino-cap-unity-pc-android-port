#if UNITY_EDITOR
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    /// <summary>
    /// TASK027N.2.21 Part P focused acceptance tests. Covers:
    /// 1/2. one A/D hit -> exactly one visible qx burst (root cause: the old
    ///      SpawnLightImpact() redundant WeaponEffectAnimation-pooled spawn,
    ///      REMOVED this task - see qiangxue_spawn_ownership_n221.json).
    /// 3. A/B/C/D -> zero ballistic blood (sub_10554 default/case1 body
    ///    PROVEN to never call sub_11124 - see
    ///    weapon_ballistic_blood_matrix_n221.json).
    /// 4/5. G/E1 -> ballistic blood preserved (ApplyHeavyWeaponHit's own
    ///    gating was ALREADY correct, untouched this task).
    /// 6/7. Type1/Type2 bite -> ordinary knockback+stun suppressed, damage
    ///    still applied, bite not interrupted (BeginLightBefired's
    ///    enemyAttacking gate / ApplyHeavyWeaponHit's EnemyBiteCommitted
    ///    gate - ALREADY correctly implemented per N.1.2/N.1.3, re-verified
    ///    this task via direct sub_10554/sub_FEF0 decompile).
    /// 8. Type3 bite -> F2/F3 still apply their own special
    ///    stun/suppression even during bite (they are proven to sit outside
    ///    sub_10554's generic reaction dispatch entirely).
    /// 9. Type4 -> mechanically identical to Type1/Type2 (sub_10554 has NO
    ///    separate species-4 branch anywhere - PROVEN by direct reading of
    ///    the full decompiled function this task, not assumed by analogy).
    /// </summary>
    public class Task027N221QxDupBloodBiteRepro
    {
        const string ScenePath = "Assets/DinoCap/Scenes/CombatSandboxV2.unity";
        static readonly string OutDir =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_2_21_QiangxueSingleInstance_BloodFamily_BiteReactionClosure/raw"));

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
            DinoCapBloodBurst.ResetDebugCounts();
        }

        static void WriteLine(string file,string line) =>
            File.AppendAllText(Path.Combine(OutDir,file), line+System.Environment.NewLine);

        IEnumerator FireOneShootFamily(int family,int tier)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetDebugEnemyHP(999f);
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(60f);
            sandbox.TestHarness_SetWeapon(family,tier,250);
            yield return null;
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            DinoCapBloodBurst.ResetDebugCounts();
            float hpBefore=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;

            // C/D are native HELD-FIRE families (see UpdateWeaponFireInput's
            // semiAutoMode list, which excludes family 2/3) - a single
            // Shoot() press-edge never triggers them (this was the actual
            // cause of QxSpawnCount_OneD_ExactlyOne reading 0 instead of 1
            // the first time this test ran: the shot never fired at all).
            bool heldFire=family==2 || family==3;
            if(heldFire) sandbox.TestHarness_SetFireHeld(true);
            else sandbox.TestHarness_Shoot();

            float start=Time.time;
            while(Time.time-start<2f)
            {
                yield return null;
                if(sandbox.CaptureRuntimeDebugSnapshot().enemyHP<hpBefore-0.001f) break;
            }
            if(heldFire) sandbox.TestHarness_SetFireHeld(false);
            yield return null;
        }

        // ----------------------------------------------------------------
        // 1/2: one A/D hit -> exactly one visible qx burst.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator QxSpawnCount_OneA_ExactlyOne()
        {
            yield return FireOneShootFamily(0,0); // family A, tier1
            int qx = DinoCapBloodBurst.QxSpawnCount;
            WriteLine("n221_qx_spawn_count.txt","family=A qxSpawnCount="+qx);
            Assert.AreEqual(1, qx, "One non-lethal A hit must spawn exactly one qx burst.");
        }

        [UnityTest]
        public IEnumerator QxSpawnCount_OneD_ExactlyOne()
        {
            yield return FireOneShootFamily(3,0); // family D, tier1
            int qx = DinoCapBloodBurst.QxSpawnCount;
            WriteLine("n221_qx_spawn_count.txt","family=D qxSpawnCount="+qx);
            Assert.AreEqual(1, qx, "One non-lethal D hit must spawn exactly one qx burst.");
        }

        // ----------------------------------------------------------------
        // 3: A/B/C/D -> zero ballistic blood.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator BallisticBlood_ABCD_Zero()
        {
            for(int family=0; family<=3; family++)
            {
                yield return FireOneShootFamily(family,0);
                int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
                WriteLine("n221_ballistic_blood_abcd.txt","family="+(char)('A'+family)+" blood="+blood);
                Assert.AreEqual(0, blood,
                    "Family "+(char)('A'+family)+" must spawn zero ballistic blood (sub_10554 never calls sub_11124 for A-D).");
            }
        }

        // ----------------------------------------------------------------
        // 4/5: G / E1 -> ballistic blood preserved (already-correct path,
        // untouched - regression guard only).
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator BallisticBlood_G1_Preserved()
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetDebugEnemyHP(999f);
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(60f);
            sandbox.TestHarness_SetMeleeTier(0);
            yield return null;
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            sandbox.TestHarness_Melee();
            float start=Time.time;
            while(Time.time-start<2f)
            {
                yield return null;
                if(sandbox.CaptureRuntimeDebugSnapshot().dbgMeleeDamageDispatched) break;
            }
            int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            WriteLine("n221_ballistic_blood_preserved.txt","G1 blood="+blood);
            Assert.AreEqual(3, blood, "G1 must still spawn 3 ballistic-blood particles (untouched this task).");
        }

        [UnityTest]
        public IEnumerator BallisticBlood_E1_Preserved()
        {
            yield return FireOneShootFamily(4,0); // family E, tier1 = E1
            int blood = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            WriteLine("n221_ballistic_blood_preserved.txt","E1 blood="+blood);
            Assert.AreEqual(3, blood, "E1 must still spawn 3 ballistic-blood particles (untouched this task).");
        }

        // ----------------------------------------------------------------
        // 6/7: TASK027N.2.22 CORRECTED these assertions. N.2.21 asserted
        // ordinary G-melee knockback/stun were SUPPRESSED during a
        // committed bite for every species. The user directly corrected
        // this (TASK027N.2.22's own "CRITICAL USER CORRECTION"): for
        // Type1/Type2/Type4, ordinary melee ALWAYS fully interrupts the
        // bite (knockback + BehitedAnim + enemyAttacking->false), exactly
        // as it would outside a bite - confirmed by a fresh full
        // re-decompile of sub_10554's case6(G)/case4(E) bodies this task,
        // which found NO dino+0xAC (bite-flag) check anywhere in either
        // case's non-Type3 branch (that check exists only in the true
        // light-family cases). This is independently corroborated by
        // TASK027N.2's own native_melee_vs_bite_end_arbitration_n2.json
        // (native's own melee-eligibility/damage-application has no
        // reaction-state gate at all) and TASK027N.2.2's own live-measured
        // melee_exact_bite_boundary_matrix_n22.json (Type1/2/4 always
        // successfully land melee damage across a -3..+3 frame sweep
        // regardless of bite timing). See bite_commit_timeline_n222.json /
        // melee_vs_bite_native_window_n222.json for the full re-derivation.
        // ----------------------------------------------------------------
        IEnumerator BiteImmunityMelee(int speciesIndex,string label)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(speciesIndex,true);
            yield return null;
            sandbox.TestHarness_SetDebugEnemyHP(999f);
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(60f);
            sandbox.TestHarness_SetMeleeTier(3); // G4: largest knockback
            yield return null;
            sandbox.TestHarness_ForceEnemyAttack();
            yield return null;
            var before = sandbox.CaptureRuntimeDebugSnapshot();
            float hpBefore = before.enemyHP;

            sandbox.TestHarness_Melee();
            float start=Time.time; bool dispatched=false;
            while(Time.time-start<2f)
            {
                yield return null;
                if(sandbox.CaptureRuntimeDebugSnapshot().dbgMeleeDamageDispatched) { dispatched=true; break; }
            }
            yield return null;

            var after = sandbox.CaptureRuntimeDebugSnapshot();
            WriteLine("n222_bite_melee_correction.txt",
                label+" dispatched="+dispatched+
                " hpBefore="+hpBefore+" hpAfter="+after.enemyHP+
                " velocityX="+after.enemyVelocityXOut+
                " primaryClipAfter="+after.enemyClipName+
                " stillAttacking="+after.enemyAttacking);

            Assert.IsTrue(dispatched, label+": melee damage must still be dispatched during bite.");
            Assert.Less(after.enemyHP, hpBefore-0.001f, label+": damage must still apply during bite.");
            Assert.AreNotEqual(0f, after.enemyVelocityXOut,
                label+": ordinary melee knockback must NOT be suppressed during bite (N.2.22 correction).");
            StringAssert.Contains("behited", after.enemyClipName ?? "",
                label+": ordinary melee hurt-reaction (BehitedAnim) must fire during bite (N.2.22 correction).");
            Assert.IsFalse(after.enemyAttacking,
                label+": ordinary melee must fully interrupt/cancel the bite (N.2.22 correction).");
        }

        [UnityTest]
        public IEnumerator Type1_Bite_NoKnockbackNoStun() { yield return BiteImmunityMelee(0,"Type1"); }

        [UnityTest]
        public IEnumerator Type2_Bite_NoKnockbackNoStun() { yield return BiteImmunityMelee(1,"Type2"); }

        // ----------------------------------------------------------------
        // 9: Type4 -> mechanically identical to Type1/Type2 (direct proof,
        // not analogy - sub_10554 has no species-4 branch).
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Type4_Bite_SameAsType1Type2() { yield return BiteImmunityMelee(3,"Type4"); }

        // ----------------------------------------------------------------
        // 8: Type3 bite -> F2/F3 stun/suppression still applies (they sit
        // outside sub_10554's generic dispatch entirely - proven prior
        // tasks, re-verified here only for "does forcing enemyAttacking
        // break it" which is the one thing this task's changes could have
        // affected).
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Type3_Bite_F3StunPreserved()
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(2,true); // Type3 K2Trex
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            // F3's cone check requires dx=head.x-player.x > 0 (atkDis=1024,
            // effectively unlimited for this test). Type3's own bite/attack
            // lunge closes distance while EnemyBiteCommitted()==true (that IS
            // the "actively biting" behavior this test needs); the original
            // 60-unit gap left no margin and let the lunge carry the head
            // anchor past the player's X within the 2.5s poll window,
            // flipping dx negative and permanently failing inCone (the first
            // run of this test read enemyF3State=False the whole time - the
            // shot never entered cone, not a bite-immunity regression).
            // Widened to 300 units - still trivially inside atkDis - so the
            // lunge cannot cross zero before F3 gets a chance to land.
            sandbox.TestHarness_SetEnemyX(300f);
            sandbox.TestHarness_SetWeapon(5,2,250); // F3
            yield return null;
            sandbox.TestHarness_ForceEnemyAttack();
            yield return null;
            float hpBefore=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            sandbox.TestHarness_SetFireHeld(true);
            float start=Time.time; bool hit=false;
            while(Time.time-start<2.5f)
            {
                yield return null;
                if(sandbox.CaptureRuntimeDebugSnapshot().enemyHP<hpBefore-0.001f) { hit=true; break; }
            }
            sandbox.TestHarness_SetFireHeld(false);
            var after = sandbox.CaptureRuntimeDebugSnapshot();
            WriteLine("n221_bite_immunity.txt",
                "Type3_F3 hit="+hit+" f3State="+after.enemyF3State+" stillAttacking="+after.enemyAttacking);
            Assert.IsTrue(hit, "Type3 F3 must still apply its DoT/suppression even while the dino is bite-committed.");
        }
    }
}
#endif
