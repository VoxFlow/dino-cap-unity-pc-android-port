using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoCap.PortV2
{
    // TASK027N.2.51 LARGE CONTINUATION Section 16 - the Main Menu / boot /
    // City Grind entry reconstruction. First real implementation pass,
    // built directly on this task's own full decompile of DinoCap::
    // menuSceneiPad (sub_59DEC = scene construction, sub_5D380 = the
    // complete event dispatcher) - see
    // Dev/Reports/TASK027N_2_51_CityGrindWholeModeClosure/mainmenu_state_machine_contract.json
    // for the full evidence trail every constant/behavior below cites.
    //
    // ARCHITECTURE: same OnGUI-driven, code-built-world convention as
    // CityGridController/CombatSandboxV2 (no pre-placed scene content,
    // everything constructed in Start()) - for consistency with the rest
    // of this project, not because native itself used OnGUI (it used
    // Triniti2D's own UIManager/UIControl framework, already proven to be
    // functionally equivalent to a standard immediate-mode button system
    // for this port's own purposes elsewhere in this codebase).
    //
    // PROVEN vs PLATFORM_ADAPTER disclosure (CLAUDE.md 11.2): the 5
    // level-select widget rects below are LIKELY, not fully PROVEN - raw
    // ARM lane-disambiguation of sub_5EEEC was not completed this pass
    // (see the contract file's own InitFunc_11_RAW_LITERALS_FOUND), but
    // their own WIDTH/HEIGHT components independently cross-validate
    // exactly against the real exported asset pixel dimensions (e.g.
    // level_leftFrame.png is literally 67x90, matching pair_B's own
    // decoded size exactly) - a strong corroborating signal even without
    // full ARM verification. The primary-screen button positions (New
    // Game/Resume/Options/Help layout) have NO equivalent InitFunc_11-style
    // literal data recovered yet - these are PLATFORM_ADAPTER layout
    // choices based on the user's own runtime testimony ("two bottom-left
    // buttons + ? bottom-right"), explicitly NOT claimed as pixel-exact
    // native positions.
    public sealed class MainMenuController : MonoBehaviour
    {
        // TASK027N.2.51 LARGE CONTINUATION Section 16.1 (boot/launch
        // sequence). PROVEN: the original IPA ships 7 named launch-image
        // variants (Default.png family); dimension-matching this port's
        // own established iPad logical resolution (DinoCapSourceProfileV11.
        // ViewWidth/ViewHeight=1024x768) identifies Default-Landscape.png
        // (literally 1024x768, confirmed via direct PNG header read) as the
        // iPad-landscape-relevant one - its own content is the "Triniti
        // Interactive" developer/publisher splash (the studio behind the
        // Triniti2D engine this whole archaeology has been citing), not a
        // Dino-Cap-specific image. This is a STATIC iOS launch image shown
        // by the OS itself before any app code runs - not something the
        // game's own UI framework displays via a tracked call - so unlike
        // every other asset in this file, there is no native "duration"
        // constant to recover; real iOS shows it until the app's own first
        // frame renders. BootSplashSeconds is therefore an explicit
        // PLATFORM_ADAPTER choice (a short, fixed duration standing in for
        // "until ready"), not a guessed native value.
        bool bootSplashActive = true;
        float bootSplashTimer;
        const float BootSplashSeconds = 1.0f;
        static bool initialApplicationScene=true;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeginApplicationBoot()
        {
            initialApplicationScene=true;
            SceneManager.sceneLoaded-=FinishApplicationBoot;
            SceneManager.sceneLoaded+=FinishApplicationBoot;
        }
        static void FinishApplicationBoot(Scene scene,LoadSceneMode mode)
        {
            initialApplicationScene=false;
            SceneManager.sceneLoaded-=FinishApplicationBoot;
        }

        // UI presentation branches; these enum numbers are host values,
        // distinct from the native selection-target field in5D380.
        internal enum MenuScreen { NewGame, Resume, Launching, LifeMode, Options, LevelSelect, LevelSelectAlt, LifeModeActive }
        MenuScreen screen = MenuScreen.NewGame;
        bool launchingJungle;

        // TASK027N.2.51: dword_214FC0, PROVEN 3-tier cycle (sub_5FD70) -
        // reuses CityGridController's own cityDifficulty persistence key
        // (DifficultyPrefKey) so a value set here is read by CityGridController's
        // own LoadProgressionState() when the player presses GO.
        int difficulty = 1;
        // byte_214FB8 (Music, PROVEN already shared with City Grid's own
        // pause menu) / byte_214FBC (SFX, a separate flag, PROVEN this
        // session via sub_604B0).
        bool musicOn = true;
        bool sfxOn = true;

        int levelSelectValue = 1;
        int maxLevelReached = 1;

        Rect gameViewRect;
        string status = "";
        bool helpVisible;

        // CITYFrame/JUNGLEFrame banner (PROVEN this session: shown on
        // NewGameEnter/LifeModeEnter respectively via sub_61D40/sub_622E8,
        // each with the SAME overshoot-tween treatment as every other
        // widget this session characterized). Transient - shown briefly on
        // entering New Game or Life Mode, matching the user's own "City
        // Grind appears with overshoot scale" testimony.
        float bannerTimer; // Compatibility observability only; native controls own their lifetime.
        const float BannerShowSeconds = 1.2f;   // PLATFORM_ADAPTER duration - the overshoot tween ITSELF is proven ~0.2-0.3s (sub_DA87C's own 2 trailing float args), but how long the banner then LINGERS before the screen settles was not independently proven this pass.
        string bannerFrame;

        // A real cache (this used to call Resources.Load on every draw). QoL 2026-10-13: first-use lag in the menu came from
        // disk reads, decoding and GPU uploads of menu textures and the click sound happening on first use (Options slide-in).
        static readonly Dictionary<string,Texture2D> textureCache=new Dictionary<string,Texture2D>();
        static bool menuAssetsPreloaded;
        static Texture2D CachedTexture(string name)
        {
            if(name=="helpFrame")name="help_tutorialFrame";
            if(!textureCache.TryGetValue(name,out var tex) || tex==null)
            {
                tex=Resources.Load<Texture2D>("DinoCapGenerated/MainMenu/"+name);
                textureCache[name]=tex;
            }
            return tex;
        }

        bool gameplayPreloadStarted;
        // Strong references keep the preloaded gameplay assets resident across the scene change (native keeps its textures loaded),
        // so Unity's automatic unload of unused assets on scene load does not throw the work away.
        static readonly List<Object> gameplayAssetsKeepAlive=new List<Object>();
        static bool cityAssetsPreloaded,jungleAssetsPreloaded,gameplayAudioPreloaded;
        IEnumerator PreloadGameplayThenLaunch()
        {
            bool needCity=!cityAssetsPreloaded;                       // Jungle Blitz also draws the City Grind sprites
            bool needJungle=launchingJungle && !jungleAssetsPreloaded;
            if(needCity || needJungle || !gameplayAudioPreloaded)
            {
                var sw=System.Diagnostics.Stopwatch.StartNew();
                var textures=new List<Texture2D>();
                if(needCity) textures.AddRange(Resources.LoadAll<Texture2D>("DinoCapGenerated/CityGrind"));
                if(needJungle) textures.AddRange(Resources.LoadAll<Texture2D>("DinoCapGenerated/JungleBlitz"));
                var rt=RenderTexture.GetTemporary(4,4,0);
                var previous=RenderTexture.active;
                int n=0;
                foreach(var tex in textures)
                {
                    if(tex==null) continue;
                    gameplayAssetsKeepAlive.Add(tex);
                    Graphics.Blit(tex,rt);                 // forces the GPU upload now, behind "Loading..."
                    if(++n%4==0) yield return null;
                }
                RenderTexture.active=previous;
                RenderTexture.ReleaseTemporary(rt);
                int clipCount=0;
                if(!gameplayAudioPreloaded)
                    foreach(var clip in Resources.LoadAll<AudioClip>("Audio"))
                    {
                        if(clip==null || clip.name.StartsWith("Music_")) continue;   // streamed music is not a first-use hitch
                        gameplayAssetsKeepAlive.Add(clip);
                        clip.LoadAudioData();
                        clipCount++;
                    }
                cityAssetsPreloaded|=needCity; jungleAssetsPreloaded|=needJungle; gameplayAudioPreloaded=true;
                Debug.Log("[DC1-PLATFORM] gameplay preload "+n+" textures + "+clipCount+" clips in "+sw.ElapsedMilliseconds+" ms");
            }
            gameplayLoadPending=false;
            gameplayPreloadStarted=false;
            if(launchingJungle)SceneManager.LoadScene("JungleBlitzScene");
            else PressResume();
        }

        // Preload (PLATFORM_ADAPTER QoL, no behavior change): load every menu texture and the menu click sound once, force the
        // GPU upload with a tiny blit, a few per frame so the menu reveal never hitches. Runs once per process, starting with
        // the first menu scene; the original "Loading..." sequence and all native timings are untouched.
        IEnumerator PreloadMenuAssets()
        {
            if(menuAssetsPreloaded) yield break;   // set only on completion: a preload interrupted by a scene reload restarts
            // Let the boot logo be drawn and presented first: Resources.LoadAll below is synchronous and would otherwise hold the very first frame
            // (the logo) back until all 61 textures are loaded - that was the grey screen at launch.
            yield return null; yield return null;
            var sw=System.Diagnostics.Stopwatch.StartNew();
            var list=new List<Texture2D>(Resources.LoadAll<Texture2D>("DinoCapGenerated/MainMenu"));
            Debug.Log("[DC1-PLATFORM] menu LoadAll "+list.Count+" textures in "+sw.ElapsedMilliseconds+" ms (after the logo frames)");
            var cityDigits=new List<Texture2D>();
            var title=Resources.Load<Texture2D>("DinoCapGenerated/CityGrind/levelFrame"); if(title!=null)cityDigits.Add(title);
            for(int d=0;d<=9;d++){var t=Resources.Load<Texture2D>("DinoCapGenerated/CityGrind/level"+d+"Frame"); if(t!=null)cityDigits.Add(t);}
            var rt=RenderTexture.GetTemporary(4,4,0);
            var previous=RenderTexture.active;
            int n=0;
            for(int pass=0;pass<2;pass++)
            {
                foreach(var tex in pass==0 ? list : cityDigits)
                {
                    if(tex==null) continue;
                    if(pass==0) textureCache[tex.name]=tex;   // menu textures only; the level-select digits are loaded from CityGrind
                    Graphics.Blit(tex,rt);                    // forces the texture's GPU upload now
                    if(++n%2==0) yield return null;
                }
            }
            RenderTexture.active=previous;
            RenderTexture.ReleaseTemporary(rt);
            var click=Resources.Load<AudioClip>("Audio/Audio_button");
            if(click!=null) click.LoadAudioData();   // decode now, not on the first button press
            menuAssetsPreloaded=true;
            Debug.Log("[DC1-PLATFORM] menu preload "+n+" textures + click sound in "+sw.ElapsedMilliseconds+" ms");
        }
        static Sprite LoadMenuSprite(string frame)
        {
            var tex = CachedTexture(frame);
            if(tex==null) return null;
            return Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f,0.5f), 100f, 0, SpriteMeshType.FullRect);
        }

        void Awake()
        {
            // TASK027N.2.51: PROVEN native default boot screen - sub_5D064
            // (which fires the 'NewGameEnter' named event, PROVEN sole
            // firing site via exhaustive string search) is called from the
            // END of sub_59DEC (Main Menu scene construction) - the menu
            // boots DIRECTLY into the New Game screen, not a neutral/blank
            // state.
            screen = MenuScreen.NewGame;
            musicOn = !NativeAudioSessionGate.LoadMusicPreference();
            sfxOn = !NativeAudioSessionGate.LoadFxPreference();
            difficulty = Mathf.Clamp(DC1Preferences.GetInt(CityGridController.DifficultyPrefKey, 1), 1, 3);
            levelSelectValue = Mathf.Max(1, DC1Preferences.GetInt("DC1_CityGrind_Level", 1));
            maxLevelReached = Mathf.Max(levelSelectValue, DC1Preferences.GetInt("DC1_CityGrind_MaxLevel", 1));
            ResetMenuReveal();
            StartCoroutine(PreloadMenuAssets());
            // Default-Landscape is an OS application launch image. A return
            // from gameplay loads menuSceneiPad directly, without another boot.
            bootSplashActive = initialApplicationScene;
            bootSplashTimer = BootSplashSeconds;
            EnsureBackgroundDinoVisual();
            dinoVisual.gameObject.SetActive(false);
            EnsureMenuPresentation();
            if(!bootSplashActive)EnsureMenuMusic();
            var sandboxEntry=SandboxModeSession.ConsumeEntry();
            if(sandboxEntry==SandboxModeSession.Entry.City)GoLevelSelect();
            else if(sandboxEntry==SandboxModeSession.Entry.Jungle)BeginJungleSelection();
        }

        AudioSource menuMusic;
        internal AudioSource TestHarness_MenuMusicSource => menuMusic;
        void EnsureMenuMusic()
        {
            if(menuMusic!=null)return;
            // Native59DEC: Music_menuAudio, loop1, channel menu_bgAudio.
            // Own the source with the scene, separate from capped FX voices.
            var clip=Resources.Load<AudioClip>("Audio/Music_menu");
            if(clip==null)return;
            var owner=new GameObject("MainMenuBgm");
            owner.transform.SetParent(transform,false);
            menuMusic=owner.AddComponent<AudioSource>();
            menuMusic.playOnAwake=false;
            menuMusic.clip=clip;
            menuMusic.loop=true;
            menuMusic.spatialBlend=0f;
            menuMusic.mute=!musicOn;
            menuMusic.Play();
        }

        // TASK027N.2.51 LARGE CONTINUATION 3 (Main Menu background dino -
        // full closure). Fresh IDA session 455561b7 traced the COMPLETE
        // native mechanism beyond the LARGE CONTINUATION 2 pass's own
        // disclosed gap:
        //  - sub_627CC's own created object is a GENERIC Triniti2D::
        //    GameObject (sub_17C720/sub_171E30, RTTI 'N9Triniti2D
        //    10GameObjectE') - 'menuObjiPad' is a string TAG, not a
        //    distinct class. Its own behavior is driven by an attached
        //    DinoCap::CAObjCallBack (vtable 0x1fe020, RTTI-confirmed).
        //  - CAObjCallBack's own Tick (vtable slot 6, sub_76A20, decompiled
        //    in full) is a GENERIC MULTI-PART SKELETAL ANIMATOR: a
        //    std::vector<GameObject*> of child parts, each looked up by
        //    name in a std::map<string,CASprite*>, each driving its own
        //    child DinoCap::CAObjSpriteCallBack's position/rotation/color/
        //    frame - PROVEN to be the SAME native mechanism this project's
        //    own dino_part_articulation_contract.json already maps to
        //    Unity's Power2DSourceTruthRenderer/DCPart/dBits pipeline
        //    (same part-vector-push_back pattern in sub_7677C, same
        //    CAObjSpriteCallBack child type) - CONFIRMING reuse is
        //    source-correct, not merely plausible.
        //  - sub_7677C ('SetAnimation', 67 call sites project-wide
        //    including sub_10554, the already-known dino hit-reaction
        //    dispatcher) takes a body-part-TABLE POINTER as its 2nd arg,
        //    reading a required-part-count from table+12 - the menu's own
        //    2 'variant' pointers (offsets 44/20 within the Main Menu
        //    scene's own C++ object) are 2 DIFFERENT body-part tables,
        //    PROVING variant selection = DINOSAUR TYPE (different
        //    skeleton), not skin/scale/direction (answers Section 3.E).
        //    Direct5D00C evidence supersedes the older inert-container claim:
        //    it loads k3_move atscene+20 andk2_move atscene+44.59DEC calls
        //    this initializer before starting the menu. Both tables are live.
        //  - PROVEN MOVEMENT SPEED: off_1FC238/off_1FC240 (the ORIGINAL
        //    LARGE CONTINUATION 2 lead) is a SEPARATE, unrelated lookup -
        //    both branches' own first element resolves to a 24-byte POD
        //    struct (sub_E9628, 3 near-identical global instances) with
        //    HARDCODED COMPILE-TIME CONSTANTS, IDENTICAL for both branches:
        //    field+4=20.0, field+12=999.0, field+20=20.0. This feeds
        //    DIRECTLY into sub_173A98(GameObject::GetPhysics(), {20.0,
        //    0.0}) - sub_172D50 is confirmed literally GameObject::
        //    GetPhysics() via its own assert string ('m_physics != NULL',
        //    GameObject.cpp:475). PROVEN horizontal velocity = 20.0 units/
        //    sec, IDENTICAL for both variants - REPLACES the prior
        //    guessed DinoWalkSpeedPlatformAdapter=60 (disarmed, see
        //    Dev/SourceTruth/disarmed_heuristics.json).
        //  - PROVEN SCALE: no scale-setter call exists anywhere in
        //    sub_627CC's own body - scale is native default 1.0 (matching
        //    this project's own established 'no scale call = scale 1.0'
        //    convention). NO randomized/variant scale exists (answers
        //    Section 7: fixed 1.0 for both variants, not a size class).
        //  - N2.51 continuation supersedes the old silhouette absence claim:
        //    627CC writes callback+88=0;76A20 multiplies authored RGB by it.
        //    5D00C loads scene+20=k3_move and scene+44=k2_move;627CC
        //    selects these exact containers for variants0 and1 respectively.
        // VISUAL NOW IMPLEMENTED: reuses Power2DSourceTruthRenderer (the
        // SAME component CityGridController/CombatSandboxV2 use for every
        // other dino in this project) via a dedicated orthographic
        // background camera (MainMenuScene previously had none - OnGUI
        // draws over whatever a camera renders, giving the correct
        // dino-behind-UI layering for free).
        float dinoLogicalX;
        float dinoLogicalY;
        int dinoVariant;
        bool dinoRoarPlayed;
        bool dinoActive;
        float dinoCreationTimer;
        const float DinoCreationInterval = 5f; // 5D00C CreateDino repeating named event.
        const float DinoWorldStartX = -672f;
        const float DinoWorldStartY = -30f;
        const float DinoWorldDeactivateX = 768f; // menuObjiPad::Tick,59A54 ARM strict greater-than.
        // PROVEN native horizontal velocity (sub_173A98 call on
        // GameObject::GetPhysics(), fed from a hardcoded 20.0f constant
        // identical for both variants - see header comment above).
        const float DinoVelocityUnitsPerSecond = 20f;
        static readonly string[] DinoVariantClipPrefix = { "k3", "k2" }; // Native5D00C/627CC.
        DCManifest dinoManifest;
        Camera dinoCamera;
        Power2DSourceTruthRenderer dinoVisual;
        RenderTexture menuDinoTarget;
        int menuLogicalWidth=1024;
        void InitBackgroundDino()
        {
            dinoLogicalX = DinoCapSourceProfileV11.ViewWidth*0.5f + DinoWorldStartX;
            dinoLogicalY = DinoCapSourceProfileV11.ViewHeight*0.5f - DinoWorldStartY;
            dinoVariant = UnityEngine.Random.value*100f >= 50f ? 1 : 0;
            dinoRoarPlayed = false;
            EnsureBackgroundDinoVisual();
            dinoActive=true;
            dinoVisual.gameObject.SetActive(true);
            if(dinoVisual!=null)
            {
                // PROVEN: this project's own already-established k1/k3 art-
                // polarity inversion (CombatSandboxV2.ApplyEnemyVisualFacing
                // / CityGridController's own SpawnCityDino citation) - the
                // imported art's own facingRight is opposite the logical
                // walk direction for this shared pipeline.
                dinoVisual.facingRight = false;   // walks rightward (logical), so visual facingRight is inverted per the proven k-family polarity
                string prefix = DinoVariantClipPrefix[dinoVariant];
                // 627CC -> 76980(...,-1,0,.025,-1): infinite actor playback,
                // including k3 whose shared asset is authored non-looping.
                dinoVisual.primary.PlayLooping(dinoVisual.FindClip(prefix+"_move"), 0);
            }
            SyncBackgroundDinoWorldPosition();
            // 627CC emits Audio_Dino2Audio on activation, even off-screen.
            dinoRoarPlayed=true;
            var roar=Resources.Load<AudioClip>("Audio/Audio_Dino2");
            if(roar!=null && sfxOn) {
                var src=GetComponent<AudioSource>();
                if(src==null)src=gameObject.AddComponent<AudioSource>();
                src.PlayOneShot(roar);
            }
        }
        void EnsureBackgroundDinoVisual()
        {
            if(dinoManifest==null) dinoManifest = SourceTruthLoader.Load();
            if(dinoCamera==null)
            {
                var camGo = new GameObject("MainMenuBackgroundDinoCamera");
                camGo.transform.SetParent(transform,false);
                dinoCamera = camGo.AddComponent<Camera>();
                dinoCamera.orthographic = true;
                dinoCamera.orthographicSize = DinoCapSourceProfileV11.ViewHeight*0.5f;
                dinoCamera.transform.position = new Vector3(0f,0f,-10f);
                dinoCamera.clearFlags = CameraClearFlags.SolidColor;
                dinoCamera.backgroundColor = Color.clear;
                dinoCamera.depth = -10;
            }
            if(dinoVisual==null)
            {
                var dinoGo = new GameObject("MainMenuBackgroundDino");
                dinoGo.transform.SetParent(transform,false);
                dinoVisual = dinoGo.AddComponent<Power2DSourceTruthRenderer>();
                dinoVisual.baseSortingOrder = 50;
                dinoVisual.Initialize(dinoManifest);
                dinoVisual.primary.EnterFrame += OnMenuDinoFrame;
                // Native627CC callback+88=0;76A20 multiplies RGB only.
                dinoVisual.actorColourScale=0f;
            }
        }
        void OnMenuDinoFrame(Power2DTrack track,int frame)
        {
            // 5D00C flags k2_move frames40/80 via78BC8.76A20 dispatches
            // those flagged frames to menuObjiPad vtable+2C (59AB0), which
            // requests Audio_DinoAudio. The k3_move clip has no such flags.
            if(!dinoActive || track.Clip==null || track.Clip.name!="k2_move" ||
                (frame!=40 && frame!=80) || !sfxOn) return;
            var clip=Resources.Load<AudioClip>("Audio/Audio_Dino");
            if(clip==null)return;
            var source=GetComponent<AudioSource>();
            if(source==null)source=gameObject.AddComponent<AudioSource>();
            NativeAudioSessionGate.Play(source,clip);
        }
        void SyncBackgroundDinoWorldPosition()
        {
            if(dinoVisual==null) return;
            float worldX = dinoLogicalX - DinoCapSourceProfileV11.ViewWidth*0.5f;
            float worldY = DinoCapSourceProfileV11.ViewHeight*0.5f - dinoLogicalY;
            dinoVisual.transform.position = new Vector3(worldX, worldY, 0f);
        }
        void UpdateBackgroundDino()
        {
            StepBackgroundDino(Time.deltaTime);
        }
        void StepBackgroundDino(float dt)
        {
            if(!dinoActive)return;
            dinoLogicalX += DinoVelocityUnitsPerSecond*dt;
            SyncBackgroundDinoWorldPosition();
            // C0CE4 registers menuObjiPad->599A0, whose vtable1FDB38
            // slot+24 overrides the generic animator tick with59A54.
            // It advances animation then deactivates only when root X>768.
            float worldX=dinoLogicalX-DinoCapSourceProfileV11.ViewWidth*0.5f;
            if(worldX > DinoWorldDeactivateX)
            {
                // Cached actor reactivation is owned by the next native5s pulse.
                dinoActive=false;
                dinoVisual.gameObject.SetActive(false);
            }
        }
        void TickDinoCreation(float dt)
        {
            dinoCreationTimer+=dt;
            if(dinoCreationTimer<DinoCreationInterval)return;
            dinoCreationTimer-=DinoCreationInterval;
            // 627CC reuses its cached GameObject and returns if already active.
            if(!dinoActive)InitBackgroundDino();
        }
        internal void TestHarness_StepDinoCreation(float dt)=>TickDinoCreation(dt);
        internal bool TestHarness_DinoActive=>dinoActive;
        internal float TestHarness_DinoCreationElapsed=>dinoCreationTimer;
        internal float TestHarness_DinoLogicalX => dinoLogicalX;
        internal float TestHarness_DinoLogicalY => dinoLogicalY;
        internal int TestHarness_DinoVariant => dinoVariant;
        internal bool TestHarness_DinoRoarPlayed => dinoRoarPlayed;
        internal void TestHarness_ForceDinoNearZero(){ dinoLogicalX = 1f; }
        internal Power2DSourceTruthRenderer TestHarness_DinoVisual => dinoVisual;
        internal Camera TestHarness_DinoCamera => dinoCamera;
        internal void TestHarness_StepBackgroundDino(float dt)=>StepBackgroundDino(dt);

        void ShowBanner(string frame)
        {
            bannerFrame = frame;
            bannerTimer = BannerShowSeconds;
        }

        bool optionsFirstDrawLogged;
        void Update()
        {
            if(Time.unscaledDeltaTime>0.06f && Time.unscaledTime>3f) Debug.Log("[DC1-HITCH] frame "+Mathf.RoundToInt(Time.unscaledDeltaTime*1000f)+" ms screen="+screen+" leaving="+(primaryLeave!=null)+" options="+(optionsTweens!=null));
            // Native62A98 consumes state4 after control13 completion.
            if(gameplayLoadPending) {
                // Native: the "Loading..." slide-in finishes (control 13), the menu state becomes 4 and the scene-load function
                // (sub_62A98 -> sub_2A8A8 / sub_D05AC -> sub_A1518) then loads ALL of the gameplay scene's atlas frames synchronously
                // before the scene appears. The port mirrors that point: the gameplay assets are loaded and GPU-uploaded here,
                // while "Loading..." stays on screen, and only then does the scene start (PLATFORM_ADAPTER; the native blocks one
                // long frame, this spreads the same work over a few frames so the slide-in image keeps animating).
                if(!gameplayPreloadStarted){ gameplayPreloadStarted=true; StartCoroutine(PreloadGameplayThenLaunch()); }
                return;
            }
            EnsureMenuPresentation();
            HandleBackKey();
            TickDinoCreation(Time.deltaTime);
            if(bannerTimer>0f) bannerTimer -= Time.deltaTime;
            if(bootSplashActive)
            {
                bootSplashTimer -= Time.deltaTime;
                if(bootSplashTimer<=0f) bootSplashActive = false;
            }
            else
            {
                EnsureMenuMusic();
                TickMenuReveal(Time.deltaTime);
                UpdateBackgroundDino();
            }
        }
        // PLATFORM_ADAPTER QoL (user-approved 2026-10-13): Android Back / Esc maps to the screen's own BACK control.
        // Ignored during the boot splash and during the "Loading..." launch sequence (the original cannot be cancelled there)
        // and on the main menu itself (left to the OS).
        bool BackKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var k=UnityEngine.InputSystem.Keyboard.current;
            return k!=null && k.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
        void HandleBackKey()
        {
            if(!BackKeyPressed() || bootSplashActive || screen==MenuScreen.Launching || primaryLeave!=null) return;
            if(DC1DebugTextInput.ConsumeBack()) return;   // an open keyboard takes the Back press first
            if(helpVisible){ helpVisible=false; return; }
            if(screen==MenuScreen.Options) BackFromOptions();
            else if(screen==MenuScreen.LevelSelect) BackFromLevelSelect();
        }
        internal void TestHarness_SkipBootSplash(){ bootSplashActive=false; }
        internal bool TestHarness_BootSplashActive => bootSplashActive;

        // ---------------------------------------------------------------
        // Navigation - mirrors sub_5D380's own named-event Enter/Leave
        // pairs (sub_61AD4) for the 4 top-level screens, and the numeric
        // dispatcher cases for level-select/difficulty/audio/GO/BACK.
        // ---------------------------------------------------------------
        void GoNewGame(){ launchingJungle=false;screen=MenuScreen.NewGame; optionsTweens=null; ResetMenuReveal(); }
        void GoResume(){ screen=MenuScreen.Resume; }
        void GoOptions()
        {
            var swGo=System.Diagnostics.Stopwatch.StartNew();
            primaryLeave=null;
            screen=MenuScreen.Options;optionsLeaving=false;
            optionsTweens=new NativeUiRectTween[8];
            for(int i=0;i<8;i++) {
                Vector4 end=OptionsRest(i);
                optionsTweens[i]=new NativeUiRectTween(ShiftX(end,1024),ShiftX(end,-102),end,0.2f,0.05f);
            }
            LogStep("GoOptions",swGo);
        }
        void BeginOptionsLeave()
        {
            primaryLeave=new NativeMenuPrimaryLeave();
            primaryLeaveComplete=GoOptions;
        }
        void BeginResumeLeave()
        {
            launchingJungle=false;
            // 61438 and614EC share the same primary leave tuples/events.
            primaryLeave=new NativeMenuPrimaryLeave();
            primaryLeaveComplete=BeginGameplayLoading;
        }
        void BeginGameplayLoading()
        {
            screen=MenuScreen.Launching;
            // InitFunc11/63A08: loading top-left828,636,196x32.
            // 5DF2C: fromX+1024 ->X-102 ->rest, .2/.05.
            var rest=new Vector4(926,652,196,32);
            loadingTween=new NativeUiRectTween(ShiftX(rest,1024),ShiftX(rest,-102),rest,0.2f,0.05f);
        }
        void GoLifeMode()
        {
            //5D380 state8 ->61588 schedules the four primary leaves;
            //LifeModeLeave control26 completion starts5DF2C loading.
            //Unlike Resume, wait for the survival control (index4).
            launchingJungle=true;NativeModeSession.Read();
            primaryLeave=new NativeMenuPrimaryLeave(4);
            if(jungleTitleLeave!=null)primaryLeave.Controls[0]=jungleTitleLeave;
            primaryLeaveComplete=BeginGameplayLoading;
        }
        void GoLevelSelect()
        {
            screen=MenuScreen.LevelSelect;
            levelPresentation=new NativeMenuLevelSelect();
            levelLeaveComplete=null;
            // 5EEEC initializes UILevelCtrliPad to1 and scene+8 to1.
            // Saved current level belongs to RESUME, not the checkpoint dial.
            levelSelectValue = 1;
        }
        void BackFromLevelSelect()
        {
            if(levelPresentation!=null && !levelPresentation.Interactive) return;
            screen=MenuScreen.NewGame;
            BeginLevelSelectLeave(()=> {
                // 5F5BC restores footer controls and enables selection ID3.
                selectionTween=null; selectionComplete=null; pressedWidget=-1;
            });
        }
        void BeginLevelSelectLeave(System.Action complete)
        {
            if(levelPresentation==null) { complete?.Invoke(); return; }
            if(!levelPresentation.Interactive) return;
            levelLeaveComplete=complete;
            levelPresentation.BeginLeave();
        }

        // TASK027N.2.51: PROVEN exact native level-select step (full
        // decompile of sub_5FBA8/sub_5FC00, the "-5"/"+5" handlers named by
        // the LARGE prompt itself) - NOT a simple +-1, and NOT a simple
        // +-5 either. A genuine SHIPPED QUIRK (CLAUDE.md 4.11, preserved
        // exactly, not "fixed"):
        //  DOWN: if already at 1, no-op. Else new = max(1, current-5).
        //  UP: special case current==1 -> jumps DIRECTLY to 5 (not 1+5=6).
        //      Otherwise new = current+5, but ONLY if current is still
        //      BELOW 5*floor(maxLevelReached/5) - i.e. the upper reachable
        //      bound on the DIAL is FLOORED to the nearest multiple of 5
        //      at or below maxLevelReached. Example: if the player has
        //      actually reached level 23, the dial's own max position is
        //      20, not 23 - up to 4 "recently reached" levels are
        //      unreachable via this dial specifically (their own saved
        //      progress - cash/tiers - is unaffected; only this UI's own
        //      selectable value is floored). This is exactly what the
        //      native code does, not an invented limitation.
        internal void TestHarness_LevelDown()
        {
            if(levelSelectValue==1) return;
            levelSelectValue = Mathf.Max(1, levelSelectValue-5);
        }
        internal void TestHarness_LevelUp()
        {
            int ceilingFlooredTo5 = 5*(Mathf.Max(1,maxLevelReached)/5);
            if(levelSelectValue>=ceilingFlooredTo5) return;
            levelSelectValue = levelSelectValue==1 ? 5 : levelSelectValue+5;
        }

        // TASK027N.2.51: case 21 (GO), PROVEN - writes dword_214FB0 (here,
        // the already-established LevelPrefKey) and triggers the CityGrind
        // scene load (sub_7EEC8's own role, already proven in an earlier
        // Task004 pass). Difficulty is also written so CityGridController's
        // own LoadProgressionState() picks it up on the next scene Start().
        void PressGo()
        {
            PrepareGoRun();
            SceneManager.LoadScene("CityGrindScene");
        }
        void PrepareGoRun()
        {
            // sub_7EEC8 resets the run and reconstructs the checkpoint preset.
            CityGrindStartPreset.ForLevel(levelSelectValue).WriteCommittedRun(levelSelectValue);
            if(maxLevelReached<levelSelectValue){ maxLevelReached=levelSelectValue; DC1Preferences.SetInt("DC1_CityGrind_MaxLevel", maxLevelReached); }
            DC1Preferences.SetInt(CityGridController.DifficultyPrefKey, difficulty);
            DC1Preferences.Save();
        }

        // TASK027N.2.51: Resume loads directly - CityGridController's own
        // Start()/LoadProgressionState() ALREADY resumes from whatever
        // level/lives/cash/tiers/difficulty are currently persisted (no
        // separate "resume state" needs to be built here - this is a
        // genuine architectural synergy, not a shortcut: native's own
        // Resume similarly just re-enters the already-persisted save).
        void PressResume()
        {
            DC1Preferences.SetInt(CityGridController.DifficultyPrefKey, difficulty);
            DC1Preferences.Save();
            SceneManager.LoadScene("CityGrindScene");
        }

        void PressDifficultyCycle()
        {
            // TASK027N.2.51: PROVEN exact native wrap (sub_5FD70): 1->2->3->1.
            difficulty = difficulty>=3 ? 1 : difficulty+1;
            DC1Preferences.SetInt(CityGridController.DifficultyPrefKey, difficulty);
        }
        void PressMusicToggle()
        {
            musicOn = !musicOn;
            NativeAudioSessionGate.SaveMusicPreference(!musicOn);
            if(menuMusic!=null)menuMusic.mute=!musicOn;
        }
        void PressSfxToggle()
        {
            sfxOn = !sfxOn;
            NativeAudioSessionGate.SaveFxPreference(!sfxOn);
        }

        // Game Center leaderboard/achievements and the trinitigame.com
        // support link (cases 11/12/23, all PROVEN this session) are
        // SOURCE_FAITHFUL-shipped features with no working modern backend
        // (OpenFeint is defunct; a bespoke Game-Center-equivalent singleton
        // was native-only) - PLATFORM_ADAPTER: present as inert/disabled
        // buttons, matching the same pattern already established for the
        // Shop's own IAP buttons (Section 15.I) rather than silently
        // omitted or fake-wired.
        internal bool TestHarness_SocialButtonsAreInert => true;

        string[] DifficultyFrameFor(int d) => d==1 ? new[]{"text_easyFrame"} : d==2 ? new[]{"text_normalFrame"} : new[]{"text_insaneFrame"};

        // ---------------------------------------------------------------
        // Rendering
        // ---------------------------------------------------------------
        Rect ComputeGameViewRect()
        {
            return MenuPresentationRect(Screen.width,Screen.height,GameplayRenderTarget.AspectMode);
        }

        internal static Rect MenuPresentationRect(int width,int height,GameplayRenderTarget.PresentationAspectMode mode)
        {
            int logical=GameplayRenderTarget.ComputeLogicalWidth(1024,768,width,height,mode);
            float scale=Mathf.Min(width/(float)logical,height/768f);
            return new Rect((width-logical*scale)*0.5f,(height-768f*scale)*0.5f,logical*scale,768f*scale);
        }
        void EnsureMenuPresentation()
        {
            menuLogicalWidth=GameplayRenderTarget.ComputeLogicalWidth(1024,768,Screen.width,Screen.height,GameplayRenderTarget.AspectMode);
            int scale=GameplayRenderTarget.InternalScale;
            int width=menuLogicalWidth*scale, height=768*scale;
            if(menuDinoTarget!=null && menuDinoTarget.width==width && menuDinoTarget.height==height) return;
            if(menuDinoTarget!=null) { dinoCamera.targetTexture=null; menuDinoTarget.Release(); Destroy(menuDinoTarget); }
            menuDinoTarget=new RenderTexture(width,height,0,RenderTextureFormat.ARGB32);
            menuDinoTarget.filterMode=FilterMode.Point; menuDinoTarget.useMipMap=false;
            menuDinoTarget.antiAliasing=1; menuDinoTarget.Create();
            dinoCamera.targetTexture=menuDinoTarget;
            dinoCamera.aspect=menuLogicalWidth/768f;
        }
        void OnDestroy()
        {
            NativeFramebufferUi.Release();
            if(menuDinoTarget!=null) { if(dinoCamera!=null)dinoCamera.targetTexture=null; menuDinoTarget.Release(); Destroy(menuDinoTarget); }
        }

        Rect L2S(Rect logical)
        {
            return NativeViewport.Map(logical,gameViewRect);
        }

        void DrawSprite(string frame, Rect logicalRect)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(DC1ConvergenceCapture.TryMenuAtlas(frame,out var atlas,out var uv)) {
                NativeFramebufferUi.DrawTextureWithTexCoords(L2S(logicalRect),atlas,uv,true);
                return;
            }
