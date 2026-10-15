#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public class DC1NativeMenuBannerRepro
    {
        DC1MenuPreferenceScope preferences;
        [SetUp] public void SetUp(){preferences=new DC1MenuPreferenceScope();}
        [TearDown] public void TearDown(){preferences?.Dispose();preferences=null;}
        [TestCase(1024,768)]
        [TestCase(1920,1080)]
        [TestCase(1920,1200)]
        [TestCase(1620,1080)]
        [TestCase(2560,1440)] [TestCase(3840,2160)]
        [TestCase(3440,1440)] [TestCase(5120,1440)] [TestCase(5760,1080)]
        public void AdaptiveMenuUsesWholeHeightAndWidthWithoutStretchingNativeControls(int width,int height)
        {
            Rect view=MainMenuController.MenuPresentationRect(width,height,GameplayRenderTarget.PresentationAspectMode.WidescreenExpand);
            // Ceil-to-native-texel backing can leave less than one logical
            // pixel of vertical containment slack (over1 display pixel at4K).
            Assert.AreEqual(height,view.height,Mathf.Max(1f,height/768f));
            Assert.AreEqual(width,view.width,1f);
            int logical=GameplayRenderTarget.ComputeLogicalWidth(1024,768,width,height,GameplayRenderTarget.PresentationAspectMode.WidescreenExpand);
            Assert.AreEqual(view.width/logical,view.height/768f,0.00001f);
        }
        [Test]
        public void WideUiPreservesAuthoredDimensionsAndPointerAlignment()
        {
            Rect view=new Rect(0,0,1920,1080);
            Rect shot=new Rect(924,532,75,76);
            Rect screen=NativeViewport.Map(shot,view,NativeViewport.Anchor.Right);
            Assert.AreEqual(new Rect(1779.375f,748.125f,105.46875f,106.875f),screen);
            Rect hp=NativeViewport.Map(new Rect(84,8,270,72),view,NativeViewport.Anchor.Left);
            Assert.AreEqual(new Rect(118.125f,11.25f,379.6875f,101.25f),hp);
            Rect band=NativeViewport.Map(new Rect(300,297,446,101),view);
            Assert.AreEqual(661.875f,band.x);
            // ScreenToLogical is the existing adapter's actual screen input
            // path. The inverse display rect must capture its clicked center.
            Assert.IsTrue(GameplayPresentationCamera.ScreenToLogical(screen.center,view,1024,768,out var logical));
            Rect target=NativeViewport.PointerRect(shot,view,NativeViewport.Anchor.Right);
            var pointer=new SyntheticPointerLayer();pointer.RegisterControl("shoot",target);
            pointer.InjectDown(logical);pointer.Update(view,1024,768,false);
            Assert.IsTrue(pointer.WasControlPressedThisFrame("shoot"));
            pointer.CancelAll();pointer.InjectDown(new Vector2(target.x-1,target.center.y));
            pointer.Update(view,1024,768,false);
            Assert.IsFalse(pointer.IsControlPressed("shoot"));
        }

        [Test]
        public void SourceFourByThreeUiIsUnchangedIncludingLetterboxOrigin()
        {
            Rect view=new Rect(120,30,1024,768),native=new Rect(924,532,75,76);
            foreach(NativeViewport.Anchor anchor in System.Enum.GetValues(typeof(NativeViewport.Anchor))) {
                Assert.AreEqual(new Rect(1044,562,75,76),NativeViewport.Map(native,view,anchor));
                Assert.AreEqual(native,NativeViewport.PointerRect(native,view,anchor));
            }
        }

        [Test]
        public void NativeMoveControlClampsAndCarriesPhaseOvershootToNextTick()
        {
            // Source CITY:39.8 ->437.8 ->398, durations0.2/0.05.
            var t=new NativeUiRectTween(new Vector4(500,450,39.8f,11.5f),
                new Vector4(500,450,437.8f,126.5f),new Vector4(500,450,398,115),0.2f,0.05f);
            t.Tick(0.225f);
            Assert.AreEqual(437.8f,t.Rect.width,0.0001f);
            Assert.IsTrue(t.Active);
            t.Tick(0.03f);
            Assert.AreEqual(398f,t.Rect.width,0.0001f);
            Assert.IsFalse(t.Active);
            Assert.AreEqual(301f,t.Rect.x,0.0001f);
        }

        [Test]
        public void NativeLevelControlRetainsStrictBoundaryAndExtrapolation()
        {
            var t=new NativeUiRectTween(new Vector4(1547,347.5f,446,101),
                new Vector4(421,347.5f,446,101),new Vector4(523,347.5f,446,101),0.2f,0.05f,true);
            t.Tick(0.2f);
            Assert.AreEqual(421f,t.Value.x,0.001f);
            t.Tick(0.01f);
            Assert.Less(t.Value.x,421f,"69444 uses strict >1 and renders the extrapolated first-phase value.");
            t.Tick(0.01f);
            Assert.AreEqual(461.8f,t.Value.x,0.002f);
        }

        [Test]
        public void LevelIntroBuildsBlackBandThenCenteredLabelAndLeavesAfterNativeHold()
        {
            var intro=new NativeLevelIntro();
            Assert.AreEqual(0f,intro.Band.Rect.height);
            for(int i=0;i<30;i++)intro.Tick(0.01f);
            Assert.IsTrue(intro.LabelVisible);
            Assert.AreEqual(192f,intro.Band.Rect.y,0.001f);
            for(int i=0;i<30;i++)intro.Tick(0.01f);
            Assert.AreEqual(NativeLevelIntro.Stage.Hold,intro.CurrentStage);
            // Rest coordinates from initializer446x101 and699FC, independently
            // of the old incorrectly copied1024x300 /812,447 claims.
            Assert.AreEqual(300f,intro.Label.Rect.x,0.02f);
            Assert.AreEqual(297f,intro.Label.Rect.y,0.001f);
            for(int i=0;i<130;i++)intro.Tick(0.01f);
            Assert.IsFalse(intro.Active);
            Assert.AreEqual(0f,intro.Band.Rect.height,0.001f);
        }

        [UnityTest]
        public IEnumerator RealMenuRevealsAllThreeModesWithSourceGeometryAndBlackTint()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null;
            var menu=Object.FindFirstObjectByType<MainMenuController>();
            menu.enabled=false;
            menu.TestHarness_ResetReveal();
            Assert.IsNull(menu.TestHarness_RevealRect(1));
            menu.TestHarness_StepReveal(0.125f);
            Assert.IsNotNull(menu.TestHarness_RevealRect(1));
            Assert.IsNull(menu.TestHarness_RevealRect(2));
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            Assert.AreEqual(new Rect(301,392.5f,398,115),menu.TestHarness_RevealRect(1).Value);
            Assert.AreEqual(new Rect(361,492.5f,278,115),menu.TestHarness_RevealRect(2).Value);
            Assert.AreEqual(new Rect(276.5f,295,447,110),menu.TestHarness_RevealRect(4).Value);
            menu.TestHarness_StepDinoCreation(5f);
            yield return null;
            Assert.AreEqual(0f,menu.TestHarness_DinoVisual.actorColourScale);
            int visibleParts=0;
            foreach(var sr in menu.TestHarness_DinoVisual.GetComponentsInChildren<SpriteRenderer>())
                if(sr.enabled) {
                    visibleParts++;
                    Assert.AreEqual(0f,sr.color.r,0.0001f);
                    Assert.AreEqual(0f,sr.color.g,0.0001f);
                    Assert.AreEqual(0f,sr.color.b,0.0001f);
                }
            Assert.Greater(visibleParts,0,"A delayed native activation must render real parts for the tint check.");
        }

        [UnityTest]
        public IEnumerator RealCityIntroIsTransientAndClearUsesTheActualNativeAsset()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            var city=Object.FindFirstObjectByType<CityGridController>();
            Assert.IsTrue(city.TestHarness_LevelIntroActive);
            float startX=city.TestHarness_PlayerX;
            float startHp=city.TestHarness_PlayerHP;
            int startAmmo=city.TestHarness_AmmoFor(0);
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            foreach(string action in new[]{"RequestMoveLeft","RequestMelee","RequestShoot","RequestNextWeapon","RequestPause"})
                typeof(CityGridController).GetMethod(action,flags).Invoke(city,null);
            Assert.AreEqual(0,city.TestHarness_EquippedRangedFamily);
            Assert.IsFalse(city.TestHarness_SpawnTimersPending);
            yield return new WaitForSeconds(0.2f);
            Assert.Greater(city.TestHarness_PlayerX,startX,"Source RIGHT flag automatically walks the intro player.");
            Assert.AreEqual(startHp,city.TestHarness_PlayerHP);
            Assert.AreEqual(startAmmo,city.TestHarness_AmmoFor(0));
            Assert.IsFalse(city.TestHarness_SpawnTimersPending);
            yield return new WaitForSeconds(2f);
            Assert.IsFalse(city.TestHarness_LevelIntroActive);
            Assert.IsNotNull(Resources.Load<Texture2D>("DinoCapGenerated/CityGrind/levelFrame"));
            Assert.IsNotNull(Resources.Load<Texture2D>("DinoCapGenerated/CityGrind/text_clearFrame"));
            typeof(CityGridController).GetMethod("BeginForcedWalkExit",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)
                .Invoke(city,new object[]{1});
            Assert.AreEqual(new Rect(0,0,1024,768),city.TestHarness_LevelClearRect.Value);
            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(new Rect(126,319.5f,772,129),city.TestHarness_LevelClearRect.Value);
            Assert.IsTrue(city.TestHarness_ForcedWalkFading,"Source clear-control completion starts the black fade before a screen-edge walk check.");
        }

        [Test]
        public void LevelSelectUsesMenuContainerAndLabelCompletionBeforeBandLeave()
        {
            var select=new NativeMenuLevelSelect();
            Assert.AreEqual(new Rect(0,342,1024,0),select.Band.Rect);
            Assert.IsFalse(select.Interactive);
            for(int i=0;i<80;i++)select.Tick(0.01f);
            Assert.IsTrue(select.Interactive);
            Assert.AreEqual(new Rect(0,192,1024,300),select.Band.Rect);
            for(int i=0;i<5;i++) {
                var actual=select.Controls[i].Rect;
                Assert.AreEqual(NativeMenuLevelSelect.Rest[i].x,actual.x,0.01f);
                Assert.AreEqual(NativeMenuLevelSelect.Rest[i].y,actual.y,0.01f);
                Assert.AreEqual(NativeMenuLevelSelect.Rest[i].size,actual.size);
            }
            select.BeginLeave();select.Tick(0.025f);
            Assert.Greater(select.Controls[0].Rect.x,300);
            Assert.Less(select.Controls[4].Rect.x,870);
            Assert.IsFalse(select.Interactive);
            for(int i=0;i<80;i++)select.Tick(0.01f);
            Assert.IsFalse(select.Visible);
            Assert.AreEqual(0,select.Band.Rect.height);
        }

        [UnityTest]
        public IEnumerator ActualCitySelectionCanReturnAndSelectAgain()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;
            var menu=Object.FindFirstObjectByType<MainMenuController>();
            menu.enabled=false;menu.TestHarness_ResetReveal();
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            menu.TestHarness_SelectCity();
            for(int i=0;i<110;i++)menu.TestHarness_StepReveal(0.01f);
            Assert.IsTrue(menu.TestHarness_LevelPresentation.Interactive);
            menu.TestHarness_BackFromLevelSelect();
            Assert.IsFalse(menu.TestHarness_PrimarySelectionEnabled);
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            Assert.IsTrue(menu.TestHarness_PrimarySelectionEnabled);
            Assert.IsNull(menu.TestHarness_LevelPresentation);
            menu.TestHarness_SelectCity();
            for(int i=0;i<110;i++)menu.TestHarness_StepReveal(0.01f);
            Assert.IsTrue(menu.TestHarness_LevelPresentation.Interactive);
            for(int d=0;d<10;d++)
                Assert.IsNotNull(Resources.Load<Texture2D>("DinoCapGenerated/CityGrind/level"+d+"Frame"));
        }

        [UnityTest]
        public IEnumerator OptionsRowsSlideAndReturnAfterNativeBackCompletion()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;
            var menu=Object.FindFirstObjectByType<MainMenuController>();menu.enabled=false;
            menu.TestHarness_GoOptions();
            Assert.AreEqual(1159,menu.TestHarness_OptionsRect(0).x);
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            Assert.AreEqual(new Rect(135,165,322,53),menu.TestHarness_OptionsRect(0));
            Assert.AreEqual(new Rect(190,312,187,53),menu.TestHarness_OptionsRect(1));
            Assert.AreEqual(new Rect(243,446,72,54),menu.TestHarness_OptionsRect(2));
            Assert.IsTrue(menu.TestHarness_MenuAssetLoads("text_onFrame"));
            Assert.IsTrue(menu.TestHarness_MenuAssetLoads("text_offFrame"));
            while(menu.TestHarness_Difficulty!=2)menu.TestHarness_PressDifficultyCycle();
            Assert.AreEqual(new Rect(648,165,250,53),menu.TestHarness_OptionsRect(3));
            menu.TestHarness_BackFromOptions();
            Assert.IsTrue(menu.TestHarness_OptionsLeaving);
            Assert.AreEqual(MainMenuController.MenuScreen.Options,menu.TestHarness_Screen);
            for(int i=0;i<80;i++)menu.TestHarness_StepReveal(0.01f);
            Assert.AreEqual(MainMenuController.MenuScreen.NewGame,menu.TestHarness_Screen);
        }
    }
}
#endif
