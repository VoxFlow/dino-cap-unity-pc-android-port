#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N254JungleMenuRepro
    {
        DC1MenuPreferenceScope preferences;
        [SetUp]public void SetUp()=>preferences=new DC1MenuPreferenceScope();
        [TearDown]public void TearDown(){preferences.Dispose();preferences=null;}
        [Test]
        public void JungleLeaveWaitsForControl26RatherThanResumeControl3()
        {
            var jungle=new NativeMenuPrimaryLeave(4);var resume=new NativeMenuPrimaryLeave();
            for(int i=0;i<60;i++){jungle.Tick(.01f);resume.Tick(.01f);}
            Assert.That(resume.Active,Is.False);Assert.That(jungle.Active,Is.True);
            for(int i=0;i<40;i++)jungle.Tick(.01f);
            Assert.That(jungle.Active,Is.False);
        }
        [UnityTest]
        public IEnumerator JungleTitleLeavesDuringSelectionBeforeOtherControls()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            var menu=Object.FindFirstObjectByType<MainMenuController>();
            menu.TestHarness_SkipBootSplash();menu.TestHarness_StepReveal(1);
            menu.TestHarness_BeginJungleSelection();menu.TestHarness_StepReveal(.05f);
            Assert.That(menu.TestHarness_SelectionVisible,Is.True);
            Assert.That(menu.TestHarness_LaunchingJungle,Is.False);
            Assert.That(menu.TestHarness_RevealRect(0).Value.center.y,Is.EqualTo(-612).Within(.01f));
            Assert.That(menu.TestHarness_RevealRect(4).Value.center.y,Is.EqualTo(350).Within(.01f));
        }
        [UnityTest]
        public IEnumerator ActualMenuLaunchUsesJungleSceneAndLiveCarryWithoutCitySave()
        {
            DC1Preferences.SetInt("DC1_CityGrind_Cash",9876);
            DC1Preferences.SetInt(CityGridController.LivesPrefKey,7);
            NativeModeSession.Publish(321,2);
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            var menu=Object.FindFirstObjectByType<MainMenuController>();
            menu.TestHarness_SkipBootSplash();menu.TestHarness_GoLifeMode();
            float deadline=Time.realtimeSinceStartup+5;
            while(Object.FindFirstObjectByType<JungleBlitzController>()==null && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("JungleBlitzScene"));
            var core=Object.FindFirstObjectByType<CityGridController>();
            yield return null;
            Assert.That(core.TestHarness_IsJungle,Is.True);
            Assert.That(core.TestHarness_BgmClipName,Is.EqualTo("Music_Bg2"));
            Assert.That(core.TestHarness_JungleCash,Is.EqualTo(321));
            Assert.That(core.TestHarness_JungleLives,Is.EqualTo(2));
            Assert.That(core.TestHarness_RangedFamilyTier(0),Is.EqualTo(3));
            Assert.That(core.TestHarness_AmmoFor(0),Is.EqualTo(999));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.EqualTo(9876));
            Assert.That(DC1Preferences.GetInt(CityGridController.LivesPrefKey),Is.EqualTo(7));
            Assert.That(GameObject.Find("CityLevelWorld_1"),Is.Null);
        }
    }
}
#endif