#endif
            var tex = CachedTexture(frame);
            if(tex!=null) NativeFramebufferUi.DrawTexture(L2S(logicalRect), tex, ScaleMode.StretchToFill);
        }

        // TASK027N.2.51 RUNTIME_TRUTH_RESET: the level-select number was
        // plain GUI.Label text - replaced with the SAME native icon_num0-9
        // Frame digit-sprite family CityGridController.cs's own HUD
        // already uses (DrawNativeNumberSprites, mirrored here), exported
        // to this scene's own Resources folder this round. Centers the
        // whole digit string horizontally around centerX, matching the
        // prior Label's own centered-Rect convention.
        void DrawNumberCentered(float centerX, float logicalY, float height, string digits)
        {
            float totalWidth = 0f;
            foreach(char c in digits)
            {
                if(c<'0' || c>'9') continue;
                var tex = CachedTexture("icon_num"+c+"Frame");
                if(tex==null) continue;
                totalWidth += tex.width * (height/Mathf.Max(1f,tex.height));
            }
            float x = centerX - totalWidth*0.5f;
            foreach(char c in digits)
            {
                if(c<'0' || c>'9') continue;
                var tex = CachedTexture("icon_num"+c+"Frame");
                if(tex==null) continue;
                float dw = tex.width * (height/Mathf.Max(1f,tex.height));
                NativeFramebufferUi.DrawTexture(L2S(new Rect(x, logicalY, dw, height)), tex, ScaleMode.ScaleToFit);
                x += dw;
            }
        }

        bool ClickSprite(string frame, Rect logicalRect)
        {
            var tex = CachedTexture(frame);
            bool wasEnabled=GUI.enabled;
            GUI.enabled=wasEnabled && (!helpVisible || frame=="helpFrame");
            Rect drawn=logicalRect;
            bool clicked=frame=="helpFrame"
                ? GUI.Button(L2S(logicalRect),"",GUIStyle.none)
                : HandleMoveInput(logicalRect,GUI.enabled,out drawn);
            GUI.enabled=wasEnabled;
            if(tex!=null) NativeFramebufferUi.DrawTexture(L2S(drawn), tex, ScaleMode.StretchToFill);
            // DA6D8's successful release emits Audio_buttonAudio before
            // dispatching the control action. Tutorial24 is UIClickButton,
            // not UIMoveCtrl, and its release only dismisses the overlay.
            if(clicked && frame!="helpFrame") PlayMenuClick();
            return clicked;
        }

        readonly System.Collections.Generic.Dictionary<int,NativeMenuMoveInput> moveInputs=new System.Collections.Generic.Dictionary<int,NativeMenuMoveInput>();
        internal bool TestHarness_SuppressRealPointer;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Rect diagnosticMoveRect;
        int diagnosticMovePhase;
        internal void TestHarness_InjectMovePointer(Rect logical,bool down) {
            diagnosticMoveRect=logical;diagnosticMovePhase=down?1:2;
        }
