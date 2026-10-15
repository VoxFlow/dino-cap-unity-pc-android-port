using UnityEngine;

namespace DinoCap.PortV2
{
    // Native4:3 coordinates are preserved exactly. Adaptive displays add
    // source units horizontally; source UI sizes always use one uniform scale.
    // Left/right HUD groups stay at their respective edges, modal/Menu UI
    // stays centered, world UI follows camera projection. These wider-screen
    // anchors are an explicit port presentation policy, not native behavior.
    internal static class NativeViewport
    {
        internal enum Anchor { Left, Center, Right }
        // Screen-pixel safe-area insets (left/right edge), set each frame by DC1PlatformIntegration on mobile.
        internal static float SafeInsetLeft,SafeInsetRight;
        internal static Rect Map(Rect native,Rect view,Anchor anchor=Anchor.Center)
        {
            if(view.width<=0f || view.height<=0f)return native;
            float scale=view.height/768f;
            float extra=view.width-1024f*scale;
            // PLATFORM_ADAPTER (notch/cutout safe area, mobile only; both insets are 0 elsewhere): edge-anchored UI is moved
            // inward by the part of the cutout inset that is not already covered by letter/pillar-box bars. Native layout is
            // otherwise unchanged. Applies identically to drawing (Map) and touch zones (PointerRect uses Map).
            // Bars are clamped at 0 so a view that extends past the screen can never create an inset when the cutout inset is 0.
            float insetL=SafeInsetLeft<=0f ? 0f : Mathf.Max(0f,SafeInsetLeft-Mathf.Max(0f,view.x));
            float insetR=SafeInsetRight<=0f ? 0f : Mathf.Max(0f,SafeInsetRight-Mathf.Max(0f,Screen.width-view.xMax));
            float offset=anchor==Anchor.Left?insetL:anchor==Anchor.Right?extra-insetR:extra*0.5f;
            return new Rect(view.x+native.x*scale+offset,view.y+native.y*scale,
                native.width*scale,native.height*scale);
        }
        // Existing pointer adapter uses normalized1024x768 coordinates.
        // Convert the displayed rectangle back into that same input space;
        // capture/drag/hold behavior remains owned by the existing adapter.
        internal static Rect PointerRect(Rect native,Rect view,Anchor anchor)
        {
            if(view.width<=0f || view.height<=0f)return native;
            Rect screen=Map(native,view,anchor);
            return new Rect((screen.x-view.x)*1024f/view.width,
                (screen.y-view.y)*768f/view.height,screen.width*1024f/view.width,
                screen.height*768f/view.height);
        }
    }
}
