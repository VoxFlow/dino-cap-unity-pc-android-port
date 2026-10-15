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
    /// TASK027N.2.2 Parts E/F/H - PREDICTIVE bite-completion scheduler.
    ///
    /// Task027N.2's own bite-boundary suite (Task027N2BiteEndBoundaryRepro)
    /// was REACTIVE (wait for enemyAttacking to go false, THEN issue the
    /// swing) - since a swing's own frame-10 contact takes ~20 source
    /// frames (~0.34s) after being issued, that scheme could only ever land
    /// contact ~20 frames AFTER completion, never testing the reported
    /// tight +/-3 frame window. This suite is PREDICTIVE: it measures (a)
    /// the swing-issue-to-frame-10-contact delay and (b) each species'
    /// bite-start-to-completion duration (with enemyRuntimeAdis jitter
    /// pinned via TestHarness_SetEnemyRuntimeAdis for repeatability), then
    /// issues the swing EARLY enough that its predicted contact moment
    /// lands at the requested offset relative to the PREDICTED completion
    /// moment. The prediction is never trusted blindly: every trial
    /// independently MEASURES the actual completion time and actual
    /// contact time after the fact and reports the ACTUAL achieved offset,
    /// never the requested one.
    /// </summary>
    public class Task027N22BiteBoundaryPredictive
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

            // BUGFIX (this task, run 2): each species sweep runs up to 14
            // trials in ONE continuous session, and unlike enemyHP (reset by
            // TestHarness_ClearDinos()+SpawnDino() every trial), PLAYER HP is
            // never reset between trials - only the dino's own bite is
            // cleared. Since every trial's dino genuinely lands its one
            // frame-12 hit during the multi-second "wait until issueAtTime"
            // window, cumulative damage across ~10 trials silently killed
            // the player mid-sweep (observed: playerClipName="p_die" and a
            // stale "no live target" block reason on later trials, because
            // a dead player's Melee() call is a silent no-op that this
            // test's own issue-retry loop didn't detect). God Mode is the
            // correct, already-proven DEBUG_ONLY tool for exactly this case
            // (Task027N.2 Part T-W: blocks HP loss/death only, never touches
            // outgoing player damage/melee-contact - the thing this suite
            // actually tests - and preserves hurt-reaction/hitstun/knockback
            // timing, which the schedule still needs to be realistic).
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

        static readonly string[] SnapCols = {
            "tag","time","playerMelee","playerHitStun","enemyAttacking","enemyHP",
            "playerX","enemyX","dbgMeleeContactEvent","dbgMeleeTargetFound",
            "dbgMeleeDamageDispatched","dbgMeleeBlockReason",
            "playerClipName","playerFrame","enemyClipName","enemyFrame",
            "playerFacingRight","enemyFacingRightOut"
        };

        CombatSandboxV2.RuntimeDebugSnapshot Snap(Csv csv, string tag)
        {
            var s = sandbox.CaptureRuntimeDebugSnapshot();
            csv.Row(tag, s.timeNow, s.playerMelee, s.playerHitStun, sandbox.TestHarness_EnemyAttacking, s.enemyHP,
                s.playerX, s.enemyX, s.dbgMeleeContactEvent, s.dbgMeleeTargetFound,
                s.dbgMeleeDamageDispatched, s.dbgMeleeBlockReason,
                s.playerClipName, s.playerFrame, s.enemyClipName, s.enemyFrame,
                s.playerFacingRight, s.enemyFacingRightOut);
            return s;
        }

        IEnumerator CalibrateContactDelay()
        {
            var csv = new Csv(SnapCols);
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
                var s = Snap(csv, "cal_" + (Time.time - budget).ToString("0.000"));
                if (s.dbgMeleeContactEvent) { contactDelaySeconds = Time.time - t0; break; }
            }
            csv.Save(Path.Combine(OutDir, "n22_contactdelay_calibration.csv"));
            if (contactDelaySeconds < 0f) contactDelaySeconds = 20f * SourceFrameSeconds;
        }

        struct SpeciesConfig { public int species; public string name; public float adis; public float spawnX; }

        // BUGFIX (this task, run 9): at spawnX=60, Type1/2/3's "tou" head
        // anchor during the attack-lunge POSE lands BEHIND the player
        // (measured dx=-27.5 for Type1) for the dino's ENTIRE bite duration
        // (confirmed: EVERY offset from -3..+3 showed the identical
        // TARGET_FOUND=False, proving this is a fixed positional artifact
        // of this test's spawn distance, not a timing-boundary effect at
        // all). Type4's own attack pose happens not to overshoot as far at
        // this distance. Moved to 120 (a safe margin beyond each species'
        // own lunge reach) so the -3..+3 sweep measures the ACTUAL
        // timing question this task exists to answer, not a positioning
        // artifact - see melee_exact_bite_boundary_matrix_n22.json for the
        // full before/after data.
        static readonly SpeciesConfig[] Species = {
            new SpeciesConfig{species=0,name="Type1_K3Blue",adis=190f,spawnX=120f},
            new SpeciesConfig{species=1,name="Type2_K1Rusher",adis=190f,spawnX=120f},
            new SpeciesConfig{species=2,name="Type3_K2Trex",adis=280f,spawnX=120f},
            new SpeciesConfig{species=3,name="Type4_K4Raptor",adis=190f,spawnX=120f},
        };

        IEnumerator MeasureBiteDuration(SpeciesConfig cfg, StringBuilder log, System.Action<float> onResult, float completionWaitBudget)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(cfg.species, true);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(cfg.spawnX);
            sandbox.TestHarness_SetEnemyRuntimeAdis(cfg.adis);
            // Keep the player free to swing across the WHOLE bite window -
            // see TestHarness_SetPlayerInvulnerableForSeconds's doc comment.
            sandbox.TestHarness_SetPlayerInvulnerableForSeconds(completionWaitBudget + 5f);

            bool started = false; float t0 = -1f;
            float budget = Time.time;
            while (Time.time - budget < 10f && !started)
            {
                yield return null;
                if (sandbox.TestHarness_EnemyAttacking) { started = true; t0 = Time.time; }
            }
            if (started)
            {
                // BUGFIX (this task, run 4): CompleteEnemyAttack() only ends
                // the attack (enemyAttacking->false) if the player is OUT of
                // the SAME live enemyRuntimeAdis cone at the clip's natural
                // end - normally guaranteed by the frame-12 hit's own
                // knockback pushing the player away. Making the player
                // invulnerable (above) to keep them free to swing also
                // silently disabled that knockback, so with the ORIGINAL
                // (large) adis still in effect the dino re-bit forever
                // (observed: 0/4 species completed within 15-60s). Since dx
                // (spawnX) is fixed and known, shrinking adis below it right
                // after the bite starts reproduces a clean "player now out
                // of cone" completion at the clip's natural end WITHOUT
                // relying on knockback - decoupling this test's needed
                // signal (one clean completion) from the incoming-damage
                // mechanic entirely, rather than fighting it.
                sandbox.TestHarness_SetEnemyRuntimeAdis(Mathf.Max(1f, cfg.spawnX - 40f));
            }
            if (!started)
            {
                log.AppendLine($"{cfg.name}: bite never started within 10s budget (bite path likely non-functional).");
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
                    log.AppendLine($"{cfg.name}: measured bite duration = {dur:0.0000}s ({(dur/SourceFrameSeconds):0.0} source frames).");
                    onResult(dur);
                    yield break;
                }
            }
            log.AppendLine($"{cfg.name}: bite never completed within a {completionWaitBudget}s budget after starting (possible sustained re-bite / cone never exited / long clip).");
            onResult(-1f);
        }

        // ================================================================
        // BUGFIX (this task, run 1): multiple trials/offsets share ONE
        // continuous PlayMode session (no scene reload between them, only
        // TestHarness_ClearDinos()+respawn). ClearDinos()/SpawnDino() only
        // touch enemy-side state - they do NOT clear a still-in-progress
        // playerHitStun/playerHurtVelocityX left over from the PREVIOUS
        // trial's own knockback. Observed effect: PLAYER_X drifted to ~-294
        // (vs the intended 0) because Update()'s hitstun block kept
        // integrating a STALE nonzero playerHurtVelocityX for the ~4s
        // "wait until issueAtTime" loop, pushing the player far enough away
        // that ExecuteMeleeContact's dx-range test then legitimately failed
        // (TARGET_FOUND=False in every trial) - a TEST setup artifact, not
        // a gameplay divergence. Fixed by waiting out any residual hitstun
        // and force-expiring invuln BEFORE repositioning for a new trial.
        // ================================================================
        IEnumerator WaitForCleanPlayerState()
        {
            float budget = Time.time;
            while (Time.time - budget < 3f)
            {
                // BUGFIX (this task, run 7): also wait out playerMelee, not
                // just playerHitStun - the PREVIOUS offset's own swing clip
                // can still be active when the next trial starts (all 7
                // offsets share one continuous scene), and Melee()'s own
                // gate (`if(playerDead||playerHitStun||playerMelee) return;`)
                // silently rejects a new swing while it is, which otherwise
                // produced a consistent ~+6-frame drift once the earlier
                // playerHitStun-only wait let trials start too early.
                var s = sandbox.CaptureRuntimeDebugSnapshot();
                if (!s.playerHitStun && !s.playerMelee) break;
                yield return null;
            }
            sandbox.TestHarness_ForceInvulnExpired();
        }

        // ================================================================
        // Predictive trial: press melee EARLY so its frame-10 contact lands
        // near completion+offsetFrames. Records the REQUESTED offset AND
        // independently measures the ACTUAL achieved offset from real
        // completion/contact timestamps - never assumes the prediction hit.
        // ================================================================
        IEnumerator RunPredictiveTrial(
            SpeciesConfig cfg, float biteDuration, int offsetFrames, int trialIndex,
            StringBuilder matrixLog, StringBuilder hostOrderLog)
        {
            var csv = new Csv(SnapCols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            yield return WaitForCleanPlayerState();
            sandbox.TestHarness_SpawnDino(cfg.species, true);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(cfg.spawnX);
            sandbox.TestHarness_SetEnemyRuntimeAdis(cfg.adis);
            sandbox.TestHarness_SetMeleeTier(0);
            // Keep the player free to swing across the WHOLE bite window -
            // see TestHarness_SetPlayerInvulnerableForSeconds's doc comment.
            // Without this, run 3's data showed the player's OWN hitstun
            // from this same bite's frame-12 hit blocked any swing attempt
            // until ~10 frames before completion, producing a systematic
            // ~+9.5 frame bias on EVERY requested offset regardless of sign.
            sandbox.TestHarness_SetPlayerInvulnerableForSeconds(biteDuration + 5f);

            bool started = false; float t0 = -1f;
            float budget = Time.time;
            while (Time.time - budget < 10f && !started)
            {
                yield return null;
                Snap(csv, "wait_start");
                if (sandbox.TestHarness_EnemyAttacking) { started = true; t0 = Time.time; }
            }
            if (started)
                sandbox.TestHarness_SetEnemyRuntimeAdis(Mathf.Max(1f, cfg.spawnX - 40f)); // see MeasureBiteDuration's doc comment
            if (!started)
            {
                matrixLog.AppendLine($"{cfg.name} offset={offsetFrames} trial={trialIndex}: SKIPPED bite never started.");
                csv.Save(Path.Combine(OutDir, $"n22_matrix_{cfg.name}_off{offsetFrames}_t{trialIndex}.csv"));
                yield break;
            }

            float predictedCompletion = t0 + biteDuration;
            float issueAtTime = predictedCompletion - contactDelaySeconds + offsetFrames * SourceFrameSeconds;

            float hpBefore = sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            NativeAudioSessionGate.TestHarness_ResetClipCounters();

            while (Time.time < issueAtTime)
            {
                yield return null;
                Snap(csv, "wait_issue");
            }

            var dinoStateBefore = sandbox.TestHarness_EnemyAttacking;
            // BUGFIX (this task, run 2): was an ATTEMPT-count cap (15 ticks),
            // not a TIME budget - under batchmode's fast unthrottled tick
            // rate (~0.4ms/tick observed elsewhere in this project's own
            // tests), 15 ticks is only ~6ms of real game time, nowhere near
            // enough to wait out a normal hitstun window lasting hundreds of
            // ms. Same class of mistake as Task027N.1.3.1's Lesson #1.
            // BUGFIX (this task, run 6): the PREVIOUS offset's own swing
            // clip (this species sweep runs all 7 offsets in ONE continuous
            // scene) can still be playing - Melee()'s OWN gate
            // (`if(playerDead||playerHitStun||playerMelee) return;`) then
            // silently rejects a same-frame new TestHarness_Melee() call,
            // and since this loop previously checked only hitStun/dead, it
            // wrongly reported "issued=true" while ExecuteMeleeContact()
            // never actually ran again - leaving dbgMeleeTargetFound/
            // dbgMeleeDamageDispatched as a STALE true carried over from the
            // PRIOR offset's real hit (both fields ARE freshly reset at the
            // top of every genuine ExecuteMeleeContact() call, but a
            // rejected Melee() call never reaches that reset). Observed as
            // every-other-offset (-2/0/+2) showing a ~20-frame-earlier
            // "achieved" offset alongside HP_DROPPED=False despite a
            // stale-true DAMAGE_DISPATCHED. Fixed by also waiting out
            // pre.playerMelee here.
            bool issued = false; float issueActualTime = -1f;
            float issueBudget = Time.time;
            int waitTick = 0;
            while (!issued && Time.time - issueBudget < 3f)
            {
                var pre = sandbox.CaptureRuntimeDebugSnapshot();
                if (pre.playerHitStun || pre.playerDead || pre.playerMelee)
                {
                    yield return null;
                    Snap(csv,"waiting_out_hitstun_"+waitTick);
                    waitTick++;
                    continue;
                }
                sandbox.TestHarness_Melee();
                issueActualTime = Time.time;
                Snap(csv, "swing_issued");
                issued = true;
            }
            if (!issued)
            {
                matrixLog.AppendLine($"{cfg.name} offset={offsetFrames} trial={trialIndex}: SKIPPED could not issue within 3s (repeated hitstun, player dead, or previous swing still active).");
                csv.Save(Path.Combine(OutDir, $"n22_matrix_{cfg.name}_off{offsetFrames}_t{trialIndex}.csv"));
                yield break;
            }

            float actualCompletionTime = sandbox.TestHarness_EnemyAttacking ? -1f : issueActualTime;
            float actualContactTime = -1f;
            bool logHostOrder = System.Math.Abs(offsetFrames) <= 1;
            string hostOrderSeq = "";

            float postBudget = Time.time;
            while (Time.time - postBudget < 6f)
            {
                yield return null;
                var s = Snap(csv, "post");
                if (actualCompletionTime < 0f && !sandbox.TestHarness_EnemyAttacking)
                {
                    actualCompletionTime = Time.time;
                    if (logHostOrder) hostOrderSeq += "[BITE_COMPLETE t="+Time.time.ToString("0.00000")+"] ";
                }
                if (actualContactTime < 0f && s.dbgMeleeContactEvent && s.playerFrame >= 10 &&
                    s.playerClipName != null && s.playerClipName.StartsWith("p_hit_G"))
                {
                    actualContactTime = Time.time;
                    if (logHostOrder) hostOrderSeq += "[CONTACT t="+Time.time.ToString("0.00000")+" playerFrame="+s.playerFrame+"] ";
                }
                if (actualCompletionTime >= 0f && actualContactTime >= 0f) break;
            }

            var final = sandbox.CaptureRuntimeDebugSnapshot();
            float hpAfter = final.enemyHP;
            bool haveBoth = actualCompletionTime >= 0f && actualContactTime >= 0f;
            float achievedOffset = haveBoth ? (actualContactTime - actualCompletionTime) / SourceFrameSeconds : float.NaN;

            int bloodCount = DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood);
            int contactAudio = NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_att1");

            csv.Save(Path.Combine(OutDir, $"n22_matrix_{cfg.name}_off{offsetFrames}_t{trialIndex}.csv"));

            matrixLog.AppendLine(
                $"species={cfg.name} trial={trialIndex} REQUESTED_OFFSET={offsetFrames} ACTUAL_CONTACT_OFFSET_SOURCE_FRAMES={achievedOffset:0.00} " +
                $"PLAYER_MELEE_FRAME={final.playerFrame} MELEE_CONTACT_EVENT={final.dbgMeleeContactEvent} " +
                $"DINO_STATE_BEFORE_ATTACKING={dinoStateBefore} DINO_STATE_AFTER_ATTACKING={sandbox.TestHarness_EnemyAttacking} " +
                $"PLAYER_X={final.playerX:0.0} DINO_ANCHOR_X={final.enemyX:0.0} " +
                $"TARGET_FOUND={final.dbgMeleeTargetFound} DAMAGE_DISPATCHED={final.dbgMeleeDamageDispatched} " +
                $"HP_BEFORE={hpBefore:0.###} HP_AFTER={hpAfter:0.###} HP_DROPPED={(hpAfter<hpBefore-0.001f)} " +
                $"CONTACT_AUDIO_att1_COUNT={contactAudio} BLOOD_EVENT_COUNT={bloodCount} " +
                $"completionMeasured={actualCompletionTime>=0f} contactMeasured={actualContactTime>=0f}");

            if (logHostOrder)
                hostOrderLog.AppendLine($"{cfg.name} requestedOffset={offsetFrames} trial={trialIndex}: {hostOrderSeq} (issueAtTime={issueAtTime:0.00000} issueActualTime={issueActualTime:0.00000} t0_biteStart={t0:0.00000} predictedCompletion={predictedCompletion:0.00000})");
        }

        // Split per-species (rather than one giant test) so NUnit's per-test
        // timeout applies independently to each - Type1/Type2's own bite
        // clips run a genuinely long ~4.3-4.8s (256/288 source frames;
        // 256/60=4.2667s matches Type1's measured 4.2673s almost exactly,
        // i.e. this is the length of ONE uninterrupted "_attack" clip, not a
        // re-bite loop), and 14 trials/species at that cost alone approaches
        // the combined test's original 180s ceiling before Type3/Type4 are
        // even reached. Timeout raised explicitly per species below.
        IEnumerator RunSpeciesSweep(SpeciesConfig cfg, float completionWaitBudget, int trialsPerOffset)
        {
            var schedulerLog = new StringBuilder();
            var matrixLog = new StringBuilder();
            var hostOrderLog = new StringBuilder();

            yield return CalibrateContactDelay();
            schedulerLog.AppendLine($"contactDelaySeconds (swing issue -> frame10 contact) = {contactDelaySeconds:0.0000}s ({(contactDelaySeconds/SourceFrameSeconds):0.0} source frames).");

            float dur = -1f;
            yield return MeasureBiteDuration(cfg, schedulerLog, v => dur = v, completionWaitBudget);
            File.AppendAllText(Path.Combine(OutDir, "n22_scheduler_calibration_log.txt"), schedulerLog.ToString());

            if (dur < 0f)
            {
                matrixLog.AppendLine($"species={cfg.name}: SKIPPED ENTIRE SPECIES - bite duration could not be measured within a {completionWaitBudget}s budget (non-functional or non-completing bite path in this port).");
                File.AppendAllText(Path.Combine(OutDir, "n22_matrix_summary.txt"), matrixLog.ToString());
                yield break;
            }

            int[] offsets = { -3, -2, -1, 0, 1, 2, 3 };
            foreach (int off in offsets)
                for (int trial = 0; trial < trialsPerOffset; trial++)
                    yield return RunPredictiveTrial(cfg, dur, off, trial, matrixLog, hostOrderLog);

            File.AppendAllText(Path.Combine(OutDir, "n22_matrix_summary.txt"), matrixLog.ToString());
            File.AppendAllText(Path.Combine(OutDir, "n22_host_order_log.txt"), hostOrderLog.ToString());
        }

        // BUGFIX (this task, run 5): a 2nd trial sharing the same scene as
        // trial 0 picked up a STALE playerFrame/clip-state carryover from
        // trial 0's own finished swing, satisfying this test's contact-
        // detection condition (dbgMeleeContactEvent - a permanent per-scene
        // latch - AND playerFrame>=10 on a p_hit_G clip) on the very first
        // post-issue poll tick instead of waiting for THIS trial's real
        // frame-10 crossing, producing achieved offsets off by a full
        // ~20-frame contactDelay. Reduced to 1 trial/offset (matching
        // Type3/4 already below) rather than adding a second, more complex
        // isolation mechanism for a "detect instability" nice-to-have this
        // task explicitly said could stay minimal ("a handful... not
        // hundreds"); trial=0's own data across all 4 species already gives
        // clean, honestly-measured full -3..+3 coverage - see
        // contradiction_resolution_iterations_n22.json for this class of
        // finding recorded in full.
        [UnityTest, Timeout(300000)]
        public IEnumerator Sweep_Type1_K3Blue() => RunSpeciesSweep(Species[0], 15f, 1);

        [UnityTest, Timeout(300000)]
        public IEnumerator Sweep_Type2_K1Rusher() => RunSpeciesSweep(Species[1], 15f, 1);

        // Type3: calibration in the first attempt did not complete within a
        // 10s budget (unlike Type1/Type2's clean single ~4.3-4.8s clip) -
        // given a much longer budget here specifically to determine whether
        // this is a longer clip/an actual multi-cycle re-bite, or a genuinely
        // non-completing path, rather than assuming either.
        [UnityTest, Timeout(300000)]
        public IEnumerator Sweep_Type3_K2Trex() => RunSpeciesSweep(Species[2], 60f, 1);

        // Type4: N.2's own bite-boundary fork already found this species'
        // bite path unreliable/non-functional in this port; reverified
        // (not assumed) below with a generous start-wait, matching that
        // precedent rather than re-litigating it at length.
        [UnityTest, Timeout(120000)]
        public IEnumerator Sweep_Type4_K4Raptor() => RunSpeciesSweep(Species[3], 15f, 1);
    }
}
#endif
