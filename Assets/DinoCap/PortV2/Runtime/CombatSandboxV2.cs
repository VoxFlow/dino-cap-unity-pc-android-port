using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// TASK027N.1.3.1: lets the EDITOR-ONLY PlayMode test assembly
// (Assets/DinoCap/Tests/, asmdef "DinoCap.PortV2.Tests") see this
// assembly's `internal` TestHarness_*/RuntimeDebugSnapshot members
// without widening anything to `public`. Grants no access in a player
// build (the test assembly itself is Editor-only and never ships).
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DinoCap.PortV2.Tests")]

namespace DinoCap.PortV2
{
    // V11 Task024D: coherent iPad source-space combat sandbox.
    //
    // Native attack entry stops the Box2D body, but the actor retains its
    // facing.  V5 incorrectly derived facing from enemyVelocityX after setting
    // that velocity to 0, so frame-12 impact always evaluated as "facing left".
    public sealed partial class CombatSandboxV2 : MonoBehaviour, IF1ProjectileOwner
    {
        // TASK027N.2.27 Part B: single authoritative version source. Every
        // future production patch updates ONLY this constant - the on-screen
        // banner (OnGUI), the console startup log (Start()), and this
        // report's own final-console block all read from it, so a stale/
        // non-recompiled Play session becomes immediately provable-or-
        // disprovable by simply reading the screen (see
        // runtime_version_banner_manifest_n227.json /
        // live_patch_persistence_n227.json - this task exists specifically
        // because prior fixes were reported as "not working" in live play
        // despite passing automated tests, raising the possibility that the
        // interactive Editor session under test was never actually running
        // the patched code, e.g. an already-running Play session that
        // predates a script recompile).
        // TASK027N.2.37 Part A: N.2.36.7/.8/.9 were all analysis-only
        // (IDA/static-closure tasks with explicit "no Unity code change, no
        // version bump" verdicts in their own reports) - the live banner
        // correctly stayed at N.2.36.6, the last task that actually patched
        // production code, through all three. This task DOES patch
        // production code (world-plane/facing/spawn/controls/audio/door/
        // popup/streaming fixes - see report.md), so the banner now
        // advances for the first time since N.2.36.6.
        // TASK027N.2.38 Part A: substantial native-owner reconstruction +
        // production patches (viewport/camera Y anchor, player world-bound,
        // bidirectional spawn staging, draw order, building/door family
        // ID, HUD, player HP/death, full keybind system - see report.md).
        public const string PortVersion = "V30.62";
        public const string PortBuildTag = "TASK027N.2.42";

        public static CombatSandboxV2 Instance { get; private set; }

        // TASK027N.2.35 Part A/B: MULTI-DINO CORE MIGRATION.
        // Every field that used to be a single "enemy*"/F2-reaction/F3-
        // reaction singleton is now owned by exactly one DinoInstance in the
        // `dinos` collection below. `current` is a per-call "which dino am I
        // operating on right now" pointer, swapped by the per-instance
        // Update loop (TickEnemy), by the per-instance animation-EnterFrame
        // closures wired at spawn time, and by each combat dispatch's own
        // target-selection helper (closest-eligible for ABCD/F2/F3, a full
        // enumeration for melee/E). Every existing method that reads/writes
        // "enemyHP"/"enemy"/"enemyVisual"/etc. is UNCHANGED - those names
        // are now properties that transparently delegate to whichever
        // DinoInstance `current` points at, so the entire hard-won native-
        // faithful state-machine/timing logic in TickEnemy, ApplyHeavyWeaponHit,
        // BeginStandardEnemyDeath, BeginGSpecialFatalDeath, BeginEWeaponDeath,
        // OnEnemyPrimaryFrame, the F2 burn state machine and the F3 laser/
        // X-ray state machine is preserved byte-for-byte; only WHERE that
        // state lives changed (per-instance instead of one shared singleton).
        // See Dev/Reports/Task027N_2_35_.../unity_singleton_inventory_n235.json
        // for the full audit this design is based on.
        internal sealed class DinoInstance
        {
            public int id;
            public DebugDinoKind kind;
            public SpawnSide side;
            public bool markedForRemoval;
            // TASK027N.2.35 Part P/Q: true only for a dino spawned by the
            // City Grind timer-spawner test mode - gates native HP/ATK level
            // scaling (Part Q) so manual/debug sandbox spawns keep their
            // existing fixed sandbox stats untouched (Part R policy split).
            public bool nativeCityGrindSpawn;
            public int cityGrindLevelAtSpawn;

            public Transform transform;
            public Power2DSourceTruthRenderer visual;

            public float hp;
            public float velocityX;
            public float runtimeAdis;
            public int adisJitter;
            public bool facingRight;
            public bool attacking;
            public bool hitThisCycle;
            public bool heavyReaction;

            public string clipPrefix = "k3";
            public float sourceSpeed;
            public float sourceAttack;
            public float sourceReward;

            // F2 (huoyan/flame) reaction-consequence state. The flame VISUAL
            // itself (f2FlameVisual) stays PLAYER-anchored (it is the
            // weapon's own jet, not per-target) - only the TARGET's own
            // burn/residual/death reaction state and its engulfing Dahuo
            // visual are per-instance.
            public bool burningF2;
            public bool f2Residual;
            public bool f2BurnDeath;
            public F2EnemyBigFireVisual f2BigFireVisual;

            // F3 (jiguang/laser) reaction-consequence state. f3LaserVisual
            // stays PLAYER-anchored (the beam itself); the affected dino's
            // own reaction-state/X-ray-phase is per-instance.
            public bool f3State;
            public bool f3Recover;
            public bool f3Death;
            public float f3ReactionStartTime;
            public bool f3TailDeferred;
            public bool xrayVisualPhase;
            public int xrayPhaseCounter;

            // TASK027N.2.22 Part E: auto-melee bite-test harness attempt
            // latch. The harness itself is explicitly a single-subject DEBUG
            // tool (see its own class doc comment); kept per-instance only
            // so it does not misfire against the wrong dino once several
            // can be attacking concurrently.
            public bool autoMeleeAttemptedThisBite;
        }

        readonly System.Collections.Generic.List<DinoInstance> dinos =
            new System.Collections.Generic.List<DinoInstance>();
        DinoInstance current;
        int nextDinoInstanceId = 1;

        // TASK027N.2.35 Part L: F2/F3 remain single-target BRIDGES (per this
        // task's own explicit instruction not to invent new multi-target F2/
        // F3 mechanics - N.2.34 left native F2/F3 multi-target semantics
        // UNRESOLVED). These track WHICH DinoInstance the player-anchored
        // flame/beam is currently affecting, selected by the same closest-
        // eligible-in-cone helper ABCD uses (FindClosestEligibleDino).
        DinoInstance f2Target;
        DinoInstance f3Target;
        // TASK027N.2.35 diagnostic: proves whether the per-instance
        // EnterFrame closure wired in SpawnDino actually fires. Debug-only,
        // zero gameplay effect.
        int debugEnemyPrimaryFrameClosureFireCount;

        // TASK027N.2.35 Part F interaction fix: several PRE-EXISTING rage
        // tests (Task027N223RageMuzzleHappyRepro.cs's own SetupDForRage())
        // deliberately place a dino 50000 units away to guarantee its AI can
        // never close to bite range within a test's own real-time duration -
        // an established DEBUG-ONLY test convention that predates this
        // task. Part F's own new live-offscreen-removal (exactly 480 native
        // source units past either screen bound, per N.2.34's own proven
        // sub_FDE0 contract) is PRODUCTION-FAITHFUL and must not be weakened
        // - so rather than shrinking or bypassing that real margin, this
        // DEBUG_ONLY flag lets a test explicitly suppress Part F's removal
        // for the duration it needs a dino parked far away, without
        // touching the 480-unit contract other callers rely on. Default
        // false (production/default sandbox behavior unaffected).
        bool debugSuppressOffscreenRemoval;

        // TASK027N.1's own "the sandbox weapon/reaction core is single-
        // target" comment is SUPERSEDED by this task - ActiveDinoCount now
        // reports the TRUE collection size (0..N), not a 0/1 invariant.
        int ActiveDinoCount => dinos.Count;
        DinoInstance PrimaryDino => dinos.Count>0 ? dinos[0] : null;

        // ---- Backward-compatible field shims -----------------------------
        // Every one of these preserves its EXACT old name/type so every
        // existing method body below (TickEnemy, ApplyHeavyWeaponHit,
        // BeginStandardEnemyDeath, OnEnemyPrimaryFrame, the whole F2/F3
        // state machine, etc.) compiles and runs completely unmodified,
        // now reading/writing whichever DinoInstance `current` points at.
        Transform enemy { get => current?.transform; }
        Power2DSourceTruthRenderer enemyVisual { get => current?.visual; }
        float enemyHP { get => current!=null?current.hp:0f; set { if(current!=null) current.hp=value; } }
        float enemyVelocityX { get => current!=null?current.velocityX:0f; set { if(current!=null) current.velocityX=value; } }
        float enemyRuntimeAdis { get => current!=null?current.runtimeAdis:0f; set { if(current!=null) current.runtimeAdis=value; } }
        int enemyAdisJitter { get => current!=null?current.adisJitter:0; set { if(current!=null) current.adisJitter=value; } }
        bool enemyFacingRight { get => current!=null && current.facingRight; set { if(current!=null) current.facingRight=value; } }
        bool enemyAttacking { get => current!=null && current.attacking; set { if(current!=null) current.attacking=value; } }
        bool enemyHitThisCycle { get => current!=null && current.hitThisCycle; set { if(current!=null) current.hitThisCycle=value; } }
        bool enemyHeavyReaction { get => current!=null && current.heavyReaction; set { if(current!=null) current.heavyReaction=value; } }
        string enemyClipPrefix { get => current!=null?current.clipPrefix:"k3"; set { if(current!=null) current.clipPrefix=value; } }
        float enemySourceSpeed { get => current!=null?current.sourceSpeed:0f; set { if(current!=null) current.sourceSpeed=value; } }
        float enemySourceAttack { get => current!=null?current.sourceAttack:0f; set { if(current!=null) current.sourceAttack=value; } }
        float enemySourceReward { get => current!=null?current.sourceReward:0f; set { if(current!=null) current.sourceReward=value; } }
        DebugDinoKind debugDinoKind { get => current!=null?current.kind:DebugDinoKind.K3Blue; set { if(current!=null) current.kind=value; } }
        bool enemyBurningF2 { get => current!=null && current.burningF2; set { if(current!=null) current.burningF2=value; } }
        bool enemyF2Residual { get => current!=null && current.f2Residual; set { if(current!=null) current.f2Residual=value; } }
        bool enemyF2BurnDeath { get => current!=null && current.f2BurnDeath; set { if(current!=null) current.f2BurnDeath=value; } }
        F2EnemyBigFireVisual f2BigFireVisual { get => current?.f2BigFireVisual; set { if(current!=null) current.f2BigFireVisual=value; } }
        bool enemyF3State { get => current!=null && current.f3State; set { if(current!=null) current.f3State=value; } }
        bool enemyF3Recover { get => current!=null && current.f3Recover; set { if(current!=null) current.f3Recover=value; } }
        bool enemyF3Death { get => current!=null && current.f3Death; set { if(current!=null) current.f3Death=value; } }
        float enemyF3ReactionStartTime { get => current!=null?current.f3ReactionStartTime:0f; set { if(current!=null) current.f3ReactionStartTime=value; } }
        bool enemyF3TailDeferred { get => current!=null && current.f3TailDeferred; set { if(current!=null) current.f3TailDeferred=value; } }
        bool f3XrayVisualPhase { get => current!=null && current.xrayVisualPhase; set { if(current!=null) current.xrayVisualPhase=value; } }
        int f3XrayPhaseCounter { get => current!=null?current.xrayPhaseCounter:0; set { if(current!=null) current.xrayPhaseCounter=value; } }
        bool autoMeleeAttemptedThisBite { get => current!=null && current.autoMeleeAttemptedThisBite; set { if(current!=null) current.autoMeleeAttemptedThisBite=value; } }
        // -------------------------------------------------------------------

        DCManifest data;
        Power2DSourceTruthRenderer playerVisual;
        Transform player;

        float playerHP, invulnUntil;
        float playerHurtVelocityX;
        float playerInvulnColourScale = 1f;
        float playerInvulnColourRate = 4f;
        bool playerDead;

        // TASK027N.2 Part T-W: DEBUG_ONLY / SANDBOX_TOOLING, not native
        // gameplay. Default OFF. Implemented at the authoritative HP-loss
        // owner (DamagePlayer) per Part U - blocks only the HP decrement/
        // death branch; every other hit-reaction side effect (hurt anim,
        // knockback, invulnerability window, facing) still runs exactly as
        // for a normal hit, so bite-timing/hurt-reaction testing remains
        // meaningful while repeating trials without dying.
        bool godMode;

        // TASK027N.2 Part S: DEBUG_ONLY overhead HP label toggle. Default
        // ON for this combat-debug sandbox.
        bool showHPOverlay = true;

        // TASK027N.2.31 Part A: DEBUG_ONLY infinite-ammo toggle. Default
        // OFF. Implemented at the SOLE authoritative ammo-decrement site
        // (DischargeCurrentWeapon(), the only "familyAmmo[x]--" in the
        // whole file) by letting the normal decrement run, then restoring
        // it immediately afterward - preserving every other side effect
        // (audio, damage, cadence, animation) exactly as-is, and correctly
        // preventing DischargeCurrentWeapon()'s own "ran out, auto-switch"
        // check further down the SAME function from ever seeing a
        // depleted value. Also bypasses the SEPARATE ammo<=0 gate in
        // BeginFireRequest() so firing is never blocked even if ammo was
        // already at/below 0 before the toggle was turned on.
        bool infiniteAmmo;

        // TASK027N.2.31 Part B: DEBUG_ONLY auto-spawn for Types 1/2/3 only
        // (excludes Type 4), random type+side, respawns strictly on the
        // existing single-active-dino invariant (ActiveDinoCount is
        // PROVEN always 0 or 1 - see its own doc-comment) going to 0 -
        // deliberately a SEPARATE system from the existing general-purpose
        // AutoSpawnTick() (which is timer-interval-based, supports all 4
        // types via autoTypeMode, and can replace a still-living dino) so
        // as not to silently change that pre-existing tool's own behavior.
        bool autoSpawn123;
        int autoSpawn123LastType = -1;   // -1 = none spawned yet this session
        SpawnSide autoSpawn123LastSide = SpawnSide.Right;

        // TASK027N.2.22 Part E/F: DEBUG_ONLY autoparry-style auto-melee bite
        // harness. Schedules exactly ONE melee attempt per detected bite at
        // a chosen offset (source frames) relative to the bite's own frame-
        // 12 commit event (bite_commit_timeline_n222.json), then logs the
        // full outcome. Hotkeys (avoid the existing space/F/Tab/G/T/H/P/I/
        // 1-4/R/O/L/B bindings): M toggles ON/OFF, N toggles FIXED/SWEEP
        // mode, comma/period adjust the fixed offset by 1 frame. Zero effect
        // on production controls while OFF (default).
        internal enum AutoMeleeMode { FixedOffset, AutoSweep }
        bool autoMeleeBiteTestEnabled;
        AutoMeleeMode autoMeleeMode = AutoMeleeMode.FixedOffset;
        int autoMeleeFixedOffset;
        static readonly int[] AutoMeleeSweepOffsets = { -4,-3,-2,-1,0,1,2,3,4 };
        int autoMeleeSweepIndex;
        bool autoMeleePendingFinalize;
        int autoMeleePendingFramesWaited;
        int autoMeleePendingIntendedOffset;
        int autoMeleePendingIssueFrame;
        bool autoMeleePendingWasAttackingBeforeIssue;
        float autoMeleePendingHpBefore;
        string autoMeleeLastResultSummary = "-";
        int autoMeleeAttemptCount;
        internal bool TestHarness_AutoMeleeEnabled
        { get => autoMeleeBiteTestEnabled; set => autoMeleeBiteTestEnabled=value; }
        internal void TestHarness_SetAutoMeleeFixedOffset(int offset)
        {
            autoMeleeMode=AutoMeleeMode.FixedOffset;
            autoMeleeFixedOffset=offset;
        }
        internal void TestHarness_SetAutoMeleeSweep() => autoMeleeMode=AutoMeleeMode.AutoSweep;
        internal string TestHarness_AutoMeleeLastResult => autoMeleeLastResultSummary;
        internal int TestHarness_AutoMeleeAttemptCount => autoMeleeAttemptCount;

        // TASK027N.2.8 Part L: DEBUG_ONLY blood-particle slow-motion toggle,
        // for comparing the first fraction of a second of blood motion
        // against the original iPad. Default OFF (DinoCapDamageParticle.
        // DebugTimeScale stays 1f, zero production-timing effect).
        bool bloodDebugSlowmo = false;
        // All gameplay/profile constants live in DinoCapSourceProfileV11.
        // Do not duplicate or screen-scale them here.

        int money;
        int equippedFamily=0;
        readonly int[] familyTier={0,0,0,0,0,0};
        readonly int[] familyAmmo={200,50,150,200,30,250};
        int meleeTier=0;
        bool playerMelee;
        bool fireHeld;
        bool previousFireHeld;
        bool guiFireHeld;
        bool firePressQueued;

        // TASK027N.1.3.2: mirrors native byte_21BE74 - a CONTINUOUS "is Blue
        // physically down right now" level, re-sampled every PollKeyboard()
        // call from the same key/mouse state that already drives the
        // existing edge-triggered Melee() calls below. Read (never edge-
        // latched) exactly once, at the hurt-completion boundary in
        // Update()'s playerHitStun block - see the fix rationale there.
        bool blueHeld;
        bool guiBlueHeld;

        // Task027L.1 V30.5:
        // A semi-auto press that BEGINs while melee owns the action is not
        // consumed merely because melee later ends. It remains blocked until
        // the real Orange release edge.
        bool semiAutoPressBlockedByMelee;

        // A real Blue edge is eligible for same-Update post-completion reclaim
        // only when the melee CLIP had already completed before PollKeyboard
        // saw that edge. An edge while the clip is still active stays rejected
        // and is never queued.
        bool bluePressedAfterMeleeClipEndedThisFrame;

        int sustainedShotCounter;
        float faceResetAt=-1f;
        // TASK027N.2.25: mirrors native's own player+188 "fire session
        // active" flag (PROVEN via sub_667B0's own reset gate:
        // `if(!*(a1+188)){...; sustainedShotCounter=0;}`). Set true on a
        // fresh C/D press (BeginFireRequest); cleared ONLY on a genuine
        // release (EndFireRequest) or player hurt/death - NOT touched by
        // weapon switching (PROVEN: sub_65A08, native's own switch handler,
        // never references this flag). Distinct from sustainedShotCounter
        // (the raw progress value) and from headRefOverride (the visible
        // face level, which decays independently on its own timer) - the
        // three are deliberately separate per the user's own explicit
        // "visible level vs hidden progression" correction.
        bool rageSessionActive;

        bool left, right;
        float cameraLeftX;
        bool cameraCatchupActive;
        bool previousPlayerFacingRight;
        bool cameraInitialized;
        bool playerHitStun;

        // Task027J2: source G-family melee attack audio.
        // G1/G2/G3 share Attack_G.wav; G4 alone uses Audio_G4.wav.
        AudioSource meleeAudioSource;
        AudioClip playerHurtClip;
        AudioClip meleeAttackGClip;
        AudioClip meleeAttackG4Clip;
        // TASK027N.2 Part J: G-tier CONTACT sound (distinct from the swing
        // sound above). Native sub_10554 LABEL_70 selects Audio_att1Audio
        // for melee tier 0/1 (G1/G2) or Audio_att2Audio for tier 2/3
        // (G3/G4) - PROVEN_SHARED within each pair, PROVEN_DIFFERENT across
        // pairs, confirming the user's audible hypothesis exactly (see
        // g_audio_pairwise_verification_n2.json). Only played on a genuine
        // hit (ExecuteMeleeContact's DebugRecordMeleeDamageDispatched()
        // call site), never merely on the swing reaching frame 10 - see
        // g_swing_vs_contact_event_ownership_n2.json's CONTACT_REQUIRES_
        // VALID_HIT=true finding.
        AudioClip meleeContactAtt1Clip;
        AudioClip meleeContactAtt2Clip;
        // TASK027N.2.49R REOPEN Phase D: fresh IDA archaeology (this
        // reopening) PROVED sub_11980 (Standard death) plays Audio_death1
        // directly at t=0 as the LAST statement of its own body - this was
        // NOT wired anywhere in this file before this change (a genuine
        // gap, not a stale doc). See dino_death_sfx_source_truth_n249r.json.
        AudioClip deathAudio1Clip;
        AudioClip deathAudio2Clip;
        // TASK027N.2.49R REOPEN Phase D: deathAudio3Clip's own former
        // BeginF2BurnDeath() consumer is REMOVED below - fresh IDA proved
        // Audio_death3 belongs to Standard/GSpecial (die.a2b frame-17 /
        // die2.a2b frame-18 authored event marks), NOT F2/F3 Ash-Death
        // (shaosi.a2b carries no event mark at all - exhaustively checked
        // against the independent Task018 EventMarkBounds ledger). The
        // field itself is RETAINED (not deleted) since the stacked-cue
        // mechanism for Standard/GSpecial remains a disclosed, not-yet-
        // implemented LIKELY-confidence follow-on (see the JSON's own
        // "recommended_correction_not_applied_this_task" - the causal
        // event-dispatch chain was not independently re-traced to PROVEN,
        // so it is not shipped here per this project's own no-heuristic
        // policy) - kept for that future work rather than removed and
        // re-added.
        AudioClip deathAudio3Clip;
        AudioClip attackF1Clip;
        AudioClip attackF2Clip;
        AudioClip burnSizzleClip;
        AudioSource attackF2Source;
        AudioSource burnSizzleSource;
        F2FlameVisual f2FlameVisual;

        // Task027M.2: F3 (jiguang / laser) - a PARALLEL subsystem. Every F2
        // field/path above is untouched. Native (Task027M.1/M.2): shares the
        // held-auto fire session shape, the front-X cone, the arm helper, the
        // per-event damage applier and the ShaoSiAnim ash death with F2 - but
        // uses a DISTINCT dino reaction state (dino+0xAC == 4), the 'BEAnim'
        // (k*_be) electrocute spasm ping-ponged frames 2..4 at EXACTLY 0.1 s
        // (PROVEN Part B: 6 healthy 60 Hz Power2D callbacks, not 3*0.025), the
        // k*_be frame-3 authored event for damage (formula, Part A), NO
        // dino-side audio, NO Audio_att3, NO residual burn, NO Dahuo child.
        F3LaserVisual f3LaserVisual;
        bool f3SessionActive;
        int  f3UpgradeLevel = 0;  // native dword_21BE18[5] equivalent; 0 in this sandbox (no upgrade system). F3Damage() covers L>=1.
        const float NativeBeFrame3Interval = 0.1f;   // Part B: steady k*_be frame-3 recurrence
        // Task027M.8.2 DEFECT A - F3 post-hit reaction lifetime.
        // Native trace (sub_12884 / sub_68A8C / sub_12C64 / sub_101A0 / sub_FD58 /
        // sub_76980 / sub_76A20): there is NO fixed post-hit stun timer. The dino
        // is frozen in reaction-state 4 (velocity 0, k*_be 2..4 ping-pong) for as
        // long as the COMMITTED first discharge keeps the beam in the front cone,
        // then plays the k*_be frames 4->7 recovery tail (state 6, ~0.10 s
        // wall-clock: 3 advances x 2 healthy 60 Hz callbacks), then AI resumes the
        // frame reaction-state 8 is reached (sub_FD58; states 3..7 all suppress
        // the AI block; state 8 also re-normalises the transform). The user's
        // "~0.5-1.0 s stunned" = (committed-discharge beam freeze) + (~0.10 s tail)
        // + (~0.033 s detect/dispatch) - a COMPOUND, not a timer. See
        // Dev/Reports/Task027M_8_2_.../f3_final_stun_contract_m82.json.
        //
        // N253 supersedes the historical V30.23 minimum-floor adapter:
        // actual12F28 effect completion owns target exit;12C64 has no floor.
        // Task027M.3 defect 2: F3 quick-tap. Native sub_663A0 F-path commits the
        // first discharge on the first Update Shoot is held (it jumps the player
        // fire-anim to frame 3 -> the frame-3 PLAYER_WEAPON_DISCHARGE event ->
        // sub_66CE8 ammo-- + sub_67CE8). Release cannot cancel it - it only
        // stops the next 3..4 re-loop cycle. So F3 release must DEFER (not Stop
        // the secondary), exactly like F2's f2ReleasePending.
        bool f3ReleasePending;
        // Task027M.8.3: F3 electrocution X-ray phase.
        // The original iPad alternates an F3-affected dino between a
        // hurt/electrocution pose and an anatomical X-ray (bones through the
        // body). Dino Cap ships a COMPLETE per-species white pixel-art skeleton
        // in every atlas - the `k*gt*` (gt = gutou / bone) parts - but no
        // recovered native code instantiates them (all string xrefs are inside
        // the atlas builders). So:
        //   * BONE ART  = SOURCE_FAITHFUL - the real k*gt* pixels, extracted to
        //                 Resources/DinoCapGenerated/Textures/f3xray_skeleton.png
        //                 and swapped in per body part by Power2DSourceTruthRenderer
        //                 under the identical k*_be part transform.
        //   * ALTERNATION TIMING = PLATFORM_ADAPTER / VISUAL_RECONSTRUCTION -
        //                 flipped on the existing k*_be frame-3 reaction event
        //                 (deterministic, source-backed cadence; NOT a new timer).
        // Presentation ONLY: no damage, no audio, no AI, no HP, no collision, no
        // F2 state. Clears on every F3 exit / death / ash / reset path. Frame-3
        // DoT ownership is unchanged and independent of the visual phase.
        bool f3XrayEnabled = true;     // production X-ray (source-faithful bone art). Debug toggle for A/B.
        // DEBUG phase cadence: 0 = flip on every BE frame-3 event; 1 = every 2nd. NOT a gameplay knob.
        int  f3XrayPhaseMode = 0;
        // The M8/M8.1 generic luminance-inversion shader path
        // (Power2DSourceTruthRenderer.xrayFlash) stays DEBUG_ONLY, default OFF -
        // superseded by the source-faithful skeleton swap above.
        AudioClip attackF3Clip;
        AudioSource attackF3Source;

        // TASK027N.2.22 Part I/K: A-D fire audio fields - see the loader's
        // own doc comment for the full native citation and the proven C/D
        // swap. attackCFamilyClip/attackDFamilyClip hold the file that each
        // FAMILY's OWN fire event actually plays (post-swap), not the file
        // matching the family's own letter.
        AudioClip attackAClip, attackBClip, attackCFamilyClip, attackDFamilyClip;
        AudioClip attackE1Clip,attackE2Clip;
        AudioSource abcdFireSource;
        internal AudioClip TestHarness_LastAbcdFireClip { get; private set; }

        // The shipped k*_shao burn loop is a regular ~0.23333 s: 7 frames x a
        // 0.03333 s effective dwell (source frameDuration 0.025 s, but native
        // sub_76A20 advances <= 1 frame per 60 Hz logic tick and hard-resets its
        // accumulator, so 0.025 s costs 2 ticks). Task027L.4.10 moved the fix to
        // where it belongs - Power2DSourceTruthRenderer now advances every
        // power2D animation on that shipped 60 Hz tick - so the Audio_att3
        // frame-3 event recurs at ~0.23333 s at any host refresh with NO audio
        // gate. This constant is retained only as the documented loop period
        // (and a telemetry reference); it no longer rate-limits anything.
        const float NativeShaoLoopSeconds = 0.233f;
        float lastF2ShaoEventTime = -999f;
        // Task027L.4.8 / V30.9 P1 SOURCE_FAITHFUL: the post-release residual burn
        // is ANIMATION-COMPLETION owned, not timer owned. Native huoyanAnim is an
        // 11-frame one-shot (combat_manifest). While Shoot is held, sub_679F4
        // re-triggers it 0->5 each fire tick; on release sub_663A0 lets it
        // continue from its CURRENT frame (0..5) to frame 10
        // (sub_76980(effect,0,curFrame,0.025,-1)), and only when effectClassiPad
        // vtable+0x24 (sub_12F28) observes frame 10 does sub_12C64 drive dino
        // state 5 -> 6. Through that finish window the dino is still state 5, so
        // sub_10294 case 5 keeps firing DoT + the frame-3 Audio_att3 event -
        // this is the deterministic "1 or 2" residual tail. V30.8 modelled the
        // finish as a fixed 0.15 s; it is really (10 - releaseFrame) Power2D
        // frames, i.e. release-phase-variable ~0.167-0.333 s, which is why the
        // burnt state was exiting too early. Model the frames directly.
        const float NativePower2DFrameSeconds = 0.0333f;   // 0.233 s / 7 frames
        const int   NativeHuoyanLastFrame     = 10;         // 11-frame one-shot
        const int   NativeHuoyanHeldLoopLast  = 5;          // sub_679F4 held ...,0,0,...,5
        // Time inside the current held huoyan 0->5 re-trigger cycle; snapshots
        // the release frame when Shoot is let go.
        float f2HuoyanCycleSeconds;
        float f2PostReleaseGrace;

        // Task027K4.1: native F2 one-shot fire animation is completion-owned.
        // Raw Orange-up must not synchronously cut the in-flight secondary clip.
        bool f2ReleasePending;

        // Task027L.3 V30.6D: mirror native player+188.
        // True only after a legitimate frame-0 F2 fire session is attached.
        bool f2SessionActive;

        // Task027L.3 / V30.6C:
        // A melee interrupt ends the current F2 firing session. If Orange
        // remains physically held after melee, F2 must re-enter through its
        // initial 0..4 startup, not jump directly to the held 3..4 cycle.
        bool f2RestartInitialAfterMelee;

        // Task027L.4.8 / V30.9 P2 SOURCE_FAITHFUL: native sub_667B0 C/D case
        // restarts the fire animation at frame 0 (not 3) and zeroes the
        // sustained/rage-face shot counter (player+132) on the FIRST auto shot
        // re-armed after a melee interrupt; steady-state held auto (sub_663A0)
        // then loops frames 3..6. byte_21BE78 (Shoot held) SURVIVES melee -
        // byte_21BE74 (melee latch) only suppresses the re-arm while a swing is
        // in progress (sub_66550: melee branch beats the fire-held branch), so
        // native C/D full-autos in the gaps between repeated melee swings with
        // no fresh Shoot press required.
        bool cdRestartFireAfterMelee;

        // DEBUG_ONLY: editable current-enemy HP for burn-state inspection.
        string debugEnemyHPText="1000";

        // TASK027N.1.2 Part Q - DEBUG_ONLY last-hit reaction readout. Written
        // at every enemy-damage site via DebugRecordHit; never read by gameplay
        // logic. The [Conditional] attribute compiles every call site out of a
        // production (non-DEVELOPMENT) build, so these fields then stay at their
        // initial values and cost nothing.
        string dbgLastHitFamily="-";
        float  dbgLastHitDamage;
        bool   dbgLastHitHurtVisual;
        bool   dbgLastHitKnockback;
        bool   dbgLastHitAttackInterrupted;
        string dbgLastHitReactionState="-";
        int    dbgLastHitFrame=-1;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        void DebugRecordHit(string family,float dmg,bool hurtVisual,
            bool knockback,bool attackInterrupted,string reactionState)
        {
            dbgLastHitFamily=family;
            dbgLastHitDamage=dmg;
            dbgLastHitHurtVisual=hurtVisual;
            dbgLastHitKnockback=knockback;
            dbgLastHitAttackInterrupted=attackInterrupted;
            dbgLastHitReactionState=reactionState;
            dbgLastHitFrame=Time.frameCount;
        }

