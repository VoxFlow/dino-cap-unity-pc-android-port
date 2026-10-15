using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Fixed-source-framebuffer presenter.
    ///
    /// Gameplay still rasterises into the source RT exactly as Task025C/V14
    /// requires. This camera only resamples the COMPLETED flat framebuffer for
    /// the physical display, so changing presentation quality cannot sample a
    /// neighbouring source-atlas frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayPresentationCamera : MonoBehaviour
    {
        public enum PresentationMode
        {
            SmoothModern = 0,
            OriginalPoint = 1
        }

        const int PresentationLayer = 31;
        const string ShaderName = "Hidden/DinoCap/DC1Present";

        static GameplayPresentationCamera instance;
        static PresentationMode globalMode = PresentationMode.SmoothModern;

        GameplayRenderTarget source;
        Camera presentationCamera;
        Camera worldCamera;
        GameObject surface;
        MeshRenderer surfaceRenderer;
        Material material;

        public static string GlobalModeName
        {
            get
            {
                return globalMode == PresentationMode.SmoothModern
                    ? "Smooth"
                    : "Original";
            }
        }

        public static void CycleGlobalMode()
        {
            globalMode =
                globalMode == PresentationMode.SmoothModern
                    ? PresentationMode.OriginalPoint
                    : PresentationMode.SmoothModern;

            if (instance != null)
                instance.ApplyMode();

            Debug.Log("[DC1-V17] Presentation mode = " + GlobalModeName);
        }

        public static GameplayPresentationCamera CreateOrInitialize(
            GameplayRenderTarget renderTarget,
            Camera sourceWorldCamera)
        {
            GameObject go = GameObject.Find("DC1_PresentationCamera");
            if (go == null)
                go = new GameObject("DC1_PresentationCamera");

            GameplayPresentationCamera presenter =
                go.GetComponent<GameplayPresentationCamera>();
            if (presenter == null)
                presenter = go.AddComponent<GameplayPresentationCamera>();

            presenter.Initialize(renderTarget, sourceWorldCamera);
            return presenter;
        }

        public void Initialize(
            GameplayRenderTarget renderTarget,
            Camera sourceWorldCamera)
        {
            instance = this;
            source = renderTarget;
            worldCamera = sourceWorldCamera;

            presentationCamera = GetComponent<Camera>();
            if (presentationCamera == null)
                presentationCamera = gameObject.AddComponent<Camera>();

            gameObject.layer = PresentationLayer;
            gameObject.tag = "Untagged";

            presentationCamera.orthographic = true;
            presentationCamera.orthographicSize = 0.5f;
            presentationCamera.transform.position =
                new Vector3(0f, 0f, -10f);
            presentationCamera.clearFlags = CameraClearFlags.SolidColor;
            presentationCamera.backgroundColor = Color.black;
            presentationCamera.cullingMask = 1 << PresentationLayer;
            presentationCamera.depth = 100f;
            presentationCamera.targetTexture = null;
            presentationCamera.targetDisplay = 0;
            presentationCamera.allowMSAA = false;
            presentationCamera.allowDynamicResolution = false;

            if (worldCamera != null)
                worldCamera.cullingMask &= ~(1 << PresentationLayer);

            EnsureSurface();
            ApplyMode();
            UpdateSurface();
        }

        void LateUpdate()
        {
            UpdateSurface();
        }

        void EnsureSurface()
        {
            if (surface != null)
                return;

            surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
            surface.name = "DC1_SourceFramebufferSurface";
            surface.layer = PresentationLayer;
            surface.transform.SetParent(transform, false);
            surface.transform.localPosition = new Vector3(0f, 0f, 10f);

            Collider col = surface.GetComponent<Collider>();
            if (col != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(col);
                else
#endif
                    Destroy(col);
            }

            surfaceRenderer = surface.GetComponent<MeshRenderer>();

            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError(
                    "[DC1-V17] Missing " + ShaderName +
                    "; falling back to Unlit/Texture.");
                shader = Shader.Find("Unlit/Texture");
            }

            material = new Material(shader);
            material.name = "DC1_SourceFramebufferPresentation";
            surfaceRenderer.sharedMaterial = material;
        }

        void ApplyMode()
        {
            if (material != null && material.HasProperty("_Mode"))
                material.SetFloat(
                    "_Mode",
                    globalMode == PresentationMode.SmoothModern ? 1f : 0f);
        }

        void UpdateSurface()
        {
            if (source == null || source.Target == null ||
                presentationCamera == null)
                return;

            EnsureSurface();

            RenderTexture rt = source.Target;

            // Keep source RT taps exact. The presentation shader performs its
            // own reconstruction from texel-centre samples.
            rt.filterMode = FilterMode.Point;
            rt.wrapMode = TextureWrapMode.Clamp;

            if (material != null)
            {
                if (material.mainTexture != rt)
                    material.mainTexture = rt;

                if (material.HasProperty("_SourceSize"))
                    material.SetVector(
                        "_SourceSize",
                        new Vector4(
                            rt.width,
                            rt.height,
                            1f / rt.width,
                            1f / rt.height));

                if (material.HasProperty("_Mode"))
                    material.SetFloat(
                        "_Mode",
                        globalMode == PresentationMode.SmoothModern ? 1f : 0f);
            }

            Vector2 fit = ComputeFitScale(rt.width, rt.height, Screen.width, Screen.height);

            surface.transform.localScale =
                new Vector3(fit.x, fit.y, 1f);
        }

        // TASK027N.2.49R REOPEN2 Phase 3: pure, parameterized, independently
        // testable core of the contain-aspect-ratio fit computation, so
        // automated tests can validate pillarbox/letterbox behavior at
        // every mandated display size without needing to resize the OS
        // window under test.
        public static Vector2 ComputeFitScale(int rtWidth, int rtHeight, int displayWidth, int displayHeight)
        {
            float screenAspect =
                displayHeight > 0
                    ? (float)displayWidth / displayHeight
                    : 4f / 3f;
            float rtAspect = (float)rtWidth / rtHeight;

            float width;
            float height;

            if (rtAspect >= screenAspect)
            {
                width = screenAspect;
                height = screenAspect / rtAspect;
            }
            else
            {
                height = 1f;
                width = rtAspect;
            }

            return new Vector2(width, height);
        }

        // TASK027N.2.49R REOPEN2 Phase 5 - logical coordinate mapping. The
        // presentation quad (this class's own surface) is a centered,
        // contain-fit rectangle within the physical screen - this is the
        // ACTUAL pillarboxed/letterboxed game-view rect a pointer must be
        // mapped through before it means anything in game-logical space.
        // Derived directly from ComputeFitScale's own already-verified
        // geometry (quad world-size == fit * presentation-camera's own
        // orthographic view-size, itself always exactly screenAspect wide
        // x 1 tall since orthographicSize=0.5 with no aspect override) -
        // not a new, independently-unverified assumption.
        public static Rect ComputeGameViewScreenRect(int rtWidth, int rtHeight, int displayWidth, int displayHeight)
        {
            if (displayWidth <= 0 || displayHeight <= 0)
                return new Rect(0, 0, displayWidth, displayHeight);

            float screenAspect = (float)displayWidth / displayHeight;
            Vector2 fit = ComputeFitScale(rtWidth, rtHeight, displayWidth, displayHeight);

            // fraction of full screen the quad occupies, per-axis
            float widthFraction = fit.x / screenAspect;
            float heightFraction = fit.y; // camera's own vertical extent is always exactly 1.0

            float gameViewWidth = displayWidth * widthFraction;
            float gameViewHeight = displayHeight * heightFraction;

            // CG-05 supersedes N2.51 Section20 integer snapping (baseline archived).
            // Hit testing and HUD drawing must use the same continuous fit
            // as UpdateSurface. Rounding up (especially the 1x minimum)
            // put controls outside small Game views and outside the quad.

            float offsetX = (displayWidth - gameViewWidth) * 0.5f;
            float offsetY = (displayHeight - gameViewHeight) * 0.5f;

            return new Rect(offsetX, offsetY, gameViewWidth, gameViewHeight);
        }

        public Rect GetCurrentGameViewScreenRect()
        {
            if (source == null || source.Target == null)
                return new Rect(0, 0, Screen.width, Screen.height);

            return ComputeGameViewScreenRect(source.Target.width, source.Target.height, Screen.width, Screen.height);
        }

        // TASK027N.2.49R REOPEN2 Phase 5 - maps a screen-space pointer
        // position (GUI convention: origin top-left, Y-down - i.e. exactly
        // what Input.mousePosition-after-Y-flip or a GUI Event.mousePosition
        // already is) through the ACTUAL current game-view rect into
        // logical coordinates in the RT's own current logical space (which
        // may be 1024x768 exactly, or wider under WidescreenExpand - the
        // caller decides what "logical" means for its own purposes by
        // passing whatever logicalWidth/Height it needs mapped into).
        // Returns false (point stays default) when the input point falls
        // outside the game-view rect entirely - i.e. a click in a
        // pillarbox/letterbox black bar, which per this phase's own
        // explicit requirement must NOT activate any game control.
        public static bool ScreenToLogical(
            Vector2 screenPointGUI, Rect gameViewRect,
            float logicalWidth, float logicalHeight,
            out Vector2 logicalPoint)
        {
            if (!gameViewRect.Contains(screenPointGUI) || gameViewRect.width<=0f || gameViewRect.height<=0f)
            {
                logicalPoint = default;
                return false;
            }

            float u = (screenPointGUI.x - gameViewRect.x) / gameViewRect.width;
            float v = (screenPointGUI.y - gameViewRect.y) / gameViewRect.height;

            logicalPoint = new Vector2(u * logicalWidth, v * logicalHeight);
            return true;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;

            if (material != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(material);
                else
#endif
                    Destroy(material);
            }
        }
    }
}
