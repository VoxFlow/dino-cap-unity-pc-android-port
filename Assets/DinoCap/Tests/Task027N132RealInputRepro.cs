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
    /// TASK027N.1.3.2 - REAL INPUT reproduction. Unlike Task027N131RuntimeRepro
    /// (which called TestHarness_Melee() -> Melee() directly, proving only that
    /// the DOWNSTREAM contact/damage chain is invuln-independent), this suite
    /// drives the melee input path through PollKeyboard()'s own edge-detection
    /// and Melee()-call code, UNMODIFIED. Melee()/TestHarness_Melee() are NEVER
    /// called from this file.
    ///
    /// Input source: TWO real Unity Input System event-injection mechanisms
    /// were attempted first (InputTestFixture.Press/Release, then raw
    /// InputSystem.QueueStateEvent+KeyboardState on a manually-added device)
    /// and BOTH failed to reach Keyboard.current.fKey in this project's
    /// headless batchmode PlayMode-test environment (see report Part B/M for
    /// the full trace - respectively an InputTestFixture-internal manager
    /// invalidation, and queued events that never took effect even for a
    /// "press after full recovery" sanity check). Per Task027N.1.3.2 Part B's
    /// own explicitly-sanctioned fallback order, this file uses option 4: a
    /// thin CombatSandboxV2.TestHarness_SetRawBlueKey(isPressed,
    /// wasPressedThisFrame) hook that supplies PollKeyboard() with the exact
    /// two booleans a real Keyboard.current.fKey would - PollKeyboard's own
    /// edge-detection branch and its call to Melee() are the SAME unmodified
    /// production code that runs for a real key; the hook itself never calls
    /// Melee() or any other gameplay method.
    /// </summary>
    public class Task027N132RealInputRepro
    {
        const string ScenePath = "Assets/DinoCap/Scenes/CombatSandboxV2.unity";
        static readonly string OutDir =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_1_3_2_RealInputPostHurtMelee/raw"));

        CombatSandboxV2 sandbox;
        bool rawBlueDownForLog;

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
            sandbox.TestHarness_ClearRawBlueKeyOverride();
            rawBlueDownForLog = false;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            if (sandbox != null)
                sandbox.TestHarness_ClearRawBlueKeyOverride();
            yield return null;
        }

        // "Press": one frame with wasPressedThisFrame=true (the edge a real
        // key-down produces), then continuously isPressed=true/edge=false
        // (held) until ReleaseF() is called. This is exactly the state shape
        // Keyboard.current.fKey would present for a real press-and-hold.
        IEnumerator PressF()
        {
            sandbox.TestHarness_SetRawBlueKey(true, true);
            rawBlueDownForLog = true;
            yield return null;
            sandbox.TestHarness_SetRawBlueKey(true, false);
        }
        IEnumerator ReleaseF()
        {
            sandbox.TestHarness_SetRawBlueKey(false, false);
            rawBlueDownForLog = false;
            yield return null;
        }

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
            "invulnUntil","playerInvulnerable","playerX","enemyX","enemyHP",
            "dbgMeleeContactEvent","dbgMeleeTargetFound","dbgMeleeDamageDispatched","dbgMeleeBlockReason",
            "playerClipName","playerFrame","dinoKindIndex","hasEnemy",
            "rawBlueDown_test_driven","keyboardCurrentIsNull"
        };

        CombatSandboxV2.RuntimeDebugSnapshot Snap(Csv csv, string tag)
        {
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            csv.Row(tag, Time.frameCount, s.timeNow, s.playerDead, s.playerMelee, s.playerHitStun,
                s.invulnUntil, s.timeNow < s.invulnUntil, s.playerX, s.enemyX, s.enemyHP,
                s.dbgMeleeContactEvent, s.dbgMeleeTargetFound, s.dbgMeleeDamageDispatched, s.dbgMeleeBlockReason,
                s.playerClipName, s.playerFrame, s.dinoKindIndex, s.hasEnemy,
                rawBlueDownForLog, sandbox.TestHarness_KeyboardCurrentIsNull);
            return s;
        }

        IEnumerator WaitUntil(Csv csv, string tagPrefix, float timeoutSeconds,
            System.Func<CombatSandboxV2.RuntimeDebugSnapshot, bool> cond,
            System.Action<bool, CombatSandboxV2.RuntimeDebugSnapshot> onDone)
        {
            float start = Time.time;
            int i = 0;
            CombatSandboxV2.RuntimeDebugSnapshot last = sandbox.CaptureRuntimeDebugSnapshot();
            while (Time.time - start < timeoutSeconds && i < 200000)
            {
                yield return null;
                i++;
                last = Snap(csv, tagPrefix + i);
                if (cond(last)) { onDone(true, last); yield break; }
            }
            onDone(false, last);
        }

        // =================================================================
        // Shared setup: spawn a species, wait for it to bite the player via
        // its own AI (no manual movement, no melee/attack call of any kind).
        // =================================================================
        IEnumerator SpawnAndWaitForBite(Csv csv, int species, float spawnX,
            System.Action<bool, CombatSandboxV2.RuntimeDebugSnapshot> onBite)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(species, true);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(spawnX);
            sandbox.TestHarness_SetMeleeTier(0);
            Snap(csv, "spawn");
            yield return WaitUntil(csv, "wait_bite_", 12f, s => s.playerHitStun, onBite);
        }

        // =================================================================
        // CASE A (all 4 types): tap Blue (press+release) EARLY during hurt,
        // well before recovery. Per the native contract (byte_21BE74 is a
        // LEVEL, re-checked only at the hurt-completion boundary) this must
        // NOT auto-start melee, because the button is no longer held by the
        // time that check runs - exactly like a real released tap. Then a
        // FRESH press after recovery must still work normally.
        // =================================================================
        IEnumerator RunTapDuringHurt(int species, float spawnX, string name)
        {
            var summary = new StringBuilder();
            var csv = new Csv(SnapCols);
            bool bitten = false;
            yield return SpawnAndWaitForBite(csv, species, spawnX, (ok, s) => bitten = ok);
            Assert.IsTrue(bitten, name + ": dino never bit the player within budget.");

            // Real tap: press, hold ~30ms of game time, release - all while
            // still hurt (native's Wudi/hurt window is >0.5s, so 30ms is
            // safely mid-hurt for every species).
            yield return PressF();
            Snap(csv, "tap_press");
            float tapStart = Time.time;
            while (Time.time - tapStart < 0.03f) yield return null;
            yield return ReleaseF();
            Snap(csv, "tap_release");

            bool hitstunCleared = false;
            CombatSandboxV2.RuntimeDebugSnapshot clearSnap = default;
            yield return WaitUntil(csv, "wait_clear_", 4f, s => !s.playerHitStun,
                (ok, s) => { hitstunCleared = ok; clearSnap = s; });
            Assert.IsTrue(hitstunCleared, name + ": hitstun never cleared.");

            // Give the (correctly) un-buffered tap a moment to PROVE it does
            // NOT auto-fire.
            bool autoStartedFromStaleTap = false;
            float settleStart = Time.time;
            while (Time.time - settleStart < 0.3f)
            {
                yield return null;
                var s = Snap(csv, "post_clear_no_input_"+(Time.time-settleStart).ToString("0.000"));
                if (s.playerMelee) autoStartedFromStaleTap = true;
            }

            // Now a FRESH press after recovery - must work exactly as normal.
            float hpBefore = sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            yield return PressF();
            Snap(csv, "fresh_press_after_recovery");
            yield return ReleaseF();

            bool contactFired = false, targetFound = false, damageDispatched = false;
            float start2 = Time.time; int i2 = 0;
            while (Time.time - start2 < 2f && i2 < 20000)
            {
                yield return null; i2++;
                var s = Snap(csv, "post_fresh_press_"+i2);
                if (s.dbgMeleeContactEvent && !contactFired)
                {
                    contactFired = true; targetFound = s.dbgMeleeTargetFound; damageDispatched = s.dbgMeleeDamageDispatched;
                }
                if (contactFired && (Time.time-start2) > 0.2f) break;
            }
            float hpAfter = sandbox.CaptureRuntimeDebugSnapshot().enemyHP;

            csv.Save(Path.Combine(OutDir, $"tap_during_hurt_{name}.csv"));
            summary.AppendLine($"=== TAP_DURING_HURT {name} ===");
            summary.AppendLine($"bitten={bitten} hitstunCleared={hitstunCleared} " +
                $"invulnUntilAtClear={clearSnap.invulnUntil:0.###} timeAtClear={clearSnap.timeNow:0.###} " +
                $"stillInvulnAtClear={(clearSnap.timeNow<clearSnap.invulnUntil)}");
            summary.AppendLine($"autoStartedFromStaleTap={autoStartedFromStaleTap} (EXPECTED false - a released tap must not buffer)");
            summary.AppendLine($"freshPressAfterRecovery: contactFired={contactFired} targetFound={targetFound} " +
                $"damageDispatched={damageDispatched} hpBefore={hpBefore:0.###} hpAfter={hpAfter:0.###} " +
                $"hpDropped={(hpAfter<hpBefore-0.001f)}");
            summary.AppendLine();
            File.AppendAllText(Path.Combine(OutDir, "real_input_summary.txt"), summary.ToString());
        }

        // =================================================================
        // CASE B (all 4 types): press Blue and HOLD continuously from mid-hurt
        // straight through recovery, WITHOUT ever pressing again. Per native
        // (byte_21BE74 still true at the hurt-completion boundary check),
        // melee must auto-start the instant control returns, with NO second
        // press.
        // =================================================================
        IEnumerator RunHoldThroughHurt(int species, float spawnX, string name)
        {
            var summary = new StringBuilder();
            var csv = new Csv(SnapCols);
            bool bitten = false;
            yield return SpawnAndWaitForBite(csv, species, spawnX, (ok, s) => bitten = ok);
            Assert.IsTrue(bitten, name + ": dino never bit the player within budget.");

            yield return PressF(); // held from here on - NOT released
            Snap(csv, "press_and_hold_start");

            bool hitstunCleared = false;
            CombatSandboxV2.RuntimeDebugSnapshot clearSnap = default;
            float hpBeforeClear = sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            yield return WaitUntil(csv, "wait_clear_holding_", 4f, s => !s.playerHitStun,
                (ok, s) => { hitstunCleared = ok; clearSnap = s; });
            Assert.IsTrue(hitstunCleared, name + ": hitstun never cleared while holding.");

            bool contactFired = false, targetFound = false, damageDispatched = false;
            bool playerMeleeSeen = false;
            float start2 = Time.time; int i2 = 0;
            while (Time.time - start2 < 1f && i2 < 20000)
            {
                yield return null; i2++;
                var s = Snap(csv, "post_clear_holding_"+i2);
                if (s.playerMelee) playerMeleeSeen = true;
                if (s.dbgMeleeContactEvent && !contactFired)
                {
                    contactFired = true; targetFound = s.dbgMeleeTargetFound; damageDispatched = s.dbgMeleeDamageDispatched;
                }
                if (contactFired && (Time.time-start2) > 0.2f) break;
            }
            yield return ReleaseF();
            float hpAfter = sandbox.CaptureRuntimeDebugSnapshot().enemyHP;

            csv.Save(Path.Combine(OutDir, $"hold_through_hurt_{name}.csv"));
            summary.AppendLine($"=== HOLD_THROUGH_HURT {name} ===");
            summary.AppendLine($"bitten={bitten} hitstunCleared={hitstunCleared} " +
                $"invulnUntilAtClear={clearSnap.invulnUntil:0.###} timeAtClear={clearSnap.timeNow:0.###} " +
                $"stillInvulnAtClear={(clearSnap.timeNow<clearSnap.invulnUntil)}");
            summary.AppendLine($"NO SECOND PRESS ISSUED. playerMeleeSeen={playerMeleeSeen} contactFired={contactFired} " +
                $"targetFound={targetFound} damageDispatched={damageDispatched} " +
                $"hpBeforeClear={hpBeforeClear:0.###} hpAfter={hpAfter:0.###} hpDropped={(hpAfter<hpBeforeClear-0.001f)} " +
                $"(EXPECTED all true - a sustained hold through recovery must auto-start melee per native sub_66550 case 5)");
            summary.AppendLine();
            File.AppendAllText(Path.Combine(OutDir, "real_input_summary.txt"), summary.ToString());
        }

        [UnityTest] public IEnumerator Tap_Type1_K3Blue() => RunTapDuringHurt(0, 220f, "Type1_K3Blue");
        [UnityTest] public IEnumerator Tap_Type2_K1Rusher() => RunTapDuringHurt(1, 220f, "Type2_K1Rusher");
        [UnityTest] public IEnumerator Tap_Type3_K2Trex() => RunTapDuringHurt(2, 300f, "Type3_K2Trex");
        [UnityTest] public IEnumerator Tap_Type4_K4Raptor() => RunTapDuringHurt(3, 220f, "Type4_K4Raptor");

        [UnityTest] public IEnumerator Hold_Type1_K3Blue() => RunHoldThroughHurt(0, 220f, "Type1_K3Blue");
        [UnityTest] public IEnumerator Hold_Type2_K1Rusher() => RunHoldThroughHurt(1, 220f, "Type2_K1Rusher");
        [UnityTest] public IEnumerator Hold_Type3_K2Trex() => RunHoldThroughHurt(2, 300f, "Type3_K2Trex");
        [UnityTest] public IEnumerator Hold_Type4_K4Raptor() => RunHoldThroughHurt(3, 220f, "Type4_K4Raptor");

        // =================================================================
        // Deep-dive timing cases (Type 3 only - the input layer is shared
        // code, not species-specific; Type 3 chosen as the largest/slowest
        // species and the one under the most other active scrutiny).
        // CASE C: press on the exact frame hitstun is observed to clear.
        // CASE D: press one full frame AFTER recovery is confirmed.
        // CASE E: tap+release during hurt, THEN a second fresh tap+release
        // after recovery (the two-press "workaround" a confused user finds).
        //
        // Each case gets its OWN [UnityTest] with its own fresh [UnitySetUp]
        // scene load (Task027N131 Lesson #4): dbgMeleeContactEvent and
        // dbgMeleeDamageDispatched are permanent latches in CombatSandboxV2
        // (DebugRecordMeleeContactEvent sets dbgMeleeContactEvent=true and
        // is the ONLY writer of either flag's "false" state, and only for
        // dbgMeleeDamageDispatched/dbgMeleeTargetFound - dbgMeleeContactEvent
        // itself is NEVER reset to false anywhere). Running C/D/E back to
        // back in one scene load let Case C's real hit leak into Case D/E's
        // very first post-press snapshot as a false-positive "contact" -
        // caught by the hpBefore==hpAfter contradiction in the first run of
        // this suite (2026-09-15 01:41 UTC) and confirmed via source read of
        // CombatSandboxV2.cs lines 271-298. Each case below asserts the
        // freshly-loaded scene starts with dbgMeleeContactEvent==false to
        // self-verify isolation before proceeding.
        // =================================================================
        [UnityTest]
        public IEnumerator CaseC_PressOnRecoveryFrame_Type3()
        {
            var summary = new StringBuilder();
            var csv = new Csv(SnapCols);
            Assert.IsFalse(sandbox.CaptureRuntimeDebugSnapshot().dbgMeleeContactEvent,
                "CaseC: dbgMeleeContactEvent already true on a freshly loaded scene - contamination.");

            bool bitten = false;
            yield return SpawnAndWaitForBite(csv, 2, 300f, (ok, s) => bitten = ok);
            Assert.IsTrue(bitten);

            bool cleared = false;
            // Poll frame-by-frame ourselves (not via WaitUntil) so we can
            // press in the SAME iteration we observe the clear.
            float start = Time.time; int i = 0;
            while (Time.time - start < 4f && i < 20000 && !cleared)
            {
                yield return null; i++;
                var s = Snap(csv, "poll_for_caseC_"+i);
                if (!s.playerHitStun)
                {
                    cleared = true;
                    yield return PressF();
                    Snap(csv,"caseC_press_on_clear_frame");
                }
            }
            Assert.IsTrue(cleared);
            bool contactFired=false, dmg=false; float hpBefore=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            float s2=Time.time; int i2=0;
            while (Time.time-s2<2f && i2<20000) { yield return null; i2++; var s=Snap(csv,"caseC_post_"+i2);
                if (s.dbgMeleeContactEvent && !contactFired){contactFired=true; dmg=s.dbgMeleeDamageDispatched;}
                if (contactFired && (Time.time-s2)>0.2f) break; }
            yield return ReleaseF();
            float hpAfter=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            csv.Save(Path.Combine(OutDir,"caseC_press_on_recovery_frame_Type3.csv"));
            summary.AppendLine($"=== CASE C: press on exact recovery frame (Type3) ===");
            summary.AppendLine($"contactFired={contactFired} damageDispatched={dmg} hpBefore={hpBefore:0.###} hpAfter={hpAfter:0.###} " +
                $"hpDropped={(hpAfter<hpBefore-0.001f)}");
            summary.AppendLine();
            File.AppendAllText(Path.Combine(OutDir, "real_input_summary.txt"), summary.ToString());
        }

        [UnityTest]
        public IEnumerator CaseD_PressAfterRecovery_Type3()
        {
            var summary = new StringBuilder();
            var csv = new Csv(SnapCols);
            Assert.IsFalse(sandbox.CaptureRuntimeDebugSnapshot().dbgMeleeContactEvent,
                "CaseD: dbgMeleeContactEvent already true on a freshly loaded scene - contamination.");

            bool bitten = false;
            yield return SpawnAndWaitForBite(csv, 2, 300f, (ok, s) => bitten = ok);
            Assert.IsTrue(bitten);
            bool cleared=false;
            yield return WaitUntil(csv,"caseD_wait_",4f, s=>!s.playerHitStun, (ok,s)=>cleared=ok);
            Assert.IsTrue(cleared);
            yield return null; // one extra frame after recovery
            yield return PressF();
            Snap(csv,"caseD_press_after_recovery");
            yield return ReleaseF();
            bool contactFired=false, dmg=false; float hpBefore=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            float s2=Time.time; int i2=0;
            while (Time.time-s2<2f && i2<20000) { yield return null; i2++; var s=Snap(csv,"caseD_post_"+i2);
                if (s.dbgMeleeContactEvent && !contactFired){contactFired=true; dmg=s.dbgMeleeDamageDispatched;}
                if (contactFired && (Time.time-s2)>0.2f) break; }
            float hpAfter=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            csv.Save(Path.Combine(OutDir,"caseD_press_after_recovery_Type3.csv"));
            summary.AppendLine($"=== CASE D: press one frame after recovery (Type3) ===");
            summary.AppendLine($"contactFired={contactFired} damageDispatched={dmg} hpBefore={hpBefore:0.###} hpAfter={hpAfter:0.###} " +
                $"hpDropped={(hpAfter<hpBefore-0.001f)}");
            summary.AppendLine();
            File.AppendAllText(Path.Combine(OutDir, "real_input_summary.txt"), summary.ToString());
        }

        [UnityTest]
        public IEnumerator CaseE_ReleaseThenRepress_Type3()
        {
            var summary = new StringBuilder();
            var csv = new Csv(SnapCols);
            Assert.IsFalse(sandbox.CaptureRuntimeDebugSnapshot().dbgMeleeContactEvent,
                "CaseE: dbgMeleeContactEvent already true on a freshly loaded scene - contamination.");

            bool bitten = false;
            yield return SpawnAndWaitForBite(csv, 2, 300f, (ok, s) => bitten = ok);
            Assert.IsTrue(bitten);
            yield return PressF();
            float t0=Time.time;
            while (Time.time-t0<0.03f) yield return null;
            yield return ReleaseF();
            Snap(csv,"caseE_first_tap_released");
            bool cleared=false;
            yield return WaitUntil(csv,"caseE_wait_",4f, s=>!s.playerHitStun,(ok,s)=>cleared=ok);
            Assert.IsTrue(cleared);
            float hpBefore=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            yield return PressF();
            Snap(csv,"caseE_second_press");
            yield return ReleaseF();
            bool contactFired=false, dmg=false;
            float s2=Time.time; int i2=0;
            while (Time.time-s2<2f && i2<20000) { yield return null; i2++; var s=Snap(csv,"caseE_post_"+i2);
                if (s.dbgMeleeContactEvent && !contactFired){contactFired=true; dmg=s.dbgMeleeDamageDispatched;}
                if (contactFired && (Time.time-s2)>0.2f) break; }
            float hpAfter=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            csv.Save(Path.Combine(OutDir,"caseE_release_then_repress_Type3.csv"));
            summary.AppendLine($"=== CASE E: release during hurt, fresh repress after recovery (Type3) ===");
            summary.AppendLine($"secondPress: contactFired={contactFired} damageDispatched={dmg} hpBefore={hpBefore:0.###} hpAfter={hpAfter:0.###} " +
                $"hpDropped={(hpAfter<hpBefore-0.001f)}");
            summary.AppendLine();
            File.AppendAllText(Path.Combine(OutDir, "real_input_summary.txt"), summary.ToString());
        }
    }
}
#endif
