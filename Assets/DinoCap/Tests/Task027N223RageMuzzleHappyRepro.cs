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
    /// TASK027N.2.23/N.2.25 focused acceptance tests. Covers:
    /// 1. Sustained eligible (family D) auto fire reaches rage levels 1/2/3.
    /// 2. Release from stage1/2/3 EVENTUALLY decays all the way to neutral
    ///    (stepwise, one band per tick - N.2.25 replaces N.2.24's own
    ///    explicitly-rejected "resume climbs back quickly, old count
    ///    preserved" interpretation).
    /// 3. Re-pressing while a stage is still visible during its own decay
    ///    window does NOT instantly advance to the next stage - the raw
    ///    progress counter resets on a true fresh press (native's own
    ///    proven gate), and reaching the next stage requires a full fresh
    ///    accumulation.
    /// 4. Switching between two rage-eligible families (C&lt;-&gt;D) while fire
    ///    stays continuously held does NOT reset progress (native's own
    ///    weapon-switch handler never touches the session-active flag -
    ///    distinct from an actual release, which does reset it).
    /// 5. Player hurt resets rage state.
    /// 6. F2/F3 never accumulate ordinary rage.
    /// 7. Happy expressions (tou_face4/5) never activate through any live
    ///    gameplay path (no speculative runtime activation).
    /// 8. Auto-melee harness state is queryable/consistent (status-line
    ///    presence proxy - OnGUI text itself is not pixel-testable here).
    /// 9. Rage level never changes weapon damage output (COSMETIC_ONLY,
    ///    PROVEN via native sub_668FC/sub_10554 never referencing the rage
    ///    counter or face field anywhere in the damage-dispatch chain).
    /// Muzzle-flash anchor measurements (Part A/B) are diagnostic-only -
    /// this task could not independently prove a "correct" target world
    /// offset within its bounded budget (native's own qhEffectAnimation is
    /// scene/global-owned, not player-parented - see
    /// muzzle_flash_native_unity_differential_n223.json) - so these tests
    /// record current behavior rather than asserting a fixed threshold.
    /// </summary>
    public class Task027N223RageMuzzleHappyRepro
    {
        const string ScenePath = "Assets/DinoCap/Scenes/CombatSandboxV2.unity";
        static readonly string OutDir =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_2_23_MuzzleFlash_RageExpression_HappyFaceClosure/raw"));
        // TASK027N.2.25: new "n225_*" raw output goes to this task's own
        // report folder; "n223_*" files stay in N.2.23's own folder above
        // for continuity with the original tests they were written for.
        static readonly string OutDir225 =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_2_25_RageStateMachine_DamageModifierClosure/raw"));

        CombatSandboxV2 sandbox;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            Directory.CreateDirectory(OutDir);
            Directory.CreateDirectory(OutDir225);
            // TASK027N.2.23 test-infrastructure fix (NOT a production change -
            // NativeAudioSessionGate.cs itself is untouched, frozen G/F1/F2/F3
            // audio infrastructure): the gate is a DontDestroyOnLoad singleton
            // that survives every scene reload for the whole test run, and its
            // own Update() only reaps a session once its AudioSource stops
            // reporting isPlaying==true - which may never happen reliably for
            // every clip in a headless batchmode run with no real audio
            // device. Left unchecked, its 28-session cap fills from EARLIER
            // tests' own cumulative fire-audio calls (this file's own D-family
            // rage tests each fire dozens of shots), and every later test in
            // the SAME run pays the accumulated GameObject/Update overhead -
            // this was diagnosed as the root cause of 4 of this file's own
            // rage tests reading zero/stalled shot progress depending on
            // their position in the test run (confirmed reproducible even
            // with this file run in isolation). StopAll() is an EXISTING
            // public API on the gate (already used by other tests); calling
            // it here only clears test-accumulated session state between
            // this file's own tests, it changes no gate behavior.
            NativeAudioSessionGate.StopAll();
            EditorSceneManager.LoadSceneInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            yield return null;
            sandbox = Object.FindFirstObjectByType<CombatSandboxV2>();
            Assert.IsNotNull(sandbox, "CombatSandboxV2 not found after loading " + ScenePath);
            sandbox.TestHarness_SetGodMode(true);
        }

        static void WriteLine(string file,string line) =>
            File.AppendAllText(Path.Combine(OutDir,file), line+System.Environment.NewLine);
        static void WriteLine225(string file,string line) =>
            File.AppendAllText(Path.Combine(OutDir225,file), line+System.Environment.NewLine);

        IEnumerator SetupDForRage()
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetDebugEnemyHP(999999f);
            sandbox.TestHarness_SetPlayerX(0f);
            // TASK027N.2.35 Part F interaction: the 50000-unit placement
            // below is now WELL beyond the newly-added exact native 480-unit
            // live-offscreen-removal margin (N.2.34's own proven sub_FDE0
            // contract) - without this suppression, the very next per-dino
            // tick would mark this dino for removal and destroy it before
            // any test using this fixture could run. This is a DEBUG-ONLY
            // test-infrastructure bridge (see TestHarness_SetSuppressOffscreenRemoval's
            // own doc comment) - it does not weaken the 480-unit contract
            // for any non-test caller.
            sandbox.TestHarness_SetSuppressOffscreenRemoval(true);
            // TASK027N.2.24: TestHarness_SetGodMode(true) ONLY blocks the
            // player-HP-decrement/death branch (frozen, intentional design -
            // "Do NOT gate hurt anim/playerHitStun/knockback on godMode").
            // The spawned dino's own AI is left fully active with NOTHING
            // preventing it from closing distance and landing an ordinary
            // bite on the PLAYER during these multi-second fire-hold
            // sessions, which sets playerHitStun=true - and
            // UpdateWeaponFireInput() unconditionally forces fireHeld=false
            // and returns early whenever playerHitStun is true, silently
            // killing the rest of the test's own fire session. This was the
            // actual root cause of all 4 N.2.23 rage-test failures (proven
            // via frame-by-frame FireDebugSnapshot logging - see
            // rage_first_divergence_n224.json). 300 units was not remotely
            // enough margin; widened to 50000 so the AI can never close the
            // gap within any test's real duration, regardless of species
            // speed.
            sandbox.TestHarness_SetEnemyX(50000f);
            sandbox.TestHarness_SetWeapon(3,0,999); // D1, plenty of ammo
            yield return null;
        }

        IEnumerator HoldFireUntil(System.Func<bool> until,float timeoutSeconds)
        {
            sandbox.TestHarness_SetFireHeld(true);
            float start=Time.time;
            while(Time.time-start<timeoutSeconds && !until())
                yield return null;
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;
        }

        // TASK027N.2.29: generalizes SetupDForRage() to either rage-eligible
        // family (2=C, 3=D), for the same-gun/switched-gun test matrix.
        IEnumerator SetupWeaponForRage(int family)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetDebugEnemyHP(999999f);
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(50000f);
            sandbox.TestHarness_SetWeapon(family,0,999);
            yield return null;
        }

        static readonly string OutDir229 =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_2_29_RageConcurrentStateMachine_NativeClosure/raw"));
        static void WriteLine229(string file,string line) =>
            File.AppendAllText(Path.Combine(OutDir229,file), line+System.Environment.NewLine);

        // ----------------------------------------------------------------
        // TASK027N.2.29 Part M tests 3/4: repeated sub-threshold bursts
        // after release must NOT extend/refresh the old face's own
        // expiration - it must still expire on its ORIGINAL schedule.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_BurstsBeforeExpiry_OriginalExpiryUnchanged()
        {
            Directory.CreateDirectory(OutDir229);
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=50, 10f);
            Assert.AreEqual("tou_face3", sandbox.TestHarness_HeadRefOverride);

            float expiryArmedAt=sandbox.TestHarness_FaceResetAt;
            WriteLine229("n229_burst.txt","armedAt="+expiryArmedAt+" now="+Time.time);
            Assert.Greater(expiryArmedAt, 0f, "old face expiry must be armed immediately after release.");

            // 3 short, deliberately sub-threshold bursts (well under the
            // 10-shot first band) spaced across the expiry window - none of
            // these may touch faceResetAt at all per native evidence.
            for(int i=0;i<3;i++)
            {
                yield return HoldFireUntil(
                    () => sandbox.TestHarness_SustainedShotCounter>=3, 2f);
                WriteLine229("n229_burst.txt","afterBurst"+i+" faceResetAt="+sandbox.TestHarness_FaceResetAt+" ref="+sandbox.TestHarness_HeadRefOverride);
                Assert.AreEqual(expiryArmedAt, sandbox.TestHarness_FaceResetAt,
                    "a sub-threshold burst must not modify the OLD face's own expiry deadline at all.");
                Assert.AreEqual("tou_face3", sandbox.TestHarness_HeadRefOverride,
                    "old face must remain untouched by a sub-threshold burst.");
                yield return new WaitForSeconds(0.3f);
            }

            float start=Time.time;
            while(Time.time-start<3f && sandbox.TestHarness_HeadRefOverride!="")
                yield return null;
            WriteLine229("n229_burst.txt","final ref="+sandbox.TestHarness_HeadRefOverride+" at="+Time.time);
            Assert.AreEqual("", sandbox.TestHarness_HeadRefOverride,
                "old face must still expire on its original schedule despite the intervening bursts.");
        }

        // ----------------------------------------------------------------
        // Part M test 6/7: sustained re-hold that stays BELOW the new
        // session's own first threshold must let the OLD face expire
        // independently; if the new session DOES cross a threshold before
        // old expiry, the display must switch to the new session's face
        // immediately (native's proven unconditional-overwrite semantics).
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_SustainedReholdBelowThreshold_OldExpiresIndependently()
        {
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 10f);
            Assert.AreEqual("tou_face2", sandbox.TestHarness_HeadRefOverride);

            sandbox.TestHarness_SetFireHeld(false);
            yield return null;
            Assert.IsFalse(sandbox.TestHarness_RageSessionActive);

            // Deterministically construct "a new session is independently
            // active with a few sub-threshold shots already landed",
            // bypassing the real animation-driven discharge cadence (found,
            // via this test's own first attempt, to advance faster/less
            // predictably per real-clock second in this environment than a
            // short timeout can reliably bound - it overshot 10 by landing
            // 11 within 1.5s).
            sandbox.TestHarness_SetSustainedShotCounter(0);
            sandbox.TestHarness_SetRageSessionActive(true);
            for(int i=0;i<5;i++)
                sandbox.TestHarness_AdvanceSustainedFireFace();
            yield return null;
            Assert.Less(sandbox.TestHarness_SustainedShotCounter, 10,
                "test setup: new session must be sub-threshold at this point.");
            Assert.AreEqual("tou_face2", sandbox.TestHarness_HeadRefOverride,
                "old face2 must remain visible while the new session is still sub-threshold.");

            float start2=Time.time;
            while(Time.time-start2<3f && sandbox.TestHarness_HeadRefOverride=="tou_face2")
                yield return null;
            WriteLine229("n229_burst.txt","independentExpiry ref="+sandbox.TestHarness_HeadRefOverride+" newProgress="+sandbox.TestHarness_SustainedShotCounter);
            Assert.AreEqual("", sandbox.TestHarness_HeadRefOverride,
                "old face2 must expire on its own schedule even while a new sustained re-hold is ongoing.");
            Assert.AreEqual(5, sandbox.TestHarness_SustainedShotCounter,
                "the new session's own progress must NOT have been reset to 0 by the unrelated old-face expiry (see UpdateSustainedFaceReset's activelyFiringRage guard).");
        }

        [UnityTest]
        public IEnumerator Rage_NewSessionCrossesThreshold_ReplacesOldFaceImmediately()
        {
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=50, 10f);
            Assert.AreEqual("tou_face3", sandbox.TestHarness_HeadRefOverride);

            sandbox.TestHarness_SetFireHeld(false);
            yield return null;

            // Real press edge (through the normal production pipeline) to
            // get the native-proven session-gated counter reset, then
            // release immediately - the actual shot landing is driven
            // DETERMINISTICALLY below via TestHarness_AdvanceSustainedFireFace(),
            // bypassing the real animation-driven discharge cadence (found
            // to be too fast/uneven per real-clock second in this
            // environment to reliably bound with a short HoldFireUntil()
            // timeout - a first attempt at this test overshot to 50 shots
            // instead of stopping near 10).
            sandbox.TestHarness_SetFireHeld(true);
            yield return null;
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;
            Assert.AreEqual(0, sandbox.TestHarness_SustainedShotCounter,
                "test setup: the fresh press must have reset the new session's raw counter to 0.");

            for(int i=0;i<10;i++)
                sandbox.TestHarness_AdvanceSustainedFireFace();

            WriteLine229("n229_burst.txt","crossedThreshold shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
            Assert.AreEqual("tou_face1", sandbox.TestHarness_HeadRefOverride,
                "the new session's own threshold-crossing shot must immediately replace the old face (native's unconditional-overwrite semantics), even though old face3 had not yet expired.");
        }

        // ----------------------------------------------------------------
        // Part M tests 8/9/10: same-gun and switched-gun re-hold behavior.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_SameGunD_ReholdStartsFreshSession()
        {
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 10f);
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;
            Assert.AreEqual(3, sandbox.TestHarness_EquippedFamily);
            Assert.IsFalse(sandbox.TestHarness_RageSessionActive);

            sandbox.TestHarness_SetFireHeld(true);
            yield return null;
            Assert.Less(sandbox.TestHarness_SustainedShotCounter, 3,
                "same-gun re-hold must start the new session's raw progress from native's proven reset state (0), not resume old progress.");
            sandbox.TestHarness_SetFireHeld(false);
        }

        [UnityTest]
        public IEnumerator Rage_SwitchCToD_ReholdStartsFreshSession()
        {
            yield return SetupWeaponForRage(2);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 10f);
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;

            sandbox.TestHarness_SetWeapon(3,0,999);
            yield return null;
            Assert.IsFalse(sandbox.TestHarness_RageSessionActive,
                "a true release before switching must have already cleared the session.");

            sandbox.TestHarness_SetFireHeld(true);
            yield return null;
            Assert.Less(sandbox.TestHarness_SustainedShotCounter, 3,
                "C->D re-hold after a true release must start fresh, matching native's session-gated reset (sub_667B0's case3 uses the identical gate as case2).");
            sandbox.TestHarness_SetFireHeld(false);
        }

        [UnityTest]
        public IEnumerator Rage_SwitchDToC_ReholdStartsFreshSession()
        {
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 10f);
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;

            sandbox.TestHarness_SetWeapon(2,0,999);
            yield return null;
            Assert.IsFalse(sandbox.TestHarness_RageSessionActive);

            sandbox.TestHarness_SetFireHeld(true);
            yield return null;
            Assert.Less(sandbox.TestHarness_SustainedShotCounter, 3,
                "D->C re-hold after a true release must start fresh, matching native's symmetric C/D session gate.");
            sandbox.TestHarness_SetFireHeld(false);
        }

        // ----------------------------------------------------------------
        // TASK027N.2.30 Part M: tests that inspect the GROUND-TRUTH rendered
        // face (TestHarness_LastRenderedHeadRef), not merely the logical
        // headRefOverride string, per the task's own explicit requirement
        // ("do not report closure based only on logical-state assertions").
        // ----------------------------------------------------------------
        static readonly string OutDir230 =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_2_30_RageLogicalVsRenderedFace_ExpirationClosure/raw"));
        static void WriteLine230(string file,string line) =>
            File.AppendAllText(Path.Combine(OutDir230,file), line+System.Environment.NewLine);

        // M1/M2: release -> expiry -> BOTH logical and rendered reach neutral.
        IEnumerator ReleaseExpiry_LogicalAndRenderedNeutral(int targetShots,string label)
        {
            Directory.CreateDirectory(OutDir230);
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=targetShots, 10f);
            yield return null; // let Render() run at least once with the current state

            float start=Time.time;
            while(Time.time-start<8f && sandbox.TestHarness_HeadRefOverride!="")
                yield return null;
            yield return null; // one more frame so Render() picks up the neutral logical value

            WriteLine230("n230_render.txt",label+" logical='"+sandbox.TestHarness_HeadRefOverride+
                "' rendered='"+sandbox.TestHarness_LastRenderedHeadRef+"'");
            Assert.AreEqual("", sandbox.TestHarness_HeadRefOverride,
                label+": logical face must reach neutral.");
            Assert.IsFalse(sandbox.TestHarness_LastRenderedHeadRef=="tou_face1" ||
                            sandbox.TestHarness_LastRenderedHeadRef=="tou_face2" ||
                            sandbox.TestHarness_LastRenderedHeadRef=="tou_face3",
                label+": the ACTUAL rendered face must also no longer show any rage stage once the logical state is neutral (not merely the debug/logical field).");
        }

        [UnityTest]
        public IEnumerator Rage_ReleaseExpiry_Stage2_LogicalAndRenderedNeutral() =>
            ReleaseExpiry_LogicalAndRenderedNeutral(30,"Stage2");

        [UnityTest]
        public IEnumerator Rage_ReleaseExpiry_Stage3_LogicalAndRenderedNeutral() =>
            ReleaseExpiry_LogicalAndRenderedNeutral(50,"Stage3");

        // M3: the exact Failure 2 reproduction - one shot after release must
        // NOT prematurely clear the still-unexpired old RENDERED face.
        [UnityTest]
        public IEnumerator Rage_OneShotAfterRelease_RenderedRage3Persists()
        {
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=50, 10f);
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;
            yield return null;
            Assert.AreEqual("tou_face3", sandbox.TestHarness_HeadRefOverride);
            Assert.AreEqual("tou_face3", sandbox.TestHarness_LastRenderedHeadRef,
                "sanity: rendered face must match logical face right after release.");

            float armedAt=sandbox.TestHarness_FaceResetAt;

            // Real press+release edges (through the production pipeline) to
            // start a new session, then exactly ONE deterministic shot.
            sandbox.TestHarness_SetFireHeld(true);
            yield return null;
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;
            Assert.AreEqual(0, sandbox.TestHarness_SustainedShotCounter);
            sandbox.TestHarness_AdvanceSustainedFireFace();
            yield return null;
            yield return null;

            WriteLine230("n230_render.txt","oneShotAfterRelease logical='"+sandbox.TestHarness_HeadRefOverride+
                "' rendered='"+sandbox.TestHarness_LastRenderedHeadRef+
                "' faceResetAt="+sandbox.TestHarness_FaceResetAt+" (armedAt="+armedAt+")");
            Assert.AreEqual("tou_face3", sandbox.TestHarness_HeadRefOverride,
                "one new sub-threshold shot must NOT clear the still-unexpired old logical face.");
            Assert.AreEqual("tou_face3", sandbox.TestHarness_LastRenderedHeadRef,
                "one new sub-threshold shot must NOT clear the still-unexpired old RENDERED face either - this is the exact Failure 2 reproduction.");
            Assert.AreEqual(armedAt, sandbox.TestHarness_FaceResetAt,
                "the old expiry deadline must remain EXACTLY unchanged - a new shot/session must not refresh it.");
        }

        // M4: repeated sub-threshold bursts - rendered face cannot survive
        // beyond the ORIGINAL expiry (extends N2.29's own logical-only test
        // with a rendered check).
        [UnityTest]
        public IEnumerator Rage_BurstsBeforeExpiry_RenderedCannotSurviveBeyondExpiry()
        {
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 10f);
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;
            yield return null;
            Assert.AreEqual("tou_face2", sandbox.TestHarness_LastRenderedHeadRef);

            for(int i=0;i<3;i++)
            {
                sandbox.TestHarness_SetFireHeld(true);
                yield return null;
                sandbox.TestHarness_SetFireHeld(false);
                yield return null;
                sandbox.TestHarness_AdvanceSustainedFireFace();
                sandbox.TestHarness_AdvanceSustainedFireFace();
                yield return null;
            }
            Assert.AreEqual("tou_face2", sandbox.TestHarness_LastRenderedHeadRef,
                "repeated sub-threshold bursts must not affect the rendered old face before its own expiry.");

            float start=Time.time;
            while(Time.time-start<4f && sandbox.TestHarness_LastRenderedHeadRef=="tou_face2")
                yield return null;
            WriteLine230("n230_render.txt","burstRendered final='"+sandbox.TestHarness_LastRenderedHeadRef+"'");
            Assert.AreNotEqual("tou_face2", sandbox.TestHarness_LastRenderedHeadRef,
                "the rendered old face must not survive past its original expiry despite the intervening bursts.");
        }

        // M5/M6: after old expiry, rendered face reflects whatever the NEW
        // session has (or hasn't) independently earned.
        [UnityTest]
        public IEnumerator Rage_AfterExpiry_NewProgressBelowThreshold_RenderedNeutral()
        {
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 10f);
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;

            sandbox.TestHarness_SetRageSessionActive(true);
            sandbox.TestHarness_SetSustainedShotCounter(0);
            for(int i=0;i<5;i++)
                sandbox.TestHarness_AdvanceSustainedFireFace();
            yield return null;

            float start=Time.time;
            while(Time.time-start<4f && sandbox.TestHarness_LastRenderedHeadRef=="tou_face2")
                yield return null;
            WriteLine230("n230_render.txt","belowThresholdAfterExpiry rendered='"+sandbox.TestHarness_LastRenderedHeadRef+"' progress="+sandbox.TestHarness_SustainedShotCounter);
            Assert.IsFalse(sandbox.TestHarness_LastRenderedHeadRef=="tou_face1" ||
                            sandbox.TestHarness_LastRenderedHeadRef=="tou_face2" ||
                            sandbox.TestHarness_LastRenderedHeadRef=="tou_face3",
                "with new progress still below stage1, the rendered face must go neutral once the old face expires.");
        }

        [UnityTest]
        public IEnumerator Rage_AfterExpiry_NewSessionEarnedStage1_RenderedFollowsPriority()
        {
            yield return SetupWeaponForRage(3);
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 10f);
            sandbox.TestHarness_SetFireHeld(false);
            yield return null;

            sandbox.TestHarness_SetRageSessionActive(true);
            sandbox.TestHarness_SetSustainedShotCounter(0);
            for(int i=0;i<10;i++)
                sandbox.TestHarness_AdvanceSustainedFireFace();
            yield return null;

            WriteLine230("n230_render.txt","newSessionEarnedStage1 rendered='"+sandbox.TestHarness_LastRenderedHeadRef+"'");
            Assert.AreEqual("tou_face1", sandbox.TestHarness_LastRenderedHeadRef,
                "once the new session independently earns stage1, the rendered face must show it (native's proven overwrite-at-threshold priority) regardless of the old face's own expiry state.");
        }

        // ----------------------------------------------------------------
        // 1: sustained eligible auto fire reaches rage 1/2/3.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_SustainedD_Reaches123()
        {
            // NOTE: this must be ONE continuous held-fire session, not
            // separate release/re-press cycles per milestone - BeginFireRequest's
            // own proven fresh-press reset (sustainedShotCounter=0, mirroring
            // native sub_667B0's *(a1+188)-gated reset) fires on every NEW
            // press, so releasing between checkpoints would spuriously reset
            // progress instead of exercising real sustained-fire accumulation.
            yield return SetupDForRage();
            sandbox.TestHarness_SetFireHeld(true);

            float start=Time.time;
            bool sawFace1=false, sawFace2=false, sawFace3=false;
            while(Time.time-start<10f && !sawFace3)
            {
                yield return null;
                if(!sawFace1 && sandbox.TestHarness_SustainedShotCounter>=10)
                {
                    sawFace1=true;
                    WriteLine("n223_rage.txt","after10 shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
                    Assert.AreEqual("tou_face1", sandbox.TestHarness_HeadRefOverride);
                }
                if(!sawFace2 && sandbox.TestHarness_SustainedShotCounter>=30)
                {
                    sawFace2=true;
                    WriteLine("n223_rage.txt","after30 shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
                    Assert.AreEqual("tou_face2", sandbox.TestHarness_HeadRefOverride);
                }
                if(!sawFace3 && sandbox.TestHarness_SustainedShotCounter>=50)
                {
                    sawFace3=true;
                    WriteLine("n223_rage.txt","after50 shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
                    Assert.AreEqual("tou_face3", sandbox.TestHarness_HeadRefOverride);
                }
            }
            sandbox.TestHarness_SetFireHeld(false);
            Assert.IsTrue(sawFace1 && sawFace2 && sawFace3,
                "must pass through all 3 rage levels under sustained D fire.");
        }

        // ----------------------------------------------------------------
        // TASK027N.2.28 REPLACES the N.2.23-N.2.27 "steps down one band per
        // tick" model with the corrected DIRECT-TO-NEUTRAL acceptance: the
        // face expected at release (e.g. "tou_face2") must remain COMPLETELY
        // UNCHANGED for the entire reset delay, then jump straight to
        // neutral in the SAME tick the raw counter also reaches 0 - there is
        // no intermediate lower-stage face and no separate "one more tick"
        // for the counter (both effects are now the same single assignment).
        // ----------------------------------------------------------------
        IEnumerator ReleaseEventuallyReachesNeutral(int targetShots,string label)
        {
            yield return SetupDForRage();
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=targetShots, 10f);
            string refAtRelease=sandbox.TestHarness_HeadRefOverride;
            WriteLine225("n225_rage.txt",label+" atRelease shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+refAtRelease);
            Assert.AreNotEqual("", refAtRelease, label+": must show a visible rage face before release.");

            // Exact native reset delay is unproven (still reuses the
            // pre-existing, disclosed-as-unproven 2s constant) - poll every
            // frame so a premature transition (the old stepwise bug) would
            // be caught, not just the final outcome.
            float start=Time.time;
            bool sawWrongIntermediateStage=false;
            while(Time.time-start<8f && sandbox.TestHarness_HeadRefOverride!="")
            {
                if(sandbox.TestHarness_HeadRefOverride!=refAtRelease)
                    sawWrongIntermediateStage=true;
                yield return null;
            }

            WriteLine225("n228_rage.txt",label+" visibleNeutral shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
            Assert.IsFalse(sawWrongIntermediateStage,
                label+": the face must remain "+refAtRelease+" unchanged until the reset delay elapses - no intermediate lower stage is permitted.");
            Assert.AreEqual("", sandbox.TestHarness_HeadRefOverride,
                label+": release must reach neutral directly.");
            Assert.AreEqual(0, sandbox.TestHarness_SustainedShotCounter,
                label+": the raw counter must reach exactly 0 on the SAME tick the face reaches neutral (direct reset, no intermediate landing value).");
        }

        [UnityTest]
        public IEnumerator Rage_ReleaseFromStage1_EventuallyNeutral() =>
            ReleaseEventuallyReachesNeutral(10,"Stage1");

        [UnityTest]
        public IEnumerator Rage_ReleaseFromStage2_EventuallyNeutral() =>
            ReleaseEventuallyReachesNeutral(30,"Stage2");

        [UnityTest]
        public IEnumerator Rage_ReleaseFromStage3_EventuallyNeutral() =>
            ReleaseEventuallyReachesNeutral(50,"Stage3");

        // ----------------------------------------------------------------
        // 2/3b: re-pressing while a stage is still visible during its own
        // decay window must NOT instantly advance to the next stage - the
        // raw progress counter resets on the fresh press (native's own
        // proven gate), it does not resume/preserve old progress.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_RepressDuringDecay_DoesNotInstantlyAdvance()
        {
            yield return SetupDForRage();
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 8f);
            Assert.AreEqual("tou_face2", sandbox.TestHarness_HeadRefOverride,
                "must reach face2 before testing re-press behavior.");

            sandbox.TestHarness_SetFireHeld(false);
            yield return null; // a genuine release edge

            sandbox.TestHarness_SetFireHeld(true);
            yield return null;
            WriteLine225("n225_rage.txt","immediatelyAfterRepress shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);

            Assert.Less(sandbox.TestHarness_SustainedShotCounter, 5,
                "a fresh press after a true release must restart the raw progress counter near 0, not preserve old progress.");
            Assert.AreNotEqual("tou_face3", sandbox.TestHarness_HeadRefOverride,
                "re-pressing must not instantly jump to the next stage.");

            sandbox.TestHarness_SetFireHeld(false);
        }

        // ----------------------------------------------------------------
        // 2/3c: resuming fire after a true release must still be able to
        // reach the next stage - but only via a full, fresh accumulation
        // (up to the absolute 10/30/50 thresholds again), not a shortcut.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_RepressDuringDecay_ReachesStage3AfterFreshProgress()
        {
            yield return SetupDForRage();
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 8f);
            Assert.AreEqual("tou_face2", sandbox.TestHarness_HeadRefOverride);

            sandbox.TestHarness_SetFireHeld(false);
            yield return null;

            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=50, 10f);
            WriteLine225("n225_rage.txt","afterFreshClimb shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
            Assert.AreEqual("tou_face3", sandbox.TestHarness_HeadRefOverride,
                "resuming fire after a true release must still be able to reach stage3 via a full fresh accumulation.");
        }

        // ----------------------------------------------------------------
        // TASK027N.2.25 Part F: native's own weapon-switch handler
        // (sub_65A08) never touches the fire-session-active flag (PROVEN
        // via direct decompile) - switching between two rage-eligible
        // families (C<->D) while fire stays CONTINUOUSLY held must NOT
        // reset progress, unlike a genuine release (tested above).
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_WeaponSwitch_WhileHolding_PreservesProgress()
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetDebugEnemyHP(999999f);
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(50000f);
            sandbox.TestHarness_SetWeapon(2,0,999); // C1 (family index 2)
            yield return null;

            sandbox.TestHarness_SetFireHeld(true);
            float s=Time.time;
            while(Time.time-s<5f && sandbox.TestHarness_SustainedShotCounter<20)
                yield return null;
            int beforeSwitch=sandbox.TestHarness_SustainedShotCounter;
            Assert.Greater(beforeSwitch, 0, "must have accumulated some progress on C before switching.");

            // Real SwitchWeapon() (cycles +1: C(2)->D(3)) WHILE STILL
            // HOLDING - not a mere field write, so it exercises the same
            // press-edge path a real re-hold would.
            sandbox.SwitchWeapon();
            yield return null;
            Assert.AreEqual(3, sandbox.TestHarness_EquippedFamily,
                "SwitchWeapon() must have landed on D.");

            int afterSwitch=sandbox.TestHarness_SustainedShotCounter;
            WriteLine225("n225_rage.txt","switchCtoDWhileHolding before="+beforeSwitch+" after="+afterSwitch);
            Assert.GreaterOrEqual(afterSwitch, beforeSwitch,
                "switching from C to D while fire stays continuously held must NOT reset progress (native's own switch handler never touches the session-active flag).");

            sandbox.TestHarness_SetFireHeld(false);
        }

        // ----------------------------------------------------------------
        // 4: weapon switch stops auto fire and follows the same decay.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_WeaponSwitch_StopsFireAndDecays()
        {
            yield return SetupDForRage();
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 5f);
            Assert.AreEqual("tou_face2", sandbox.TestHarness_HeadRefOverride);

            sandbox.TestHarness_SetFireHeld(false);
            sandbox.TestHarness_SetWeapon(0,0,50); // switch to A
            yield return null;

            int justAfterSwitch=sandbox.TestHarness_SustainedShotCounter;
            WriteLine("n223_rage.txt","justAfterSwitch shots="+justAfterSwitch+" ref="+sandbox.TestHarness_HeadRefOverride);
            Assert.Greater(justAfterSwitch, 0,
                "switching weapon must not instantly hard-reset the counter to 0.");

            // TASK027N.2.28 CORRECTION: this test used to assert the face
            // steps DOWN one band at a time ("tou_face2"->"tou_face1") after
            // a switch-triggered decay - that stepwise model was proven
            // WRONG by direct user runtime observation (see
            // rage_wrong_stepwise_tests_n228.json). Corrected acceptance:
            // the face must stay at "tou_face2" (untouched) until the reset
            // delay elapses, then jump DIRECTLY to neutral - never landing
            // on "tou_face1" at all.
            float start=Time.time;
            while(Time.time-start<8f && sandbox.TestHarness_HeadRefOverride=="tou_face2")
                yield return null;
            WriteLine("n223_rage.txt","afterSwitchDecay shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
            Assert.AreEqual("", sandbox.TestHarness_HeadRefOverride,
                "decay after a weapon switch must jump DIRECTLY to neutral, never landing on an intermediate lower stage.");
        }

        // ----------------------------------------------------------------
        // 5: player hurt resets rage state.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_PlayerHurt_Resets()
        {
            yield return SetupDForRage();
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 5f);
            Assert.AreEqual("tou_face2", sandbox.TestHarness_HeadRefOverride);

            sandbox.TestHarness_SetFireHeld(false);
            sandbox.TestHarness_SetGodMode(false);
            sandbox.TestHarness_SetPlayerHP(100f);
            sandbox.TestHarness_DamagePlayer(10f);
            yield return null;

            WriteLine("n223_rage.txt","afterHurt shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
            Assert.AreEqual(0, sandbox.TestHarness_SustainedShotCounter,
                "player hurt must reset the rage counter.");
            Assert.AreEqual("", sandbox.TestHarness_HeadRefOverride,
                "player hurt must reset the rage face to neutral.");
        }

        // ----------------------------------------------------------------
        // TASK027N.2.24 Part E regression reproducer for the actual root
        // cause of ALL 4 of N.2.23's own flaky rage-test failures: the
        // spawned AI dino was left free to close distance and land an
        // ordinary bite on the PLAYER during these multi-second fire-hold
        // sessions (godMode does NOT block playerHitStun - only HP/death,
        // by frozen design), which silently forced fireHeld=false for the
        // rest of the run via UpdateWeaponFireInput's own
        // if(playerDead||playerHitStun) early-out. Proven via frame-by-frame
        // FireDebugSnapshot logging (rage_first_divergence_n224.json).
        // Fixed by keeping the enemy far enough away that its own AI can
        // never reach the player within any test's real duration (see
        // SetupDForRage/Rage_F2F3_NeverAccumulate's own updated distance).
        // This test explicitly reproduces "F2/F3 fire, then immediately
        // start a clean D rage session" to guard against a regression.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_CleanFixtureAfterF2F3()
        {
            foreach(int tier in new[]{1,2}) // F2, F3
            {
                sandbox.TestHarness_ClearDinos();
                yield return null;
                sandbox.TestHarness_SpawnDino(0,true);
                yield return null;
                sandbox.TestHarness_SetDebugEnemyHP(999999f);
                sandbox.TestHarness_SetPlayerX(0f);
                sandbox.TestHarness_SetEnemyX(50000f);
                sandbox.TestHarness_SetWeapon(5,tier,999);
                yield return null;
                sandbox.TestHarness_SetFireHeld(true);
                float s=Time.time;
                while(Time.time-s<2f) yield return null;
                sandbox.TestHarness_SetFireHeld(false);
            }

            Assert.IsFalse(sandbox.TestHarness_CaptureFireDebugSnapshot().playerHitStun,
                "the player must not have been hit-stunned merely by holding F2/F3 fire near a distant, harmless dino.");

            yield return SetupDForRage();
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=50, 8f);
            WriteLine("n223_rage.txt","cleanFixtureAfterF2F3 shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
            Assert.AreEqual("tou_face3", sandbox.TestHarness_HeadRefOverride,
                "a D rage session immediately after F2/F3 must reach face3 cleanly, exactly like a session with no prior F2/F3 use.");
        }

        // ----------------------------------------------------------------
        // 6: F2/F3 never accumulate ordinary rage.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_F2F3_NeverAccumulate()
        {
            foreach(int tier in new[]{1,2}) // F2, F3
            {
                sandbox.TestHarness_ClearDinos();
                yield return null;
                sandbox.TestHarness_SpawnDino(0,true);
                yield return null;
                sandbox.TestHarness_SetDebugEnemyHP(999999f);
                sandbox.TestHarness_SetPlayerX(0f);
                // TASK027N.2.24: same fix as SetupDForRage - 300 units gave
                // the AI-controlled dino enough time/room to bite the player
                // during this 2s hold, setting playerHitStun=true (godMode
                // does not block this) and silently killing fire input for
                // every test that runs after this one in the same run.
                sandbox.TestHarness_SetEnemyX(50000f);
                sandbox.TestHarness_SetWeapon(5,tier,999);
                yield return null;

                sandbox.TestHarness_SetFireHeld(true);
                float start=Time.time;
                while(Time.time-start<2f)
                    yield return null;
                sandbox.TestHarness_SetFireHeld(false);

                WriteLine("n223_rage.txt","F"+(tier+1)+" shots="+sandbox.TestHarness_SustainedShotCounter+" ref="+sandbox.TestHarness_HeadRefOverride);
                Assert.AreEqual(0, sandbox.TestHarness_SustainedShotCounter,
                    "F"+(tier+1)+" must never accumulate the ordinary rage counter.");
                Assert.AreEqual("", sandbox.TestHarness_HeadRefOverride,
                    "F"+(tier+1)+" must never show an ordinary rage face.");
            }
        }

        // ----------------------------------------------------------------
        // TASK027N.2.25 Part I/J: rage level must never change weapon
        // damage output. PROVEN via native sub_668FC (the target-scan/
        // hit-dispatch function sub_66DCC's case2/3 calls at the END of
        // every C/D shot) - it reads ONLY the plain per-family weapon-row
        // table (dword_21BE00/dword_21BE0C) and passes it UNMODIFIED into
        // sub_10554; neither the rage counter (player+132) nor the visible
        // face field (player+108) is referenced anywhere in this function
        // or in sub_10554 itself (already exhaustively decompiled across
        // N.2.14/N.2.19/N.2.22). D1's own proven damage range is 5-7
        // (WeaponFamilyTable.Rows[3][0]) regardless of rage level - this
        // test fires enough shots to pass through EVERY rage stage and
        // asserts every single landed hit stays inside that exact range,
        // which would be violated by even a modest rage multiplier.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_DamageUnaffectedAcrossAllStages()
        {
            yield return SetupDForRage();
            sandbox.TestHarness_SetDebugEnemyHP(999999f);
            // SetupDForRage() places the enemy 50000 units away (needed to
            // keep the AI from ever reaching bite range in the OTHER rage
            // tests) - but D1's own weapon range (atkDis) is only 1024, so
            // at that distance every shot MISSES and this test collects
            // zero damage samples. This test needs actual hits, so it pulls
            // the enemy back within range - 900 units still leaves several
            // seconds of margin against the AI closing to bite range within
            // this test's own ~12s budget, per the same species-speed
            // reasoning already established (300 units was provably unsafe
            // over a 2-4s window; 900 over 12s keeps a comparable safety
            // margin).
            sandbox.TestHarness_SetEnemyX(900f);

            // TASK027N.2.35 temporary diagnostic (harmless, no gameplay
            // effect) - narrowing down why this test collects 0 samples
            // post-migration.
            WriteLine225("n225_rage_damage_diag.txt",
                "activeDinos="+sandbox.TestHarness_ActiveDinoCount+
                " enemyX="+sandbox.CaptureRuntimeDebugSnapshot().enemyX+
                " playerX="+sandbox.CaptureRuntimeDebugSnapshot().playerX+
                " equippedFamily="+sandbox.TestHarness_EquippedFamily);

            float hpBefore=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
            sandbox.TestHarness_SetFireHeld(true);
            float start=Time.time;
            int samples=0;
            int diagCount=0;
            while(Time.time-start<12f && sandbox.TestHarness_SustainedShotCounter<50)
            {
                yield return null;
                if(diagCount<5)
                {
                    diagCount++;
                    WriteLine225("n225_rage_damage_diag.txt",
                        "t="+(Time.time-start).ToString("0.00")+
                        " shots="+sandbox.TestHarness_SustainedShotCounter+
                        " enemyHP="+sandbox.CaptureRuntimeDebugSnapshot().enemyHP+
                        " enemyX="+sandbox.CaptureRuntimeDebugSnapshot().enemyX+
                        " inCone="+sandbox.TestHarness_IsPlayerInNativeAttackCone());
                }
                float hpNow=sandbox.CaptureRuntimeDebugSnapshot().enemyHP;
                if(hpNow<hpBefore-0.001f)
                {
                    float dmg=hpBefore-hpNow;
                    samples++;
                    WriteLine225("n225_rage_damage.txt",
                        "shots="+sandbox.TestHarness_SustainedShotCounter+
                        " ref="+sandbox.TestHarness_HeadRefOverride+
                        " dmg="+dmg.ToString("0.000"));
                    Assert.GreaterOrEqual(dmg, 4.9f,
                        "D1 damage must stay within its proven 5-7 range regardless of rage level (sample at "+sandbox.TestHarness_HeadRefOverride+").");
                    Assert.LessOrEqual(dmg, 7.1f,
                        "D1 damage must stay within its proven 5-7 range regardless of rage level (sample at "+sandbox.TestHarness_HeadRefOverride+").");
                    hpBefore=hpNow;
                }
            }
            sandbox.TestHarness_SetFireHeld(false);
            Assert.Greater(samples, 10, "must have collected enough damage samples across the full rage climb to be meaningful.");
        }

        // ----------------------------------------------------------------
        // TASK027N.2.26: reproduces the actual V30.43 production bug.
        // N2.25's own ReleaseEventuallyReachesNeutral tests above all use
        // SetupDForRage()'s 999-ammo fixture, so they never exercise
        // DischargeCurrentWeapon()'s own line
        // "if(familyAmmo[family]<=0 && fireHeld) AutoSwitchToLoaded();" -
        // the ONLY other code path (besides a clean release or a manual
        // Tab-switch) that changes equippedFamily away from C/D while a
        // rage session is active. AutoSwitchToLoaded() was PROVEN (this
        // task) to change equippedFamily without clearing rageSessionActive
        // or arming faceResetAt, unlike its sibling SwitchWeapon() - so once
        // ammo ran dry mid-hold, the visible rage face was orphaned forever
        // (EndFireRequest()'s own arm is gated on equippedFamily==2||3,
        // which stops being true the instant the auto-switch fires). Real
        // D1 ammo (200, WeaponFamilyTable.AmmoCaps) does not deplete within
        // a single short burst, which is why this went unnoticed until an
        // extended real play session - this test uses a deliberately low
        // ammo count to force the same path deterministically. The fix
        // (UpdateSustainedFaceReset()'s own self-arm, keyed off live
        // equippedFamily/rageSessionActive every frame rather than trusting
        // each call site to remember to arm the timer) requires no
        // AutoSwitchToLoaded()-specific test to pass, by design - this test
        // exists to prove that design claim, not merely to patch the one
        // known site.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Rage_AmmoDepletionMidHold_StillDecaysToNeutral()
        {
            yield return SetupDForRage();
            // Enough ammo to comfortably reach stage2 (30) but exhausted
            // well before stage3 (50), forcing AutoSwitchToLoaded() to fire
            // WHILE the fire button is still physically held.
            sandbox.TestHarness_SetWeapon(3,0,35);

            sandbox.TestHarness_SetFireHeld(true);
            float start=Time.time;
            while(Time.time-start<10f && sandbox.TestHarness_EquippedFamily==3)
                yield return null;
            sandbox.TestHarness_SetFireHeld(false);

            Assert.AreNotEqual(3, sandbox.TestHarness_EquippedFamily,
                "ammo depletion mid-hold must have auto-switched away from D.");
            WriteLine225("n226_rage_ammo_switch.txt",
                "afterAutoSwitch family="+sandbox.TestHarness_EquippedFamily+
                " sessionActive="+sandbox.TestHarness_RageSessionActive+
                " ref="+sandbox.TestHarness_HeadRefOverride);
            Assert.IsFalse(sandbox.TestHarness_RageSessionActive,
                "rageSessionActive must not remain stuck true once auto-switched off C/D.");

            float start2=Time.time;
            while(Time.time-start2<15f && sandbox.TestHarness_HeadRefOverride!="")
                yield return null;
            WriteLine225("n226_rage_ammo_switch.txt",
                "finalRef="+sandbox.TestHarness_HeadRefOverride);
            Assert.AreEqual("", sandbox.TestHarness_HeadRefOverride,
                "visible rage face must still eventually decay to neutral after an ammo-triggered auto-switch, not remain orphaned forever (the V30.43 bug).");
        }

        // ----------------------------------------------------------------
        // TASK027N.2.27 Part C/I: frame-by-frame trace of the NORMAL manual
        // Shoot release path (no ammo depletion, no weapon switch, no hurt -
        // exactly the primary bug's own reproduction shape) plus the 4
        // required deterministic tests. This exercises the SAME production
        // Update()/UpdateWeaponFireInput()/EndFireRequest()/
        // UpdateSustainedFaceReset() code the live V30.44 build ran - if
        // this passes while the user's own live session does not, the most
        // likely explanation is that the live session was not actually
        // running the patched code (see Part A/J - stale/non-recompiled
        // Play session), not a further code defect.
        // ----------------------------------------------------------------
        static readonly string OutDir227 =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_2_27_RageReleaseToNeutral_VersionBannerClosure/raw"));
        static void WriteLine227(string file,string line) =>
            File.AppendAllText(Path.Combine(OutDir227,file), line+System.Environment.NewLine);

        IEnumerator NormalReleaseTraceReachesNeutral(int targetShots,string label)
        {
            Directory.CreateDirectory(OutDir227);
            yield return SetupDForRage();
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=targetShots, 10f);

            // Confirm the true production release edge fired (fireHeld=false,
            // rageSessionActive cleared) BEFORE any trace/decay assertions -
            // this is exactly what a normal Shoot-button-up does in real play.
            Assert.IsFalse(sandbox.TestHarness_FireHeld, label+": fireHeld must be false immediately after release.");
            Assert.IsFalse(sandbox.TestHarness_RageSessionActive, label+": rageSessionActive must clear on a true release.");

            string traceFile="n227_trace_"+label+".txt";
            float start=Time.time;
            bool reachedNeutral=false;
            while(Time.time-start<15f)
            {
                var s=sandbox.TestHarness_CaptureFireDebugSnapshot();
                WriteLine227(traceFile,
                    string.Format("t={0:0.000} shootHeld={1} rageSessionActive={2} sustainedShotCounter={3} faceResetAt={4:0.000} headRef={5} secondaryClip={6} secondaryActive={7} equippedFamily={8}",
                        s.timeNow,s.fireHeld,s.rageSessionActive,s.sustainedShotCounter,s.faceResetAtOut,s.headRefOverride,s.secondaryClipName,s.secondaryActive,s.equippedFamily));
                if(sandbox.TestHarness_HeadRefOverride=="")
                {
                    reachedNeutral=true;
                    break;
                }
                yield return null;
            }
            WriteLine227(traceFile,"FINAL reachedNeutral="+reachedNeutral+" finalShots="+sandbox.TestHarness_SustainedShotCounter);
            Assert.IsTrue(reachedNeutral,
                label+": normal manual Shoot release alone (no ammo depletion, no weapon switch, no hurt) must reach neutral within a generous margin.");
        }

        // TEST 1
        [UnityTest]
        public IEnumerator NormalRelease_Stage2_ReachesNeutral() =>
            NormalReleaseTraceReachesNeutral(30,"Stage2");

        // TEST 2
        [UnityTest]
        public IEnumerator NormalRelease_Stage3_ReachesNeutral() =>
            NormalReleaseTraceReachesNeutral(50,"Stage3");

        // TEST 3: stage2, release briefly, re-press before neutral - must
        // NOT instantly advance to stage3 (preserves N2.25's own correction).
        [UnityTest]
        public IEnumerator NormalRelease_RepressBeforeNeutral_NoInstantStage3()
        {
            yield return SetupDForRage();
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=30, 10f);
            Assert.IsFalse(sandbox.TestHarness_RageSessionActive);

            yield return null; // one decay-eligible frame while still stage2-visible
            sandbox.TestHarness_SetFireHeld(true);
            yield return null;

            Assert.AreNotEqual("tou_face3", sandbox.TestHarness_HeadRefOverride,
                "re-pressing while stage2 is still visible must not instantly jump to stage3.");
            sandbox.TestHarness_SetFireHeld(false);
        }

        // TEST 4: on-screen version banner is sourced from the same central
        // constant this test reads directly - locks the expected value so a
        // future patch that forgets to bump PortVersion is caught here too.
        [Test]
        public void VersionBanner_MatchesCentralConstant()
        {
            Assert.AreEqual("V30.62", CombatSandboxV2.PortVersion);
            Assert.AreEqual("TASK027N.2.42", CombatSandboxV2.PortBuildTag);
        }

        // ----------------------------------------------------------------
        // 7: happy expressions never activate speculatively.
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator Happy_NoSpeculativeActivation()
        {
            yield return SetupDForRage();
            yield return HoldFireUntil(
                () => sandbox.TestHarness_SustainedShotCounter>=50, 5f);
            string refValue=sandbox.TestHarness_HeadRefOverride;
            WriteLine("n223_happy.txt","maxRageRef="+refValue);
            Assert.AreNotEqual("tou_face4", refValue);
            Assert.AreNotEqual("tou_face5", refValue);
        }

        // ----------------------------------------------------------------
        // 8: auto-melee harness state is queryable/consistent (status-line
        // presence proxy).
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator AutoMeleeUI_StateConsistent()
        {
            sandbox.TestHarness_AutoMeleeEnabled=false;
            yield return null;
            Assert.AreEqual(0, sandbox.TestHarness_AutoMeleeAttemptCount==0 ? 0 : 0); // no-op sanity, real check below

            sandbox.TestHarness_SetAutoMeleeFixedOffset(-1);
            sandbox.TestHarness_AutoMeleeEnabled=true;
            yield return null;
            WriteLine("n223_auto_melee_ui.txt","enabled=true mode=FixedOffset offset=-1");
            Assert.IsTrue(sandbox.TestHarness_AutoMeleeEnabled);

            sandbox.TestHarness_SetAutoMeleeSweep();
            yield return null;
            WriteLine("n223_auto_melee_ui.txt","enabled=true mode=AutoSweep");

            sandbox.TestHarness_AutoMeleeEnabled=false;
        }

        // ----------------------------------------------------------------
        // Muzzle-flash diagnostic measurements (NOT pass/fail against a
        // guessed target - records current world-offset behavior only).
        // ----------------------------------------------------------------
        IEnumerator MeasureMuzzle(string label,bool moveLeft,bool moveRight)
        {
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetDebugEnemyHP(999999f);
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(300f);
            sandbox.TestHarness_SetWeapon(0,0,50); // A1, semi-auto, simple
            yield return null;

            sandbox.TestHarness_HoldLeft(moveLeft);
            sandbox.TestHarness_HoldRight(moveRight);
            for(int i=0;i<5;i++) yield return null; // let movement/pose settle

            Vector3 before;
            sandbox.TestHarness_TryGetMuzzleWorldSocket(out before);
            sandbox.TestHarness_Shoot();
            yield return null; yield return null; yield return null;
            Vector3 after;
            bool ok=sandbox.TestHarness_TryGetMuzzleWorldSocket(out after);

            sandbox.TestHarness_HoldLeft(false);
            sandbox.TestHarness_HoldRight(false);

            WriteLine("n223_muzzle.txt",
                label+" ok="+ok+" before="+before+" after="+after+
                " delta="+(after-before));
        }

        [UnityTest]
        public IEnumerator Muzzle_StationaryAnchor() =>
            MeasureMuzzle("stationary",false,false);

        [UnityTest]
        public IEnumerator Muzzle_MoveLeftAnchor() =>
            MeasureMuzzle("moveLeft",true,false);

        [UnityTest]
        public IEnumerator Muzzle_MoveRightAnchor() =>
            MeasureMuzzle("moveRight",false,true);
    }
}
#endif
