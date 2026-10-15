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
    /// TASK027N.2 Parts K-P - melee bite-end boundary bug reproduction.
    ///
    /// Per Task027N.2 Part Y, the player's own melee INPUT path is closed
    /// (Task027N.1.3.2) and explicitly out of scope here - this suite drives
    /// the swing via TestHarness_Melee() directly (correct for THIS task,
    /// unlike N.1.3.2 where that was forbidden for a different, input-layer
    /// question). What is under test here is entirely downstream:
    /// ExecuteMeleeContact's target/range evaluation at the moment a dino's
    /// bite animation is completing.
    ///
    /// Native finding (this session, IDA-proven): sub_668FC (player melee
    /// target scan) excludes a dino only when reaction_state==3 (dead/dying,
    /// confirmed via sub_10294 case 3 playing "Audio_death3Audio") or when
    /// dino+0xA4 (a boolean initialised to 1 by every dino constructor -
    /// sub_FBF4/sub_57BD8/sub_18858 - and never observed written to 0 at
    /// runtime anywhere in this binary) is false. Reaction state 1
    /// (committed bite/attack) is NOT excluded. sub_10554 (the per-dino
    /// damage applicator) applies damage purely on a position/range test,
    /// with NO reaction-state check at all. CONCLUSION: native has no
    /// bite-state eligibility gate whatsoever - a valid-range hit during or
    /// immediately after a bite always lands. Unity's ExecuteMeleeContact()
    /// likewise has no explicit bite-state gate (checked by source read
    /// before writing this test) - only enemyHP<=0 and a dx-range test using
    /// the "tou" (head) anchor. The suspected divergence is therefore an
    /// ORDER/ANCHOR hazard, not a missing exclusion rule: CompleteEnemyAttack()
    /// (called from TickEnemy() when the enemy's bite clip finishes) can
    /// re-home the enemy's facing (ResignEnemyTowardPlayer) and swap its clip
    /// to "_move" on the SAME host frame the player's melee contact
    /// (OnPlayerPrimaryFrame, frame 10) evaluates the head anchor - if the
    /// enemy's own tick runs first that frame, the anchor/facing used by the
    /// contact test may already reflect the POST-bite pose, producing a
    /// transient dx that can fall outside the (0,200) window even though the
    /// swing was correctly timed. This suite measures that directly.
    ///
    /// TIMING NOTE (lesson re-applied from Task027N.1.3.1): Unity batchmode
    /// PlayMode runs Update() far faster than real time, and per this
    /// project's own animation code comments the melee/bite clips are
    /// quantised to a 60 Hz LOGIC tick separate from the host Update rate -
    /// so counting raw Update-tick iterations does NOT correspond to
    /// animation frames (confirmed empirically: an isolated calibration
    /// swing did not reach its frame-10 contact within 60 raw Update ticks).
    /// All scheduling below is therefore Time.time-based, using 1/60s as the
    /// one-"source-frame" unit, exactly like N.1.3.1's real-time wait budgets.
    /// </summary>
    public class Task027N2BiteEndBoundaryRepro
    {
        const string ScenePath = "Assets/DinoCap/Scenes/CombatSandboxV2.unity";
        const float SourceFrameSeconds = 1f / 60f;
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
        }

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
            "tag","time","playerMelee","enemyAttacking","enemyHP",
            "playerX","enemyX","dbgMeleeContactEvent","dbgMeleeTargetFound",
            "dbgMeleeDamageDispatched","dbgMeleeBlockReason",
            "playerClipName","playerFrame","enemyClipName","enemyFrame",
            "playerFacingRight","enemyFacingRightOut"
        };

        CombatSandboxV2.RuntimeDebugSnapshot Snap(Csv csv, string tag)
        {
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            csv.Row(tag, s.timeNow, s.playerMelee, s.enemyAttacking, s.enemyHP,
                s.playerX, s.enemyX, s.dbgMeleeContactEvent, s.dbgMeleeTargetFound,
                s.dbgMeleeDamageDispatched, s.dbgMeleeBlockReason,
                s.playerClipName, s.playerFrame, s.enemyClipName, s.enemyFrame,
                s.playerFacingRight, s.enemyFacingRightOut);
            return s;
        }

        // ================================================================
        // CALIBRATION: measure the swing-to-contact REAL-TIME delay
        // (Melee() call to dbgMeleeContactEvent becoming true) with NO dino
        // present, so later trials can schedule a swing to land its CONTACT
        // moment at a chosen source-frame offset relative to the dino's
        // bite-completion moment.
        // ================================================================
        [UnityTest]
        public IEnumerator Calibrate_SwingToContactTickDelay()
        {
            var csv = new Csv(SnapCols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SetMeleeTier(0);
            Snap(csv, "pre_swing");
            float t0 = Time.time;
            sandbox.TestHarness_Melee();
            float delay = -1f;
            float budgetStart = Time.time;
            while (Time.time - budgetStart < 5f)
            {
                yield return null;
                var s = Snap(csv, "cal_" + (Time.time - budgetStart).ToString("0.000"));
                if (s.dbgMeleeContactEvent) { delay = Time.time - t0; break; }
            }
            csv.Save(Path.Combine(OutDir, "n2biteend_calibration.csv"));
            Assert.Greater(delay, -1f, "Swing never reached contact within 5s (no dino present).");
            File.WriteAllText(Path.Combine(OutDir, "n2biteend_calibration_result.txt"),
                "swing_to_contact_seconds=" + delay.ToString("0.0000", CultureInfo.InvariantCulture));
        }

        // ================================================================
        // MAIN TRIALS: for each species, position the dino so it commits to
        // a bite, wait for enemyAttacking to go true, then poll for the
        // true->false transition (bite completion). Schedule a fresh
        // TestHarness_Melee() call so its contact moment lands at each of
        // the required offsets relative to the completion moment: -3,-2,-1,
        // 0 (same source-frame), +1, +2 (each unit = 1/60s). One offset per
        // dino (fresh scene each time, per the established N.1.3.1 Lesson #4
        // isolation rule - dbgMeleeContactEvent/dbgMeleeDamageDispatched are
        // permanent per-scene latches).
        // ================================================================
        IEnumerator RunOffsetTrial(int species, string name, int offsetFrames, float contactDelaySeconds, StringBuilder summary)
        {
            var csv = new Csv(SnapCols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SetMeleeTier(0);

            sandbox.TestHarness_SetPlayerX(0f);
            float spawnX = species == 2 ? 140f : 100f;
            sandbox.TestHarness_SpawnDino(species, true);
            yield return null;
            sandbox.TestHarness_SetEnemyX(spawnX);

            bool attackStarted = false;
            float startBudget = Time.time;
            while (Time.time - startBudget < 8f && !attackStarted)
            {
                yield return null;
                var s = Snap(csv, "wait_attack_" + (Time.time - startBudget).ToString("0.000"));
                if (s.enemyAttacking) attackStarted = true;
            }
            if (!attackStarted)
            {
                summary.AppendLine($"=== {name} offset={offsetFrames} ===\nSKIPPED: enemy never entered attack state within budget (bite path may be unimplemented for this species).\n");
                csv.Save(Path.Combine(OutDir, $"n2biteend_{name}_offset{offsetFrames}.csv"));
                yield break;
            }

            bool completed = false;
            float completionTime = -1f;
            float completeBudget = Time.time;
            while (Time.time - completeBudget < 8f && !completed)
            {
                yield return null;
                var s = Snap(csv, "wait_complete_" + (Time.time - completeBudget).ToString("0.000"));
                if (!s.enemyAttacking) { completed = true; completionTime = Time.time; }
            }
            if (!completed)
            {
                summary.AppendLine($"=== {name} offset={offsetFrames} ===\nSKIPPED: bite never completed within budget.\n");
                csv.Save(Path.Combine(OutDir, $"n2biteend_{name}_offset{offsetFrames}.csv"));
                yield break;
            }

            float targetContactTime = completionTime + offsetFrames * SourceFrameSeconds;
            float issueAtTime = targetContactTime - contactDelaySeconds;

            // NOTE: contactDelaySeconds (~0.34s / ~20 source-frames, measured
            // by calibration) is much larger than the +/-3-frame offset
            // window under test, so issueAtTime is ALWAYS earlier than
            // completionTime for every offset in range - this reactive
            // "wait for completion, then issue" scheme can therefore never
            // land the resulting contact closer than ~20 frames AFTER
            // completion, regardless of offsetFrames' sign (confirmed
            // empirically in run 1: achievedOffsetFrames was a constant
            // ~20.4 across every requested offset). This is recorded
            // explicitly rather than hidden - see melee_bite_end_boundary_repro_n2.json
            // "timing_model_limitation". The +/-3-frame requests below are
            // therefore all answered by the SAME "roughly one contact-delay
            // after completion" empirical point; that point is still a
            // valid, useful sample (it lands inside the fuzzy real-world
            // "around bite completion" window the user described) and is
            // reported as such rather than mislabelled as frame-exact.

            float hpBefore = sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            bool issued = false;
            float issueActualTime = -1f;

            // Wait out any pre-existing player hitstun (the dino's bite
            // landing ON the player is part of this exact scenario) so
            // TestHarness_Melee() is not silently rejected by Melee()'s own
            // playerHitStun gate - a rejected call would leave the dbgMelee*
            // fields as stale carryover from an earlier trial in this same
            // scene, exactly the Lesson #4 contamination class already
            // documented in Task027N.1.3.1/N.1.3.2. Confirmed by run 1's
            // data: every trial whose dbgMeleeDamageDispatched read true
            // with NO matching HP drop had playerClipName=="p_behit_A1"
            // (player mid-hurt) at the moment TestHarness_Melee() was called.
            int attempts = 0;
            while (!issued && attempts < 20)
            {
                var pre = sandbox.CaptureRuntimeDebugSnapshot();
                if (pre.playerHitStun)
                {
                    yield return null;
                    Snap(csv, "waiting_out_player_hitstun_" + attempts);
                    attempts++;
                    continue;
                }
                sandbox.TestHarness_Melee();
                issueActualTime = Time.time;
                Snap(csv, "swing_issue_attempt_" + attempts);
                // Confirm the swing genuinely started (playerMelee true)
                // within a few frames before trusting downstream readings.
                bool started = false;
                for (int c = 0; c < 6; c++)
                {
                    yield return null;
                    var cs = Snap(csv, "confirm_start_" + attempts + "_" + c);
                    if (cs.playerMelee) { started = true; break; }
                }
                if (started) { issued = true; }
                else { attempts++; }
            }
            if (!issued)
            {
                summary.AppendLine($"=== {name} offset={offsetFrames} ===\nSKIPPED: could not get an unblocked TestHarness_Melee() start within {attempts} attempts (player kept re-entering hitstun).\n");
                csv.Save(Path.Combine(OutDir, $"n2biteend_{name}_offset{offsetFrames}.csv"));
                yield break;
            }

            // BUG FOUND IN AN EARLIER RUN OF THIS SUITE: breaking on
            // `s.dbgMeleeContactEvent` is wrong - that field is a PERMANENT
            // per-scene latch (never reset to false; only its siblings
            // dbgMeleeTargetFound/dbgMeleeDamageDispatched get reset, and
            // only at the START of a NEW contact event), so it reads true
            // from the moment ANY earlier swing in this trial (including a
            // stale hitstun-retry attempt) ever connected - long before THIS
            // swing's own frame-10 contact actually happens. Combined with a
            // 0.3s cutoff shorter than the measured ~0.34s swing-to-contact
            // delay, the loop was exiting ~40ms BEFORE this swing's real
            // contact fired, capturing HP one frame too early and making a
            // genuinely-undispatched hit look like "damageDispatched=True,
            // HP unchanged" - a TEST artifact, not a gameplay bug. Fixed:
            // wait for this swing's own clip to actually reach frame 10
            // (playerFrame>=10 while still on a p_hit_G clip), with a
            // generous time-based fallback so the loop cannot hang forever.
            float postBudget = Time.time;
            bool reachedContactFrame = false;
            while (Time.time - postBudget < 6f)
            {
                yield return null;
                var s = Snap(csv, "post_" + (Time.time - postBudget).ToString("0.000"));
                if (s.playerFrame >= 10 && s.playerClipName != null && s.playerClipName.StartsWith("p_hit_G"))
                    reachedContactFrame = true;
                if (reachedContactFrame && Time.time - issueActualTime > contactDelaySeconds + 0.15f)
                    break;
                if (Time.time - issueActualTime > contactDelaySeconds + 1.5f)
                    break; // safety fallback if frame-10 is never observed
            }

            var final = sandbox.CaptureRuntimeDebugSnapshot();
            float hpAfter = final.enemyHP;
            csv.Save(Path.Combine(OutDir, $"n2biteend_{name}_offset{offsetFrames}.csv"));

            float achievedOffsetFrames = issued ? (issueActualTime + contactDelaySeconds - completionTime) / SourceFrameSeconds : float.NaN;

            summary.AppendLine($"=== {name} offset={offsetFrames} (requested contact = completion{(offsetFrames>=0?"+":"")}{offsetFrames}f) ===");
            summary.AppendLine($"completionTime={completionTime:0.###} issueAtTime(planned)={issueAtTime:0.###} issueActualTime={issueActualTime:0.###} " +
                $"achievedOffsetFrames={achievedOffsetFrames:0.##} contactFired={final.dbgMeleeContactEvent} targetFound={final.dbgMeleeTargetFound} " +
                $"damageDispatched={final.dbgMeleeDamageDispatched} blockReason='{final.dbgMeleeBlockReason}' " +
                $"hpBefore={hpBefore:0.###} hpAfter={hpAfter:0.###} hpDropped={(hpAfter<hpBefore-0.001f)}");
            summary.AppendLine();
        }

        [UnityTest]
        public IEnumerator OffsetSweep_Type1_K3Blue()
        {
            float contactDelay = ReadCalibration();
            var summary = new StringBuilder();
            foreach (int offset in new[] { -3, -2, -1, 0, 1, 2 })
                yield return RunOffsetTrial(0, "Type1_K3Blue", offset, contactDelay, summary);
            File.AppendAllText(Path.Combine(OutDir, "n2biteend_summary.txt"), summary.ToString());
        }

        [UnityTest]
        public IEnumerator OffsetSweep_Type2_K1Rusher()
        {
            float contactDelay = ReadCalibration();
            var summary = new StringBuilder();
            foreach (int offset in new[] { -3, -2, -1, 0, 1, 2 })
                yield return RunOffsetTrial(1, "Type2_K1Rusher", offset, contactDelay, summary);
            File.AppendAllText(Path.Combine(OutDir, "n2biteend_summary.txt"), summary.ToString());
        }

        [UnityTest]
        public IEnumerator OffsetSweep_Type3_K2Trex()
        {
            float contactDelay = ReadCalibration();
            var summary = new StringBuilder();
            foreach (int offset in new[] { -3, -2, -1, 0, 1, 2 })
                yield return RunOffsetTrial(2, "Type3_K2Trex", offset, contactDelay, summary);
            File.AppendAllText(Path.Combine(OutDir, "n2biteend_summary.txt"), summary.ToString());
        }

        [UnityTest]
        public IEnumerator OffsetSweep_Type4_K4Raptor()
        {
            float contactDelay = ReadCalibration();
            var summary = new StringBuilder();
            foreach (int offset in new[] { -3, -2, -1, 0, 1, 2 })
                yield return RunOffsetTrial(3, "Type4_K4Raptor", offset, contactDelay, summary);
            File.AppendAllText(Path.Combine(OutDir, "n2biteend_summary.txt"), summary.ToString());
        }

        static float ReadCalibration()
        {
            string path = Path.Combine(OutDir, "n2biteend_calibration_result.txt");
            if (!File.Exists(path)) return 10f * SourceFrameSeconds; // fallback: player melee frame-10 contact
            string txt = File.ReadAllText(path);
            int idx = txt.IndexOf('=');
            if (idx < 0) return 10f * SourceFrameSeconds;
            return float.TryParse(txt.Substring(idx + 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v)
                ? v : 10f * SourceFrameSeconds;
        }
    }
}
#endif
