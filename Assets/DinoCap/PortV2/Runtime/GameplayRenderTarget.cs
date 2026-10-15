using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Task025C compatibility framebuffer.
    ///
    /// The shipped iPad game rasterised its 1024x768 logical world into a
    /// 1024x768 no-MSAA framebuffer. Modern display scaling happened after
    /// that rasterisation.
    ///
    /// 1x preserves that source-faithful contract. The optional persistent
    /// 2x mode is an explicit modern visual enhancement: source-space geometry
    /// is unchanged, but the framebuffer gets 2x2 samples per source unit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayRenderTarget : MonoBehaviour
    {
        const string SupersamplePlayerPrefsKey = "DC1_InternalSupersample2x";

        // TASK027N.2.51 RUNTIME_TRUTH_RESET (supersedes the N.2.49R REOPEN2
        // Phase 3 policy below, preserved per CLAUDE.md 4.9): fresh user
        // runtime testing explicitly rejected SourceFaithful4x3 as the
        // NORMAL production default - "4:3 mode still dominates instead of
        // adapting correctly to Unity Play/Game-view resolution/aspect".
        // Per the reopening prompt's own explicit instruction ("do not
        // retain a 'Faithful 4:3' mode as the normal production
        // presentation"), WidescreenExpand (which already correctly derives
        // visible world extent from the actual active viewport - see
        // ComputeLogicalWidth below) is now the DEFAULT normal runtime
        // mode. SourceFaithful4x3 remains fully available as an explicit
        // REFERENCE/DEBUG mode (exact native 1024x768 reproduction for
        // fidelity comparison), not deleted - just no longer the silent
        // default a normal player experiences.
        // MODERN_VISUAL_ENHANCEMENT classification unchanged: WidescreenExpand
        // may show world regions native never displayed on its own fixed-4:3
        // hardware - this is an intentional, disclosed port-policy choice
        // (CLAUDE.md 11.2), not a claim that native itself was widescreen.
        //
        // HISTORICAL (superseded) rationale, preserved per CLAUDE.md 4.9:
        // "The shipped iPad game's own logical view is fixed 4:3 (1024x768)
        // - this is the SOURCE_FAITHFUL default, reproducing exactly what
        // native ever displayed. WIDESCREEN_EXPAND (this port's own prior
        // default, Task024D/E) is retained as an explicit, reversible,
        // persistent MODERN_VISUAL_ENHANCEMENT opt-in."
        public enum PresentationAspectMode { SourceFaithful4x3 = 0, WidescreenExpand = 1 }
        const string AspectModePlayerPrefsKey = "DC1_PresentationAspectMode";

        static bool aspectModeLoaded;
        static PresentationAspectMode aspectMode;

        static bool supersampleLoaded;
        static bool supersample2x;
        public static void ReloadPreferences(){aspectModeLoaded=false;supersampleLoaded=false;}

        Camera worldCamera;
        RenderTexture target;
        int sourceWidth;
        int sourceHeight;
        int currentWidth;
        int currentHeight;

        public RenderTexture Target { get { return target; } }
        public int SourceWidth { get { return sourceWidth; } }
        public int SourceHeight { get { return sourceHeight; } }

        public static bool Supersample2xEnabled
        {
            get
            {
                EnsureSupersampleSettingLoaded();
                return supersample2x;
            }
        }

        public static int InternalScale
        {
            get { return Supersample2xEnabled ? 2 : 1; }
        }

        public static string InternalScaleName
        {
            get { return Supersample2xEnabled ? "2X" : "1X"; }
        }

        static void EnsureSupersampleSettingLoaded()
        {
            if(supersampleLoaded)
                return;

            // Explicit user preference: fresh/reset saves default to2x.
            // An existing manual1x/2x selection remains authoritative.
            supersample2x =
                DC1Preferences.GetInt(SupersamplePlayerPrefsKey,1)!=0;
            supersampleLoaded=true;
        }

        public static void TogglePersistentSupersample()
        {
            EnsureSupersampleSettingLoaded();
            supersample2x=!supersample2x;

            DC1Preferences.SetInt(
                SupersamplePlayerPrefsKey,
                supersample2x ? 1 : 0);
            DC1Preferences.Save();

            Debug.Log(
                "[DC1-V24] Internal framebuffer = "+
                InternalScaleName+
                " (persistent)");
        }

        public static PresentationAspectMode AspectMode
        {
            get
            {
                EnsureAspectModeLoaded();
                return aspectMode;
            }
        }

        public static bool IsSourceFaithful4x3
        {
            get { return AspectMode == PresentationAspectMode.SourceFaithful4x3; }
        }

        static void EnsureAspectModeLoaded()
        {
            if(aspectModeLoaded)
                return;

            // TASK027N.2.51 RUNTIME_TRUTH_RESET: WidescreenExpand is now the
            // default (see the enum's own header comment above for the full
            // policy change and historical rationale). SourceFaithful4x3
            // remains available as an explicit, reversible REFERENCE/DEBUG
            // opt-in via TogglePersistentAspectMode/SetPersistentAspectMode.
            aspectMode = (PresentationAspectMode)DC1Preferences.GetInt(
                AspectModePlayerPrefsKey, (int)PresentationAspectMode.WidescreenExpand);
            aspectModeLoaded = true;
        }

        public static void SetPersistentAspectMode(PresentationAspectMode mode)
        {
            EnsureAspectModeLoaded();
            aspectMode = mode;

            DC1Preferences.SetInt(AspectModePlayerPrefsKey, (int)mode);
            DC1Preferences.Save();

            Debug.Log("[DC1-V33] Presentation aspect mode = " + mode + " (persistent)");
        }

        public static void TogglePersistentAspectMode()
        {
            EnsureAspectModeLoaded();
            SetPersistentAspectMode(
                aspectMode == PresentationAspectMode.SourceFaithful4x3
                    ? PresentationAspectMode.WidescreenExpand
                    : PresentationAspectMode.SourceFaithful4x3);
        }

        public void Initialize(Camera cameraToOwn, int canonicalWidth, int canonicalHeight)
        {
            worldCamera = cameraToOwn;
            sourceWidth = Mathf.Max(1, canonicalWidth);
            sourceHeight = Mathf.Max(1, canonicalHeight);

            // Native source path had no MSAA. This is intentionally camera-local
            // rather than changing unrelated project cameras/quality settings.
            worldCamera.allowMSAA = false;
            worldCamera.allowDynamicResolution = false;

            EnsureTarget();
        }

        void LateUpdate()
        {
            if (worldCamera != null)
                EnsureTarget();
        }

        void EnsureTarget()
        {
            int internalScale=InternalScale;
            int logicalWidth=ComputeLogicalWidthForDisplay();
            int wantedWidth = logicalWidth*internalScale;
            int wantedHeight = sourceHeight*internalScale;

            // MODERN_VISUAL_ENHANCEMENT bound (user-chosen 2X is "up to 2x2 samples per source
            // unit"): never rasterize more pixels than the display can show. Beyond the display's
            // own height the extra samples are only bilinearly resampled back down by the
            // presentation shader (a second filter that softens the image) while costing area
            // quadratically in fill. Measured on a 2400x1080 phone: 2X = 3414x1536 (5.2 MP, 2.0x
            // the display) missed 60 fps in City Grind/Jungle; the display-native target is
            // 2400x1080 with no resampling. 1X (source-faithful 1:1) is never altered. Bounded
            // below by 1X, above by the requested scale; no tuned constant.
            if(internalScale>1 && DisplayCapActive)
            {
                int cappedHeight=Mathf.Clamp(DisplayHeightForCap(), sourceHeight, wantedHeight);
                if(cappedHeight<wantedHeight)
                {
                    wantedHeight=cappedHeight;
                    wantedWidth=Mathf.Max(1,Mathf.RoundToInt(logicalWidth*(float)cappedHeight/sourceHeight));
                }
            }

            if (target != null &&
                currentWidth == wantedWidth &&
                currentHeight == wantedHeight)
            {
                if (worldCamera.targetTexture != target)
                    worldCamera.targetTexture = target;
                return;
            }

            ReleaseTarget();

            RenderTextureDescriptor d =
                new RenderTextureDescriptor(
                    wantedWidth,
                    wantedHeight,
                    RenderTextureFormat.ARGB32,
                    24);

            d.msaaSamples = 1;
            d.useMipMap = false;
            d.autoGenerateMips = false;
            d.sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);

            target = new RenderTexture(d);
            target.name = "DC1_SourceGameplayFramebuffer";
            target.filterMode = FilterMode.Point;
            target.wrapMode = TextureWrapMode.Clamp;
            target.useMipMap = false;
            target.autoGenerateMips = false;
            target.Create();

            currentWidth = wantedWidth;
            currentHeight = wantedHeight;

            worldCamera.targetTexture = target;

            Debug.Log("[DC1-V24] Gameplay framebuffer " +
                currentWidth + "x" + currentHeight +
                " ARGB32, MSAA=1, mips=off, internal=" +
                InternalScaleName +
                (InternalScale==1
                    ? ", faithful world->RT=1:1."
                    : ", modern supersample=2x2 samples/source-unit."));
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Dev-only A/B switch for measurement (-dc1-smoke-displaycap 0|1); -1 = production rule.
        public static int DisplayCapOverride=-1;
#endif
        // PLATFORM_ADAPTER scope: applied on mobile platforms, where fill rate is the limit.
        // Desktop keeps its user-accepted uncapped 2X until the user chooses otherwise.
        static bool DisplayCapActive
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if(DisplayCapOverride>=0) return DisplayCapOverride!=0;
#endif
                return Application.isMobilePlatform;
            }
        }

        static int DisplayHeightForCap()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(DiagnosticViewportSize.x>0 && DiagnosticViewportSize.y>0) return DiagnosticViewportSize.y;
