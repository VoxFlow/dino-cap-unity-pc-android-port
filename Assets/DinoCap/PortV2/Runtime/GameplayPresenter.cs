using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Presents the already-rasterised source gameplay framebuffer to any
    /// physical display. Fractional final scaling may be bilinear because it
    /// can no longer sample across source atlas-frame boundaries.
    ///
    /// This is intentionally a lightweight sandbox presenter. Future City/UI
    /// composition can move this to a dedicated presentation layer without
    /// changing the gameplay framebuffer contract.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayPresenter : MonoBehaviour
    {
        GameplayRenderTarget source;

        public void Initialize(GameplayRenderTarget renderTarget)
        {
            source = renderTarget;
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint ||
                source == null ||
                source.Target == null)
                return;

            RenderTexture rt = source.Target;
            Rect destination = FitInsideScreen(rt.width, rt.height);

            // Draw presentation behind the sandbox/debug IMGUI.
            int oldDepth = GUI.depth;
            Color oldColor = GUI.color;

            GUI.depth = 1000;

            // Explicit bars for tall/narrow displays.
            GUI.color = Color.black;
            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                Texture2D.whiteTexture,
                ScaleMode.StretchToFill,
                false);

            GUI.color = Color.white;

            // Task025D: preserve every authored source-framebuffer texel row
            // during final presentation. The faint top rows of the shared
            // 'yingzi' shadow sprite are a real source feature. Fractional
            // Bilinear presentation averaged that ~1-row rim into the wider
            // shadow body at common desktop scales. The world RT itself is
            // already clean and atlas-safe, so present the finished image with
            // nearest replication at every scale.
            rt.filterMode = FilterMode.Point;

            GUI.DrawTexture(
                destination,
                rt,
                ScaleMode.StretchToFill,
                false);

            GUI.color = oldColor;
            GUI.depth = oldDepth;
        }

        static Rect FitInsideScreen(int textureWidth, int textureHeight)
        {
            if (textureWidth <= 0 || textureHeight <= 0 ||
                Screen.width <= 0 || Screen.height <= 0)
                return new Rect(0f, 0f, Screen.width, Screen.height);

            float scale = Mathf.Min(
                (float)Screen.width / textureWidth,
                (float)Screen.height / textureHeight);

            float w = textureWidth * scale;
            float h = textureHeight * scale;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            return new Rect(x, y, w, h);
        }
    }
}
