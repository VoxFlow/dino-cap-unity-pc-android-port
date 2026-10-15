using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DinoCap.PortV2
{
    public sealed class Power2DTrack
    {
        public DCAnimation Clip { get; private set; }
        public int Frame { get; private set; }
        public bool Active { get; private set; }
        public event Action<Power2DTrack,int> EnterFrame;
        // Task027L.4.12 DEBUG_ONLY observability: number of frame advances since
        // the last ResetAdvanceCount(). Not read by gameplay.
        public int AdvanceCount { get; private set; }
        public void ResetAdvanceCount() { AdvanceCount=0; }
        float acc;
        int lastFrame=-1;
        bool boundedRange;
        bool forceLoop;
        // Native sub_76A20 DRAWS the current frame and only THEN advances; Unity ticks first and draws afterwards. A clip started at a frame >= 1
        // is seeded (start*dt) so it advances on its very first tick, which at 60 Hz means the start frame would never be drawn at all (the
        // forward-recoil frame 3 of an F2/F3 hold, the discharge frame 3 of a C/D cycle...). With DrawSeededStartFrame the first advance is
        // held back until the start frame has been rendered once, restoring native's "start frame is on screen for one tick".
        // Only owners that also continue clips synchronously from Completed (below) opt in, otherwise a polled restart would add a tick.
        public bool DrawSeededStartFrame;
        bool startFramePendingRender;
        public void NotifyRendered() { startFramePendingRender=false; }
        // Raised inside Tick the moment a bounded clip completes (native: the owner's completion callback, e.g. vtable+40 = sub_663A0 for the
        // player, sub_101A0 for dinos). The owner may restart or stop the clip synchronously, exactly like native.
        public event Action<Power2DTrack> Completed;
        int loopStart;   // native sub_76980/sub_769D0 store the requested start frame (+60 / +64) as the loop restart frame

        public void Play(DCAnimation clip, int startFrame)
        {
            PlayInternal(clip,startFrame,
                clip==null || clip.frames==null ? 0 : clip.frames.Length-1,
                false);
        }

        public void PlayRange(DCAnimation clip,int startFrame,int endFrame)
        {
            PlayInternal(clip,startFrame,endFrame,true);
        }


        // Native sub_76980(this,-1,start,dt,bound): loop counter -1 = infinite. Frames start..end repeat; a wrap goes back to the start
        // frame WITHOUT re-seeding the accumulator (only the first start is seeded). Used by the F3 laser's held 4..6 loop (sub_12F28).
        public void PlayRangeLooping(DCAnimation clip,int startFrame,int endFrame)
        {
            PlayInternal(clip,startFrame,endFrame,false);
            forceLoop=true;
        }

        // Native76980 can request infinite playback independently of the
        // animation asset. Keep this request local to the actor's track.
        public void PlayLooping(DCAnimation clip,int startFrame)
        {
            Play(clip,startFrame);
            forceLoop=true;
        }

        // Native7677C changes the primary binding without restarting76980's
        // frame/cadence. Weapon selection therefore preserves the current pose.
        public void Rebind(DCAnimation clip)
        {
            if(clip==null || clip.frames==null || clip.frames.Length==0)return;
            Clip=clip;
            Frame=Mathf.Clamp(Frame,0,clip.frames.Length-1);
            lastFrame=clip.frames.Length-1;
        }

        void PlayInternal(DCAnimation clip,int startFrame,int endFrame,bool bounded)
        {
            Clip=clip;
            int max=clip==null || clip.frames==null
                ? 0 : Mathf.Max(0,clip.frames.Length-1);
            Frame=Mathf.Clamp(startFrame,0,max);
            lastFrame=Mathf.Clamp(endFrame,Frame,max);
            boundedRange=bounded;
            forceLoop=false;
            // NATIVE (PROVEN ARM, sub_76980 primary / sub_769D0 secondary): starting a clip seeds the accumulator with
            // float(startFrame) * frameDuration (+16 / +20). For startFrame>=1 the seed already reaches frameDuration, so the first
            // update advances off the start frame immediately: the start frame is emitted once and lasts a single tick. Example:
            // the F3 hold loop starts at frame 3 with 0.2 s frames, so its cycle is 1 tick + 0.2 s (~0.217 s), not 0.4 s.
            loopStart=startFrame<0?0:startFrame;
            acc=(clip!=null ? clip.frameDuration : 0f)*(float)Frame;
            startFramePendingRender=DrawSeededStartFrame && clip!=null && acc>=clip.frameDuration;
            Active=true;
            if (EnterFrame != null) EnterFrame(this,Frame);
        }

        public void Stop()
        {
            Active=false;
            Clip=null;
            Frame=0;
            lastFrame=-1;
            boundedRange=false;
            forceLoop=false;
            acc=0;
        }

        public void Tick(float dt)
        {
            if (!Active || Clip==null || Clip.frames==null || Clip.frames.Length==0)
                return;

            if(startFramePendingRender){ acc += dt; return; }   // draw the seeded start frame once before advancing (see DrawSeededStartFrame)
            acc += dt;
            // Task027L.4.13 Part I: no float epsilon. In fp32 2f*(1f/60f) is
            // BIT-IDENTICAL to 1f/30f (0.03333333507...), and the audited
            // callback streams produce zero near-boundary accumulator values,
            // so the L4.12 1e-4 guard was unnecessary and is removed - this is
            // the plain native compare (sub_76A20: acc < frameDuration).
            if (acc < Clip.frameDuration)
                return;

            // Native shipped quirk: max one frame per Update and reset accumulator.
            acc=0f;
            AdvanceCount++;
            int next=Frame+1;

            if (next>lastFrame)
            {
                if (!boundedRange && (forceLoop || Clip.loop))
                    next=Mathf.Clamp(loopStart,0,lastFrame);   // native loops restart at the requested start frame
                else
                {
                    Frame=lastFrame;
                    Active=false;
                    if(Completed!=null) Completed(this);
                    return;
                }
            }

            Frame=next;
            if (EnterFrame != null) EnterFrame(this,Frame);
        }

        public DCFrame Current
        {
            get
            {
                return Clip==null || Clip.frames==null || Clip.frames.Length==0
                    ? null
                    : Clip.frames[Frame];
            }
        }

        public DCFrame FrameZero
        {
            get
            {
                return Clip==null || Clip.frames==null || Clip.frames.Length==0
                    ? null
                    : Clip.frames[0];
            }
        }
    }

    public sealed class Power2DSourceTruthRenderer : MonoBehaviour
    {
        public float actorScale=1f;
        // Task026: native actor colourScale. RGB only; alpha is untouched.
        public float actorColourScale=1f;
        // ============================================================
        // Task027M.8.3 - F3 anatomical X-ray = SOURCE-FAITHFUL part swap.
        // ============================================================
        // The original Dino Cap atlases ship a COMPLETE per-species white
        // pixel-art X-ray skeleton: the `k*gt*` (gt = gutou / bone) parts -
        // skull (k*gttou), jaw (k*gtxiaba), torso with spine + rib cage +
        // pelvis + tail vertebrae (k3gtshenti / k2gtshen), six limb bones
        // (k*gttui1..6) and a foot (k*gtzua). Every gt part is the EXACT rect
        // size of its living counterpart (verified) so it is a drop-in swap
        // under the identical k*_be part transform. M8.2 missed these because
        // its string search used "gutou/skeleton/xray"; the token is "gt".
        //
        // Native consumer: UNRESOLVED. All `k*gt*Frame` string xrefs are inside
        // the four atlas builders (sub_1BDC0 / sub_2A8A8 / sub_8925C /
        // sub_A1518); no gameplay/render code and no sprintf assembles the
        // names. The art shipped in every atlas but the F3 reaction path traced
        // in M8.2 never swaps to it. So: the BONE PIXELS are source-faithful
        // (extracted verbatim from iPad dino1/dino2 into
        // Resources/DinoCapGenerated/Textures/f3xray_skeleton.png); the
        // ALTERNATION TIMING is a PORT VISUAL_RECONSTRUCTION on the existing
        // k*_be frame-3 reaction cadence.
        //
        // xraySkeleton = production path. When true, Draw() swaps each body
        // part's sprite to its `xr_<rig>_<part>` (shell + bones, as shipped) or
        // `xrb_<rig>_<part>` (bones only) equivalent - same transform, pivot,
        // sort order, facing, twitch. Presentation only: no damage / audio /
        // AI / HP / collision / F2. Cleared on every F3 exit path.
        public bool xraySkeleton=false;
        public bool xraySkeletonShell=true;   // true = keep the shipped dark body shell; false = bones only

        // living atlas ref -> X-ray atlas ref stem. k1/k4 k*_be clips use k4*
        // refs, k3_be uses k3* refs, all share rig A (k3gt*); k2 uses rig B
        // (k2gt*). Populated for both the k3* and k4* living names.
        static readonly Dictionary<string,string> s_xraySwap = BuildXraySwap();
        static Dictionary<string,string> BuildXraySwap()
        {
            var m = new Dictionary<string,string>();
            // rig A - serves species k1 / k3 / k4
            string[] a = { "tou2:tou", "xiaba:xiaba", "shen:shen",
                           "t1:t1", "t2:t2", "t3:t3", "t4:t4", "t5:t5", "t6:t6",
                           "zhua:zhua" };
            foreach(string pair in a)
            {
                int c=pair.IndexOf(':');
                string live=pair.Substring(0,c), part=pair.Substring(c+1);
                m["k3"+live]="k3_"+part;
                m["k4"+live]="k3_"+part;   // k1/k4 living refs -> same rig-A skeleton
            }
            // rig B - species k2. The living k2_be clip uses `k2tui1..6` for the
            // legs (not `k2t1..6`), so map BOTH spellings to the rig-B bones.
            string[] b = { "tou:tou", "tou2:tou", "xiaba:xiaba", "shen:shen",
                           "t1:t1", "t2:t2", "t3:t3", "t4:t4", "t5:t5", "t6:t6",
                           "tui1:t1", "tui2:t2", "tui3:t3", "tui4:t4", "tui5:t5", "tui6:t6",
                           "zhua:zhua" };
            foreach(string pair in b)
            {
                int c=pair.IndexOf(':');
                string live=pair.Substring(0,c), part=pair.Substring(c+1);
                m["k2"+live]="k2_"+part;
            }
            return m;
        }

        // Task027M.8 / M.8.1 generic luminance-inversion shader path.
        // CLASSIFICATION: DEBUG_ONLY. FALSIFIED as the "bones through body" look
        // (Task027M.8.2 DEFECT B) - superseded by xraySkeleton above. Kept only
        // as a developer A/B comparison; CombatSandboxV2 never sets xrayFlash in
        // production. Fail-safe material machinery retained so the debug toggle
        // can never turn the dinosaur magenta. No damage/audio/AI/HP/collision.
        public bool xrayFlash=false;

        // Task027M.8 DEBUG-only appearance tuning (not gameplay timing). Static
        // so one shared X-ray material serves every actor. Production defaults
        // are the closest iPad match.
        public static float XrayAlpha      = 0.62f;
        public static float XrayBrightness = 1.35f;
        public static float XrayFloor      = 0.12f;
        public static Color XrayTint       = new Color(0.72f, 0.87f, 1f, 1f);

        // Task027M.8.1: FAIL-SAFE. If the F3 X-ray shader is missing, has compile
        // errors, or the material fell back to Unity's internal error shader,
        // the X-ray is disabled and normal dino rendering is kept - a shader
        // failure can NEVER turn the dinosaur magenta.
        public enum XrayStatus { Unknown, Ok, ShaderMissing, ShaderUnsupported, MaterialError }
        public static XrayStatus XrayShaderStatus = XrayStatus.Unknown;

        // TASK027N.2.51 LARGE CONTINUATION (Section 7, pause atomic freeze):
        // this renderer is a separate MonoBehaviour with its own LateUpdate,
        // scene-agnostic (shared by player/dino instances, and reusable by
        // Sandbox) - it has no knowledge of any scene's own "paused" field.
        // Without this, the scene's own Update()-level early-return (which
        // stops movement/gameplay logic) left this component's own
        // independent per-frame clip-time advance running, which is exactly
        // why dinos/player limbs kept animating briefly/continuously while
        // "frozen" - the root cause of the reported atomic-freeze bug. Any
        // owning scene controller sets this once per frame; LateUpdate()
        // below returns before touching ANY internal clock/accumulator
        // state when true, so resuming continues exactly where it left off
        // rather than losing or double-counting elapsed time.
        public static bool GlobalPaused;

        static Material s_xrayMat;
        static bool     s_xrayResolved = false;   // resolution attempted this session
        static int      s_xrayPropFrame = -1;

        bool _xrayApplied = false;                 // X-ray MATERIAL currently on this actor's pool
        // exact original sharedMaterial per pool slot, captured before first X-ray apply
        readonly List<Material> origMat = new List<Material>();

        // Returns the shared F3 X-ray material, or NULL if it is unavailable /
        // unsupported. NEVER returns an invalid / error-shader material.
        static Material XrayMaterial()
        {
            if(!s_xrayResolved)
            {
                s_xrayResolved = true;
                Shader sh = Resources.Load<Shader>("F3XraySprite");
                if(sh == null) sh = Shader.Find("DinoCap/F3XraySprite");
                if(sh == null)
                {
                    XrayShaderStatus = XrayStatus.ShaderMissing;
                    s_xrayMat = null;
                }
                else if(!sh.isSupported)
                {
                    XrayShaderStatus = XrayStatus.ShaderUnsupported;
                    s_xrayMat = null;
                }
                else
                {
                    Material m = new Material(sh){ hideFlags = HideFlags.HideAndDontSave };
                    // reject a silent fallback to the internal error (magenta) shader
                    if(m.shader == null || m.shader.name == "Hidden/InternalErrorShader" || !m.shader.isSupported)
                    {
                        XrayShaderStatus = XrayStatus.MaterialError;
                        s_xrayMat = null;
                    }
                    else
                    {
                        XrayShaderStatus = XrayStatus.Ok;
                        s_xrayMat = m;
                    }
                }
                if(s_xrayMat == null)
                    Debug.LogWarning("[DC1 F3 X-ray] shader unavailable ("+XrayShaderStatus+
                        ") - X-ray disabled, normal dino rendering kept. No magenta.");
            }

            if(s_xrayMat != null && s_xrayPropFrame != Time.frameCount)
            {
                s_xrayPropFrame = Time.frameCount;
                s_xrayMat.SetFloat("_XrayAlpha",      Mathf.Clamp01(XrayAlpha));
                s_xrayMat.SetFloat("_XrayBrightness", Mathf.Clamp(XrayBrightness,0f,3f));
                s_xrayMat.SetFloat("_XrayFloor",      Mathf.Clamp01(XrayFloor));
                s_xrayMat.SetColor("_XrayTint",       XrayTint);
            }
            return s_xrayMat;
        }

        // Task027M.8.1: capture each pool slot's original sharedMaterial once, so
        // it can be restored by EXACT reference (not by blindly assigning null).
        void CaptureOriginalMaterials()
        {
            while(origMat.Count < pool.Count)
                origMat.Add(pool[origMat.Count]!=null ? pool[origMat.Count].sharedMaterial : null);
        }

        void ApplyXrayMaterial()
        {
            Material m = XrayMaterial();
            if(m == null) { _xrayApplied = false; return; }   // unavailable -> keep normal rendering
            CaptureOriginalMaterials();
            for(int k=0;k<pool.Count;k++)
                if(pool[k]!=null) pool[k].sharedMaterial = m;
            _xrayApplied = true;
        }

        // Restore the EXACT original materials + clear any property block.
        void RestoreOriginalMaterials()
        {
            for(int k=0;k<pool.Count;k++)
            {
                if(pool[k]==null) continue;
                pool[k].sharedMaterial = (k < origMat.Count) ? origMat[k] : null;
                pool[k].SetPropertyBlock(null);
            }
            _xrayApplied = false;
        }

        // Task027M.8.1 Part I: one-click recovery. Clears the X-ray flag and
        // restores every original material + normal colour path. Gameplay state
        // untouched.
        public void ForceRestoreMaterials()
        {
            xrayFlash = false;
            xraySkeleton = false;   // Task027M.8.3: also drop the anatomical X-ray part swap
            RestoreOriginalMaterials();
        }
        public bool facingRight=true;
        public int baseSortingOrder=100;
        public Vector2 drawOffset=Vector2.zero;
        public bool verboseSecondaryDiagnostics=false;
        // Task027 native player+0x6C suffix equivalent for sustained-fire faces.
        public string headRefOverride="";
        string faceComposeOwn, faceComposeOverride, faceComposeResult;   // cache of the native ref+suffix lookup for the head part
        // TASK027N.2.51 SECOND FRESH BATCH (item 4 - "weapons swapped
        // temporarily on building exit"). p_house/pd_house are each a
        // SINGLE generic clip (unlike every other player action, which has
        // 6 per-family variants A-F) - their own com1/yyj5 parts bake in
        // whatever family the clip happened to be authored with (p_house:
        // com1="A2"; pd_house: yyj5="B3", com1="shoubi1"), structurally
        // proving (via the conspicuous ABSENCE of per-family variants for
        // this one action, unlike all others) that native itself must
        // apply a runtime weapon-ref substitution here, matching whatever
        // is actually equipped - the SAME pattern already proven and
        // applied for yyj6/headRefOverride above. com1RefOverride/
        // yyj5RefOverride are empty (no-op) during every normal per-family
        // clip (p_stand_A1 etc. already bake the CORRECT weapon), and are
        // set only while p_house/pd_house are playing.
        public string com1RefOverride="";
        public string yyj5RefOverride="";
        public string LastRenderedCom1Ref="";
        public string LastRenderedYyj5Ref="";
        // TASK027N.2.30 Part D/E: the LOGICAL headRefOverride string is not,
        // by itself, proof of what is actually drawn - Render()'s own yyj6
        // branch only APPLIES it when non-empty, and Draw()'s own sprite
        // lookup can silently no-op (ResetSequenceSlot+disable) if the ref
        // fails to resolve. This field is written UNCONDITIONALLY every
        // time the yyj6 part is processed in Render() (both the override
        // and the natural-primary-clip path), so a test/debug consumer can
        // read the GROUND-TRUTH ref actually handed to Draw() this frame,
        // independent of whatever headRefOverride currently says.
        public string LastRenderedHeadRef="";

        public Power2DTrack primary=new Power2DTrack();
        public Power2DTrack secondary=new Power2DTrack();

        DCManifest manifest;

        readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        readonly List<SpriteRenderer> pool=new List<SpriteRenderer>();

        readonly Dictionary<string,List<Sprite>> sequenceSprites=
            new Dictionary<string,List<Sprite>>();
        readonly List<string> sequenceKeyBySlot=new List<string>();
        readonly List<int> sequenceIndexBySlot=new List<int>();
        readonly List<float> sequenceAccBySlot=new List<float>();
        const float NestedSequenceFrameDuration=1f/30f;

        // Task027L.4.10: the shipped power2D / Triniti OGLESShell runs its logic
        // update from an NSTimer scheduled at exactly 1/60 s
        // (-[EngineAppDelegate application:didFinishLaunchingWithOptions:],
        // PROVEN by the timer-interval double 0x3F91111111111111). Each fire
        // sub_16A2CC measures a real gettimeofday delta (ms resolution, clamped
        // <= 0.1 s) and passes it, ONCE, through sub_164440 -> sub_17BA5C -> the
        // CAObj animator sub_76A20 (single call per actor - confirmed no dt
        // sub-stepping loop in the native chain, Task027L.4.11). sub_76A20
        // advances AT MOST ONE animation frame per call and hard-resets its
        // accumulator. So a 0.025 s source frameDuration dwells 2 healthy ticks
        // (0.03333 s) and a 7-frame k*_shao loop is a regular 0.23333 s.
        //
        // Task027L.4.11/4.12/4.13/4.14 - THREE-MODE host cadence adapter. The port
        // classifies the ESTABLISHED host cadence (rolling median of the last 24
        // host frame deltas; ratio = establishedFrameDt / NativeTickDt) against an
        // explicit MAX PERSISTENT SOURCE-RATE ERROR BUDGET, and commits a mode
        // change only after the robust median has stayed across a threshold for a
        // PERSISTENCE WINDOW of frames:
        //
        //  NATIVE_MEASURED  (|ratio - 1| <= 0.25%, host ~59.85-60.15 Hz - the
        //     robust median is serving as the original ~60 Hz source scheduler,
        //     jitter and transient missed frames included): emit exactly ONE
        //     source callback per host frame carrying min(Time.deltaTime, 0.1 s).
        //     The exact native path - one -[EngineAppDelegate update], one
        //     sub_16A2CC measured dt clamped to 0.1 s, one sub_76A20(actor, dt),
        //     <= 1 frame. A long frame carries a longer clamped dt (== native
        //     missed fire). Native jitter / event phase preserved bit-for-bit.
        //  RATE_CONVERT_HIGH  (ratio persistently < 1 - 0.25%, host faster than
        //     ~60.15 Hz): emit one FIXED 1/60 s source callback each time a
        //     scheduled 1/60 s slot comes due (schedule accumulator; cap 3).
        //     (Measured-elapsed synthetic dt was FALSIFIED at 66-80 Hz - it
        //     doubles the clip speed; Task027L.4.13 Part E.)
        //  RATE_CONVERT_LOW  (ratio persistently > 1 + 0.25%, host slower than
        //     ~59.85 Hz): synthesize N fixed 1/60 s source sub-ticks per host
        //     frame to hold source speed; a frame >= 2x the established cadence
        //     collapses to one clamped Tick. PLATFORM_ADAPTER - the shipped game
        //     never ran below 60 Hz.
        //
        // Task027L.4.14 replaces the L4.13 wide 0.90/1.10 ratio-hysteresis (which
        // could TRAP a persistent 55/66 Hz host in NATIVE_MEASURED forever at
        // 91.7% / 110% source speed - a state-machine path-dependence bug). The
        // final steady mode is now a pure function of the persistent robust
        // cadence: NATIVE_MEASURED iff its speed error stays within the budget,
        // else rate-convert. The only history dependence left is a documented
        // 0.20-0.25% deadband where the mode is held (max speed error there ==
        // the 0.25% budget, not a violation).
        //
        // NativeTickDt / NativeMaxFrameDt are SOURCE constants (PROVEN). The
        // PortCadence* / PortSourceRate* values are PORT POLICY tied to an
        // explicit modern-port timing-accuracy requirement (see
        // power2d_rate_error_budget_l414.json). No absolute 50 ms floor, no
        // 3x-EMA factor (retired L4.12); no 1e-4 float epsilon (retired L4.13);
        // no ratio-hysteresis band (retired L4.14).
        // Movement / camera / gameplay timers are NOT touched (real Time.deltaTime).
        const float NativeTickDt=1f/60f;             // EngineAppDelegate NSTimer interval (PROVEN)
        const float NativeMaxFrameDt=0.1f;           // sub_16A2CC dt clamp (PROVEN)
        const int   PortMaxCallbacksPerFrame=3;      // PORT: rate-convert emission cap
        const int   PortCadenceWindow=24;            // PORT: rolling-median window over host frame deltas
        const int   PortCadenceMinSamples=8;         // PORT: samples before a cadence is "established"
        const float PortSourceRateErrBudget=0.0025f; // PORT: max persistent source-rate error to STAY NATIVE_MEASURED (0.25%)
        const float PortSourceRateReenter=0.0020f;   // PORT: tighter error to RE-ENTER NATIVE_MEASURED (0.05% deadband, anti-flap)
        const int   PortCadencePersistFrames=12;     // PORT: frames the robust median must hold across a threshold before a mode commit
        const float PortLowStallSlotRatio=2f;        // PORT: LOW-mode collapse trigger (>= one scheduled slot skipped)
        readonly float[] hostDtRing=new float[PortCadenceWindow];
        readonly float[] hostDtSortScratch=new float[PortCadenceWindow];
        int   hostDtRingCount, hostDtRingHead;
        float establishedFrameDt;                    // rolling median of hostDtRing; 0 until established
        bool  cadenceEstablished;
        float srcScheduleAcc;                        // wall time toward the next scheduled 1/60 s slot (convert modes)
        readonly float[] sourceDtBuf=new float[8];   // this-frame ORDERED source-callback dt list (fed identically to primary/secondary/nested)
        int   sourceDtCount;
        // Scene actors can share the existing source cadence without a second clock.
        public event Action<float> BeforeSourceTick;
        public event Action AfterSourceTick;

        public enum HostCadenceMode { Warmup, NativeMeasured, RateConvertHigh, RateConvertLow }
        HostCadenceMode cadenceMode=HostCadenceMode.Warmup;
        HostCadenceMode pendingCadenceMode=HostCadenceMode.Warmup;  // candidate mode awaiting the persistence window
        int   pendingCadenceFrames;                                 // consecutive frames the candidate has held

        // Task027L.4.12/4.13 DEBUG_ONLY observability (not read by gameplay).
        public HostCadenceMode LastAdapterMode => cadenceMode;
        public int   LastSourceCallbacks => sourceDtCount;
        public float LastHostDtSeconds { get; private set; }
        public float EstablishedHostHz => (cadenceEstablished && establishedFrameDt>0f) ? 1f/establishedFrameDt : 0f;
        public string LastSourceDtListMs()
        {
            var sb=new System.Text.StringBuilder();
            for(int i=0;i<sourceDtCount;i++){ if(i>0) sb.Append(','); sb.Append((sourceDtBuf[i]*1000f).ToString("0.0")); }
            return sb.ToString();
        }

        // Task027L.4.11 DEBUG_ONLY: one-frame stall injector for hitch-robustness
        // acceptance testing. Set from an editor/development-build debug control
        // only; production builds have no way to arm it, so this is inert there.
        public static float DebugInjectStallSeconds;
        static int debugInjectStallFrame=-1;
        public static void DebugArmOneFrameStall(float seconds)
        {
            DebugInjectStallSeconds=Mathf.Max(0f,seconds);
            debugInjectStallFrame=Time.frameCount+1;
        }

        readonly Dictionary<string,DCPart> secondaryByName=new Dictionary<string,DCPart>();
        readonly Dictionary<string,DCPart> secondaryFrame0ByName=new Dictionary<string,DCPart>();

        string lastDiagnosticKey="";

        public void Initialize(DCManifest m)
        {
            manifest=m;
            BuildSprites();
            BuildSequenceSprites();
        }

        static long nextCompositeDepth;
        Transform actorPartsRoot;
        SortingGroup actorPartsGroup;

        // Native CAObjCallBack allocates disjoint depth blocks of50 in
        // sub764C4; sub76A20 gives children baseDepth+ordinal. Main scene
        // layer10 mode5 sorts that depth globally (sub179990). Unity groups
        // only the rig's parts, keeping attached blood/effects outside it.
        public void EnableActorComposition()
        {
            if(actorPartsRoot!=null) return;
            actorPartsRoot=new GameObject("ActorParts").transform;
            actorPartsRoot.SetParent(transform,false);
            nextCompositeDepth+=50;
            // Orthographic depth encodes creation order among equal-layer
            // groups. It is a Unity adapter; native depth is an integer key.
            actorPartsRoot.localPosition=new Vector3(0f,0f,-nextCompositeDepth*0.0001f);
            actorPartsGroup=actorPartsRoot.gameObject.AddComponent<SortingGroup>();
            actorPartsGroup.sortingOrder=baseSortingOrder;
            foreach(var renderer in pool) renderer.transform.SetParent(actorPartsRoot,false);
        }

        public DCAnimation FindClip(string name)
        {
            for(int i=0;i<manifest.animations.Length;i++)
                if(manifest.animations[i].name==name)
                    return manifest.animations[i];

            throw new InvalidOperationException("Clip not exported: "+name);
        }

        readonly Dictionary<string, DCAnimation> weaponHouseClips = new Dictionary<string, DCAnimation>();

        public DCAnimation FindWeaponHouseClip(string weaponName)
        {
            if(weaponHouseClips.TryGetValue(weaponName,out var cached)) return cached;
            DCPatch patch = Array.Find(manifest.patches,p => p.name==weaponName);
            if(patch==null) throw new InvalidOperationException("Missing house weapon patch: "+weaponName);
            DCPart q = patch.part;
            string baseName = q.name=="com1" ? "p_house" : "pd_house";
            DCAnimation authored = FindClip(baseName);
            var result = new DCAnimation { name="p_house_"+weaponName,
                frameDuration=0.025f, loop=false, frames=new DCFrame[authored.frames.Length] };
            // Native sub_7D554/sub_7D81C clone the rig-specific house
            // container and apply sub_78A38: ADD X/Y/C, COPY W/H/A/B/ref.
            // Ref-only replacement leaves the other rig's geometry intact.
            for(int frame=0;frame<authored.frames.Length;frame++)
            {
                var parts = new List<DCPart>();
                foreach(var sourcePart in authored.frames[frame].parts)
                {
                    // Native7D81C case5 modes1/2 both clone pd_house and
                    // apply their C2B patch. 7DCAE is F1's nested-anim call.
                    DCPart p = ClonePart(sourcePart);
                    if(p.name==q.name) {
                        p.x+=q.x; p.y+=q.y; p.c+=q.c;
                        p.w=q.w; p.h=q.h; p.a=q.a; p.b=q.b;
                        p.@ref=q.Ref; p.refName=q.Ref;
                    }
                    parts.Add(p);
                }
                result.frames[frame] = new DCFrame { ordinal=authored.frames[frame].ordinal, parts=parts.ToArray() };
            }
            weaponHouseClips.Add(weaponName,result);
            return result;
        }

        public bool TryGetSprite(string refName, out Sprite sprite)
        {
            return sprites.TryGetValue(refName, out sprite);
        }

        void BuildSprites()
        {
            for(int i=0;i<manifest.atlas.Length;i++)
            {
                DCAtlas a=manifest.atlas[i];
                Texture2D tex=Resources.Load<Texture2D>(a.textureResource);
                if(tex==null)
                    continue;

                // Native1BDC0:1EEFC sets17EC00(1) before dino1Image: LINEAR.
                // Native2A8A8:30D80 sets17EC00(0) before dino2Image: NEAREST.
                // The old uniform-nearest assumption missed the first switch.
                bool nativeLinearDinoPage=a.textureResource=="DinoCapGenerated/Textures/ipad_dino1";
                tex.filterMode=nativeLinearDinoPage?FilterMode.Bilinear:FilterMode.Point;
                tex.wrapMode=nativeLinearDinoPage?TextureWrapMode.Repeat:TextureWrapMode.Clamp;

                float yBottom=a.textureHeight-a.yTop-a.h;

                Sprite s=Sprite.Create(
                    tex,
                    new Rect(a.x,yBottom,a.w,a.h),
                    new Vector2(.5f,.5f),
                    1f,
                    0,
                    SpriteMeshType.FullRect);

                s.name=a.@ref;
                sprites[a.@ref]=s;
            }
        }

        void BuildSequenceSprites()
        {
            sequenceSprites.Clear();
            foreach(KeyValuePair<string,Sprite> kv in sprites)
            {
                System.Text.RegularExpressions.Match m=
                    System.Text.RegularExpressions.Regex.Match(
                        kv.Key,@"^(.*?)(\d{4})$");
                if(!m.Success)
                    continue;

                string baseName=m.Groups[1].Value;
                List<Sprite> list;
                if(!sequenceSprites.TryGetValue(baseName,out list))
                {
                    list=new List<Sprite>();
                    sequenceSprites[baseName]=list;
                }
                list.Add(kv.Value);
            }

            foreach(KeyValuePair<string,List<Sprite>> kv in sequenceSprites)
                kv.Value.Sort((a,b)=>string.CompareOrdinal(a.name,b.name));

            // Native 7D81C applies 78D64 to the F1 house yyj5 slot:
            // its bare "F1" ref resolves through F1Anim, registered at
            // 3AF14/3AFE8/3B0C0 as F1-1Frame then F1-2Frame. Normal
            // exported F1 clips already contain direct frame refs. Keep
            // the existing nested source-callback clock for the house.
            if(sprites.TryGetValue("F1-1",out var f1First) &&
               sprites.TryGetValue("F1-2",out var f1Second))
                sequenceSprites["F1"]=new List<Sprite>{f1First,f1Second};
        }

        void EnsureSequenceSlot(int i)
        {
            while(sequenceKeyBySlot.Count<=i)
            {
                sequenceKeyBySlot.Add("");
                sequenceIndexBySlot.Add(0);
                sequenceAccBySlot.Add(0f);
            }
        }

        void ResetSequenceSlot(int i)
        {
            EnsureSequenceSlot(i);
            sequenceKeyBySlot[i]="";
            sequenceIndexBySlot[i]=0;
            sequenceAccBySlot[i]=0f;
        }

        bool TryResolveSequenceBase(int slot,string rawRef,out Sprite sprite)
        {
            sprite=null;
            if(string.IsNullOrEmpty(rawRef))
                return false;

            string baseName=rawRef;
            List<Sprite> frames;
            if(!sequenceSprites.TryGetValue(baseName,out frames))
            {
                string stripped=System.Text.RegularExpressions.Regex.Replace(
                    rawRef,@"\d+$","");
                if(stripped==rawRef ||
                   !sequenceSprites.TryGetValue(stripped,out frames))
                    return false;
                baseName=stripped;
            }

            if(frames==null || frames.Count==0)
                return false;

            EnsureSequenceSlot(slot);
            if(sequenceKeyBySlot[slot]!=baseName)
            {
                sequenceKeyBySlot[slot]=baseName;
                sequenceIndexBySlot[slot]=0;
                sequenceAccBySlot[slot]=0f;
            }
            else
            {
                // Task027L.4.10/4.11/4.13 Part H: advance nested sprite
                // sequences with the EXACT ordered source-callback dt list the
                // primary/secondary tracks got this LateUpdate (one iteration
                // per emitted callback, same hard-reset). This keeps
                // primary/secondary/nested coherent in every adapter mode -
                // one clamped callback in NATIVE_MEASURED, N fixed 1/60 s
                // callbacks under rate conversion.
                for(int st=0;st<sourceDtCount;st++)
                {
                    sequenceAccBySlot[slot]+=sourceDtBuf[st];
                    if(sequenceAccBySlot[slot]>=NestedSequenceFrameDuration)
                    {
                        sequenceAccBySlot[slot]=0f;
                        sequenceIndexBySlot[slot]=
                            (sequenceIndexBySlot[slot]+1)%frames.Count;
                    }
                }
            }

            sprite=frames[sequenceIndexBySlot[slot]];
            return true;
        }

        float MedianHostDt()
        {
            int n=hostDtRingCount;
            Array.Copy(hostDtRing,hostDtSortScratch,n);
            Array.Sort(hostDtSortScratch,0,n);
            return (n&1)==1 ? hostDtSortScratch[n>>1]
                            : 0.5f*(hostDtSortScratch[(n>>1)-1]+hostDtSortScratch[n>>1]);
        }

        // Task027L.4.14: map a robust cadence ratio (medianHostDt / (1/60)) to the
        // mode it SHOULD be in, given the current mode. Pure function of ratio +
        // current mode; the persistence window in LateUpdate decides WHEN a change
        // is committed. `current` only selects which side of the 0.20-0.25% anti-
        // flap deadband applies - it never makes the final steady mode depend on
        // how the cadence was reached.
        HostCadenceMode ClassifyCadence(float ratio, HostCadenceMode current)
        {
            float err = ratio>1f ? ratio-1f : 1f-ratio;
            if(current==HostCadenceMode.NativeMeasured)
            {
                if(err<=PortSourceRateErrBudget)
                    return HostCadenceMode.NativeMeasured;      // still within the source-rate budget
            }
            else if(err<PortSourceRateReenter)
            {
                return HostCadenceMode.NativeMeasured;          // dropped below the tighter re-entry error
            }
            if(err<=PortSourceRateErrBudget)
                return (current==HostCadenceMode.RateConvertHigh
                     || current==HostCadenceMode.RateConvertLow)
                    ? current                                   // inside the deadband - hold the convert mode
                    : HostCadenceMode.NativeMeasured;
            return ratio>1f ? HostCadenceMode.RateConvertLow    // host slower than 60 Hz
                            : HostCadenceMode.RateConvertHigh;   // host faster than 60 Hz
        }

        void LateUpdate()
        {
            if(actorPartsGroup!=null) actorPartsGroup.sortingOrder=baseSortingOrder;
            // TASK027N.2.51 LARGE CONTINUATION (Section 7): atomic pause
            // freeze - return before reading Time.deltaTime or touching any
            // cadence-tracking/accumulator state at all, so paused time
            // never enters this component's own clocks (no catch-up jump,
            // no lost cadence calibration, on resume).
            if(GlobalPaused) return;
            // Task027L.4.13: THREE-MODE host cadence adapter (see the class-field
            // comment). Classify the established host cadence, then emit this
            // frame's ORDERED source-callback dt list and feed it identically to
            // primary / secondary (and nested sequences via sourceDtBuf).
            float host=Time.deltaTime;
            if(Time.frameCount==debugInjectStallFrame)
                host+=DebugInjectStallSeconds;   // DEBUG_ONLY hitch injection
            LastHostDtSeconds=host;

            // Rolling-median cadence estimate. EVERY frame feeds it - the median
            // is outlier-robust, so a lone hitch never moves it, while a
            // genuinely persistent cadence change migrates it within ~WINDOW/2
            // frames (the bounded PLATFORM_ADAPTER convergence window).
            hostDtRing[hostDtRingHead]=host;
            hostDtRingHead=(hostDtRingHead+1)%PortCadenceWindow;
            if(hostDtRingCount<PortCadenceWindow) hostDtRingCount++;
            if(hostDtRingCount>=PortCadenceMinSamples)
            {
                establishedFrameDt=MedianHostDt();
                cadenceEstablished=true;
            }

            // --- classify the established host cadence against the source-rate
            //     error budget, committing a change only after PERSIST frames ---
            HostCadenceMode prevMode=cadenceMode;
            if(!cadenceEstablished)
            {
                cadenceMode=HostCadenceMode.Warmup;          // < MinSamples: behave as native
                pendingCadenceMode=HostCadenceMode.Warmup;
                pendingCadenceFrames=0;
            }
            else
            {
                float ratio=establishedFrameDt/NativeTickDt; // >1 == host slower than 60 Hz
                HostCadenceMode target=ClassifyCadence(
                    ratio,
                    cadenceMode==HostCadenceMode.Warmup ? HostCadenceMode.NativeMeasured : cadenceMode);
                if(cadenceMode==HostCadenceMode.Warmup)
                {
                    cadenceMode=target;                      // first classification commits immediately
                    pendingCadenceMode=target;
                    pendingCadenceFrames=0;
                }
                else if(target==cadenceMode)
                {
                    pendingCadenceMode=cadenceMode;          // candidate agrees - reset the counter
                    pendingCadenceFrames=0;
                }
                else
                {
                    if(target==pendingCadenceMode) pendingCadenceFrames++;
                    else { pendingCadenceMode=target; pendingCadenceFrames=1; }
                    if(pendingCadenceFrames>=PortCadencePersistFrames)
                    {
                        cadenceMode=target;                  // persistent - commit the mode change
                        pendingCadenceFrames=0;
                    }
                }
            }
            if(cadenceMode!=prevMode)
                srcScheduleAcc=0f;   // fresh scheduled-slot phase for the new mode

            // --- emit this frame's ordered source-callback dt list -----------
            sourceDtCount=0;
            HostCadenceMode effMode=cadenceMode==HostCadenceMode.Warmup
                ? HostCadenceMode.NativeMeasured : cadenceMode;

            if(effMode==HostCadenceMode.NativeMeasured)
            {
                // The host IS the ~60 Hz source callback stream. ONE callback
                // per host frame carrying the measured delta clamped to 0.1 s -
                // exactly -[EngineAppDelegate update] -> sub_16A2CC -> one
                // sub_76A20(actor, dt). A long frame simply carries a longer
                // clamped dt == the native NSTimer missed fire. No conversion,
                // no replay, native jitter preserved.
                sourceDtBuf[sourceDtCount++]=Mathf.Min(host,NativeMaxFrameDt);
                srcScheduleAcc=0f;
            }
            else if(effMode==HostCadenceMode.RateConvertHigh)
            {
                // Host persistently faster than 60 Hz. Emit one FIXED 1/60 s
                // source callback per scheduled slot that has come due
                // (schedule accumulator). Measured-elapsed synthetic dt was
                // FALSIFIED (Part E): two 72 Hz frames sum past a 0.025 s
                // frameDuration and double the clip speed.
                srcScheduleAcc+=host;
                while(srcScheduleAcc>=NativeTickDt && sourceDtCount<PortMaxCallbacksPerFrame)
                {
                    srcScheduleAcc-=NativeTickDt;
                    sourceDtBuf[sourceDtCount++]=NativeTickDt;
                }
                if(srcScheduleAcc>NativeTickDt) srcScheduleAcc=0f;   // drop overshoot past the emit cap
            }
            else // RateConvertLow - PLATFORM_ADAPTER (the shipped game never ran below 60 Hz)
            {
                if(host>=establishedFrameDt*PortLowStallSlotRatio)
                {
                    // A genuine hitch on top of the already-slow host: collapse
                    // to one clamped Tick (native missed-fire), reset phase.
                    sourceDtBuf[sourceDtCount++]=Mathf.Min(host,NativeMaxFrameDt);
                    srcScheduleAcc=0f;
                }
                else
                {
                    // Synthesize enough fixed 1/60 s sub-ticks to preserve the
                    // source animation/gameplay speed (at least one).
                    srcScheduleAcc+=host;
                    while(srcScheduleAcc>=NativeTickDt && sourceDtCount<PortMaxCallbacksPerFrame)
                    {
                        srcScheduleAcc-=NativeTickDt;
                        sourceDtBuf[sourceDtCount++]=NativeTickDt;
                    }
                    if(srcScheduleAcc>NativeTickDt) srcScheduleAcc=0f;   // drop overshoot past the emit cap
                    if(sourceDtCount==0)
                        sourceDtBuf[sourceDtCount++]=NativeTickDt;       // LOW mode always emits >= 1 sub-tick
                }
            }

            for(int i=0;i<sourceDtCount;i++)
            {
                BeforeSourceTick?.Invoke(sourceDtBuf[i]);
                primary.Tick(sourceDtBuf[i]);
                secondary.Tick(sourceDtBuf[i]);
                AfterSourceTick?.Invoke();
            }
            Render();
        }

        // The secondary pose is composed while the track plays. A completed clip is restarted or stopped synchronously by its owner from
        // Power2DTrack.Completed (native completion callback), so a drawn frame never lacks the pose while the weapon is held.
        bool SecondaryComposed=>secondary.Active;

        void Render()
        {
            DCFrame pf=primary.Current;
            if(pf==null)
            {
                HideFrom(0);
                return;
            }

            secondaryByName.Clear();
            secondaryFrame0ByName.Clear();

            bool secondaryComposed=SecondaryComposed;
            primary.NotifyRendered();
            if(secondaryComposed) secondary.NotifyRendered();
            DCFrame sf=secondaryComposed ? secondary.Current : null;
            DCFrame s0=secondaryComposed ? secondary.FrameZero : null;

            if(sf!=null)
            {
                for(int i=0;i<sf.parts.Length;i++)
                    secondaryByName[sf.parts[i].name]=sf.parts[i];
            }

            if(s0!=null)
            {
                for(int i=0;i<s0.parts.Length;i++)
                    secondaryFrame0ByName[s0.parts[i].name]=s0.parts[i];
            }

            for(int i=0;i<pf.parts.Length;i++)
            {
                DCPart p=ClonePart(pf.parts[i]);

                DCPart s;
                if(secondaryByName.TryGetValue(p.name,out s))
                {
                    DCPart b;
                    float dx=s.x;
                    float dy=s.y;
                    float dc=s.c;

                    if(secondaryFrame0ByName.TryGetValue(p.name,out b))
                    {
                        // Task022 native sub_78BE0 semantics, enforced HERE at
                        // consumption time:
                        //
                        // raw secondary clip:
                        //     delta = current - frame0
                        //
                        // already-relativized secondary clip:
                        //     frame0 == 0, therefore this is still correct.
                        //
                        // This makes the renderer robust to either exported
                        // representation and prevents manifest/cache state from
                        // deciding runtime geometry.
                        dx = s.x - b.x;
                        dy = s.y - b.y;
                        dc = WrapPi(s.c - b.c);
                    }

                    p.x += dx;
                    p.y += dy;
                    p.c = Wrap0To2Pi(p.c + dc);

                    // Native matched-secondary overrides.
                    p.a = s.a;
                    p.b = s.b;
                    p.rgba = s.rgba;
                    p.@ref = s.Ref;
                    p.refName = s.Ref;

                    if(verboseSecondaryDiagnostics)
                        EmitSecondaryDiagnostic(p.name, p, s, b, dx, dy, dc);
                }

                if(p.name=="yyj6")
                {
                    if(!string.IsNullOrEmpty(headRefOverride))
                    {
                        // Native sub_76A20 composes every part's sprite as ref + actor face suffix (+108, e.g. "_face3") + "Frame" and uses it only if
                        // that sprite exists, otherwise the part keeps its own sprite. headRefOverride holds the full face ref of the standard head
                        // ("tou" + suffix). Clips whose head ref is "tou" (stand/move/fire/melee) therefore show the rage face, while the hurt clips
                        // (head ref "tou 1") and the die clip ("si tou") keep their own faces and the suffix waits for the clip to end.
                        string face=headRefOverride;
                        if(face.StartsWith("tou",System.StringComparison.Ordinal))
                        {
                            string own=p.Ref;
                            if(own!=faceComposeOwn || faceComposeOverride!=headRefOverride)
                            {
                                string composed=own+face.Substring(3);
                                faceComposeResult=sprites.ContainsKey(composed)?composed:own;
                                faceComposeOwn=own; faceComposeOverride=headRefOverride;
                            }
                            face=faceComposeResult;
                        }
                        p.@ref=face;
                        p.refName=face;
                    }
                    // TASK027N.2.30: unconditional - records whatever ref is
                    // ACTUALLY about to be handed to Draw() this frame,
                    // whether that came from the override above or (when
                    // headRefOverride is empty) from the primary clip's own
                    // natural per-frame data via ClonePart() above.
                    LastRenderedHeadRef=p.Ref;
                }
                else if(p.name=="com1")
                {
                    if(!string.IsNullOrEmpty(com1RefOverride))
                    {
                        p.@ref=com1RefOverride;
                        p.refName=com1RefOverride;
                    }
                    LastRenderedCom1Ref=p.Ref;
                }
                else if(p.name=="yyj5")
                {
                    if(!string.IsNullOrEmpty(yyj5RefOverride))
                    {
                        p.@ref=yyj5RefOverride;
                        p.refName=yyj5RefOverride;
                    }
                    LastRenderedYyj5Ref=p.Ref;
                }

                Draw(i,p);
            }

            HideFrom(pf.parts.Length);
        }

        void EmitSecondaryDiagnostic(
            string partName,
            DCPart composed,
            DCPart currentSecondary,
            DCPart frame0Secondary,
            float dx,
            float dy,
            float dc)
        {
            string clipName=secondary.Clip==null ? "null" : secondary.Clip.name;
            string key=clipName+"#"+secondary.Frame+"#"+partName;

            if(key==lastDiagnosticKey)
                return;

            lastDiagnosticKey=key;

            string baseText=frame0Secondary==null
                ? "NO_FRAME0_MATCH"
                : "base0=("+
                  frame0Secondary.x.ToString("0.###")+","+
                  frame0Secondary.y.ToString("0.###")+","+
                  frame0Secondary.c.ToString("0.###")+")";

            Debug.Log(
                "[DC1 Secondary] clip="+clipName+
                " frame="+secondary.Frame+
                " part="+partName+
                " current=("+
                currentSecondary.x.ToString("0.###")+","+
                currentSecondary.y.ToString("0.###")+","+
                currentSecondary.c.ToString("0.###")+") "+
                baseText+
                " delta=("+
                dx.ToString("0.###")+","+
                dy.ToString("0.###")+","+
                dc.ToString("0.###")+")");
        }

        static DCPart ClonePart(DCPart p)
        {
            return new DCPart
            {
                name=p.name,
                @ref=p.Ref,
                refName=p.Ref,
                x=p.x,
                y=p.y,
                w=p.w,
                h=p.h,
                a=p.a,
                b=p.b,
                c=p.c,
                rgba=p.rgba,
                dBits=p.dBits
            };
        }

        //76A20 adds GameObject rotation to each sprite angle, but does NOT
        //rotate the authored part translations around the actor root.
        //Set this for native actors with root-angle state; keep Unity's root
        //rotation identity so its transform hierarchy cannot orbit the parts.
        public float nativeRootRotationRadians;

        void Draw(int i, DCPart p)
        {
            Ensure(i);

            SpriteRenderer sr=pool[i];
            Sprite sp;

            if(string.IsNullOrEmpty(p.Ref))
            {
                ResetSequenceSlot(i);
                sr.enabled=false;
                return;
            }

            if(!sprites.TryGetValue(p.Ref,out sp))
            {
                if(!TryResolveSequenceBase(i,p.Ref,out sp))
                {
                    ResetSequenceSlot(i);
                    sr.enabled=false;
                    return;
                }
            }
            else
            {
                ResetSequenceSlot(i);
            }

            // Task027M.8.3: SOURCE-FAITHFUL anatomical X-ray. While xraySkeleton
            // is set, swap this body part's sprite to its `k*gt*` bone
            // equivalent. Same p.x/p.y/p.a/p.b/p.c transform, same pivot, same
            // sort order, same flipX and drawOffset - the skeleton tracks the
            // exact k*_be pose/twitch/facing because it IS the part rig with
            // bone sprites. If the swap sprite is missing the living part is
            // kept (never blank, never magenta).
            bool xrOn=false;
            if(xraySkeleton)
            {
                string stem;
                if(primary.Clip!=null && primary.Clip.name=="k4_be" &&
                   K4XrayCompatibility.TryGetSkeletalRef(p.Ref,out string skeletalRef))
                {
                    //Non-crashing adapter: original atlas pixels, authored
                    //pose/colour/alpha, and existing F3 visual-phase owner.
                    if(!sprites.TryGetValue(skeletalRef,out sp) || sp==null)
                    {
                        sr.enabled=false;
                        Debug.LogError("[DC1] Missing exported K4 skeletal frame: "+skeletalRef);
                        return;
                    }
                }
                else if(s_xraySwap.TryGetValue(p.Ref,out stem))
                {
                    string xref=(xraySkeletonShell ? "xr_" : "xrb_")+stem;
                    Sprite xsp;
                    if(sprites.TryGetValue(xref,out xsp) && xsp!=null)
                    {
                        sp=xsp;
                        xrOn=true;
                    }
                }
            }

            sr.enabled=true;
            sr.sprite=sp;
            // TASK027N.2.51 Part 4.7: CORRECTED. 'i' (the part's own CURRENT
            // FRAME array-position index) is NOT a stable draw-order key -
            // PROVEN this task via direct combat_manifest.json inspection:
            // native's own per-part 'dBits' field (already deserialized into
            // DCPart.dBits, already flowing through this exact call's own
            // 'p' parameter, but never consumed until now) is the REAL
            // native draw-order value, and it is NOT always identical to
            // array position - confirmed to genuinely vary frame-to-frame
            // for k2_befired/k4_behited (e.g. k4_behited's own 'tou' part
            // goes from dBits=12 to dBits=14 between two of its own frames)
            // and to shift purely from SIBLING part visibility changes
            // (array index moves even when a part's OWN semantic layer has
            // not), which directly explains 'especially lower jaw overlap
            // incorrectly in hurt/bite/other non-walk states' - those are
            // exactly the animations where dBits-vs-array-index divergence
            // was found.
            //
            // TASK027N.2.51 FRESH 9-BUG BATCH (item 5) SUPERSEDES this
            // comment's own prior "preserving ties...not inventing a
            // secondary sort key" stance, per CLAUDE.md 4.9 - user
            // explicitly rejected "head can still get displayed over the
            // torso randomly... I want faithful reconstruction, not
            // defensive/guessed workarounds". Direct combat_manifest.json
            // inspection across every p_* player clip (p_stand/move/fire/
            // behit A-F, p_die, p_house, pd_house) found yyj4(torso) and
            // yyj6(head) ALWAYS share an EQUAL dBits value (both 0 or both
            // 2, never different) - i.e. this is not a rare edge case, it
            // is the native data's OWN permanent, by-design state for this
            // pair. A bare 'equal sortingOrder' leaves Unity's own internal
            // tie-break (sprite-batch/atlas-page order, not a stable game-
            // visible value) to decide draw order - genuinely
            // non-deterministic from the game's own perspective, which
            // matches the user's own "randomly" wording exactly. This is
            // NOT a guessed workaround: native 2D engines of this era
            // render parts by sequential array/command order (painter's
            // algorithm) with dBits as an explicit Z-test override only
            // where authored - so when dBits ties, the array position
            // itself (higher index drawn later = on top) IS native's own
            // implicit tie-break, already present in the very same proven
            // data, not invented. Folding it in as a true secondary key
            // (never able to override a genuine dBits difference, since no
            // clip's own frame.parts exceeds a few dozen entries) makes an
            // already-tied case fully deterministic without touching any
            // case where dBits genuinely differs.
            //
            // TASK027N.2.51 SECOND FRESH BATCH (item 1 - "dino spiky blood
            // renders under the dino head") SUPERSEDES the *1000+i integer
            // encoding immediately above, per CLAUDE.md 4.9 - that encoding
            // is semantically correct but has a side effect this session's
            // own further testimony exposed: MANY external effect systems
            // (DinoCapDamageParticle blood/chunks, DinoCapBloodBurst,
            // FProjectileParticle, WeaponEffectAnimation) key their OWN
            // draw order off a SMALL fixed offset from this SAME
            // baseSortingOrder (+9/+11/+12/+13/+60/+65), implicitly
            // assuming body-part sortingOrder never strays far from
            // baseSortingOrder+dBits (dBits observed up to ~14). Scaling
            // dBits by 1000 pushed body-part sortingOrder past ALL of
            // those small external offsets, so blood/chunks/effects (still
            // using the old small offsets) started rendering BEHIND body
            // parts instead of in front - a real regression this task's
            // own fresh testimony caught. FIX: keep sortingOrder EXACTLY
            // as base+dBits (restoring every external system's own already-
            // correct relationship to the body), and move the array-index
            // tie-break to a microscopic per-part Z offset instead (the
            // camera is confirmed orthographic - see SetupCamera's own
            // "c.orthographic=true" - so Unity's default Transparency Sort
            // Mode sorts same-sortingOrder sprites by world Z, giving the
            // exact same deterministic "higher array index drawn in front"
            // result without perturbing the sortingOrder integer itself).
            sr.sortingOrder=baseSortingOrder+p.dBits;

            float ds=manifest.sourceScale*actorScale;

            float x=p.x*ds;
            float y=p.y*ds;
            float w=p.w*ds;
            float h=p.h*ds;

            float pivotX=p.a*w*.5f;
            float pivotY=p.b*h*.5f;
            float c=p.c;

            if(!facingRight)
            {
                x=-x;
                pivotX=-pivotX;
                c=-c;
            }

            Vector2 pivot=new Vector2(pivotX,pivotY);
            Vector2 P=new Vector2(x,y)+pivot;

            float deg=(c+nativeRootRotationRadians)*Mathf.Rad2Deg;
            Vector2 rotated=Rotate(pivot,deg);

            // Task021B model C.
            Vector2 centre=P-rotated;

            // TASK027N.2.51 SECOND FRESH BATCH (item 1): the array-index
            // tie-break for equal-dBits parts (see sortingOrder's own
            // comment above) lives here instead, as a microscopic Z nudge -
            // higher 'i' gets a more-negative Z (closer to the orthographic
            // camera at Z=-10, Unity's default "camera looks down +Z"
            // orientation - confirmed via BuildWorld()'s own camera setup,
            // no rotation override), rendering in front for EQUAL
            // sortingOrder exactly like native's own painter's-algorithm
            // tie-break, without changing sortingOrder's own integer value
            // (which every external effect system's small fixed offset
            // from baseSortingOrder depends on staying undisturbed).
            sr.transform.localPosition=new Vector3(
                centre.x+drawOffset.x,
                centre.y+drawOffset.y,
                -i*0.0001f);
            sr.transform.localRotation=Quaternion.Euler(0,0,deg);
            sr.transform.localScale=new Vector3(
                w/sp.rect.width,
                h/sp.rect.height,
                1);

            sr.flipX=!facingRight;

            // Task027M.8.1: F3 X-ray presentation, FAIL-SAFE. Swap on the phase
            // transition only (one pool sweep, ~10/s). If the shader/material is
            // unavailable, ApplyXrayMaterial() leaves _xrayApplied=false and the
            // normal colour path below runs -> no magenta, normal rendering.
            if(xrayFlash && !_xrayApplied)
                ApplyXrayMaterial();
            else if(!xrayFlash && _xrayApplied)
                RestoreOriginalMaterials();
            // a part slot created by Ensure() AFTER the transition still needs it
            if(_xrayApplied && s_xrayMat!=null && sr.sharedMaterial!=s_xrayMat)
            {
                CaptureOriginalMaterials();
                sr.sharedMaterial=s_xrayMat;
            }

            uint q=unchecked((uint)p.rgba);
            if(xrOn)
            {
                // Task027M.8.3: the k*gt* bone sprite is already fully shaded
                // (dark shell + cool-white bones). Draw it at full white so the
                // authored pixels come through unmodified; keep the k*_be pose
                // alpha so the electrocution twitch still fades it in/out.
                sr.color=new Color32(255,255,255,(byte)(q>>24));
            }
            else if(xrayFlash && _xrayApplied)
            {
                // feed the X-ray shader the clean sprite masked by the authored
                // k*_be pose alpha; all look/colour comes from the material.
                sr.color=new Color32(255,255,255,(byte)(q>>24));
            }
            else
            {
                float cs=Mathf.Clamp(actorColourScale,0f,1f);
                sr.color=new Color32(
                    (byte)Mathf.Clamp(Mathf.RoundToInt(((byte)q)*cs),0,255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(((byte)(q>>8))*cs),0,255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(((byte)(q>>16))*cs),0,255),
                    (byte)(q>>24));
            }
        }

        // TASK027N.2.51 FOCUSED INTEGRATION PASS Bug 1 live-owner audit
        // ("player head display order can shift underneath torso after
        // exiting buildings"). Accessor exposing the part's ACTUAL live
        // SpriteRenderer.sortingOrder (read directly from pool[i], the
        // same object Draw() just wrote) rather than independently
        // recomputing the formula - TASK027N.2.51 FRESH 9-BUG BATCH found
        // this accessor had silently drifted stale (recomputing the OLD
        // base+dBits formula) after Draw()'s own formula was corrected to
        // base+dBits*1000+i, causing this test to fail against a FIXED
        // production bug by comparing against its own now-wrong
        // expectation, not a real regression. Reading the live renderer
        // directly eliminates this entire class of drift permanently.
        // Returns int.MinValue if the part is not present in the current
        // frame.
        public int TestHarness_PartSortingOrder(string partName)
        {
            DCFrame pf=primary.Current;
            if(pf==null || string.IsNullOrEmpty(partName))
                return int.MinValue;

            for(int i=0;i<pf.parts.Length;i++)
                if(pf.parts[i].name==partName)
                    return (pool!=null && i<pool.Count && pool[i]!=null) ? pool[i].sortingOrder : int.MinValue;
            return int.MinValue;
        }

        // TASK027N.2.51 SECOND FRESH BATCH (item 1): the live Z position
        // (the actual tie-break for equal sortingOrder, per Draw()'s own
        // "-i*0.0001f" comment above) - more negative is closer to camera/
        // drawn in front. Returns float.NaN if the part/pool slot doesn't
        // exist this frame.
        public float TestHarness_PartLocalZ(string partName)
        {
            DCFrame pf=primary.Current;
            if(pf==null || string.IsNullOrEmpty(partName))
                return float.NaN;
            for(int i=0;i<pf.parts.Length;i++)
                if(pf.parts[i].name==partName)
                    return (pool!=null && i<pool.Count && pool[i]!=null) ? pool[i].transform.localPosition.z : float.NaN;
            return float.NaN;
        }

        // Companion diagnostic: the actual pool-slot ARRAY INDEX ('i') this
        // part currently occupies - the index Ensure(i)/pool[i] uses to pick
        // which persistent, creation-order-fixed-sibling-index
        // SpriteRenderer GameObject renders it. If two different clips
        // place the SAME part name at DIFFERENT array positions, Unity's
        // own tie-break for EQUAL sortingOrder values (which this rig's own
        // data legitimately produces - see Draw()'s own 2.51 Part 4.7
        // comment) can differ between clips even though the authored
        // dBits value did not change.
        public int TestHarness_PartPoolIndex(string partName)
        {
            DCFrame pf=primary.Current;
            if(pf==null || string.IsNullOrEmpty(partName))
                return -1;

            for(int i=0;i<pf.parts.Length;i++)
                if(pf.parts[i].name==partName)
                    return i;

            return -1;
        }

        public bool TryGetPartWorldAnchor(string partName, out Vector3 world)
        {
            world=transform.position;

            DCFrame pf=primary.Current;
            if(pf==null || string.IsNullOrEmpty(partName))
                return false;

            DCPart chosen=null;
            for(int i=0;i<pf.parts.Length;i++)
            {
                if(pf.parts[i].name==partName)
                {
                    chosen=ClonePart(pf.parts[i]);
                    break;
                }
            }

            if(chosen==null)
                return false;

            if(SecondaryComposed && secondary.Current!=null)
            {
                DCPart s=null;
                DCPart b=null;

                for(int i=0;i<secondary.Current.parts.Length;i++)
                    if(secondary.Current.parts[i].name==partName)
                    { s=secondary.Current.parts[i]; break; }

                if(secondary.FrameZero!=null)
                    for(int i=0;i<secondary.FrameZero.parts.Length;i++)
                        if(secondary.FrameZero.parts[i].name==partName)
                        { b=secondary.FrameZero.parts[i]; break; }

                if(s!=null)
                {
                    float dx=s.x,dy=s.y,dc=s.c;
                    if(b!=null)
                    {
                        dx=s.x-b.x;
                        dy=s.y-b.y;
                        dc=WrapPi(s.c-b.c);
                    }

                    chosen.x+=dx;
                    chosen.y+=dy;
                    chosen.c=Wrap0To2Pi(chosen.c+dc);
                    chosen.a=s.a;
                    chosen.b=s.b;
                    chosen.@ref=s.Ref;
                    chosen.refName=s.Ref;
                }
            }

            float ds=manifest.sourceScale*actorScale;
            float x=chosen.x*ds;
            float y=chosen.y*ds;
            float w=chosen.w*ds;
            float h=chosen.h*ds;
            float pivotX=chosen.a*w*.5f;
            float pivotY=chosen.b*h*.5f;

            if(!facingRight)
            {
                x=-x;
                pivotX=-pivotX;
            }

            // Task021B/Model-C named-part anchor P = position + pivot offset.
            world=transform.TransformPoint(new Vector3(x+pivotX,y+pivotY,0f));
            return true;
        }

        // TASK027N.2.9: exposes a named part's own local half-width/height
        // (native sub_172CC0's {W,H} read) - needed to compute the Blood
        // Impact Burst's randomized in-bbox spawn offset (native sub_10ECC).
        // Width/height are magnitude-only in native too, so this
        // deliberately skips the secondary-track delta and facing-mirror
        // TryGetPartWorldAnchor applies to POSITION.
        public bool TryGetPartHalfExtents(
            string partName,out float halfWidth,out float halfHeight)
        {
            halfWidth=0f;
            halfHeight=0f;

            DCFrame pf=primary.Current;
            if(pf==null || string.IsNullOrEmpty(partName))
                return false;

            DCPart chosen=null;
            for(int i=0;i<pf.parts.Length;i++)
                if(pf.parts[i].name==partName)
                { chosen=pf.parts[i]; break; }

            if(chosen==null)
                return false;

            float ds=manifest.sourceScale*actorScale;
            halfWidth=chosen.w*ds*.5f;
            halfHeight=chosen.h*ds*.5f;
            return true;
        }

        public bool TryGetPartWorldSocket(
            string partName,
            float widthFactor,
            float heightFactor,
            out Vector3 world,
            out float rotationDeg,
            out Vector2 size)
        {
            world=transform.position;
            rotationDeg=0f;
            size=Vector2.zero;

            DCFrame pf=primary.Current;
            if(pf==null || string.IsNullOrEmpty(partName))
                return false;

            DCPart chosen=null;
            for(int i=0;i<pf.parts.Length;i++)
                if(pf.parts[i].name==partName)
                { chosen=ClonePart(pf.parts[i]); break; }

            if(chosen==null)
                return false;

            if(SecondaryComposed && secondary.Current!=null)
            {
                DCPart s=null,b=null;
                for(int i=0;i<secondary.Current.parts.Length;i++)
                    if(secondary.Current.parts[i].name==partName)
                    { s=secondary.Current.parts[i]; break; }
                if(secondary.FrameZero!=null)
                    for(int i=0;i<secondary.FrameZero.parts.Length;i++)
                        if(secondary.FrameZero.parts[i].name==partName)
                        { b=secondary.FrameZero.parts[i]; break; }

                if(s!=null)
                {
                    float dx=s.x,dy=s.y,dc=s.c;
                    if(b!=null)
                    {
                        dx=s.x-b.x;
                        dy=s.y-b.y;
                        dc=WrapPi(s.c-b.c);
                    }
                    chosen.x+=dx;
                    chosen.y+=dy;
                    chosen.c=Wrap0To2Pi(chosen.c+dc);
                    chosen.a=s.a;
                    chosen.b=s.b;
                    // Native keeps PRIMARY W/H during secondary composition.
                }
            }

            float ds=manifest.sourceScale*actorScale;
            float x=chosen.x*ds;
            float y=chosen.y*ds;
            float w=chosen.w*ds;
            float h=chosen.h*ds;
            float px=chosen.a*w*.5f;
            float py=chosen.b*h*.5f;
            float c=chosen.c;

            if(!facingRight)
            {
                x=-x;
                px=-px;
                c=-c;
            }

            Vector2 anchorLocal=new Vector2(x+px,y+py); // Model-C P
            rotationDeg=(c+nativeRootRotationRadians)*Mathf.Rad2Deg;

            Vector2 pivotV=new Vector2(px,py);
            Vector2 spriteCentreLocal=
                anchorLocal-Rotate(pivotV,rotationDeg);

            float ox=widthFactor*w;
            if(!facingRight)
                ox=-ox;

            Vector2 offset=Rotate(
                new Vector2(ox,heightFactor*h),rotationDeg);

            world=transform.TransformPoint(
                new Vector3(spriteCentreLocal.x+offset.x,
                            spriteCentreLocal.y+offset.y,0f));
            size=new Vector2(w,h);
            return true;
        }

        /// <summary>
        /// Task027K1 source-faithful named-part socket:
        /// Model-C anchor P + a width/height fraction in the part's rotated
        /// local frame. Unlike TryGetPartWorldSocket(), the base point is the
        /// authored anchor P, not the sprite visual centre.
        /// </summary>
        public bool TryGetPartWorldAnchorSocket(
            string partName,
            float widthFactor,
            float heightFactor,
            out Vector3 world,
            out float rotationDeg,
            out Vector2 size)
        {
            world=transform.position;
            rotationDeg=0f;
            size=Vector2.zero;

            DCFrame pf=primary.Current;
            if(pf==null || string.IsNullOrEmpty(partName))
                return false;

            DCPart chosen=null;
            for(int i=0;i<pf.parts.Length;i++)
                if(pf.parts[i].name==partName)
                { chosen=ClonePart(pf.parts[i]); break; }

            if(chosen==null)
                return false;

            if(SecondaryComposed && secondary.Current!=null)
            {
                DCPart s=null,b=null;
                for(int i=0;i<secondary.Current.parts.Length;i++)
                    if(secondary.Current.parts[i].name==partName)
                    { s=secondary.Current.parts[i]; break; }

                if(secondary.FrameZero!=null)
                    for(int i=0;i<secondary.FrameZero.parts.Length;i++)
                        if(secondary.FrameZero.parts[i].name==partName)
                        { b=secondary.FrameZero.parts[i]; break; }

                if(s!=null)
                {
                    float dx=s.x,dy=s.y,dc=s.c;
                    if(b!=null)
                    {
                        dx=s.x-b.x;
                        dy=s.y-b.y;
                        dc=WrapPi(s.c-b.c);
                    }

                    chosen.x+=dx;
                    chosen.y+=dy;
                    chosen.c=Wrap0To2Pi(chosen.c+dc);
                    chosen.a=s.a;
                    chosen.b=s.b;
                    // Native keeps PRIMARY W/H during secondary composition.
                }
            }

            float ds=manifest.sourceScale*actorScale;
            float x=chosen.x*ds;
            float y=chosen.y*ds;
            float w=chosen.w*ds;
            float h=chosen.h*ds;
            float px=chosen.a*w*.5f;
            float py=chosen.b*h*.5f;
            float c=chosen.c;

            if(!facingRight)
            {
                x=-x;
                px=-px;
                c=-c;
            }

            Vector2 anchorLocal=new Vector2(x+px,y+py); // Model-C P
            rotationDeg=(c+nativeRootRotationRadians)*Mathf.Rad2Deg;

            float ox=widthFactor*w;
            if(!facingRight)
                ox=-ox;

            Vector2 offset=Rotate(
                new Vector2(ox,heightFactor*h),rotationDeg);

            world=transform.TransformPoint(
                new Vector3(anchorLocal.x+offset.x,
                            anchorLocal.y+offset.y,0f));
            size=new Vector2(w,h);
            return true;
        }

        static float WrapPi(float v)
        {
            float twoPi=Mathf.PI*2f;

            while(v>Mathf.PI)
                v-=twoPi;

            while(v<-Mathf.PI)
                v+=twoPi;

            return v;
        }

        static float Wrap0To2Pi(float v)
        {
            float twoPi=Mathf.PI*2f;

            while(v>=twoPi)
                v-=twoPi;

            while(v<0f)
                v+=twoPi;

            return v;
        }

        static Vector2 Rotate(Vector2 v,float deg)
        {
            float r=deg*Mathf.Deg2Rad;
            float cs=Mathf.Cos(r);
            float sn=Mathf.Sin(r);

            return new Vector2(
                v.x*cs-v.y*sn,
                v.x*sn+v.y*cs);
        }

        void Ensure(int i)
        {
            while(pool.Count<=i)
            {
                GameObject g=new GameObject("part_"+pool.Count);
                g.transform.SetParent(actorPartsRoot!=null?actorPartsRoot:transform,false);
                SpriteRenderer nsr=g.AddComponent<SpriteRenderer>();
                pool.Add(nsr);
                // Task027M.8.1: keep the original-material list aligned with the pool.
                origMat.Add(nsr.sharedMaterial);
                EnsureSequenceSlot(pool.Count-1);
            }
        }

        void HideFrom(int n)
        {
            for(int i=n;i<pool.Count;i++)
            {
                pool[i].enabled=false;
                ResetSequenceSlot(i);
            }
        }
    }
}