        // TASK027N.1.3 Part Q - DEBUG_ONLY outgoing-melee readout. Traces the
        // melee dispatch chain (contact event -> target test -> damage) so a
        // future repro of the Part L/M/N invulnerability report can capture
        // the exact BLOCK REASON instead of relying on visual guesswork. All
        // writers are [Conditional]-stripped from production builds.
        bool   dbgMeleeContactEvent;
        bool   dbgMeleeTargetFound;
        bool   dbgMeleeDamageDispatched;
        string dbgMeleeBlockReason="none";
        int    dbgMeleeFrame=-1;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        void DebugRecordMeleeContactEvent()
        {
            dbgMeleeContactEvent=true;
            dbgMeleeTargetFound=false;
            dbgMeleeDamageDispatched=false;
            dbgMeleeBlockReason="none";
            dbgMeleeFrame=Time.frameCount;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        void DebugRecordMeleeTargetFound() { dbgMeleeTargetFound=true; }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        void DebugRecordMeleeDamageDispatched() { dbgMeleeDamageDispatched=true; }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        void DebugRecordMeleeBlock(string reason) { dbgMeleeBlockReason=reason; }

        // TASK027N.1.3.2 Part N - DEBUG_ONLY real-input readout fields.
        bool dbgBluePressEdgeThisFrame;
        bool dbgBlueBufferedStartThisFrame;
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        void DebugRecordBluePressEdge() { dbgBluePressEdgeThisFrame=true; }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        void DebugRecordBlueBufferedStart() { dbgBlueBufferedStartThisFrame=true; }

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
        // TASK027N.1.3.1: internal-only test-harness surface for the PlayMode
        // runtime-repro suite (Assets/DinoCap/Tests/Task027N131RuntimeRepro.cs).
        // Thin wrappers over EXISTING private methods/fields - no gameplay
        // logic added, changed, or duplicated. `internal` is visible to any
        // class in this assembly (the test script has no asmdef, so it also
        // compiles into Assembly-CSharp) without widening any PUBLIC surface,
        // and the whole block is compiled out of a non-editor/non-development
        // build by this conditional guard, same convention as every other
        // debug affordance in this file.
        // speciesIndex: 0=K3Blue 1=K1Rusher 2=K2Trex 3=K4Raptor (matches
        // DebugDinoKind's declaration order). Takes a plain int rather than
        // the enum type so the test script (a different class) does not need
        // that private nested enum's accessibility widened.
        internal void TestHarness_SpawnDino(int speciesIndex,bool rightSide)
            => SpawnDino((DebugDinoKind)speciesIndex, rightSide ? SpawnSide.Right : SpawnSide.Left);
        internal void TestHarness_ClearDinos() => ClearDinos();

        // TASK027N.2.35 Part S: multi-dino test accessors. Every one below
        // is a strictly read-only (or explicitly-scoped) accessor over the
        // SAME `dinos` collection/property-shim architecture production
        // code uses - no parallel test-only state.
        internal int TestHarness_ActiveDinoCount => dinos.Count;
        internal int TestHarness_DinoId(int index) => dinos[index].id;
        internal float TestHarness_DinoHP(int index) => dinos[index].hp;
        internal float TestHarness_DinoX(int index) => dinos[index].transform.position.x;
        internal bool TestHarness_DinoMarkedForRemoval(int index) => dinos[index].markedForRemoval;
        internal bool TestHarness_DinoBurningF2(int index) => dinos[index].burningF2;
        internal bool TestHarness_DinoF3State(int index) => dinos[index].f3State;
        // Selects `current` to dinos[index] so every existing single-target
        // TestHarness_* accessor (enemyHP, enemy position, etc.) reads/
        // writes that SPECIFIC instance for the remainder of the test step.
        internal void TestHarness_SelectDino(int index) => current=dinos[index];
        internal void TestHarness_SetDinoX(int index,float x)
        {
            var d=dinos[index];
            if(d.transform!=null) d.transform.position=new Vector3(x,d.transform.position.y,d.transform.position.z);
        }
        // TASK027N.2.35 Parts P/Q: City Grind spawner test-mode accessors.
        internal void TestHarness_SetCityGrindSpawner(bool on) => cityGrindSpawnerOn=on;
        internal void TestHarness_SetCityGrindLevel(int n) => cityGrindLevel=Mathf.Max(1,n);
        internal void TestHarness_SetCityGrindDifficulty(int d) => cityGrindDifficulty=Mathf.Clamp(d,1,3);
        internal void TestHarness_ForceCityGrindSpawnNow(int typeIndex0to2)
            => SpawnCityGrindDino(typeIndex0to2,Mathf.Max(1,cityGrindLevel),Mathf.Clamp(cityGrindDifficulty,1,3));
        internal bool TestHarness_DinoNativeCityGrindSpawn(int index) => dinos[index].nativeCityGrindSpawn;

        // TASK027N.2.35 Part F test-infrastructure bridge - see
        // debugSuppressOffscreenRemoval's own doc comment.
        internal void TestHarness_SetSuppressOffscreenRemoval(bool on) => debugSuppressOffscreenRemoval=on;

        internal void TestHarness_ForceOffscreenRemoval(int index)
        {
            // Test-only: jumps a living dino's X far enough past the screen
            // bound to trigger Part F's own 480-unit live-removal check on
            // its own next TickEnemy(), without waiting on real AI movement.
            var d=dinos[index];
            if(d.transform!=null)
                d.transform.position=new Vector3(cameraLeftX-1000f,d.transform.position.y,d.transform.position.z);
        }
        internal void TestHarness_Melee() => Melee();
        internal void TestHarness_HoldRight(bool on) => HoldRight(on);
        internal void TestHarness_HoldLeft(bool on) => HoldLeft(on);
        internal void TestHarness_SetDebugEnemyHP(float hp)
        {
            // Mirrors SetDebugEnemyHPFromText()'s effect without the UI text
            // round-trip, for precisely staging a lethal-vs-non-lethal hit.
            enemyHP=Mathf.Clamp(hp,1f,999999f);
        }
        internal void TestHarness_SetWeapon(int family,int tier,int ammo)
        {
            equippedFamily=Mathf.Clamp(family,0,5);
            familyTier[equippedFamily]=tier;
            familyAmmo[equippedFamily]=ammo;
        }
        internal void TestHarness_SetMeleeTier(int tier) => meleeTier=Mathf.Clamp(tier,0,3);
        internal void TestHarness_SetGodMode(bool on) => godMode=on;
        internal bool TestHarness_GodMode => godMode;
        // TASK027N.2.31 Part A/B test accessors.
        internal void TestHarness_SetInfiniteAmmo(bool on) => infiniteAmmo=on;
        internal bool TestHarness_InfiniteAmmo => infiniteAmmo;
        internal void TestHarness_SetAutoSpawn123(bool on) => autoSpawn123=on;
        internal bool TestHarness_AutoSpawn123 => autoSpawn123;
        internal int TestHarness_AutoSpawn123LastType => autoSpawn123LastType;
        internal bool TestHarness_AutoSpawn123LastSideIsLeft => autoSpawn123LastSide==SpawnSide.Left;
        internal int TestHarness_FamilyAmmo(int family) => familyAmmo[family];
        internal void TestHarness_SetShowHPOverlay(bool on) => showHPOverlay=on;
        internal void TestHarness_Shoot() => Shoot();
        // TASK027N.2.22: "TYPE 1/B intentionally ignores a re-press while
        // active" (BeginFireRequest's own pre-existing, source-faithful
        // comment) - a new Shoot() request is silently dropped while the
        // player's own fire animation is still active, so a test that
        // switches weapon family and fires again immediately can lose the
        // press. Exposed so tests can poll for "previous shot finished"
        // before issuing the next one, instead of guessing a fixed wait.
        internal bool TestHarness_PlayerSecondaryActive =>
            playerVisual!=null && playerVisual.secondary.Active;
        // C/D (and F2/F3) are native HELD-FIRE families - a single Shoot()
        // press-edge never triggers them (see UpdateWeaponFireInput's
        // semiAutoMode list). Test-only sustained-hold setter.
        internal void TestHarness_SetFireHeld(bool held) => fireHeld=held;
        // TASK027N.2.23/N.2.25: rage/sustained-fire-face state, for the
        // accumulation/decay/repress/switch/hurt/exclusion focused tests.
        internal int TestHarness_SustainedShotCounter => sustainedShotCounter;
        internal string TestHarness_HeadRefOverride =>
            playerVisual!=null ? playerVisual.headRefOverride : "";
        // TASK027N.2.30 Part D: the GROUND-TRUTH rendered face ref, as
        // actually applied to the yyj6 part by Render() this frame -
        // independent of the logical headRefOverride string above (see
        // rage_unity_logical_vs_render_trace_n230.json). A test/consumer
        // must call this AFTER a frame has actually run Render() (i.e.
        // after a yield) to see the current value.
        internal string TestHarness_LastRenderedHeadRef =>
            playerVisual!=null ? playerVisual.LastRenderedHeadRef : "";
        internal bool TestHarness_RageSessionActive => rageSessionActive;
        internal int TestHarness_EquippedFamily => equippedFamily;
        // TASK027N.2.29: direct, deterministic hook into the exact
        // production increment function a repress/burst actually drives -
        // lets a test control PRECISELY how many shots land without racing
        // the real animation-driven discharge cadence (which this
        // environment was found to advance much faster/more unevenly per
        // real-clock second than a short timeout-based HoldFireUntil() can
        // reliably bound - see Rage_NewSessionCrossesThreshold_
        // ReplacesOldFaceImmediately's own revised approach).
        internal void TestHarness_AdvanceSustainedFireFace() => AdvanceSustainedFireFace();
        // TASK027N.2.29: direct setters so a concurrency test can construct
        // "a new session is independently active with exactly N sub-
        // threshold shots" as a precise, race-free starting state, rather
        // than trying to bound the real animation-driven discharge cadence
        // (found to vary too much per real-clock second in this environment
        // to reliably stop AT a specific shot count via a short timeout).
        internal void TestHarness_SetRageSessionActive(bool active) => rageSessionActive=active;
        internal void TestHarness_SetSustainedShotCounter(int v) => sustainedShotCounter=v;
        // TASK027N.2.27 Part C: exposed so a frame-by-frame trace test can
        // log the exact decay-timer state (faceResetAt) and raw fireHeld
        // alongside the fields already exposed above, matching the
        // rage_manual_release_live_trace_n227.json field list exactly.
        internal float TestHarness_FaceResetAt => faceResetAt;
        internal bool TestHarness_FireHeld => fireHeld;
        internal void TestHarness_DamagePlayer(float d) => DamagePlayer(d);
        // TASK027N.2.23 Part B: diagnostic-only accessor for the muzzle
        // socket's own current world position (same call SpawnMuzzleForCurrentWeapon
        // itself uses) - no gameplay effect, exposed purely so a test can
        // MEASURE the current stationary-vs-moving offset without asserting
        // a specific "correct" value this task could not independently prove.
        internal bool TestHarness_TryGetMuzzleWorldSocket(out Vector3 pos)
        {
            pos=Vector3.zero;
            if(playerVisual==null) return false;
            string socket=GetFireSocket();
            Vector3 p; float rot; Vector2 size;
            bool ok=playerVisual.TryGetPartWorldSocket(socket,0.5f,0.2f,out p,out rot,out size);
            pos=p;
            return ok;
        }
        internal void TestHarness_SetPlayerX(float x)
        {
            if(player!=null) player.position=new Vector3(x,player.position.y,player.position.z);
        }
        internal float TestHarness_PlayerX => player!=null?player.position.x:0f;
        internal bool TestHarness_PlayerFacingRight => playerVisual!=null && playerVisual.facingRight;
        internal void TestHarness_SetEnemyX(float x)
        {
            if(enemy!=null) enemy.position=new Vector3(x,enemy.position.y,enemy.position.z);
        }
        // TASK027N.2.24 Part C: fire-state diagnostic snapshot, DEBUG_ONLY,
        // no gameplay effect - exposes every field the N.2.23 investigation
        // flagged as a candidate for the rage-test flakiness (fireHeld edge
        // state, F2/F3 session/release flags, weapon/ammo, player action
        // gates, and the player's own secondary-track playback state) so a
        // test can log the EXACT per-frame state during a failing run
        // instead of guessing.
        internal struct FireDebugSnapshot
        {
            public bool fireHeld,previousFireHeld,firePressQueued,semiAutoPressBlockedByMelee;
            public bool playerMelee,playerDead,playerHitStun;
            public int equippedFamily,ammoD;
            public bool f2ReleasePending,f2SessionActive,f3ReleasePending,f3SessionActive;
            public bool secondaryActive;
            public int secondaryFrame;
            public string secondaryClipName;
            public int sustainedShotCounter;
            public string headRefOverride;
            public float faceResetAtOut,timeNow;
            public bool keyboardCurrentIsNull;
            // TASK027N.2.27 Part C: rageSessionActive postdates this struct
            // (added N2.25) - added here so the release live-trace test can
            // log it alongside every other fire-related field in one snapshot.
            public bool rageSessionActive;
        }
        internal FireDebugSnapshot TestHarness_CaptureFireDebugSnapshot()
        {
            var s=new FireDebugSnapshot();
            s.fireHeld=fireHeld; s.previousFireHeld=previousFireHeld;
            s.firePressQueued=firePressQueued; s.semiAutoPressBlockedByMelee=semiAutoPressBlockedByMelee;
            s.playerMelee=playerMelee; s.playerDead=playerDead; s.playerHitStun=playerHitStun;
            s.equippedFamily=equippedFamily; s.ammoD=familyAmmo[3];
            s.f2ReleasePending=f2ReleasePending; s.f2SessionActive=f2SessionActive;
            s.f3ReleasePending=f3ReleasePending; s.f3SessionActive=f3SessionActive;
            s.secondaryActive=playerVisual!=null && playerVisual.secondary.Active;
            s.secondaryFrame=playerVisual!=null?playerVisual.secondary.Frame:-1;
            s.secondaryClipName=(playerVisual!=null && playerVisual.secondary.Clip!=null)?playerVisual.secondary.Clip.name:"-";
            s.sustainedShotCounter=sustainedShotCounter;
            s.headRefOverride=playerVisual!=null?playerVisual.headRefOverride:"";
            s.faceResetAtOut=faceResetAt;
            s.timeNow=Time.time;
            s.keyboardCurrentIsNull=TestHarness_KeyboardCurrentIsNull;
            s.rageSessionActive=rageSessionActive;
            return s;
        }
        internal float TestHarness_EnemySourceSpeed => enemySourceSpeed;
        internal float TestHarness_EnemyRuntimeAdis => enemyRuntimeAdis;
        // TASK027N.2.2 Part E: eliminates enemyRuntimeAdis's per-spawn
        // Random jitter (UnityEngine.Random.Range(0,AdisJitterExclusiveMax))
        // as a scheduling confound, so the predictive bite-boundary
        // scheduler's calibration is repeatable across trials. Does not
        // change the jitter mechanism itself for normal (non-test) play.
        internal void TestHarness_SetEnemyRuntimeAdis(float adis) => enemyRuntimeAdis=adis;
        internal bool TestHarness_EnemyAttacking => enemyAttacking;
        internal float TestHarness_PlayerHP => playerHP;

        // TASK027N.2.21 Part P: forces the enemy into the SAME committed-bite
        // state a real AI-triggered attack reaches (enemyAttacking=true, the
        // "_attack" primary clip active) so the focused bite-mode reaction
        // tests can deterministically test EnemyBiteCommitted()==true without
        // waiting on AI distance/timing. Test-only; no gameplay effect.
        internal void TestHarness_ForceEnemyAttack() => BeginEnemyAttack();
        internal int TestHarness_EnemyPrimaryFrameClosureFireCount => debugEnemyPrimaryFrameClosureFireCount;
        internal bool TestHarness_IsPlayerInNativeAttackCone() => IsPlayerInNativeAttackCone();
        internal float TestHarness_EnemyRuntimeAdisValue => enemyRuntimeAdis;
        internal bool TestHarness_EnemyHitThisCycle => enemyHitThisCycle;
        internal bool TestHarness_EnemyBiteCommitted() => EnemyBiteCommitted();

        // TASK027N.2.3 Part A: strictly read-only accessors for the "tou"
        // anchor trace - no gameplay effect, mirrors exactly what
        // ExecuteMeleeContact() itself reads.
        internal bool TestHarness_TryGetEnemyTouWorldX(out float worldX)
        {
            Vector3 head=enemy!=null?enemy.position:Vector3.zero;
            bool ok=enemyVisual!=null && enemyVisual.TryGetPartWorldAnchor("tou",out head);
            worldX=head.x;
            return ok;
        }
        internal float TestHarness_EnemyActorScale => enemyVisual!=null?enemyVisual.actorScale:1f;
        internal bool TestHarness_EnemyVisualFacingRight => enemyVisual!=null && enemyVisual.facingRight;
        internal Vector3 TestHarness_EnemyPosition => enemy!=null?enemy.position:Vector3.zero;
        internal Vector3 TestHarness_PlayerPosition => player!=null?player.position:Vector3.zero;
        internal float TestHarness_PlayerMaxHP => data!=null?data.combat.playerHP:playerHP;
        internal bool TestHarness_PlayerDead => playerDead;
        internal void TestHarness_SetPlayerHP(float hp) => playerHP=Mathf.Max(0.0001f,hp);
        internal void TestHarness_ForceInvulnExpired() => invulnUntil=Time.time-0.01f;
        // TASK027N.2.2 Part E: lets a test prevent the PLAYER from being
        // hitstunned by the specific dino bite it's using as a scheduling
        // reference, so the player remains free to swing at any point
        // across the bite's duration. Necessary because DamagePlayer()'s
        // OWN frame-12 damage-to-player check (a genuine, separate mechanic
        // from this task's outgoing-melee question) was found to hitstun
        // the player for nearly the bite's full remaining duration, which
        // otherwise makes most of the -3..+3 offset window physically
        // unreachable by a player who was themselves just bitten - not a
        // bite-boundary bug, a different, already-frozen incoming-damage
        // mechanic this task must not perturb by disabling entirely.
        internal void TestHarness_SetPlayerInvulnerableForSeconds(float seconds) => invulnUntil=Time.time+seconds;

        // TASK027N.1.3.2 Part B: raw-button-state test hook, consumed by
        // PollKeyboard() in place of Keyboard.current.fKey - see the
        // rationale comment there. Nullable: null means "use real input."
        // Setting these does NOT call Melee() or any gameplay method; it
        // only supplies the same two booleans a real key would.
        internal bool? TestOverrideBlueIsPressed;
        internal bool? TestOverrideBlueWasPressedThisFrame;
        internal void TestHarness_SetRawBlueKey(bool isPressed,bool wasPressedThisFrame)
        {
            TestOverrideBlueIsPressed=isPressed;
            TestOverrideBlueWasPressedThisFrame=wasPressedThisFrame;
        }
        internal void TestHarness_ClearRawBlueKeyOverride()
        {
            TestOverrideBlueIsPressed=null;
            TestOverrideBlueWasPressedThisFrame=null;
        }
#if ENABLE_INPUT_SYSTEM
        internal bool TestHarness_KeyboardCurrentIsNull => Keyboard.current==null;
#else
        internal bool TestHarness_KeyboardCurrentIsNull => true;
#endif

        internal struct RuntimeDebugSnapshot
        {
            public bool playerDead,playerMelee,playerHitStun;
            public float invulnUntil,timeNow,playerX,enemyX,enemyHP;
            public bool enemyAttacking,enemyHeavyReaction,enemyBurningF2,enemyF3State;
            public bool dbgMeleeContactEvent,dbgMeleeTargetFound,dbgMeleeDamageDispatched;
            public string dbgMeleeBlockReason,playerClipName,enemyClipName,enemyClipPrefixOut;
            public int playerFrame,enemyFrame;
            public bool playerFacingRight,enemyFacingRightOut;
            public int dinoKindIndex;
            public bool hasPlayer,hasEnemy;
            // TASK027N.2.21 Part J/K/L: expose the two fields the bite-mode
            // reaction tests need to distinguish KNOCKBACK from STUN/HURT_ANIM
            // (Part N) - the dino's own horizontal recoil velocity and its
            // secondary (cosmetic reaction) track's currently-playing clip
            // name, so a test can assert "no knockback" (velocity unchanged)
            // and "no stun/hurt-anim" (secondary clip did not restart to
            // *_befired/*_behited) independently rather than conflating them.
            public float enemyVelocityXOut;
            public string enemySecondaryClipName;
        }

        internal RuntimeDebugSnapshot CaptureRuntimeDebugSnapshot()
        {
            var s=new RuntimeDebugSnapshot();
            s.playerDead=playerDead; s.playerMelee=playerMelee; s.playerHitStun=playerHitStun;
            s.invulnUntil=invulnUntil; s.timeNow=Time.time;
            s.hasPlayer=player!=null; s.hasEnemy=enemy!=null && enemy.gameObject.activeSelf;
            s.playerX=player!=null?player.position.x:float.NaN;
            s.enemyX=enemy!=null?enemy.position.x:float.NaN;
            s.enemyHP=enemyHP;
            s.enemyAttacking=enemyAttacking; s.enemyHeavyReaction=enemyHeavyReaction;
            s.enemyBurningF2=enemyBurningF2; s.enemyF3State=enemyF3State;
            s.dbgMeleeContactEvent=dbgMeleeContactEvent; s.dbgMeleeTargetFound=dbgMeleeTargetFound;
            s.dbgMeleeDamageDispatched=dbgMeleeDamageDispatched; s.dbgMeleeBlockReason=dbgMeleeBlockReason;
            s.playerClipName=(playerVisual!=null && playerVisual.primary.Clip!=null)?playerVisual.primary.Clip.name:"-";
            s.playerFrame=playerVisual!=null?playerVisual.primary.Frame:-1;
            s.playerFacingRight=playerVisual!=null && playerVisual.facingRight;
            s.enemyClipName=(enemyVisual!=null && enemyVisual.primary.Clip!=null)?enemyVisual.primary.Clip.name:"-";
            s.enemyFrame=enemyVisual!=null?enemyVisual.primary.Frame:-1;
            s.enemyClipPrefixOut=enemyClipPrefix;
            s.enemyFacingRightOut=enemyFacingRight;
            s.dinoKindIndex=(int)debugDinoKind;
            s.enemyVelocityXOut=enemyVelocityX;
            s.enemySecondaryClipName=(enemyVisual!=null && enemyVisual.secondary.Clip!=null)?enemyVisual.secondary.Clip.name:"-";
            return s;
        }
#endif

        // ============================================================
        // TASK027N.1 - DEBUG_ONLY / SANDBOX_TOOLING all-species spawner.
        // NOT the production Jungle Blitz / City Grind spawn or progression
        // system - purely a manual + automated test-spawn affordance.
        // ============================================================
        // 4 real source species. "prefix" is the runtime clip family in
        // combat_manifest.json; every _move/_attack/_be/_shao/_die clip for
        // all four is present (k2_* / k4_* added additively by
        // Dev/Tools/UnityExport/add_species_k2_k4_n1.py). Type 3/4 are the
        // actual k2 (large T-rex) and k4 ("njz" jungle raptor) art+anim
        // families - NOT recolours/rescales of Type 1/2.
        // TASK027N.2.35: internal (not private) so DinoInstance - also
        // internal, for TestHarness_* multi-dino accessor visibility - can
        // hold a field of this type without a CS0052 accessibility error.
        internal enum DebugDinoKind { K3Blue, K1Rusher, K2Trex, K4Raptor }
        // TASK027N.2.35: debugDinoKind is now a DinoInstance-backed property
        // (see the shim block near Instance) - "current" species, not a
        // standalone field.

        struct DinoSpeciesDef
        {
            public DebugDinoKind kind;
            public int typeNumber;          // 1..4 (user-facing)
            public string prefix;           // manifest clip family
            public string label;
            public float hp, speed, attack, adisBase;
            public string xrayRig;          // "A" (k3gt*) | "B" (k2gt*) | "none"
            public string notes;
        }

        // TASK027N.1.1 DEFECT B - stats recovered from the correct source files.
        // Type 1/2 = City iPad dinosaur.prop rows 1/2 (via the accepted
        // DinoCapSourceProfileV11 constants - do NOT regress).
        // Type 3   = City iPad dinosaur.prop row 3 (Task024A): HP25 SPD46 ATK13
        //            ADIS280 (+ Random[0,20) jitter at spawn; NO level scaling).
        //            V30.26 wrongly used the PHONE prop (ADIS130 SPD30).
        // Type 4   = Jungle Blitz iPad lifedinosaur.prop <Dinosaur type="4">,
        //            intervals 0/30/60 (Task024B/C): HP5 SPD50 ATK10 ADIS190.
        //            This is the "as first encountered" row; later intervals buff
        //            it (that is run-elapsed scaling, out of sandbox scope).
        //            V30.26 wrongly labelled it SANDBOX_ESTIMATE - it is RECOVERED.
        static readonly DinoSpeciesDef[] DinoSpecies =
        {
            new DinoSpeciesDef{ kind=DebugDinoKind.K3Blue,   typeNumber=1, prefix="k3",
                label="T1 K3 BLUE",
                hp=DinoCapSourceProfileV11.Type1HP, speed=DinoCapSourceProfileV11.Type1Speed,
                attack=DinoCapSourceProfileV11.Type1Attack, adisBase=DinoCapSourceProfileV11.Type1AdisBase,
                xrayRig="A", notes="City iPad dinosaur.prop row 1. Full weapon-reaction coverage." },
            new DinoSpeciesDef{ kind=DebugDinoKind.K1Rusher,  typeNumber=2, prefix="k1",
                label="T2 K1 RUSHER",
                hp=DinoCapSourceProfileV11.RusherHP, speed=DinoCapSourceProfileV11.RusherSpeed,
                attack=DinoCapSourceProfileV11.RusherAttack, adisBase=DinoCapSourceProfileV11.RusherAdisBase,
                xrayRig="A", notes="City iPad dinosaur.prop row 2 (k1 clips -> k4 art). Full coverage." },
            new DinoSpeciesDef{ kind=DebugDinoKind.K2Trex,    typeNumber=3, prefix="k2",
                label="T3 K2 TANK",
                hp=25f, speed=46f, attack=13f, adisBase=280f,
                xrayRig="B", notes="City iPad dinosaur.prop row 3 (Task024A): HP25 SPD46 ATK13 ADIS280. Melee flinch = shared sub_10554 case-6 BehitedAnim path (Task023). X-ray = k2gt* rig B." },
            new DinoSpeciesDef{ kind=DebugDinoKind.K4Raptor,  typeNumber=4, prefix="k4",
                label="T4 K4 RAPTOR",
                hp=5f, speed=50f, attack=10f, adisBase=190f,
                xrayRig="none", notes="Jungle Blitz iPad lifedinosaur.prop type 4, intervals 0-60 (Task024B/C): HP5 SPD50 ATK10 ADIS190. Melee reaction = shared sub_10554 case-6 (dino-type-agnostic). No gt X-ray skeleton -> X-ray phase shows the living body (future art task)." },
        };

        DinoSpeciesDef CurrentSpecies()
        {
            for(int i=0;i<DinoSpecies.Length;i++)
                if(DinoSpecies[i].kind==debugDinoKind) return DinoSpecies[i];
            return DinoSpecies[0];
        }

        // TASK027N.1 manual + auto spawn debug state (DEBUG_ONLY).
        internal enum SpawnSide { Left, Right }   // TASK027N.2.35: see DebugDinoKind's own comment.
        SpawnSide spawnSide = SpawnSide.Right;
        int manualTypeIndex = 0;                 // 0..3 -> DinoSpecies[]
        bool autoSpawn = false;                  // Part H - default OFF
        enum AutoTypeMode { Cycle, Random, Current }
        enum AutoSideMode { Alternate, Random, Current }
        AutoTypeMode autoTypeMode = AutoTypeMode.Cycle;
        AutoSideMode autoSideMode = AutoSideMode.Alternate;
        float autoIntervalSeconds = 2f;          // Part H - debug-safe default
        float nextAutoSpawnTime = 0f;
        int autoTypeCursor = 0;
        int autoSideCursor = 0;                  // 0 = Left, 1 = Right
        const int MaxActiveDinosDefault = 8;     // Part L - DEBUG safety cap
        int maxActiveDinos = MaxActiveDinosDefault;
        bool autoPausedByCap = false;
        int lastSpawnedType = 0;
        SpawnSide lastSpawnedSide = SpawnSide.Right;
        // TASK027N.2.35: ActiveDinoCount and the per-dino state fields are
        // now defined once, near Instance, as the authoritative collection
        // (see the DinoInstance / dinos / current block above). This task's
        // own former single-target singletons (enemyClipPrefix etc.) are
        // DinoInstance fields now, reached through the same-named property
        // shims declared there.
        string status = "Ready";

        // DEBUG_ONLY Task027K4.2 telemetry state.
        bool debugLastFacingRight;
        bool debugFacingInitialized;
        bool debugLastMoving;

        void Awake() { Instance = this; }

        void Start()
        {
            // Global persisted debug-panel hidden/shown state (shared with every scene).
            debugUiHidden = DC1DebugSettings.UiHidden;
            // TASK027N.2.27 Part B: console startup log, sourced from the
            // same central PortVersion/PortBuildTag constants as the
            // on-screen banner - lets a stale/non-recompiled Play session be
            // caught immediately from the Console window alone.
            Debug.Log("[DC1-PORT] Dino Cap Port "+PortVersion+" / Build "+PortBuildTag+" starting.");

            try
            {
                data = SourceTruthLoader.Load();
                DinoCapSourceProfileV11.ValidateManifest(data);
                BuildWorld();
            }
            catch (Exception e)
            {
                Debug.LogError("[DC1-V11] " + e);
                status = e.Message;
            }
        }

        void BuildWorld()
        {
            Camera c = Camera.main;
            if (c == null)
            {
                GameObject cg = new GameObject("Main Camera");
                c = cg.AddComponent<Camera>();
                cg.tag = "MainCamera";
            }

            c.orthographic = true;
            c.orthographicSize = DinoCapSourceProfileV11.ViewHeight * 0.5f;
            c.transform.position = new Vector3(0, 0, -10);
            c.backgroundColor = new Color(.12f, .14f, .16f);

            // Task025C: reproduce the shipped iPad rasterisation layer.
            // Gameplay/world sprites render into a fixed-height logical framebuffer
            // at exactly one pixel per source world unit, with NO MSAA. Physical
            // display scaling happens only after the world image is complete.
            GameplayRenderTarget sourceRT = c.GetComponent<GameplayRenderTarget>();
            if (sourceRT == null)
                sourceRT = c.gameObject.AddComponent<GameplayRenderTarget>();
            sourceRT.Initialize(c,
                Mathf.RoundToInt(DinoCapSourceProfileV11.ViewWidth),
                Mathf.RoundToInt(DinoCapSourceProfileV11.ViewHeight));

            // Task025E: keep the world camera on the fixed source RT, but
            // present that finished RT through a real second camera targeting
            // Display 1. This retires the temporary IMGUI-only presentation.
            GameplayPresenter oldPresenter = c.GetComponent<GameplayPresenter>();
            if (oldPresenter != null)
                oldPresenter.enabled = false;

            GameplayPresentationCamera.CreateOrInitialize(sourceRT, c);

            player = new GameObject("Player_SourceTruth").transform;
            player.position = new Vector3(0f, 0f, 0f);
            playerVisual = player.gameObject.AddComponent<Power2DSourceTruthRenderer>();
            playerVisual.Initialize(data);
            playerVisual.EnableActorComposition();
            playerVisual.primary.Play(playerVisual.FindClip("p_stand_"+CurrentWeaponName()), 0);
            playerVisual.secondary.EnterFrame += OnPlayerSecondaryFrame;
            playerVisual.primary.EnterFrame += OnPlayerPrimaryFrame;

            // Platform adapter only: the shipped native contract is one-shot,
            // non-spatial G attack audio triggered by the melee frame-10 event.
            // This sandbox scene did not previously need an audio pipeline.
            // Unity requires at least one enabled AudioListener or every
            // AudioSource is inaudible even when PlayOneShot succeeds.
            if(UnityEngine.Object.FindFirstObjectByType<AudioListener>()==null)
            {
                Camera audioCamera=Camera.main;
                if(audioCamera!=null)
                    audioCamera.gameObject.AddComponent<AudioListener>();
                else
                    Debug.LogError("[DC1-V26.1] No Camera.main available for AudioListener.");
            }

            meleeAudioSource = player.gameObject.AddComponent<AudioSource>();
            meleeAudioSource.playOnAwake = false;
            meleeAudioSource.loop = false;
            meleeAudioSource.spatialBlend = 0f;
            meleeAudioSource.volume = 1f;
            meleeAudioSource.mute = false;
            meleeAttackGClip = Resources.Load<AudioClip>("Audio/Attack_G");
            playerHurtClip = Resources.Load<AudioClip>("Audio/Audio_Role");
            meleeAttackG4Clip = Resources.Load<AudioClip>("Audio/Audio_G4");
            meleeContactAtt1Clip = Resources.Load<AudioClip>("Audio/Audio_att1");
            meleeContactAtt2Clip = Resources.Load<AudioClip>("Audio/Audio_att2");
            deathAudio1Clip = Resources.Load<AudioClip>("Audio/Audio_death1");
            deathAudio2Clip = Resources.Load<AudioClip>("Audio/Audio_death2");
            deathAudio3Clip = Resources.Load<AudioClip>("Audio/Audio_death3");
            attackF1Clip = Resources.Load<AudioClip>("Audio/Attack_F1");
            attackF2Clip = Resources.Load<AudioClip>("Audio/Attack_F2");
            burnSizzleClip = Resources.Load<AudioClip>("Audio/Audio_att3");

            attackF2Source=player.gameObject.AddComponent<AudioSource>();
            attackF2Source.playOnAwake=false;
            attackF2Source.loop=false;
            attackF2Source.pitch=1f;
            attackF2Source.spatialBlend=0f;
            attackF2Source.volume=1f;
            attackF2Source.clip=attackF2Clip;

            burnSizzleSource=player.gameObject.AddComponent<AudioSource>();
            burnSizzleSource.playOnAwake=false;
            burnSizzleSource.loop=false;
            burnSizzleSource.pitch=1f;
            burnSizzleSource.spatialBlend=0f;
            burnSizzleSource.volume=1f;
            burnSizzleSource.clip=burnSizzleClip;

            // Task027M.2: F3 laser weapon audio. Native sub_67CE8 issues
            // sub_7EB04("Attack_F3Audio",0,0) once per ~0.4 s discharge cycle -
            // the SAME request helper as Attack_F2, different WAV, and NO
            // dino-side audio. Fresh overlapping voices via NativeAudioSessionGate.
            attackF3Clip = Resources.Load<AudioClip>("Audio/Attack_F3");
            attackF3Source=player.gameObject.AddComponent<AudioSource>();
            attackF3Source.playOnAwake=false;
            attackF3Source.loop=false;
            attackF3Source.pitch=1f;
            attackF3Source.spatialBlend=0f;
            attackF3Source.volume=1f;
            attackF3Source.clip=attackF3Clip;

            // TASK027N.2.22 Part I/K: A-D fire audio - PREVIOUSLY MISSING
            // ENTIRELY (zero audio calls existed anywhere in
            // DischargeCurrentWeapon or OnPlayerSecondaryFrame for these
            // families). Native's sub_66DCC (A-D's fire/audio dispatcher,
            // switch on the SAME PROVEN weaponClass 0/1/2/3 A/B/C/D values)
            // calls sub_7EB04 unconditionally on EVERY shot (hit or miss,
            // fired BEFORE the hit-test/damage), one string per family:
            // case0(A)="Attack_AAudio", case1(B)="Attack_BAudio",
            // case3(D)="Attack_CAudio", case2(C)="Attack_DAudio" (primary
            // branch, gated on an unidentified global counter dword_21BE0C+8
            // - a secondary, UNRESOLVED branch of case2 instead plays
            // "Attack_E1Audio" under a narrower condition on that same
            // counter that this task's bounded budget did not fully trace;
            // see abcd_audio_native_matrix_n222.json). PROVEN, SHIPPED
            // QUIRK preserved exactly (CLAUDE.md 4.11, not normalized): C
            // and D's own fire sounds are SWAPPED relative to their own
            // family letter - C plays the audio content of Attack_D.wav,
            // D plays the audio content of Attack_C.wav. No native hit-
            // sound or empty/no-ammo sound was found for A-D anywhere in
            // sub_66DCC or the shared sub_668FC target/damage scan
            // (A-D's own non-lethal reaction body was already exhaustively
            // enumerated in N.2.14 as exactly 5 non-audio calls) - one
            // fire-and-forget shot sound is native's complete A-D audio
            // contract.
            attackAClip = Resources.Load<AudioClip>("Audio/Attack_A");
            attackBClip = Resources.Load<AudioClip>("Audio/Attack_B");
            attackCFamilyClip = Resources.Load<AudioClip>("Audio/Attack_D"); // SWAP, see above
            attackDFamilyClip = Resources.Load<AudioClip>("Audio/Attack_C"); // SWAP, see above
            // N253 supersedes the old unresolved header:21BE0C+8 is the
            // owned C tier;66DCC usesAttack_E1 at2/3,67458 usesAttack_E2 for E.
            attackE1Clip = Resources.Load<AudioClip>("Audio/Attack_E1"); //66DCC C tier2/3.
            attackE2Clip = Resources.Load<AudioClip>("Audio/Attack_E2"); //67458 both E tiers.
            abcdFireSource=player.gameObject.AddComponent<AudioSource>();
            abcdFireSource.playOnAwake=false;
            abcdFireSource.loop=false;
            abcdFireSource.spatialBlend=0f;
            abcdFireSource.volume=1f;
            if(attackAClip==null || attackBClip==null ||
               attackCFamilyClip==null || attackDFamilyClip==null)
                Debug.LogError("[DC1-N222] Missing A-D Attack_*Audio Resources.");

            if(meleeAttackGClip==null || meleeAttackG4Clip==null ||
               deathAudio1Clip==null || deathAudio2Clip==null || attackF1Clip==null)
                Debug.LogError("[DC1-V29] Missing existing G/F1/death audio Resources.");
            if(meleeContactAtt1Clip==null || meleeContactAtt2Clip==null)
                Debug.LogError("[DC1-N2] Missing Audio_att1/Audio_att2 G-tier contact SFX Resources.");
            if(attackF2Clip==null || burnSizzleClip==null)
                Debug.LogWarning("[DC1-V29] F2 WAVs missing; run copy_f2_audio_v29.ps1.");

            // TASK027N.2.35: the single pre-created "CityDino_SourceTruth"
            // GameObject is gone - SpawnDino() (via ResetDebugDino below) now
            // creates the FIRST DinoInstance itself, exactly the same way it
            // creates every subsequent one, so there is only one construction
            // path for the whole file instead of one for "the first dino"
            // and a different one for "every dino after that".
            playerHP = data.combat.playerHP;
            // Debug sandbox ownership: A-E first authored tiers plus F1
            // are available. Task027K source cap for F is 250.
            for(int i=0;i<6;i++)
                familyAmmo[i]=WeaponFamilyTable.AmmoCaps[i];

            // Task026B: BLUE Type-1/k3 is the default primary debug enemy.
            ResetDebugDino(DebugDinoKind.K3Blue);

            InitializeSourceCamera();
        }

        void Update()
        {
            if (data == null)
                return;

            bluePressedAfterMeleeClipEndedThisFrame=false;
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            dbgBluePressEdgeThisFrame=false;
            dbgBlueBufferedStartThisFrame=false;
#endif
            PollKeyboard();
            AutoSpawnTick();   // TASK027N.1 DEBUG_ONLY auto spawner
            AutoSpawn123Tick();   // TASK027N.2.31 Part B DEBUG_ONLY auto spawner (Types 1/2/3 only)
            CityGrindSpawnerTick();   // TASK027N.2.35 Part P/Q test-mode spawner
            AutoMeleeBiteHarnessTick();   // TASK027N.2.22 Part E DEBUG_ONLY
            UpdateSustainedFaceReset();
            UpdateWeaponFireInput();
            UpdatePlayerInvulnerabilityVisual();
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            TickDebugK4Projectiles(Time.deltaTime);
#endif

            if (playerDead)
            {
                left = right = false;
                UpdateSourceCamera();
                return;
            }

            if(playerMelee)
            {
                left=right=false;

                if(!playerVisual.primary.Active)
                {
                    playerMelee=false;
                    playerVisual.primary.Play(
                        playerVisual.FindClip(
                            "p_stand_"+CurrentWeaponName()),0);

                    // Task027L.1 native ordering closure:
                    // native installs stand on completion, but a real Blue
                    // touch delivered after completion can synchronously claim
                    // melee again before that stand frame is rendered.
                    //
                    // Crucially, PollKeyboard only sets this flag if the melee
                    // clip was ALREADY inactive when the Blue edge was seen.
                    // A Blue press while the clip is still executing remains
                    // rejected and is not queued.
                    if(bluePressedAfterMeleeClipEndedThisFrame)
                    {
                        bluePressedAfterMeleeClipEndedThisFrame=false;
                        Melee();
                    }

                    // Task027L.2 native case-3/LABEL_13 contract:
                    // a semi-auto press that began during melee fires exactly
                    // once at melee completion iff Orange is still held and a
                    // fresh Blue did not just reclaim melee. Blue wins.
                    if(semiAutoPressBlockedByMelee)
                    {
                        bool blueReclaimedThisFrame=playerMelee;

                        // Task027L.2 V30.6A.1:
                        // Native does NOT retain a second queued-press object
                        // through melee. The surviving source truth is the live
                        // Orange-held mirror. semiAutoPressBlockedByMelee already
                        // proves that a semi-auto DOWN began while melee owned
                        // the action, so requiring firePressQueued here adds a
                        // duplicate Unity-only gate that can suppress the shot.
                        if(!blueReclaimedThisFrame && fireHeld)
                        {
                            firePressQueued=false;
                            semiAutoPressBlockedByMelee=false;
                            BeginFireRequest();
                        }
                        else
                        {
                            firePressQueued=false;
                            semiAutoPressBlockedByMelee=false;
                        }
                    }

                    // Native sub_66550 case-3/LABEL_13 performs one discrete
                    // Orange-held check at melee completion. If F2 was
                    // interrupted and Orange is still physically held, re-enter
                    // through the only legitimate start point: frame 0.
                    if(f2RestartInitialAfterMelee)
                    {
                        f2RestartInitialAfterMelee=false;
                        bool f2Mode=
                            equippedFamily==5 && familyTier[5]==1;

                        if(!playerMelee &&
                           fireHeld &&
                           f2Mode &&
                           familyAmmo[5]>0)
                        {
                            PlayF2InitialFire();
                            F2DebugTelemetry.Log(
                                "POST_MELEE_DISCRETE_FRAME0_START",
                                F2DebugStateSummary());
                        }
                    }
                }

                UpdateSourceCamera();
                TickAllDinos();
                return;
            }

            if (playerHitStun)
            {
                // Source: constant +/-100 velocity until behitAnim's velocity-zero
                // event. That exact event index remains bounded; V18 conservatively
                // stops on primary completion rather than inventing a frame number.
                player.position +=
                    Vector3.right*(playerHurtVelocityX*Time.deltaTime);

                if (!playerVisual.primary.Active)
                {
                    playerHurtVelocityX=0f;
                    playerHitStun=false;
                    playerVisual.primary.Play(
                        playerVisual.FindClip("p_stand_"+CurrentWeaponName()),0);

                    // TASK027N.1.3.2 SOURCE_FAITHFUL: native sub_66550 case 5
                    // (player state == HURT) unconditionally re-checks the
                    // CONTINUOUS Blue-held flag (byte_21BE74) at this exact
                    // hurt-completion boundary and immediately starts melee
                    // (sub_666BC, which itself requires state<=2 - i.e. NOT
                    // still MELEE/HURT - and clears byte_21BE74 so a sustained
                    // hold cannot auto-chain a second swing) if the button is
                    // STILL physically down at that instant. This is a LEVEL
                    // recheck, not a remembered/buffered press: a tap released
                    // before recovery does NOT auto-fire (blueHeld is false by
                    // the time this runs), matching native exactly.
                    //
                    // Unity's F-key/on-screen-button paths (PollKeyboard,
                    // OnGUI's "BLUE\nMelee") are edge-only
                    // (wasPressedThisFrame / GUI.Button's click return), with
                    // no held-state re-check anywhere - so a press OR a
                    // sustained hold that began during hitstun was silently
                    // and permanently lost (Task027N.1.3.2 Part E, PROVEN).
                    // This is the single missing check that restores native's
                    // contract; nothing here reads invulnUntil, and this runs
                    // exactly once per hitstun-clear event (not every frame),
                    // so it cannot chain multiple swings from one hold.
                    if (blueHeld)
                    {
                        DebugRecordBlueBufferedStart();
                        Melee();
                    }
                }

                left=right=false;
                UpdateSourceCamera();
                TickAllDinos();
                return;
            }

            float dx = (right ? 1 : 0) - (left ? 1 : 0);

            if (dx != 0)
            {
                playerVisual.facingRight = dx > 0;
                player.position += Vector3.right * (dx * data.combat.playerSpeed * Time.deltaTime);

                string n = "p_move_" + CurrentWeaponName();
                if (playerVisual.primary.Clip == null || playerVisual.primary.Clip.name != n)
                    playerVisual.primary.Play(playerVisual.FindClip(n), 0);
            }
            else
            {
                string n = "p_stand_" + CurrentWeaponName();
                if (playerVisual.primary.Clip == null || playerVisual.primary.Clip.name != n)
                    playerVisual.primary.Play(playerVisual.FindClip(n), 0);
            }

            left = right = false;
            UpdateSourceCamera();
            TickAllDinos();
        }

        // TASK027N.2.35 Part E/F/G/H: per-instance Update dispatch. Loops a
        // SNAPSHOT of the collection (TickEnemy/BeginF2BurnDeath/etc. can
        // themselves cause a removal-eligible state change; mutating `dinos`
        // while enumerating it directly would throw), swapping `current` to
        // each dino in turn so its own TickEnemy() call - completely
        // unmodified from the pre-migration single-dino version - reads/
        // writes only that instance's own state. Removal is DEFERRED: any
        // instance TickEnemy() marks via RemoveCurrentDino() is torn down
        // only after every dino has had its own tick this frame, so one
        // dino's death/offscreen-cleanup can never skip or corrupt another
        // dino still due to be ticked in the same pass (Part H).
        void TickAllDinos()
        {
            var prevCurrent=current;
            foreach(var d in dinos.ToArray())
            {
                if(d.markedForRemoval) continue;
                current=d;
                TickEnemy();
            }

            for(int i=dinos.Count-1;i>=0;i--)
            {
                if(!dinos[i].markedForRemoval) continue;
                DestroyDinoInstance(dinos[i]);
                dinos.RemoveAt(i);
            }

            current = (prevCurrent!=null && !prevCurrent.markedForRemoval)
                ? prevCurrent
                : PrimaryDino;
        }

        // TASK027N.2.35 Part H/I/K: tears down one instance's own effect/
        // visual state and clears any dangling cross-references (F2/F3
        // target, `current`) before its GameObject is destroyed - so one
        // dino's cleanup can never leave another dino's effect ownership (or
        // the shared player-anchored F2/F3 visuals) pointing at a stale/
        // destroyed object (Part N).
        void DestroyDinoInstance(DinoInstance d)
        {
            if(d.f2BigFireVisual!=null) d.f2BigFireVisual.StopNow();
            if(f2Target==d) { if(f2FlameVisual!=null) f2FlameVisual.EndHeld(false); f2Target=null; }
            if(f3Target==d) { if(f3LaserVisual!=null) f3LaserVisual.EndHeld(false); f3Target=null; }
            if(current==d) current=null;
            if(d.transform!=null && d.transform.gameObject!=null)
                UnityEngine.Object.Destroy(d.transform.gameObject);
        }

        void PollKeyboard()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            // TASK027N.1.3.2 Part B fallback: a genuine Unity Input System
            // event injection attempt (InputSystem.QueueStateEvent on an
            // AddDevice<Keyboard>()'d device) proved unreliable in this
            // project's headless batchmode PlayMode-test environment across
            // several independent attempts - the queued state never actually
            // reached Keyboard.current.fKey (see report Part B/M). This is
            // the task's own explicitly-sanctioned fallback ("a thin test
            // hook that sets the exact same raw button state consumed by
            // Update()... must NOT invoke the resulting gameplay action
            // directly"). It replaces ONLY the two raw reads below; every
            // downstream decision (blueHeld, the press-edge branch, and
            // whether Melee() is called) is the SAME unmodified production
            // code that runs for a real key. Null = "use real input" (zero
            // effect outside a test that explicitly sets it).
            bool blueIsPressedRaw = TestOverrideBlueIsPressed ?? (keyboard != null && keyboard.fKey.isPressed);
            bool blueEdgeRaw = TestOverrideBlueWasPressedThisFrame ?? (keyboard != null && keyboard.fKey.wasPressedThisFrame);
#else
            bool blueIsPressedRaw = keyboard != null && keyboard.fKey.isPressed;
            bool blueEdgeRaw = keyboard != null && keyboard.fKey.wasPressedThisFrame;
#endif
            blueHeld = blueIsPressedRaw || guiBlueHeld;
            if (blueEdgeRaw)
            {
                DebugRecordBluePressEdge();
                bluePressedAfterMeleeClipEndedThisFrame=
                    playerMelee && !playerVisual.primary.Active;
                Melee();
            }

            if (keyboard == null)
                return;

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            if(DC1DebugSettings.EffectiveEnabled && keyboard.f1Key.wasPressedThisFrame) SetDebugUiHidden(!debugUiHidden);
#endif

            left = left || keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed;
            right = right || keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed;

            if(keyboard.spaceKey.wasPressedThisFrame)
                firePressQueued=true;
            fireHeld=keyboard.spaceKey.isPressed || guiFireHeld;
            if (keyboard.tabKey.wasPressedThisFrame) SwitchWeapon();
            if(DC1DebugSettings.EffectiveEnabled && keyboard.gKey.wasPressedThisFrame) CycleMeleeTier();
            if(DC1DebugSettings.EffectiveEnabled && keyboard.tKey.wasPressedThisFrame) CycleFMode();
            if(DC1DebugSettings.EffectiveEnabled && keyboard.hKey.wasPressedThisFrame) SetDebugEnemyHPFromText();
            if(DC1DebugSettings.EffectiveEnabled && keyboard.pKey.wasPressedThisFrame) GameplayPresentationCamera.CycleGlobalMode();
            if(DC1DebugSettings.EffectiveEnabled && keyboard.iKey.wasPressedThisFrame) GameplayRenderTarget.TogglePersistentSupersample();
            if(DC1DebugSettings.EffectiveEnabled && keyboard.digit1Key.wasPressedThisFrame) { manualTypeIndex=0; SpawnDino(DebugDinoKind.K3Blue,spawnSide); }
            if(DC1DebugSettings.EffectiveEnabled && keyboard.digit2Key.wasPressedThisFrame) { manualTypeIndex=1; SpawnDino(DebugDinoKind.K1Rusher,spawnSide); }
            if(DC1DebugSettings.EffectiveEnabled && keyboard.digit3Key.wasPressedThisFrame) { manualTypeIndex=2; SpawnDino(DebugDinoKind.K2Trex,spawnSide); }
            if(DC1DebugSettings.EffectiveEnabled && keyboard.digit4Key.wasPressedThisFrame) { manualTypeIndex=3; SpawnDino(DebugDinoKind.K4Raptor,spawnSide); }
            if(DC1DebugSettings.EffectiveEnabled && keyboard.rKey.wasPressedThisFrame) ResetCurrentDebugDino();
            // TASK027N.2.35 Part C: lifecycle-safe CLEAR DINOS hotkey.
            if(DC1DebugSettings.EffectiveEnabled && keyboard.cKey.wasPressedThisFrame) ClearDinos();
            if(DC1DebugSettings.EffectiveEnabled && keyboard.oKey.wasPressedThisFrame) godMode=!godMode;
            if(DC1DebugSettings.EffectiveEnabled && keyboard.lKey.wasPressedThisFrame) showHPOverlay=!showHPOverlay;
            if(DC1DebugSettings.EffectiveEnabled && keyboard.bKey.wasPressedThisFrame)
            {
                bloodDebugSlowmo=!bloodDebugSlowmo;
                DinoCapDamageParticle.DebugTimeScale=bloodDebugSlowmo?0.25f:1f;
            }
            // TASK027N.2.22 Part F: auto-melee bite harness hotkeys.
            if(DC1DebugSettings.EffectiveEnabled && keyboard.mKey.wasPressedThisFrame)
                autoMeleeBiteTestEnabled=!autoMeleeBiteTestEnabled;
            if(DC1DebugSettings.EffectiveEnabled && keyboard.nKey.wasPressedThisFrame)
                autoMeleeMode=autoMeleeMode==AutoMeleeMode.FixedOffset
                    ? AutoMeleeMode.AutoSweep : AutoMeleeMode.FixedOffset;
            if(DC1DebugSettings.EffectiveEnabled && keyboard.commaKey.wasPressedThisFrame)
                autoMeleeFixedOffset--;
            if(DC1DebugSettings.EffectiveEnabled && keyboard.periodKey.wasPressedThisFrame)
                autoMeleeFixedOffset++;
            // TASK027N.2.31: Infinite Ammo / Auto Spawn 1-2-3 hotkeys.
            if(DC1DebugSettings.EffectiveEnabled && keyboard.kKey.wasPressedThisFrame)
                infiniteAmmo=!infiniteAmmo;
            if(DC1DebugSettings.EffectiveEnabled && keyboard.jKey.wasPressedThisFrame)
                autoSpawn123=!autoSpawn123;
#else
            left = left || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
            right = right || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

            if(Input.GetKeyDown(KeyCode.Space))
                firePressQueued=true;
            fireHeld=Input.GetKey(KeyCode.Space) || guiFireHeld;
            blueHeld=Input.GetKey(KeyCode.F) || guiBlueHeld;
            if (Input.GetKeyDown(KeyCode.Tab)) SwitchWeapon();
            if (Input.GetKeyDown(KeyCode.F))
            {
                DebugRecordBluePressEdge();
                bluePressedAfterMeleeClipEndedThisFrame=
                    playerMelee && !playerVisual.primary.Active;
                Melee();
            }
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.G)) CycleMeleeTier();
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.T)) CycleFMode();
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.H)) SetDebugEnemyHPFromText();
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.P)) GameplayPresentationCamera.CycleGlobalMode();
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.I)) GameplayRenderTarget.TogglePersistentSupersample();
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.F1)) SetDebugUiHidden(!debugUiHidden);
#endif
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.Alpha1)) { manualTypeIndex=0; SpawnDino(DebugDinoKind.K3Blue,spawnSide); }
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.Alpha2)) { manualTypeIndex=1; SpawnDino(DebugDinoKind.K1Rusher,spawnSide); }
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.Alpha3)) { manualTypeIndex=2; SpawnDino(DebugDinoKind.K2Trex,spawnSide); }
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.Alpha4)) { manualTypeIndex=3; SpawnDino(DebugDinoKind.K4Raptor,spawnSide); }
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.R)) ResetCurrentDebugDino();
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.C)) ClearDinos();
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.O)) godMode=!godMode;
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.L)) showHPOverlay=!showHPOverlay;
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.B))
            {
                bloodDebugSlowmo=!bloodDebugSlowmo;
                DinoCapDamageParticle.DebugTimeScale=bloodDebugSlowmo?0.25f:1f;
            }
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.M))
                autoMeleeBiteTestEnabled=!autoMeleeBiteTestEnabled;
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.N))
                autoMeleeMode=autoMeleeMode==AutoMeleeMode.FixedOffset
                    ? AutoMeleeMode.AutoSweep : AutoMeleeMode.FixedOffset;
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.Comma))
                autoMeleeFixedOffset--;
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.Period))
                autoMeleeFixedOffset++;
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.K))
                infiniteAmmo=!infiniteAmmo;
            if(DC1DebugSettings.EffectiveEnabled && Input.GetKeyDown(KeyCode.J))
                autoSpawn123=!autoSpawn123;
