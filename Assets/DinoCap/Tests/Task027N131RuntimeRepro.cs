#if UNITY_EDITOR
using System;
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
    /// TASK027N.1.3.1 live PlayMode runtime-repro harness. EDITOR-ONLY
    /// (guarded by UNITY_EDITOR, so it cannot ship in any player build).
    /// Drives the REAL CombatSandboxV2 MonoBehaviour through its actual
    /// Update()/animation-event pipeline via the internal TestHarness_* API
    /// added to CombatSandboxV2.cs - no gameplay code path is bypassed,
    /// mocked, or reimplemented here. Every measurement below is a genuine
    /// frame-by-frame observation of the running game, not a static trace.
    ///
    /// V2 (this file): the first pass waited a fixed NUMBER of
    /// Update() ticks per phase, assuming ~60fps. Batchmode-without-a-display
    /// runs the player loop far faster than that (observed Time.deltaTime
    /// ~0.0004-0.0005s per tick here, i.e. thousands of ticks/sec), so a
    /// 90-tick budget was only ~0.04s of GAME time - nowhere near enough for
    /// the weapon's frame-3-gated discharge or the bite/hurt cycle. Every
    /// wait below is now bounded by real GAME TIME (Time.time), not a tick
    /// count, which is what the game's own timers (frame-3 discharge, the
    /// 0.1s light-recoil timer, the 1.5s invuln window) actually run on.
    /// </summary>
    public class Task027N131RuntimeRepro
    {
        const string ScenePath = "Assets/DinoCap/Scenes/CombatSandboxV2.unity";
        static readonly string OutDir =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_1_3_1_LightRecoilInvulnClosure/raw"));

        CombatSandboxV2 sandbox;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Directory.CreateDirectory(OutDir);
            EditorSceneManager.LoadSceneInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            yield return null;
            sandbox = UnityEngine.Object.FindFirstObjectByType<CombatSandboxV2>();
            Assert.IsNotNull(sandbox, "CombatSandboxV2 not found after loading " + ScenePath);
        }

        // ---------------------------------------------------------------
        // Small CSV writer (no external JSON dependency).
        // ---------------------------------------------------------------
        class Csv
        {
            readonly List<string> header;
            readonly List<string[]> rows = new List<string[]>();
            public Csv(params string[] cols) { header = cols.ToList(); }
            public void Row(params object[] vals)
            {
                var s = new string[vals.Length];
                for (int i = 0; i < vals.Length; i++)
                {
                    var v = vals[i];
                    s[i] = v switch
                    {
                        null => "",
                        float f => f.ToString("0.####", CultureInfo.InvariantCulture),
                        double d => d.ToString("0.####", CultureInfo.InvariantCulture),
                        bool b => b ? "1" : "0",
                        _ => v.ToString().Replace(",", ";")
                    };
                }
                rows.Add(s);
            }
            public void Save(string path)
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", header));
                foreach (var r in rows) sb.AppendLine(string.Join(",", r));
                File.WriteAllText(path, sb.ToString());
            }
        }

        static readonly string[] SnapCols = {
            "tag","frame","time","playerDead","playerMelee","playerHitStun",
            "invulnUntil","playerInvulnerable","playerX","enemyX","dx",
            "enemyHP","enemyAttacking","enemyHeavyReaction","enemyBurningF2","enemyF3State",
            "dbgMeleeContactEvent","dbgMeleeTargetFound","dbgMeleeDamageDispatched","dbgMeleeBlockReason",
            "playerClipName","playerFrame","playerFacingRight",
            "enemyClipName","enemyFrame","enemyClipPrefix","enemyFacingRight",
            "dinoKindIndex","hasEnemy"
        };

        CombatSandboxV2.RuntimeDebugSnapshot Snap(Csv csv, string tag)
        {
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            double dx = (s.hasEnemy && s.hasPlayer) ? (s.enemyX - s.playerX) : double.NaN;
            csv.Row(tag, Time.frameCount, s.timeNow, s.playerDead, s.playerMelee, s.playerHitStun,
                s.invulnUntil, s.timeNow < s.invulnUntil, s.playerX, s.enemyX, dx,
                s.enemyHP, s.enemyAttacking, s.enemyHeavyReaction, s.enemyBurningF2, s.enemyF3State,
                s.dbgMeleeContactEvent, s.dbgMeleeTargetFound, s.dbgMeleeDamageDispatched, s.dbgMeleeBlockReason,
                s.playerClipName, s.playerFrame, s.playerFacingRight,
                s.enemyClipName, s.enemyFrame, s.enemyClipPrefixOut, s.enemyFacingRightOut,
                s.dinoKindIndex, s.hasEnemy);
            return s;
        }

        /// <summary>
        /// Ticks Update() (logging every frame to csv under tagPrefix+index)
        /// until cond(snapshot) is true or timeoutSeconds of GAME time
        /// (Time.time) elapses. A hard 200000-iteration safety cap prevents
        /// a genuine runaway loop even if Time.time somehow stalls.
        /// </summary>
        IEnumerator WaitUntil(Csv csv, string tagPrefix, float timeoutSeconds,
            Func<CombatSandboxV2.RuntimeDebugSnapshot, bool> cond,
            Action<bool, CombatSandboxV2.RuntimeDebugSnapshot> onDone)
        {
            float start = Time.time;
            int i = 0;
            CombatSandboxV2.RuntimeDebugSnapshot last = sandbox.CaptureRuntimeDebugSnapshot();
            while (Time.time - start < timeoutSeconds && i < 200000)
            {
                yield return null;
                i++;
                last = Snap(csv, tagPrefix + i);
                if (cond(last))
                {
                    onDone(true, last);
                    yield break;
                }
            }
            onDone(false, last);
        }

        // =================================================================
        // TEST 1 - Type 3 (and, for comparison, Type 1) light-recoil
        // wall-clock measurement for A/B/C/D/F1. Parts C/D.
        // =================================================================
        [UnityTest]
        public IEnumerator LightRecoilMeasurement()
        {
            var summary = new StringBuilder();
            foreach (int speciesIndex in new[] { 2, 0 })
            {
                sandbox.TestHarness_ClearDinos();
                yield return null;
                sandbox.TestHarness_SpawnDino(speciesIndex, true);
                yield return null;
                sandbox.TestHarness_SetPlayerX(0f);
                sandbox.TestHarness_SetEnemyX(320f); // outside every species' ADIS (<=280) -> chasing, not biting
                yield return null;

                var families = new (int fam, int tier, string label)[] {
                    (0,0,"A1"), (1,0,"B1"), (2,0,"C1"), (3,0,"D1"), (5,0,"F1")
                };

                foreach (var f in families)
                {
                    var csv = new Csv(SnapCols);
                    sandbox.TestHarness_SetEnemyX(320f);
                    sandbox.TestHarness_SetDebugEnemyHP(9999f);
                    sandbox.TestHarness_SetFireHeld(false);
                    yield return null;
                    sandbox.TestHarness_SetWeapon(f.fam, f.tier, 50);
                    var pre = Snap(csv, "pre");
                    float xBefore = pre.enemyX;
                    float hpBefore = pre.enemyHP;

                    // C(2)/D(3) are native HELD-FIRE families - a single
                    // Shoot() press-edge never triggers them.
                    bool heldFire = f.fam == 2 || f.fam == 3;
                    if (heldFire) sandbox.TestHarness_SetFireHeld(true);
                    else sandbox.TestHarness_Shoot();

                    // Wait (real game time) for a confirmed HP drop, up to 4s -
                    // generous versus the ~0.2s frame-3-gated discharge cadence.
                    bool hpDropped = false;
                    float xAtHpDrop = float.NaN;
                    CombatSandboxV2.RuntimeDebugSnapshot atDrop = default;
                    yield return WaitUntil(csv, "post_", 4f,
                        s => s.enemyHP < hpBefore - 0.001f,
                        (ok, s) => { hpDropped = ok; atDrop = s; if (ok) xAtHpDrop = s.enemyX; });

                    if (heldFire) sandbox.TestHarness_SetFireHeld(false);

                    // A few extra real-time frames past the hit so any
                    // recoil velocity has had a chance to be integrated and
                    // (per the port model) overwritten again by the next AI
                    // tick, then read the settled position.
                    float settleStart = Time.time;
                    while (Time.time - settleStart < 0.3f) { yield return null; Snap(csv, "settle_"+(Time.time-settleStart).ToString("0.000")); }
                    var final = sandbox.CaptureRuntimeDebugSnapshot();
                    float xFinal = final.enemyX;

                    csv.Save(Path.Combine(OutDir,
                        $"light_recoil_species{speciesIndex}_{f.label}.csv"));

                    summary.AppendLine(
                        $"species={speciesIndex} family={f.label} " +
                        $"xBefore={xBefore:0.####} hpDropped={hpDropped} xAtHpDrop={xAtHpDrop:0.####} " +
                        $"xFinal={xFinal:0.####} netDisplacementSinceHit={(hpDropped ? (xFinal - xAtHpDrop) : float.NaN):0.####} " +
                        $"netDisplacementTotal={(xFinal - xBefore):0.####} speciesSpeed={sandbox.TestHarness_EnemySourceSpeed:0.####} " +
                        $"timeToHit={(hpDropped ? (atDrop.timeNow - pre.timeNow) : float.NaN):0.####}s");
                }
            }
            File.WriteAllText(Path.Combine(OutDir, "light_recoil_summary.txt"), summary.ToString());
        }

        // =================================================================
        // TEST 2 - player-invuln / outgoing-melee reproduction, all 4 types.
        // Parts G, H, I, J, N.
        //
        // FOUR SEPARATE [UnityTest] methods (not one loop) - each gets its
        // own fresh [UnitySetUp] scene load, which is required for
        // correctness here: dbgMeleeContactEvent/TargetFound/DamageDispatched
        // are plain fields on CombatSandboxV2 that only get reset the next
        // time ExecuteMeleeContact() actually fires. A single shared scene
        // looping over 4 species would let species N's leftover TRUE flags
        // read as an instant (false-positive) "contact fired" the moment
        // species N+1 calls Melee(), before any genuine new event. A fresh
        // scene load guarantees the fields start at their true default
        // (false/"none") for every case.
        // =================================================================
        IEnumerator RunInvulnMeleeCase(int species, float spawnX, string name)
        {
            var summary = new StringBuilder();
            var csv = new Csv(SnapCols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(species, true);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(spawnX);
            sandbox.TestHarness_SetMeleeTier(0); // G1 - weakest, avoid an accidental one-shot kill mid-repro
            var spawnSnap = Snap(csv, "spawn");
            Assert.IsFalse(spawnSnap.dbgMeleeContactEvent,
                "Harness precondition failed: dbgMeleeContactEvent was already true at spawn " +
                "(fresh-scene assumption broken) - results for " + name + " would be contaminated.");

            // --- phase 1: wait for the bite (playerHitStun becomes true) ---
            bool bitten = false;
            float distanceAtImpact = float.NaN;
            CombatSandboxV2.RuntimeDebugSnapshot biteSnap = default;
            yield return WaitUntil(csv, "wait_bite_", 12f,
                s => s.playerHitStun,
                (ok, s) => { bitten = ok; biteSnap = s; if (ok) distanceAtImpact = s.enemyX - s.playerX; });

            // --- phase 2: wait for hitstun to clear ---
            bool hitstunCleared = false;
            float distanceAtHurtEnd = float.NaN;
            CombatSandboxV2.RuntimeDebugSnapshot hurtEndSnap = default;
            if (bitten)
            {
                yield return WaitUntil(csv, "wait_clear_", 4f,
                    s => !s.playerHitStun,
                    (ok, s) => { hitstunCleared = ok; hurtEndSnap = s; if (ok) distanceAtHurtEnd = s.enemyX - s.playerX; });
            }

            // --- phase 3: as soon as control returns, start melee WHILE
            // still (by construction) inside the invuln window. ---
            bool meleeStarted = false;
            bool stillInvulnAtMeleeStart = false;
            float distanceAtMeleeStart = float.NaN;
            if (hitstunCleared)
            {
                var s0 = sandbox.CaptureRuntimeDebugSnapshot();
                stillInvulnAtMeleeStart = s0.timeNow < s0.invulnUntil;
                distanceAtMeleeStart = s0.enemyX - s0.playerX;
                sandbox.TestHarness_Melee();
                meleeStarted = true;
                Snap(csv, "melee_call");
            }

            bool contactEventFired = false, targetFound = false, damageDispatched = false;
            string blockReason = "n/a";
            float hpBeforeMelee = sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            float distanceAtContact = float.NaN;
            bool playerMeleeConfirmed = false;
            if (meleeStarted)
            {
                float start = Time.time;
                int i = 0;
                while (Time.time - start < 3f && i < 50000)
                {
                    yield return null;
                    i++;
                    var s = Snap(csv, "post_melee_" + i);
                    if (s.playerMelee) playerMeleeConfirmed = true;
                    if (s.dbgMeleeContactEvent && !contactEventFired)
                    {
                        contactEventFired = true;
                        targetFound = s.dbgMeleeTargetFound;
                        damageDispatched = s.dbgMeleeDamageDispatched;
                        blockReason = s.dbgMeleeBlockReason;
                        distanceAtContact = s.enemyX - s.playerX;
                    }
                    if (contactEventFired && (Time.time - start) > 0.2f) break;
                }
            }
            float hpAfterMelee = sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            bool hpActuallyDropped = hpAfterMelee < hpBeforeMelee - 0.001f;

            csv.Save(Path.Combine(OutDir, $"invuln_melee_{name}.csv"));

            summary.AppendLine($"=== {name} (species {species}) ===");
            summary.AppendLine($"bitten={bitten} timeAtBite={biteSnap.timeNow:0.###} distanceAtImpact={distanceAtImpact:0.###}");
            summary.AppendLine($"hitstunCleared={hitstunCleared} timeAtHurtEnd={hurtEndSnap.timeNow:0.###} distanceAtHurtEnd={distanceAtHurtEnd:0.###} " +
                $"invulnUntilAtHurtEnd={hurtEndSnap.invulnUntil:0.###} stillInvulnAtHurtEnd={(hurtEndSnap.timeNow < hurtEndSnap.invulnUntil)}");
            summary.AppendLine($"meleeStarted={meleeStarted} playerMeleeConfirmedNextFrames={playerMeleeConfirmed} " +
                $"stillInvulnAtMeleeStart={stillInvulnAtMeleeStart} distanceAtMeleeStart={distanceAtMeleeStart:0.###}");
            summary.AppendLine($"contactEventFired={contactEventFired} targetFound={targetFound} damageDispatched={damageDispatched} " +
                $"blockReason={blockReason} distanceAtContact={distanceAtContact:0.###}");
            summary.AppendLine($"hpBeforeMelee={hpBeforeMelee:0.###} hpAfterMelee={hpAfterMelee:0.###} hpActuallyDropped={hpActuallyDropped}");
            summary.AppendLine();

            File.AppendAllText(Path.Combine(OutDir, "invuln_melee_summary.txt"), summary.ToString());
        }

        [UnityTest] public IEnumerator InvulnMelee_Type1_K3Blue() => RunInvulnMeleeCase(0, 220f, "Type1_K3Blue");
        [UnityTest] public IEnumerator InvulnMelee_Type2_K1Rusher() => RunInvulnMeleeCase(1, 220f, "Type2_K1Rusher");
        [UnityTest] public IEnumerator InvulnMelee_Type3_K2Trex() => RunInvulnMeleeCase(2, 300f, "Type3_K2Trex");
        [UnityTest] public IEnumerator InvulnMelee_Type4_K4Raptor() => RunInvulnMeleeCase(3, 220f, "Type4_K4Raptor");

        // =================================================================
        // TEST 3 - Type 3 decapitation runtime validation (F1, G3, G4).
        // Part F.
        // =================================================================
        // Each decap scenario gets its OWN fresh [UnitySetUp] scene load (see
        // the rationale on RunInvulnMeleeCase above - a shared scene let an
        // earlier scenario's still-landing flying head / still-biting dino
        // contaminate the next one's measurement).
        IEnumerator RunDecapCase(string tag, bool useMelee, int meleeTier, float spawnX)
        {
            var summary = new StringBuilder();
            var csv = new Csv(SnapCols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(2, true); // K2Trex
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(spawnX);
            sandbox.TestHarness_SetDebugEnemyHP(1f); // guarantee the next hit is lethal
            Snap(csv, "spawn");
            int headsBefore = GameObject.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Count(t => t.name.StartsWith("Task027J3_FlyingHead_"));

            if (useMelee)
            {
                // The dino may already be biting the player (it spawns well
                // inside its own ADIS so the melee dx range is guaranteed
                // reachable) - Melee()'s own gate silently no-ops while
                // playerHitStun/playerMelee is true, so wait for a clean
                // window before arming, exactly like a real player would
                // have to.
                yield return WaitUntil(csv, "wait_clear_for_melee_", 5f,
                    s => !s.playerHitStun && !s.playerMelee, (ok, s) => { });
                sandbox.TestHarness_SetMeleeTier(meleeTier);
                sandbox.TestHarness_Melee();
            }
            else
            {
                sandbox.TestHarness_SetWeapon(5, 0, 50); // F1
                sandbox.TestHarness_Shoot();
            }

            bool killed = false;
            yield return WaitUntil(csv, "wait_kill_", 8f,
                s => s.enemyHP <= 0f, (ok, s) => killed = ok);

            // Give the death sequence (head toss + DieAnim + audio) real
            // extra time to instantiate everything, but check EARLY (right
            // after the kill, before the head can finish falling and
            // self-deactivate) so a fast landing doesn't read as "no head".
            var headsRightAfterKill = GameObject.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(t => t.name.StartsWith("Task027J3_FlyingHead_")).ToList();
            string earlyHeadName = headsRightAfterKill.Count > 0 ? headsRightAfterKill[^1].name : "none";
            var earlySr = headsRightAfterKill.Count > 0 ? headsRightAfterKill[^1].GetComponent<SpriteRenderer>() : null;
            string earlyHeadSprite = earlySr != null && earlySr.sprite != null ? earlySr.sprite.name : "none";
            Vector3 earlyHeadScale = headsRightAfterKill.Count > 0 ? headsRightAfterKill[^1].localScale : Vector3.zero;
            bool earlyHeadActive = headsRightAfterKill.Count > 0 && headsRightAfterKill[^1].gameObject.activeInHierarchy;

            float extraStart = Time.time;
            while (Time.time - extraStart < 0.5f) { yield return null; Snap(csv, "post_death_"+(Time.time-extraStart).ToString("0.000")); }

            csv.Save(Path.Combine(OutDir, $"decap_{tag}.csv"));

            var chunks = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Where(t => t.name.StartsWith("Task026_Chunk")).ToList();
            var s2 = sandbox.CaptureRuntimeDebugSnapshot();

            summary.AppendLine($"=== {tag} ===");
            summary.AppendLine($"killed={killed} headsBefore={headsBefore} headFoundRightAfterKill={(headsRightAfterKill.Count>0)} " +
                $"earlyHeadName={earlyHeadName} earlyHeadActive={earlyHeadActive} earlyHeadSprite={earlyHeadSprite} earlyHeadScale={earlyHeadScale}");
            summary.AppendLine($"chunkCountAfterLanding={chunks.Count} " +
                $"chunkKinds=[{string.Join(";", chunks.Select(c2 => c2.name))}]");
            summary.AppendLine($"enemyPrimaryClipAfterDeath={s2.enemyClipName}");
            summary.AppendLine();

            File.AppendAllText(Path.Combine(OutDir, "decap_runtime_summary.txt"), summary.ToString());
        }

        [UnityTest] public IEnumerator Decap_F1_Type3() => RunDecapCase("F1_lethal", false, 0, 200f);
        [UnityTest] public IEnumerator Decap_G3_Type3() => RunDecapCase("G3_lethal", true, 2, 150f);
        [UnityTest] public IEnumerator Decap_G4_Type3() => RunDecapCase("G4_lethal", true, 3, 150f);
    }
}
#endif
