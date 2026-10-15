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
    /// TASK027N.2.3 Parts A/F/M. Reuses Task027N22BiteBoundaryPredictive's
    /// calibrated predictive-scheduling method (same contactDelay/
    /// biteDuration measurement approach, same TestHarness_* hooks) but adds
    /// full "tou" anchor logging (Part A) and tests BOTH facing directions
    /// for Type3 (Part F) via a mirrored spawn side. Part M (Type2 harness
    /// anomaly) is investigated separately below, reusing the same
    /// calibration helpers.
    ///
    /// This file does not modify or re-run Task027N22BiteBoundaryPredictive
    /// itself - its N.2.2 results are frozen. All logging hooks added to
    /// CombatSandboxV2.cs for this purpose (TestHarness_TryGetEnemyTouWorldX,
    /// TestHarness_EnemyActorScale, TestHarness_EnemyVisualFacingRight,
    /// TestHarness_EnemyPosition, TestHarness_PlayerPosition) are strictly
    /// read-only accessors mirroring exactly what ExecuteMeleeContact()
    /// itself reads - zero gameplay effect.
    /// </summary>
    public class Task027N23AnchorTrace
    {
        const string ScenePath = "Assets/DinoCap/Scenes/CombatSandboxV2.unity";
        const float SourceFrameSeconds = 1f / 60f;
        static readonly string OutDir =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_2_BloodAudioBiteEndDebugGodMode/raw"));

        CombatSandboxV2 sandbox;
        float contactDelaySeconds = -1f;

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
            // Same rationale as N.2.2: God Mode blocks only incoming HP loss/
            // death, never outgoing melee/contact (the thing under test), and
            // preserves hurt-reaction/hitstun/knockback timing.
            sandbox.TestHarness_SetGodMode(true);
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

        static readonly string[] Cols = {
            "tag","time","dinoRootX","touWorldX","touLocalX_derived",
            "playerRootX","playerMeleeReferenceX","dxUsedByMeleeTest",
            "dinoAttackFrame","dinoAttackClip","enemyVisualFacingRight","enemyFacingRightLogical",
            "enemyActorScale","playerFacingRight","targetFound","damageDispatched",
            "playerFrame","playerClipName","meleeContactEvent"
        };

        // Replicates ExecuteMeleeContact()'s own dx formula exactly, using
        // only read-only accessors - does not call ExecuteMeleeContact.
        float ComputeDx(bool playerFacingRight, float touWorldX, float playerRootX) =>
            playerFacingRight ? touWorldX - playerRootX : playerRootX - touWorldX;

        void Snap(Csv csv, string tag)
        {
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            float touWorldX;
            bool haveTou = sandbox.TestHarness_TryGetEnemyTouWorldX(out touWorldX);
            float dinoRootX = sandbox.TestHarness_EnemyPosition.x;
            float playerRootX = sandbox.TestHarness_PlayerPosition.x;
            float touLocal = haveTou ? touWorldX - dinoRootX : float.NaN;
            float dx = haveTou ? ComputeDx(s.playerFacingRight, touWorldX, playerRootX) : float.NaN;

            csv.Row(tag, s.timeNow, dinoRootX, haveTou ? touWorldX : (float?)null, touLocal,
                playerRootX, playerRootX, dx,
                s.enemyFrame, s.enemyClipName, sandbox.TestHarness_EnemyVisualFacingRight, s.enemyFacingRightOut,
                sandbox.TestHarness_EnemyActorScale, s.playerFacingRight, s.dbgMeleeTargetFound, s.dbgMeleeDamageDispatched,
                s.playerFrame, s.playerClipName, s.dbgMeleeContactEvent);
        }

        IEnumerator CalibrateContactDelay()
        {
            var csv = new Csv(Cols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SetMeleeTier(0);
            Snap(csv, "pre");
            float t0 = Time.time;
            sandbox.TestHarness_Melee();
            float budget = Time.time;
            while (Time.time - budget < 5f)
            {
                yield return null;
                var s = sandbox.CaptureRuntimeDebugSnapshot();
                Snap(csv, "cal");
                if (s.dbgMeleeContactEvent) { contactDelaySeconds = Time.time - t0; break; }
            }
            csv.Save(Path.Combine(OutDir, "n23_contactdelay_calibration.csv"));
            if (contactDelaySeconds < 0f) contactDelaySeconds = 20f * SourceFrameSeconds;
        }

        struct RunConfig { public bool rightSide; public float adis; public float spawnAbsX; public string label; public int species; }

        IEnumerator WaitForCleanPlayerState()
        {
            float budget = Time.time;
            while (Time.time - budget < 3f)
            {
                var s = sandbox.CaptureRuntimeDebugSnapshot();
                if (!s.playerHitStun && !s.playerMelee) break;
                yield return null;
            }
            sandbox.TestHarness_ForceInvulnExpired();
        }

        IEnumerator MeasureBiteDuration(RunConfig cfg, StringBuilder log, System.Action<float> onResult, float completionWaitBudget)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(cfg.species, cfg.rightSide);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            float spawnX = cfg.rightSide ? cfg.spawnAbsX : -cfg.spawnAbsX;
            sandbox.TestHarness_SetEnemyX(spawnX);
            sandbox.TestHarness_SetEnemyRuntimeAdis(cfg.adis);
            sandbox.TestHarness_SetPlayerInvulnerableForSeconds(completionWaitBudget + 5f);

            bool started = false; float t0 = -1f;
            float budget = Time.time;
            while (Time.time - budget < 15f && !started)
            {
                yield return null;
                if (sandbox.TestHarness_EnemyAttacking) { started = true; t0 = Time.time; }
            }
            if (started)
                sandbox.TestHarness_SetEnemyRuntimeAdis(Mathf.Max(1f, cfg.spawnAbsX - 40f));
            if (!started)
            {
                log.AppendLine($"{cfg.label}: bite never started within 15s budget.");
                onResult(-1f);
                yield break;
            }

            float budget2 = Time.time;
            while (Time.time - budget2 < completionWaitBudget)
            {
                yield return null;
                if (!sandbox.TestHarness_EnemyAttacking)
                {
                    float dur = Time.time - t0;
                    log.AppendLine($"{cfg.label}: measured bite duration = {dur:0.0000}s ({(dur/SourceFrameSeconds):0.0} source frames).");
                    onResult(dur);
                    yield break;
                }
            }
            log.AppendLine($"{cfg.label}: bite never completed within a {completionWaitBudget}s budget.");
            onResult(-1f);
        }

        IEnumerator RunAnchorTrial(
            RunConfig cfg, float biteDuration, int offsetFrames,
            StringBuilder summaryLog, Dictionary<int, string> keyFrameSummaries)
        {
            var csv = new Csv(Cols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            yield return WaitForCleanPlayerState();
            sandbox.TestHarness_SpawnDino(cfg.species, cfg.rightSide);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            float spawnX = cfg.rightSide ? cfg.spawnAbsX : -cfg.spawnAbsX;
            sandbox.TestHarness_SetEnemyX(spawnX);
            sandbox.TestHarness_SetEnemyRuntimeAdis(cfg.adis);
            sandbox.TestHarness_SetMeleeTier(0);
            // Part M finding (this file, run 1): Type2/K1Rusher has only 1
            // native HP - the test's OWN melee swing (needed to measure
            // contact timing) kills it outright near every offset, which
            // corrupts the "bite completion" signal into a DEATH event
            // (dinoAttackClip observed as "k1_die") instead of a natural
            // clip-end, collapsing the measured achieved-offset toward the
            // death moment regardless of requested offset - the root cause
            // of N.2.2's Type2 "-3/-2/-1 all read 0.00" anomaly. Same fix
            // N.2.2 itself already used for an analogous one-shot-kill
            // confound (see contradiction_resolution_iterations_n22.json).
            sandbox.TestHarness_SetDebugEnemyHP(999f);
            sandbox.TestHarness_SetPlayerInvulnerableForSeconds(biteDuration + 5f);

            bool started = false; float t0 = -1f;
            float budget = Time.time;
            while (Time.time - budget < 15f && !started)
            {
                yield return null;
                Snap(csv, "wait_start");
                if (sandbox.TestHarness_EnemyAttacking) { started = true; t0 = Time.time; }
            }
            if (started)
                sandbox.TestHarness_SetEnemyRuntimeAdis(Mathf.Max(1f, cfg.spawnAbsX - 40f));
            if (!started)
            {
                summaryLog.AppendLine($"{cfg.label} offset={offsetFrames}: SKIPPED bite never started.");
                csv.Save(Path.Combine(OutDir, $"n23_anchor_{cfg.label}_off{offsetFrames}.csv"));
                yield break;
            }

            float predictedCompletion = t0 + biteDuration;
            float issueAtTime = predictedCompletion - contactDelaySeconds + offsetFrames * SourceFrameSeconds;

            while (Time.time < issueAtTime)
            {
                yield return null;
                Snap(csv, "wait_issue");
            }

            bool issued = false;
            float issueBudget = Time.time;
            while (!issued && Time.time - issueBudget < 3f)
            {
                var pre = sandbox.CaptureRuntimeDebugSnapshot();
                if (pre.playerHitStun || pre.playerDead || pre.playerMelee)
                {
                    yield return null;
                    Snap(csv, "waiting_out_hitstun");
                    continue;
                }
                sandbox.TestHarness_Melee();
                Snap(csv, "swing_issued");
                issued = true;
            }
            if (!issued)
            {
                summaryLog.AppendLine($"{cfg.label} offset={offsetFrames}: SKIPPED could not issue within 3s.");
                csv.Save(Path.Combine(OutDir, $"n23_anchor_{cfg.label}_off{offsetFrames}.csv"));
                yield break;
            }

            float actualCompletionTime = sandbox.TestHarness_EnemyAttacking ? -1f : Time.time;
            float actualContactTime = -1f;
            string keyFrameLine = null;

            float postBudget = Time.time;
            while (Time.time - postBudget < 6f)
            {
                yield return null;
                var s = Snap2(csv);
                if (actualCompletionTime < 0f && !sandbox.TestHarness_EnemyAttacking)
                    actualCompletionTime = Time.time;
                if (actualContactTime < 0f && s.dbgMeleeContactEvent && s.playerFrame >= 10 &&
                    s.playerClipName != null && s.playerClipName.StartsWith("p_hit_G"))
                {
                    actualContactTime = Time.time;
                    float touWorldX; sandbox.TestHarness_TryGetEnemyTouWorldX(out touWorldX);
                    float dinoRootX = sandbox.TestHarness_EnemyPosition.x;
                    float playerRootX = sandbox.TestHarness_PlayerPosition.x;
                    float dx = ComputeDx(s.playerFacingRight, touWorldX, playerRootX);
                    keyFrameLine = $"AT_CONTACT: dinoRootX={dinoRootX:0.00} touWorldX={touWorldX:0.00} touLocalX={touWorldX-dinoRootX:0.00} " +
                        $"playerRootX={playerRootX:0.00} dxUsed={dx:0.00} enemyVisualFacingRight={sandbox.TestHarness_EnemyVisualFacingRight} " +
                        $"playerFacingRight={s.playerFacingRight} dinoAttackFrame={s.enemyFrame} dinoAttackClip={s.enemyClipName} " +
                        $"TARGET_FOUND={s.dbgMeleeTargetFound} DAMAGE_DISPATCHED={s.dbgMeleeDamageDispatched}";
                }
                if (actualCompletionTime >= 0f && actualContactTime >= 0f) break;
            }

            var final = sandbox.CaptureRuntimeDebugSnapshot();
            bool haveBoth = actualCompletionTime >= 0f && actualContactTime >= 0f;
            float achievedOffset = haveBoth ? (actualContactTime - actualCompletionTime) / SourceFrameSeconds : float.NaN;

            csv.Save(Path.Combine(OutDir, $"n23_anchor_{cfg.label}_off{offsetFrames}.csv"));

            summaryLog.AppendLine(
                $"{cfg.label} REQUESTED_OFFSET={offsetFrames} ACHIEVED_OFFSET={achievedOffset:0.00} " +
                $"TARGET_FOUND={final.dbgMeleeTargetFound} DAMAGE_DISPATCHED={final.dbgMeleeDamageDispatched} | {keyFrameLine}");
            keyFrameSummaries[offsetFrames] = keyFrameLine ?? "NO_CONTACT_EVENT_OBSERVED";
        }

        CombatSandboxV2.RuntimeDebugSnapshot Snap2(Csv csv)
        {
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            Snap(csv, "post");
            return s;
        }

        IEnumerator RunFullSweep(RunConfig cfg)
        {
            var schedulerLog = new StringBuilder();
            var summaryLog = new StringBuilder();
            var keyFrames = new Dictionary<int, string>();

            yield return CalibrateContactDelay();
            schedulerLog.AppendLine($"{cfg.label}: contactDelaySeconds={contactDelaySeconds:0.0000}s");

            float dur = -1f;
            yield return MeasureBiteDuration(cfg, schedulerLog, v => dur = v, 60f);
            File.AppendAllText(Path.Combine(OutDir, "n23_scheduler_log.txt"), schedulerLog.ToString());

            if (dur < 0f)
            {
                summaryLog.AppendLine($"{cfg.label}: SKIPPED ENTIRE SWEEP - bite duration could not be measured.");
                File.AppendAllText(Path.Combine(OutDir, "n23_anchor_summary.txt"), summaryLog.ToString());
                yield break;
            }

            int[] offsets = { -3, -2, -1, 0, 1, 2, 3 };
            foreach (int off in offsets)
                yield return RunAnchorTrial(cfg, dur, off, summaryLog, keyFrames);

            File.AppendAllText(Path.Combine(OutDir, "n23_anchor_summary.txt"), summaryLog.ToString());

            var jsonLike = new StringBuilder();
            jsonLike.AppendLine($"=== {cfg.label} KEY FRAMES (JSON-ready) ===");
            foreach (var kv in keyFrames)
                jsonLike.AppendLine($"offset {kv.Key}: {kv.Value}");
            File.AppendAllText(Path.Combine(OutDir, "n23_key_frames.txt"), jsonLike.ToString());
        }

        // Part A + baseline: matches N.2.2's exact convention (spawn right,
        // adis=280, spawnAbsX=120) - dino attacks LEFTWARD.
        [UnityTest, Timeout(300000)]
        public IEnumerator Type3_RightSide_AttackingLeft() =>
            RunFullSweep(new RunConfig{rightSide=true,adis=280f,spawnAbsX=120f,label="Type3_RightSide_AttackLeft",species=2});

        // Part F: mirrored - dino spawns LEFT of player, attacks RIGHTWARD.
        [UnityTest, Timeout(300000)]
        public IEnumerator Type3_LeftSide_AttackingRight() =>
            RunFullSweep(new RunConfig{rightSide=false,adis=280f,spawnAbsX=120f,label="Type3_LeftSide_AttackRight",species=2});

        // Part M (light touch): re-verify Type2's -3/-2/-1 "achieved=0.00"
        // anomaly using this file's cleaner/independently-written harness -
        // matches N.2.2's own RightSide/adis=190/spawnX=120 convention
        // exactly, to see if the anomaly is a property of Type2's own
        // timing (reproduces here too) or was specific to the N.2.2 test
        // file's particular code path (does NOT reproduce here).
        [UnityTest, Timeout(300000)]
        public IEnumerator Type2_RightSide_AttackingLeft_HarnessCheck() =>
            RunFullSweep(new RunConfig{rightSide=true,adis=190f,spawnAbsX=120f,label="Type2_RightSide_HarnessCheck",species=1});
    }
}
#endif