#endif
        }

        void InitializeSourceCamera()
        {
            Camera c = Camera.main;
            if (c == null || player == null)
                return;

            bool facingRight = playerVisual.facingRight;
            float k = facingRight
                ? DinoCapSourceProfileV11.CameraKRight
                : DinoCapSourceProfileV11.CameraKLeft;

            cameraLeftX = player.position.x - DinoCapSourceProfileV11.ViewWidth * k;
            previousPlayerFacingRight = facingRight;
            cameraCatchupActive = false;
            cameraInitialized = true;
            WriteSourceCameraTransform(c);
        }

        void UpdateSourceCamera()
        {
            Camera c = Camera.main;
            if (c == null || player == null)
                return;

            if (!cameraInitialized)
            {
                InitializeSourceCamera();
                return;
            }

            bool facingRight = playerVisual.facingRight;
            if (facingRight != previousPlayerFacingRight)
                cameraCatchupActive = true;
            previousPlayerFacingRight = facingRight;

            float k = facingRight
                ? DinoCapSourceProfileV11.CameraKRight
                : DinoCapSourceProfileV11.CameraKLeft;
            float targetLeftX = player.position.x - DinoCapSourceProfileV11.ViewWidth * k;

            if (cameraCatchupActive)
            {
                float step = DinoCapSourceProfileV11.CameraPanSpeed * Time.deltaTime;
                if (facingRight)
                    cameraLeftX += step;
                else
                    cameraLeftX -= step;

                if (facingRight)
                {
                    if (cameraLeftX - targetLeftX > 0f)
                        cameraCatchupActive = false;
                }
                else
                {
                    if (cameraLeftX - targetLeftX < 0f)
                        cameraCatchupActive = false;
                }
            }
            else
            {
                cameraLeftX = targetLeftX;
            }

            WriteSourceCameraTransform(c);
        }

        void WriteSourceCameraTransform(Camera c)
        {
            float centreX = cameraLeftX + DinoCapSourceProfileV11.ViewWidth * 0.5f;
            c.transform.position = new Vector3(centreX, c.transform.position.y, -10f);
        }

        void TickEnemy()
        {
            if(enemyF2BurnDeath)
            {
                if(!enemyVisual.primary.Active)
                {
                    // TASK027N.2.35 Part G/H: native removal is death-clip-
                    // completion owned, not a visibility toggle - mark for
                    // the deferred collection-safe removal TickAllDinos()
                    // performs after every dino has had its own tick.
                    enemyF2BurnDeath=false;
                    if(f2BigFireVisual!=null) f2BigFireVisual.StopNow();
                    current.markedForRemoval=DebugCorpseSettings.DespawnEnabled;
                }
                return;
            }

            if(enemyBurningF2)
            {
                UpdateF2BurningEnemy();
                return;
            }

            // Task027M.2: F3 laser reaction (native dino state 4). Mutually
            // exclusive with the F2 states above.
            if(enemyF3Death)
            {
                if(!enemyVisual.primary.Active)
                {
                    // TASK027N.2.35 Part G/H: see enemyF2BurnDeath above.
                    enemyF3Death=false;
                    if(f3LaserVisual!=null) f3LaserVisual.EndHeld(false);
                    current.markedForRemoval=DebugCorpseSettings.DespawnEnabled;
                }
                return;
            }

            if(enemyF3State)
            {
                UpdateF3StateEnemy();
                return;
            }

            if(enemyF3Recover)
            {
                // Task027M.8.2 DEFECT A: native reaction-state 6. AI stays fully
                // suppressed (velocity 0, no chase/attack/facing) until the k*_be
                // frames 4->7 tail completes -> equivalent of state 6 -> 8 ->
                // sub_FD58 resume.12C64 enters recovery directly; no minimum
                // reaction-duration gate is present in the native owner.
                enemyVelocityX=0f;



                if(!enemyVisual.primary.Active)
                {
                    enemyF3Recover=false;
                    enemyVisual.drawOffset=Vector2.zero;
                    enemyVisual.primary.Play(
                        enemyVisual.FindClip(enemyClipPrefix+"_move"),0);
                    ResignEnemyTowardPlayer();
                    ApplyEnemyVisualFacing();
                }
                return;
            }

            if(enemyF2Residual)
            {
                enemyVelocityX=0f;

                if(!enemyVisual.primary.Active)
                {
                    enemyF2Residual=false;
                    enemyVisual.drawOffset=Vector2.zero;
                    enemyVisual.primary.Play(
                        enemyVisual.FindClip(enemyClipPrefix+"_move"),0);
                    ResignEnemyTowardPlayer();
                    ApplyEnemyVisualFacing();
                }
                return;
            }

            if (enemyHP <= 0)
            {
                // TASK027N.2.35 Part G (N.2.34 Part E/G/H PROVEN native
                // contract): the SAME dino object remains as its own corpse
                // and is removed ONLY when its own death-animation clip
                // finishes playing - never a camera/offscreen heuristic and
                // never a global/shared timer. The pre-migration version's
                // camera-cull hide here never actually fired in practice for
                // a stationary corpse (it never left camera bounds), which
                // is WHY dead dinos previously lingered indefinitely; this
                // is the fix, not merely a refactor.
                if(!enemyVisual.primary.Active && DebugCorpseSettings.DespawnEnabled)
                    current.markedForRemoval=true;
                return;
            }

            // TASK027N.2.35 Part F (N.2.34 Part C native contract, sub_FDE0):
            // a LIVING dino that drifts beyond either screen bound is
            // removed - checked for every live sub-state below (chase/
            // attack-windup/heavy-reaction) so it cannot be skipped by,
            // e.g., staying in enemyHeavyReaction forever. F2/F3 reaction
            // states return earlier above and are correctly exempt, matching
            // native's own [3,7]-state exemption from this same check.
            // TASK027N.2.49R Phase 12: the 480f this comment previously
            // cited was N.2.34's own approximation, SUPERSEDED by N.2.39's
            // own full sub_FDE0 decompile (see CityGridController.cs's own
            // DinoOffscreenMargin comment: exact IEEE-754 bit patterns
            // 0xC3FA0000/0x43FA0000 = -500.0f/+500.0f) - this file's own
            // 480f was simply never updated to match. Corrected to the SAME
            // 500f both scenes must share.
            if(!debugSuppressOffscreenRemoval)
            {
                float screenLeft=cameraLeftX;
                float screenRight=cameraLeftX+DinoCapSourceProfileV11.ViewWidth;
                if(enemy.position.x<screenLeft-500f || enemy.position.x>screenRight+500f)
                {
                    current.markedForRemoval=true;
                    return;
                }
            }

            if(enemyHeavyReaction)
            {
                enemy.position += Vector3.right*(enemyVelocityX*Time.deltaTime);

                if(!enemyVisual.primary.Active)
                {
                    enemyHeavyReaction=false;
                    ResignEnemyTowardPlayer();
                    ApplyEnemyVisualFacing();
                    enemyVisual.primary.Play(
                        enemyVisual.FindClip(enemyClipPrefix+"_move"),0);
                }
                return;
            }

            // Native state 1 is animation-driven: no chase/range work until
            // AttackAnim completes. Then sub_101A0 either immediately replays
            // the attack if still in the cone, or resumes chase with zero delay.
            if (enemyAttacking)
            {
                if (!enemyVisual.primary.Active)
                    CompleteEnemyAttack();
                return;
            }

            // Native ordering:
            // 1) test attack cone using CURRENT facing;
            // 2) if not attackable, re-sign travel/facing toward current player side.
            if (IsPlayerInNativeAttackCone())
            {
                BeginEnemyAttack();
                return;
            }

            ResignEnemyTowardPlayer();

            enemy.position += Vector3.right * (enemyVelocityX * Time.deltaTime);
            ApplyEnemyVisualFacing();

            // Continuous native chase presentation: the finite move clip must
            // restart when it completes. Clip remains assigned after Active=false,
            // so checking only Clip.name freezes the last frame while translation
            // continues and produces visible sliding (especially obvious on K3).
            if (!enemyVisual.primary.Active ||
                enemyVisual.primary.Clip == null ||
                enemyVisual.primary.Clip.name != enemyClipPrefix+"_move")
            {
                enemyVisual.primary.Play(
                    enemyVisual.FindClip(enemyClipPrefix+"_move"),0);
            }
        }


        void CompleteEnemyAttack()
        {
            if (IsPlayerInNativeAttackCone())
            {
                enemyHitThisCycle = false;
                enemyVelocityX = 0f;
                enemyVisual.primary.Play(enemyVisual.FindClip(enemyClipPrefix+"_attack"), 0);
                status = "Attack complete while still in ADIS: immediate native re-bite.";
                return;
            }

            enemyAttacking = false;
            enemyHitThisCycle = false;
            ResignEnemyTowardPlayer();
            ApplyEnemyVisualFacing();
            enemyVisual.primary.Play(enemyVisual.FindClip(enemyClipPrefix+"_move"), 0);
            status = "Attack complete out of range: zero-delay native chase resume.";
        }

        void ResignEnemyTowardPlayer()
        {
            float dx = player.position.x - enemy.position.x;

            if (dx > 0f)
                SetEnemyTravelDirection(+1f);
            else if (dx < 0f)
                SetEnemyTravelDirection(-1f);
            // Exact overlap: keep previous facing/sign.
        }

        void SetEnemyTravelDirection(float sign)
        {
            if (sign > 0f)
            {
                enemyVelocityX = enemySourceSpeed;
                enemyFacingRight = true;
            }
            else if (sign < 0f)
            {
                enemyVelocityX = -enemySourceSpeed;
                enemyFacingRight = false;
            }

            ApplyEnemyVisualFacing();
        }

        void ApplyEnemyVisualFacing()
        {
            // Imported k1 art polarity is opposite the renderer's player-facing
            // boolean. Keep this representation correction local.
            enemyVisual.facingRight = !enemyFacingRight;
        }

        bool IsPlayerInNativeAttackCone()
        {
            float dx = player.position.x - enemy.position.x;
            float adis = enemyRuntimeAdis;

            if (enemyFacingRight)
                return dx > 0f && dx < adis;

            return dx < 0f && dx > -adis;
        }

        // TASK027N.1.2: true while the enemy is in the native committed-bite
        // state. Native sub_FEF0 sets the reaction-state word (dino+0xAC) to 1
        // on attack entry and holds the PRIMARY anim slot with "AttackAnim";
        // in the port that is `enemyAttacking` with the *_attack primary still
        // running. In this window sub_10554's generic reaction switch routes
        // every ordinary weapon family (A/B/C/D = default cases, B = case 1,
        // E = case 4, F1-base = case 5, G-melee = case 6) into a branch that is
        // gated out - the light sub-branch by `if(dino+0xAC==0)` and the heavy
        // branch by `if(dino+0xBC!=3)` (dino+0xBC==3 while attack-committed;
        // LIKELY, corroborated by Task023, sub_10294 case-1's `dino+0xBC<=3`
        // bite-damage gate, and the runtime acceptance observation). Only F2
        // (state 5) and F3 (state 4) escape it: sub_10554 case 5 early-returns
        // for their tier and their own reaction-states sit in the [3,7] band
        // that sub_FD58 uses to suspend the whole chase/attack AI.
        bool EnemyBiteCommitted()
        {
            return enemyAttacking
                && enemyVisual != null
                && enemyVisual.primary.Active
                && enemyVisual.primary.Clip != null
                && enemyVisual.primary.Clip.name == enemyClipPrefix + "_attack";
        }

        void BeginEnemyAttack()
        {
            enemyAttacking = true;
            enemyHitThisCycle = false;
            autoMeleeAttemptedThisBite = false;   // TASK027N.2.22 Part E

            // Stop travel but DO NOT change enemyFacingRight.
            enemyVelocityX = 0f;

            enemyVisual.primary.Play(enemyVisual.FindClip(enemyClipPrefix+"_attack"), 0);
        }

        // TASK027N.2.22 Part E: autoparry-style auto-melee bite harness.
        // Detects the CURRENT bite via enemyAttacking/OnEnemyPrimaryFrame
        // (no new bite-detection mechanic - reuses the exact same signal the
        // native frame-12 damage check already uses) and schedules exactly
        // ONE melee attempt per bite, at frame (12 + offset) where 12 is the
        // proven native bite-commit frame (bite_commit_timeline_n222.json).
        // offset<0 fires before commit (should cancel the bite - the damage-
        // to-player event on frame 12 never reaches a clip that has already
        // been swapped to BehitedAnim); offset>=0 fires at/after commit
        // (bite damage already applied; melee still fully staggers the
        // dino). AUTO SWEEP advances through AutoMeleeSweepOffsets by one
        // step per completed bite, wrapping around.
        void AutoMeleeBiteHarnessTick()
        {
            if(!autoMeleePendingFinalize)
                return;

            autoMeleePendingFramesWaited++;
            // 6 frames (~0.1s) is ample for ExecuteMeleeContact's own
            // frame-10 swing-contact window plus ApplyHeavyWeaponHit's
            // synchronous reaction application to have resolved.
            if(autoMeleePendingFramesWaited>=6)
                AutoMeleeFinalizeLog();
        }

        void AutoMeleeScheduleIfDue(int frame)
        {
            if(!autoMeleeBiteTestEnabled || !enemyAttacking || autoMeleeAttemptedThisBite)
                return;

            int offset = autoMeleeMode==AutoMeleeMode.FixedOffset
                ? autoMeleeFixedOffset
                : AutoMeleeSweepOffsets[((autoMeleeSweepIndex%AutoMeleeSweepOffsets.Length)+AutoMeleeSweepOffsets.Length)%AutoMeleeSweepOffsets.Length];
            int targetFrame = BiteCommitFrame + offset;
            if(frame != targetFrame)
                return;

            autoMeleeAttemptedThisBite = true;
            autoMeleePendingIntendedOffset = offset;
            autoMeleePendingIssueFrame = frame;
            autoMeleePendingWasAttackingBeforeIssue = enemyAttacking;
            autoMeleePendingHpBefore = enemyHP;
            autoMeleeAttemptCount++;

            Melee();

            autoMeleePendingFinalize = true;
            autoMeleePendingFramesWaited = 0;
        }

        void AutoMeleeFinalizeLog()
        {
            bool biteDamageOccurred = enemyHitThisCycle;
            bool biteCancelled =
                autoMeleePendingWasAttackingBeforeIssue &&
                autoMeleePendingIntendedOffset < 0 &&
                !biteDamageOccurred;
            bool knockback = enemyHeavyReaction && debugDinoKind!=DebugDinoKind.K2Trex;
            bool stunHurtReaction = enemyHeavyReaction;
            bool damageLanded = enemyHP < autoMeleePendingHpBefore - 0.001f;

            if(autoMeleeMode==AutoMeleeMode.AutoSweep)
                autoMeleeSweepIndex++;

            autoMeleeLastResultSummary =
                "offset="+autoMeleePendingIntendedOffset+
                " issueFrame="+autoMeleePendingIssueFrame+
                " damageLanded="+damageLanded+
                " biteCancelled="+biteCancelled+
                " biteDamageOccurred="+biteDamageOccurred+
                " knockback="+knockback+
                " stunHurtReaction="+stunHurtReaction;

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            Debug.Log("[N222 AutoMeleeBite] species="+debugDinoKind+
                " intendedOffset="+autoMeleePendingIntendedOffset+
                " actualIssueFrame="+autoMeleePendingIssueFrame+
                " (commitFrame="+BiteCommitFrame+")"+
                " DAMAGE_LANDED="+damageLanded+
                " BITE_CANCELLED="+biteCancelled+
                " BITE_DAMAGE_OCCURRED="+biteDamageOccurred+
                " KNOCKBACK="+knockback+
                " STUN_HURT_REACTION="+stunHurtReaction);
#endif
        }

        void OnEnemyPrimaryFrame(Power2DTrack t, int frame)
        {
            if(NativeDinoLandingAudio.IsMarkedDeathFrame(t,frame,enemyHP<=0f,debugDinoKind!=DebugDinoKind.K4Raptor))
                NativeDinoLandingAudio.Play(meleeAudioSource);

            // Task027L.4.6 + Task027L.4.10 source-faithful event-dispatch:
            // k*_shao carries exactly ONE authored event mark on frame 3, and
            // native sub_76A20 dispatches it once per 7-frame loop. With the
            // power2D animation now quantised to the shipped 60 Hz logic tick
            // (Power2DSourceTruthRenderer.LateUpdate), this frame-3 EnterFrame
            // recurs at a regular ~0.23333 s at ANY host refresh rate - proven
            // offline (30/60/90/120/144/240 Hz all give std <= 3 ms).
            //
            // The V30.7 spam, V30.8/V30.9 min-period floor and V30.10 catch-up
            // scheduler were all workarounds for the animation running faster
            // than shipped. Native has exactly ONE timing system for this event
            // (the animation tick), so the port must too: the audio gate is
            // removed. One Audio_att3 request per frame-3 entry;
            // NativeAudioSessionGate still mints an independent overlapping
            // voice per request (28-cap, 2 s GC). lastF2ShaoEventTime is kept
            // for telemetry/diagnostics only - it no longer gates anything.
            if(enemyBurningF2 && frame==3)
            {
                lastF2ShaoEventTime = Time.time;
                enemyHP-=UnityEngine.Random.Range(10f,14f);

                if(enemyHP<=0f)
                {
                    enemyHP=0f;
                    BeginF2BurnDeath();
                    return;
                }

                StartF2Sizzle();
            }

            // Task027M.2: the k*_be authored frame-3 event owns the F3 DoT
            // (native sub_10294 case 4 -> sub_10554). It recurs at exactly
            // 0.1 s while the BEAnim ping-pongs 2..4 (Part B). NO dino-side
            // audio here (contrast F2's StartF2Sizzle / Audio_att3).
            // Task027M.8.4 DEFECT B: the X-ray phase is owned by the COMMITTED
            // F3 reaction presentation - reaction-state 4 (enemyF3State) OR the
            // post-release committed-reaction freeze (enemyF3TailDeferred, still
            // running the k*_be 2..4 ping-pong) - NOT by ShootHeld / beam
            // contact. A brief/light valid hit therefore still reaches the X-ray
            // at the FIRST k*_be frame-3 authored event (~0.10 s after state-4
            // entry, well before 1 s) and keeps alternating through the whole
            // committed reaction even after the beam is gone. No arbitrary delay.
            if(frame==3 && (enemyF3State || enemyF3TailDeferred))
            {
                // DoT + death stay gated on the ACTIVE reaction only (frozen:
                // F3 damage formula + ~0.1 s BE cadence). Native's deferred
                // window is the recovery phase and carries no damage event.
                if(enemyF3State)
                {
                    enemyHP-=F3Damage();
                    if(enemyHP<=0f)
                    {
                        enemyHP=0f;
                        money=Mathf.Min(999999,money+(int)enemySourceReward);
                        BeginF3Death();
                        return;
                    }
                }

                // Alternate the X-ray phase on the k*_be frame-3 reaction event
                // (deterministic, source-backed cadence - NOT a new timer).
                // Mode 0: flip every event. Mode 1: flip every 2nd. When XRAY,
                // Power2DSourceTruthRenderer swaps each body part to its real
                // k*gt* bone sprite. Presentation only.
                f3XrayPhaseCounter++;
                int step = (f3XrayPhaseMode==1) ? 2 : 1;
                if(f3XrayPhaseCounter % step == 0)
                    f3XrayVisualPhase=!f3XrayVisualPhase;
                enemyVisual.xraySkeleton=
                    f3XrayEnabled && (enemyF3State || enemyF3TailDeferred) && f3XrayVisualPhase;
            }

            // Native frame-12 hit re-check uses retained actor facing, not
            // velocity sign. Crossing behind during the bite therefore misses.
            // Native10294 excludes species4 from ordinary bite damage.
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            if(debugDinoKind==DebugDinoKind.K4Raptor && enemyAttacking && frame==10)
                CreateDebugK4Projectile();
#endif
            if (debugDinoKind!=DebugDinoKind.K4Raptor && enemyAttacking &&
                !enemyHitThisCycle &&
                frame == BiteCommitFrame &&
                IsPlayerInNativeAttackCone())
            {
                enemyHitThisCycle = true;
                DamagePlayer(enemySourceAttack);
            }

            AutoMeleeScheduleIfDue(frame);   // TASK027N.2.22 Part E
        }

        // TASK027N.2.22 Part A: the bite's own damage-to-player commit frame
        // (Task024B, frozen/pre-existing - "frame-12 live recheck" - re-cited
        // here as a named constant rather than re-deriving it, and confirmed
        // this task to be a flat literal with NO species conditioning
        // anywhere in OnEnemyPrimaryFrame, i.e. uniform across Type1/2/3/4).
        const int BiteCommitFrame = 12;

        public void HoldLeft(bool on) { if (on) left = true; }
        public void HoldRight(bool on) { if (on) right = true; }

        string CurrentWeaponName()
        {
            int maxTier=equippedFamily==5 ? 2 : 3;
            int tier=Mathf.Clamp(familyTier[equippedFamily],0,maxTier);
            return WeaponFamilyTable.FamilyLetter(equippedFamily)+(tier+1);
        }

        DC1WeaponRow CurrentWeaponRow()
        {
            int maxTier=equippedFamily==5 ? 2 : 3;
            return WeaponFamilyTable.Get(
                equippedFamily,
                Mathf.Clamp(familyTier[equippedFamily],0,maxTier));
        }

        public void Shoot()
        {
            if(playerDead || playerHitStun)
                return;
            firePressQueued=true;
        }

        void UpdateWeaponFireInput()
        {
            DebugSampleF2Transitions();

            // Task027K4.2 runtime telemetry closure:
            // A re-hold can arrive on the same Update in which the secondary
            // clip finishes. Native held-state recovery wins; do not finalize
            // a stale pending release after the held signal is already true.
            if(f2ReleasePending && fireHeld)
            {
                F2DebugTelemetry.Log(
                    "RELEASE_PENDING_CANCEL_REHOLD",
                    F2DebugStateSummary());
                f2ReleasePending=false;
            }
            else if(f2ReleasePending && !playerVisual.secondary.Active)
            {
                CompletePendingF2Release();
            }

            // Task027M.3: F3 deferred-release completion (quick-tap safe).
            if(f3ReleasePending && fireHeld)
                f3ReleasePending=false;
            else if(f3ReleasePending && !playerVisual.secondary.Active)
                CompletePendingF3Release();

            if(playerDead || playerHitStun)
            {
                if(previousFireHeld)
                    EndFireRequest();
                fireHeld=false;
                previousFireHeld=false;
                firePressQueued=false;
                semiAutoPressBlockedByMelee=false;
                cdRestartFireAfterMelee=false;
                return;
            }

            // Task027C/V20.4 + Task027L.1 V30.5:
            // Semi-auto families still use one real press edge. If that edge
            // begins while melee owns the player, remember that it was BLOCKED
            // rather than converting "melee ended" into a fake fire trigger.
            // TASK027N.2.49R Phase 4: classification now delegates to the
            // shared WeaponFireModeCore.IsHeldFullAutoFamily (Combat/
            // WeaponFireModeCore.cs) so City Grid's own input dispatch
            // consumes the identical decision - not a copy.
            bool semiAutoMode = !WeaponFireModeCore.IsHeldFullAutoFamily(equippedFamily, familyTier[5]);

            if(playerMelee && firePressQueued && semiAutoMode)
                semiAutoPressBlockedByMelee=true;

            // Normal (non-melee-blocked) semi-auto press behaviour is unchanged.
            if(!playerMelee &&
               firePressQueued &&
               semiAutoMode &&
               !semiAutoPressBlockedByMelee)
            {
                firePressQueued=false;
                BeginFireRequest();
            }

            // C/D, F2 and F3 are native held-fire modes.
            bool f2Mode=equippedFamily==5 && familyTier[5]==1;
            bool f3Mode=equippedFamily==5 && familyTier[5]==2;   // Task027M.2
            if((equippedFamily==2 || equippedFamily==3 || f2Mode || f3Mode) &&
               fireHeld && !previousFireHeld)
                BeginFireRequest();

            // Task027L.4.8 P1: advance the held huoyan 0->5 re-trigger clock so
            // the release frame (0..5) can be snapshotted when Shoot is let go.
            // DischargeF2 (the fire tick that re-issues sub_679F4's 0->5
            // one-shot) and PlayF2InitialFire zero it.
            if(f2Mode && f2SessionActive && fireHeld && !f2ReleasePending)
                f2HuoyanCycleSeconds+=Time.deltaTime;

            bool fireReleasedThisFrame=!fireHeld && previousFireHeld;
            if(fireReleasedThisFrame)
            {
                // Task027L.2: Orange-UP only ends the held state. It does not
                // own the deferred post-melee semi-auto shot.
                EndFireRequest();
                // Task027L.4.8: the pending post-melee C/D frame-0 ramp only
                // applies while Shoot is still held (native byte_21BE78).
                cdRestartFireAfterMelee=false;
            }

            // Task027L V30.1: native melee state owns the action and detaches
            // the ranged secondary. Held-auto input may remain physically held,
            // but C/D/F2 must not re-arm the secondary until melee has ended.
            if(!playerMelee &&
               fireHeld &&
               !playerVisual.secondary.Active &&
               familyAmmo[equippedFamily]>0)
            {
                if(equippedFamily==2 || equippedFamily==3)
                {
                    if(cdRestartFireAfterMelee)
                    {
                        // Native sub_667B0 C/D case: first auto shot re-armed
                        // after a melee interrupt ramps from frame 0 and resets
                        // the sustained-fire counter (player+132 = 0). The
                        // between-swing full-auto itself is unchanged / already
                        // source-faithful - byte_21BE78 outlives the melee.
                        cdRestartFireAfterMelee=false;
                        sustainedShotCounter=0;
                        PlayCurrentFireRange(0,6);
                    }
                    else PlayCurrentFireRange(3,6);
                }
                else if(f2Mode)
                {
                    // Native 3..4 held continuation is only valid while the
                    // already-established F2 session (player+188) is active.
                    // A bare fireHeld level cannot create a frame-3 session.
                    if(f2SessionActive)
                        PlayF2HeldCycle();
                }
                else if(f3Mode)
                {
                    // Task027M.2: identical held-cycle shape to F2 (native
                    // sub_663A0 F-path re-loops fire frames 3..4 @ 0.2 s for
                    // BOTH tier 1 and tier 2).
                    if(f3SessionActive)
                        PlayF3HeldCycle();
                }
            }

            previousFireHeld=fireHeld;
        }

        void BeginFireRequest()
        {
            // Task027L source-faithful shared action gate:
            // native ranged entry rejects while MELEE owns player action state.
            if(playerMelee)
                return;

            if(equippedFamily<0 || equippedFamily>5)
                return;

            // TASK027N.2.31 Part A: with Infinite Ammo on, never treat the
            // current weapon as depleted here - the authoritative decrement
            // (DischargeCurrentWeapon()) will restore itself immediately
            // after each shot anyway, but WITHOUT this bypass a weapon
            // that's already sitting at exactly 0 (e.g. right after the
            // toggle was turned on) would still incorrectly trigger an
            // auto-switch or block firing before ever reaching that point.
            if(!infiniteAmmo && familyAmmo[equippedFamily]<=0)
            {
                if(!AutoSwitchToLoaded())
                    return;
            }

            if(equippedFamily==5 && familyTier[5]==2)
            {
                // Task027M.2: F3 laser held-auto session (parallel to F2). The
                // press edge starts / re-holds the session; DischargeF3 (driven
                // by the player fire-anim frame-3 event, ~0.4 s cycle) runs the
                // cone, the beam re-position and the Attack_F3 request.
                sustainedShotCounter=0;
                if(!f3SessionActive && (!playerVisual.secondary.Active ||
                    (playerVisual.secondary.Clip!=null && playerVisual.secondary.Clip.name.EndsWith("_release"))))
                    PlayF3InitialFire();
                return;
            }

            // TASK027N.2.25 CORRECTION of N.2.24's own fix, per an explicit
            // user rejection of "resume preserves shot count/quickly returns
            // to the old stage" - that interpretation is NOT accepted. Fresh
            // native decompile of sub_667B0 (native's own C/D press-edge
            // handler) PROVES the reset IS real, but is GATED on the
            // session-active flag (player+188), not unconditional on every
            // press: `if(!*(a1+188)){ ...; *(a1+132)=0; }` - the counter
            // only resets when the session was GENUINELY inactive, not on
            // every press-edge. Separately PROVEN: sub_65A08 (native's own
            // weapon-switch handler) never touches player+188 - switching
            // weapons does NOT itself end a session. The exact native
            // trigger that clears player+188 back to 0 (i.e., a true
            // release) could not be located despite an exhaustive further
            // search this task (sub_667B0's own other 3 callers either
            // failed to decompile or turned out unrelated) - rageSessionActive
            // below is this task's own reasoned, PLATFORM_ADAPTER placement
            // of that one remaining gap: true release clears it (see
            // EndFireRequest), weapon switch does not (matching the PROVEN
            // switch-preserves-session finding), matching the user's own
            // explicit requirement that a brief release+re-press must NOT
            // preserve/resume old progress - re-pressing after a TRUE
            // release now correctly restarts the raw counter from 0, while
            // the VISIBLE face level remains a fully separate field that
            // only ever decays on its own independent timer.
            //
            // TASK027N.2.29 CORRECTION: this comment's own claim ("fully
            // separate field... independent timer") was CONTRADICTED by the
            // code directly below it, which used to also reset
            // faceResetAt=-1f right here - unconditionally cancelling the
            // OLD visible face's own pending expiration on EVERY fresh
            // press, even a single zero-shot tap. Fresh re-decompile of
            // sub_667B0 (native's own press-edge handler, this task) PROVES
            // it has ZERO references to a1+108 (the visible face field)
            // anywhere in its body - a native press-edge cannot touch that
            // field at all. faceResetAt now belongs EXCLUSIVELY to the OLD
            // face's own expiration countdown and must never be touched by
            // a fresh press - only sustainedShotCounter (mirroring native's
            // own proven, session-gated a1+132 reset) is reset here. See
            // rage_v3046_native_differential_n229.json.
            if(equippedFamily==2 || equippedFamily==3)
            {
                if(!rageSessionActive)
                    sustainedShotCounter=0;
                rageSessionActive=true;
            }

            if(equippedFamily==5 && familyTier[5]==1)
                sustainedShotCounter=0;

            if(equippedFamily==5 && familyTier[5]==1)
            {
                if(f2ReleasePending)
                    F2DebugTelemetry.Log("REHOLD",F2DebugStateSummary());
                f2ReleasePending=false;
                f2RestartInitialAfterMelee=false;
            }

            if(equippedFamily==0 && playerVisual.secondary.Active)
            {
                // Native TYPE-0 re-tap accelerator:
                // a new press edge during the still-attached fire container
                // restarts at frame 1. No timer/cooldown.
                PlayCurrentFire(1);
                return;
            }

            bool releasedF2Tail=equippedFamily==5 && familyTier[5]==1 && !f2SessionActive &&
                playerVisual.secondary.Clip!=null && playerVisual.secondary.Clip.name=="p_fire_F2_release";
            if(!playerVisual.secondary.Active || releasedF2Tail)
            {
                if(equippedFamily==2 || equippedFamily==3)
                    PlayCurrentFireRange(0,6);
                else if(equippedFamily==5 && familyTier[5]==1)
                    PlayF2InitialFire();
                else
                    PlayCurrentFire(0);
            }
            // TYPE 1/B intentionally ignores a re-press while active.
        }

        void EndFireRequest()
        {
            if(equippedFamily==5 && familyTier[5]==1)
            {
                RequestF2ReleaseAfterNaturalCompletion();
                return;
            }

            if(equippedFamily==5 && familyTier[5]==2)
            {
                // Task027M.3: DEFER, do not Stop. Native sub_663A0 lets the
                // in-flight player fire-anim reach its frame-3 discharge even
                // after Shoot goes up; release only stops the next held cycle.
                // The beam wind-down + BEAnim recovery run once the fire-anim
                // completes (CompletePendingF3Release).
                RequestF3ReleaseAfterNaturalCompletion();
                return;
            }

            if(equippedFamily==2 || equippedFamily==3)
            {
                // TASK027N.2.25: a genuine release ends the fire session -
                // the NEXT press must see rageSessionActive==false so
                // BeginFireRequest() correctly resets the raw progress
                // counter from 0 (matching native's own proven
                // if(!*(a1+188)) gate), rather than resuming/preserving old
                // progress toward the next visible stage. The VISIBLE face
                // level itself is untouched here - it only ever changes via
                // its own separate decay timer.
                rageSessionActive=false;
                // TASK027N.2.30 CORRECTION: this used to unconditionally
                // re-arm faceResetAt=Time.time+2f on EVERY release, even a
                // SECOND release following a brief sub-threshold tap/burst
                // that never touched the still-pending OLD face's own
                // expiry (per N2.29's own fix). Found via this task's own
                // render-truth trace (n230_render.txt): a deliberate one-
                // shot-after-release test showed faceResetAt drift by
                // exactly the real elapsed time between two release events
                // (~1.9ms in one captured run), proving the SECOND release
                // was silently EXTENDING the deadline instead of leaving it
                // untouched - directly violating the task's own "future...
                // taps, bursts... must NOT... restart it, extend it" model.
                // Only arm when nothing is already pending - a release that
                // follows a sub-threshold burst (where no shot ever crossed
                // a band and overwrote the display) must not touch an
                // already-ticking OLD face's own deadline at all.
                if(faceResetAt<0f)
                    faceResetAt=Time.time+2f;
            }
        }

        void PlayCurrentFire(int startFrame)
        {
            DCAnimation clip=playerVisual.FindClip(
                "p_fire_"+CurrentWeaponName());
            playerVisual.secondary.Play(clip,startFrame);
        }

        void PlayCurrentFireRange(int startFrame,int endFrame)
        {
            DCAnimation clip=playerVisual.FindClip(
                "p_fire_"+CurrentWeaponName());
            playerVisual.secondary.PlayRange(clip,startFrame,endFrame);
        }

        // Task027L.4.10 / .4.15: native effectClassiPad / CAObj draw-order key
        // lives at owner+0x5C, and sub_7677C copies it to EVERY sprite child+36
        // (and sub_76A20 refreshes it every tick, plus a per-part secondary
        // child+0x30 = owner+0x4C + partIndex) - so one native owner occupies ONE
        // draw key and the renderer sorts sprites by (child+36, then child+0x30).
        //
        // Task027L.4.15 CORRECTION of the L4.10 model. Proven native keys
        // (re-verified this task): Dahuo/engulfing burn effect+0x5C = 9
        // (sub_12884 @ 0x12a92); dx/qxEffectAnimation blood/hit = 10 (sub_10ECC);
        // huoyan/flamethrower stream = 13 (sub_679F4 @ 0x67b80). The DINO BODY
        // draw key (dino+0x5C) is a base-class DEFAULT - it is NOT set in the
        // dino spawn (sub_18858) or the MoveAnim attach (sub_FF68); value not
        // byte-read, BOUNDED > 9 (LIKELY 10-13, Task027K3). The iPad shows the
        // engulfing Dahuo BEHIND the whole dinosaur, so:
        //
        //   ENGULFING Dahuo (key 9)  <  DINO BODY (default key > 9)  <=
        //   small flames xiaohuo1/xiaohuo2 (authored LAST parts of the k*_shao
        //   body clip - same owner as the body, foreground by part index, NOT a
        //   separate key)  ;  BLOOD/HIT (key 10, renders over the body)  <
        //   FLAME/huoyan (key 13, on top).
        //
        // L4.10's "DINO BODY < 9 < BURN FX" was the wrong visual role (the key 9
        // is proven; its role is BEHIND the body, not above it). NativeDrawOrder
        // below maps effect keys that sit ABOVE the dino body (blood 10, flame
        // 13) into their own non-overlapping bands (stride 16 > the 14-part
        // k*_shao body span; band 0 = the body's existing baseSortingOrder +
        // [0..13], unchanged). The key-9 engulfing Dahuo is BELOW the body and is
        // placed by NativeEngulfingBurnBelowBody instead (see BeginF2Burn).
        const int NativeDrawKeyStride=16;
        const int NativeDinoBodyKeyCeiling=8;
        // Structural equivalent of native z(Dahuo)=9 < z(dino body)>9: one clear
        // slot below the body's lowest part (enemyVisual.baseSortingOrder). NOT
        // an arbitrary magnitude - anchored to "just under the body".
        const int NativeEngulfingBurnBelowBody=2;
        // TASK027N.2.35: uses the constant 200 (every DinoInstance's own
        // baseSortingOrder, set once in SpawnDino) rather than dereferencing
        // enemyVisual - this is called from DischargeF2/DischargeF3 to
        // create the PLAYER-anchored flame/beam visual, which can happen
        // with zero dinos currently spawned (current==null).
        const int DinoBaseSortingOrderConst=200;
        int NativeDrawOrder(int nativeKey,int partIndex=0)
            => DinoBaseSortingOrderConst
               + Mathf.Max(0,nativeKey-NativeDinoBodyKeyCeiling)*NativeDrawKeyStride
               + partIndex;

        void PlayF2InitialFire()
        {
            // Native sub_667B0: every legitimate F2 (re)start attaches at
            // frame 0 and sets player+188.
            f2SessionActive=true;
            f2HuoyanCycleSeconds=0f;
            playerVisual.secondary.PlayRange(
                playerVisual.FindClip("p_fire_F2"),0,4);
        }

        void PlayF2HeldCycle()
        {
            playerVisual.secondary.PlayRange(
                playerVisual.FindClip("p_fire_F2_hold"),3,4);
        }

        void RequestF2ReleaseAfterNaturalCompletion()
        {
            // Task027K4.1 / sub_663A0 contract:
            // the in-flight F2 one-shot fire clip is not cancelled by Orange-up.
            // Release becomes actionable only after natural secondary completion.
            f2ReleasePending=true;
            F2DebugTelemetry.Log(
                "RELEASE_PENDING_SET",
                F2DebugStateSummary());

            if(!playerVisual.secondary.Active)
                CompletePendingF2Release();
        }

        void CompletePendingF2Release()
        {
            if(!f2ReleasePending)
                return;

            f2ReleasePending=false;
            f2RestartInitialAfterMelee=false;
            f2SessionActive=false;
            F2DebugTelemetry.Log(
                "RELEASE_PENDING_CLEAR",
                F2DebugStateSummary());

            // The native effect release preserves its current outer flame frame.
            if(f2FlameVisual!=null)
                f2FlameVisual.EndHeld(true);
            DCAnimation release=playerVisual.FindClip("p_fire_F2_release");
            if(release!=null)playerVisual.secondary.PlayRange(release,4,release.frames.Length-1);

            //12F28 completes all state5 targets when the actual flame ends.

            faceResetAt=Time.time+2f;
        }

        void RequestF3ReleaseAfterNaturalCompletion()
        {
            // Task027M.3 / sub_663A0 tier-2 held-branch:
            // the in-flight player F3 fire-anim is NOT cancelled by Shoot-up.
            // Its frame-3 discharge event (sub_66CE8: ammo-- + sub_67CE8 beam/
            // audio/cone) already committed on the first held Update; release
            // only stops the next 3..4 re-loop. So mark the release pending and
            // let the fire-input handler finish it once the secondary clip is
            // no longer Active (quick-tap: press+immediate release still fires).
            f3ReleasePending=true;
            f3SessionActive=false;
            if(!playerVisual.secondary.Active)
                CompletePendingF3Release();
        }

        void CompletePendingF3Release()
        {
            if(!f3ReleasePending)
                return;

            f3ReleasePending=false;

            // The native effect release winds the beam down from its CURRENT
            // frame (F3LaserVisual.EndHeld); no residual beam.
            if(f3LaserVisual!=null)
                f3LaserVisual.EndHeld(true);
            DCAnimation release=playerVisual.FindClip("p_fire_F3_release");
            if(release!=null)playerVisual.secondary.PlayRange(release,4,release.frames.Length-1);

            // sub_68A8C else-branch / sub_12C64: once the fire session is over
            // the affected dino leaves state 4 via the BEAnim recovery tail.
            //12F28 owns living target exit after the beam tail completes.
            faceResetAt=Time.time+2f;
        }

        void EndF2Fire(bool clearSurvivor)
        {
            // Native sub_66310/melee teardown clears player+188.
            f2SessionActive=false;

            // Immediate interrupt owner (hurt/melee/switch/zero-ammo), not the
            // ordinary Orange-up path.
            f2ReleasePending=false;
            playerVisual.secondary.Stop();
            if(f2FlameVisual!=null) f2FlameVisual.EndHeld(false);
            EndAttachedFStates(true); //66310 ->12F28 completes every living state5 target.
            faceResetAt=Time.time+2f;
        }

        void OnPlayerSecondaryFrame(Power2DTrack t,int frame)
        {
            if(frame==3)
            {
                DischargeCurrentWeapon();
                // Task027B: frame 3 is the discharge point, not A's loop point.
                return;
            }

            //N253:7D81C marks only3;76A20's flagged producer does not
            //dispatch66CE8 for4. Consumer-body unconditional calls alone
            //do not prove an additional frame event.
        }

        void EndAttachedFStates(bool flame)
        {
            var saved=current;
            foreach(var d in dinos.ToArray()) {
                if(d.hp<=0f)continue;
                current=d;
                if(flame && enemyBurningF2)EndF2Burn();
                else if(!flame && enemyF3State)EndF3();
            }
            current=saved;
        }

        // TASK027N.2.35 Part J/L: shared closest-eligible-dino selector.
        // Mirrors N.2.34's own PROVEN sub_668FC closest-in-range analytical
        // target selection (not a simulated/first-collision projectile
        // model) - used by ordinary A-D fire's own single-target resolution
        // (Part J) and, unchanged, by the F2/F3 single-target acquisition
        // bridge (Part L). `current` is restored before returning; the
        // caller is responsible for re-setting it to the returned target
        // (if any) before reading/writing that target's own state.
        DinoInstance FindClosestEligibleDino(
            float maxRange,out Vector3 closestHead,out float closestSigned)
        {
            var prevCurrent=current;
            DinoInstance best=null;
            closestHead=Vector3.zero;
            closestSigned=0f;
            float bestAbs=float.MaxValue;

            foreach(var d in dinos.ToArray())
            {
                if(d.markedForRemoval) continue;
                current=d;
                if(enemyHP<=0f) continue;

                Vector3 head=enemy.position;
                enemyVisual.TryGetPartWorldAnchor("tou",out head);
                float signed=head.x-player.position.x;
                bool inFront=playerVisual.facingRight ? signed>0f : signed<0f;
                if(!inFront || Mathf.Abs(signed)>=maxRange) continue;

                if(Mathf.Abs(signed)<bestAbs)
                {
                    bestAbs=Mathf.Abs(signed);
                    best=d;
                    closestHead=head;
                    closestSigned=signed;
                }
            }

            current=prevCurrent;
            return best;
        }

        void DischargeCurrentWeapon()
        {
            if(playerDead || equippedFamily<0 || equippedFamily>5)
                return;

            int family=equippedFamily;
            if(!infiniteAmmo && familyAmmo[family]<=0)
                return;

            // TASK027N.2.31 Part A: let the normal, authoritative decrement
            // run exactly as source-faithful (preserving every downstream
            // side effect that reads the ammo count within this same call),
            // then restore it immediately if Infinite Ammo is on - BEFORE
            // this function's own later "ran out -> AutoSwitchToLoaded()"
            // check, so that check correctly sees ammo as still available
            // and never fires. Damage/cadence/animation are untouched by
            // this - nothing else in this function branches on the ammo
            // VALUE itself, only on whether it just reached <=0.
            familyAmmo[family]--;
            if(infiniteAmmo)
                familyAmmo[family]++;
            DC1WeaponRow w=CurrentWeaponRow();

            if(family==4)
            {
                DischargeE(w);
                return;
            }

            if(family==5)
            {
                if(familyTier[5]==0) DischargeF1(w);
                else if(familyTier[5]==1) DischargeF2(w);
                else if(familyTier[5]==2) DischargeF3(w);
                return;
            }

            // TASK027N.2.22 Part I/K: native sub_66DCC plays this
            // unconditionally on EVERY A-D shot, hit or miss, before the
            // hit-test - see the Resources.Load site's doc comment for the
            // full citation and the proven C/D swap.
            if(abcdFireSource!=null)
            {
                AudioClip fireClip = family switch
                {
                    0 => attackAClip,
                    1 => attackBClip,
                    2 => familyTier[2]>=2 ? attackE1Clip : attackCFamilyClip,
                    3 => attackDFamilyClip,
                    _ => null
                };
                if(fireClip!=null)
                {
                    NativeAudioSessionGate.Play(abcdFireSource,fireClip);
                    TestHarness_LastAbcdFireClip=fireClip;
                }
            }

            // TASK027N.2.35 Part J: ordinary A-D fire terminates on the
            // CLOSEST eligible dino only (never piercing/multi-hit), matching
            // N.2.34's own proven sub_668FC single-target analytical
            // selection - not a simulated first-collision projectile model.
            var abcdPrevCurrent=current;
            DinoInstance abcdTarget=FindClosestEligibleDino(
                w.atkDis,out Vector3 targetHead,out float signed);
            bool hit=abcdTarget!=null;
            if(hit) current=abcdTarget;

            if(hit)
            {
                float dmg=UnityEngine.Random.Range(w.atkMin,w.atkMax);

                // Task004 weapon-B point-blank multiplier.
                if(family==1)
                {
                    Vector3 muzzle;
                    float rot;
                    Vector2 sz;
                    string socket=GetFireSocket();
                    if(playerVisual.TryGetPartWorldSocket(
                        socket,0.5f,0.2f,out muzzle,out rot,out sz))
                    {
                        float ratio=Mathf.Clamp01(
                            Mathf.Abs(muzzle.x-targetHead.x)/w.atkDis);
                        dmg*=3f-2f*ratio;
                    }
                }

                enemyHP-=dmg;

                if(enemyHP<=0f)
                {
                    enemyHP=0f;
                    money=Mathf.Min(999999,money+(int)enemySourceReward);
                    BeginStandardEnemyDeath();
                }
                else
                {
                    BeginLightBefired();

                    // TASK027N.2.21 Part D/E: TASK027N.2's own original
                    // "weapon_blood_matrix_n2.json" citation below (now
                    // superseded) was only LIKELY-confidence and never
                    // independently re-verified against the full case body -
                    // it turned out to be WRONG. TASK027N.2.14's later,
                    // EXHAUSTIVE, full-instruction decompile of sub_10554
                    // (abcd_full_success_hit_side_effect_callgraph_n214.json)
                    // already PROVED A-D's entire non-lethal reaction body is
                    // exactly 5 calls (BefiredAnim setup x2, recoil velocity,
                    // recoil damping, sub_10ECC(dino,0)) with "no other calls
                    // present" - explicitly NO sub_11124 (ballistic blood)
                    // call anywhere in A-D's own case body (native's default
                    // case / case1, weaponClass 0/1/2/3). This was never
                    // reconciled against this file's own stale comment/code
                    // until TASK027N.2.21 independently re-decompiled
                    // sub_10554's FULL switch this task and confirmed the
                    // same result directly (default case for A/C/D and case1
                    // for B: BefiredAnim + recoil physics + sub_10ECC(a1,0)
                    // only - zero sub_11124 calls). Native's sub_11124
                    // ballistic-blood call is PROVEN to belong only to case4
                    // (E, unconditional on species) and case6 (G, unconditional
                    // on species) - see weapon_ballistic_blood_matrix_n221.json.
                    // The erroneous 3x SpawnTask026Blood call that used to sit
                    // here (added under the old, superseded citation) is
                    // REMOVED. A-D correctly keep: damage, hurt reaction
                    // (BeginLightBefired), and the qx Blood Impact Burst
                    // below (DinoCapBloodBurst.Spawn - the OLD redundant
                    // SpawnLightImpact() duplicate-qx-spawn call that used to
                    // sit here was ALSO removed this task, see
                    // qiangxue_spawn_ownership_n221.json) - matching the
                    // user's own runtime correction ("A/B/C/D ordinary
                    // ranged: ballistic blood = NO; attached qiangxue burst =
                    // YES [exactly once]").

                    // TASK027N.2.9: native sub_10ECC(dino,0) - A-D's ordinary
                    // ranged hit unconditionally creates the qx/ranged Blood
                    // Impact Burst variant.
                    // TASK027N.2.10/N.2.12/N.2.13/N.2.19/N.2.20: qiangxue's
                    // own visibility has flip-flopped across this whole arc
                    // as deeper evidence emerged each time - see
                    // DinoCapBloodBurst.cs's own class doc-comment for the
                    // full chain. FINAL (N.2.20) state: N.2.19's raw
                    // instruction-level trace (addresses 0x3dbd0/0x3dc04 in
                    // sub_2A8A8) proved qiangxue's "qiangxueAnim" lookup
                    // SUCCEEDS (real, resolvable index 761), reversing
                    // N.2.12/N.2.13's "PROVEN INVISIBLE" claim. N.2.20 then
                    // recovered the exact content by tracing sub_2A8A8's own
                    // TextureManager::CreateAnimation("qiangxueAnim") call
                    // (0x3b39a) forward to two concrete
                    // AddAnimationFrame("qiangxueAnim", frameKey) calls
                    // (0x3b438 frameKey="qiang0001Frame"; ~0x3b4ec
                    // frameKey="qiang0002Frame") - both PROVEN at the raw-
                    // instruction level, both resolving to real Task019
                    // registry rects on the same shared iPad dino2.png atlas
                    // daoxue lives on. Pixel-crop confirmed both are dark-red
                    // spike-fan burst art, color-identical to daoxue
                    // (152.7,0,0) but a distinct shape/asset - classification
                    // B.DISTINCT_QIANGXUE_ART, not a reused daoxue pixel and
                    // not the "unrelated gun icon" N.2.10 had guessed from
                    // dimensions alone. The temporary daoxue-pixel-reuse
                    // adapter is RETIRED: fix_qiangxue_pixels_n220.py
                    // repointed the qiangxue0001/qiangxue0002 atlas rects at
                    // native's own real qiang0001Frame/qiang0002Frame
                    // coordinates. qx's own distinctly-authored
                    // transform-pulse timing/geometry (qxEffectAnimation's
                    // own 5-frame curve) is untouched - only the terminal
                    // sprite-pixel identity changed, now SOURCE_FAITHFUL. See
                    // qiangxue761_exact_frames_n220.json /
                    // qiangxue761_vs_daoxue_n220.json.
                    DinoCapBloodBurst.Spawn(
                        enemyVisual,data,false,false,
                        enemyVisual.baseSortingOrder+65);
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
                    // Native sub_10554 light path: hurt visual + recoil only
                    // when the reaction word is 0 (neither chase nor bite).
                    bool lightReacted = !EnemyBiteCommitted() && !enemyAttacking;
                    DebugRecordHit(
                        "ABCD("+CurrentWeaponName()+")",
                        dmg,
                        lightReacted, lightReacted, false,
                        EnemyBiteCommitted() ? "ATTACK(1)" : "CHASE/IDLE(8)");
#endif
                }

                status=CurrentWeaponName()+" hit "+
                    enemyClipPrefix+" for "+dmg.ToString("0.00")+".";
            }
            else
            {
                status=CurrentWeaponName()+
                    " discharged at frame 3; no target in front/range.";
            }
            current=abcdPrevCurrent;

            SpawnCasingForCurrentWeapon();
            SpawnMuzzleForCurrentWeapon();

            if(family==2 || family==3)
                AdvanceSustainedFireFace();

            if(familyAmmo[family]<=0 && fireHeld)
                AutoSwitchToLoaded();
        }

        string GetFireSocket()
        {
            int t=familyTier[equippedFamily];
            if(equippedFamily==0)
                return "com1";
            if(equippedFamily==1)
                return "yyj5";
            if(equippedFamily==2)
                return t<=1 ? "com1" : "yyj5";
            if(equippedFamily==4)
                return t==0 ? "yyj5" : "com1";
            return "yyj5";
        }

        int GetCasingSourceZ()
        {
            int t=familyTier[equippedFamily];
            if(equippedFamily==0)
                return 8;
            if(equippedFamily==1)
                return 12;
            if(equippedFamily==2)
                return t<=1 ? 8 : 12;
            return 12;
        }

        void SpawnCasingForCurrentWeapon()
        {
            string socket=GetFireSocket();
            Vector3 p;
            float rot;
            Vector2 size;
            if(!playerVisual.TryGetPartWorldSocket(
                socket,0f,0.2f,out p,out rot,out size))
                return;

            string refName=equippedFamily==1 ? "danke2" : "danke1";
            WeaponCasing.Spawn(
                playerVisual,
                refName,
                p,
                playerVisual.facingRight,
                player.position.y-250f,
                playerVisual.baseSortingOrder+GetCasingSourceZ());
        }

        void SpawnMuzzleForCurrentWeapon()
        {
            string socket=GetFireSocket();
            Vector3 p;
            float rot;
            Vector2 size;
            if(!playerVisual.TryGetPartWorldSocket(
                socket,0.5f,0.2f,out p,out rot,out size))
                return;

            WeaponEffectAnimation.Spawn(
                data,
                "qhEffectAnimation",
                p,
                rot,
                playerVisual.facingRight,
                equippedFamily==0 ? 0.6f : 1f,
                playerVisual.baseSortingOrder+9);
        }

        // TASK027N.2.21 Part A/B/C: SpawnLightImpact() USED TO independently
        // spawn a second "qxEffectAnimation" instance (via the generic
        // WeaponEffectAnimation pool) at every one of its 3 call sites -
        // ALWAYS alongside an ALREADY-EXISTING DinoCapBloodBurst.Spawn(...,
        // melee:false,...) call for the exact same hit (DischargeCurrentWeapon
        // for A-D; ApplyHeavyWeaponHit's Type3-demoted and bite-demoted
        // branches for E/G). This was a genuine Unity-side literal
        // double-dispatch bug, not a native ownership/lifecycle question:
        // native's own sub_10ECC creates exactly ONE effect object per call
        // (PROVEN this task via sub_10ECC/sub_584D4 decompile - the object
        // pool sub_584D4 draws from is a generic shared free-list with no
        // owner-slot/replace-on-new-hit policy, so it does not itself cause
        // or prevent duplication; the duplication came entirely from Unity
        // calling TWO SEPARATE SPAWNER FUNCTIONS for one native-equivalent
        // event). DinoCapBloodBurst.Spawn is the more complete, more
        // rigorously source-verified implementation of native's sub_10ECC
        // (randomized head-relative offset contract proven N.2.16, correct
        // transform-pulse curve, dino-parented mount) and already runs at
        // every one of this method's 3 former call sites - removing this
        // method's own redundant WeaponEffectAnimation-pooled qx spawn loses
        // nothing. See qiangxue_spawn_ownership_n221.json /
        // native_qiangxue_instance_contract_n221.json. Native's own object
        // system permits genuinely distinct hits' bursts to overlap (no
        // single-instance enforcement exists anywhere in sub_10ECC/sub_584D4)
        // - so no cooldown/replace-existing logic was added; per this task's
        // own decision tree ("If native permits overlap from distinct valid
        // hits: do not globally ban overlap"), that is left alone.

        // TASK027N.2.23 Part D-N: native sub_66DCC (A-D fire dispatcher,
        // already used for N.2.22's audio contract) PROVES the rage-face
        // counter (player+132) is incremented ONLY in case2/case3
        // (weaponClass C/D) - case0/case1 (A/B) never touch it. This is
        // PROVEN eligibility, not inferred from "automatic"-sounding English
        // labels (E/F1/F2/F3/G use entirely separate discharge functions
        // that never reach sub_66DCC or player+132 at all - see
        // rage_weapon_eligibility_matrix_n223.json). Thresholds are read
        // from v17 (the counter BEFORE increment): _face1 at v17 in [9,28]
        // (i.e. shots 10-29), _face2 at [29,48] (shots 30-49), _face3 at
        // v17+1>=50 (shot 50+) - matching this method's own pre-existing
        // 10/30/50 constants exactly (re-verified, not changed).
        static int SustainedFaceLevel(int shots) =>
            shots>=50 ? 3 : shots>=30 ? 2 : shots>=10 ? 1 : 0;

        static readonly string[] SustainedFaceRefs = { "", "tou_face1", "tou_face2", "tou_face3" };

        void AdvanceSustainedFireFace()
        {
            sustainedShotCounter++;

            // TASK027N.2.25 fix: native's own sub_66DCC does NOT touch
            // a1+108 (the visible face field) at all for shots 1-9 of any
            // session (v17 in [0,8] falls through every threshold branch
            // with no assignment - PROVEN by direct decompile) - it only
            // ever writes a1+108 once the counter actually crosses INTO a
            // named band (>=10/30/50). The OLD code here unconditionally
            // overwrote headRefOverride on EVERY shot via
            // SustainedFaceRefs[SustainedFaceLevel(...)], which for shots
            // 1-9 evaluates to SustainedFaceRefs[0]="" - immediately
            // blanking a still-elevated, still-decaying visible level the
            // instant a NEW session's first shot landed (e.g. after a true
            // release+re-press while stage2 was still visible), rather than
            // leaving it alone as native does. Only assign when the level
            // is nonzero, matching native's own branch structure exactly.
            int level=SustainedFaceLevel(sustainedShotCounter);
            // TASK027N.2.29 CORRECTION of N.2.23's own fix: cancelling
            // faceResetAt used to be UNCONDITIONAL on every landed shot,
            // including sub-threshold shots (level==0) of a brand-new
            // session that has not yet touched the display at all. This
            // directly caused the user's own reported "burst-fire refresh"
            // bug - a NEW session's own early shots (which native's
            // sub_66DCC PROVES never write a1+108 below the first
            // threshold - re-confirmed via fresh decompile this task) were
            // silently cancelling the OLD visible face's own independent
            // expiration countdown, keeping old rage alive indefinitely
            // across repeated sub-threshold bursts. Only cancel faceResetAt
            // when THIS shot actually overwrites the display (level>0) -
            // matching native's own proven "unconditional overwrite only at
            // threshold-crossing" semantics (rage_overlap_display_priority_
            // n229.json): at that exact point any pending OLD-face expiry
            // is genuinely moot, since the display has already moved on to
            // the NEW session's own face. N2.23's original concern (a
            // single stray mid-session release blip arming a spurious
            // countdown) remains correctly handled - once a session is
            // already past its own first threshold, every further landed
            // shot has level>0 and still cancels normally.
            if(level>0)
            {
                playerVisual.headRefOverride=SustainedFaceRefs[level];
                faceResetAt=-1f;
            }
        }

        // TASK027N.2.28 CORRECTS the N.2.23-N.2.27 "stepwise decay" model
        // (3->2->1->neutral), which the user's own live V30.45 runtime
        // observation directly REJECTED as visibly wrong (CLAUDE.md 11.1 -
        // this is the SECOND time this exact function's own model has been
        // corrected by direct runtime observation, first in N2.25 for the
        // hidden-progression/re-press question, now for the VISIBLE
        // transition shape itself). Corrected acceptance: the current
        // visible rage face remains completely UNCHANGED for the reset
        // delay, then jumps DIRECTLY to neutral - there must be no
        // user-visible intermediate lower-stage face at all. Native
        // evidence (re-checked N2.27, re-confirmed this task): no decay/
        // step-down write of ANY kind was ever found for a1+108 anywhere in
        // the binary - sub_66DCC only ever WRITES UPWARD into a named band
        // (>=10/30/50), and no native function anywhere decrements it or
        // walks it through intermediate bands. The entire "step down one
        // band per tick" behavior removed this task was therefore ALWAYS a
        // Unity-only invention (introduced N2.23, never native-derived) -
        // removing it does not contradict any native evidence, it removes
        // an assumption that was never proven and is now proven WRONG by
        // direct user runtime observation. The delay itself (still the
        // pre-existing 2s constant) remains explicitly UNPROVEN at the
        // native level - see rage_direct_neutral_timing_n228.json.
        void UpdateSustainedFaceReset()
        {
            // TASK027N.2.26: SELF-ARMING decay, replacing the scattered
            // per-callsite "arm faceResetAt on release" approach. Investigation
            // this task found a genuine, deterministic V30.43 production gap:
            // AutoSwitchToLoaded() (DischargeCurrentWeapon() line ~2409, fired
            // automatically whenever C/D ammo hits 0 while fireHeld is still
            // true) changes equippedFamily away from C/D WITHOUT clearing
            // rageSessionActive or arming faceResetAt - unlike its sibling
            // SwitchWeapon() (manual Tab), which already did both correctly.
            // Once this fires, the visible rage face was orphaned forever:
            // EndFireRequest()'s own arm is gated on
            // equippedFamily==2||3, which is no longer true post-auto-switch,
            // so decay could never start - exactly the user's reported
            // "reach a rage stage, stop shooting, expression never returns to
            // neutral" symptom. Default ammo caps (C=150,D=200,
            // WeaponFamilyTable.AmmoCaps) mean this does not trigger on a
            // single short burst, but reliably does across a real, extended
            // play session (or repeated rage testing) - which is exactly why
            // N2.25's own 999-ammo test fixtures never caught it.
            //
            // Fix: decide "is a rage-eligible session CURRENTLY still able to
            // land a shot" directly from live state every frame, rather than
            // trusting that every call site that can end/redirect firing
            // remembers to arm the timer. This one central check transparently
            // covers every present and future "stopped firing" path (clean
            // release, manual switch, ammo-empty auto-switch, hurt/death
            // interrupt) with no per-callsite bookkeeping required.
            bool activelyFiringRage=
                rageSessionActive && (equippedFamily==2 || equippedFamily==3);
            if(faceResetAt<0f && !activelyFiringRage && sustainedShotCounter>0)
                faceResetAt=Time.time+2f;

            if(faceResetAt<0f || Time.time<faceResetAt)
                return;

            // TASK027N.2.28: DIRECT reset only. Whatever level is currently
            // showing (1, 2, or 3), the single reset delay elapsing takes it
            // straight to neutral - there is no intermediate band to step
            // through here at all. (The old code's own "level<=0" branch
            // and its separate "else step down one band" branch are
            // collapsed into this one unconditional reset, since both used
            // to produce the same neutral write - the ONLY behavior removed
            // is the intermediate step-down that used to run when level>0.)
            faceResetAt=-1f;
            playerVisual.headRefOverride="";

            // TASK027N.2.29 CORRECTION: this timer firing means the OLD
            // face has expired - it does NOT mean the CURRENT value of
            // sustainedShotCounter belongs to that old face. Per native
            // evidence (a1+132's own session-gated reset in sub_667B0 and
            // a1+108's own threshold-gated writes in sub_66DCC never being
            // touched by the same condition), the raw counter and the
            // visible face are proven-independent native fields. If a NEW
            // session is CURRENTLY active (activelyFiringRage), this
            // counter already belongs to ITS OWN in-progress accumulation
            // toward the next threshold - zeroing it here would silently
            // destroy real, still-relevant progress the instant the
            // unrelated OLD face happened to expire. Only clear it when no
            // session is actively using it.
            if(!activelyFiringRage)
                sustainedShotCounter=0;
        }

        // TASK027N.2.22 Part G/H/L: A1-A4/B1-B4/C1-C4/D1-D4 are ALL already
        // present with source-faithful damage/range data (WeaponFamilyTable.
        // Rows[0..3], "Task027/027K source rows", TierCount=4 for each) -
        // verified this task, no missing tier, no cloned/guessed stats
        // needed (abcd_weapon_inventory_native_unity_n222.json). The ONE
        // real gap: no debug control could select a SPECIFIC A-D tier at
        // runtime (SwitchWeapon() only cycles FAMILIES using whatever tier
        // is already set; only F/G had their own tier-select buttons). This
        // compact selector closes that gap without adding 16 separate
        // buttons.
        int debugAbcdFamilySelect;
        int debugAbcdTierSelect;
        internal void TestHarness_ApplyAbcdSelection(int family,int tier) =>
            DebugApplyAbcdSelection(family,tier);
        void DebugApplyAbcdSelection(int family,int tier)
        {
            family=Mathf.Clamp(family,0,3);
            tier=Mathf.Clamp(tier,0,WeaponFamilyTable.TierCount[family]-1);
            equippedFamily=family;
            familyTier[family]=tier;
            if(!playerMelee)
                playerVisual.primary.Play(
                    playerVisual.FindClip("p_stand_"+CurrentWeaponName()),0);
            status="DEBUG A-D select -> "+CurrentWeaponName()+".";
        }

        public void SwitchWeapon()
        {
            if(playerDead || playerHitStun)
                return;

            f2SessionActive=false;
            f2RestartInitialAfterMelee=false;
            if(equippedFamily==5 && familyTier[5]==1) EndF2Fire(true);
            if(equippedFamily==5 && familyTier[5]==2) EndF3Fire(true);
            playerVisual.secondary.Stop();
            fireHeld=false;
            previousFireHeld=false;
            guiFireHeld=false;
            firePressQueued=false;
            semiAutoPressBlockedByMelee=false;
            // TASK027N.2.30: same correction as EndFireRequest() - only arm
            // when nothing is already pending, so a switch following a
            // sub-threshold session (which never overwrote the still-
            // ticking OLD face) cannot extend its deadline.
            if(faceResetAt<0f)
                faceResetAt=Time.time+2f;

            int start=equippedFamily;
            int f=start;
            do
            {
                f=(f+1)%6;
                if(f<6 && familyTier[f]>=0)
                {
                    equippedFamily=f;
                    if(!playerMelee)
                    {
                        playerVisual.primary.Play(
                            playerVisual.FindClip(
                                "p_stand_"+CurrentWeaponName()),0);
                    }
                    status="Green switch -> "+CurrentWeaponName()+
                        " (manual switch does not skip zero ammo).";
                    return;
                }
            } while(f!=start);
        }

        bool AutoSwitchToLoaded()
        {
            int start=equippedFamily;
            int f=-1;
            //Native66BA4 then66C18: higher ascending, lower descending.
            //Do not perform65A08 cancellation unless a destination exists.
            for(int candidate=start+1;candidate<=5;candidate++)
                if(IsNativeLoadedFamily(candidate)) { f=candidate; break; }
            if(f<0)
                for(int candidate=start-1;candidate>=0;candidate--)
                    if(IsNativeLoadedFamily(candidate)) { f=candidate; break; }
            if(f<0) return false;
            if(equippedFamily==5 && familyTier[5]==1) EndF2Fire(false);
            if(equippedFamily==5 && familyTier[5]==2) EndF3Fire(false);
                    // TASK027N.2.26: this ammo-empty auto-switch previously
                    // left rageSessionActive stale-true when leaving C/D
                    // (visible-decay itself is now fixed independently via
                    // UpdateSustainedFaceReset()'s own self-arm above, since
                    // it re-derives "actively firing" from equippedFamily
                    // every frame - but rageSessionActive staying stuck true
                    // would still wrongly suppress the fresh-press progress
                    // reset on a later re-equip of C/D, contradicting N2.25's
                    // own proven "a true stop, not a mere family-cycle,
                    // restarts raw progress from 0" contract). Clear it here
                    // exactly like SwitchWeapon() already does.
                    if(equippedFamily==2 || equippedFamily==3)
                        rageSessionActive=false;
                    equippedFamily=f;
                    playerVisual.secondary.Stop();
                    if(!playerMelee)
                    {
                        playerVisual.primary.Play(
                            playerVisual.FindClip(
                                "p_stand_"+CurrentWeaponName()),0);
                    }
                    status="Empty weapon auto-switch -> "+CurrentWeaponName()+".";
                    if(fireHeld)
                        PlayCurrentFire(0);
                    return true;
        }

        bool IsNativeLoadedFamily(int family)
        {
            int tier=familyTier[family];
            //Preserve the shipped tier-indexed row-count lookup66BCE/66C46.
            return tier>=0 && tier<WeaponFamilyTable.TierCount.Length &&
                tier<WeaponFamilyTable.TierCount[tier] && familyAmmo[family]>=1;
        }

        public void CycleFMode()
        {
            f2SessionActive=false;
            f2RestartInitialAfterMelee=false;
            if(equippedFamily==5 && familyTier[5]==1) EndF2Fire(true);
            if(equippedFamily==5 && familyTier[5]==2) EndF3Fire(true);
            if(equippedFamily==5 && familyTier[5]==2) EndF3Fire(true);
            // Task027M.2: cycle all three authored F tiers (0/1/2 = F1/F2/F3).
            familyTier[5]=(Mathf.Clamp(familyTier[5],0,2)+1)%3;
            if(equippedFamily==5)
            {
                playerVisual.secondary.Stop();
                if(!playerMelee)
                {
                    playerVisual.primary.Play(
                        playerVisual.FindClip("p_stand_"+CurrentWeaponName()),0);
                }
            }
            status="DEBUG F mode -> F"+(familyTier[5]+1)+
                " (F1 circular saw / F2 flamethrower / F3 laser).";
        }

        public void CycleMeleeTier()
        {
            // Task027J/J1 source closure:
            // native family-G runtime levels are exactly 0..3 -> G1..G4.
            // G0.c2b is shipped/loaded but has no runtime merge consumer.
            // Debug-only selector: does not alter death routing or any A-F state.
            meleeTier=(Mathf.Clamp(meleeTier,0,3)+1)%4;
            status="DEBUG G tier -> G"+(meleeTier+1)+
                " (next Blue melee uses its source clip + damage row).";
        }

        public void Melee()
        {
            // Task027L V30.4 iPad acceptance closure:
            // Blue pressed before the current melee is fully complete is ignored.
            // It does NOT restart the active swing and does NOT queue a second one.
            if(playerDead || playerHitStun || playerMelee)
                return;

            if(equippedFamily==5 && familyTier[5]==1)
            {
                // Task027L.3: melee is an immediate F2-session interrupt.
                // Preserve physical Orange held, but invalidate held-cycle
                // continuation so the next between-melee resume starts at 0.
                f2RestartInitialAfterMelee=fireHeld;
                EndF2Fire(true);
            }
            // Task027M.2: melee interrupts a held F3 the same way (native
            // MELEE_PRIORITY). A fresh Shoot press is required to re-arm.
            if(equippedFamily==5 && familyTier[5]==2)
                EndF3Fire(true);

            // Task027L.4.8: melee owns the action channel while its latch is up
            // (native byte_21BE74 beats byte_21BE78 in sub_66550). The held C/D
            // auto does not fire during the swing, but Shoot stays latched, so
            // the next inter-swing gap re-arms it - and native sub_667B0 makes
            // that first re-armed shot a frame-0 ramp with the rage-face counter
            // reset.
            if((equippedFamily==2 || equippedFamily==3) && fireHeld)
                cdRestartFireAfterMelee=true;

            playerVisual.secondary.Stop();

            // Preserve the physical Orange-held state. Semi-auto press edges
            // received during melee remain pending in firePressQueued and are
            // consumed once after melee fully releases playerMelee.
            DCAnimation clip=playerVisual.FindClip(
                "p_hit_G"+(meleeTier+1));

            if(clip==null)
            {
                status="Missing generated G melee clip.";
                return;
            }

            playerMelee=true;
            playerVisual.primary.Play(clip,0);
            status="G"+(meleeTier+1)+
                " melee: contact event = frame 10.";
        }

        // TASK027N.2.35 Part I: ONE G-family melee contact enumerates ALL
        // active living dinos and hits EVERY one whose independently-
        // evaluated source-faithful range/eligibility test passes (matches
        // N.2.34's own proven sub_668FC multi-hit dispatch mode) - not just
        // the single closest one. The per-target eligibility test itself
        // (dx range, species reaction, blood/audio) is completely UNCHANGED
        // from the pre-migration single-dino version; only the loop around
        // it is new.
        void ExecuteMeleeContact()
        {
            // TASK027N.1.3 Part Q: this is the melee CONTACT EVENT (frame 10
            // of the player's swing) - it fires whether or not a target is
            // found, matching Task027J2's audio-on-every-swing contract.
            DebugRecordMeleeContactEvent();

            var prevCurrent=current;
            bool anyTargetFound=false;
            bool anyDamageDispatched=false;
            string lastBlockReason="no live target (enemyHP<=0)";

            DC1WeaponRow w=WeaponFamilyTable.MeleeRows[Mathf.Clamp(meleeTier,0,3)];
            MeleeContactCore.Dispatch(dinos.ToArray(), player.position, playerVisual.facingRight,
                w.atkDis,
                d => { current=d; return !d.markedForRemoval && enemyHP>0f; },
                d => {
                    current=d;
                    Vector3 head=enemy.position;
                    enemyVisual.TryGetPartWorldAnchor("tou",out head);
                    return head;
                },
                d => {
                    current=d;
                    anyTargetFound=true;
                    ApplyHeavyWeaponHit(w, Mathf.Clamp(meleeTier,0,3)>=2
                        ? HeavyFatalMode.GSpecial : HeavyFatalMode.Standard);
                    anyDamageDispatched=true;
                    AudioClip contactClip=Mathf.Clamp(meleeTier,0,3)>=2
                        ? meleeContactAtt2Clip : meleeContactAtt1Clip;
                    if(meleeAudioSource!=null && contactClip!=null)
                        NativeAudioSessionGate.Play(meleeAudioSource,contactClip);
                });

            current=prevCurrent;

            if(anyTargetFound) DebugRecordMeleeTargetFound();
            else DebugRecordMeleeBlock(lastBlockReason);
            if(anyDamageDispatched) DebugRecordMeleeDamageDispatched();
        }

        void DischargeF1(DC1WeaponRow w)
        {
            Vector3 spawn=player.position;
            float rot;
            Vector2 sourceSize;
            // Task027K1 / sub_67884:
            // local offset = (W * 0.5 * 0.15, H * 0.5 * 0.04)
            //              = (W * 0.075,      H * 0.02)
            const float F1SpawnWidthFactor=0.075f;
            const float F1SpawnHeightFactor=0.02f;
            if(!playerVisual.TryGetPartWorldAnchorSocket(
                "yyj5",
                F1SpawnWidthFactor,
                F1SpawnHeightFactor,
                out spawn,
                out rot,
                out sourceSize))
            {
                status="F1: missing yyj5 source socket.";
                return;
            }

            FProjectileParticle.Spawn(
                data,
                this,
                spawn,
                playerVisual.facingRight,
                playerVisual.baseSortingOrder+11);

            if(meleeAudioSource!=null && attackF1Clip!=null)
                NativeAudioSessionGate.Play(meleeAudioSource,attackF1Clip);

            status="F1 jupan: frame-3 spawn, +/-500 X, piercing X<48.";

            if(familyAmmo[5]<=0 && fireHeld)
                AutoSwitchToLoaded();
        }

        public void TryF1ProjectileContact(
            FProjectileParticle projectile)
        {
            if(projectile==null || enemy==null || enemyHP<=0f)
                return;

            if(Mathf.Abs(enemy.position.x-projectile.transform.position.x)>=48f)
                return;

            if(projectile.HasHit(enemy))
                return;

            projectile.MarkHit(enemy);

            // TASK027N.2 Part C/E: F1 is PROVEN excluded from ordinary blood
            // (weapon_blood_matrix_n2.json) even though it shares GSpecial
            // fatalMode with G3/G4 melee here - see ApplyHeavyWeaponHit's
            // spawnBlood parameter doc comment.
            ApplyHeavyWeaponHit(
                WeaponFamilyTable.Rows[5][0],
                HeavyFatalMode.GSpecial,
                spawnBlood:false);
        }

        void DischargeF2(DC1WeaponRow w)
        {
            // Native sub_679F4 re-issues the huoyan 0->5 one-shot on this fire
            // tick, so the held huoyan phase clock restarts here (Task027L.4.8).
            f2HuoyanCycleSeconds=0f;

            if(f2FlameVisual==null)
                // Task027L.4.10: native huoyan flame draw key = 13
                // (effectClassiPad+0x5C, set in sub_679F4). Mapped through the
                // shared NativeDrawOrder band translation -> above the dino
                // body, above the burn FX and above the blood/hit band.
                f2FlameVisual=F2FlameVisual.Create(
                    data,playerVisual,player,NativeDrawOrder(13));
            f2FlameVisual.BeginOrRearmHeld();

            if(attackF2Source!=null && attackF2Clip!=null)
            {
                // Task027K4.4 source closure:
                // AudioManager::Play has no dynamic same-clip busy gate for
                // Dino Cap's <=8 s F2 clips. Every native 0.2 s Attack_F2
                // request succeeds, so overlapping voices are source-faithful.
                attackF2Source.loop=false;
                attackF2Source.pitch=1f;
                NativeAudioSessionGate.Play(attackF2Source,attackF2Clip);
                F2DebugTelemetry.Log(
                    "ATTACK_F2_PLAY",
                    "overlap; frame3; ammo="+familyAmmo[5]);
            }

            SweepF2Targets(w);
            status="F2: -1 ammo; forward head-X 0.."+w.atkDis.ToString("0")+".";
            if(familyAmmo[5]<=0) EndF2Fire(true);
        }

        // TASK027N.2.35 Part L: F2 remains a single-target BRIDGE (native
        // F2 multi-target semantics are explicitly UNRESOLVED per N.2.34 -
        // this task does not invent them). `f2Target` now tracks WHICH
        // DinoInstance the flame is currently affecting; BeginF2Burn/
        // EndF2Burn/UpdateF2BurningEnemy are completely UNCHANGED and simply
        // read/write whichever instance `current` points at for the
        // duration of each call.
        void SweepF2Targets(DC1WeaponRow w)
        {
            var prevCurrent=current;

            // Re-check the EXISTING target first (if any) - a target that
            // walked out of range or died must be released before a new one
            // can be acquired, exactly matching the pre-migration single-
            // dino in-range/out-of-range toggle.
            if(f2Target!=null && !f2Target.markedForRemoval)
            {
                current=f2Target;
                bool stillInRange = enemyHP>0f && enemy!=null && enemy.gameObject.activeSelf &&
                    InF2Range(w,out _);
                if(!stillInRange)
                {
                    EndF2Burn();
                    f2Target=null;
                }
            }
            else
            {
                f2Target=null;
            }

            if(f2Target==null)
            {
                DinoInstance candidate=FindClosestEligibleDino(w.atkDis,out _,out _);
                if(candidate!=null)
                {
                    f2Target=candidate;
                    current=candidate;
                    if(!enemyBurningF2)
                        BeginF2Burn();
                }
            }

            current=prevCurrent;
        }

        // Shared in-range test for the CURRENT `current` dino against F2's
        // own front-cone/range geometry - factored out of SweepF2Targets so
        // the re-check and the acquisition path use IDENTICAL geometry.
        bool InF2Range(DC1WeaponRow w,out float dx)
        {
            Vector3 head=enemy.position;
            enemyVisual.TryGetPartWorldAnchor("tou",out head);
            dx=playerVisual.facingRight
                ? head.x-player.position.x
                : player.position.x-head.x;
            return dx>0f && dx<w.atkDis;
        }

        // Same shape as InF2Range, for F3's own front-cone geometry.
        bool InF3Cone(DC1WeaponRow w)
        {
            Vector3 head=enemy.position;
            enemyVisual.TryGetPartWorldAnchor("tou",out head);
            float dx=playerVisual.facingRight
                ? head.x-player.position.x
                : player.position.x-head.x;
            return dx>0f && dx<w.atkDis;
        }

        void BeginF2Burn()
        {
            if(enemyHP<=0f || enemyBurningF2 || enemyF2BurnDeath) return;
            enemyF2Residual=false;
            enemyBurningF2=true;
            // V30.8: prime the shipped-cadence clock so the first Audio_att3
            // fires on the first frame-3 entry, and clear any stale grace.
            lastF2ShaoEventTime = Time.time - NativeShaoLoopSeconds;
            f2PostReleaseGrace = 0f;
            f2HuoyanCycleSeconds = 0f;
            enemyVisual.drawOffset=Vector2.zero;
            enemyAttacking=false;
            enemyHeavyReaction=false;
            enemyHitThisCycle=false;
            enemyVelocityX=0f;
            enemyVisual.secondary.Stop();
            enemyVisual.primary.Play(
                enemyVisual.FindClip(enemyClipPrefix+"_shao"),0);

            if(f2BigFireVisual==null)
                // Task027L.4.15: the engulfing Dahuo (native effect+0x5C = 9,
                // sub_12884 @ 0x12a92) renders BEHIND the whole dinosaur on iPad
                // - native key 9 < the dino body's default key (> 9). Place it
                // one clear slot below the body's lowest part. (L4.10's
                // NativeDrawOrder(9) = 216 wrongly lifted it ABOVE the body and
                // the two small flames.) The 2 small attached flames are NOT
                // handled here - they are authored last-parts (xiaohuo1/xiaohuo2)
                // of the k*_shao clip and already draw at baseSortingOrder+12/13.
                f2BigFireVisual=F2EnemyBigFireVisual.Create(
                    data,enemyVisual.baseSortingOrder-NativeEngulfingBurnBelowBody);
            // Native small species:
            // living root = groundBaseline + 240
            // Dahuo      = groundBaseline + 192
            // => relative to the living root: -48 source units; species4 bypasses this write.
            f2BigFireVisual.BeginNative(enemy,enemyVisual,(DinoFatalDeathCore.Species)(int)debugDinoKind);
            // Audio_att3 is NOT owned by burn entry. It is owned by the
            // marked frame-3 callback above.

            // TASK027N.1.2 Part Q: F2 is a proven bite-override (native
            // sub_10554 case 5 early-returns for its tier; reaction-state 5
            // suspends the AI in sub_FD58). enemyAttacking was just forced
            // false above, so this DOES interrupt a committed bite.
            DebugRecordHit("F2",0f,true,false,true,"F2_BURN(5)");
        }

        void UpdateF2BurningEnemy()
        {
            enemyVelocityX=0f;
            if((f2FlameVisual==null || !f2FlameVisual.gameObject.activeSelf) && enemyHP>0f)
                EndF2Burn();
        }

        void EndF2Burn()
        {
            if(!enemyBurningF2) return;
            enemyBurningF2=false;

            if(f2BigFireVisual!=null) f2BigFireVisual.StopNow();
            if(enemyHP<=0f) return;

            // Native sub_12C64 state 5 -> 6:
            // replay the normally-looping k*_shao container ONCE, then return
            // to normal movement. PlayRange is the Unity adapter for native
            // loopCount=0 and prevents the V30.6E indefinite-burn regression.
            enemyF2Residual=true;
            enemyAttacking=false;
            enemyHeavyReaction=false;
            enemyVelocityX=0f;
            enemyVisual.drawOffset=Vector2.zero;

            DCAnimation shaoClip=
                enemyVisual.FindClip(enemyClipPrefix+"_shao");
            if(shaoClip!=null && shaoClip.frames!=null && shaoClip.frames.Length>0)
                enemyVisual.primary.PlayRange(
                    shaoClip,0,shaoClip.frames.Length-1);
            else
                enemyF2Residual=false;
        }

        void BeginF2BurnDeath()
        {
            if(enemyF2BurnDeath) return;
            enemyBurningF2=false;
            enemyF2Residual=false;
            enemyF2BurnDeath=true;
            enemyAttacking=false;
            enemyHeavyReaction=false;
            enemyVelocityX=0f;
            enemyVisual.secondary.Stop();

            // TASK027N.2.49R REOPEN Phase D: the former "Task027L.4.2:
            // native sub_10294 state-3 branch plays Audio_death3Audio when
            // F2 burn becomes lethal" claim here is SUPERSEDED - fresh IDA
            // this reopening exhaustively disassembled sub_118E8 (the
            // shared F2/F3 ash-death function, zero audio calls) and
            // checked shaosi.a2b (the clip it attaches) against the
            // independent Task018 EventMarkBounds ledger: it carries NO
            // authored event mark at all, so the sub_10294-case-3 mechanism
            // that drives Audio_death3 for Standard/GSpecial (die.a2b/
            // die2.a2b, which DO carry that mark) cannot fire here. F2/F3
            // Ash-Death has NO native death SFX of any kind - the Audio_att3
            // sizzle that was already playing while the dino was alive and
            // burning simply has no further trigger once it dies. See
            // dino_death_sfx_source_truth_n249r.json. Per CLAUDE.md 4.9 this
            // is preserved as a superseded interpretation, not deleted.

            money=Mathf.Min(999999,money+(int)enemySourceReward);
            PlaySharedAshDeath();
            status="F2 fatal: authored ShaoSi ash/soot + two gutou1 bones.";
        }

        // Task027M.2: the native shared special-death visual (sub_118E8 -
        // 'ShaoSiAnim' swap + species scale + reposition). Called EXACT-shared
        // by F2 (sub_10B90 type-5 tier-1) and F3 (tier-2) - no tier branch in
        // sub_118E8. F2-specific bits (Audio_death3, reward, state flags) stay
        // in BeginF2BurnDeath / BeginF3Death.
        void PlaySharedAshDeath()
        {
            enemyAttacking=false;
            enemyHeavyReaction=false;
            enemyVelocityX=0f;
            // TASK027N.2.49R Phase 5 remainder: delegates to the shared
            // Combat/DinoFatalDeathCore.cs implementation (same values,
            // now the single source of truth CityGridController's own F2
            // burn-death also consumes - see shared_ash_death_contract_n249r.json).
            DinoFatalDeathCore.PlaySharedAshDeath(enemyVisual,(DinoFatalDeathCore.Species)(int)debugDinoKind);
        }

        // ============================================================
        // Task027M.2 - F3 (jiguang / laser) runtime. Parallel subsystem;
        // reuses source-shared primitives (fire-session shape, front-X cone,
        // frame-3 event, ShaoSi ash death) without touching F2 code paths.
        // ============================================================

        void PlayF3InitialFire()
        {
            // Native sub_667B0 case 5 (tier 2): attach the player fire-anim at
            // frame 0, 0..4. Identical shape to PlayF2InitialFire.
            f3SessionActive=true;
            playerVisual.secondary.PlayRange(
                playerVisual.FindClip("p_fire_F3"),0,4);
        }

        void PlayF3HeldCycle()
        {
            // Native sub_663A0 F-path: restart fire frames 3..4 @ 0.2 s; each restart seeds 3*0.2 so a cycle is one tick + 0.2 s (~0.217 s).
            // Frame 3 -> OnPlayerSecondaryFrame -> DischargeF3.
            playerVisual.secondary.PlayRange(
                playerVisual.FindClip("p_fire_F3_hold"),3,4);
        }

        float F3Damage()
        {
            // Task027M.2 Part A - native sub_10554 type-5 branch, executable.
            // v18 = dword_21BE18[5] (F upgrade level). level 0 -> Random[row].
            // level L>=1 -> Random[3*(L+2), 3*(L+2) + (L+2<10 ? 3 : 2)].
            DC1WeaponRow row=WeaponFamilyTable.Rows[5][2];   // F3 row: 3..6
            int L=f3UpgradeLevel;
            if(L<1)
                return UnityEngine.Random.Range(row.atkMin,row.atkMax);
            int b=3*(L+2);
            return UnityEngine.Random.Range((float)b, b + ((L+2<10) ? 3f : 2f));
        }

        void DischargeF3(DC1WeaponRow w)
        {
            // 1) beam re-position (persistent effect; created once).
            if(f3LaserVisual==null)
                f3LaserVisual=F3LaserVisual.Create(
                    data,playerVisual,player,NativeDrawOrder(13));
            f3LaserVisual.BeginOrRearmHeld();

            // 2) Attack_F3 request. Native sub_67CE8 fires
            //    sub_7EB04("Attack_F3Audio") once per call, and sub_66CE8 calls
            //    sub_67CE8 on each MARKED playerFireAnimation event. 7D81C
            //    marks only3;76A20 never emits4. The3<->4 hold loop uses
            //    0.20 s/frame, so steady requests occur about every0.40 s.
            //    Fresh overlapping voice per request (NativeAudioSessionGate);
            //    NO dino-side audio.
            if(attackF3Source!=null && attackF3Clip!=null)
                NativeAudioSessionGate.Play(attackF3Source,attackF3Clip);

            // 3) native shared cone (sub_68A8C): front-X, 0 < dx < ATK_DIS,
            //    NO Y band, NO occlusion, per N.2.34's own citation
            //    conceptually "ALL qualifying dinos" - but N.2.34 explicitly
            //    left F3's true simultaneous-multi-target electrocution/
            //    X-ray presentation UNRESOLVED (only one beam/laser visual
            //    exists), and this task's own Part L instruction is "no
            //    speculative beam model". TASK027N.2.35 therefore keeps F3 a
            //    single-target BRIDGE, same shape as F2 (Part L): `f3Target`
            //    tracks the one affected DinoInstance, re-checked for
            //    continued eligibility before a new closest-eligible target
            //    can be acquired. This is a DOCUMENTED LIMITATION, not an
            //    invented mechanic - see f2f3_collection_bridge_n235.json.
            var f3PrevCurrent=current;
            if(f3Target!=null && !f3Target.markedForRemoval)
            {
                current=f3Target;
                bool stillInCone = enemyHP>0f && enemy!=null && enemy.gameObject.activeSelf &&
                    InF3Cone(w);
                if(!stillInCone && enemyF3State && enemyHP>0f)
                {
                    EndF3();   // native sub_68A8C else-branch -> sub_12C64
                    f3Target=null;
                }
                else if(!stillInCone)
                {
                    f3Target=null;
                }
            }
            else
            {
                f3Target=null;
            }

            if(f3Target==null)
            {
                DinoInstance candidate=FindClosestEligibleDino(w.atkDis,out _,out _);
                if(candidate!=null)
                {
                    current=candidate;
                    if(!enemyF3State && !enemyF3Death)
                    {
                        f3Target=candidate;
                        BeginF3();
                    }
                }
            }
            current=f3PrevCurrent;

            status="F3: -1 ammo; laser cone 0.."+w.atkDis.ToString("0")+
                "; DoT every "+NativeBeFrame3Interval.ToString("0.###")+" s.";
            if(familyAmmo[5]<=0) EndF3Fire(true);
        }

        void BeginF3()
        {
            // Native sub_12884 v22==2: swap the dino primary to species BEAnim
            // (k*_be), state 4, ZERO velocity, interrupt chase/attack.
            // NO normal hurt audio, NO Audio_att3, NO Dahuo child.
            enemyF3State=true;
            enemyF3Recover=false;
            enemyF3TailDeferred=false;
            enemyF3ReactionStartTime=Time.time;   // Diagnostic reaction-entry timestamp; no lifetime gate.
            enemyAttacking=false;
            enemyHeavyReaction=false;
            enemyHitThisCycle=false;
            enemyVelocityX=0f;
            enemyVisual.drawOffset=Vector2.zero;
            enemyVisual.secondary.Stop();
            // Task027M.8.3: deterministic X-ray phase init - HURT pose first,
            // then it alternates to the k*gt* skeleton on each BE frame-3 event.
            f3XrayVisualPhase=false;
            f3XrayPhaseCounter=0;
            enemyVisual.xraySkeleton=false;
            enemyVisual.xrayFlash=false;
            // sub_76980(dino,0,0,0.025,last=4) - play BEAnim 0..4 once.
            enemyVisual.primary.PlayRange(
                enemyVisual.FindClip(enemyClipPrefix+"_be"),0,4);

            // TASK027N.1.2 Part Q: F3 is a proven bite-override (native
            // sub_10554 case 5 early-returns for its tier; reaction-state 4
            // suspends the AI in sub_FD58). enemyAttacking was just forced
            // false above, so this DOES interrupt a committed bite.
            DebugRecordHit("F3",0f,true,false,true,"F3_ELEC(4)");
        }

        void UpdateF3StateEnemy()
        {
            enemyVelocityX=0f;
            if(f3LaserVisual==null || !f3LaserVisual.gameObject.activeSelf) { EndF3();return; }
            // Native sub_101A0 case 3: when the BEAnim reaches frame 4 (clip
            // completion) restart at frame 2 -> a deterministic 2..4 ping-pong.
            // Steady k*_be frame-3 recurrence = 0.1 s (Part B). The DoT is on
            // the frame-3 EnterFrame event; no independent timer.
            if(!enemyVisual.primary.Active)
                enemyVisual.primary.PlayRange(
                    enemyVisual.FindClip(enemyClipPrefix+"_be"),2,4);
        }

        void EndF3()
        {
            // Native sub_12C64 (state 4): play BEAnim from frame 4 to end
            // (recovery tail), dino state 4 -> 6 -> 8 -> normal. NO residual
            // DoT, NO residual FX, NO Audio_att3.
            if(!enemyF3State) return;
            enemyF3State=false;
            enemyF3Recover=true;
            enemyVelocityX=0f;
            enemyVisual.xrayFlash=false;
            DCAnimation be=enemyVisual.FindClip(enemyClipPrefix+"_be");

            //N253 fresh12C64 state4 immediately resumes4..end, without a floor.
            // Real recovery tail (k*_be 4..7) = the hurt/recovery pose only.
            enemyF3TailDeferred=false;
            f3XrayVisualPhase=false;
            enemyVisual.xraySkeleton=false;
            enemyVisual.primary.PlayRange(be,4,be.frames.Length-1);
        }

        void BeginF3Death()
        {
            // Native sub_10554 HP<=0 -> sub_10B90 (type 5, tier 2) -> sub_118E8:
            // the SAME ash/bones path as F2. Route straight in; DO NOT pass
            // through the F2 residual state.
            if(enemyF3Death) return;
            enemyF3State=false;
            enemyF3Recover=false;
            enemyF3TailDeferred=false;
            enemyF3Death=true;
            f3XrayVisualPhase=false;
            enemyVisual.xraySkeleton=false;   // Task027M.8.3: no X-ray on the ash-death corpse (cleared before ash transition)
            enemyVisual.xrayFlash=false;
            PlaySharedAshDeath();
            status="F3 fatal: shared ShaoSi ash/soot + two gutou1 bones (sub_118E8).";
        }

        void EndF3Fire(bool clearTarget)
        {
            // Immediate interrupt owner (melee / weapon switch / zero-ammo /
            // hurt / death). Mirrors EndF2Fire's shape.
            f3SessionActive=false;
            playerVisual.secondary.Stop();
            if(f3LaserVisual!=null) f3LaserVisual.EndHeld(false);
            EndAttachedFStates(false); //66310 ->12F28 completes every living state4 target.
            faceResetAt=Time.time+2f;
        }

        void ResetF3State()
        {
            f3SessionActive=false;
            f3ReleasePending=false;
            enemyF3State=false;
            enemyF3Recover=false;
            enemyF3TailDeferred=false;
            enemyF3Death=false;
            f3XrayVisualPhase=false;
            f3XrayPhaseCounter=0;
            // Task027M.8.1: arena reset / despawn / weapon-switch cleanup -
            // immediately restore the EXACT original materials (not just clear
            // the flag) so no X-ray material can leak to a reused dino.
            if(enemyVisual!=null) enemyVisual.ForceRestoreMaterials();
            if(f3LaserVisual!=null) f3LaserVisual.EndHeld(false);
        }

        void StartF2Sizzle()
        {
            if(burnSizzleSource==null || burnSizzleClip==null)
                return;

            // Task027K4.4 source closure supersedes K4.1/K4.2's inferred
            // isPlaying gate. Audio_att3 uses the same native AudioManager::Play
            // mechanism as Attack_F2; for this <=8 s clip every qualifying
            // request succeeds and may overlap the previous voice.
            burnSizzleSource.loop=false;
            burnSizzleSource.pitch=1f;
            NativeAudioSessionGate.Play(burnSizzleSource,burnSizzleClip);
            F2DebugTelemetry.Log(
                "AUDIO_ATT3_PLAY",
                "overlap; enemyHP="+enemyHP.ToString("0.00"));
        }

        void StopF2Sizzle(bool hardTeardown=false)
        {
            // Native traced F2 paths do not explicitly stop Audio_att3.
            // Once state 5 ends, marked-frame requests cease and existing
            // one-shot voices decay naturally.
            if(!hardTeardown)
                return;

            NativeAudioSessionGate.StopAll();
            F2DebugTelemetry.Log("AUDIO_NATIVE_GATE_HARD_STOP","");
        }

        void DischargeE(DC1WeaponRow w)
        {
            // qh launch flash: E0 yyj5 / E1 com1, native y factor 0.25.
            Vector3 muzzle;
            float rot;
            Vector2 sz;
            string socket=GetFireSocket();
            if(playerVisual.TryGetPartWorldSocket(
                socket,0.5f,0.25f,out muzzle,out rot,out sz))
            {
                WeaponEffectAnimation.Spawn(
                    data,
                    "qhEffectAnimation",
                    muzzle,
                    rot,
                    playerVisual.facingRight,
                    1f,
                    playerVisual.baseSortingOrder+9);
            }

            Vector3 blast=player.position+
                Vector3.right*(playerVisual.facingRight ? 230f : -230f);

            // Source: baozha z=12, E suirou z=11. They must share the
            // same Unity sorting basis or their native z relation is destroyed.
            // TASK027N.2.35: every DinoInstance shares the SAME constant
            // baseSortingOrder (200, set once in SpawnDino) - use the
            // literal here rather than dereferencing a specific (possibly
            // absent) target's own renderer, since this blast VFX spawns
            // before any target is selected/may land with zero dinos alive.
            const int DinoBaseSortingOrder=200;
            WeaponEffectAnimation.Spawn(
                data,
                "baozhaAnim",
                blast,
                0f,
                playerVisual.facingRight,
                1f,
                DinoBaseSortingOrder+12);

            // TASK027N.2.35 Part K: E generalizes to an AoE hit against
            // EVERY active living dino within the existing, unchanged +/-280
            // blast radius (not just one) - preserving the exact tuning
            // (ballistic blood 3, attached daoxue only on Type3) via the
            // completely unmodified ApplyHeavyWeaponHit body, called once
            // per qualifying dino.
            var ePrevCurrent=current;
            foreach(var d in dinos.ToArray())
            {
                if(d.markedForRemoval) continue;
                current=d;
                if(enemyHP>0f && Mathf.Abs(enemy.position.x-blast.x)<280f)
                    ApplyHeavyWeaponHit(w,HeavyFatalMode.EExplosion);
            }
            current=ePrevCurrent;

            // Native67458 emits Attack_E2 after the blast scan, for both tiers.
            if(abcdFireSource!=null && attackE2Clip!=null)
                NativeAudioSessionGate.Play(abcdFireSource,attackE2Clip);

            status=CurrentWeaponName()+
                " blast at X="+blast.x.ToString("0.0")+
                " AoE +/-280 (all qualifying dinos).";

            // E is semi-auto. Ammo is already decremented at frame 3.
        }

        enum HeavyFatalMode
        {
            Standard,
            EExplosion,
            GSpecial
        }

        // TASK027N.2 Part E: spawnBlood defaults true (E and G-melee, the
        // callers weapon_blood_matrix_n2.json proves get ordinary blood).
        // TryF1ProjectileContact is the ONE caller that must pass false:
        // it shares this function's GSpecial fatalMode with G3/G4 melee
        // (see HeavyFamilyLabel's "G/F1" label), but F1's OWN blood is
        // PROVEN false (sub_11124 is never called for reaction-type case 5)
        // - fatalMode alone cannot distinguish F1 from G3/G4 here, so this
        // is a separate, explicit parameter rather than inferred from mode.
        void ApplyHeavyWeaponHit(DC1WeaponRow w,HeavyFatalMode fatalMode,bool spawnBlood=true)
        {
            if(enemyHP<=0f)
                return;

            float dmg=UnityEngine.Random.Range(w.atkMin,w.atkMax);
            enemyHP-=dmg;

            if(enemyHP<=0f)
            {
                enemyHP=0f;
                money=Mathf.Min(
                    999999,
                    money+(int)enemySourceReward);

                if(fatalMode==HeavyFatalMode.EExplosion)
                    BeginEWeaponDeath();
                else if(fatalMode==HeavyFatalMode.GSpecial)
                    BeginGSpecialFatalDeath();
                else
                    BeginStandardEnemyDeath();

                return;
            }

            // TASK027N.2 Part E: native sub_10554 calls the blood spawner
            // (sub_11124) from reaction-type cases 4 (E) and 6 (G) for EVERY
            // nonlethal hit, regardless of which of the three sub-branches
            // below is subsequently taken (Type3-light / bite-committed-
            // light-only / full-heavy-stagger) - proven in
            // weapon_blood_matrix_n2.json. Placed once, before the branches
            // diverge, so it is not duplicated per-branch. Gated on
            // spawnBlood (see the parameter's doc comment above) so F1's
            // shared call path is correctly excluded.
            DinoNonFatalBloodCore.SpawnBallistic(enemyVisual,spawnBlood);

            // TASK027N.2.9: Blood Impact Burst (native sub_10ECC), the LAST
            // side effect of native's non-lethal reaction dispatch - placed
            // here (before the branches diverge) for the same reason as the
            // blood loop above: it fires uniformly regardless of which of
            // the three sub-branches below is subsequently taken. Gating
            // (ARM-verified, g/e_family_blood_burst_matrix_n29.json):
            //  - F1 (spawnBlood=false, shares GSpecial with G3/G4): checked
            //    FIRST so it can't be misread as G3/G4. TASK027N.2.11
            //    flagged (LIKELY, not PROVEN) that F1 might actually take
            //    sub_10554 case 5 (dx/melee-visible), not qx, based on a
            //    2-data-point case-number==weapon.prop-type-number pattern;
            //    the user was asked and chose to leave gating unchanged
            //    pending a literal-value read. TASK027N.2.13 OBTAINED that
            //    proof: a direct instruction-level trace (not decompiler
            //    text, not pattern inference) of weapon.prop's loader found
            //    the raw XML attribute text from "<Weapon type=\"5\">"
            //    (weapon.prop line 120, the F1/F2/F3 family block) is copied
            //    verbatim into F1's own runtime weaponClass field -
            //    F1_LITERAL_VALUE=5, PROVEN. This lands in sub_10554's own
            //    case 5 (already fully enumerated in N.2.11: no
            //    ballistic-blood call, matching F1's own already-proven
            //    zero-blood signature; falls through to sub_10ECC(a1,1)).
            //    F1's burst is therefore PROVEN dx/melee-VISIBLE (the same
            //    daoxue sprite G-tier shows), not qx - CORRECTING N.2.9/
            //    N.2.10/N.2.11's own qx/ranged-variant assumption. See
            //    f1_literal_weaponclass_n213.json /
            //    f1_unity_routing_decision_n213.json. A-D (a separate
            //    function, DischargeCurrentWeapon) were independently
            //    PROVEN weaponClass 0/1/2/3 respectively by the same
            //    instruction-level trace - all confirmed correctly qx,
            //    unchanged.
            //  - E (fatalMode==EExplosion, spawnBlood=true): dx/melee-
            //    visible variant ONLY on Type3 species (with Type3's fixed-
            //    offset+facing-flip override); NO burst at all on
            //    Type1/2/4 - native structurally never reaches sub_10ECC on
            //    that branch, unlike E's own ballistic blood which fires on
            //    both species branches.
            //  - G1-G4 (fatalMode==Standard or GSpecial, spawnBlood=true):
            //    ALWAYS the dx/melee-visible variant, both species branches
            //    identically (Type3 gets the same fixed-offset override as
            //    E-Type3; non-Type3 gets the general randomized offset).
            if(!spawnBlood)
            {
                // TASK027N.2.13: F1 PROVEN case5/meleeFlag=1 - dx/melee-
                // visible, same call shape as G-tier (below), including the
                // Type3 override (that check lives inside native sub_10ECC
                // itself, gated on the TARGET's species, not on which
                // sub_10554 case triggered the call).
                DinoCapBloodBurst.Spawn(
                    enemyVisual,data,true,
                    debugDinoKind==DebugDinoKind.K2Trex,
                    enemyVisual.baseSortingOrder+65);
            }
            else if(fatalMode==HeavyFatalMode.EExplosion)
            {
                if(debugDinoKind==DebugDinoKind.K2Trex)
                    DinoCapBloodBurst.Spawn(
                        enemyVisual,data,true,true,
                        enemyVisual.baseSortingOrder+65);
            }
            else
            {
                DinoCapBloodBurst.Spawn(
                    enemyVisual,data,true,
                    debugDinoKind==DebugDinoKind.K2Trex,
                    enemyVisual.baseSortingOrder+65);
            }

            // TASK027N.1.3 CORRECTION to N.1.2: sub_10554's heavy branches
            // (case 4 E-explosive, case 6 G-melee) gate on *(dino+0xBC), which
            // this task's native re-trace (sub_57BD8 `v19[47]=v80`, corroborated
            // by sub_1141C's decapitation switch on the SAME field) PROVES is
            // the dino's permanent SPECIES-TYPE byte, set once at spawn - NOT a
            // transient bite-state debounce as N.1.2 guessed. `if (*(dino+0xBC)
            // != 3) HEAVY else LIGHT`, so Type 3 (species==3) NEVER takes the
            // heavy branch, in or out of a bite: it is a permanent, explicit
            // per-species exclusion, not a bite-state artifact. Every OTHER
            // species (1,2, and 4 via the Jungle spawner's own species write)
            // takes the heavy branch normally when not attack-committed.
            // TASK027N.2.49R Phase 2B: the THREE-WAY reaction DECISION below
            // (Type3-light / bite-committed-damage-only / heavy-stagger) is
            // now computed via the shared DinoHeavyHitCore.ApplyNonFatalHeavyReaction
            // (Combat/DinoHeavyHitCore.cs) so City Grind's own future heavy-
            // hit reaction consumes the IDENTICAL decision, not a copy. Every
            // per-branch SIDE EFFECT below (BeginLightBefired/DebugRecordHit/
            // status text/enemyVisual calls) is byte-for-byte unchanged from
            // before this refactor - only the branch CONDITION and the
            // +/-192 magic number moved into the shared function.
            // N255 source10554 case5 (F1) never enters the primary heavy
            // reaction. Retain its dx burst and state0 light recoil only.
            if(!spawnBlood)
            {
                //10554 requires state0. FF68 restores state0 for walking;
                //active primary reactions remain distinct nonzero states.
                if(!enemyAttacking && !enemyHeavyReaction && !enemyBurningF2 &&
                   !enemyF2Residual && !enemyF3State && !enemyF3Recover && !enemyF3TailDeferred)
                    BeginLightBefired();
                return;
            }
            var heavyReactionDecision = DinoHeavyHitCore.ApplyNonFatalHeavyReaction(
                debugDinoKind==DebugDinoKind.K2Trex,
                !spawnBlood && EnemyBiteCommitted(),
                enemyFacingRight);

            if(heavyReactionDecision.kind==DinoHeavyHitCore.ReactionKind.Type3Light)
            {
                // Type 3: always the demoted light sub-branch. Its own inner
                // gate (`if(!*(dino+0xAC))`) is the same one BeginLightBefired
                // already models via EnemyBiteCommitted/enemyAttacking, so no
                // separate visible-knockback constant is introduced here - only
                // ordinary A/B/C/D's existing tiny, source-proven recoil shape.
                if(!EnemyBiteCommitted())
                    BeginLightBefired();
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
                DebugRecordHit(HeavyFamilyLabel(fatalMode),dmg,!EnemyBiteCommitted(),false,false,
                    EnemyBiteCommitted()?"ATTACK(1)":"LIGHT(species-3 exclusion)");
#endif
                status="Heavy hit "+dmg.ToString("0.00")+
                    " on Type 3: species-permanent no-knockback exclusion "+
                    "(native dino+0xBC==3 always routes to the light branch).";
                return;
            }

            // TASK027N.1.2 claimed the light-branch's `if(dino+0xAC==0)` gate
            // ALSO applied to the heavy branch (E-explosive case4/G-melee
            // case6), so an ordinary heavy hit during a committed bite would
            // land as damage-only with no knockback/behited/cancel. TASK027
            // N.2.21 built on that claim uniformly for ALL species. Both were
            // WRONG for non-Type3 species: TASK027N.2.22 fully re-decompiled
            // sub_10554's case4/case6 bodies (not just cited N.1.2's own
            // summary) and found the non-Type3 branch of BOTH cases calls
            // sub_7677C(...,"BehitedAnim",...) and the knockback-velocity
            // setter (sub_173A98) UNCONDITIONALLY - there is NO read of
            // dino+0xAC (*(a1+172)) anywhere in either case's non-Type3 body.
            // That gate exists ONLY in the true light-family cases (default/
            // case1 for A-D/B, and case5 for F1, all of which check
            // `if(!*(a1+172))` before re-triggering BefiredAnim) - it was
            // never part of the heavy/melee reaction at all. This is
            // independently corroborated by TWO already-proven prior-task
            // findings this task cites rather than re-derives:
            // native_melee_vs_bite_end_arbitration_n2.json (TASK027N.2 Part
            // O: sub_668FC's own melee-eligibility check and sub_10554's own
            // damage-application have NO reaction-state gate at all, aside
            // from "already dead") and melee_exact_bite_boundary_matrix_n22.
            // json (TASK027N.2.2: live PlayMode measurement across a -3..+3
            // frame sweep found Type1/2/4 ALWAYS successfully land melee
            // damage regardless of timing relative to the bite; only Type3
            // shows a real miss window, and it is a lunge-animation POSITION/
            // range artifact, not a discrete state gate - see
            // bite_commit_timeline_n222.json / melee_vs_bite_native_window_
            // n222.json for the full re-derivation this task performed).
            //
            // The "early melee cancels the bite" behavior the user actually
            // observes falls out naturally from the SAME unconditional
            // BehitedAnim swap below: it replaces the dino's active primary
            // clip, so if it lands before the "_attack" clip's own frame-12
            // damage-to-player event (OnEnemyPrimaryFrame, Task024B, frozen,
            // untouched by this task) that event never fires on the
            // now-replaced clip - the bite's OWN damage to the player is
            // cancelled as a side effect, not via any new mechanic. Landing
            // after frame 12 does not undo already-dealt player damage, but
            // still fully interrupts/staggers the dino exactly the same way.
            //
            // Type 3's OWN species-permanent exclusion (dino+0xBC==3, handled
            // above, unconditional regardless of bite state) is UNCHANGED -
            // it was never based on the (now-removed) dino+0xAC check either.
            //
            // F1 is DELIBERATELY EXCLUDED from this correction and keeps its
            // PRE-EXISTING bite gate below. Native's own case5 (F1) does NOT
            // share case4/case6's unconditional BehitedAnim pattern - it uses
            // the SAME light-style `if(!*(a1+172))` gate as the true light
            // family (default/case1), never calling BehitedAnim or the
            // +/-192 knockback at all. This task's own user correction and
            // FREEZE list name only "TYPE1/2/4 MELEE" (i.e. G-family) and
            // "F1 daoxue" is explicitly frozen - F1's reaction-shape mismatch
            // with its OWN native case5 (a separate, pre-existing, narrower
            // issue this task's native re-trace happened to surface) is
            // intentionally left untouched to avoid unrequested scope creep;
            // see report.md's Unresolved Questions for this residual lead.
            if(heavyReactionDecision.kind==DinoHeavyHitCore.ReactionKind.BiteCommittedDamageOnly)
            {
                enemyVisual.secondary.Stop();
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
                DebugRecordHit(HeavyFamilyLabel(fatalMode),dmg,false,false,false,"ATTACK(1)");
#endif
                status="Heavy hit "+dmg.ToString("0.00")+
                    " during committed bite: damage only (F1's own pre-"+
                    "existing, out-of-scope bite gate - see report.md).";
                return;
            }

            enemyAttacking=false;
            enemyHeavyReaction=true;
            enemyVisual.secondary.Stop();

            // Native sub_10554 case 6 (Task023): melee = HEAVY full-body stagger
            // "BehitedAnim" (PRIMARY) + Box2D +/-192 linear velocity backward
            // (dino facing). TASK027N.1.3 CORRECTION: this branch is reached by
            // every species EXCEPT Type 3, which is permanently excluded above
            // (native dino+0xBC==3 species check) - "no species branch" (N.1.1/
            // Task023) was true only insofar as the WEAPON-TYPE switch itself
            // has no species term; the reaction gate one level in does. K1/K3
            // (and K4 on the tiers it survives) still get the full stagger here.
            // The V30.26 "Type 3 never flinches" was ALSO a downstream effect of
            // the wrong ADIS: with ADIS 130 the k2 mouth (~123 px ahead of its
            // root) landed behind the player, so ExecuteMeleeContact's
            // `dx = head.x - player.x <= 0` gate returned before this method
            // ran at all. Fixed by ADIS 130->280 (N.1.1); the species exclusion
            // above is the separate, now-corrected reason Type 3 still shows no
            // knockback even once the ADIS/dx gate passes.
            enemyVelocityX = heavyReactionDecision.velocityX;

            enemyVisual.primary.Play(
                enemyVisual.FindClip(enemyClipPrefix+"_behited"),0);

            // TASK027N.2.22: this branch used to ALSO spawn its own 3x
            // SpawnTask026Blood loop here, duplicating the single,
            // already-unconditional loop above (before the branches
            // diverge, per its own doc comment - "placed once... not
            // duplicated per-branch"). That gave 6 blood particles per hit
            // instead of 3 for every ordinary E/G non-Type3 hit - a
            // pre-existing bug, surfaced (not introduced) by this task's
            // regression run, fixed here by simply not re-spawning blood a
            // second time in this branch.

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            DebugRecordHit(HeavyFamilyLabel(fatalMode),dmg,true,true,true,"HEAVY(2)");
#endif
            status="Heavy hit "+dmg.ToString("0.00")+
                "; Behited +/-192.";
        }

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
        static string HeavyFamilyLabel(HeavyFatalMode m)
        {
            if(m==HeavyFatalMode.EExplosion) return "E";
            if(m==HeavyFatalMode.GSpecial)  return "G/F1";
            return "G";
        }
