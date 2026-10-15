#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoCap.PortV2
{
    // Explicit development-player diagnostic only; inert without -dc1-capture.
    // Captures the real presented framebuffer including IMGUI. Never runs in shipping builds.
    public sealed partial class DC1ConvergenceCapture : MonoBehaviour
    {
        static bool captureStarted;
        [Serializable] sealed class ShopRasterGeometry {
            public Vector3 cameraPosition; public float orthographicSize; public int targetWidth,targetHeight;
            public string projection,gpuProjection; public List<ShopRasterTile> tiles=new List<ShopRasterTile>();
        }
        [Serializable] sealed class ShopRasterTile {
            public string name; public Vector3 position,scale; public Rect spriteRect;
            public string shader; public float materialBottom,materialHeight,strictBottom;
            public Vector2[] vertices,uv; public ushort[] triangles; public Vector3[] worldCorners,screenCorners,gpuNdc;
        }
        void WriteShopRasterGeometry(RenderTexture target) {
            var camera=Camera.main;
            var gpu=GL.GetGPUProjectionMatrix(camera.projectionMatrix,true);
            var geometry=new ShopRasterGeometry {cameraPosition=camera.transform.position,
                orthographicSize=camera.orthographicSize,targetWidth=target.width,targetHeight=target.height,
                projection=camera.projectionMatrix.ToString("R"),gpuProjection=gpu.ToString("R")};
            foreach(var renderer in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)) {
                if(!renderer.name.StartsWith("Shop_tableFrame") || renderer.sprite==null)continue;
                var sprite=renderer.sprite;var vertices=sprite.vertices;
                var tile=new ShopRasterTile {name=renderer.name,position=renderer.transform.position,
                    scale=renderer.transform.lossyScale,spriteRect=sprite.rect,vertices=vertices,uv=sprite.uv,
                    triangles=sprite.triangles,worldCorners=new Vector3[vertices.Length],
                    screenCorners=new Vector3[vertices.Length],gpuNdc=new Vector3[vertices.Length]};
                tile.shader=renderer.sharedMaterial.shader.name;
                if(renderer.sharedMaterial.HasProperty("_StrictBottom")) {
                    tile.materialBottom=renderer.sharedMaterial.GetFloat("_NativeBottomY");
                    tile.materialHeight=renderer.sharedMaterial.GetFloat("_TargetHeight");
                    tile.strictBottom=renderer.sharedMaterial.GetFloat("_StrictBottom");
                }
                for(int i=0;i<vertices.Length;i++) {
                    var world=renderer.transform.TransformPoint(vertices[i]);tile.worldCorners[i]=world;
                    tile.screenCorners[i]=camera.WorldToScreenPoint(world);
                    var clip=gpu*camera.worldToCameraMatrix*new Vector4(world.x,world.y,world.z,1);
                    tile.gpuNdc[i]=new Vector3(clip.x/clip.w,clip.y/clip.w,clip.z/clip.w);
                }
                geometry.tiles.Add(tile);
            }
            File.WriteAllText(Path.Combine(output,"shop_raster_geometry.json"),JsonUtility.ToJson(geometry,true));
        }
        [Serializable] sealed class MenuAtlasEntry {public string name,path;public float x,yTop,w,h;}
        [Serializable] sealed class MenuAtlasMap {public MenuAtlasEntry[] entries;}
        static MenuAtlasMap menuAtlasMap;
        static bool useMenuAtlas;
        static readonly Dictionary<string,Texture2D> menuAtlases=new Dictionary<string,Texture2D>();
        internal static bool TryMenuAtlas(string name,out Texture2D texture,out Rect uv)
        {
            texture=null;uv=default;
            if(!useMenuAtlas || menuAtlasMap==null || NativeFramebufferUi.DiagnosticEnabled!=true)return false;
            foreach(var entry in menuAtlasMap.entries)if(entry.name==name) {
                if(!menuAtlases.TryGetValue(entry.path,out texture)) {
                    texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
                    if(!texture.LoadImage(File.ReadAllBytes(entry.path)))throw new InvalidOperationException("Source atlas PNG could not load");
                    texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
                    menuAtlases[entry.path]=texture;
                }
                uv=new Rect(entry.x/texture.width,(texture.height-entry.yTop-entry.h)/texture.height,entry.w/texture.width,entry.h/texture.height);
                return true;
            }
            return false;
        }
        string output;
        GameplayRenderTarget.PresentationAspectMode savedAspect;
        readonly Dictionary<string,int?> savedCampaign=new Dictionary<string,int?>();
        float? savedHealth;
        bool captureHouse;
        int captureFHouseTier=-1;
        bool captureKillCash;
        bool captureSpecies;
        bool captureMenuClock;
        bool captureMenuSustain;
        string audioRestartPhase;
        bool captureMenuCausal;
        bool captureMenuOptions;
        bool captureRenderChain;
        bool captureNativeUi;
        bool captureNativeMenuUi;
        bool captureMenuLaunch;
        bool captureFinishAi;
        bool captureExactDisplay;
        bool captureMenuReturn;
        bool captureAtlasReadback;
        bool captureShopBorder;
        bool captureN253;
        bool captureJungle;
        bool captureN255;
        NativeModeSession.Snapshot savedModeSession;
        bool savedInternal2x;
        int captureDeadPoseFrame=17;
        float deadline;
        [Serializable] sealed class CampaignValue {public string key;public bool present;public int value;}
        [Serializable] sealed class CampaignSnapshot {
            public List<CampaignValue> values=new List<CampaignValue>();public bool healthPresent;public float health;
        }
        [Serializable] sealed class CaptureIdentity {
            public string managedPath,sha256,unityVersion,graphicsApi,utc;
            public int internalScale;
        }
        void Update()
        {
            if(deadline<=0 || Time.realtimeSinceStartup<deadline)return;
            deadline=0;
            File.WriteAllText(Path.Combine(output,"capture_timeout.json"),"{\"status\":\"FAILED\",\"reason\":\"180s capture watchdog\"}");
            RestoreCampaign();GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
            Application.Quit(2);
        }
        [Serializable] sealed class MenuSample
        {
            public float seconds,x;
            public bool actorActive,trackActive,roarPlaying;
            public string clip;
            public int frame,advances,actorInstance;
        }
        [Serializable] sealed class MenuSamples
        {
            public bool diagnosticSpeciesSeeds=true;
            public List<MenuSample> samples=new List<MenuSample>();
        }
        string houseFailure;
        [Serializable] sealed class HouseSnapshot
        {
            public string stage, occupancy, playerClip;
            public int playerFrame, doorFrame;
            public bool hidden, suspended, doorPlaying;
            public float playerX;
            public string weapon;
            public List<string> enabledSpriteNames=new List<string>();
        }
        [Serializable] sealed class HouseSnapshots
        {
            public List<HouseSnapshot> stages=new List<HouseSnapshot>();
        }
        [Serializable] sealed class CashSnapshot
        {
            public int before,after,afterCommitReload;
        }
        readonly HouseSnapshots houseSnapshots=new HouseSnapshots();

        void PreserveCampaign()
        {
            foreach(string key in new[]{"DC1_CityGrind_Level","DC1_CityGrind_MaxLevel","DC1_CityGrind_Lives",
                "DC1_CityGrind_Cash","DC1_CityGrind_Difficulty","DC1_CityGrind_OwnershipModelMigratedV1","DC1_CityGrind_Tier_G",CityGrindStartPreset.EquippedPrefKey,"DC1_InternalSupersample2x",NativeAudioSessionGate.MusicMutedPrefKey,"DC1_MainMenu_MusicOn",NativeAudioSessionGate.FxMutedPrefKey,"DC1_MainMenu_SfxOn","DC1_Debug_Jungle_FutureSpeciesMask"})
                savedCampaign[key]=DC1Preferences.HasKey(key)?(int?)DC1Preferences.GetInt(key):null;
            for(int i=0;i<6;i++)foreach(string prefix in new[]{"DC1_CityGrind_Tier_",CityGrindStartPreset.AmmoPrefPrefix,CityGrindStartPreset.BonusPrefPrefix}) {
                string key=prefix+i;savedCampaign[key]=DC1Preferences.HasKey(key)?(int?)DC1Preferences.GetInt(key):null;
            }
            savedHealth=DC1Preferences.HasKey(CityGrindStartPreset.HealthPrefKey)
                ?(float?)DC1Preferences.GetFloat(CityGrindStartPreset.HealthPrefKey):null;
        }
        void RestoreCampaign()
        {
            NativeModeSession.Restore(savedModeSession);
            if((captureN253 || captureN255) && GameplayRenderTarget.Supersample2xEnabled!=savedInternal2x)GameplayRenderTarget.TogglePersistentSupersample();
            foreach(var pair in savedCampaign)
                if(pair.Value.HasValue)DC1Preferences.SetInt(pair.Key,pair.Value.Value);
                else DC1Preferences.DeleteKey(pair.Key);
            if(savedHealth.HasValue)DC1Preferences.SetFloat(CityGrindStartPreset.HealthPrefKey,savedHealth.Value);
            else DC1Preferences.DeleteKey(CityGrindStartPreset.HealthPrefKey);
            DC1Preferences.Save();
            if(captureJungle || captureN255) {
                var restored=new CampaignSnapshot {
                    healthPresent=DC1Preferences.HasKey(CityGrindStartPreset.HealthPrefKey),
                    health=DC1Preferences.GetFloat(CityGrindStartPreset.HealthPrefKey,0)};
                foreach(var pair in savedCampaign)restored.values.Add(new CampaignValue {
                    key=pair.Key,present=DC1Preferences.HasKey(pair.Key),value=DC1Preferences.GetInt(pair.Key,0)});
                File.WriteAllText(Path.Combine(output,"campaign_restored.json"),JsonUtility.ToJson(restored,true));
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartIfRequested()
        {
            // One persistent diagnostic runner owns the fixture and its
            // saved preferences, even if initialization is invoked again.
            if(captureStarted)return;
            string[] args=Environment.GetCommandLineArgs();
            int index=Array.IndexOf(args,"-dc1-capture");
            if(index<0 || index+1>=args.Length) return;
            captureStarted=true;
            var go=new GameObject("DC1 Development Capture");
            DontDestroyOnLoad(go);
            var capture=go.AddComponent<DC1ConvergenceCapture>();
            capture.output=Path.GetFullPath(args[index+1]);
            capture.captureHouse=Array.IndexOf(args,"-dc1-house")>=0;
            for(int tier=0;tier<3;tier++)
                if(Array.IndexOf(args,"-dc1-f"+(tier+1)+"-house")>=0)capture.captureFHouseTier=tier;
            capture.captureHouse|=capture.captureFHouseTier>=0;
            capture.captureKillCash=Array.IndexOf(args,"-dc1-kill-cash")>=0;
            capture.captureSpecies=Array.IndexOf(args,"-dc1-species")>=0;
            capture.captureMenuClock=Array.IndexOf(args,"-dc1-native-menu-clock")>=0;
            capture.captureMenuSustain=Array.IndexOf(args,"-dc1-native-menu-sustain")>=0;
            int audioIndex=Array.IndexOf(args,"-dc1-audio-restart");
            if(audioIndex>=0 && audioIndex+1<args.Length)capture.audioRestartPhase=args[audioIndex+1];
            capture.captureMenuCausal=Array.IndexOf(args,"-dc1-menu-causal")>=0;
            capture.captureMenuOptions=Array.IndexOf(args,"-dc1-menu-options")>=0;
            capture.captureRenderChain=Array.IndexOf(args,"-dc1-render-chain")>=0;
            capture.captureNativeUi=Array.IndexOf(args,"-dc1-native-ui")>=0;
            capture.captureNativeMenuUi=Array.IndexOf(args,"-dc1-native-menu-ui")>=0;
            int atlasIndex=Array.IndexOf(args,"-dc1-menu-atlas-map");
            if(atlasIndex>=0 && atlasIndex+1<args.Length)
                menuAtlasMap=JsonUtility.FromJson<MenuAtlasMap>(File.ReadAllText(args[atlasIndex+1]));
            capture.captureMenuLaunch=Array.IndexOf(args,"-dc1-menu-launch")>=0;
            capture.captureFinishAi=Array.IndexOf(args,"-dc1-finish-ai")>=0;
            capture.captureExactDisplay=Array.IndexOf(args,"-dc1-exact-display")>=0;
            int deadPoseIndex=Array.IndexOf(args,"-dc1-dead-pose-frame");
            if(deadPoseIndex>=0 && deadPoseIndex+1<args.Length)
                capture.captureDeadPoseFrame=int.Parse(args[deadPoseIndex+1]);
            capture.captureMenuReturn=Array.IndexOf(args,"-dc1-menu-return")>=0;
            capture.captureAtlasReadback=Array.IndexOf(args,"-dc1-atlas-readback")>=0;
            capture.captureShopBorder=Array.IndexOf(args,"-dc1-shop-border")>=0;
            capture.captureN253=Array.IndexOf(args,"-dc1-n253")>=0;
            capture.captureJungle=Array.IndexOf(args,"-dc1-n254-jungle")>=0;
            capture.captureN255=Array.IndexOf(args,"-dc1-n255")>=0;
            int virtualViewportIndex=Array.IndexOf(args,"-dc1-virtual-view");
            if(virtualViewportIndex>=0 && virtualViewportIndex+2<args.Length)
                GameplayRenderTarget.DiagnosticViewportSize=new Vector2Int(
                    int.Parse(args[virtualViewportIndex+1]),int.Parse(args[virtualViewportIndex+2]));
            capture.savedModeSession=NativeModeSession.Capture();
            capture.savedAspect=GameplayRenderTarget.AspectMode;
            capture.savedInternal2x=GameplayRenderTarget.Supersample2xEnabled;
            capture.PreserveCampaign();
            if(capture.captureN253 || capture.captureN255) {
                if(GameplayRenderTarget.Supersample2xEnabled && Array.IndexOf(args,"-dc1-boot-death-shop")<0 && Array.IndexOf(args,"-dc1-k4-parity")<0)GameplayRenderTarget.TogglePersistentSupersample();
                NativeAudioSessionGate.SaveMusicPreference(false);
                NativeAudioSessionGate.SaveFxPreference(false);
            }
            GameplayRenderTarget.SetPersistentAspectMode(Array.IndexOf(args,"-dc1-wide")>=0
                ? GameplayRenderTarget.PresentationAspectMode.WidescreenExpand
                : GameplayRenderTarget.PresentationAspectMode.SourceFaithful4x3);
            Application.runInBackground=true;
            Application.targetFrameRate=60;
            Directory.CreateDirectory(capture.output);
            string managedPath=typeof(DC1ConvergenceCapture).Assembly.Location;
            string managedHash=null;
            if(File.Exists(managedPath))using(var hash=System.Security.Cryptography.SHA256.Create())
                managedHash=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(managedPath))).Replace("-","").ToLowerInvariant();
            File.WriteAllText(Path.Combine(capture.output,"capture_identity.json"),JsonUtility.ToJson(new CaptureIdentity {
                managedPath=managedPath,sha256=managedHash,unityVersion=Application.unityVersion,
                graphicsApi=SystemInfo.graphicsDeviceType.ToString(),internalScale=GameplayRenderTarget.InternalScale,
                utc=DateTime.UtcNow.ToString("o")},true));
            var snapshot=new CampaignSnapshot {healthPresent=capture.savedHealth.HasValue,health=capture.savedHealth??0};
            foreach(var pair in capture.savedCampaign)snapshot.values.Add(new CampaignValue {key=pair.Key,present=pair.Value.HasValue,value=pair.Value??0});
            File.WriteAllText(Path.Combine(capture.output,"campaign_before.json"),JsonUtility.ToJson(snapshot,true));
            capture.deadline=Time.realtimeSinceStartup+180f;
            capture.StartCoroutine(capture.Run());
            Debug.Log("[DC1-CAPTURE] runner="+capture.GetInstanceID()+" menuLaunch="+capture.captureMenuLaunch);
        }

        static void Set(CityGridController city,string field,object value)
        {
            typeof(CityGridController).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic)
                .SetValue(city,value);
        }

        IEnumerator Save(string name)
        {
            string path=Path.Combine(output,name+".png");
            ScreenCapture.CaptureScreenshot(path);
            float end=Time.realtimeSinceStartup+10f;
            while((!File.Exists(path) || new FileInfo(path).Length==0) && Time.realtimeSinceStartup<end)
                yield return null;
            if(!File.Exists(path) || new FileInfo(path).Length==0)
                throw new InvalidOperationException("Screenshot did not complete: "+path);
            var size=GameplayRenderTarget.DiagnosticViewportSize;
            if(size.x>0 && size.y>0 && FindFirstObjectByType<CityGridController>()!=null && NativeFramebufferUi.Completed!=null)
            {
                yield return new WaitForEndOfFrame();
                var source=NativeFramebufferUi.Completed;
                var scaled=RenderTexture.GetTemporary(size.x,size.y,0,RenderTextureFormat.ARGB32);
                var previous=RenderTexture.active;
                var readback=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);
                try
                {
                    Graphics.Blit(source,scaled);
                    RenderTexture.active=scaled;
                    readback.ReadPixels(new Rect(0,0,size.x,size.y),0,0);readback.Apply();
                    File.WriteAllBytes(Path.Combine(output,name+"_virtual_view.png"),readback.EncodeToPNG());
                    File.WriteAllText(Path.Combine(output,name+"_virtual_view.json"),
                        "{\"scope\":\"Virtual production world/UI framebuffer; host-monitor and physical spanning not verified\",\"width\":"+size.x+",\"height\":"+size.y+",\"native_framebuffer_width\":"+source.width+",\"native_framebuffer_height\":"+source.height+",\"physical_screen_width\":"+Screen.width+",\"physical_screen_height\":"+Screen.height+"}");
                }
                finally {RenderTexture.active=previous;RenderTexture.ReleaseTemporary(scaled);Destroy(readback);}
            }
        }

        [Serializable] sealed class AtlasReadbackInfo {
            public string resource,api,textureFormat,graphicsFormat,filter,wrap,status;
            public int width,height,mipCount,bytes;
            public bool readable;
        }
        IEnumerator CaptureAtlasReadback()
        {
            foreach(string name in new[]{"ipad_dino1","ipad_dino2"}) {
                string resource="DinoCapGenerated/Textures/"+name;
                var texture=Resources.Load<Texture2D>(resource);
                if(texture==null)throw new InvalidOperationException("Missing atlas: "+resource);
                var info=new AtlasReadbackInfo {resource=resource,api=SystemInfo.graphicsDeviceType.ToString(),
                    textureFormat=texture.format.ToString(),graphicsFormat=texture.graphicsFormat.ToString(),
                    filter=texture.filterMode.ToString(),wrap=texture.wrapMode.ToString(),width=texture.width,
                    height=texture.height,mipCount=texture.mipmapCount,readable=texture.isReadable};
                var request=UnityEngine.Rendering.AsyncGPUReadback.Request(texture,0,TextureFormat.RGBA32);
                while(!request.done)yield return null;
                if(request.hasError) {
                    info.status="FAILED";
                    File.WriteAllText(Path.Combine(output,name+"_gpu.json"),JsonUtility.ToJson(info,true));
                    throw new InvalidOperationException("GPU atlas readback failed: "+resource);
                }
                byte[] bytes=request.GetData<byte>().ToArray();
                info.bytes=bytes.Length;info.status="VERIFIED_READBACK";
                File.WriteAllBytes(Path.Combine(output,name+"_gpu.rgba"),bytes);
                File.WriteAllText(Path.Combine(output,name+"_gpu.json"),JsonUtility.ToJson(info,true));
            }
        }

        [Serializable] sealed class AudioRestartSample
        {
            public string phase,clip;
            public bool musicOn,playing,muted,cityMuted,cityPlaying;
            public int swapRequests;
        }
        void RestoreSmokeMusic()
        {
            var saved=JsonUtility.FromJson<CampaignSnapshot>(File.ReadAllText(Path.Combine(output,"music_original.json")));
            foreach(var v in saved.values)
                if(v.present)DC1Preferences.SetInt(v.key,v.value);else DC1Preferences.DeleteKey(v.key);
            DC1Preferences.Save();
        }
        IEnumerator CaptureAudioRestart()
        {
            if(audioRestartPhase=="restore") {
                RestoreSmokeMusic();yield break;
            }
            if(audioRestartPhase=="write_off") {
                string path=Path.Combine(output,"music_original.json");
                if(File.Exists(path))throw new InvalidOperationException("Existing music snapshot: restore it before starting another smoke test.");
                var saved=new CampaignSnapshot();
                foreach(string key in new[]{NativeAudioSessionGate.MusicMutedPrefKey,"DC1_MainMenu_MusicOn"})
                    saved.values.Add(new CampaignValue {key=key,present=DC1Preferences.HasKey(key),value=DC1Preferences.GetInt(key)});
                File.WriteAllText(path,JsonUtility.ToJson(saved,true));
            }
            yield return SceneManager.LoadSceneAsync("MainMenuScene");
            for(int i=0;i<5;i++)yield return null;
            var menu=FindFirstObjectByType<MainMenuController>();
            var bgm=menu.TestHarness_MenuMusicSource;
            if(bgm==null || bgm.clip==null || bgm.clip.name!="Music_menu" || !bgm.isPlaying)
                throw new InvalidOperationException("Native menu BGM must be loaded and playing.");
            var sample=new AudioRestartSample {phase=audioRestartPhase,clip=bgm.clip.name};
            if(audioRestartPhase=="write_off") {
                if(menu.TestHarness_MusicOn)menu.TestHarness_PressMusicToggle();
                if(menu.TestHarness_MusicOn || !bgm.mute)throw new InvalidOperationException("OFF toggle failed.");
            } else if(audioRestartPhase=="read_off_write_on") {
                if(menu.TestHarness_MusicOn || !bgm.mute)throw new InvalidOperationException("OFF did not survive process restart.");
            } else if(audioRestartPhase=="read_on_restore") {
                if(!menu.TestHarness_MusicOn || bgm.mute)throw new InvalidOperationException("ON did not survive process restart.");
            } else throw new InvalidOperationException("Unknown audio restart phase.");
            sample.musicOn=menu.TestHarness_MusicOn;sample.playing=bgm.isPlaying;sample.muted=bgm.mute;
            if(audioRestartPhase=="read_off_write_on") {
                yield return SceneManager.LoadSceneAsync("CityGrindScene");
                for(int i=0;i<5;i++)yield return null;
                var city=FindFirstObjectByType<CityGridController>();
                sample.cityMuted=city.TestHarness_MusicMuted;sample.cityPlaying=city.TestHarness_BgmPlaying;
                if(!sample.cityMuted || !sample.cityPlaying)throw new InvalidOperationException("OFF not carried into City music source.");
                city.TestHarness_SkipLevelIntro();
                int before=NativeAudioSessionGate.TotalRequests;
                NativeAudioSessionGate.TestHarness_ResetClipCounters();
                city.TestHarness_SwitchWeapon();sample.swapRequests=NativeAudioSessionGate.TotalRequests-before;
                if(sample.swapRequests!=1)throw new InvalidOperationException("Swap did not request native Button1 exactly once.");
                foreach(string clip in new[]{"Attack_A","Attack_B","Attack_C","Attack_D"})
                    if(NativeAudioSessionGate.TestHarness_AdmittedCount(clip)!=0)throw new InvalidOperationException("Swap requested a shot voice: "+clip);
                city.TestHarness_SetMusicMuted(false);
            }
            File.WriteAllText(Path.Combine(output,audioRestartPhase+".json"),JsonUtility.ToJson(sample,true));
            if(audioRestartPhase=="read_on_restore") {
                RestoreSmokeMusic();
                File.WriteAllText(Path.Combine(output,"music_restored.json"),File.ReadAllText(Path.Combine(output,"music_original.json")));
            }
        }

        IEnumerator Run()
        {
            if(captureN255) {
                yield return CaptureN255();
                RestoreCampaign();GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                Application.Quit(0);yield break;
            }
            if(captureN253) {
                if(captureJungle)yield return CaptureN254Jungle();
                else yield return CaptureN253();
                RestoreCampaign();GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                Application.Quit(0);yield break;
            }
            if(audioRestartPhase!=null) {
                yield return CaptureAudioRestart();
                RestoreCampaign();GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                Application.Quit(0);yield break;
            }
            if(captureShopBorder) {
                CityGrindStartPreset.ForLevel(1).WriteCommittedRun(1);
                yield return SceneManager.LoadSceneAsync("CityGrindScene");yield return null;yield return null;
                var shopCity=FindFirstObjectByType<CityGridController>();
                shopCity.TestHarness_SkipLevelIntro();shopCity.TestHarness_ClearDinos();shopCity.TestHarness_DebugUiHidden=true;
                shopCity.TestHarness_TriggerLevelClear();shopCity.TestHarness_ForceShopFadeComplete();
                yield return null;yield return null;Time.timeScale=0f;
                bool strictShopBottom=Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-shop-strict-bottom")>=0;
                bool shopShaderControl=Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-shop-boundary-control")>=0;
                if(strictShopBottom || shopShaderControl) {
                    var shader=Resources.Load<Shader>("Shaders/DC1ShopBoundaryDiagnostic");
                    if(shader==null || !shader.isSupported)throw new InvalidOperationException("Shop boundary diagnostic shader unavailable");
                    var target=FindFirstObjectByType<GameplayRenderTarget>().Target;
                    foreach(var renderer in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)) {
                        if(!renderer.name.StartsWith("Shop_tableFrame"))continue;
                        var material=new Material(shader);
                        float bottom=float.PositiveInfinity;
                        foreach(var vertex in renderer.sprite.vertices)
                            bottom=Mathf.Min(bottom,Camera.main.WorldToScreenPoint(renderer.transform.TransformPoint(vertex)).y);
                        material.SetFloat("_NativeBottomY",bottom);
                        material.SetFloat("_TargetHeight",target.height);
                        material.SetFloat("_StrictBottom",Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-shop-boundary-values")>=0?2:strictShopBottom?1:0);
                        renderer.sharedMaterial=material;
                    }
                    yield return null;yield return null;
                }
                yield return Save("shop_rest");
                var frame=FindFirstObjectByType<GameplayRenderTarget>().Target;
                var previous=RenderTexture.active;
                try {
                    RenderTexture.active=frame;
                    var pixels=new Texture2D(frame.width,frame.height,TextureFormat.RGBA32,false);
                    pixels.ReadPixels(new Rect(0,0,frame.width,frame.height),0,0);pixels.Apply();
                    File.WriteAllBytes(Path.Combine(output,"shop_world.png"),pixels.EncodeToPNG());Destroy(pixels);
                } finally {RenderTexture.active=previous;}
                File.WriteAllText(Path.Combine(output,"shop_world_metadata.json"),"{\"width\":"+frame.width+",\"height\":"+frame.height+",\"api\":\""+SystemInfo.graphicsDeviceType+"\"}");
                WriteShopRasterGeometry(frame);
                int sourceAtlas=Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-shop-atlas");
                if(sourceAtlas>=0) {
                    var atlas=new Texture2D(2,2,TextureFormat.RGBA32,false);
                    if(!atlas.LoadImage(File.ReadAllBytes(Environment.GetCommandLineArgs()[sourceAtlas+1])))throw new InvalidOperationException("Cannot load original shop atlas");
                    atlas.filterMode=FilterMode.Point;atlas.wrapMode=TextureWrapMode.Clamp;
                    var sprite=Sprite.Create(atlas,new Rect(359,atlas.height-766-83,133,83),new Vector2(0.5f,0.5f),1,0,SpriteMeshType.FullRect);
                    int replaced=0;
                    foreach(var renderer in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                        if(renderer.gameObject.name=="Shop_tableFrame"){renderer.sprite=sprite;replaced++;}
                    yield return null;yield return null;yield return Save("shop_original_atlas_rest");
                    previous=RenderTexture.active;
                    try {
                        RenderTexture.active=frame;
                        var pixels=new Texture2D(frame.width,frame.height,TextureFormat.RGBA32,false);
                        pixels.ReadPixels(new Rect(0,0,frame.width,frame.height),0,0);pixels.Apply();
                        File.WriteAllBytes(Path.Combine(output,"shop_original_atlas_world.png"),pixels.EncodeToPNG());Destroy(pixels);
                    } finally {RenderTexture.active=previous;}
                    File.WriteAllText(Path.Combine(output,"shop_original_atlas_metadata.json"),"{\"registered_rect\":[359,766,133,83],\"replaced_tiles\":"+replaced+"}");
                }
                RestoreCampaign();GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                Application.Quit(0);yield break;
            }
            if(captureAtlasReadback) {
                yield return CaptureAtlasReadback();
                RestoreCampaign();GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                Application.Quit(0);yield break;
            }
            if(captureFinishAi) {
                yield return CaptureFinishAi();
                RestoreCampaign();GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                Application.Quit(0);yield break;
            }
            if(captureMenuReturn) {
                yield return CaptureMenuReturns();
                RestoreCampaign();GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                Application.Quit(0);yield break;
            }
            if(captureMenuLaunch) {
                yield return CaptureMenuLaunch();
                RestoreCampaign();GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                Application.Quit(0);yield break;
            }
            if(captureNativeMenuUi) {
                yield return CaptureNativeMenuUi();
                RestoreCampaign();Application.Quit(0);yield break;
            }
            if(captureNativeUi) {
                yield return CaptureNativeUi();
                RestoreCampaign();
                Application.Quit(0);
                yield break;
            }
            if(captureRenderChain)
            {
                yield return CaptureRenderChain();
                RestoreCampaign();
                Application.Quit(0);
                yield break;
            }
            // Match the video's existing-run menu state, with RESUME available.
            // The user's original campaign was captured before this temporary preset.
            CityGrindStartPreset.ForLevel(1).WriteCommittedRun(1);
            yield return SceneManager.LoadSceneAsync("MainMenuScene");
            for(int i=0;i<5;i++)yield return null;
            var menu=FindFirstObjectByType<MainMenuController>();
            menu.TestHarness_SkipBootSplash();
            if(captureMenuOptions) {
                Time.timeScale=0f;
                menu.TestHarness_ResetReveal();
                for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
                yield return Save("menu_primary_rest");
                menu.TestHarness_SelectOptions();
                menu.TestHarness_StepReveal(0.05f);
                yield return Save("options_selection_feedback");
                for(int i=0;i<30;i++)menu.TestHarness_StepReveal(0.01f);
                yield return Save("options_primary_leave");
                for(int i=0;i<100;i++)menu.TestHarness_StepReveal(0.01f);
                yield return Save("options_rows_revealed");
                Time.timeScale=1f;
                GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                RestoreCampaign();
                Application.Quit(0);
                yield break;
            }
            if(captureMenuCausal) {
                Time.timeScale=0f;
                menu.TestHarness_ResetReveal();
                for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
                menu.TestHarness_StepDinoCreation(5f);
                yield return Save("menu_reference_clip_left");
                menu.TestHarness_StepBackgroundDino(68f);
                yield return Save("menu_reference_clip_right");
                menu.TestHarness_ShowHelp();
                yield return Save("menu_authentic_help");
                menu.TestHarness_CloseHelp();
                menu.TestHarness_SelectCity();
                menu.TestHarness_StepReveal(0.05f);
                yield return Save("menu_city_selection_phase1");
                menu.TestHarness_StepReveal(0.21f);
                yield return Save("menu_city_selection_completed");
                Time.timeScale=1f;
                GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                RestoreCampaign();
                Application.Quit(0);
                yield break;
            }
            if(captureMenuSustain) {
                yield return CaptureMenuSustain(menu);
                GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                RestoreCampaign();
                Application.Quit(0);
                yield break;
            }
            if(captureMenuClock) {
                float elapsedBefore=menu.TestHarness_DinoCreationElapsed;
                float start=Time.time;
                bool activeBefore=menu.TestHarness_DinoActive;
                yield return Save("menu_activation_before");
                float end=Time.realtimeSinceStartup+7f;
                while(!menu.TestHarness_DinoActive && Time.realtimeSinceStartup<end)yield return null;
                float activationAge=elapsedBefore+Time.time-start;
                var roarSource=menu.GetComponent<AudioSource>();
                bool roarPlaying=roarSource!=null && roarSource.isPlaying;
                File.WriteAllText(Path.Combine(output,"menu_activation_clock.json"),
                    "{\"active_before\":"+activeBefore.ToString().ToLowerInvariant()+
                    ",\"active_after\":"+menu.TestHarness_DinoActive.ToString().ToLowerInvariant()+
                    ",\"activation_age_seconds\":"+activationAge.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+
                    ",\"roar_requested\":"+menu.TestHarness_DinoRoarPlayed.ToString().ToLowerInvariant()+
                    ",\"roar_source_playing\":"+roarPlaying.ToString().ToLowerInvariant()+"}");
                yield return Save("menu_activation_after");
            }
            Time.timeScale=0f;
            menu.TestHarness_ResetReveal();
            menu.TestHarness_StepReveal(0f);
            yield return Save("menu_reveal_000");
            menu.TestHarness_StepReveal(0.125f);
            yield return Save("menu_reveal_125");
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            menu.TestHarness_StepDinoCreation(5f);
            menu.TestHarness_StepBackgroundDino(25f);
            yield return Save("menu_presented");
            if(captureSpecies)yield return CaptureSpecies(menu);
            menu.TestHarness_GoLevelSelect();
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            yield return Save("menu_level_select_presented");
            menu.TestHarness_BackFromLevelSelect();
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            yield return Save("menu_return_presented");
            menu.TestHarness_GoOptions();
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            yield return Save("menu_options_presented");
            Time.timeScale=1f;
            CityGrindStartPreset.ForLevel(1).WriteCommittedRun(1);
            yield return SceneManager.LoadSceneAsync("CityGrindScene");
            for(int i=0;i<10;i++) yield return null;
            var city=FindFirstObjectByType<CityGridController>();
            city.TestHarness_DebugUiHidden=true;
            city.TestHarness_ClearDinos();
            yield return new WaitForSeconds(0.45f);
            yield return Save("city_intro_presented");
            while(city.TestHarness_LevelIntroActive)yield return null;
            if(captureKillCash) {
                Time.timeScale=0f;
                int before=city.TestHarness_CashTotal;
                yield return Save("cash_before_kill");
                city.TestHarness_ForceSpawn(0);
                city.TestHarness_ApplyRangedHitToDino(0,1000f);
                int after=city.TestHarness_CashTotal;
                yield return Save("cash_after_kill");
                city.TestHarness_SaveProgressionState();
                city.TestHarness_ReloadProgressionFromPrefs();
                File.WriteAllText(Path.Combine(output,"kill_cash_observed.json"),JsonUtility.ToJson(new CashSnapshot {
                    before=before,after=after,afterCommitReload=city.TestHarness_CashTotal }));
                Time.timeScale=1f;
                GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                RestoreCampaign();
                Application.Quit(0);
                yield break;
            }
            if(captureFHouseTier>=0) {
                city.TestHarness_SetFMode(captureFHouseTier);
                city.TestHarness_SetRangedFamily(5);
                for(int i=0;i<3;i++)yield return null;
                yield return Save("f"+(captureFHouseTier+1)+"_before_house");
            }
            if(captureHouse) {
                yield return CaptureHouse(city);
                File.WriteAllText(Path.Combine(output,"house_snapshots.json"),JsonUtility.ToJson(houseSnapshots,true));
                if(houseFailure!=null) {
                    File.WriteAllText(Path.Combine(output,"house_capture_error.txt"),houseFailure);
                    Time.timeScale=1f;
                    GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                    RestoreCampaign();
                    Application.Quit(1);
                    yield break;
                }
                if(captureFHouseTier>=0) {
                    Time.timeScale=1f;
                    GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
                    RestoreCampaign();
                    Application.Quit(0);
                    yield break;
                }
            }
            Time.timeScale=0f; // Hold diagnostic snapshots; automated tests cover actual transition flow.
            yield return Save("city_presented");
            Set(city,"forcedWalkFading",true);
            Set(city,"forcedWalkFadeTimer",city.TestHarness_GetShopFadeSeconds());
            yield return Save("black_fade_full");
            Set(city,"forcedWalkFading",false);
            typeof(CityGridController).GetMethod("BeginForcedWalkExit",BindingFlags.Instance|BindingFlags.NonPublic)
                .Invoke(city,new object[]{1});
            var tick=typeof(CityGridController).GetMethod("TickLevelPresentation",BindingFlags.Instance|BindingFlags.NonPublic);
            tick.Invoke(city,new object[]{0.02f});tick.Invoke(city,new object[]{0.5f});
            Set(city,"forcedWalkFading",true);
            Set(city,"forcedWalkFadeTimer",city.TestHarness_GetShopFadeSeconds());
            yield return Save("clear_above_full_black");
            Set(city,"forcedWalkFading",false);Set(city,"forcedWalkActive",false);
            Set(city,"levelClearTween",null);
            city.TestHarness_TriggerLevelClear();
            Set(city,"shopFadeTimer",0f);
            yield return Save("shop_black_fade_full");
            city.TestHarness_ForceShopFadeComplete();
            for(int i=0;i<3;i++) yield return null;
            yield return Save("store_presented");
            File.WriteAllText(Path.Combine(output,"capture_result.json"),
                "{\"width\":"+Screen.width+",\"height\":"+Screen.height+
                ",\"aspect\":\""+GameplayRenderTarget.AspectMode+
                "\",\"store_control_suspended\":"+city.TestHarness_PlayerControlSuspended.ToString().ToLowerInvariant()+"}");
            Time.timeScale=1f;
            GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
            RestoreCampaign();
            Application.Quit(0);
        }

        // Explicit, staged development fixture. Screenshots come from the real
        // player presentation; synthetic pointer input uses the production owner.
        IEnumerator CaptureN253()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-n253-social")>=0) {
                yield return CaptureN253Social();yield break;
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-n253-sandbox")>=0) {
                yield return CaptureN253Sandbox();yield break;
            }
            CityGrindStartPreset.ForLevel(1).WriteCommittedRun(1);
            yield return SceneManager.LoadSceneAsync("CityGrindScene");
            yield return null;yield return null;
            var city=FindFirstObjectByType<CityGridController>();
            city.TestHarness_SkipLevelIntro();city.TestHarness_ClearDinos();
            city.TestHarness_DebugUiHidden=true;city.TestHarness_GodMode=true;
            city.TestHarness_SuppressRealInput=true;
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-n253-combat")>=0) {
                yield return CaptureN253Combat(city);yield break;
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-n253-death")>=0) {
                city.TestHarness_GodMode=false;city.TestHarness_SetLives(3);
                city.TestHarness_SetCash(321);city.TestHarness_SaveProgressionState();
                city.TestHarness_DamagePlayer(999f);
                yield return Save("death_body");
                while(!city.TestHarness_DeathUIActive)yield return null;
                yield return Save("death_text_pop");
                var field=typeof(CityGridController).GetField("deathPresentation",BindingFlags.Instance|BindingFlags.NonPublic);
                var death=(NativeDeathPresentation)field.GetValue(city);
                while(death.CurrentStage!=NativeDeathPresentation.Stage.BlackFade)yield return null;
                yield return new WaitForSeconds(.5f);
                yield return Save("death_black_fade");
                while(death.CurrentStage!=NativeDeathPresentation.Stage.TextRise)yield return null;
                yield return new WaitForSeconds(.25f);
                yield return Save("death_text_rise");
                while(death.CurrentStage!=NativeDeathPresentation.Stage.DieEndWait)yield return null;
                yield return Save("death_lives_rest");
                File.WriteAllText(Path.Combine(output,"death_capture.json"),
                    "{\"fixture\":\"staged lethal hit in real player\",\"lives\":"+city.TestHarness_Lives+
                    ",\"alpha_byte\":"+death.BlackAlphaByte+",\"width\":"+Screen.width+",\"height\":"+Screen.height+"}");
                // Follow actual scene retirement and intro rather than a guessed delay.
                while(city!=null)yield return null;
                do {city=FindFirstObjectByType<CityGridController>();yield return null;} while(city==null);
                city.TestHarness_DebugUiHidden=true;city.TestHarness_GodMode=true;
                city.TestHarness_SuppressRealInput=true;
                yield return Save("death_replay_intro");
                while(city.TestHarness_LevelIntroActive)yield return null;
                yield return Save("death_replay_control");
                city.TestHarness_TogglePause();yield return null;
                yield return Save("replay_pause");
                city.TestHarness_PauseMenu();
                while(FindFirstObjectByType<MainMenuController>()==null)yield return null;
                var menu=FindFirstObjectByType<MainMenuController>();
                yield return new WaitForSeconds(1.5f);
                yield return Save("replay_return_menu");
                menu.TestHarness_PressResume();yield return null;yield return null;
                city=FindFirstObjectByType<CityGridController>();
                city.TestHarness_SuppressRealInput=true;
                yield return Save("relaunch_intro");
                while(city.TestHarness_LevelIntroActive)yield return null;
                city.TestHarness_DebugUiHidden=true;city.TestHarness_ClearDinos();
                yield return Save("relaunch_control");
                File.WriteAllText(Path.Combine(output,"replay_capture.json"),
                    "{\"fixture\":\"real scene death replay, pause-menu, Resume relaunch\",\"lives\":"+city.TestHarness_Lives+
                    ",\"cash\":"+city.TestHarness_CashTotal+",\"intro_complete\":true}");
                city.TestHarness_GodMode=false;city.TestHarness_ClearDinos();
                city.TestHarness_SetLives(1);city.TestHarness_SaveProgressionState();
                city.TestHarness_DamagePlayer(999f);
                while(FindFirstObjectByType<MainMenuController>()==null)yield return null;
                yield return new WaitForSeconds(1.5f);
                yield return Save("game_over_menu_no_resume");
                File.WriteAllText(Path.Combine(output,"game_over_capture.json"),
                    "{\"fixture\":\"staged last life through real death/menu owners\",\"committed_lives\":"+
                    DC1Preferences.GetInt(CityGridController.LivesPrefKey,-1)+"}");
                yield break;
            }
            city.TestHarness_SetPlayerX(200f);
            city.TestHarness_ForceSpawn(0);city.TestHarness_SetDinoX(0,230f);
            city.TestHarness_SetDinoHP(0,1f);
            city.TestHarness_SetRangedFamily(4);city.TestHarness_SetRangedTier(4,0);
            city.TestHarness_DischargeE();
            yield return Save("E_gore_flight");
            yield return new WaitForSeconds(2.5f);
            yield return Save("E_gore_settled");
            yield return new WaitForSeconds(.5f);
            yield return Save("E_gore_persisted");
            int meat=FindObjectsByType<WeaponSuirouParticle>(FindObjectsSortMode.None).Length;
            int blood=FindObjectsByType<DinoCapDamageParticle>(FindObjectsSortMode.None).Length;
            city.TestHarness_TriggerLevelClear();city.TestHarness_ForceShopFadeComplete();
            city.TestHarness_SetCash(100000);city.TestHarness_SetRangedFamilyTierRaw(1,-1);
            yield return null;yield return null;
            yield return Save("shop_icon_rest");
            Vector3 v=Camera.main.WorldToViewportPoint(new Vector3(-165f,130f,0));
            int pointer=city.TestHarness_PointerLayer.InjectDown(new Vector2(v.x*1024f,(1-v.y)*768f));
            yield return null;yield return null;
            yield return Save("shop_icon_pressed");
            city.TestHarness_PointerLayer.InjectUp(pointer);
            yield return null;yield return null;
            yield return Save("shop_icon_released");
            File.WriteAllText(Path.Combine(output,"n253_capture.json"),
                "{\"fixture\":\"staged real player; synthetic shop pointer\",\"meat_count\":"+meat+
                ",\"blood_count\":"+blood+",\"purchased_B_tier\":"+city.TestHarness_RangedFamilyTier(1)+
                ",\"width\":"+Screen.width+",\"height\":"+Screen.height+"}");
        }


        [Serializable] sealed class N253CombatSample {
            public string phase,bodyClip,weapon,lastFireClip;
            public int bodyFrame,flames,beams,audioRequests,audioAdmitted,ammo;
            public int audioD,audioE1,audioE2,audioF2,audioF3;
            public bool secondaryActive;
        }
        [Serializable] sealed class N253CombatSamples {
            public string fixture="Real player, staged arsenal and production shoot/switch/equip owners; no original-device acceptance";
            public List<N253CombatSample> samples=new List<N253CombatSample>();
        }
        IEnumerator CaptureN253Combat(CityGridController city) {
            var samples=new N253CombatSamples();
            city.TestHarness_SetPlayerX(200f);
            var listener=FindFirstObjectByType<AudioListener>();
            var pcm=listener!=null?listener.gameObject.AddComponent<DC1DiagnosticPcm>():null;
            NativeAudioSessionGate.StopAll();NativeAudioSessionGate.ResetDiagnostics();
            NativeAudioSessionGate.TestHarness_ResetClipCounters();
            city.TestHarness_SetRangedFamilyTierRaw(1,0);
            city.TestHarness_GrantAmmo(1,1-city.TestHarness_ItemStock(1));
            city.TestHarness_SwitchWeapon();
            yield return Save("switch_icon_begin");
            yield return new WaitForSeconds(.08f);
            yield return Save("switch_icon_mid");
            yield return new WaitForSeconds(.4f);
            yield return Save("switch_icon_rest");
            city.TestHarness_Shoot();yield return new WaitForSeconds(.5f);
            yield return Save("empty_overlay_before_auto");
            city.TestHarness_Shoot();yield return null;
            yield return Save("empty_auto_selection");
            ObserveN253(city,samples,"empty_auto_selection");
            for(int tier=0;tier<4;tier++) {
                city.TestHarness_SetRangedTier(2,tier);city.TestHarness_GrantAmmo(2,999-city.TestHarness_ItemStock(2));
                typeof(CityGridController).GetMethod("EquipCityRangedFamily",BindingFlags.Instance|BindingFlags.NonPublic)
                    .Invoke(city,new object[]{2,false});
                city.TestHarness_Shoot();yield return new WaitForSeconds(.25f);
                ObserveN253(city,samples,"C"+(tier+1)+"_native_audio");
            }
            city.TestHarness_SetRangedTier(4,0);city.TestHarness_GrantAmmo(4,999-city.TestHarness_ItemStock(4));
            typeof(CityGridController).GetMethod("EquipCityRangedFamily",BindingFlags.Instance|BindingFlags.NonPublic)
                .Invoke(city,new object[]{4,false});
            city.TestHarness_Shoot();yield return new WaitForSeconds(.25f);
            ObserveN253(city,samples,"E1_native_audio");
            for(int mode=1;mode<=2;mode++) {
                city.TestHarness_SetRangedFamily(5);city.TestHarness_SetFMode(mode);
                city.TestHarness_GrantAmmo(5,999-city.TestHarness_ItemStock(5));
                city.TestHarness_ResetShootCooldown();
                city.TestHarness_Shoot();
                yield return Save("F"+(mode+1)+"_intro");
                yield return new WaitForSeconds(.15f);
                yield return Save("F"+(mode+1)+"_tap_effect");
                ObserveN253(city,samples,"F"+(mode+1)+"_tap_effect");
                yield return new WaitForSeconds(.9f);
                yield return Save("F"+(mode+1)+"_tap_complete");
                ObserveN253(city,samples,"F"+(mode+1)+"_tap_complete");
                city.TestHarness_Shoot();
                float holdUntil=Time.time+.8f;
                while(Time.time<holdUntil) {city.TestHarness_Shoot();yield return null;}
                yield return Save("F"+(mode+1)+"_held");
                ObserveN253(city,samples,"F"+(mode+1)+"_held");
                yield return new WaitForSeconds(1f);
                yield return Save("F"+(mode+1)+"_held_release_complete");
                ObserveN253(city,samples,"F"+(mode+1)+"_held_release_complete");
            }
            city.TestHarness_GodMode=false;city.TestHarness_DamagePlayer(5f);
            yield return Save("player_hurt");yield return new WaitForSeconds(.6f);
            ObserveN253(city,samples,"player_hurt_complete");
            city.TestHarness_GodMode=true;
            city.TestHarness_SetCash(100000);city.TestHarness_SetRangedFamilyTierRaw(0,2);
            city.TestHarness_TryBuyWeaponTier(0,3);
            city.TestHarness_TriggerLevelClear();city.TestHarness_ForceShopFadeComplete();
            yield return null;yield return null;
            yield return Save("post_table_bonus1_shop");
            city.TestHarness_TryBuyWeaponTier(0,4);
            yield return Save("post_table_bonus2_shop");
            city.TestHarness_ExitShop();yield return null;yield return null;
            yield return Save("next_level_intro_no_cached_F");
            ObserveN253(city,samples,"next_level_intro");
            File.WriteAllText(Path.Combine(output,"combat_capture.json"),JsonUtility.ToJson(samples,true));
            if(pcm!=null) {pcm.Write(Path.Combine(output,"combat_output.wav"));Destroy(pcm);}
        }
        static void ObserveN253(CityGridController city,N253CombatSamples samples,string phase) {
            int flames=0,beams=0;
            foreach(var f in FindObjectsByType<F2FlameVisual>(FindObjectsSortMode.None))if(f.gameObject.activeInHierarchy)flames++;
            foreach(var f in FindObjectsByType<F3LaserVisual>(FindObjectsSortMode.None))if(f.gameObject.activeInHierarchy)beams++;
            samples.samples.Add(new N253CombatSample {phase=phase,bodyClip=city.TestHarness_SecondaryClipName(),
                bodyFrame=city.TestHarness_SecondaryFrame(),secondaryActive=city.TestHarness_PlayerSecondaryActive,
                weapon=city.TestHarness_HudWeaponLabel(),ammo=city.TestHarness_ItemStock(city.TestHarness_EquippedRangedFamily),
                lastFireClip=city.TestHarness_LastFireClip!=null?city.TestHarness_LastFireClip.name:null,
                audioD=NativeAudioSessionGate.TestHarness_AdmittedCount("Attack_D"),audioE1=NativeAudioSessionGate.TestHarness_AdmittedCount("Attack_E1"),
                audioE2=NativeAudioSessionGate.TestHarness_AdmittedCount("Attack_E2"),audioF2=NativeAudioSessionGate.TestHarness_AdmittedCount("Attack_F2"),
                audioF3=NativeAudioSessionGate.TestHarness_AdmittedCount("Attack_F3"),
                flames=flames,beams=beams,audioRequests=NativeAudioSessionGate.TotalRequests,audioAdmitted=NativeAudioSessionGate.TotalAdmitted});
        }
        IEnumerator CaptureN253Social() {
            yield return SceneManager.LoadSceneAsync("MainMenuScene");yield return null;yield return null;
            var menu=FindFirstObjectByType<MainMenuController>();
            menu.TestHarness_SuppressRealPointer=true;menu.TestHarness_SkipBootSplash();
            yield return new WaitForSeconds(1.5f);
            var listener=FindFirstObjectByType<AudioListener>();
            var pcm=listener!=null?listener.gameObject.AddComponent<DC1DiagnosticPcm>():null;
            NativeAudioSessionGate.TestHarness_ResetClipCounters();
            yield return Save("social_rest");
            Rect[] targets={new Rect(0,572,113,100),new Rect(110,584,103,86)};
            string[] names={"list","achievement"};
            for(int i=0;i<targets.Length;i++) {
                menu.TestHarness_InjectMovePointer(targets[i],true);yield return null;yield return null;
                yield return Save(names[i]+"_pressed");yield return new WaitForSeconds(.25f);
                yield return Save(names[i]+"_held");
                menu.TestHarness_InjectMovePointer(targets[i],false);yield return null;yield return null;
                yield return Save(names[i]+"_released");
            }
            File.WriteAllText(Path.Combine(output,"social_capture.json"),
                "{\"fixture\":\"staged logical pointer through real repaint/per-control NativeMenuMoveInput and ClickSprite; external service disconnected\",\"button_admissions\":"+
                NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_button")+",\"width\":"+Screen.width+",\"height\":"+Screen.height+"}");
            if(pcm!=null) {pcm.Write(Path.Combine(output,"social_output.wav"));Destroy(pcm);}
        }
        IEnumerator CaptureN253Sandbox() {
            yield return SceneManager.LoadSceneAsync("CombatSandboxV2");yield return null;yield return null;
            var sandbox=FindFirstObjectByType<CombatSandboxV2>();
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(CombatSandboxV2).GetField("debugUiHidden",flags).SetValue(sandbox,true);
            sandbox.TestOverrideBlueIsPressed=false;sandbox.TestOverrideBlueWasPressedThisFrame=false;
            sandbox.TestHarness_SetAutoSpawn123(false);sandbox.TestHarness_SetCityGrindSpawner(false);
            sandbox.TestHarness_SetGodMode(true);sandbox.TestHarness_ClearDinos();
            sandbox.TestHarness_SetPlayerX(0f);sandbox.TestHarness_SpawnDino(0,true);
            sandbox.TestHarness_SelectDino(0);sandbox.TestHarness_SetDinoX(0,230f);
            sandbox.TestHarness_SetDebugEnemyHP(1f);sandbox.TestHarness_SetWeapon(4,0,200);
            var listener=FindFirstObjectByType<AudioListener>();
            var pcm=listener!=null?listener.gameObject.AddComponent<DC1DiagnosticPcm>():null;
            NativeAudioSessionGate.StopAll();NativeAudioSessionGate.ResetDiagnostics();
            typeof(CombatSandboxV2).GetMethod("DischargeE",flags).Invoke(sandbox,new object[]{WeaponFamilyTable.Rows[4][0]});
            yield return Save("sandbox_E_gore_flight");yield return new WaitForSeconds(2.5f);
            yield return Save("sandbox_E_gore_settled");yield return new WaitForSeconds(.5f);
            yield return Save("sandbox_E_gore_persisted");
            int meat=FindObjectsByType<WeaponSuirouParticle>(FindObjectsSortMode.None).Length;
            int eAudio=NativeAudioSessionGate.TestHarness_AdmittedCount("Attack_E2");
            NativeAudioSessionGate.TestHarness_ResetClipCounters();
            sandbox.TestHarness_SetGodMode(false);sandbox.TestHarness_DamagePlayer(5f);
            yield return Save("sandbox_player_hurt");yield return new WaitForSeconds(.5f);
            File.WriteAllText(Path.Combine(output,"sandbox_capture.json"),
                "{\"fixture\":\"staged E fatal and nonfatal player hit through production owners\",\"meat_count\":"+meat+
                ",\"e_audio_admissions\":"+eAudio+
                ",\"hurt_audio_admissions\":"+NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Role")+
                ",\"width\":"+Screen.width+",\"height\":"+Screen.height+"}");
            if(pcm!=null) {pcm.Write(Path.Combine(output,"sandbox_output.wav"));Destroy(pcm);}
        }

        static void SeedMenuVariant(int variant)
        {
            for(int seed=0;seed<100;seed++) {
                UnityEngine.Random.InitState(seed);
                if((UnityEngine.Random.value*100f>=50f?1:0)!=variant)continue;
                UnityEngine.Random.InitState(seed);
                return;
            }
            throw new InvalidOperationException("Could not seed menu variant");
        }

        IEnumerator CaptureMenuSustain(MainMenuController menu)
        {
            // Real player Update/source ticks and native timer, no synthetic
            // time stepping. Seeds select each source species for coverage;
            // they do not claim a measured random distribution.
            var randomState=UnityEngine.Random.state;
            SeedMenuVariant(0);
            bool seededSecond=false;
            var samples=new MenuSamples();
            float start=Time.time,next=0f;
            int capture=0;
            float[] captureTimes={16f,40f,80f};
            while(Time.time-start<88f) {
                float elapsed=Time.time-start;
                var visual=menu.TestHarness_DinoVisual;
                if(menu.TestHarness_DinoActive && !seededSecond) {
                    SeedMenuVariant(1);seededSecond=true;
                }
                if(elapsed>=next) {
                    var audio=menu.GetComponent<AudioSource>();
                    samples.samples.Add(new MenuSample {seconds=elapsed,x=menu.TestHarness_DinoLogicalX,
                        actorActive=menu.TestHarness_DinoActive,trackActive=visual.primary.Active,
                        clip=visual.primary.Clip?.name,frame=visual.primary.Frame,
                        advances=visual.primary.AdvanceCount,actorInstance=visual.gameObject.GetInstanceID(),
                        roarPlaying=audio!=null && audio.isPlaying});
                    next=elapsed+0.25f;
                    File.WriteAllText(Path.Combine(output,"menu_sustained.json"),JsonUtility.ToJson(samples));
                }
                if(capture<captureTimes.Length && elapsed>=captureTimes[capture]) {
                    yield return Save("menu_sustained_"+captureTimes[capture].ToString("0"));capture++;
                }
                yield return null;
            }
            UnityEngine.Random.state=randomState;
        }

        IEnumerator WaitHouse(Func<bool> condition,string stage)
        {
            float end=Time.realtimeSinceStartup+4f;
            while(!condition() && Time.realtimeSinceStartup<end)yield return null;
            if(!condition())houseFailure="Timed out waiting for actual house stage: "+stage;
        }

        IEnumerator CaptureSpecies(MainMenuController menu)
        {
            var savedRandom=UnityEngine.Random.state;
            for(int variant=0;variant<2;variant++) {
                int seed=0;
                for(;seed<100;seed++) {
                    UnityEngine.Random.InitState(seed);
                    if((UnityEngine.Random.value*100f>=50f?1:0)==variant)break;
                }
                UnityEngine.Random.InitState(seed);
                typeof(MainMenuController).GetMethod("InitBackgroundDino",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(menu,null);
                menu.TestHarness_StepBackgroundDino(25f);
                for(int i=0;i<12;i++)menu.TestHarness_DinoVisual.primary.Tick(0.025f);
                yield return null; // Render the selected source clip before framebuffer capture.
                yield return Save(variant==0?"menu_native_k3":"menu_native_k2");
            }
            UnityEngine.Random.state=savedRandom;
        }

        [Serializable] sealed class RenderSample
        {
            public string mode,setting,shader;
            public int width,height,msaa;
            public int bitingFrame,deadFrame,deadClipFrames;
            public float shaderMode;
            public bool mipmaps,pointFramebuffer;
        }
        [Serializable] sealed class RenderSamples { public List<RenderSample> samples=new List<RenderSample>(); }
        [Serializable] sealed class NativeUiMotionSample {
            public int frame,collected,presented,mismatches,cash,dinoFrame;
            public float playerX,dinoX;
        }
        [Serializable] sealed class MenuLaunchSample {
            public int frame,level,cash;public float seconds,health;
            public string scene,screen;public bool selectionVisible,loadPending;
            public Rect loading;
        }
        [Serializable] sealed class MenuLaunchSamples {
            public string path;public bool usesUnpausedUpdate=true,physicalPointerInput=false;
            public List<MenuLaunchSample> samples=new List<MenuLaunchSample>();
        }
        IEnumerator CaptureMenuLaunch()
        {
            foreach(string path in new[]{"Resume","GO"}) {
                CityGrindStartPreset.ForLevel(7).WriteCommittedRun(7);
                DC1Preferences.SetInt("DC1_CityGrind_MaxLevel",20);
                DC1Preferences.SetInt("DC1_CityGrind_Cash",321);
                DC1Preferences.SetFloat(CityGrindStartPreset.HealthPrefKey,73f);DC1Preferences.Save();
                Debug.Log("[DC1-CAPTURE] "+path+" fixture before scene: level="+DC1Preferences.GetInt("DC1_CityGrind_Level")+" cash="+DC1Preferences.GetInt("DC1_CityGrind_Cash"));
                yield return SceneManager.LoadSceneAsync("MainMenuScene");
                yield return null;yield return null;
                var menu=FindFirstObjectByType<MainMenuController>();
                Debug.Log("[DC1-CAPTURE] "+path+" fixture after scene: level="+DC1Preferences.GetInt("DC1_CityGrind_Level")+" cash="+DC1Preferences.GetInt("DC1_CityGrind_Cash")+" runners="+FindObjectsByType<DC1ConvergenceCapture>(FindObjectsSortMode.None).Length);
                menu.TestHarness_SkipBootSplash();
                // Only initial fixture setup is stepped. Selection, leave, loading,
                // and scene consumption below run on the real unpaused Update clock.
                menu.TestHarness_StepReveal(5f);
                if(path=="GO") {
                    menu.TestHarness_GoLevelSelect();
                    while(menu.TestHarness_LevelPresentation!=null && !menu.TestHarness_LevelPresentation.Interactive)
                        yield return null;
                    menu.TestHarness_LevelUp();menu.TestHarness_LevelUp();
                }
                yield return Save(path+"_before_selection");
                var result=new MenuLaunchSamples {path=path};
                float started=Time.realtimeSinceStartup;
                if(path=="Resume")menu.TestHarness_SelectResume();else menu.TestHarness_SelectGo();
                var screenshots=new HashSet<string>();
                while(Time.realtimeSinceStartup-started<15f) {
                    var city=FindFirstObjectByType<CityGridController>();
                    var sample=new MenuLaunchSample {
                        frame=Time.frameCount,seconds=Time.realtimeSinceStartup-started,
                        scene=SceneManager.GetActiveScene().name,
                        level=DC1Preferences.GetInt("DC1_CityGrind_Level"),cash=DC1Preferences.GetInt("DC1_CityGrind_Cash"),
                        health=DC1Preferences.GetFloat(CityGrindStartPreset.HealthPrefKey)
                    };
                    if(menu!=null) {
                        sample.screen=menu.TestHarness_Screen.ToString();
                        sample.selectionVisible=menu.TestHarness_SelectionVisible;
                        sample.loadPending=menu.TestHarness_GameplayLoadPending;
                        if(sample.screen=="Launching")sample.loading=menu.TestHarness_LoadingRect;
                    }
                    result.samples.Add(sample);
                    string stage=city!=null?"city_loaded":sample.screen=="Launching"
                        ?sample.loadPending?"loading_rest":sample.loading.x<1000?"loading_visible":"loading_enter"
                        :sample.selectionVisible?"selection":"leave";
                    if(screenshots.Add(stage))ScreenCapture.CaptureScreenshot(Path.Combine(output,path+"_"+stage+".png"));
                    if(city!=null) {
                        yield return null;yield return null;
                        float until=Time.realtimeSinceStartup+10f;
                        while(city.TestHarness_LevelIntroActive && Time.realtimeSinceStartup<until)yield return null;
                        yield return Save(path+"_city_presented");break;
                    }
                    yield return null;
                }
                File.WriteAllText(Path.Combine(output,path+"_launch.json"),JsonUtility.ToJson(result,true));
                if(FindFirstObjectByType<CityGridController>()==null) {
                    deadline=Time.realtimeSinceStartup+1f;
                    throw new InvalidOperationException(path+" did not load City within15seconds");
                }
            }
        }
        [Serializable] sealed class FinishActorSample {
            public int index,frame;public float x;public bool attacking,hit,facingRight;public string clip;
        }
        [Serializable] sealed class FinishSample {
            public int frame;public float seconds,playerX,hp;public bool forcedWalk,fading,shop;
            public List<FinishActorSample> actors=new List<FinishActorSample>();
        }
        [Serializable] sealed class FinishSamples {
            public bool syntheticSpawnAndPositionFixture=true,usesActualFinishCrossingAndUnpausedUpdate=true;
            public int biteFrame12Events;public List<FinishSample> samples=new List<FinishSample>();
        }
        IEnumerator CaptureFinishAi()
        {
            CityGrindStartPreset.ForLevel(1).WriteCommittedRun(1);DC1Preferences.Save();
            yield return SceneManager.LoadSceneAsync("CityGrindScene");yield return null;yield return null;
            var city=FindFirstObjectByType<CityGridController>();
            city.TestHarness_SkipLevelIntro();city.TestHarness_DebugUiHidden=true;
            // timeScale0 holds animation but does not suppress AI admission.
            // Disable only during fixture staging so two walkers remain walkers.
            city.enabled=false;
            city.TestHarness_ClearDinos();city.TestHarness_GodMode=false;
            city.TestHarness_SetPlayerX(CityGridController.FinishX-1f);
            Time.timeScale=0f;
            var result=new FinishSamples();
            var list=(System.Collections.IList)typeof(CityGridController).GetField("dinos",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
            for(int i=0;i<3;i++) {
                city.TestHarness_ForceSpawn(0);
                city.TestHarness_SetDinoX(i,city.TestHarness_PlayerX+(i==0?-20f:20f));
                city.TestHarness_SetDinoFacing(i,i!=2);
                city.TestHarness_SetDinoAttacking(i,i==2);
                list[i].GetType().GetField("entering").SetValue(list[i],false);
                city.TestHarness_ForceDinoClip(i,i==2?"_attack":"_move");
                var visual=(Power2DSourceTruthRenderer)list[i].GetType().GetField("visual").GetValue(list[i]);
                if(i==2)visual.primary.EnterFrame+=(t,f)=>{if(f==12 && t.Clip!=null && t.Clip.name.EndsWith("_attack"))result.biteFrame12Events++;};
            }
            yield return Save("finish_before_crossing");
            city.TestHarness_SetPlayerX(CityGridController.FinishX+1f);city.enabled=true;Time.timeScale=1f;
            float started=Time.realtimeSinceStartup;
            var images=new HashSet<string>();
            while(Time.realtimeSinceStartup-started<8f) {
                var sample=new FinishSample {frame=Time.frameCount,seconds=Time.realtimeSinceStartup-started,
                    playerX=city.TestHarness_PlayerX,hp=city.TestHarness_PlayerHP,
                    forcedWalk=city.TestHarness_ForcedWalkActive,fading=city.TestHarness_ForcedWalkFading,
                    shop=city.TestHarness_ShopScreenActive};
                if(!sample.shop)for(int i=0;i<Math.Min(3,city.TestHarness_DinoCount);i++) {
                    var visual=(Power2DSourceTruthRenderer)list[i].GetType().GetField("visual").GetValue(list[i]);
                    sample.actors.Add(new FinishActorSample {index=i,x=city.TestHarness_DinoX(i),
                        frame=visual.primary.Frame,clip=city.TestHarness_DinoClipName(i),
                        attacking=city.TestHarness_DinoAttacking(i),hit=city.TestHarness_DinoHitThisCycle(i),
                        facingRight=city.TestHarness_DinoFacingRight(i)});
                }
                result.samples.Add(sample);
                string stage=sample.shop?"shop":sample.fading?"fade":sample.forcedWalk?"walking":"crossing";
                if(images.Add(stage))ScreenCapture.CaptureScreenshot(Path.Combine(output,"finish_"+stage+".png"));
                if(sample.shop)break;
                yield return null;
            }
            File.WriteAllText(Path.Combine(output,"finish_ai.json"),JsonUtility.ToJson(result,true));
            if(!city.TestHarness_ShopScreenActive){deadline=Time.realtimeSinceStartup+1f;throw new InvalidOperationException("Finish did not reach shop");}
            yield return null;yield return null;
            yield return Save("finish_shop_presented");
        }
        IEnumerator CaptureNativeMenuUi()
        {
            float oldTime=Time.timeScale;
            bool oldPaused=Power2DSourceTruthRenderer.GlobalPaused;
            bool old2x=GameplayRenderTarget.Supersample2xEnabled;
            bool had2x=DC1Preferences.HasKey("DC1_InternalSupersample2x");
            bool oldSmooth=GameplayPresentationCamera.GlobalModeName=="Smooth";
            try {
                CityGrindStartPreset.ForLevel(7).WriteCommittedRun(7);
                yield return SceneManager.LoadSceneAsync("MainMenuScene");
                yield return null;yield return null;
                var menu=FindFirstObjectByType<MainMenuController>();
                menu.TestHarness_SkipBootSplash();menu.TestHarness_StepReveal(5f);
                menu.TestHarness_ForceDinoNearZero();Time.timeScale=0f;
                Power2DSourceTruthRenderer.GlobalPaused=true;
                foreach(string state in new[]{"Primary","Options","Checkpoint","Help"}) {
                    if(state=="Options")menu.TestHarness_GoOptions();
                    if(state=="Checkpoint"){menu.TestHarness_GoNewGame();menu.TestHarness_GoLevelSelect();}
                    if(state=="Help"){menu.TestHarness_GoNewGame();menu.TestHarness_ShowHelp();}
                    menu.TestHarness_StepReveal(5f);menu.TestHarness_StepReveal(5f);
                    foreach(int mode in menuAtlasMap==null?new[]{0,1}:new[]{0,1,2}) {
                        NativeFramebufferUi.DiagnosticEnabled=mode!=0;
                        useMenuAtlas=mode==2;
                        foreach(string setting in new[]{"Original_1x","Smooth_1x","Smooth_2x"}) {
                            bool want2x=setting.EndsWith("2x"),wantSmooth=setting.StartsWith("Smooth");
                            if(GameplayRenderTarget.Supersample2xEnabled!=want2x)GameplayRenderTarget.TogglePersistentSupersample();
                            if((GameplayPresentationCamera.GlobalModeName=="Smooth")!=wantSmooth)GameplayPresentationCamera.CycleGlobalMode();
                            yield return null;yield return null;
                            string label=mode==0?"Baseline_":menuAtlasMap==null?"Candidate_":mode==1?"Crop_":"Atlas_";
                            yield return Save(state+"_"+label+setting);
                            if(captureExactDisplay && state=="Primary" && mode==1 && setting=="Smooth_1x" && NativeFramebufferUi.Completed!=null)
                                SaveExactDisplay(NativeFramebufferUi.Completed,"Primary_Smooth_1x");
                        }
                    }
                }
            } finally {
                NativeFramebufferUi.DiagnosticEnabled=null;NativeFramebufferUi.Release();
                useMenuAtlas=false;
                Time.timeScale=oldTime;Power2DSourceTruthRenderer.GlobalPaused=oldPaused;
                if(GameplayRenderTarget.Supersample2xEnabled!=old2x)GameplayRenderTarget.TogglePersistentSupersample();
                if((GameplayPresentationCamera.GlobalModeName=="Smooth")!=oldSmooth)GameplayPresentationCamera.CycleGlobalMode();
                if(!had2x)DC1Preferences.DeleteKey("DC1_InternalSupersample2x");
                GameplayRenderTarget.SetPersistentAspectMode(savedAspect);DC1Preferences.Save();
            }
        }
        [Serializable] sealed class NativeUiMotionSamples {
            public List<NativeUiMotionSample> samples=new List<NativeUiMotionSample>();
        }
        IEnumerator CaptureNativeUi()
        {
            bool old2x=GameplayRenderTarget.Supersample2xEnabled;
            bool oldSmooth=GameplayPresentationCamera.GlobalModeName=="Smooth";
            bool had2x=DC1Preferences.HasKey("DC1_InternalSupersample2x");
            float oldTime=Time.timeScale;
            bool oldPaused=Power2DSourceTruthRenderer.GlobalPaused;
            bool oldDespawn=DebugCorpseSettings.DespawnEnabled;
            var records=new RenderSamples();
            try {
                CityGrindStartPreset.ForLevel(1).WriteCommittedRun(1);
                yield return SceneManager.LoadSceneAsync("CityGrindScene");
                yield return null;yield return null;
                var city=FindFirstObjectByType<CityGridController>();
                city.TestHarness_SkipLevelIntro();city.TestHarness_ClearDinos();
                city.TestHarness_DebugUiHidden=true;
                city.TestHarness_SetPlayerX(0f);
                Time.timeScale=0f;
                city.TestHarness_ForceSpawn(0);city.TestHarness_ForceSpawn(0);
                var dinos=(IList)typeof(CityGridController).GetField("dinos",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
                for(int i=0;i<2;i++) {
                    var d=(CityGridController.CityDinoInstance)dinos[i];
                    d.hp=0;d.entering=false;d.entryMode=CityGridController.CityDinoEntry.WalkIn;
                    d.transform.position=new Vector3(i==0?-180:180,d.transform.position.y,d.transform.position.z);
                    d.visual.primary.Play(d.visual.FindClip(i==0?"k3_attack":"k3_die"),i==0?12:17);
                }
                Power2DSourceTruthRenderer.GlobalPaused=true;
                foreach(bool candidate in new[]{false,true}) {
                    NativeFramebufferUi.DiagnosticEnabled=candidate;
                    foreach(string setting in new[]{"Original_1x","Smooth_1x","Smooth_2x"}) {
                        bool want2x=setting.EndsWith("2x"),wantSmooth=setting.StartsWith("Smooth");
                        if(GameplayRenderTarget.Supersample2xEnabled!=want2x)GameplayRenderTarget.TogglePersistentSupersample();
                        if((GameplayPresentationCamera.GlobalModeName=="Smooth")!=wantSmooth)GameplayPresentationCamera.CycleGlobalMode();
                        yield return null;yield return null;
                        var rt=FindFirstObjectByType<GameplayRenderTarget>().Target;
                        records.samples.Add(new RenderSample{mode=candidate?"Candidate":"Baseline",setting=setting,width=rt.width,height=rt.height,shader=candidate?"Native framebuffer UI;draws="+NativeFramebufferUi.LastDrawCount:"Existing GUI after world",shaderMode=wantSmooth?1:0,pointFramebuffer=rt.filterMode==FilterMode.Point,msaa=rt.antiAliasing,mipmaps=rt.useMipMap});
                        yield return Save((candidate?"Candidate_":"Baseline_")+setting);
                        if(candidate && NativeFramebufferUi.Completed!=null) {
                            var before=RenderTexture.active;
                            try {
                                var frame=NativeFramebufferUi.Completed;
                                RenderTexture.active=frame;
                                var pixels=new Texture2D(frame.width,frame.height,TextureFormat.RGBA32,false);
                                pixels.ReadPixels(new Rect(0,0,frame.width,frame.height),0,0);pixels.Apply();
                                File.WriteAllBytes(Path.Combine(output,"Candidate_"+setting+"_Completed.png"),pixels.EncodeToPNG());
                                Destroy(pixels);
                                if(captureExactDisplay && setting.EndsWith("1x"))SaveExactDisplay(frame,setting);
                            } finally {RenderTexture.active=before;}
                        }
                    }
                }
                File.WriteAllText(Path.Combine(output,"native_ui_observed.json"),JsonUtility.ToJson(records,true));
                NativeFramebufferUi.DiagnosticEnabled=true;
                city.TestHarness_GodMode=true;
                DebugCorpseSettings.SetDespawnEnabled(false);
                Power2DSourceTruthRenderer.GlobalPaused=false;Time.timeScale=1f;
                var motion=new NativeUiMotionSamples();
                for(int i=0;i<30;i++) {
                    city.TestHarness_SetCash(700+i);
                    city.TestHarness_SetPlayerX(i*2);
                    city.TestHarness_SetDinoX(0,-180+i*3);
                    yield return null;
                    yield return new WaitForEndOfFrame();
                    NativeFramebufferUi.FlushPending();
                    motion.samples.Add(new NativeUiMotionSample {frame=Time.frameCount,
                        collected=NativeFramebufferUi.LastCollectedFrame,presented=NativeFramebufferUi.LastPresentedFrame,
                        mismatches=NativeFramebufferUi.FrameMismatchCount,cash=city.TestHarness_CashTotal,
                        playerX=city.TestHarness_PlayerX,dinoX=city.TestHarness_DinoX(0),
                        dinoFrame=((CityGridController.CityDinoInstance)dinos[0]).visual.primary.Frame});
                    if(i==0 || i==29)yield return city.TestHarness_CaptureFinalFrame("moving_"+i,output,"native-ui",0);
                }
                File.WriteAllText(Path.Combine(output,"native_ui_motion.json"),JsonUtility.ToJson(motion,true));
            } finally {
                NativeFramebufferUi.DiagnosticEnabled=null;
                NativeFramebufferUi.Release();Time.timeScale=oldTime;
                Power2DSourceTruthRenderer.GlobalPaused=oldPaused;
                DebugCorpseSettings.SetDespawnEnabled(oldDespawn);
                if(GameplayRenderTarget.Supersample2xEnabled!=old2x)GameplayRenderTarget.TogglePersistentSupersample();
                if((GameplayPresentationCamera.GlobalModeName=="Smooth")!=oldSmooth)GameplayPresentationCamera.CycleGlobalMode();
                if(!had2x)DC1Preferences.DeleteKey("DC1_InternalSupersample2x");
                GameplayRenderTarget.SetPersistentAspectMode(savedAspect);DC1Preferences.Save();
            }
        }

        [Serializable] sealed class MenuReturnSample {
            public string path,phase,screen;public int frame;public float seconds;
            public bool selection,primaryInteractive,help,optionsLeaving;
        }
        [Serializable] sealed class MenuReturnRecord {
            public bool physicalPointerInput=false;public List<MenuReturnSample> samples=new List<MenuReturnSample>();
        }
        bool PrimaryRest(MainMenuController menu)
        {
            if(!menu.TestHarness_PrimarySelectionEnabled)return false;
            for(int i=0;i<5;i++)if(menu.TestHarness_RevealActive(i))return false;
            return true;
        }
        IEnumerator WaitMenuReturn(MainMenuController menu,string path,string phase,Func<bool> ready,MenuReturnRecord record)
        {
            float started=Time.realtimeSinceStartup,until=started+8f;bool mid=false;
            while(!ready() && Time.realtimeSinceStartup<until) {
                yield return null;
                record.samples.Add(new MenuReturnSample {path=path,phase=phase,screen=menu.TestHarness_Screen.ToString(),frame=Time.frameCount,seconds=Time.realtimeSinceStartup-started,
                    selection=menu.TestHarness_SelectionVisible,primaryInteractive=menu.TestHarness_PrimarySelectionEnabled,help=menu.TestHarness_HelpVisible,optionsLeaving=menu.TestHarness_OptionsLeaving});
                if(!mid && Time.realtimeSinceStartup-started>=0.15f){mid=true;yield return Save(path+"_"+phase+"_moving");}
            }
            if(!ready()){deadline=Time.realtimeSinceStartup+1f;throw new InvalidOperationException(path+" "+phase+" timed out");}
            yield return Save(path+"_"+phase+"_rest");
        }
        IEnumerator CaptureMenuReturns()
        {
            CityGrindStartPreset.ForLevel(7).WriteCommittedRun(7);
            yield return SceneManager.LoadSceneAsync("MainMenuScene");yield return null;yield return null;
            var menu=FindFirstObjectByType<MainMenuController>();menu.TestHarness_SkipBootSplash();
            var record=new MenuReturnRecord();
            yield return WaitMenuReturn(menu,"Primary","initial",()=>PrimaryRest(menu),record);
            menu.TestHarness_SelectOptions();
            bool OptionsRest() {
                var rows=(NativeUiRectTween[])typeof(MainMenuController).GetField("optionsTweens",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(menu);
                return menu.TestHarness_Screen==MainMenuController.MenuScreen.Options && rows!=null && !Array.Exists(rows,t=>t.Active);
            }
            yield return WaitMenuReturn(menu,"Options","enter",OptionsRest,record);
            menu.TestHarness_BackFromOptions();
            yield return WaitMenuReturn(menu,"Options","return",()=>PrimaryRest(menu),record);
            menu.TestHarness_SelectCity();
            yield return WaitMenuReturn(menu,"Checkpoint","enter",()=>menu.TestHarness_LevelPresentation!=null && menu.TestHarness_LevelPresentation.Interactive,record);
            menu.TestHarness_BackFromLevelSelect();
            yield return WaitMenuReturn(menu,"Checkpoint","return",()=>PrimaryRest(menu) && menu.TestHarness_LevelPresentation==null,record);
            menu.TestHarness_ShowHelp();yield return null;yield return Save("Help_visible");
            if(!menu.TestHarness_HelpVisible){deadline=Time.realtimeSinceStartup+1f;throw new InvalidOperationException("Help not visible");}
            menu.TestHarness_CloseHelp();yield return null;yield return Save("Help_closed");
            if(!PrimaryRest(menu)){deadline=Time.realtimeSinceStartup+1f;throw new InvalidOperationException("Help did not restore primary input");}
            // Overlay smoke check only; this is the existing out-of-scope placeholder.
            menu.TestHarness_GoLifeMode();yield return null;yield return Save("Life_placeholder");
            menu.TestHarness_GoNewGame();
            yield return WaitMenuReturn(menu,"LifePlaceholder","return",()=>PrimaryRest(menu),record);
            File.WriteAllText(Path.Combine(output,"menu_return.json"),JsonUtility.ToJson(record,true));
        }

        [Serializable] sealed class ExactDisplayRecord {
            public string setting,api;public int sourceWidth,sourceHeight,width,height;public bool linear=true;
        }
        void SaveExactDisplay(RenderTexture frame,string setting)
        {
            // Offscreen display magnification only. World raster stays native1x.
            var descriptor=frame.descriptor;descriptor.width*=2;descriptor.height*=2;
            descriptor.depthBufferBits=0;descriptor.msaaSamples=1;descriptor.useMipMap=false;
            var target=new RenderTexture(descriptor);target.Create();
            var material=new Material(Resources.Load<Shader>("Shaders/DC1NativeUi"));
            material.EnableKeyword("DC1_PRESENT_LINEAR");
            var mesh=new Mesh();mesh.vertices=new[]{new Vector3(0,0,0),new Vector3(1,0,0),new Vector3(1,1,0),new Vector3(0,1,0)};
            mesh.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};mesh.triangles=new[]{0,1,2,0,2,3};
            var props=new MaterialPropertyBlock();props.SetTexture("_MainTex",frame);props.SetColor("_Tint",Color.white);
            props.SetVector("_RectClip",new Vector4(-1,-1,2,2));
            // Offscreen target readback orientation differs from backbuffer
            // presentation on D3D. Keep PNG rows in the logical source order.
            props.SetVector("_UvRect",SystemInfo.graphicsUVStartsAtTop?new Vector4(0,1,1,-1):new Vector4(0,0,1,1));
            var commands=new UnityEngine.Rendering.CommandBuffer();var previous=RenderTexture.active;
            var previousFilter=frame.filterMode;
            Texture2D pixels=null;
            try {
                // GL uses the texture's sampler state rather than the
                // D3D/Metal inline sampler. This diagnostic is display-only.
                frame.filterMode=FilterMode.Bilinear;
                commands.SetRenderTarget(target);commands.SetViewport(new Rect(0,0,target.width,target.height));
                commands.DrawMesh(mesh,Matrix4x4.identity,material,0,1,props);Graphics.ExecuteCommandBuffer(commands);
                RenderTexture.active=target;pixels=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
                pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();
                string name="Candidate_"+setting+"_ExactDisplay2x";
                File.WriteAllBytes(Path.Combine(output,name+".png"),pixels.EncodeToPNG());
                File.WriteAllText(Path.Combine(output,name+".json"),JsonUtility.ToJson(new ExactDisplayRecord {
                    setting=setting,api=SystemInfo.graphicsDeviceType.ToString(),sourceWidth=frame.width,sourceHeight=frame.height,width=target.width,height=target.height},true));
            } finally {
                frame.filterMode=previousFilter;
                RenderTexture.active=previous;commands.Release();target.Release();Destroy(target);Destroy(material);Destroy(mesh);
                if(pixels!=null)Destroy(pixels);
            }
        }

        IEnumerator CaptureRenderChain()
        {
            bool old2x=GameplayRenderTarget.Supersample2xEnabled;
            bool had2x=DC1Preferences.HasKey("DC1_InternalSupersample2x");
            bool oldSmooth=GameplayPresentationCamera.GlobalModeName=="Smooth";
            var records=new RenderSamples();
            const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
            try
            {
                foreach(string mode in new[]{"City","Sandbox"})
                {
                    Power2DSourceTruthRenderer.GlobalPaused=false;
                    SceneManager.LoadScene(mode=="City"?"CityGrindScene":"CombatSandboxV2");
                    yield return null;yield return null;
                    Component controller;
                    IList dinos;
                    float playerX;
                    if(mode=="City")
                    {
                        var city=UnityEngine.Object.FindFirstObjectByType<CityGridController>();
                        city.TestHarness_SkipLevelIntro();city.TestHarness_ClearDinos();
                        city.TestHarness_DebugUiHidden=true;city.enabled=false;
                        if(captureExactDisplay)city.TestHarness_SetPlayerX(Camera.main.transform.position.x);
                        city.TestHarness_ForceSpawn(0);city.TestHarness_ForceSpawn(0);
                        controller=city;playerX=city.TestHarness_PlayerX;
                    }
                    else
                    {
                        var sandbox=UnityEngine.Object.FindFirstObjectByType<CombatSandboxV2>();
                        typeof(CombatSandboxV2).GetMethod("ClearDinos",flags).Invoke(sandbox,null);
                        typeof(CombatSandboxV2).GetField("debugUiHidden",flags).SetValue(sandbox,true);
                        sandbox.enabled=false;sandbox.TestHarness_SpawnDino(0,true);sandbox.TestHarness_SpawnDino(0,true);
                        controller=sandbox;playerX=sandbox.TestHarness_PlayerX;
                    }
                    dinos=(IList)controller.GetType().GetField("dinos",flags).GetValue(controller);
                    // Exact display fixtures apply poses without advancing the animation clock.
                    if(captureExactDisplay)Power2DSourceTruthRenderer.GlobalPaused=true;
                    Power2DSourceTruthRenderer bitingVisual=null,deadVisual=null;
                    for(int i=0;i<2;i++)
                    {
                        object d=dinos[i];var type=d.GetType();
                        if(captureExactDisplay && mode=="City") {
                            var instance=(CityGridController.CityDinoInstance)d;
                            instance.hp=0;instance.entering=false;instance.entryMode=CityGridController.CityDinoEntry.WalkIn;
                        }
                        var transform=(Transform)type.GetField("transform").GetValue(d);
                        transform.position=new Vector3(playerX+(i==0?-180:180),transform.position.y,transform.position.z);
                        var visual=(Power2DSourceTruthRenderer)type.GetField("visual").GetValue(d);
                        var clip=visual.FindClip(i==0?"k3_attack":"k3_die");
                        int pose=i==0?12:(captureDeadPoseFrame<0?clip.frames.Length-1:captureDeadPoseFrame);
                        visual.primary.Play(clip,pose);
                        if(captureExactDisplay)
                            typeof(Power2DSourceTruthRenderer).GetMethod("Render",flags).Invoke(visual,null);
                        if(i==0)bitingVisual=visual;else deadVisual=visual;
                    }
                    yield return null; // Present the selected pose before holding it.
                    Power2DSourceTruthRenderer.GlobalPaused=true;
                    foreach(string setting in new[]{"Original_1x","Smooth_1x","Smooth_2x"})
                    {
                        bool want2x=setting.EndsWith("2x"),wantSmooth=setting.StartsWith("Smooth");
                        if(GameplayRenderTarget.Supersample2xEnabled!=want2x)GameplayRenderTarget.TogglePersistentSupersample();
                        if((GameplayPresentationCamera.GlobalModeName=="Smooth")!=wantSmooth)GameplayPresentationCamera.CycleGlobalMode();
                        yield return null;yield return null;
                        var rt=UnityEngine.Object.FindFirstObjectByType<GameplayRenderTarget>().Target;
                        var presenter=UnityEngine.Object.FindFirstObjectByType<GameplayPresentationCamera>();
                        var material=(Material)typeof(GameplayPresentationCamera).GetField("material",flags).GetValue(presenter);
                        records.samples.Add(new RenderSample {mode=mode,setting=setting,shader=material.shader.name,width=rt.width,height=rt.height,msaa=rt.antiAliasing,mipmaps=rt.useMipMap,pointFramebuffer=rt.filterMode==FilterMode.Point,shaderMode=material.GetFloat("_Mode"),bitingFrame=bitingVisual.primary.Frame,deadFrame=deadVisual.primary.Frame,deadClipFrames=deadVisual.primary.Clip.frames.Length});
                        yield return Save(mode+"_"+setting);
                        if(captureExactDisplay) {
                            var previous=RenderTexture.active;
                            Texture2D pixels=null;
                            try {
                                RenderTexture.active=rt;
                                pixels=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);
                                pixels.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);pixels.Apply();
                                File.WriteAllBytes(Path.Combine(output,mode+"_"+setting+"_World.png"),pixels.EncodeToPNG());
                                if(setting.EndsWith("1x"))SaveExactDisplay(rt,mode+"_"+setting);
                            } finally {RenderTexture.active=previous;if(pixels!=null)Destroy(pixels);}
                        }
                    }
                }
                File.WriteAllText(Path.Combine(output,"render_chain_observed.json"),JsonUtility.ToJson(records,true));
            }
            finally
            {
                Power2DSourceTruthRenderer.GlobalPaused=false;
                if(GameplayRenderTarget.Supersample2xEnabled!=old2x)GameplayRenderTarget.TogglePersistentSupersample();
                if((GameplayPresentationCamera.GlobalModeName=="Smooth")!=oldSmooth)GameplayPresentationCamera.CycleGlobalMode();
                if(!had2x)DC1Preferences.DeleteKey("DC1_InternalSupersample2x");
                DC1Preferences.Save();
            }
        }

        IEnumerator SaveHouse(CityGridController city,string stage)
        {
            // Capture the rendered pose first, then hold all source clocks while writing PNG.
            yield return new WaitForEndOfFrame();
            bool wasPaused=Power2DSourceTruthRenderer.GlobalPaused;
            Power2DSourceTruthRenderer.GlobalPaused=true;
            Time.timeScale=0f;
            var visual=(Power2DSourceTruthRenderer)typeof(CityGridController)
                .GetField("playerVisual",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
            var snapshot=new HouseSnapshot {
                stage=stage, occupancy=city.TestHarness_BuildingOccupancy(0).ToString(),
                playerClip=city.TestHarness_PlayerPrimaryClipName, playerFrame=visual.primary.Frame,
                doorFrame=city.TestHarness_BuildingDoorFrame(0),doorPlaying=city.TestHarness_BuildingDoorPlaying(0),
                hidden=(bool)typeof(CityGridController).GetField("playerHiddenForEntry",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city),
                suspended=city.TestHarness_PlayerControlSuspended,playerX=city.TestHarness_PlayerX,
                weapon=captureFHouseTier>=0?"F"+(captureFHouseTier+1):"unspecified"
            };
            foreach(var sprite in visual.GetComponentsInChildren<SpriteRenderer>())
                if(sprite.enabled && sprite.sprite!=null)snapshot.enabledSpriteNames.Add(sprite.sprite.name);
            houseSnapshots.stages.Add(snapshot);
            yield return Save(stage);
            Time.timeScale=1f;
            Power2DSourceTruthRenderer.GlobalPaused=wasPaused;
        }

        IEnumerator CaptureHouse(CityGridController city)
        {
            var visual=(Power2DSourceTruthRenderer)typeof(CityGridController)
                .GetField("playerVisual",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
            city.TestHarness_SetPlayerNear(0);
            city.TestHarness_PressDoor();
            yield return WaitHouse(()=>city.TestHarness_BuildingOccupancy(0)==CityGridController.BuildingOccupancy.Entering
                && visual.primary.Frame>=4,"house_enter_frame4");
            if(houseFailure!=null)yield break;
            yield return SaveHouse(city,"house_enter_frame4");
            yield return WaitHouse(()=>city.TestHarness_BuildingOccupancy(0)==CityGridController.BuildingOccupancy.Inside
                && !city.TestHarness_BuildingDoorPlaying(0),"house_inside");
            if(houseFailure!=null)yield break;
            yield return SaveHouse(city,"house_inside");
            city.TestHarness_PressDoor();
            yield return SaveHouse(city,"house_exit_before_event");
            yield return WaitHouse(()=>city.TestHarness_BuildingOccupancy(0)==CityGridController.BuildingOccupancy.Exiting
                && visual.primary.Clip!=null && visual.primary.Frame>=4,"house_exit_frame4");
            if(houseFailure!=null)yield break;
            yield return SaveHouse(city,"house_exit_frame4");
            yield return WaitHouse(()=>!city.TestHarness_BuildingDoorPlaying(0)
                && city.TestHarness_BuildingOccupancy(0)==CityGridController.BuildingOccupancy.Exiting,"house_door_closed_player_active");
            if(houseFailure!=null)yield break;
            yield return SaveHouse(city,"house_door_closed_player_active");
            yield return WaitHouse(()=>city.TestHarness_BuildingOccupancy(0)==CityGridController.BuildingOccupancy.Outside,"house_outside");
            if(houseFailure!=null)yield break;
            yield return SaveHouse(city,"house_outside");
        }
    }
    public sealed class DC1DiagnosticPcm : MonoBehaviour {
        readonly object sync=new object();
        readonly List<float> samples=new List<float>();
        int channels=2;
        void OnAudioFilterRead(float[] data,int count) {
            lock(sync) {
                channels=count;
                if(samples.Count<48000*count*60)samples.AddRange(data);
            }
        }
        public void Write(string path) {
            lock(sync) {
                using(var writer=new BinaryWriter(File.Create(path))) {
                    int bytes=samples.Count*2,rate=AudioSettings.outputSampleRate;
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+bytes);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);
                    writer.Write((short)1);writer.Write((short)channels);writer.Write(rate);
                    writer.Write(rate*channels*2);writer.Write((short)(channels*2));writer.Write((short)16);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(bytes);
                    foreach(float sample in samples)writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample,-1f,1f)*32767f));
                }
            }
        }
    }

}
#endif
