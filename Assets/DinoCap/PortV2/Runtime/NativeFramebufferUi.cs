using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace DinoCap.PortV2
{
    // N2.52: native sprites and UI share one completed backing,
    // then the display scales it. Input remains in the existing OnGUI path.
    // Draw commands execute after GUI submission, outside its graphics context,
    // and are presented in the same frame. Direct D3D12 drawing inside OnGUI fails.
    public static class NativeFramebufferUi
    {
        public static bool? DiagnosticEnabled;
        public static int LastDrawCount {get;private set;}
        public static int LastCollectedFrame {get;private set;}
        public static int LastPresentedFrame {get;private set;}
        public static int FrameMismatchCount {get;private set;}
        public static RenderTexture Completed=>completed;
        static RenderTexture completed;
        static Material drawMaterial;
        static Material presentMaterial;
        static Rect view;
        static bool active;
        static CommandBuffer commands;
        static Mesh quad;
        static MaterialPropertyBlock properties;
        static GameObject runner;
        static RenderTexture overlays;
        static readonly List<Rect> overlayRects=new List<Rect>();
        static bool pending;
        static readonly bool copyOnly=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-dc1-native-ui-copy-only")>=0;
        static readonly bool whiteOnly=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-dc1-native-ui-white")>=0;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        static readonly bool rasterMath=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-dc1-native-ui-interpolated")<0;
#else
        const bool rasterMath=true;
#endif

        public static void Begin(RenderTexture world,Rect screenView,bool enableByDefault=true)
        {
            active=false;
            if(!(DiagnosticEnabled??enableByDefault) || Event.current==null || Event.current.type!=EventType.Repaint || world==null)return;
            if(drawMaterial==null) {
                var shader=Resources.Load<Shader>("Shaders/DC1NativeUi");
                if(shader==null)throw new System.InvalidOperationException("Missing native UI compositor shader");
                drawMaterial=new Material(shader);
                presentMaterial=new Material(shader);
                if(rasterMath)drawMaterial.EnableKeyword("DC1_TEXEL_SPACE_NEAREST");
                if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-dc1-native-ui-solid")>=0)
                    drawMaterial.EnableKeyword("DC1_DIAGNOSTIC_SOLID");
            }
            if(completed==null || completed.width!=world.width || completed.height!=world.height) {
                if(completed!=null){completed.Release();Object.Destroy(completed);}
                var descriptor=world.descriptor;
                descriptor.depthBufferBits=0;descriptor.msaaSamples=1;descriptor.useMipMap=false;
                completed=new RenderTexture(descriptor){name="DC1_CompletedWorldAndUi"};
                completed.Create();
            }
            view=screenView;LastDrawCount=0;LastCollectedFrame=Time.frameCount;
            if(quad==null) {
                quad=new Mesh {name="DC1_NativeUiQuad"};
                quad.vertices=new[]{new Vector3(0,0,0),new Vector3(1,0,0),new Vector3(1,1,0),new Vector3(0,1,0)};
                quad.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};
                quad.triangles=new[]{0,1,2,0,2,3};
                properties=new MaterialPropertyBlock();
            }
            if(commands==null)commands=new CommandBuffer {name="DC1 native framebuffer UI diagnostic"};
            commands.Clear();
            overlayRects.Clear();
            if(overlays==null || overlays.width!=Screen.width || overlays.height!=Screen.height) {
                if(overlays!=null){overlays.Release();Object.Destroy(overlays);}
                overlays=new RenderTexture(Screen.width,Screen.height,0,RenderTextureFormat.ARGB32);
                overlays.Create();
            }
            commands.Blit(BuiltinRenderTextureType.CameraTarget,overlays);
            commands.Blit(world,completed);
            commands.SetRenderTarget(completed);
            commands.SetViewport(new Rect(0,0,completed.width,completed.height));
            if(runner==null) {
                runner=new GameObject("DC1_NativeUiFrameCompletion");
                runner.AddComponent<NativeFramebufferUiFlush>();
            }
            active=true;
        }

        public static void DrawTexture(Rect rect,Texture texture,ScaleMode mode=ScaleMode.StretchToFill,bool alphaBlend=true)
        {
            if(!active) {GUI.DrawTexture(rect,texture,mode,alphaBlend);return;}
            // Keep GUI-only adapter labels and editor overlays available to the snapshot.
            // The completed native backing replaces the game viewport later this frame.
            GUI.DrawTexture(rect,texture,mode,alphaBlend);
            if(texture==null || rect.width<=0 || rect.height<=0)return;
            Rect uv=new Rect(0,0,1,1);
            float imageAspect=texture.width/(float)texture.height;
            float rectAspect=rect.width/rect.height;
            if(mode==ScaleMode.ScaleToFit) {
                if(rectAspect>imageAspect) {
                    float w=rect.height*imageAspect;rect.x+=(rect.width-w)/2;rect.width=w;
                } else {
                    float h=rect.width/imageAspect;rect.y+=(rect.height-h)/2;rect.height=h;
                }
            } else if(mode==ScaleMode.ScaleAndCrop) {
                if(rectAspect>imageAspect){uv.height=imageAspect/rectAspect;uv.y=(1-uv.height)/2;}
                else {uv.width=rectAspect/imageAspect;uv.x=(1-uv.width)/2;}
            }
            QueueTexture(rect,texture,uv,alphaBlend);
        }

        public static void DrawTextureWithTexCoords(Rect rect,Texture texture,Rect uv,bool alphaBlend=true)
        {
            GUI.DrawTextureWithTexCoords(rect,texture,uv,alphaBlend);
            if(active && texture!=null)QueueTexture(rect,texture,uv,alphaBlend);
        }

        public static void DrawTextureClipped(Rect rect,Texture texture,Rect clip)
        {
            GUI.BeginGroup(clip);
            GUI.DrawTexture(new Rect(rect.x-clip.x,rect.y-clip.y,rect.width,rect.height),texture,ScaleMode.StretchToFill,true);
            GUI.EndGroup();
            if(!active || texture==null)return;
            var cropped=Rect.MinMaxRect(Mathf.Max(rect.xMin,clip.xMin),Mathf.Max(rect.yMin,clip.yMin),
                Mathf.Min(rect.xMax,clip.xMax),Mathf.Min(rect.yMax,clip.yMax));
            if(cropped.width<=0 || cropped.height<=0)return;
            var uv=new Rect((cropped.xMin-rect.xMin)/rect.width,(rect.yMax-cropped.yMax)/rect.height,
                cropped.width/rect.width,cropped.height/rect.height);
            QueueTexture(cropped,texture,uv,true);
        }

        static void QueueTexture(Rect rect,Texture texture,Rect uv,bool alphaBlend)
        {
            float sx=completed.width/view.width,sy=completed.height/view.height;
            Rect target=new Rect((rect.x-view.x)*sx,(rect.y-view.y)*sy,rect.width*sx,rect.height*sy);
            properties.Clear();
            properties.SetTexture("_MainTex",whiteOnly?Texture2D.whiteTexture:texture);
            properties.SetColor("_Tint",GUI.color);
            bool flip=SystemInfo.graphicsUVStartsAtTop;
            properties.SetVector("_RectClip",new Vector4(2*target.xMin/completed.width-1,
                flip?2*target.yMin/completed.height-1:1-2*target.yMax/completed.height,
                2*target.width/completed.width,2*target.height/completed.height));
            properties.SetVector("_UvRect",new Vector4(uv.x,flip?uv.yMax:uv.y,uv.width,flip?-uv.height:uv.height));
            if(rasterMath) {
                properties.SetFloat("_UseRasterNearest",1);
                properties.SetVector("_RasterRect",new Vector4(target.x,target.y,target.width,target.height));
                properties.SetVector("_TexelUvRect",new Vector4(uv.x*texture.width,uv.yMax*texture.height,uv.width*texture.width,-uv.height*texture.height));
                properties.SetVector("_TextureDimensions",new Vector4(texture.width,texture.height,completed.height,0));
            }
            if(!copyOnly) {
                commands.DrawMesh(quad,Matrix4x4.identity,drawMaterial,0,alphaBlend?0:1,properties);
            }
            LastDrawCount++;
        }

        public static void End()
        {
            if(!active)return;
            active=false;
            commands.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            {
                bool smooth=GameplayPresentationCamera.GlobalModeName=="Smooth";
                completed.filterMode=smooth?FilterMode.Bilinear:FilterMode.Point;
                if(smooth)presentMaterial.EnableKeyword("DC1_PRESENT_LINEAR");
                else presentMaterial.DisableKeyword("DC1_PRESENT_LINEAR");
                commands.SetViewport(new Rect(view.x,Screen.height-view.yMax,view.width,view.height));
                properties.Clear();properties.SetTexture("_MainTex",completed);
                properties.SetColor("_Tint",Color.white);
                properties.SetVector("_RectClip",new Vector4(-1,-1,2,2));
                properties.SetVector("_UvRect",new Vector4(0,0,1,1));
                commands.DrawMesh(quad,Matrix4x4.identity,presentMaterial,0,1,properties);
            }
            commands.SetViewport(new Rect(0,0,Screen.width,Screen.height));
            pending=true;
        }

        public static void PreserveOverlay(Rect rect)
        {
            if((active || pending) && Event.current.type==EventType.Repaint)overlayRects.Add(rect);
        }

        public static void FlushPending()
        {
            if(!pending || runner==null)return;
            pending=false;
            foreach(var rect in overlayRects) {
                properties.Clear();properties.SetTexture("_MainTex",overlays);properties.SetColor("_Tint",Color.white);
                properties.SetVector("_RectClip",new Vector4(2*rect.x/Screen.width-1,1-2*rect.yMax/Screen.height,
                    2*rect.width/Screen.width,2*rect.height/Screen.height));
                bool flip=SystemInfo.graphicsUVStartsAtTop;
                properties.SetVector("_UvRect",new Vector4(rect.x/Screen.width,
                    flip?rect.yMax/Screen.height:1-rect.yMax/Screen.height,
                    rect.width/Screen.width,(flip?-1:1)*rect.height/Screen.height));
                // Separate material prevents the UI presentation's pending keyword from changing.
                commands.DrawMesh(quad,Matrix4x4.identity,drawMaterial,0,1,properties);
            }
            var previous=RenderTexture.active;
            try {
                Graphics.ExecuteCommandBuffer(commands);
                LastPresentedFrame=Time.frameCount;
                if(LastPresentedFrame!=LastCollectedFrame)FrameMismatchCount++;
            }
            finally {
                RenderTexture.active=previous;
            }
        }

        public static void Release()
        {
            active=false;
            pending=false;
            if(completed!=null){completed.Release();Object.Destroy(completed);completed=null;}
            if(drawMaterial!=null){Object.Destroy(drawMaterial);drawMaterial=null;}
            if(presentMaterial!=null){Object.Destroy(presentMaterial);presentMaterial=null;}
            if(quad!=null){Object.Destroy(quad);quad=null;}
            if(commands!=null){commands.Release();commands=null;}
            overlayRects.Clear();
            if(overlays!=null){overlays.Release();Object.Destroy(overlays);overlays=null;}
            if(runner!=null){Object.Destroy(runner);runner=null;}
        }
    }

    public sealed class NativeFramebufferUiFlush : MonoBehaviour
    {
        void OnEnable(){StartCoroutine(CompleteFrames());}
        System.Collections.IEnumerator CompleteFrames()
        {
            while(true) {
                yield return new WaitForEndOfFrame();
                NativeFramebufferUi.FlushPending();
            }
        }
    }
}