#endif

        void BeginEWeaponDeath()
        {
            enemyAttacking=false;
            enemyHeavyReaction=false;
            enemyVelocityX=0f;
            enemyVisual.secondary.Stop();

            Vector3 body=enemy.position;
            if(!enemyVisual.TryGetPartWorldAnchor("shen",out body))body=Vector3.down*136f; //Native world0 translated into workbench actor-baseline0 space.

            //D9BC floor is fixed to this sandbox's actor baseline0 minus250.
            float floorY=-250f;

            // TASK027N.2.49R Phase 3 remainder: the 4-part suirou spec table
            // now comes from the shared DinoFatalDeathCore (Combat/) -
            // values UNCHANGED, only the table's OWNER moved.
            foreach(var part in DinoFatalDeathCore.EWeaponSuirouParts)
                WeaponSuirouParticle.Spawn(
                    enemyVisual,part.refName,body,
                    new Vector2(part.width,part.height),floorY,
                    enemyVisual.baseSortingOrder+11);

            for(int i=0;i<DinoFatalDeathCore.EWeaponBloodSpawnCount;i++)
                SpawnTask026Blood(body,floorY);

            // Task027E source type-4 uses scene "Die" rather than k?_die.
            // The sandbox has no scene-state renderer equivalent yet, so hide
            // the actor after emitting the complete proven fatal burst instead
            // of substituting a wrong k?_die animation.
            enemyVisual.primary.Stop();
            status="E fatal: exact 4 suirou + 3 blood; source scene-Die hook has no sandbox equivalent.";
        }

        void DamagePlayer(float d)
        {
            // Native guard ordering: invulnerability or states 4/5/6 drop
            // the entire hit before HP/anim/knockback/timer writes.
            if(playerDead || Time.time<invulnUntil || playerHitStun)
                return;

            if(equippedFamily==5 && familyTier[5]==1) EndF2Fire(true);
            if(equippedFamily==5 && familyTier[5]==2) EndF3Fire(true);

            // TASK027N.2 Part U: God Mode blocks ONLY the HP decrement/death
            // branch at this authoritative owner. Everything below (hurt
            // anim, playerHitStun, knockback velocity, facing, invulnUntil)
            // still runs exactly as for a normal hit - "bite lands -> normal
            // hit reaction executes -> God Mode prevents HP decrement/death
            // only", per spec. Do NOT gate any of the reaction code below on
            // godMode.
            if(!godMode)
            {
                playerHP-=d;

                if(playerHP<=0f)
                {
                    playerHP=0f;
                    playerDead=true;
                    playerHitStun=false;
                    playerMelee=false;
                    playerHurtVelocityX=0f;
                    playerVisual.secondary.Stop();
                    fireHeld=false;
                    previousFireHeld=false;
                    sustainedShotCounter=0;
                    rageSessionActive=false;
                    faceResetAt=-1f;
                    playerVisual.headRefOverride="";
                    playerInvulnColourScale=1f;
                    playerVisual.actorColourScale=1f;

                    DCAnimation die=playerVisual.FindClip("p_die");
                    if(die!=null)
                        playerVisual.primary.Play(die,0);

                    status="K1 fatal contact: player HP 0; death bypasses hurt/invulnerability.";
                    return;
                }
            }

            // Native player state transitions from MELEE(3) to HURT(5).
            // `playerMelee` is only our Unity adapter for state 3, so it must
            // be cleared before the hurt primary replaces p_hit.
            playerMelee=false;
            playerVisual.secondary.Stop();

            // TASK027N.2.23 Part J: user-observed runtime behavior ("being
            // hurt interrupts/resets the rage expression"). This ordinary
            // (non-fatal) hurt path previously left the rage-face state
            // (sustainedShotCounter/headRefOverride/faceResetAt) completely
            // untouched - a genuine gap, not a preserved native contract.
            // The fatal branch above (playerHP<=0) already hard-resets
            // headRefOverride on death; mirrored here for the ordinary hurt
            // case, also cancelling any pending decay step.
            faceResetAt=-1f;
            sustainedShotCounter=0;
            rageSessionActive=false;
            playerVisual.headRefOverride="";

            DCAnimation hit=playerVisual.FindClip(
                "p_behit_"+CurrentWeaponName());
            if(hit!=null)
            {
                playerHitStun=true;
                playerVisual.primary.Play(hit,0);
            }

            // signed = dinoX-playerX. Source tie-break: same X is "attacker right".
            // TASK027N.2.35: `enemy` can now legitimately be null (no dino
            // currently exists / `current` unset) where it never could be
            // before this migration (the old singleton was always non-null
            // once BuildWorld ran) - e.g. TestHarness_DamagePlayer calling
            // this directly to test the player-hurt reaction in isolation.
            // Falls back to the existing "same X" tie-break (attacker right)
            // rather than dereferencing a possibly-null instance.
            float signed = enemy!=null ? enemy.position.x-player.position.x : 0f;
            playerHurtVelocityX=signed>=0f ? -100f : +100f;

            // Task026 corrected full disassembly: player turns toward the attacker.
            bool shouldFaceRight=signed>=0f;
            if(playerVisual.facingRight!=shouldFaceRight)
            {
                playerVisual.facingRight=shouldFaceRight;
                cameraCatchupActive=true;
                previousPlayerFacingRight=shouldFaceRight;
            }

            invulnUntil=Time.time+1.5f;
            //Shared68148 nonfatal tail; use native FX/session admission.
            NativeAudioSessionGate.Play(meleeAudioSource,playerHurtClip);
            playerInvulnColourScale=1f;
            playerInvulnColourRate=-4f;
            playerVisual.actorColourScale=1f;

            status="Dino frame-12 contact: -"+d+
                " HP; velocity knockback="+
                playerHurtVelocityX.ToString("0")+
                "; face attacker; Wudi=1.5s.";
        }

        void UpdatePlayerInvulnerabilityVisual()
        {
            if(playerVisual==null)
                return;

            if(Time.time>=invulnUntil)
            {
                playerInvulnColourScale=1f;
                playerVisual.actorColourScale=1f;
                return;
            }

            NativePlayerDamageFlash.Tick(ref playerInvulnColourScale,ref playerInvulnColourRate,Time.deltaTime);

            playerVisual.actorColourScale=playerInvulnColourScale;
        }

        void PlaySourceGMeleeAttackAudio()
        {
            if(meleeAudioSource==null)
                return;

            AudioClip clip =
                Mathf.Clamp(meleeTier,0,3)==3
                    ? meleeAttackG4Clip
                    : meleeAttackGClip;

            if(clip!=null)
                NativeAudioSessionGate.Play(meleeAudioSource,clip);
        }

        void OnPlayerPrimaryFrame(Power2DTrack t,int frame)
        {
            if(!playerMelee || frame!=10 ||
               t.Clip==null ||
               !t.Clip.name.StartsWith("p_hit_G"))
                return;

            // Task027J2 native contract:
            // audio is tied to the frame-10 melee event, NOT hit success.
            // Therefore it plays on a miss/no-target swing as long as the
            // attack reaches frame 10. An earlier hurt interruption prevents it.
            PlaySourceGMeleeAttackAudio();
            ExecuteMeleeContact();
        }

        void BeginLightBefired()
        {
            // Native sub_10554 default / case-1 / case-5 light path is entirely
            // inside one gate: `if(dino+0xAC==0)`. That word is non-zero in BOTH
            // chase (state 8) and committed bite (state 1), so the visible
            // Befired secondary AND the small recoil impulse are skipped whenever
            // the dino is attacking - not just the animation. (TASK027N.1.2:
            // the net effect today is nil because TickEnemy's enemyAttacking
            // branch returns before enemyVelocityX is integrated and
            // CompleteEnemyAttack overwrites it, but keeping the write inside the
            // guard is source-faithful and removes a latent ordering hazard.)
            if(!enemyAttacking)
            {
                enemyVisual.secondary.Play(
                    enemyVisual.FindClip(enemyClipPrefix+"_befired"),0);

                // Light recoil = +/- 0.5 * species SPD.
                enemyVelocityX=
                    enemyFacingRight
                        ? -enemySourceSpeed*0.5f
                        : +enemySourceSpeed*0.5f;
            }
        }

        void BeginGSpecialFatalDeath()
        {
            // TASK027N.1.3: native decapitation eligibility (sub_10B90 ARM
            // trace) is WEAPON-ONLY - type==5/tier==0 (F1) and type==6/tier in
            // {2,3} (G3,G4) reach sub_1141C; it is already reached correctly
            // for every species by ExecuteMeleeContact's meleeTier>=2 test and
            // TryF1ProjectileContact's unconditional GSpecial. sub_1141C itself
            // then switches on the SPECIES byte (dino+0xBC) to pick the head
            // asset: case 1->k3toufeiFrame, case 2->k4toufeiFrame (K1Rusher;
            // Task027J4 quirk, preserved), case 3->k2toufeiFrame. There is NO
            // case 4 (Type 4/k4njz) - the switch falls through with the head
            // particle's frame-rect never configured. N255 correction: the
            // unconditional tail still plays its die2 binding and death2 audio.
            // Preserve the unassigned pooled slot rather than inventing a head asset.
            // TASK027N.2.49R Phase 3 remainder: mode-selection/per-species
            // data now comes from the shared DinoFatalDeathCore (Combat/) -
            // same DebugDinoKind/Species enum ordering (K3Blue,K1Rusher,
            // K2Trex,K4Raptor), safe direct cast. Behavior UNCHANGED, only
            // the data table's OWNER moved so CityGridController can share
            // the exact same source-truth table rather than a scene-local
            // copy.
            var presentation = DinoFatalDeathCore.GetGSpecialPresentation((DinoFatalDeathCore.Species)(int)debugDinoKind);
            if(!presentation.supported)
            {
                BeginStandardEnemyDeath();
                status="G"+(meleeTier+1)+" fatal (T"+CurrentSpecies().typeNumber+
                    "): standard death (native sub_1141C has no species-4 case - "+
                    "PROVEN_UNSUPPORTED, not invented).";
                return;
            }

            enemyAttacking=false;
            enemyHeavyReaction=false;
            enemyVelocityX=0f;
            enemyVisual.secondary.Stop();

            // Native sub_1141C samples the PRE-death head socket before
            // attaching the headless die2 container.
            Vector3 head=enemy.position;
            enemyVisual.TryGetPartWorldAnchor("tou",out head);

            float floorY=enemy.position.y-250f;

            string die2Clip=presentation.die2ClipName;
            string flyingHeadRef=presentation.flyingHeadRefName;
            Vector2 flyingHeadSize=new Vector2(presentation.flyingHeadWidth,presentation.flyingHeadHeight);
            FlyingHeadParticle.Species species=presentation.particleSpecies;

            DCAnimation die2=enemyVisual.FindClip(die2Clip);
            if(die2==null)
            {
                Debug.LogError("[DC1-V27] Missing special-fatal clip "+die2Clip+
                    ". Re-run Dev/Tools/UnityExport/export_combat_sandbox_v11.py.");
                status="Missing "+die2Clip+"; regenerate combat manifest.";
                return;
            }

            // Native ordering: severed head is spawned/configured first,
            // then DieAnim (die2) is attached, then Audio_death2 plays.
            FlyingHeadParticle.Spawn(
                enemyVisual,
                flyingHeadRef,
                head,
                flyingHeadSize,
                floorY,
                enemyFacingRight,
                species,
                enemyVisual.baseSortingOrder+11);

            enemyVisual.primary.Play(die2,0);

            if(meleeAudioSource!=null && deathAudio2Clip!=null)
                NativeAudioSessionGate.Play(meleeAudioSource,deathAudio2Clip);

            status="G"+(meleeTier+1)+
                " special fatal: "+die2Clip+
                (flyingHeadRef==null ? " + native unassigned head slot + death2." :
                " + shipped "+flyingHeadRef+" head + death2.");
        }

        void BeginStandardEnemyDeath()
        {
            enemyAttacking=false;
            enemyVelocityX=0f;
            enemyVisual.secondary.Stop();

            // Task026B: native sub_77498 reads the LAST RENDERED pre-death
            // head pivot. Capture it BEFORE selecting the die clip.
            Vector3 head=enemy.position;
            enemyVisual.TryGetPartWorldAnchor("tou",out head);

            enemyVisual.primary.Play(
                enemyVisual.FindClip(enemyClipPrefix+"_die"),0);

            // TASK027N.2.49R REOPEN Phase D: PROVEN native (fresh IDA this
            // reopening) - sub_11980 plays Audio_death1Audio at t=0 as the
            // LAST statement of its own body. Was NOT wired anywhere before
            // this change (a genuine gap, not a stale doc).
            if(meleeAudioSource!=null && deathAudio1Clip!=null)
                NativeAudioSessionGate.Play(meleeAudioSource,deathAudio1Clip);

            // Proven native particle floor relationship:
            // floor = actorRootY - 250, not Task024D FloorYAnchor.
            float floorY=enemy.position.y-250f;

            // Native standard death spawns chunks before 3 blood particles.
            SpawnTask026Chunk(
                DinoCapDamageParticle.Kind.ChunkS3,
                head,new Vector2(61f,51f),floorY);
            SpawnTask026Chunk(
                DinoCapDamageParticle.Kind.ChunkS4,
                head,new Vector2(50f,43f),floorY);

            if(debugDinoKind==DebugDinoKind.K3Blue)
            {
                // Preserve accepted K3 runtime representation unchanged.
                SpawnTask026Chunk(
                    DinoCapDamageParticle.Kind.ChunkK3S1,
                    head,new Vector2(30f,49f),floorY);
                SpawnTask026Chunk(
                    DinoCapDamageParticle.Kind.ChunkK3S2,
                    head,new Vector2(35f,33f),floorY);
            }
            else if(debugDinoKind==DebugDinoKind.K1Rusher)
            {
                // Task027H / native sub_11980 species==1:
                // K1 standard death = s3 + s4 + k4s1 + k4s2 + 3 blood.
                // k4s1/k4s2 are the authentic tan/copper skin chunks.
                SpawnTask026Chunk(
                    DinoCapDamageParticle.Kind.ChunkK4S1,
                    head,new Vector2(30f,49f),floorY);
                SpawnTask026Chunk(
                    DinoCapDamageParticle.Kind.ChunkK4S2,
                    head,new Vector2(35f,33f),floorY);
            }
            else if(debugDinoKind==DebugDinoKind.K2Trex)
            {
                SpawnTask026Chunk(DinoCapDamageParticle.Kind.ChunkK2S1,head,new Vector2(44f,72f),floorY);
                SpawnTask026Chunk(DinoCapDamageParticle.Kind.ChunkK2S2,head,new Vector2(52f,49f),floorY);
            }
            else
            {
                // Native11980 allocates these slots without species-4 frames.
                SpawnTask026Chunk(DinoCapDamageParticle.Kind.BlankChunkS1,head,Vector2.zero,floorY);
                SpawnTask026Chunk(DinoCapDamageParticle.Kind.BlankChunkS2,head,Vector2.zero,floorY);
            }

            for(int i=0;i<3;i++)
                SpawnTask026Blood(head,floorY);
        }

        // TASK027N.2.49R Part C: body moved into the shared
        // DinoCapDamageParticle.SpawnOrdinaryBlood() (byte-for-byte
        // identical randomization/constants - a pure delegate, not a
        // rewrite) so City Grind can call the same native-derived logic.
        // See DinoCapDamageParticle.cs's own TASK027N.2.7 correction note
        // for the vy=200f/+700, vx=[-400,400) provenance.
        void SpawnTask026Blood(Vector3 head,float floorY)
        {
            DinoCapDamageParticle.SpawnOrdinaryBlood(
                enemyVisual, head, floorY, enemyVisual.baseSortingOrder);
        }

        // TASK027N.2.49R Phase 3: body moved into the shared
        // DinoCapDamageParticle.SpawnDeathChunk() (byte-for-byte identical
        // randomization/constants - a pure delegate) so City Grind can call
        // the same native-derived logic. See that method's own TASK027N.2.8
        // correction note for the vx/vy/angular provenance.
        void SpawnTask026Chunk(
            DinoCapDamageParticle.Kind kind,
            Vector3 head,
            Vector2 size,
            float floorY)
        {
            DinoCapDamageParticle.SpawnDeathChunk(
                enemyVisual, kind, head, size, floorY, enemyVisual.baseSortingOrder);
        }

        // TASK027N.1: kept for the legacy digit-key / RESET-DINO callers.
        // Spawns on the current spawnSide.
        void ResetDebugDino(DebugDinoKind kind)
        {
            SpawnDino(kind, spawnSide);
        }

        void ResetCurrentDebugDino()
        {
            SpawnDino(debugDinoKind, spawnSide);
        }

        // TASK027N.2.35 Part C: manual/debug/auto/City-Grind-timer spawning
        // now ALWAYS ADDS a brand-new DinoInstance (its own GameObject +
        // Power2DSourceTruthRenderer) to the `dinos` collection - it never
        // replaces or reuses an existing one. `current` is set to the new
        // instance for the duration of this call so every existing species/
        // stat/placement/animation line below (unchanged from the pre-
        // migration single-dino version) writes through the property shims
        // onto the NEW instance specifically.
        DinoInstance SpawnDino(DebugDinoKind kind, SpawnSide side)
        {
            var inst=new DinoInstance{ id=nextDinoInstanceId++, kind=kind, side=side };
            inst.transform=new GameObject("CityDino_SourceTruth_"+inst.id).transform;
            inst.visual=inst.transform.gameObject.AddComponent<Power2DSourceTruthRenderer>();
            inst.visual.baseSortingOrder=200;
            inst.visual.Initialize(data);
            inst.visual.EnableActorComposition();
            // TASK027N.2.35 Part A: per-instance EnterFrame routing - the
            // closure captures THIS instance and swaps `current` to it for
            // the duration of the shared OnEnemyPrimaryFrame handler, then
            // restores whatever `current` was before (animation events can
            // fire from a different instance's renderer than the one the
            // per-dino Update loop was last processing).
            inst.visual.primary.EnterFrame += (t,frame) =>
            {
                debugEnemyPrimaryFrameClosureFireCount++;
                var prev=current;
                current=inst;
                OnEnemyPrimaryFrame(t,frame);
                current=prev;
            };
            dinos.Add(inst);
            current=inst;

            debugDinoKind=kind;
            DinoSpeciesDef sp=CurrentSpecies();
            enemyClipPrefix=sp.prefix;

            enemyHP=sp.hp;
            enemySourceSpeed=sp.speed;
            enemySourceAttack=sp.attack;
            enemySourceReward=DinoCapSourceProfileV11.Type1Reward;   // reward is not gameplay-critical in the sandbox
            enemyRuntimeAdis=sp.adisBase+(kind==DebugDinoKind.K4Raptor?0:
                UnityEngine.Random.Range(0,DinoCapSourceProfileV11.AdisJitterExclusiveMax));

            enemyBurningF2=false;
            enemyF2Residual=false;
            enemyF2BurnDeath=false;
            ResetF3State();   // Task027M.2 (also clears the X-ray skeleton swap)
            enemyAttacking=false;
            enemyHeavyReaction=false;
            enemyHitThisCycle=false;
            enemyVisual.actorScale=1f;
            enemyVisual.drawOffset=Vector2.zero;
            enemyVisual.secondary.Stop();
            enemy.gameObject.SetActive(true);

            // PART N - source-space placement, resolution-independent (the whole
            // sandbox runs in 1024x768 source space). Both sides use the same
            // offset so L and R are symmetric and safely inside A1 range (1024)
            // and outside ADIS.
            const float SpawnOffsetX=430f;
            float sx=side==SpawnSide.Left
                ? player.position.x-SpawnOffsetX
                : player.position.x+SpawnOffsetX;
            enemy.position=new Vector3(sx,player.position.y,0f);

            // PART D - use the existing facing mechanism. Face/chase toward the
            // player from whichever side we spawned on.
            ResignEnemyTowardPlayer();
            enemyVisual.primary.Play(
                enemyVisual.FindClip(enemyClipPrefix+"_move"),0);

            spawnSide=side;
            lastSpawnedType=sp.typeNumber;
            lastSpawnedSide=side;

            status=sp.label+" ("+(side==SpawnSide.Left?"LEFT":"RIGHT")+
                "): HP="+enemyHP.ToString("0")+
                " SPD="+enemySourceSpeed.ToString("0")+
                " ADIS="+enemyRuntimeAdis.ToString("0")+
                " ATK="+enemySourceAttack.ToString("0")+
                "  xray="+sp.xrayRig+
                "  ACTIVE="+dinos.Count+".";

            // TASK027N.2.35: deliberately do NOT restore `current` to
            // prevCurrent here - `current` is left pointing at the
            // newly-spawned instance, matching the pre-migration single-
            // dino semantics where "the enemy" always meant "whatever was
            // just spawned". This keeps every existing TestHarness_*
            // accessor and single-dino test (which reads enemyHP/enemy/etc.
            // immediately after a spawn call, outside of Update()'s own
            // per-instance loop) working unchanged. TickAllDinos() itself
            // is unaffected either way, since it explicitly sets `current`
            // for every instance it processes regardless of this value.
            return inst;
        }

        // PART M/H: safely despawn EVERY active sandbox dino - collection-
        // safe teardown (iterates a snapshot array, not the live list, so
        // nothing is mutated mid-enumeration), cleans all per-dino
        // presentation / weapon-target state and the shared player-anchored
        // F2/F3 weapon visuals, then empties the collection.
        void ClearDinos()
        {
            var prevCurrent=current;
            foreach(var d in dinos.ToArray())
            {
                current=d;
                enemyBurningF2=false;
                enemyF2Residual=false;
                enemyF2BurnDeath=false;
                ResetF3State();
                enemyAttacking=false;
                enemyHeavyReaction=false;
                enemyHitThisCycle=false;
                if(enemyVisual!=null)
                {
                    enemyVisual.ForceRestoreMaterials();   // drop any X-ray / inversion material
                    enemyVisual.xraySkeleton=false;
                    enemyVisual.secondary.Stop();
                    enemyVisual.actorScale=1f;
                    enemyVisual.drawOffset=Vector2.zero;
                }
                if(d.f2BigFireVisual!=null) d.f2BigFireVisual.StopNow();
                if(enemy!=null && enemy.gameObject!=null)
                    UnityEngine.Object.Destroy(enemy.gameObject);
                enemyHP=0f;
            }
            dinos.Clear();
            current=null;

            // Shared, player-anchored weapon visuals/session state - not
            // per-dino, torn down once regardless of how many dinos existed.
            f2ReleasePending=false;
            f2PostReleaseGrace=0f;
            f2HuoyanCycleSeconds=0f;
            StopF2Sizzle(true);
            if(f2FlameVisual!=null) f2FlameVisual.EndHeld(false);
            if(f3LaserVisual!=null) f3LaserVisual.EndHeld(false);
            f2Target=null;
            f3Target=null;

            autoPausedByCap=false;
            current=prevCurrent;
            status="CLEAR DINOS: all "+"active dinos despawned; F2/F3/X-ray/beam-target state cleared.";
        }

        // PART H/I/J/K/L - automated debug spawner. DEBUG_ONLY; never a
        // production spawn/progression system.
        void AutoSpawnTick()
        {
            if(!autoSpawn) return;
            if(Time.time<nextAutoSpawnTime) return;
            nextAutoSpawnTime=Time.time+Mathf.Max(0.25f,autoIntervalSeconds);

            // PART L - cap. Never delete a living dino to satisfy it.
            if(ActiveDinoCount>=maxActiveDinos)
            {
                autoPausedByCap=true;
                status="AUTO SPAWN PAUSED: CAP ("+ActiveDinoCount+"/"+maxActiveDinos+")";
                return;
            }
            autoPausedByCap=false;

            // PART I - type selection.
            DebugDinoKind kind;
            if(autoTypeMode==AutoTypeMode.Current)
                kind=DinoSpecies[Mathf.Clamp(manualTypeIndex,0,DinoSpecies.Length-1)].kind;
            else if(autoTypeMode==AutoTypeMode.Random)
                kind=DinoSpecies[UnityEngine.Random.Range(0,DinoSpecies.Length)].kind;
            else
            {
                kind=DinoSpecies[autoTypeCursor%DinoSpecies.Length].kind;
                autoTypeCursor=(autoTypeCursor+1)%DinoSpecies.Length;
            }

            // PART J - side selection.
            SpawnSide side;
            if(autoSideMode==AutoSideMode.Current)
                side=spawnSide;
            else if(autoSideMode==AutoSideMode.Random)
                side=UnityEngine.Random.value<0.5f ? SpawnSide.Left : SpawnSide.Right;
            else
            {
                side=autoSideCursor==0 ? SpawnSide.Left : SpawnSide.Right;
                autoSideCursor^=1;
            }

            SpawnDino(kind,side);
        }

        // TASK027N.2.31 Part B: DEBUG_ONLY. Types 1/2/3 only (DinoSpecies[0..2]
        // - index 3 is Type4, explicitly excluded per this task's own spec),
        // random type + random side, single-active-dino invariant enforced
        // by ActiveDinoCount (PROVEN always 0 or 1, see its own doc-comment)
        // - only spawns when it reads exactly 0, which SpawnDino() itself
        // immediately flips to 1 via enemy.gameObject.SetActive(true) in the
        // SAME call, making this self-correcting with no separate "pending"
        // flag needed. This also naturally satisfies "avoid spawning during
        // teardown frames where old corpse/state is still authoritative" -
        // the death/corpse sequence keeps the GameObject active (and
        // ActiveDinoCount==1) for its own full duration; only once it's
        // fully deactivated does this tick see 0 and spawn the next one.
        void AutoSpawn123Tick()
        {
            if(!autoSpawn123) return;
            if(ActiveDinoCount>0) return;

            int idx=UnityEngine.Random.Range(0,3);   // 0,1,2 -> Types 1,2,3 only
            SpawnSide side=UnityEngine.Random.value<0.5f ? SpawnSide.Left : SpawnSide.Right;
            autoSpawn123LastType=DinoSpecies[idx].typeNumber;
            autoSpawn123LastSide=side;
            SpawnDino(DinoSpecies[idx].kind,side);
        }

        // ============================================================
        // TASK027N.2.35 Parts P/Q/R - CITY GRIND TIMER SPAWNER TEST MODE.
        // DEBUG-VISIBLE test-mode toggle, NOT the full City Grind scene/
        // shop/world migration (spawn-system integration only, per the
        // task's own explicit scope). Independent Type1/2/3 timers using
        // Task002/Task027N.2.33's own PROVEN exact respawn-interval band
        // formulas and unlock gates (Type1 L>=1, Type2 L>=4, Type3 L>=8);
        // native HP/ATK level+difficulty scaling applied ONLY to dinos
        // spawned by this system (nativeCityGrindSpawn=true) - manual/debug
        // sandbox spawns are completely unaffected (Part R policy split).
        // ============================================================
        bool cityGrindSpawnerOn;
        int cityGrindLevel=1;
        int cityGrindDifficulty=1;
        string cityGrindLevelInputText="1";
        float cgT1NextSpawn=-1f, cgT2NextSpawn=-1f, cgT3NextSpawn=-1f;

        // Task002/N.2.33 city_grind_spec.json "respawn_interval" - PROVEN,
        // ARM sub_5744C 0x57540..0x57946. N = cityGrindLevel, d = difficulty.
        static float CityGrindBandType1(int N)
        {
            if(N<=20) return Mathf.Max(2.0f, 4f+(N-1)*(-0.05f)+UnityEngine.Random.value-0.5f);
            if(N<=30) return 5.0f;
            if(N<=50) return 10.0f;
            return 20.0f;
        }
        static float CityGrindBandType2(int N)
        {
            if(N<=30) return Mathf.Max(3.5f, 14f+(N-4)*(-0.3f));
            if(N<=50) return Mathf.Max(3.5f, 14f+(N-4)*(-0.4f));
            return 2.0f;
        }
        static float CityGrindBandType3(int N)
        {
            if(N<=20) return Mathf.Max(6.0f, 30f+(N-7)*(-0.067f));
            if(N<=30) return Mathf.Max(6.0f, 30f+(N-7)*(-0.25f));
            if(N<=50) return Mathf.Max(6.0f, 30f+(N-7)*(-0.4f));
            return (int)(UnityEngine.Random.value*3.0f)+6;   // random {6,7,8}
        }
        float CityGrindInterval(int typeIndex1to3,int N,int d)
        {
            float band = typeIndex1to3==1 ? CityGrindBandType1(N)
                        : typeIndex1to3==2 ? CityGrindBandType2(N)
                        : CityGrindBandType3(N);
            return band*(1f-0.2f*(d-1));
        }

        void CityGrindSpawnerTick()
        {
            if(!cityGrindSpawnerOn) return;

            int N=Mathf.Max(1,cityGrindLevel);
            int d=Mathf.Clamp(cityGrindDifficulty,1,3);

            if(cgT1NextSpawn<0f) cgT1NextSpawn=Time.time+CityGrindInterval(1,N,d);
            if(N>=4 && cgT2NextSpawn<0f) cgT2NextSpawn=Time.time+CityGrindInterval(2,N,d);
            if(N>=8 && cgT3NextSpawn<0f) cgT3NextSpawn=Time.time+CityGrindInterval(3,N,d);

            // Type1: unlocked L>=1. Independent of the other two timers and
            // NOT gated on any other dino dying first (N.2.33/N.2.34 PROVEN:
            // native timers rearm unconditionally on every fire).
            if(cgT1NextSpawn>=0f && Time.time>=cgT1NextSpawn)
            {
                SpawnCityGrindDino(0,N,d);
                cgT1NextSpawn=Time.time+CityGrindInterval(1,N,d);
            }
            if(N>=4)
            {
                if(cgT2NextSpawn<0f) cgT2NextSpawn=Time.time+CityGrindInterval(2,N,d);
                if(Time.time>=cgT2NextSpawn)
                {
                    SpawnCityGrindDino(1,N,d);
                    cgT2NextSpawn=Time.time+CityGrindInterval(2,N,d);
                }
            }
            else cgT2NextSpawn=-1f;
            if(N>=8)
            {
                if(cgT3NextSpawn<0f) cgT3NextSpawn=Time.time+CityGrindInterval(3,N,d);
                if(Time.time>=cgT3NextSpawn)
                {
                    SpawnCityGrindDino(2,N,d);
                    cgT3NextSpawn=Time.time+CityGrindInterval(3,N,d);
                }
            }
            else cgT3NextSpawn=-1f;
        }

        // Task002/N.2.33 "stat_scaling" - PROVEN. iPad base HP/ATK per type
        // (Task027N.2.33's own preserved City Grind facts): Type1 HP5 ATK10,
        // Type2 HP1 ATK7, Type3 HP25 ATK13. SPD is NOT level-scaled (species
        // base speed used as-is). ADIS is NOT level-scaled - base + a fresh
        // [0,20) jitter at spawn (the sandbox's own existing
        // AdisJitterExclusiveMax spawn-time jitter already matches this
        // shape and is left as SpawnDino's own untouched behavior).
        static readonly float[] CgBaseHP  = { 5f, 1f, 25f };
        static readonly float[] CgBaseAtk = { 10f, 7f, 13f };
        void SpawnCityGrindDino(int typeIndex0to2,int N,int d)
        {
            SpawnSide side=UnityEngine.Random.value<0.5f ? SpawnSide.Left : SpawnSide.Right;
            DinoInstance inst=SpawnDino(DinoSpecies[typeIndex0to2].kind,side);
            inst.nativeCityGrindSpawn=true;
            inst.cityGrindLevelAtSpawn=N;

            var prevCurrent=current;
            current=inst;

            float hpMod = typeIndex0to2==0 ? (N-1)*2.6f
                        : typeIndex0to2==1 ? (N-4)*1.6f
                        : (N-7)*CityGrindType3M3(N);
            float atkMod = typeIndex0to2==0 ? (N-1)*0.2f
                         : typeIndex0to2==1 ? (N-4)*0.1f
                         : (N-7)*0.3f;

            float k=d-1;
            float hp = (CgBaseHP[typeIndex0to2]+hpMod) * (1f+0.1f*k);
            float atk = (CgBaseAtk[typeIndex0to2]+atkMod) * (1f+0.1f*k);

            enemyHP=Mathf.Max(1f,hp);
            enemySourceAttack=Mathf.Max(0f,atk);
            // SPD (enemySourceSpeed) intentionally left as SpawnDino's own
            // species-base value - PROVEN not level-scaled.

            current=prevCurrent;
            status="CITY GRIND L"+N+" D"+d+": spawned "+DinoSpecies[typeIndex0to2].label+
                " HP="+hp.ToString("0.0")+" ATK="+atk.ToString("0.0")+" (native scaling).";
        }
        static float CityGrindType3M3(int N) => N<=20 ? 3.0f : (N<=40 ? 4.0f : 4.4f);

        void DebugSampleF2Transitions()
        {
            if(playerVisual==null)
                return;

            bool facing=playerVisual.facingRight;
            bool moving=left || right;

            if(!debugFacingInitialized)
            {
                debugFacingInitialized=true;
                debugLastFacingRight=facing;
                debugLastMoving=moving;
                return;
            }

            if(facing!=debugLastFacingRight)
            {
                F2DebugTelemetry.Log("FACING_CHANGE",F2DebugStateSummary());
                debugLastFacingRight=facing;
            }

            if(moving!=debugLastMoving)
            {
                F2DebugTelemetry.Log(
                    "IDLE_WALK_CHANGE",
                    (moving ? "MOVING; " : "IDLE; ")+F2DebugStateSummary());
                debugLastMoving=moving;
            }
        }

        string F2DebugStateSummary()
        {
            string sec=
                playerVisual!=null && playerVisual.secondary.Clip!=null
                ? playerVisual.secondary.Clip.name
                : "<none>";

            string flame=
                f2FlameVisual!=null
                ? f2FlameVisual.DebugStateSummary()
                : "huoyan=<null>";

            return
                "held="+fireHeld+
                " prev="+previousFireHeld+
                " pending="+f2ReleasePending+
                " facing="+
                    (playerVisual!=null && playerVisual.facingRight ? "R" : "L")+
                " move="+((left ? -1 : 0)+(right ? 1 : 0)).ToString()+
                " sec="+sec+
                " secActive="+
                    (playerVisual!=null && playerVisual.secondary.Active)+
                " secFrame="+
                    (playerVisual!=null ? playerVisual.secondary.Frame : -1)+
                " sinceAtt3="+
                    (lastF2ShaoEventTime<0f ? -1f
                        : (Time.time-lastF2ShaoEventTime)).ToString("0.000")+
                " "+flame;
        }

        void SetDebugEnemyHPFromText()
        {
            float value;
            if(!float.TryParse(debugEnemyHPText,out value))
            {
                status="DEBUG HP: enter a numeric value.";
                return;
            }

            value=Mathf.Clamp(value,1f,999999f);
            enemyHP=value;
            debugEnemyHPText=value.ToString("0");
            status="DEBUG_ONLY current dino HP="+debugEnemyHPText+
                "; source spawn/reset HP remains unchanged.";
        }

        // TASK027N.2 Parts Q/R/S: DEBUG/SANDBOX overhead HP overlay.
        // World-space (Camera.main.WorldToScreenPoint), so it tracks each
        // actor's real position/facing without touching hitboxes, sorting,
        // camera, source-space transforms, animation timing, input, or
        // damage cadence (Part X) - it is read-only against already-live
        // playerHP/enemyHP and the same "tou" head anchor used elsewhere in
        // this file for blood/melee dx (Part D's Type-3-large-body warning),
        // so there is exactly one HP source of truth, never a duplicate
        // counter. Gated on showHPOverlay (Part S, default ON).
        void DrawDebugHPOverlay()
        {
            if(!showHPOverlay)
                return;

            Camera c=Camera.main;
            if(c==null)
                return;

            if(player!=null && !playerDead)
            {
                float maxHP=data!=null?data.combat.playerHP:playerHP;
                DrawWorldHPLabel(
                    c,
                    player.position+Vector3.up*190f,
                    "PLAYER  HP "+Mathf.CeilToInt(Mathf.Max(0f,playerHP))+"/"+Mathf.CeilToInt(maxHP)+
                    (godMode?"  [GOD]":""));
            }

            if(enemy!=null && enemy.gameObject.activeSelf && enemyHP>0f)
            {
                Vector3 head=enemy.position;
                enemyVisual.TryGetPartWorldAnchor("tou",out head);
                DinoSpeciesDef sp=CurrentSpecies();
                DrawWorldHPLabel(
                    c,
                    head+Vector3.up*60f,
                    sp.typeNumber+" "+Mathf.CeilToInt(Mathf.Max(0f,enemyHP))+"/"+Mathf.CeilToInt(sp.hp));
            }
        }

        void DrawWorldHPLabel(Camera c,Vector3 worldPos,string text)
        {
            Vector3 screen=c.WorldToScreenPoint(worldPos);
            if(screen.z<=0f)
                return;

            float guiX=screen.x;
            float guiY=Screen.height-screen.y;
            Vector2 size=GUI.skin.label.CalcSize(new GUIContent(text));
            Rect labelRect=new Rect(
                guiX-size.x*0.5f-4f,
                guiY-size.y*0.5f-2f,
                size.x+8f,
                size.y+4f);

            Color prevColour=GUI.color;
            GUI.color=new Color(0f,0f,0f,0.55f);
            GUI.DrawTexture(labelRect,Texture2D.whiteTexture);
            GUI.color=Color.white;
            GUI.Label(labelRect,text);
            GUI.color=prevColour;
        }

        // TASK027N.2.31 Part F/G: centralized Rect calculation, replacing
        // scattered hardcoded magic-Y coordinates across many earlier
        // tasks' own individually-placed controls (the exact pattern that
        // produced the 4 confirmed overlaps in debug_ui_inventory_n231.json).
        // A single running Y cursor per column - NextY(h) always advances by
        // h+padding regardless of whether the caller actually draws
        // anything into that slot this frame, so a conditionally-hidden row
        // (e.g. the F3-only readout) still reserves fixed vertical space and
        // every row BELOW it stays at a stable position frame-to-frame
        // instead of visually jittering as conditions change.
        struct DebugColumn
        {
            public float x, y, padding;
            public float NextY(float rowHeight)
            {
                float cur=y;
                y+=rowHeight+padding;
                return cur;
            }
        }

        bool debugUiHidden;
        void SetDebugUiHidden(bool hidden){ debugUiHidden=hidden; DC1DebugSettings.UiHidden=hidden; }
        Vector2 debugPanelScroll;
        int debugPanelTab;
        void OnGUI()
        {
            if(DC1DebugSettings.EffectiveEnabled && debugUiHidden)
            {
                if(DC1DebugPanel.RestoreChipButton()) SetDebugUiHidden(false);
            }
            else if(DC1DebugSettings.EffectiveEnabled)
            {
            int tab=GUI.Toolbar(new Rect(10,10,340,28),debugPanelTab,new[]{"PLAYER","DINOSAUR","ADVANCED"});
            if(tab!=debugPanelTab){debugPanelTab=tab;debugPanelScroll=Vector2.zero;}
            // HIDE sits next to the tab toolbar (fixed, outside the scroll view) so it never needs scrolling to reach.
            { var chip=DC1DebugPanel.RestoreChipRect(); if(DC1DebugPanel.HideChipButton(new Rect(356,10,Mathf.Max(110f,chip.width),Mathf.Max(28f,chip.height)))) SetDebugUiHidden(true); }
            if(debugPanelTab<2)DrawSandboxQuickControls();
            else {
            DC1DebugPanel.Begin(ref debugPanelScroll,1600,1700,600,0,44);
            DrawDebugHPOverlay();

            GUI.Box(
                new Rect(10,10,690,150),
                "Dino Cap 1 - Combat Sandbox\n" +
                "HP " + playerHP.ToString("0") +
                "   " + enemyClipPrefix.ToUpperInvariant() + " HP " + enemyHP.ToString("0.00") +
                "   Ammo " + familyAmmo[equippedFamily] +
                "   Money " + money +
                "   Weapon " + CurrentWeaponName() +
                "   G Tier G" + (meleeTier+1) +
                "   Dino Facing " + (enemyFacingRight ? "RIGHT" : "LEFT") +
                "   InvulnRGB " + playerInvulnColourScale.ToString("0.00") + "\n" +
                "Dino SPD " + enemySourceSpeed.ToString("0") +
                "  Player SPD 205  ADIS " + enemyRuntimeAdis.ToString("0") +
                "  ATK " + enemySourceAttack.ToString("0") +
                "  FireCount " + sustainedShotCounter + "\n" +
                "LastHitFamily " + CurrentWeaponName() +
                "  QxSpawn " + DinoCapBloodBurst.QxSpawnCount +
                "  DxSpawn " + DinoCapBloodBurst.DxSpawnCount +
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
                "  BallisticBlood " + DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood) +
#endif
                "  BiteState " + (EnemyBiteCommitted()?"BITING":"NO") + "\n" +
                status + "\n" +
                (debugDinoKind==DebugDinoKind.K4Raptor
                    ? "K4 debug: native frame10 shared projectile; ordinary frame12 bite excluded."
                    : "Task024B: strict root-X cone, frame-12 live recheck, native attack completion."));

            // TASK027N.2.23 Part O: the N.2.22 AutoMeleeBite status used to
            // live as the LAST line of the box above's own ever-growing,
            // multi-paragraph version-banner text - by now that paragraph
            // wraps to enough lines that the status line renders well past
            // the box's own drawn background, effectively invisible without
            // scrolling/resizing the game view. Given its own small,
            // dedicated, ALWAYS-in-the-same-place box directly under the
            // main one instead - no new buttons, no occlusion of anything
            // else, exact "AUTO MELEE: <mode>" wording + hotkey hints per
            // this task's own spec.
            string autoMeleeModeText=!autoMeleeBiteTestEnabled
                ? "OFF"
                : autoMeleeMode==AutoMeleeMode.FixedOffset
                    ? "FIXED "+autoMeleeFixedOffset
                    : "SWEEP";
            GUI.Box(
                new Rect(10,165,400,44),
                "AUTO MELEE: "+autoMeleeModeText+
                "   Attempts "+autoMeleeAttemptCount+"\n"+
                "[M] toggle  [N] mode  [,/.] offset   Last: "+autoMeleeLastResultSummary);

            // ---- TASK027N.1 DEBUG all-species spawn controller ----
            // TASK027N.2.31 Part F: RIGHT PANEL - "DINO / SPAWN". Rebuilt
            // onto the same DebugColumn helper as the LEFT PANEL, with a
            // NEW row (Part B/C: AUTO SPAWN 1-2-3 toggle + status) appended
            // without disturbing any existing row's position relative to
            // the others (manual controls all preserved unchanged - Part D).
            {
                DebugColumn rightCol=new DebugColumn{x=705,y=10,padding=4};
                DinoSpeciesDef selSp=DinoSpecies[Mathf.Clamp(manualTypeIndex,0,DinoSpecies.Length-1)];

                // Row 1: manual type cycle / side toggle / SPAWN
                float r1=rightCol.NextY(26);
                if (GUI.Button(new Rect(705,r1,150,26),"DINO TYPE: "+selSp.typeNumber+"  "+selSp.label))
                    manualTypeIndex=(manualTypeIndex+1)%DinoSpecies.Length;
                if (GUI.Button(new Rect(860,r1,120,26),"SPAWN SIDE: "+(spawnSide==SpawnSide.Left?"LEFT":"RIGHT")))
                    spawnSide=spawnSide==SpawnSide.Left?SpawnSide.Right:SpawnSide.Left;
                if (GUI.Button(new Rect(985,r1,95,26),"SPAWN DINO"))
                    SpawnDino(selSp.kind,spawnSide);

                // Row 2: AUTO on/off + type mode + side mode (existing
                // general-purpose auto-spawner - untouched, Part D).
                float r2=rightCol.NextY(26);
                if (GUI.Button(new Rect(705,r2,90,26),autoSpawn?"AUTO: ON":"AUTO: OFF"))
                {
                    autoSpawn=!autoSpawn;
                    nextAutoSpawnTime=Time.time+Mathf.Max(0.25f,autoIntervalSeconds);
                }
                if (GUI.Button(new Rect(800,r2,140,26),"AUTO TYPE: "+autoTypeMode.ToString().ToUpperInvariant()))
                    autoTypeMode=(AutoTypeMode)(((int)autoTypeMode+1)%3);
                if (GUI.Button(new Rect(945,r2,135,26),"AUTO SIDE: "+autoSideMode.ToString().ToUpperInvariant()))
                    autoSideMode=(AutoSideMode)(((int)autoSideMode+1)%3);

                // Row 3: interval / cap / clear
                float r3=rightCol.NextY(26);
                if (GUI.Button(new Rect(705,r3,120,26),"INTERVAL: "+autoIntervalSeconds.ToString("0.0")+"s"))
                    autoIntervalSeconds=autoIntervalSeconds>=4f?1f:autoIntervalSeconds+1f;
                if (GUI.Button(new Rect(830,r3,90,26),"CAP: "+maxActiveDinos))
                    maxActiveDinos=maxActiveDinos>=12?4:maxActiveDinos+4;
                if (GUI.Button(new Rect(925,r3,105,26),"CLEAR DINOS"))
                    ClearDinos();

                // Row 4: existing general auto-spawner status.
                // TASK027N.2.35: "single active dino - replaces" is now
                // stale (manual/auto spawn ADD, Part C) - corrected below.
                float r4=rightCol.NextY(22);
                GUI.Label(new Rect(705,r4,420,22),
                    "ACTIVE "+ActiveDinoCount+"/"+maxActiveDinos+
                    (autoPausedByCap?"  (AUTO PAUSED: CAP)":"")+
                    "   LAST: T"+lastSpawnedType+" "+(lastSpawnedSide==SpawnSide.Left?"LEFT":"RIGHT")+
                    "   [digits 1-4 / SPAWN DINO ADD a dino; CLEAR DINOS / C removes all]");

                // Row 5 (NEW, Part B/C): AUTO SPAWN 1-2-3 toggle + status.
                // A deliberately separate row/system from the general
                // auto-spawner above - see autospawn_debug_manifest_n231.json.
                float r5=rightCol.NextY(26);
                if (GUI.Button(new Rect(705,r5,170,26),autoSpawn123?"AUTO SPAWN 1-2-3: ON":"AUTO SPAWN 1-2-3: OFF"))
                    autoSpawn123=!autoSpawn123;
                GUI.Label(new Rect(880,r5,300,26),
                    "ACTIVE TYPE: "+(autoSpawn123LastType<0?"NONE":autoSpawn123LastType.ToString())+
                    "   SIDE: "+(autoSpawn123LastSide==SpawnSide.Left?"LEFT":"RIGHT")+
                    "   [J]");

                // Row 6 (NEW, TASK027N.2.35 Part P/Q): CITY GRIND SPAWNER
                // test mode - SOURCE/NATIVE (timers/bands/unlock gates/
                // HP-ATK scaling), explicitly separate from every DEBUG
                // control above (Part R policy split; see
                // native_vs_debug_spawn_policy_n235.json).
                float r6=rightCol.NextY(26);
                if (GUI.Button(new Rect(705,r6,190,26),cityGrindSpawnerOn?"CITY GRIND SPAWNER: ON":"CITY GRIND SPAWNER: OFF"))
                    cityGrindSpawnerOn=!cityGrindSpawnerOn;
                // TASK027N.2.36 Part B: numeric text input + LOAD/Enter commit
                // replaces the old fixed-step cycling button. Text editing
                // alone never changes cityGrindLevel; only LOAD/Enter commits.
                GUI.Label(new Rect(900,r6,60,26),"LEVEL:");
                const string cgLevelCtrl="cgSandboxLevelField";
                GUI.SetNextControlName(cgLevelCtrl);
                cityGrindLevelInputText=DC1DebugTextInput.Field(new Rect(960,r6,60,26),cityGrindLevelInputText);
                bool cgLevelEnter = Event.current!=null && Event.current.type==EventType.KeyDown &&
                    (Event.current.keyCode==KeyCode.Return || Event.current.keyCode==KeyCode.KeypadEnter) &&
                    GUI.GetNameOfFocusedControl()==cgLevelCtrl;
                if (GUI.Button(new Rect(1025,r6,55,26),"LOAD") || cgLevelEnter)
                {
                    if(int.TryParse(cityGrindLevelInputText, out int cgN) && cgN>=1)
                        cityGrindLevel=cgN;
                    else
                        cityGrindLevelInputText=cityGrindLevel.ToString();
                }
                if (GUI.Button(new Rect(1085,r6,95,26),"DIFF: "+cityGrindDifficulty))
                    cityGrindDifficulty=cityGrindDifficulty>=3?1:cityGrindDifficulty+1;

                // TASK027N.2.36 Part C: visible control to enter the SEPARATE
                // reconstructed CityGrindScene. CombatSandboxV2 itself stays
                // an isolated combat/debug laboratory - this only navigates
                // away to the real campaign scene, it does not change what
                // this scene owns.
                // TASK027N.2.36.1 Part A: fixed - the live scene ASSET is
                // "CityGrindScene.unity" (canonical contract per this task's
                // own Part A); the string here previously read "CityGridScene"
                // (missing the "n"), which would have failed to resolve at
                // runtime in any context relying on SceneManager's own
                // name-based lookup (Build Settings or Resources-based load).
                if (GUI.Button(new Rect(1185,r6,140,26),"OPEN CITY GRIND"))
                    SandboxModeSession.OpenCity();

                float r7=rightCol.NextY(20);
                GUI.Label(new Rect(705,r7,420,20),
                    "T1(L1+) "+(cityGrindLevel>=1?"armed":"locked")+
                    "  T2(L4+) "+(cityGrindLevel>=4?"armed":"locked")+
                    "  T3(L8+) "+(cityGrindLevel>=8?"armed":"locked")+
                    "  [independent timers; native HP/ATK scaling]");

                // Row 8 (NEW, Part O): collection-safe debug HP/status list.
                float r8=rightCol.NextY(20);
                GUI.Label(new Rect(705,r8,200,20),"ACTIVE DINOS: "+dinos.Count);
                for(int i=0;i<dinos.Count && i<6;i++)
                {
                    var dd=dinos[i];
                    float ry8=rightCol.NextY(18);
                    int ddType=dd.kind switch {
                        DebugDinoKind.K3Blue=>1, DebugDinoKind.K1Rusher=>2,
                        DebugDinoKind.K2Trex=>3, _=>4 };
                    GUI.Label(new Rect(705,ry8,420,18),
                        "  #"+dd.id+"  T"+ddType+
                        "  HP="+dd.hp.ToString("0")+
                        "  STATE="+(dd.hp<=0?"DEAD/CORPSE":dd.burningF2?"F2-BURN":dd.f3State?"F3-ELEC":dd.attacking?"ATTACK":"LIVE")+
                        (dd.nativeCityGrindSpawn?"  [CG L"+dd.cityGrindLevelAtSpawn+"]":""));
                }
                if(dinos.Count>6)
                {
                    float ryMore=rightCol.NextY(18);
                    GUI.Label(new Rect(705,ryMore,300,18),"  ... +"+(dinos.Count-6)+" more");
                }
            }

            // TASK027N.2.31 Part F/G REBUILD of N2.9 Part Q's own prior
            // ad-hoc cleanup. That cleanup fixed ONE overlap cluster by
            // moving 2 groups of controls to a then-empty region, but left
            // 4 OTHER controls (F3/hit-reaction readout, F3 X-ray buttons,
            // XRAY tuning sliders, melee/invuln readout) still using
            // hardcoded magic-Y coordinates from even earlier tasks -
            // re-auditing this whole region (debug_ui_inventory_n231.json)
            // found those 4 genuinely overlapping the narrative box and/or
            // the auto-melee box. Rebuilt as ONE auto-advancing LEFT PANEL
            // ("PLAYER / WEAPON / DIAGNOSTICS") below both boxes (which end
            // at y=160/209 respectively) instead of more ad-hoc moves -
            // every row below reserves its own space via DebugColumn
            // regardless of whether its own condition is true this frame,
            // so nothing shifts/jitters as F3/Type3/etc toggle on and off.
            // Starts below the TOP/STATUS group (narrative box y10-160,
            // auto-melee box y165-209, version banner y215-237, rage debug
            // line y241-263) - see the version-banner/rage-line Rects
            // further down in this same method for that group's own exact
            // positions.
            DebugColumn leftCol=new DebugColumn{x=10,y=270,padding=6};

            {
                float ry=leftCol.NextY(26);
                if (GUI.Button(new Rect(10,ry,130,26),godMode?"GOD MODE: ON":"GOD MODE: OFF"))
                    godMode=!godMode;
                if (GUI.Button(new Rect(145,ry,130,26),showHPOverlay?"SHOW HP: ON":"SHOW HP: OFF"))
                    showHPOverlay=!showHPOverlay;
                // TASK027N.2.31 Part A: Infinite Ammo toggle - grouped with
                // the other player/weapon debug toggles per this task's own
                // LEFT PANEL guidance.
                if (GUI.Button(new Rect(280,ry,140,26),infiniteAmmo?"INFINITE AMMO: ON":"INFINITE AMMO: OFF"))
                    infiniteAmmo=!infiniteAmmo;
                if (GUI.Button(new Rect(425,ry,110,26),bloodDebugSlowmo?"BLOOD 0.25x":"BLOOD 1x"))
                {
                    bloodDebugSlowmo=!bloodDebugSlowmo;
                    DinoCapDamageParticle.DebugTimeScale=bloodDebugSlowmo?0.25f:1f;
                }
                if (GUI.Button(new Rect(540,ry,100,26),"DUMP F2 LOG"))
                    F2DebugTelemetry.DumpToConsole();
            }

            {
                float ry=leftCol.NextY(20);
                // Task027M.2 Part P: F3 runtime-state readout (F3-gated).
                if (familyTier[5]==2)
                {
                    GUI.Label(new Rect(10,ry,980,20),
                        "F3  session="+f3SessionActive+
                        "  relPending="+f3ReleasePending+
                        "  enemyState4="+enemyF3State+
                        "  recover="+enemyF3Recover+
                        "  death="+enemyF3Death+
                        "  beFrame="+(enemyF3State?enemyVisual.primary.Frame:-1)+
                        "  xrayPhase="+(f3XrayVisualPhase?"XRAY":"HURT")+
                        "  xraySkeleton="+(enemyVisual!=null&&enemyVisual.xraySkeleton)+
                        "  beam="+(f3LaserVisual!=null?f3LaserVisual.DebugStateSummary():"<none>"));
                }
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
                // TASK027N.1.2 Part Q: DEBUG_ONLY last-hit reaction readout.
                // Shares this row with the F3 readout above and yields it
                // when F3 is equipped. Alters no gameplay state.
                else if (familyTier==null || familyTier[5]!=2)
                {
                    GUI.Label(new Rect(10,ry,1060,20),
                        "HIT-REACTION (N.1.2)  enemyState="+
                            (enemyBurningF2?"F2_BURN(5)":
                             enemyF3State?"F3_ELEC(4)":
                             enemyHeavyReaction?"HEAVY(2)":
                             EnemyBiteCommitted()?"ATTACK/BITE(1)":
                             enemyAttacking?"ATTACK(1)":"CHASE/IDLE(8)")+
                        "   biteCommitted="+EnemyBiteCommitted()+
                        "     LAST HIT: fam="+dbgLastHitFamily+
                        " dmg="+dbgLastHitDamage.ToString("0.00")+
                        " hurtVis="+(dbgLastHitHurtVisual?"Y":"N")+
                        " knock="+(dbgLastHitKnockback?"Y":"N")+
                        " atkInterrupted="+(dbgLastHitAttackInterrupted?"Y":"N")+
                        " reactionState="+dbgLastHitReactionState+
                        " ("+(dbgLastHitFrame<0?"never":("frame "+dbgLastHitFrame))+")");
                }
#endif
            }

            // Task027M.8.3: F3 anatomical X-ray controls. The bone ART is
            // SOURCE-FAITHFUL (real k*gt* skeleton parts, packed into
            // Resources/DinoCapGenerated/Textures/f3xray_skeleton.png); the
            // alternation TIMING is the port VISUAL_RECONSTRUCTION on the k*_be
            // frame-3 cadence. Presentation only. The old generic-inversion
            // shader toggle is retired to DEBUG_ONLY (further right).
            {
                float ry=leftCol.NextY(26);
                if (GUI.Button(new Rect(10,ry,170,26),
                    f3XrayEnabled ? "F3 XRAY: ON" : "F3 XRAY: OFF"))
                {
                    f3XrayEnabled=!f3XrayEnabled;
                    if(!f3XrayEnabled && enemyVisual!=null)
                        enemyVisual.xraySkeleton=false;
                }
                if (enemyVisual!=null && GUI.Button(new Rect(185,ry,150,26),
                    enemyVisual.xraySkeletonShell ? "XRAY SHELL: ON" : "XRAY SHELL: OFF"))
                    enemyVisual.xraySkeletonShell=!enemyVisual.xraySkeletonShell;
                if (GUI.Button(new Rect(340,ry,175,26),
                    "XRAY PHASE: "+(f3XrayPhaseMode==0 ? "EVERY EVENT" : "EVERY 2 EVENTS")))
                    f3XrayPhaseMode = (f3XrayPhaseMode+1)%2;

                // DEBUG_ONLY: the retired M8/M8.1 generic luminance-inversion
                // shader path + its fail-safe status (never used in production).
                if (GUI.Button(new Rect(520,ry,140,26),
                    (enemyVisual!=null&&enemyVisual.xrayFlash) ? "INV SHADER: ON" : "INV SHADER: OFF"))
                    if(enemyVisual!=null) enemyVisual.xrayFlash=!enemyVisual.xrayFlash;

                if (GUI.Button(new Rect(665,ry,170,26),"RESET DINO MATERIAL"))
                    if(enemyVisual!=null) enemyVisual.ForceRestoreMaterials();
            }

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            {
                float ry=leftCol.NextY(18);
                // DEBUG-only appearance tuning (NOT gameplay timing).
                GUI.Label(new Rect(10,ry,520,18),
                    "XRAY  alpha="+Power2DSourceTruthRenderer.XrayAlpha.ToString("0.00")+
                    "  bright="+Power2DSourceTruthRenderer.XrayBrightness.ToString("0.00")+
                    "  floor="+Power2DSourceTruthRenderer.XrayFloor.ToString("0.00"));
                Power2DSourceTruthRenderer.XrayAlpha=
                    GUI.HorizontalSlider(new Rect(540,ry+2,120,14),Power2DSourceTruthRenderer.XrayAlpha,0f,1f);
                Power2DSourceTruthRenderer.XrayBrightness=
                    GUI.HorizontalSlider(new Rect(670,ry+2,120,14),Power2DSourceTruthRenderer.XrayBrightness,0f,3f);
                Power2DSourceTruthRenderer.XrayFloor=
                    GUI.HorizontalSlider(new Rect(800,ry+2,120,14),Power2DSourceTruthRenderer.XrayFloor,0f,1f);
            }
#endif

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            // TASK027N.1.3 Part Q: DEBUG_ONLY outgoing-melee + Type 3
            // knockback/stun/decapitation readout. Alters no gameplay state.
            {
                float ry=leftCol.NextY(20);
                // Decapitation eligibility depends only on the weapon that
                // WOULD land the kill (F1, or G3/G4) - report it against the
                // currently selected melee tier and F1 ammo, matching
                // ExecuteMeleeContact/TryF1ProjectileContact exactly.
                bool meleeWouldDecap = Mathf.Clamp(meleeTier,0,3)>=2;
                bool f1Available = familyTier[5]==0 && familyAmmo[5]>0;

                GUI.Label(new Rect(10,ry,1060,20),
                    "PLAYER INVULNERABLE="+(Time.time<invulnUntil?"Y":"N")+
                    "  MELEE CONTACT EVENT="+(dbgMeleeContactEvent?"Y":"N")+
                    "  MELEE TARGET FOUND="+(dbgMeleeTargetFound?"Y":"N")+
                    "  MELEE DAMAGE DISPATCHED="+(dbgMeleeDamageDispatched?"Y":"N")+
                    "  BLOCK REASON="+dbgMeleeBlockReason+
                    " (frame "+(dbgMeleeFrame<0?"never":dbgMeleeFrame.ToString())+")");
            }

            {
                float ry=leftCol.NextY(20);
                bool meleeWouldDecap = Mathf.Clamp(meleeTier,0,3)>=2;
                bool f1Available = familyTier[5]==0 && familyAmmo[5]>0;
                if (debugDinoKind==DebugDinoKind.K2Trex)
                {
                    GUI.Label(new Rect(10,ry,1060,20),
                        "TYPE3  KNOCKBACK APPLIED=N (species-permanent exclusion)"+
                        "  STUN APPLIED="+((enemyBurningF2||enemyF3State)?"Y":"N")+
                        "  DECAPITATION ELIGIBLE="+
                        ((meleeWouldDecap||f1Available)?"Y":"N")+
                        " (G"+(meleeTier+1)+(meleeWouldDecap?"=decap":"=standard")+
                        "; F1 ammo="+familyAmmo[5]+")");
                }
            }

            {
                float ry=leftCol.NextY(20);
                // TASK027N.1.3.2 Part N: real-input melee readout. Lets a
                // MANUAL user reproduction reveal exactly which stage differs
                // from automation - alters no gameplay state.
                GUI.Label(new Rect(10,ry,1060,20),
                    "BLUE RAW HELD="+(blueHeld?"Y":"N")+
                    "  PRESS EDGE="+(dbgBluePressEdgeThisFrame?"Y":"N")+
                    "  BUFFERED-START="+(dbgBlueBufferedStartThisFrame?"Y":"N")+
                    "  HITSTUN="+(playerHitStun?"Y":"N")+
                    "  INVULNERABLE="+(Time.time<invulnUntil?"Y":"N")+
                    "  MELEE STARTED(playerMelee)="+(playerMelee?"Y":"N")+
                    "  CONTACT FIRED="+(dbgMeleeContactEvent?"Y":"N")+
                    "  DAMAGE DISPATCHED="+(dbgMeleeDamageDispatched?"Y":"N"));
            }
#endif

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            {
                float ry=leftCol.NextY(22);
                // Task027L.4.11/4.12 DEBUG_ONLY: arm a one-frame power2D hitch
                // of a selectable size to verify missed-fire collapse (no
                // Audio_att3 / melee / attack event burst on recovery).
                // Compiled out of production builds.
                if (GUI.Button(new Rect(10,ry,60,22),"H33"))
                    Power2DSourceTruthRenderer.DebugArmOneFrameStall(0.033f);
                if (GUI.Button(new Rect(75,ry,60,22),"H50"))
                    Power2DSourceTruthRenderer.DebugArmOneFrameStall(0.050f);
                if (GUI.Button(new Rect(140,ry,60,22),"H75"))
                    Power2DSourceTruthRenderer.DebugArmOneFrameStall(0.075f);
                if (GUI.Button(new Rect(205,ry,60,22),"H120"))
                    Power2DSourceTruthRenderer.DebugArmOneFrameStall(0.120f);
                if (enemyVisual != null)
                {
                    // Task027L.4.13 Part K DEBUG_ONLY observability.
                    GUI.Label(new Rect(275,ry,420,20),
                        string.Format(
                            "power2D  hostDt={0:0.0}ms  est={1:0}Hz  mode={2}  srcCB={3}  dtList=[{4}]ms",
                            enemyVisual.LastHostDtSeconds*1000f,
                            enemyVisual.EstablishedHostHz,
                            enemyVisual.LastAdapterMode,
                            enemyVisual.LastSourceCallbacks,
                            enemyVisual.LastSourceDtListMs()));
                }
            }
#endif

            // TASK027N.2.27 Part A: a clearly-readable, high-contrast,
            // ALWAYS-in-the-same-place version banner, deliberately separate
            // from the dense multi-paragraph narrative box above (which by
            // this point in the arc buries the actual version number in the
            // middle of a wall of dev-log prose - not something a user could
            // reliably screenshot/report back). Sourced from the SAME
            // PortVersion/PortBuildTag constants as Start()'s own console
            // log, so a stale/non-recompiled Play session becomes
            // immediately provable by comparing this box against the
            // expected version after any future patch.
            // TASK027N.2.31 Part F: kept in the TOP/STATUS group (directly
            // under the narrative/auto-melee boxes) rather than interleaved
            // with the LEFT PANEL diagnostic column below - Part G's own
            // "preserve version banner readability" requirement is best
            // served by keeping it near the top, not pushed down by however
            // many diagnostic rows happen to be reserved that frame.
            Color prevBoxColour=GUI.color;
            GUI.color=new Color(1f,0.95f,0.2f,1f);
            GUI.Box(new Rect(10,215,420,22),
                "DINO CAP PORT "+PortVersion+"   BUILD: "+PortBuildTag);
            GUI.color=prevBoxColour;

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            // TASK027N.2.30 Part L CORRECTION of N.2.27's own debug line.
            // The OLD line's own "RAGE FACE" read SustainedFaceLevel(
            // sustainedShotCounter) - the NEW SESSION's own raw progress
            // counter - NOT the actual logical/rendered face. Since N2.29
            // correctly decoupled these two concepts (an old face can keep
            // displaying while a brand-new sub-threshold session
            // independently accumulates from 0), that old debug field was
            // GUARANTEED to show "0" whenever a new sub-threshold session
            // was active, even while the OLD face was still correctly
            // showing on screen - this is PROVEN (by direct code
            // inspection) to be the root cause of the user's own reported
            // "RAGE FACE: 0 but visual still shows rage2/3" observation
            // (rage_stale_visual_root_cause_n230.json). Now shows LOGICAL
            // (derived from headRefOverride, the field that actually drives
            // rendering), RENDERED (the ground-truth ref Render() last
            // actually applied, via the new LastRenderedHeadRef - see
            // Power2DSourceTruthRenderer.cs), and NEW PROGRESS
            // (sustainedShotCounter) as three separate, correctly-labelled
            // values.
            int rageLogicalLevel=System.Array.IndexOf(SustainedFaceRefs,
                playerVisual!=null?playerVisual.headRefOverride:"");
            int rageRenderedLevel=System.Array.IndexOf(SustainedFaceRefs,
                playerVisual!=null?playerVisual.LastRenderedHeadRef:"");
            float rageResetTimerRemaining =
                faceResetAt<0f ? -1f : Mathf.Max(0f,faceResetAt-Time.time);
            GUI.Box(new Rect(10,241,460,22),
                "RAGE LOGICAL: "+Mathf.Max(0,rageLogicalLevel)+
                "   RENDERED: "+Mathf.Max(0,rageRenderedLevel)+
                "   NEW PROGRESS: "+sustainedShotCounter+
                "   SESSION: "+(rageSessionActive?"ON":"OFF")+
                "   OLD EXPIRY: "+(rageResetTimerRemaining<0f?"--":rageResetTimerRemaining.ToString("0.00")));
#endif

            // TASK027N.2.9 Part Q: shifted from x=1065 - overlapped Row 1's
            // SPAWN DINO button (705-1080 range) at y=8-36.
            GUI.Label(new Rect(1090,8,120,20),"DINO HP");
            debugEnemyHPText=DC1DebugTextInput.Field(
                new Rect(1090,29,75,30),debugEnemyHPText);
            if (GUI.Button(new Rect(1170,29,70,30),"SET"))
                SetDebugEnemyHPFromText();

            if (GUI.Button(new Rect(1220,10,170,55),
                "PRESENT\n" + GameplayPresentationCamera.GlobalModeName))
                GameplayPresentationCamera.CycleGlobalMode();

            if (GUI.Button(new Rect(1395,10,145,55),
                "INTERNAL\n" + GameplayRenderTarget.InternalScaleName))
                GameplayRenderTarget.TogglePersistentSupersample();

            // TASK027N.2.49R REOPEN2 Phase 3/6: same shared, persistent
            // GameplayRenderTarget.AspectMode state City Grind's own toggle
            // controls - confirms/exercises the ONE shared 768-high logical
            // world contract across both scenes (Phase 6's own "shared
            // viewport" requirement).
            if (GUI.Button(new Rect(1220,70,320,40),
                "VIEW: " + (GameplayRenderTarget.IsSourceFaithful4x3 ? "4:3 FAITHFUL" : "WIDESCREEN")))
                GameplayRenderTarget.TogglePersistentAspectMode();

#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            if(GUI.Button(new Rect(425,165,270,26),"DINO CORPSE DESPAWN: "+(DebugCorpseSettings.DespawnEnabled?"ON":"OFF")))
                DebugCorpseSettings.SetDespawnEnabled(!DebugCorpseSettings.DespawnEnabled);
#endif
            DC1DebugPanel.End();
            }
            }

            float h=70;
            float y=Screen.height-h-20;

            if(GUI.RepeatButton(new Rect(20,y,80,h),"<")) left=true;
            if(GUI.RepeatButton(new Rect(110,y,80,h),">")) right=true;
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            if(DC1DebugSettings.EffectiveEnabled && !debugUiHidden)
            {
            // TASK027N.2.22 Part L: compact A-D family/tier debug selector -
            // 2 cycle buttons + 1 apply button instead of 16 tier buttons.
            if(GUI.Button(new Rect(Screen.width-820,y,90,h/2-1),
                "A-D FAM\n"+WeaponFamilyTable.FamilyLetter(debugAbcdFamilySelect)))
                debugAbcdFamilySelect=(debugAbcdFamilySelect+1)%4;
            if(GUI.Button(new Rect(Screen.width-820,y+h/2+1,90,h/2-1),
                "TIER "+(debugAbcdTierSelect+1)))
                debugAbcdTierSelect=(debugAbcdTierSelect+1)%
                    WeaponFamilyTable.TierCount[debugAbcdFamilySelect];
            if(GUI.Button(new Rect(Screen.width-720,y,90,h),
                "APPLY\n"+WeaponFamilyTable.FamilyLetter(debugAbcdFamilySelect)+
                (debugAbcdTierSelect+1)+" ammo "+familyAmmo[debugAbcdFamilySelect]))
                DebugApplyAbcdSelection(debugAbcdFamilySelect,debugAbcdTierSelect);
            GUI.Label(new Rect(Screen.width-820,y-20,300,18),
                "Current weapon: "+CurrentWeaponName()+
                "  ammo "+familyAmmo[equippedFamily]);
            }
