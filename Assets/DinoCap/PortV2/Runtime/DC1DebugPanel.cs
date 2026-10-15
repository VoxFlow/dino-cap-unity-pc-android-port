using UnityEngine;

namespace DinoCap.PortV2
{
    public static class DC1DebugPanel
    {
        static Matrix4x4 previousMatrix;
        // Touch devices (QoL, 2026-10-13): the debug UI must be tappable. On mobile the scale is also raised toward the
        // physical density (dpi/160, i.e. Android density-independent pixels), capped at 2 so the panel still fits.
        // The 1080-height rule is unchanged for desktop and for explicit-height callers (tests).
        public static float ScaleForHeight(float height)
        {
            float scale=Mathf.Clamp(height/1080f,1f,2f);
            if(Application.isMobilePlatform && Screen.dpi>0f && Mathf.Abs(height-Screen.height)<1f)
                scale=Mathf.Max(scale,Mathf.Clamp(Screen.dpi/160f,1f,2f));
            return scale;
        }
        // The small "debug" restore chip shown while the panel is hidden. Minimum touch target ~48 dp on mobile,
        // proportional to the panel scale everywhere else.
        public static Rect RestoreChipRect()
        {
            float scale=ScaleForHeight(Screen.height);
            float h=Mathf.Max(18f*scale,24f);
            float w=60f*scale;
            if(Application.isMobilePlatform)
            {
                float dpi=Screen.dpi>0f?Screen.dpi:160f;
                h=Mathf.Max(h,48f*dpi/160f);
                w=Mathf.Max(w,h*2.6f);
            }
            return new Rect(10,10,w,h);
        }
        public static bool RestoreChipButton()
        {
            var rect=RestoreChipRect();
            var style=new GUIStyle(GUI.skin.button){fontSize=Mathf.Max(12,Mathf.RoundToInt(rect.height*0.42f)),alignment=TextAnchor.MiddleCenter};
            return GUI.Button(rect,"debug",style);
        }
        // "HIDE (F1)" button, drawn in fixed screen pixels OUTSIDE the scroll view (top-left, same size as the restore chip) so it is always
        // reachable without scrolling on wide resolutions where the panel has scroll bars.
        public static bool HideChipButton(Rect rect)
        {
            var style=new GUIStyle(GUI.skin.button){fontSize=Mathf.Max(12,Mathf.RoundToInt(rect.height*0.42f)),alignment=TextAnchor.MiddleCenter};
            return GUI.Button(rect,"HIDE (F1)",style);
        }
        public static Rect Viewport(float width,float height,float contentHeight)
        {
            float scale=ScaleForHeight(height);
            return new Rect(10,10,Mathf.Min(800*scale,Mathf.Max(180,width*.55f)),Mathf.Min(contentHeight*scale,height*.55f));
        }
        public static Rect CurrentViewport(float contentHeight)=>Viewport(Screen.width,Screen.height,contentHeight);
        public static void Begin(ref Vector2 scroll,float width,float height,float visibleHeight,float offsetX=0,float top=10)
        {
            var viewport=CurrentViewport(visibleHeight);
            viewport.y=top;
            float scale=ScaleForHeight(Screen.height);
            previousMatrix=GUI.matrix;
            GUI.matrix=previousMatrix*Matrix4x4.Scale(new Vector3(scale,scale,1));
            var rect=new Rect(viewport.x/scale,viewport.y/scale,viewport.width/scale,viewport.height/scale);
            GUI.Box(rect,GUIContent.none);
            scroll=GUI.BeginScrollView(rect,scroll,new Rect(offsetX,0,width,height));
        }
        public static void End(){GUI.EndScrollView();GUI.matrix=previousMatrix;}
    }
}
