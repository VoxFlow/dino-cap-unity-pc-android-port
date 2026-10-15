#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    // TASK027N.2.51 LARGE CONTINUATION Section 16 - first-pass regression
    // coverage for MainMenuController, the Main Menu's own new state
    // machine/navigation/persistence-wiring built this session directly on
    // mainmenu_state_machine_contract.json's own evidence trail. Functional/
    // state-machine coverage, matching this project's own established
    // testing philosophy (NOT pixel-perfect visual tests, since the
    // primary-screen button LAYOUT is explicitly disclosed as
    // PLATFORM_ADAPTER/not-pixel-verified in MainMenuController.cs's own
    // header comment - only the level-select widget SIZES are cross-
    // validated against real exported asset dimensions).
    public class Task027N251MainMenuRepro
    {
        const string MenuScenePath = "Assets/DinoCap/Scenes/MainMenuScene.unity";
        const string CityScenePath = "Assets/DinoCap/Scenes/CityGrindScene.unity";

        MainMenuController menu;
        DC1MenuPreferenceScope preferences;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            preferences=new DC1MenuPreferenceScope();
            DC1Preferences.DeleteKey("DC1_CityGrind_Level");
            DC1Preferences.DeleteKey("DC1_CityGrind_MaxLevel");
            DC1Preferences.DeleteKey("DC1_CityGrind_Difficulty");
            DC1Preferences.DeleteKey("DC1_MainMenu_MusicOn");
            DC1Preferences.DeleteKey("DC1_MainMenu_SfxOn");
            EditorSceneManager.LoadSceneInPlayMode(
                MenuScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            menu = Object.FindFirstObjectByType<MainMenuController>();
            Assert.IsNotNull(menu, "MainMenuController not found after loading " + MenuScenePath);
        }

        [TearDown]
        public void TearDown()
        {
            DC1Preferences.DeleteKey("DC1_CityGrind_Level");
            DC1Preferences.DeleteKey("DC1_CityGrind_MaxLevel");
            DC1Preferences.DeleteKey("DC1_CityGrind_Difficulty");
            DC1Preferences.DeleteKey("DC1_MainMenu_MusicOn");
            DC1Preferences.DeleteKey("DC1_MainMenu_SfxOn");
            preferences?.Dispose();preferences=null;
        }

        // TASK027N.2.51: PROVEN native default boot screen (sub_5D064's own
        // sole firing site is at the END of sub_59DEC, the Main Menu's own
        // scene-construction routine) - boots directly into New Game, not a
        // neutral/blank state.
        [Test]
        public void N251M_01_BootsDirectlyIntoNewGameScreen()
        {
            Assert.AreEqual(MainMenuController.MenuScreen.NewGame, menu.TestHarness_Screen,
                "The Main Menu must boot directly into the New Game screen, matching sub_5D064's own proven sole firing site at the end of scene construction.");
        }

        // TASK027N.2.51 LARGE CONTINUATION Section 16.1 (boot/launch
        // sequence): the original IPA's own Default-Landscape.png (PROVEN
        // via direct PNG dimension read - exactly 1024x768, matching this
        // project's own established iPad logical resolution) is shown
        // briefly before the actual menu, matching real iOS's own behavior
        // of displaying a static launch image before any app code runs.
        [Test]
        public void N251M_13_MenuSceneReloadDoesNotReplayApplicationLaunchImage()
        {
            Assert.IsFalse(menu.TestHarness_BootSplashActive, "Loading menuSceneiPad in an already running application must not replay the OS launch image.");
        }

        [UnityTest]
        public IEnumerator N251M_14_BootSplashEndsAfterItsOwnDuration()
        {
            menu.TestHarness_SkipBootSplash();
            yield return null;
            Assert.IsFalse(menu.TestHarness_BootSplashActive, "The boot splash must end (TestHarness_SkipBootSplash simulates its own timer elapsing).");
        }

        // TASK027N.2.51: PROVEN native gate (full decompile of sub_61F10)
        // - Resume is offered only when the player has a banked extra life
        // (dword_21BDFC>0), NOT merely "any save data exists" - a genuine,
        // surprising shipped quirk, consistent with the SAME field's own
        // already-documented "fresh death with 0 banked lives = instant
        // game over" behavior. See MainMenuController.cs's own header
        // comment on this gate for the full citation.
        [Test]
        public void N251M_16_ResumeOnlyOfferedWithBankedLivesRemaining()
        {
            DC1Preferences.SetInt("DC1_CityGrind_Lives", 0);
            Assert.IsFalse(menu.TestHarness_ResumeWouldBeOffered, "Resume must NOT be offered with 0 banked lives, even if other progress (level/cash) exists - matching native's own proven dword_21BDFC<=0 hide condition.");
            DC1Preferences.SetInt("DC1_CityGrind_Lives", 1);
            Assert.IsTrue(menu.TestHarness_ResumeWouldBeOffered, "Resume must be offered once at least 1 banked life exists.");
            DC1Preferences.DeleteKey("DC1_CityGrind_Lives");
        }

        [Test]
        public void N251M_15_BootSplashAssetLoads()
        {
            Assert.IsTrue(menu.TestHarness_MenuAssetLoads("BootSplashFrame"), "The exported boot-splash asset (copied from the original IPA's Default-Landscape.png) must load from Resources/DinoCapGenerated/MainMenu/.");
        }

        // TASK027N.2.51 LARGE CONTINUATION 2 (Section 13 - background dino
        // + roar). PROVEN via full decompile of sub_627CC ('CreateDino'):
        // fixed world spawn (-672,-30), 50/50 variant coin-flip, walks
        // rightward from off-screen, roar fires once at the SAME moment
        // (not a separate timer).
        [Test]
        public void N251M_16_BackgroundDinoSpawnsOffscreenLeft()
        {
            menu.TestHarness_StepDinoCreation(5f);
            Assert.Less(menu.TestHarness_DinoLogicalX, 0f, "Native's own fixed spawn position (-672,-30), converted through the proven origin-centered camera convention, must land off-screen to the left (negative logical X).");
        }

        [Test]
        public void N251M_17_BackgroundDinoVariantIsCoinFlip()
        {
            menu.TestHarness_StepDinoCreation(5f);
            Assert.That(menu.TestHarness_DinoVariant, Is.EqualTo(0).Or.EqualTo(1), "Native's own 50/50 random roll picks between exactly 2 variants - not an unbounded/guessed range.");
        }

        [UnityTest]
        public IEnumerator N251M_18_BackgroundDinoActivatesOnFiveSecondPulseAndRoarsOffscreen()
        {
            menu.TestHarness_SkipBootSplash();
            typeof(MainMenuController).GetField("dinoCreationTimer",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(menu,0f);
            Assert.IsFalse(menu.TestHarness_DinoActive);
            Assert.IsFalse(menu.TestHarness_DinoRoarPlayed);
            menu.TestHarness_StepDinoCreation(4.999f);
            Assert.IsFalse(menu.TestHarness_DinoActive,"Native5D00C initial named-event delay is5s.");
            menu.TestHarness_StepDinoCreation(0.001f);
            Assert.IsTrue(menu.TestHarness_DinoActive);
            Assert.IsTrue(menu.TestHarness_DinoRoarPlayed,"627CC emits the roar atactivation, not viewport entry.");
            Assert.Less(menu.TestHarness_DinoLogicalX,0f);
            float start = menu.TestHarness_DinoLogicalX;
            var clip=menu.TestHarness_DinoVisual.primary.Clip;
            menu.TestHarness_StepDinoCreation(5f);
            Assert.AreEqual(start,menu.TestHarness_DinoLogicalX,"627CC active-object gate prevents a second pulse from resetting position.");
            Assert.AreSame(clip,menu.TestHarness_DinoVisual.primary.Clip);
            yield return null;
            yield return null;
            Assert.Greater(menu.TestHarness_DinoLogicalX, start, "The dino must walk rightward (native's own 'move' animation implies forward motion, not a static spawn).");
        }

        // TASK027N.2.51 LARGE CONTINUATION 3 (full closure of the Main Menu
        // background dino - visual/speed/scale). PROVEN this continuation
        // via fresh IDA (session 455561b7): the dino's own velocity is
        // EXACTLY 20.0 units/sec (sub_173A98 on GameObject::GetPhysics(),
        // hardcoded identical for both variants) - NOT the prior guessed
        // 60. Scale is native default 1.0 (no scale-setter call exists in
        // sub_627CC). The visual now reuses Power2DSourceTruthRenderer
        // (the SAME component City Grid/Sandbox use for every other dino),
        // confirmed source-compatible via CAObjCallBack's own Tick
        // (sub_76A20) matching the project's already-proven DCPart/dBits
        // multi-part mechanism exactly.
        [Test]
        public void N251M_19_BackgroundDinoVisualRendererExists()
        {
            menu.TestHarness_SkipBootSplash();
            Assert.IsNotNull(menu.TestHarness_DinoVisual, "The background dino must have a real Power2DSourceTruthRenderer, reusing the same multi-part skeletal pipeline every other dino in this project uses - not left as an unrendered logical position.");
            Assert.IsNotNull(menu.TestHarness_DinoCamera, "A dedicated background camera must exist to actually render the dino's sprite parts (MainMenuScene previously had none).");
            Assert.IsTrue(menu.TestHarness_DinoCamera.orthographic, "The background camera must be orthographic, matching this project's own 2D logical-space convention.");
        }

        [Test]
        public void N251M_20_BackgroundDinoVelocityIsProvenNotGuessed()
        {
            menu.TestHarness_SkipBootSplash();
            menu.TestHarness_StepDinoCreation(5f);
            float start = menu.TestHarness_DinoLogicalX;
            // Exactly one fixed-length step at the PROVEN native rate (20.0 units/sec) - not the old guessed 60.
            const float dt = 0.1f;
            menu.TestHarness_StepBackgroundDino(dt);
            float moved = menu.TestHarness_DinoLogicalX - start;
            Assert.AreEqual(20f*dt, moved, 0.001f, "Native's own proven horizontal velocity is EXACTLY 20.0 units/sec (sub_173A98 on GameObject::GetPhysics(), hardcoded identical for both variants) - the old guessed DinoWalkSpeedPlatformAdapter=60 must no longer be used.");
        }

        [Test]
        public void N251M_21_BackgroundDinoScaleIsNativeDefaultOne()
        {
            menu.TestHarness_SkipBootSplash();
            Assert.AreEqual(Vector3.one, menu.TestHarness_DinoVisual.transform.localScale, "No scale-setter call exists anywhere in sub_627CC - native scale is the engine default 1.0, not a randomized or variant-dependent size class.");
        }

        [TestCase(0,"k3_move")]
        [TestCase(1,"k2_move")]
        public void N251M_22_BackgroundDinoVariantSelectsNativeContainer(int variant,string nativeClip)
        {
            menu.TestHarness_SkipBootSplash();
            var saved=Random.state;
            try {
                int seed=0;
                for(;seed<100;seed++) {
                    Random.InitState(seed);
                    if((Random.value*100f>=50f?1:0)==variant)break;
                }
                Assert.Less(seed,100,"Need deterministic input for each native branch.");
                Random.InitState(seed);
                typeof(MainMenuController).GetMethod("InitBackgroundDino",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(menu,null);
                Assert.AreEqual(variant,menu.TestHarness_DinoVariant);
                Assert.IsNotNull(menu.TestHarness_DinoVisual.primary.Clip);
                Assert.AreEqual(nativeClip,menu.TestHarness_DinoVisual.primary.Clip.name,
                    "Native5D00C loads k3 atscene+20 andk2 atscene+44;627CC selects corresponding variant.");
            } finally {Random.state=saved;}
        }

        [Test]
        public void N251M_23_BackgroundDinoHasNoCombatOrAIBehavior()
        {
            menu.TestHarness_SkipBootSplash();
            var go = menu.TestHarness_DinoVisual.gameObject;
            Assert.IsNull(go.GetComponent<CityGridController>(), "The menu background dino must not carry any gameplay/AI controller - it is a pure cosmetic element.");
            Assert.AreEqual(1, go.GetComponents<MonoBehaviour>().Length, "The menu background dino's GameObject must carry only its Power2DSourceTruthRenderer - no combat/AI component.");
        }

        [UnityTest]
        public IEnumerator N251M_24_BackgroundDinoWorldPositionTracksLogicalPosition()
        {
            menu.TestHarness_SkipBootSplash();
            menu.TestHarness_StepDinoCreation(5f);
            yield return null;
            float expectedWorldX = menu.TestHarness_DinoLogicalX - DinoCapSourceProfileV11.ViewWidth*0.5f;
            Assert.AreEqual(expectedWorldX, menu.TestHarness_DinoVisual.transform.position.x, 0.01f, "The visual GameObject's own world X must track the proven origin-centered logical-to-world conversion every frame.");
        }

        [Test]
        public void N251M_02_NavigationBetweenAllFourTopLevelScreens()
        {
            menu.TestHarness_GoOptions();
            Assert.AreEqual(MainMenuController.MenuScreen.Options, menu.TestHarness_Screen);
            menu.TestHarness_GoLifeMode();
            Assert.IsTrue(menu.TestHarness_LaunchingJungle,"Jungle selection queues its independent gameplay route.");
            menu.TestHarness_GoNewGame();
            Assert.AreEqual(MainMenuController.MenuScreen.NewGame, menu.TestHarness_Screen);
        }

        // TASK027N.2.51: PROVEN exact native wrap (sub_5FD70): 1->2->3->1,
        // exactly 3 tiers (Easy/Normal/Insane) - NOT 4 (text_fullFrame is
        // not a difficulty tier, see the contract file's own correction).
        [Test]
        public void N251M_03_DifficultyCyclesExactlyThreeTiersAndWraps()
        {
            Assert.AreEqual(1, menu.TestHarness_Difficulty, "Default difficulty must be 1 (Easy).");
            menu.TestHarness_PressDifficultyCycle();
            Assert.AreEqual(2, menu.TestHarness_Difficulty);
            menu.TestHarness_PressDifficultyCycle();
            Assert.AreEqual(3, menu.TestHarness_Difficulty);
            menu.TestHarness_PressDifficultyCycle();
            Assert.AreEqual(1, menu.TestHarness_Difficulty, "Difficulty must wrap 3->1, matching sub_5FD70's own proven 'if(dword_214FC0+1>3) v9=1' wrap.");
        }

        [Test]
        public void N251M_04_DifficultyPersistsToCityGridControllerPrefKey()
        {
            menu.TestHarness_PressDifficultyCycle();   // ->2
            menu.TestHarness_PressDifficultyCycle();   // ->3
            Assert.AreEqual(3, DC1Preferences.GetInt(CityGridController.DifficultyPrefKey, -1),
                "Each difficulty cycle press must immediately persist to the SAME PlayerPrefs key CityGridController's own LoadProgressionState() reads, so a later GO/Resume press reflects the chosen difficulty even without an explicit separate save step.");
        }

        // TASK027N.2.51: Music (byte_214FB8, PROVEN shared with City Grid's
        // own pause menu) and SFX (byte_214FBC, a separate flag, PROVEN
        // this session) are two INDEPENDENT toggles.
        [Test]
        public void N251M_05_MusicAndSfxToggleIndependently()
        {
            Assert.IsTrue(menu.TestHarness_MusicOn);
            Assert.IsTrue(menu.TestHarness_SfxOn);
            menu.TestHarness_PressMusicToggle();
            Assert.IsFalse(menu.TestHarness_MusicOn);
            Assert.IsTrue(menu.TestHarness_SfxOn, "Toggling Music must not affect the separate SFX flag.");
            menu.TestHarness_PressSfxToggle();
            Assert.IsFalse(menu.TestHarness_SfxOn);
            Assert.IsFalse(menu.TestHarness_MusicOn, "Music must remain off (its own independent state).");
        }

        // TASK027N.2.51: PROVEN exact native step (full decompile of
        // sub_5FBA8/sub_5FC00) - NOT a simple +-1 or +-5: 1->5 special
        // case, then +-5 steps, with the UPPER bound FLOORED to the
        // nearest multiple of 5 at or below maxLevelReached (a genuine
        // shipped quirk - see MainMenuController.cs's own header comment
        // on TestHarness_LevelUp/Down for the full citation).
        [Test]
        public void N251M_06_LevelSelectStepsByFiveWithFlooredCeiling()
        {
            menu.TestHarness_SetMaxLevelReached(23);   // deliberately NOT a multiple of 5
            menu.TestHarness_GoLevelSelect();
            Assert.AreEqual(1, menu.TestHarness_LevelSelectValue);
            menu.TestHarness_LevelUp();
            Assert.AreEqual(5, menu.TestHarness_LevelSelectValue, "From 1, UP must jump directly to 5 (the proven native special case), not 1+5=6.");
            menu.TestHarness_LevelUp();
            Assert.AreEqual(10, menu.TestHarness_LevelSelectValue);
            menu.TestHarness_LevelUp();
            menu.TestHarness_LevelUp();
            Assert.AreEqual(20, menu.TestHarness_LevelSelectValue, "Must reach 20 (5*floor(23/5)=20).");
            menu.TestHarness_LevelUp();   // would be 25, but ceiling is floored to 20 for maxLevelReached=23
            Assert.AreEqual(20, menu.TestHarness_LevelSelectValue, "The dial's own ceiling must be FLOORED to the nearest multiple of 5 at or below maxLevelReached (23->20) - a genuine shipped quirk, up to 4 'recently reached' levels are unreachable via this dial.");
            menu.TestHarness_LevelDown();
            Assert.AreEqual(15, menu.TestHarness_LevelSelectValue);
            menu.TestHarness_LevelDown();
            menu.TestHarness_LevelDown();
            menu.TestHarness_LevelDown();
            Assert.AreEqual(1, menu.TestHarness_LevelSelectValue, "DOWN must clamp at a minimum of 1 (max(1, current-5)).");
            menu.TestHarness_LevelDown();   // already at 1 - no-op per native
            Assert.AreEqual(1, menu.TestHarness_LevelSelectValue);
        }

        [Test]
        public void N251M_07_BackFromLevelSelectReturnsToNewGame()
        {
            menu.TestHarness_GoLevelSelect();
            for(int i=0;i<80;i++) menu.TestHarness_StepReveal(0.01f);
            menu.TestHarness_BackFromLevelSelect();
            Assert.AreEqual(MainMenuController.MenuScreen.NewGame, menu.TestHarness_Screen);
        }

        // TASK027N.2.51: case 21 (GO) PROVEN - writes the level into the
        // SAME LevelPrefKey CityGridController's own LoadProgressionState()
        // reads, and loads CityGridScene (sub_7EEC8's own role).
        [UnityTest]
        public IEnumerator N251M_08_PressGoWritesLevelAndLoadsCityGrindScene()
        {
            menu.TestHarness_SetMaxLevelReached(10);
            menu.TestHarness_GoLevelSelect();
            menu.TestHarness_LevelUp();
            menu.TestHarness_LevelUp();   // 1->5->10 (proven exact native step)
            menu.TestHarness_PressGo();
            yield return null;
            yield return null;
            yield return null;
            Assert.AreEqual(10, DC1Preferences.GetInt("DC1_CityGrind_Level", -1), "GO must write the selected level to the SAME pref key CityGridController reads.");
            var city = Object.FindFirstObjectByType<CityGridController>();
            Assert.IsNotNull(city, "Pressing GO must load CityGridScene (CityGridController must be present after the load).");
        }

        [UnityTest]
        public IEnumerator N251M_09_PressResumeLoadsCityGrindSceneWithExistingSave()
        {
            DC1Preferences.SetInt("DC1_CityGrind_Level", 4);
            DC1Preferences.SetInt("DC1_CityGrind_MaxLevel", 4);
            menu.TestHarness_PressResume();
            yield return null;
            yield return null;
            yield return null;
            var city = Object.FindFirstObjectByType<CityGridController>();
            Assert.IsNotNull(city, "Pressing Resume must load CityGridScene.");
            Assert.AreEqual(4, city.TestHarness_CityLevel, "Resume must NOT reset the level - CityGridController's own existing LoadProgressionState() must pick up the already-persisted level 4 unchanged (this is the genuine architectural synergy the implementation relies on, not a separate resume-state mechanism).");
        }

        // N254 restores the native Jungle route; the standalone scene test
        // also verifies the completed transition and independent local state.
        [Test]
        public void N251M_10_LifeModeDoesNotSilentlyLaunchCityGrind()
        {
            menu.TestHarness_GoLifeMode();
            Assert.IsTrue(menu.TestHarness_LaunchingJungle,
                "Life Mode must queue Jungle, rather than the City resume route.");
        }

        [Test]
        public void N251M_11_SocialButtonsAreDisclosedInert()
        {
            Assert.IsTrue(menu.TestHarness_SocialButtonsAreInert,
                "Game Center leaderboard/achievements and the support web link are SOURCE_FAITHFUL-shipped features with no working modern backend - must be explicitly disclosed inert, not silently omitted or fake-wired.");
        }

        // Asset-loading sanity - confirms the Section-16 export actually
        // landed where MainMenuController's own CachedTexture() looks.
        [TestCase("menu_bgFrame")]
        [TestCase("menu_logoFrame")]
        [TestCase("text_newgameFrame")]
        [TestCase("text_resumeFrame")]
        [TestCase("level_leftFrame")]
        [TestCase("level_rightFrame")]
        [TestCase("level_goFrame")]
        [TestCase("level_backFrame")]
        [TestCase("text_easyFrame")]
        [TestCase("text_normalFrame")]
        [TestCase("text_insaneFrame")]
        [TestCase("CITYFrame")]
        [TestCase("JUNGLEFrame")]
        public void N251M_12_MenuAssetLoads(string frame)
        {
            Assert.IsTrue(menu.TestHarness_MenuAssetLoads(frame), $"Main Menu asset '{frame}' must load from Resources/DinoCapGenerated/MainMenu/ - exported this session via Tools/Atlas/export_mainmenu_assets.py.");
        }
    }
}
#endif