#endif
            if(DC1DebugSettings.EffectiveEnabled && !debugUiHidden && GUI.Button(new Rect(Screen.width-490,y,90,h),
                "F MODE\nF"+(familyTier[5]+1))) CycleFMode();
            if(DC1DebugSettings.EffectiveEnabled && !debugUiHidden && GUI.Button(new Rect(Screen.width-390,y,90,h),
                "G TIER\nG"+(meleeTier+1))) CycleMeleeTier();
            Rect blueRect=new Rect(Screen.width-290,y,85,h);
            if(GUI.Button(blueRect,"BLUE\nMelee")) { DebugRecordBluePressEdge(); Melee(); }
            {
                // TASK027N.1.3.2: same raw-event latch idiom as ORANGE/Shoot
                // just below (Task027L.2 V30.6B) - GUI.Button's click return
                // value is an edge (kept above, unchanged, for the normal
                // immediate-swing feel); guiBlueHeld is a SEPARATE, persistent
                // mouse/touch-down..up latch on the SAME rect, feeding
                // PollKeyboard's continuous `blueHeld` read (mirrors native
                // byte_21BE74) so a hold-through-hurt can be re-checked at the
                // hitstun-clear boundary. Captured via rawType so it is not
                // affected by GUI.Button already consuming the event.
                Event ev=Event.current;
                EventType rawType=ev!=null ? ev.rawType : EventType.Ignore;
                int rawButton=ev!=null ? ev.button : -1;
                Vector2 rawMouse=ev!=null ? ev.mousePosition : Vector2.zero;
                if(rawButton==0)
                {
                    if(rawType==EventType.MouseDown && blueRect.Contains(rawMouse))
                        guiBlueHeld=true;
                    else if(rawType==EventType.MouseUp && guiBlueHeld)
                        guiBlueHeld=false;
                }
            }
            Rect shootRect=new Rect(Screen.width-195,y,85,h);
            if(equippedFamily==0 ||
               equippedFamily==1 ||
               equippedFamily==4 ||
               (equippedFamily==5 && familyTier[5]==0))
            {
                // Task027L.2 V30.6B â€” source-faithful semi-auto input adapter.
                //
                // Native Orange is a raw touch-DOWN latch, and its held mirror
                // remains true until touch-UP. The previous Unity branch used
                // GUI.Button's completed-click result and then forcibly set
                // guiFireHeld=false, so A1 could never preserve "Orange still
                // held" through a melee. That made the proven post-melee
                // deferred-shot contract impossible regardless of downstream
                // arbitration patches.
                //
                // Capture the physical IMGUI edge before GUI.Button consumes it.
                Event ev=Event.current;
                EventType rawType=ev!=null ? ev.rawType : EventType.Ignore;
                int rawButton=ev!=null ? ev.button : -1;
                Vector2 rawMouse=ev!=null ? ev.mousePosition : Vector2.zero;

                // Keep Button only for its normal visual/pressed feedback.
                GUI.Button(shootRect,"ORANGE\nShoot");

                if(rawButton==0)
                {
                    if(rawType==EventType.MouseDown &&
                       shootRect.Contains(rawMouse))
                    {
                        guiFireHeld=true;
                        firePressQueued=true;
                    }
                    else if(rawType==EventType.MouseUp && guiFireHeld)
                    {
                        guiFireHeld=false;
                    }
                }
            }
            else
            {
                // Task027K4.2 telemetry closure:
                // GUI.RepeatButton is event-phase based. Assigning its return
                // value directly to persistent gameplay state made guiFireHeld
                // pulse false/true across IMGUI passes, producing hundreds of
                // fake Orange-up/re-hold edges while the mouse was physically
                // held down.
                //
                // Keep RepeatButton only for its normal IMGUI rendering/press
                // feedback. Persistent gameplay held state is latched from the
                // actual mouse/touch-down and mouse/touch-up edges.
                // Capture the raw IMGUI event BEFORE RepeatButton consumes it.
                // Event.type may become EventType.Used after the control handles
                // MouseDown/MouseUp; Event.rawType preserves the original event.
                Event ev=Event.current;
                EventType rawType=ev!=null ? ev.rawType : EventType.Ignore;
                int rawButton=ev!=null ? ev.button : -1;
                Vector2 rawMouse=ev!=null ? ev.mousePosition : Vector2.zero;

                GUI.RepeatButton(shootRect,"ORANGE\nShoot");

                if(rawButton==0)
                {
                    if(rawType==EventType.MouseDown &&
                       shootRect.Contains(rawMouse))
                    {
                        guiFireHeld=true;
                        F2DebugTelemetry.Log(
                            "GUI_FIRE_DOWN",
                            F2DebugStateSummary());
                    }
                    else if(rawType==EventType.MouseUp && guiFireHeld)
                    {
                        guiFireHeld=false;
                        F2DebugTelemetry.Log(
                            "GUI_FIRE_UP",
                            F2DebugStateSummary());
                    }
                }
            }
            if(GUI.Button(new Rect(Screen.width-100,y,85,h),"GREEN\n>>")) SwitchWeapon();
        }
    }
}
