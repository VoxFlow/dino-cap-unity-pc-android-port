#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public class Task027N252MenuCausalRepro
    {
        MainMenuController menu;
        DC1MenuPreferenceScope preferences;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            preferences=new DC1MenuPreferenceScope();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            menu=Object.FindFirstObjectByType<MainMenuController>();
            Assert.IsNotNull(menu);
        }
        [TearDown]
        public void TearDown(){preferences?.Dispose();preferences=null;}

        [Test]
        public void MenuSceneHasActiveDisplayCameraAlongsideOffscreenActorCamera()
        {
            Camera display=null;
            foreach(var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if(cam.isActiveAndEnabled && cam.targetTexture==null && cam.targetDisplay==0)display=cam;
            Assert.IsNotNull(display,"Display1 needs a real camera; an offscreen RT camera cannot own the Game view.");
            Assert.AreEqual(0,display.cullingMask,"Menu actor is already composited from its RT; display camera must not draw it twice.");
            Assert.IsNotNull(menu.TestHarness_DinoCamera.targetTexture);
        }

        [Test]
        public void MenuAudioHasExactlyOneEnabledListener()
        {
            int active=0;
            foreach(var listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                if(listener.isActiveAndEnabled)active++;
            Assert.AreEqual(1,active,"Playing AudioSources do not produce audible menu audio without a listener.");
        }

        [UnityTest]
        public IEnumerator N252_MusicPlaybackAndPreferenceSurviveMenuCityMenuReload()
        {
            var source=menu.TestHarness_MenuMusicSource;
            Assert.IsNotNull(source,"Menu must own its native music source.");
            Assert.AreEqual("Music_menu",source.clip.name);
            Assert.IsTrue(source.isPlaying);
            Assert.IsFalse(source.mute);
            menu.TestHarness_PressMusicToggle();
            Assert.IsTrue(source.mute);
            DC1Preferences.Save();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null;
            var city=Object.FindFirstObjectByType<CityGridController>();
            Assert.IsTrue(city.TestHarness_MusicMuted,"Menu OFF must carry into gameplay.");
            city.TestHarness_SetMusicMuted(false);
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            menu=Object.FindFirstObjectByType<MainMenuController>();
            Assert.IsTrue(menu.TestHarness_MusicOn,"Gameplay ON must carry back into recreated menu.");
            source=menu.TestHarness_MenuMusicSource;
            Assert.IsTrue(source.isPlaying);
            Assert.IsFalse(source.mute);
            var sources=Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach(var s in sources)Assert.IsFalse(s.clip!=null && s.clip.name=="Music_Bg1","Scene exit must dispose gameplay BGM.");
        }

        [UnityTest]
        public IEnumerator N252_WeaponSwitchProducesNoShotAudioRequest()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null;
            var city=Object.FindFirstObjectByType<CityGridController>();
            city.TestHarness_SkipLevelIntro();
            NativeAudioSessionGate.FxMuted=false;
            NativeAudioSessionGate.TestHarness_ResetClipCounters();
            int before=NativeAudioSessionGate.TotalRequests;
            city.TestHarness_SwitchWeapon();
            Assert.AreEqual(before+1,NativeAudioSessionGate.TotalRequests,"Native56E94 independently requests Button1 on switch.");
            Assert.AreEqual(1,NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Button1"));
            foreach(string clip in new[]{"Attack_A","Attack_B","Attack_C","Attack_D"})
                Assert.AreEqual(0,NativeAudioSessionGate.TestHarness_AdmittedCount(clip),"Switch cannot request a shot voice.");
        }

        [TestCase(0,0,true)]
        [TestCase(1,1,true)]
        [TestCase(0,1,false)]
        public void N252_MusicLegacyConflictPreservesOffAndNextToggleAligns(int cityMuted,int menuOn,bool muted)
        {
            DC1Preferences.SetInt(NativeAudioSessionGate.MusicMutedPrefKey,cityMuted);
            DC1Preferences.SetInt("DC1_MainMenu_MusicOn",menuOn);
            Assert.AreEqual(muted,NativeAudioSessionGate.LoadMusicPreference());
            NativeAudioSessionGate.SaveMusicPreference(!muted);
            Assert.AreEqual(!muted,NativeAudioSessionGate.LoadMusicPreference());
            Assert.AreEqual(muted?0:1,DC1Preferences.GetInt(NativeAudioSessionGate.MusicMutedPrefKey));
            Assert.AreEqual(muted?1:0,DC1Preferences.GetInt("DC1_MainMenu_MusicOn"));
        }

        [Test]
        public void N252_MusicOffKeepsMenuFxAudible()
        {
            if(menu.TestHarness_MusicOn)menu.TestHarness_PressMusicToggle();
            if(!menu.TestHarness_SfxOn)menu.TestHarness_PressSfxToggle();
            NativeAudioSessionGate.StopAll();
            menu.TestHarness_PlayMenuClick();
            AudioSource click=null;
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                if(source.clip!=null && source.clip.name=="Audio_button")click=source;
            Assert.IsNotNull(click);
            Assert.IsFalse(click.mute,"Music OFF must not mute the separately enabled FX channel.");
            Assert.AreNotSame(menu.TestHarness_MenuMusicSource,click);
        }

        [Test]
        public void N252_MenuSilhouetteRemainsActiveUntilNativeWorldBoundary()
        {
            menu.enabled=false;
            menu.TestHarness_StepDinoCreation(5f);
            typeof(MainMenuController).GetField("dinoLogicalX",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(menu,1270f);
            typeof(MainMenuController).GetMethod("UpdateBackgroundDino",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(menu,null);
            Assert.IsTrue(menu.TestHarness_DinoActive,"World758 is below native768; old logical1224 adapter deactivated too early.");
        }

        [TestCase(767.99f,true)]
        [TestCase(768f,true)]
        [TestCase(768.01f,false)]
        public void N252_MenuSilhouetteStrictBoundaryAndCachedReuse(float worldX,bool active)
        {
            menu.enabled=false;
            menu.TestHarness_StepDinoCreation(5f);
            var camera=menu.TestHarness_DinoCamera;
            var fields=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            typeof(MainMenuController).GetField("dinoCreationTimer",fields).SetValue(menu,0f);
            var actor=typeof(MainMenuController).GetField("dinoVisual",fields).GetValue(menu);
            typeof(MainMenuController).GetField("dinoLogicalX",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(menu,worldX+512f);
            menu.TestHarness_StepBackgroundDino(0f);
            Assert.AreEqual(active,menu.TestHarness_DinoActive);
            if(!active) {
                menu.TestHarness_StepDinoCreation(4.99f);
                Assert.IsFalse(menu.TestHarness_DinoActive,"No immediate recycle before the next creation pulse.");
                menu.TestHarness_StepDinoCreation(.02f);
                Assert.IsTrue(menu.TestHarness_DinoActive);
                Assert.AreSame(camera,menu.TestHarness_DinoCamera,"Cached menu actor/camera are retained.");
                Assert.AreSame(actor,typeof(MainMenuController).GetField("dinoVisual",fields).GetValue(menu));
                Assert.AreEqual(-160f,menu.TestHarness_DinoLogicalX);
            }
        }

        [Test]
        public void PrimarySelectionClickAdmitsOneNativeVoiceAndHonorsSfxMute()
        {
            bool muted=NativeAudioSessionGate.FxMuted;
            bool sfx=menu.TestHarness_SfxOn;
            const string key="DC1_MainMenu_SfxOn";
            int? saved=DC1Preferences.HasKey(key)?(int?)DC1Preferences.GetInt(key):null;
            try {
                NativeAudioSessionGate.FxMuted=false;
                NativeAudioSessionGate.StopAll();
                NativeAudioSessionGate.TestHarness_ResetClipCounters();
                var clip=Resources.Load<AudioClip>("Audio/Audio_button");
                Assert.IsNotNull(clip);
                if(!menu.TestHarness_SfxOn)menu.TestHarness_PressSfxToggle();
                int requests=NativeAudioSessionGate.TotalRequests;
                menu.TestHarness_SelectCity();
                Assert.AreEqual(1,NativeAudioSessionGate.TestHarness_AdmittedCount(clip.name),
                    $"Empty voice pool: clip={clip.name},sfx={menu.TestHarness_SfxOn},muted={NativeAudioSessionGate.FxMuted},requests={NativeAudioSessionGate.TotalRequests-requests},admitted={NativeAudioSessionGate.TotalAdmitted},dropped={NativeAudioSessionGate.TotalDropped},sessions={NativeAudioSessionGate.CountedSessions}");
                menu.TestHarness_PressSfxToggle();
                menu.TestHarness_PlayMenuClick();
                Assert.AreEqual(1,NativeAudioSessionGate.TestHarness_AdmittedCount(clip.name),"Muted FX cannot admit another menu click voice.");
            } finally {
                NativeAudioSessionGate.FxMuted=muted;
                if(menu.TestHarness_SfxOn!=sfx)menu.TestHarness_PressSfxToggle();
                if(saved.HasValue)DC1Preferences.SetInt(key,saved.Value);else DC1Preferences.DeleteKey(key);
            }
        }

        [TestCase(0,"k3_move")]
        [TestCase(1,"k2_move")]
        public void NativeMenuActorKeepsAnimatingBeyondFourFullClips(int variant,string clipName)
        {
            var saved=Random.state;
            try {
                for(int seed=0;seed<100;seed++) {
                    Random.InitState(seed);
                    if((Random.value*100f>=50f?1:0)!=variant)continue;
                    Random.InitState(seed);
                    typeof(MainMenuController).GetMethod("InitBackgroundDino",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(menu,null);
                    break;
                }
                var track=menu.TestHarness_DinoVisual.primary;
                Assert.AreEqual(clipName,track.Clip.name);
                int count=track.Clip.frames.Length;
                bool assetLoop=track.Clip.loop;
                for(int i=0;i<count*4+3;i++)track.Tick(track.Clip.frameDuration);
                Assert.IsTrue(track.Active,"Native627CC passes loop count -1; the run cannot freeze after its first cycle.");
                Assert.AreEqual(3,track.Frame);
                Assert.AreEqual(assetLoop,track.Clip.loop,"Per-actor loop request must not mutate the shared asset.");
            } finally { Random.state=saved; }
        }

        [Test]
        public void CitySelectionHighlightExitsAtNativeTwoPhaseCompletion()
        {
            menu.enabled=false;
            menu.TestHarness_ResetReveal();
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            menu.TestHarness_SelectCity();
            menu.TestHarness_StepReveal(0.05f);
            Assert.IsTrue(menu.TestHarness_SelectionVisible,"Highlight survives the first .05s phase.");
            menu.TestHarness_StepReveal(0.19f);
            Assert.IsTrue(menu.TestHarness_SelectionVisible,"The .2s expansion has not completed.");
            menu.TestHarness_StepReveal(0.02f);
            Assert.IsFalse(menu.TestHarness_SelectionVisible,"Completion hides Lplay_dFrame before the level-band transition.");
            Assert.IsNotNull(menu.TestHarness_LevelPresentation);
            Assert.IsFalse(menu.TestHarness_PrimarySelectionEnabled);
        }

        [Test]
        public void OptionsSelectionWaitsForNativePrimaryControlLeaveBeforeRowsEnter()
        {
            menu.enabled=false;menu.TestHarness_ResetReveal();
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            menu.TestHarness_SelectOptions();
            menu.TestHarness_StepReveal(0.05f);menu.TestHarness_StepReveal(0.21f);
            Assert.AreEqual(MainMenuController.MenuScreen.NewGame,menu.TestHarness_Screen,
                "614EC schedules primary leave; Options rows cannot replace the entire screen at highlight completion.");
            Assert.IsFalse(menu.TestHarness_PrimarySelectionEnabled);
            for(int i=0;i<40;i++)menu.TestHarness_StepReveal(0.01f);
            Assert.AreEqual(MainMenuController.MenuScreen.NewGame,menu.TestHarness_Screen,
                "62704's OPTION leave has not completed yet.");
            for(int i=0;i<20;i++)menu.TestHarness_StepReveal(0.01f);
            Assert.AreEqual(MainMenuController.MenuScreen.Options,menu.TestHarness_Screen);
        }

        [Test]
        public void HelpUsesExistingAuthenticAssetAndReturnsToUnderlyingMenu()
        {
            var before=menu.TestHarness_Screen;
            var help=Resources.Load<Texture2D>("DinoCapGenerated/MainMenu/help_tutorialFrame");
            Assert.IsNotNull(help);
            Assert.AreEqual(1024,help.width);
            Assert.AreEqual(768,help.height);
            menu.TestHarness_ShowHelp();
            Assert.IsTrue(menu.TestHarness_HelpVisible);
            Assert.IsFalse(menu.TestHarness_PrimarySelectionEnabled,"Native layer3 tutorial blocks underlying controls.");
            menu.TestHarness_CloseHelp();
            Assert.IsFalse(menu.TestHarness_HelpVisible);
            Assert.AreEqual(before,menu.TestHarness_Screen);
        }

        [TestCase(1)]
        [TestCase(3)]
        public void N252_MixedMenuRoundTripsDoNotRetainPreviousTransitionState(int repetitions)
        {
            // Exercise complete routes together: individual return tests do
            // not detect callbacks/tweens leaking into the next subscreen.
            menu.enabled=false;menu.TestHarness_ResetReveal();
            for(int step=0;step<80;step++)menu.TestHarness_StepReveal(0.01f);
            for(int round=0;round<repetitions;round++) {
                menu.TestHarness_SelectOptions();
                for(int step=0;step<100 && menu.TestHarness_Screen!=MainMenuController.MenuScreen.Options;step++)
                    menu.TestHarness_StepReveal(0.01f);
                Assert.AreEqual(MainMenuController.MenuScreen.Options,menu.TestHarness_Screen);
                Assert.IsFalse(menu.TestHarness_SelectionVisible);
                menu.TestHarness_BackFromOptions();
                Assert.IsFalse(menu.TestHarness_OptionsLeaving,
                    "BACK cannot start while the native Options controls are still entering.");
                for(int step=0;step<80;step++)menu.TestHarness_StepReveal(0.01f);
                menu.TestHarness_BackFromOptions();
                Assert.IsTrue(menu.TestHarness_OptionsLeaving);
                menu.TestHarness_StepReveal(0.04f);
                Assert.AreEqual(MainMenuController.MenuScreen.Options,menu.TestHarness_Screen,
                    "BACK release must not replace the leaving rows immediately.");
                for(int step=0;step<100;step++)menu.TestHarness_StepReveal(0.01f);
                Assert.IsTrue(menu.TestHarness_PrimarySelectionEnabled);
                Assert.IsNull(menu.TestHarness_LevelPresentation);

                menu.TestHarness_SelectCity();
                for(int step=0;step<110;step++)menu.TestHarness_StepReveal(0.01f);
                Assert.IsTrue(menu.TestHarness_LevelPresentation.Interactive);
                menu.TestHarness_BackFromLevelSelect();
                menu.TestHarness_StepReveal(0.04f);
                Assert.IsFalse(menu.TestHarness_PrimarySelectionEnabled,
                    "Primary input stays blocked until label and band leave callbacks complete.");
                for(int step=0;step<80;step++)menu.TestHarness_StepReveal(0.01f);
                Assert.IsNull(menu.TestHarness_LevelPresentation);
                Assert.IsTrue(menu.TestHarness_PrimarySelectionEnabled);
                Assert.IsFalse(menu.TestHarness_GameplayLoadPending,
                    "Returning with BACK must never retain the GO/RESUME loading action.");

                menu.TestHarness_ShowHelp();
                Assert.IsFalse(menu.TestHarness_PrimarySelectionEnabled);
                menu.TestHarness_CloseHelp();
                Assert.IsTrue(menu.TestHarness_PrimarySelectionEnabled);
            }
        }

        [UnityTest]
        public IEnumerator SavedInProgressLevelDoesNotBecomeAnInvalidCheckpointDialEntry()
        {
            const string level="DC1_CityGrind_Level",max="DC1_CityGrind_MaxLevel";
            int? savedLevel=DC1Preferences.HasKey(level)?(int?)DC1Preferences.GetInt(level):null;
            int? savedMax=DC1Preferences.HasKey(max)?(int?)DC1Preferences.GetInt(max):null;
            try {
                DC1Preferences.SetInt(level,17);DC1Preferences.SetInt(max,23);
                EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                menu=Object.FindFirstObjectByType<MainMenuController>();
                menu.TestHarness_GoLevelSelect();
                Assert.AreEqual(1,menu.TestHarness_LevelSelectValue);
                foreach(int next in new[]{5,10,15,20,20}) {
                    menu.TestHarness_LevelUp();
                    Assert.AreEqual(next,menu.TestHarness_LevelSelectValue);
                }
                Assert.AreEqual(17,DC1Preferences.GetInt(level),"Entering/turning the dial must not overwrite exact RESUME progress.");
                menu.TestHarness_SetMaxLevelReached(4);
                menu.TestHarness_GoLevelSelect();
                menu.TestHarness_LevelUp();
                Assert.AreEqual(1,menu.TestHarness_LevelSelectValue);
            } finally {
                if(savedLevel.HasValue)DC1Preferences.SetInt(level,savedLevel.Value);else DC1Preferences.DeleteKey(level);
                if(savedMax.HasValue)DC1Preferences.SetInt(max,savedMax.Value);else DC1Preferences.DeleteKey(max);
            }
        }

        [Test]
        public void NativeMovePressUsesOriginalBoundsAndDragCancelReleaseOwnership()
        {
            var state=new NativeMenuMoveInput();var rect=new Rect(100,100,80,40);
            Assert.IsFalse(state.Down(rect,new Vector2(100,120)),"Native boundary comparison is strict.");
            Assert.IsTrue(state.Down(rect,rect.center));
            Assert.AreEqual(rect.center,state.Presentation(rect).center);
            Assert.AreEqual(new Vector2(40,20),state.Presentation(rect).size);
            state.Drag(new Vector2(105,120));
            Assert.IsTrue(state.Pressed,"Dragging outside the shrunken visual but inside original capture bounds preserves capture.");
            state.Drag(new Vector2(180,120));
            Assert.IsFalse(state.Pressed);
            state.Drag(rect.center);
            Assert.IsFalse(state.Up(),"A cancelled drag does not recapture on return.");
            Assert.IsTrue(state.Down(rect,rect.center));
            Assert.IsTrue(state.Up());
            Assert.IsFalse(state.Up(),"Only the captured release emits selection/audio.");
        }

        [UnityTest]
        public IEnumerator ResumeSelectionDoesNotLoadCityBeforeNativePrimaryLeave()
        {
            menu.enabled=false;
            menu.TestHarness_ResetReveal();
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            DC1Preferences.SetInt(CityGridController.LivesPrefKey,3);
            DC1Preferences.SetInt("DC1_CityGrind_Level",7);
            menu.TestHarness_SelectResume();
            menu.TestHarness_StepReveal(0.05f);
            menu.TestHarness_StepReveal(0.21f);
            yield return null;
            Assert.AreEqual("MainMenuScene",SceneManager.GetActiveScene().name,
                "Native5D380/22 ->61438 leaves primary controls before control13 loading completion may load City.");
            var leave=typeof(MainMenuController).GetField("primaryLeave",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(menu);
            Assert.IsNotNull(leave,"Resume uses the native primary-leave phase.");
        }

        [Test]
        public void GoCommitsPresetBeforeLevelControlsLeave()
        {
            menu.enabled=false;
            menu.TestHarness_GoLevelSelect();
            for(int i=0;i<100;i++)menu.TestHarness_StepReveal(0.01f);
            Assert.IsFalse(menu.TestHarness_ResumeControlVisible);
            DC1Preferences.SetInt("DC1_CityGrind_Cash",2468);
            DC1Preferences.SetFloat(CityGrindStartPreset.HealthPrefKey,37.5f);
            menu.TestHarness_SelectGo();
            Assert.AreEqual(0,DC1Preferences.GetInt("DC1_CityGrind_Cash"),"Native5D380/21 calls7EEC8 before60D28 leave starts.");
            Assert.AreEqual(100f,DC1Preferences.GetFloat(CityGrindStartPreset.HealthPrefKey));
            Assert.IsTrue(menu.TestHarness_ResumeWouldBeOffered,"GO saves the resumable run before gameplay starts.");
            Assert.IsFalse(menu.TestHarness_ResumeControlVisible,"Native61F10 visibility remains latched for the outgoing menu; GO does not recreate Resume.");
            Assert.AreEqual("MainMenuScene",SceneManager.GetActiveScene().name);
            Assert.IsNotNull(menu.TestHarness_LevelPresentation);
            Assert.IsFalse(menu.TestHarness_LevelPresentation.Interactive);
        }

        [UnityTest]
        public IEnumerator ResumeLoadsOnlyAfterNativeLoadingControlCompletes()=>CheckGameplayLoading(false);

        [UnityTest]
        public IEnumerator GoLoadsOnlyAfterNativeLoadingControlCompletes()=>CheckGameplayLoading(true);

        IEnumerator CheckGameplayLoading(bool go)
        {
            menu.enabled=false;menu.TestHarness_ResetReveal();
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            DC1Preferences.SetInt("DC1_CityGrind_Level",7);
            DC1Preferences.SetInt(CityGridController.LivesPrefKey,3);
            if(go) {
                menu.TestHarness_GoLevelSelect();
                for(int i=0;i<100;i++)menu.TestHarness_StepReveal(0.01f);
                menu.TestHarness_SelectGo();
            } else menu.TestHarness_SelectResume();
            for(int i=0;i<300 && menu.TestHarness_Screen!=MainMenuController.MenuScreen.Launching;i++)
                menu.TestHarness_StepReveal(0.01f);
            Assert.AreEqual(MainMenuController.MenuScreen.Launching,menu.TestHarness_Screen);
            Assert.AreEqual("MainMenuScene",SceneManager.GetActiveScene().name);
            Assert.IsFalse(menu.TestHarness_PrimarySelectionEnabled);
            Assert.AreEqual(new Rect(1852,636,196,32),menu.TestHarness_LoadingRect);
            menu.TestHarness_StepReveal(0.2f);
            Assert.AreEqual(new Rect(726,636,196,32),menu.TestHarness_LoadingRect);
            menu.TestHarness_StepReveal(0.04f);
            Assert.IsFalse(menu.TestHarness_GameplayLoadPending,"Second native .05 phase has not completed.");
            menu.TestHarness_StepReveal(0.011f);
            Assert.IsTrue(menu.TestHarness_GameplayLoadPending);
            Assert.AreEqual(new Rect(828,636,196,32),menu.TestHarness_LoadingRect);
            Assert.AreEqual("MainMenuScene",SceneManager.GetActiveScene().name,"62A98 consumes completion on scene update.");
            menu.enabled=true;
            // The gameplay asset preload (PLATFORM_ADAPTER, MainMenuController.PreloadGameplayThenLaunch) uploads textures a few per frame
            // behind the Loading slide, so on a cold process the scene appears after many frames; bounded wait, never a fixed frame count.
            CityGridController city=null;
            float loadDeadline=Time.realtimeSinceStartup+20f;
            while(city==null && Time.realtimeSinceStartup<loadDeadline){ yield return null; city=Object.FindFirstObjectByType<CityGridController>(); }
            Assert.IsNotNull(city);
            Assert.AreEqual(go?1:7,city.TestHarness_CityLevel,"GO reconstructs level1; RESUME retains exact level7.");
        }

        [UnityTest]
        public IEnumerator ResumeRetainsExactCommittedRunRatherThanCheckpointPreset()
        {
            int[] tiers={2,0,-1,1,-1,1};
            int[] ammo={63,27,0,49,0,118};
            DC1Preferences.SetInt("DC1_CityGrind_Level",7);
            DC1Preferences.SetInt("DC1_CityGrind_MaxLevel",13);
            DC1Preferences.SetInt("DC1_CityGrind_Lives",3);
            DC1Preferences.SetInt("DC1_CityGrind_Cash",2468);
            DC1Preferences.SetFloat(CityGrindStartPreset.HealthPrefKey,37.5f);
            DC1Preferences.SetInt("DC1_CityGrind_Tier_G",1);
            DC1Preferences.SetInt("DC1_CityGrind_OwnershipModelMigratedV1",1);
            for(int i=0;i<6;i++) {
                DC1Preferences.SetInt("DC1_CityGrind_Tier_"+i,tiers[i]);
                DC1Preferences.SetInt(CityGrindStartPreset.AmmoPrefPrefix+i,ammo[i]);
            }
            menu.TestHarness_PressDifficultyCycle(); // Existing menu difficulty1 ->2.
            menu.TestHarness_PressResume();
            yield return null;yield return null;yield return null;
            var city=Object.FindFirstObjectByType<CityGridController>();
            Assert.IsNotNull(city);
            Assert.AreEqual(7,city.TestHarness_CityLevel,"Resume must not floor the saved level to a dial checkpoint.");
            Assert.AreEqual(3,city.TestHarness_Lives);
            Assert.AreEqual(2468,city.TestHarness_CashTotal);
            Assert.AreEqual(37.5f,city.TestHarness_PlayerHP);
            Assert.AreEqual(13,DC1Preferences.GetInt("DC1_CityGrind_MaxLevel"));
            Assert.AreEqual(2,DC1Preferences.GetInt(CityGridController.DifficultyPrefKey));
            Assert.AreEqual(1,DC1Preferences.GetInt("DC1_CityGrind_Tier_G"));
            for(int i=0;i<6;i++) {
                Assert.AreEqual(tiers[i],city.TestHarness_RangedFamilyTier(i),"Owned tier family"+i);
                Assert.AreEqual(ammo[i],city.TestHarness_AmmoFor(i),"Committed stock family"+i);
            }
        }

        [UnityTest]
        public IEnumerator NativeFxSettingIsSharedAcrossMenuCityAndReturn()
        {
            menu.TestHarness_PressSfxToggle();
            Assert.IsTrue(NativeAudioSessionGate.FxMuted);
            yield return SceneManager.LoadSceneAsync("CityGrindScene");
            yield return null;
            var city=Object.FindFirstObjectByType<CityGridController>();
            Assert.IsTrue(city.TestHarness_FxMuted,"City must consume the same native FX flag set by Options.");
            city.TestHarness_SetFxMuted(false);
            yield return SceneManager.LoadSceneAsync("MainMenuScene");
            yield return null;
            menu=Object.FindFirstObjectByType<MainMenuController>();
            Assert.IsTrue(menu.TestHarness_SfxOn,"Menu must consume the flag changed through City's pause settings.");
            Assert.IsFalse(NativeAudioSessionGate.FxMuted);
        }
    }
}
#endif