#endif
            return Screen.height;
        }

        int ComputeLogicalWidthForDisplay()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Capture-only virtual viewport exercises expanded production
            // world/UI rendering when the host monitor clamps player windows.
            // Physical screen output and virtual framebuffer exports are
            // recorded separately; this does not emulate display hardware.
            if(DiagnosticViewportSize.x>0 && DiagnosticViewportSize.y>0)
                return ComputeLogicalWidth(sourceWidth,sourceHeight,DiagnosticViewportSize.x,DiagnosticViewportSize.y,AspectMode);
#endif
            return ComputeLogicalWidth(sourceWidth, sourceHeight, Screen.width, Screen.height, AspectMode);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static Vector2Int DiagnosticViewportSize;
#endif

        // TASK027N.2.49R REOPEN2 Phase 3: pure, parameterized, independently
        // testable core of the width computation - the exact same formula
        // ComputeLogicalWidthForDisplay() uses, extracted so automated tests
        // can validate every mandated display size (1024x768, 1280x720,
        // 1920x1080, 2560x1440, 3440x1440, and a tall/narrow window)
        // without needing to actually resize the OS window under test.
        public static int ComputeLogicalWidth(
            int sourceWidth, int sourceHeight,
            int displayWidth, int displayHeight,
            PresentationAspectMode mode)
        {
            // SourceFaithful4x3 NEVER widens the logical framebuffer beyond
            // the shipped iPad's own fixed 1024 width - GameplayPresentationCamera's
            // own existing fit-to-screen math already pillarboxes a
            // narrower-than-screen RT correctly (rtAspect<screenAspect ->
            // width scaled down, height stays full), so a fixed 4:3 RT
            // naturally produces a correct pillarboxed presentation on any
            // wider display with zero further logic needed here.
            if (mode == PresentationAspectMode.SourceFaithful4x3)
                return sourceWidth;

            if (displayHeight <= 0)
                return sourceWidth;

            float displayAspect = (float)displayWidth / displayHeight;

            // Task024D/E widescreen policy (WidescreenExpand mode only):
            // keep 768 logical pixels high, widen horizontally as needed.
            // Tall/narrow displays retain at least the canonical 1024 width
            // and are letterboxed by GameplayPresenter.
            int aspectWidth = Mathf.CeilToInt(sourceHeight * displayAspect);

            return Mathf.Max(sourceWidth, aspectWidth);
        }

        void OnDisable()
        {
            ReleaseTarget();
        }

        void OnDestroy()
        {
            ReleaseTarget();
        }

        void ReleaseTarget()
        {
            if (worldCamera != null && worldCamera.targetTexture == target)
                worldCamera.targetTexture = null;

            if (target != null)
            {
                if (target.IsCreated())
                    target.Release();

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(target);
                else
#endif
                    Destroy(target);
            }

            target = null;
            currentWidth = 0;
            currentHeight = 0;
        }
    }
}