#endif
        bool HandleMoveInput(Rect logical,bool enabled,out Rect drawn)
        {
            int id=GUIUtility.GetControlID(FocusType.Passive);
            if(!moveInputs.TryGetValue(id,out var state)) {state=new NativeMenuMoveInput();moveInputs.Add(id,state);}
            Rect hit=L2S(logical);
            var ev=Event.current;
            bool clicked=false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // The owned capture injects into the same per-control input owner
            // and normal ClickSprite release/audio path, on a real repaint.
            if(diagnosticMovePhase!=0 && ev.type==EventType.Repaint && logical==diagnosticMoveRect) {
                if(diagnosticMovePhase==1 && enabled)state.Down(hit,hit.center);
                else if(diagnosticMovePhase==2)clicked=state.Up() && enabled;
                diagnosticMovePhase=0;
            }
#endif
            if(!TestHarness_SuppressRealPointer && enabled && GUIUtility.hotControl==0 && ev.type==EventType.MouseDown && ev.button==0 && state.Down(hit,ev.mousePosition))
            {GUIUtility.hotControl=id;ev.Use();}
            if(!TestHarness_SuppressRealPointer && GUIUtility.hotControl==id && ev.type==EventType.MouseDrag) {
                state.Drag(ev.mousePosition);
                if(!state.Pressed)GUIUtility.hotControl=0;
                ev.Use();
            }
            if(!TestHarness_SuppressRealPointer && GUIUtility.hotControl==id && ev.type==EventType.MouseUp && ev.button==0) {
                clicked=state.Up() && enabled;GUIUtility.hotControl=0;ev.Use();
            }
            drawn=state.Presentation(logical);
            return clicked;
        }

        // Hitch diagnostics (cheap; logs only when a step takes >12 ms or a frame >60 ms): locates first-use costs in the menu.
        static void LogStep(string what,System.Diagnostics.Stopwatch sw){ if(sw.ElapsedMilliseconds>12) Debug.Log("[DC1-HITCH] "+what+" "+sw.ElapsedMilliseconds+" ms"); }
        void PlayMenuClick()
        {
            var swClick=System.Diagnostics.Stopwatch.StartNew();
            var clip=Resources.Load<AudioClip>("Audio/Audio_button");
            if(clip!=null && sfxOn) {
                // Unity's destroyed/missing-object wrappers need its overloaded
                // null check; CLR ?? can retain a nonfunctional template.
                var source=GetComponent<AudioSource>();
                if(source==null)source=gameObject.AddComponent<AudioSource>();
                NativeAudioSessionGate.Play(source,clip);
            }
            LogStep("PlayMenuClick",swClick);
        }

        // DEBUG_ONLY/PLATFORM_ADAPTER: the port-only Options rows use Unity's default IMGUI font (deliberately unlike the original
        // sprite text). The first IMGUI text use in a session builds the style and rasterizes glyphs (~100+ ms on tablets), which
        // showed as a stall before the first Options slide-in. Measure every string those rows can show once, at boot, so the cost
        // lands behind the logo. CalcSize uses the exact skin/style the rows draw with; nothing is drawn.
        static bool imguiWarmed;
        static void WarmImguiText()
        {
            if(imguiWarmed) return;
            imguiWarmed=true;
            var sw=System.Diagnostics.Stopwatch.StartNew();
            var style=GUI.skin.button;
            string[] onOff={"ON","OFF"};
            var texts=new List<string>{"BACK","COMBAT WORKBENCH","0123456789 .:-()"};
            foreach(var v in onOff){ texts.Add("DEBUG TOOLS: "+v); texts.Add("DEBUG AFFECTS SAVES: "+v); texts.Add("V-SYNC: "+v); }
            foreach(var l in new[]{"UNLIMITED","30","45","60","72","90","120","144","240"}){
                texts.Add("FRAME LIMIT: "+l); texts.Add("FRAME LIMIT: "+l+" (V-SYNC ON: SCREEN RATE)"); }
            foreach(var t in texts) style.CalcSize(new GUIContent(t));
            GUI.skin.label.CalcSize(new GUIContent("Life Mode - not yet implemented in this port."));
            Debug.Log("[DC1-PLATFORM] IMGUI text warmup "+sw.ElapsedMilliseconds+" ms");
        }

        // The warm-up blocks for ~0.1-0.8 s (the first IMGUI text use). It must run only AFTER the boot logo has been rendered and presented, or
        // the logo cannot appear until it finishes and the player sees a grey screen. Two Repaint passes have already reached the screen by the
        // third one, so the logo is visible while this runs.
        static float firstMenuRepaintTime=-1f;
        void OnGUI()
        {
            if(Event.current.type==EventType.Repaint)
            {
                // Frames 1-3 happen within ~70 ms of each other at startup, so a frame count is not "the logo has reached the screen". Wait until the
                // first menu frame (the boot logo) has been on screen for 0.3 s of real time, then run the one-time IMGUI warm-up behind it.
                float now=Time.realtimeSinceStartup;
                if(firstMenuRepaintTime<0f) firstMenuRepaintTime=now;
                else if(!imguiWarmed && now-firstMenuRepaintTime>=0.3f) WarmImguiText();
            }
            gameViewRect = ComputeGameViewRect();
            GUI.depth = 0;
            // Native UI and actors share a logical backing before display scaling.
            NativeFramebufferUi.Begin(menuDinoTarget,gameViewRect);

            if(bootSplashActive)
            {
                DrawSprite("BootSplashFrame", new Rect(0,0,1024,768));
                NativeFramebufferUi.End();
                return;
            }

            var background=CachedTexture("menu_bgFrame");
            if(background!=null)
            {
                // Height-fit, source pixels retained; clamp exterior edge texels
                // in adaptive widescreen rather than horizontally stretching art.
                background.wrapMode=TextureWrapMode.Clamp;
                float visibleWidth=menuLogicalWidth/1024f;
                NativeFramebufferUi.DrawTextureWithTexCoords(gameViewRect,background,new Rect((1-visibleWidth)*0.5f,0,visibleWidth,1));
            }
            if(menuDinoTarget!=null) {
                // The native actor was visible only through the 1024x768
                // iPad viewport. Adaptive background/UI may use wider space,
                // but must not expose the actor's offscreen approach there.
                Rect actorView=L2S(new Rect(0,0,1024,768));
                NativeFramebufferUi.DrawTextureClipped(gameViewRect,menuDinoTarget,actorView);
            }
            if(screen!=MenuScreen.Options && screen!=MenuScreen.Launching) DrawMenuWidget(0, false);

            switch(screen)
            {
                case MenuScreen.NewGame: DrawPrimaryScreen(true); break;
                case MenuScreen.Resume: DrawPrimaryScreen(false); break;
                case MenuScreen.Options: { var swOpt=System.Diagnostics.Stopwatch.StartNew(); DrawOptionsScreen(); if(!optionsFirstDrawLogged){ optionsFirstDrawLogged=true; Debug.Log("[DC1-HITCH] first Options OnGUI draw "+swOpt.ElapsedMilliseconds+" ms"); } } break;
                case MenuScreen.LifeMode: DrawLifeModeScreen(); break;
                case MenuScreen.LevelSelect: case MenuScreen.LevelSelectAlt: DrawPrimaryScreen(false); break;
                case MenuScreen.Launching:
                    if(loadingTween!=null) DrawSprite("text_loadingFrame",loadingTween.Rect);
                    break;
            }
            if(levelPresentation!=null && levelPresentation.Visible) DrawLevelSelectScreen();

            if(!string.IsNullOrEmpty(status))
            {
                GUI.Label(L2S(new Rect(10,740,1000,24)), status);
                NativeFramebufferUi.PreserveOverlay(L2S(new Rect(10,740,1000,24)));
            }
            // 61618 creates click control24 on layer3 with the original
            // helpFrame for both states; 5D380 case24 removes it on release.
            if(helpVisible && ClickSprite("helpFrame",new Rect(0,0,1024,768))) helpVisible=false;
            NativeFramebufferUi.End();
            if((SandboxModeSession.Active || screen==MenuScreen.Options) && DC1DebugSettings.Enabled)
            {
                var scale=DC1DebugPanel.ScaleForHeight(Screen.height);
                var previous=GUI.matrix;
                GUI.matrix=previous*Matrix4x4.Scale(new Vector3(scale,scale,1));
                NativeFramebufferUi.PreserveOverlay(new Rect(Screen.width-220*scale,10*scale,210*scale,40*scale));
                if(GUI.Button(new Rect(Screen.width/scale-220,10,210,40),"COMBAT WORKBENCH"))
                    SandboxModeSession.ReturnToWorkbench();
                GUI.matrix=previous;
            }
            if(Debug.isDebugBuild)NativeFramebufferUi.PreserveOverlay(new Rect(Screen.width-200,Screen.height-25,200,25));
        }

        // 5D064 creates all primary controls, at staggered source delays.
        void DrawPrimaryScreen(bool newGameMode)
        {
            bool interactive=levelPresentation==null && primaryLeave==null;
            if(DrawMenuWidget(1,interactive)) BeginMenuSelection("Lplay_dFrame",1,GoLevelSelect);
            if(resumeControlVisible && DrawMenuWidget(2,interactive))
                SelectResume();
            // Level selection owns this interval;5F5BC restores option ID3.
            if(levelPresentation==null && DrawMenuWidget(3,interactive)) BeginMenuSelection("Loption_dFrame",3,BeginOptionsLeave);
            if(DrawMenuWidget(4,interactive)) BeginJungleSelection();
            if(levelPresentation==null) DrawPrimaryIcons();
            DrawMenuSelection();
        }

        void DrawPrimaryIcons()
        {
            if(primaryLeave!=null) {
                DrawSprite("HELPFrame",primaryLeave.Help.Rect);
                return; // 614EC deletes social controls11/12 immediately.
            }
            // Native11/12 external service routes (7FB84/7FCA8) have no
            // modern backend here. Retain DA6D8 press/release and click
            // feedback while leaving the service action disconnected.
            ClickSprite("LISTFrame",new Rect(0,572,113,100));
            ClickSprite("ACHIEVEMENTFrame",new Rect(110,584,103,86));
            if(ClickSprite("HELPFrame",new Rect(815,584,103,86))) helpVisible=true;
            // 5F5BC creates iPad control9 hidden (+68=0), input-disabled
            // (+76=0).185DCC renders only when its +72 visibility getter is1.
            // Asset retained; Options18 owns the music toggle.
        }

        NativeUiRectTween[] optionsTweens;
        bool optionsLeaving;
        static Vector4 ShiftX(Vector4 value,float delta){value.x+=delta;return value;}
        Vector4 OptionsRest(int index)
        {
            // 5E144 scalar float literals, centers and source frame sizes.
            switch(index) {
                case 0:return new Vector4(296,191.5f,322,53);
                case 1:return new Vector4(283.5f,338.5f,187,53);
                case 2:return new Vector4(279,473,72,54);
                case 3:return new Vector4(773,191.5f,difficulty==1?138:difficulty==2?250:211,53);
                case 4:return new Vector4(770,338.5f,musicOn?84:108,53);
                case 5:return new Vector4(770,472.5f,sfxOn?84:108,53);
                case 6:return new Vector4(77,636.5f,154,63);
                default:return new Vector4(511.5f,605.5f,243,59);
            }
        }
        Rect OptionsRect(int index)
        {
            if(optionsTweens==null) {
                Vector4 rest=OptionsRest(index);
                return new Rect(rest.x-rest.z/2,rest.y-rest.w/2,rest.z,rest.w);
            }
            Rect rect=optionsTweens[index].Rect;
            // On/off and difficulty frame changes retain their native center.
            Vector2 center=rect.center;rect.width=OptionsRest(index).z;rect.center=center;
            return rect;
        }
        void BackFromOptions()
        {
            if(optionsTweens==null || optionsLeaving || optionsTweens[6].Active) return;
            optionsLeaving=true;
            for(int i=0;i<8;i++) {
                Rect rect=OptionsRect(i);
                Vector4 start=new Vector4(rect.center.x,rect.center.y,rect.width,rect.height);
                // 60784 all rows: current ->X+102 ->X-1024, .05/.2.
                optionsTweens[i]=new NativeUiRectTween(start,ShiftX(start,102),ShiftX(start,-1024),0.05f,0.2f);
            }
        }
        void DrawOptionsScreen()
        {
            // Primary controls including logo are removed by 5D380/3.
            // 5E144 creates eight rows; no invented static OPTION title.
            bool enabled=GUI.enabled;
            // 614EC deleted11/12;5FAB4 moved HELP below the viewport before
            // these rows were created. Do not redraw the static old footer.
            GUI.enabled=enabled&&!optionsLeaving&&(optionsTweens==null || !optionsTweens[6].Active);
            DrawSprite("text_difficultyFrame",OptionsRect(0));
            DrawSprite("text_musicFrame",OptionsRect(1));
            DrawSprite("text_fxFrame",OptionsRect(2));
            if(ClickSprite(DifficultyFrameFor(difficulty)[0],OptionsRect(3))) PressDifficultyCycle();
            if(ClickSprite(musicOn?"text_onFrame":"text_offFrame",OptionsRect(4))) PressMusicToggle();
            if(ClickSprite(sfxOn?"text_onFrame":"text_offFrame",OptionsRect(5))) PressSfxToggle();
            if(ClickSprite("level_backFrame",OptionsRect(6))) BackFromOptions();
            GUI.enabled=false;
            ClickSprite("text_supportFrame",OptionsRect(7));
            GUI.enabled=enabled;
            if(!optionsLeaving && (optionsTweens==null || !optionsTweens[6].Active))
            {
                Rect master=L2S(new Rect(260,505,500,30));
                Rect saves=L2S(new Rect(260,540,500,30));
                if(GUI.Button(master,"DEBUG TOOLS: "+(DC1DebugSettings.Enabled?"ON":"OFF")))
                    DC1DebugSettings.Enabled=!DC1DebugSettings.Enabled;
                if(GUI.Button(saves,"DEBUG AFFECTS SAVES: "+(DC1DebugSettings.AffectsSaves?"ON":"OFF")))
                    DC1DebugSettings.AffectsSaves=!DC1DebugSettings.AffectsSaves;
                Rect fps=L2S(new Rect(260,575,500,30));
                if(GUI.Button(fps,"FRAME LIMIT: "+DC1FramePacing.LimitName+(DC1FramePacing.VSync&&!DC1FramePacing.IsMobile?" (V-SYNC ON: SCREEN RATE)":"")))
                    DC1FramePacing.CycleLimit();
                NativeFramebufferUi.PreserveOverlay(fps);
                if(DC1FramePacing.VSyncSelectable)
                {
                    Rect vs=L2S(new Rect(260,610,500,30));
                    if(GUI.Button(vs,"V-SYNC: "+(DC1FramePacing.VSync?"ON":"OFF"))) DC1FramePacing.ToggleVSync();
                    NativeFramebufferUi.PreserveOverlay(vs);
                }
                NativeFramebufferUi.PreserveOverlay(master);NativeFramebufferUi.PreserveOverlay(saves);
            }
        }
        internal Rect TestHarness_OptionsRect(int index)=>OptionsRect(index);
        internal bool TestHarness_OptionsLeaving=>optionsLeaving;
        internal void TestHarness_BackFromOptions()=>BackFromOptions();

        void DrawLifeModeScreen()
        {
            // TASK027N.2.51: Life Mode's own OWN gameplay does not exist in
            // this port (deliberately out of this task's own named scope -
            // see sandbox_citygrind_shared_owner_audit_FULL.json's own
            // 'E-family behavior'/weapon-roster scope notes for the same
            // kind of deliberate narrowing applied elsewhere). Rather than
            // silently launching City Grind under a false "Life Mode"
            // label (which would misrepresent a different, jungle-themed,
            // separately-progressed native mode as City Grind), this
            // screen is honestly presented as not-yet-implemented.
            GUI.Label(L2S(new Rect(300,400,400,60)), "Life Mode - not yet implemented in this port.");
            if(GUI.Button(L2S(new Rect(40,670,100,40)), "BACK")) GoNewGame();
            NativeFramebufferUi.PreserveOverlay(L2S(new Rect(300,400,400,60)));
            NativeFramebufferUi.PreserveOverlay(L2S(new Rect(40,670,100,40)));
        }

        void DrawLevelSelectScreen()
        {
            Color before=GUI.color;
            // 610EC full UIImage layer1 RGBA80000000, opaque band layer2.
            GUI.color=new Color(0,0,0,128f/255f);
            NativeFramebufferUi.DrawTexture(gameViewRect,Texture2D.whiteTexture);
            Rect band=L2S(levelPresentation.Band.Rect);
            band.x=gameViewRect.x;band.width=gameViewRect.width;
            GUI.color=Color.black;NativeFramebufferUi.DrawTexture(band,Texture2D.whiteTexture);GUI.color=before;
            if(!levelPresentation.ControlsVisible) return;
            bool enabled=GUI.enabled;GUI.enabled=enabled&&levelPresentation.Interactive;
            if(ClickSprite("level_leftFrame",levelPresentation.Controls[1].Rect)) TestHarness_LevelDown();
            if(ClickSprite("level_rightFrame",levelPresentation.Controls[2].Rect)) TestHarness_LevelUp();
            if(ClickSprite("level_backFrame",levelPresentation.Controls[3].Rect)) BackFromLevelSelect();
            if(ClickSprite("level_goFrame",levelPresentation.Controls[4].Rect)) SelectGo();
            GUI.enabled=enabled;
            Rect label=levelPresentation.Controls[0].Rect;
            float y=label.center.y-43;
            var title=Resources.Load<Texture2D>("DinoCapGenerated/CityGrind/levelFrame");
            if(title!=null) NativeFramebufferUi.DrawTexture(L2S(new Rect(label.x,y,264,86)),title);
            float x=label.x+350;
            foreach(char c in levelSelectValue.ToString()) {
                int d=c-'0';if(d<0||d>9)continue;
                var texture=Resources.Load<Texture2D>("DinoCapGenerated/CityGrind/level"+d+"Frame");
                if(texture!=null) NativeFramebufferUi.DrawTexture(L2S(new Rect(x,y,LevelSelectDigitWidth[d],86)),texture);
                x+=LevelSelectDigitWidth[d];
            }
        }

        static readonly float[] LevelSelectDigitWidth={62,43,58,61,65,65,59,61,64,59};
        NativeMenuLevelSelect levelPresentation;
        System.Action levelLeaveComplete;


        static readonly string[] RevealFrames={"menu_logoFrame","CITYFrame","RESUMEFrame","OPTIONFrame","JUNGLEFrame"};
        static readonly Vector4[] RevealFinal={
            new Vector4(512,156,655,239),new Vector4(500,450,398,115),
            new Vector4(500,550,278,115),new Vector4(970,620,117,100),
            new Vector4(500,350,447,110)};
        static readonly float[] RevealDelay={0f,0.1f,0.2f,0.3f,0.4f};
        readonly NativeUiRectTween[] reveal=new NativeUiRectTween[5];
        float menuRevealElapsed;
        int pressedWidget=-1;
        NativeUiRectTween selectionTween;
        string selectionFrame;
        System.Action selectionComplete;
        float selectionElapsed;
        NativeMenuPrimaryLeave primaryLeave;
        System.Action primaryLeaveComplete;
        NativeUiRectTween loadingTween;
        bool gameplayLoadPending;
        NativeUiRectTween jungleTitleLeave;
        bool resumeControlVisible;

        void ResetMenuReveal()
        {
            levelPresentation=null;levelLeaveComplete=null;
            primaryLeave=null;
            jungleTitleLeave=null;
            primaryLeaveComplete=null;loadingTween=null;gameplayLoadPending=false;
            menuRevealElapsed=0f; pressedWidget=-1;
            resumeControlVisible=false;
            selectionTween=null; selectionComplete=null;
            for(int i=0;i<reveal.Length;i++) reveal[i]=null;
            CreateReveal(0);
        }
        void CreateReveal(int index)
        {
            //61F10 gates control2 when ResumeEnter creates it. GO commits
            //lives through7EEC8 but never re-enables this outgoing control.
            if(index==2)resumeControlVisible=DC1Preferences.GetInt(CityGridController.LivesPrefKey,0)>0;
            Vector4 end=RevealFinal[index];
            var from=new Vector4(end.x,end.y,end.z*0.1f,end.w*0.1f);
            var via=new Vector4(end.x,end.y,end.z*1.1f,end.w*1.1f);
            reveal[index]=new NativeUiRectTween(from,via,end,0.2f,0.05f);
        }
        void TickMenuReveal(float dt)
        {
            var previousLoading=loadingTween;
            if(primaryLeave==null)jungleTitleLeave?.Tick(dt);
            menuRevealElapsed+=dt;
            if(primaryLeave!=null) {
                primaryLeave.Tick(dt);
                if(!primaryLeave.Active) {
                    primaryLeave=null;
                    var done=primaryLeaveComplete;primaryLeaveComplete=null;done?.Invoke();
                }
            } else for(int i=0;i<reveal.Length;i++) {
                if(reveal[i]==null && menuRevealElapsed>=RevealDelay[i])CreateReveal(i);
                reveal[i]?.Tick(dt);
            }
            if(selectionTween!=null && selectionTween.Active)
            {
                selectionElapsed+=dt;
                selectionTween.Tick(dt);
                if(!selectionTween.Active) {
                    // 5D380 case22 hides the selected L*_d control when
                    // DA3C0 emits completion1 after both .05/.2 phases.
                    var done=selectionComplete; selectionComplete=null;
                    selectionTween=null; done?.Invoke();
                }
            }
            if(levelPresentation!=null) {
                levelPresentation.Tick(dt);
                if(!levelPresentation.Visible) {
                    levelPresentation=null;
                    var done=levelLeaveComplete;levelLeaveComplete=null;done?.Invoke();
                }
            }
            if(optionsTweens!=null) {
                foreach(var tween in optionsTweens) tween.Tick(dt);
                if(optionsLeaving && !optionsTweens[6].Active) GoNewGame();
            }
            if(previousLoading!=null && previousLoading==loadingTween && loadingTween.Active) {
                loadingTween.Tick(dt);
                if(!loadingTween.Active)gameplayLoadPending=true;
            }
        }
        bool DrawMenuWidget(int index,bool interactive)
        {
            var tween=(index==0?jungleTitleLeave:null)??primaryLeave?.Controls[index]??reveal[index];
            if(tween==null) return false;
            Rect rect=tween.Rect;
            bool canPress=interactive && !helpVisible && primaryLeave==null && !tween.Active && selectionTween==null;
            bool clicked=HandleMoveInput(rect,canPress,out rect);
            DrawSprite(RevealFrames[index],rect);
            return clicked;
        }
        void BeginJungleSelection()
        {
            // 5D380 case26 starts 5DE80 title leave immediately, parallel to selection.
            jungleTitleLeave=new NativeUiRectTween(new Vector4(512,156,655,239),
                new Vector4(512,-612,655,239),Vector4.zero,.05f,.2f);
            BeginMenuSelection("syrvival_dFrame",4,GoLifeMode);
        }
        void BeginMenuSelection(string frame,int index,System.Action complete)
        {
            Vector4 start=RevealFinal[index];
            selectionFrame=frame; selectionComplete=complete; selectionElapsed=0f;
            selectionTween=new NativeUiRectTween(start,start,new Vector4(start.x,start.y,start.z*1.5f,start.w*1.5f),0.05f,0.2f);
            PlayMenuClick();
        }
        void DrawMenuSelection()
        {
            if(selectionTween==null) return;
            Color before=GUI.color;
            GUI.color=new Color(before.r,before.g,before.b,before.a*Mathf.Min(1f,selectionElapsed/0.05f));
            DrawSprite(selectionFrame,selectionTween.Rect); GUI.color=before;
        }
        internal void TestHarness_ResetReveal()=>ResetMenuReveal();
        internal bool TestHarness_LaunchingJungle=>launchingJungle;
        internal void TestHarness_BeginJungleSelection()=>BeginJungleSelection();
        internal void TestHarness_StepReveal(float dt)=>TickMenuReveal(dt);
        internal Rect? TestHarness_RevealRect(int index)=>((index==0?jungleTitleLeave:null)??primaryLeave?.Controls[index]??reveal[index])?.Rect;
        internal bool TestHarness_RevealActive(int index)=>reveal[index]!=null && reveal[index].Active;
        internal NativeMenuLevelSelect TestHarness_LevelPresentation=>levelPresentation;
        internal bool TestHarness_PrimarySelectionEnabled=>(screen==MenuScreen.NewGame || screen==MenuScreen.Resume) && !helpVisible && primaryLeave==null && levelPresentation==null && selectionTween==null;
        internal bool TestHarness_SelectionVisible=>selectionTween!=null;
        internal bool TestHarness_HelpVisible=>helpVisible;
        internal void TestHarness_ShowHelp()=>helpVisible=true;
        internal void TestHarness_CloseHelp()=>helpVisible=false;
        internal void TestHarness_PlayMenuClick()=>PlayMenuClick();
        void SelectGo()
        {
            if(levelPresentation==null || !levelPresentation.Interactive)return;
            // 5D380/21 resets the committed run before60D28 leaves controls.
            PrepareGoRun();
            BeginLevelSelectLeave(BeginResumeLeave);
        }
        internal void TestHarness_SelectGo()=>SelectGo();
        void SelectResume()=>BeginMenuSelection("Lresume_dFrame",2,BeginResumeLeave);
        internal void TestHarness_SelectResume()=>SelectResume();
        internal Rect TestHarness_LoadingRect=>loadingTween.Rect;
        internal bool TestHarness_GameplayLoadPending=>gameplayLoadPending;
        internal void TestHarness_SelectCity()=>BeginMenuSelection("Lplay_dFrame",1,GoLevelSelect);
        internal void TestHarness_SelectOptions()=>BeginMenuSelection("Loption_dFrame",3,BeginOptionsLeave);

        // ---- TestHarness (mirrors CityGridController's own convention) ----
        internal MenuScreen TestHarness_Screen => screen;
        internal int TestHarness_Difficulty => difficulty;
        internal bool TestHarness_MusicOn => musicOn;
        internal bool TestHarness_SfxOn => sfxOn;
        internal int TestHarness_LevelSelectValue => levelSelectValue;
        internal void TestHarness_GoNewGame() => GoNewGame();
        internal void TestHarness_GoResume() => GoResume();
        internal void TestHarness_GoOptions() => GoOptions();
        internal void TestHarness_GoLifeMode() => GoLifeMode();
        internal void TestHarness_GoLevelSelect() => GoLevelSelect();
        internal void TestHarness_BackFromLevelSelect() => BackFromLevelSelect();
        internal void TestHarness_PressGo() => PressGo();
        internal void TestHarness_PressResume() => PressResume();
        internal void TestHarness_PressDifficultyCycle() => PressDifficultyCycle();
        internal void TestHarness_PressMusicToggle() => PressMusicToggle();
        internal void TestHarness_PressSfxToggle() => PressSfxToggle();
        internal void TestHarness_SetMaxLevelReached(int n) => maxLevelReached = Mathf.Max(1,n);
        internal int TestHarness_MaxLevelReached => maxLevelReached;
        internal bool TestHarness_MenuAssetLoads(string frame) => CachedTexture(frame) != null;
        internal bool TestHarness_ResumeWouldBeOffered => DC1Preferences.GetInt(CityGridController.LivesPrefKey, 0)>0;
        internal bool TestHarness_ResumeControlVisible=>resumeControlVisible;
    }
}
