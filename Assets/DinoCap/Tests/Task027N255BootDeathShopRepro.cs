#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N255BootDeathShopRepro
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        IDisposable storage;
        DC1MenuPreferenceScope preferences;
        CityGridController core;
        [UnitySetUp] public IEnumerator SetUp()
        {
            storage=DC1Preferences.TestHarness_UseDirectory(Path.GetFullPath(Path.Combine(
                Application.dataPath,"../../Dev/Operator/DC1-MASTER/TestSaves",Guid.NewGuid().ToString("N"))));
            preferences=new DC1MenuPreferenceScope();DC1DebugSettings.AffectsSaves=true;
            CityGrindStartPreset.ForLevel(1).WriteCommittedRun(3);DC1Preferences.Save();
            GameplayRenderTarget.ReloadPreferences();
            yield return null;
        }
        [TearDown] public void TearDown()
        {
            core?.TestHarness_ResetCameraAspect();preferences?.Dispose();storage?.Dispose();
            GameplayRenderTarget.ReloadPreferences();NativeAudioSessionGate.StopAll();
        }
        IEnumerator LoadCity()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            core=Object.FindFirstObjectByType<CityGridController>();core.TestHarness_SkipLevelIntro();core.TestHarness_ClearDinos();
        }
        [UnityTest] public IEnumerator BootLogoHasNoMenuVoiceAndMenuReturnStartsImmediately()
        {
            typeof(MainMenuController).GetField("initialApplicationScene",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            var menu=Object.FindFirstObjectByType<MainMenuController>();
            Assert.That(menu.TestHarness_BootSplashActive,Is.True);
            Assert.That(menu.TestHarness_MenuMusicSource,Is.Null,"Menu scene audio must not start under the launch logo.");
            menu.TestHarness_SkipBootSplash();yield return null;
            Assert.That(menu.TestHarness_MenuMusicSource,Is.Not.Null);
            Assert.That(menu.TestHarness_MenuMusicSource.isPlaying,Is.True);
            typeof(MainMenuController).GetField("initialApplicationScene",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,false);
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            menu=Object.FindFirstObjectByType<MainMenuController>();
            Assert.That(menu.TestHarness_BootSplashActive,Is.False);
            Assert.That(menu.TestHarness_MenuMusicSource.isPlaying,Is.True,"Gameplay return has no launch-logo gate.");
        }
        [UnityTest] public IEnumerator DeathRestoresCheckpointWithoutReplacingOldHudBeforeReload()
        {
            DC1Preferences.SetInt("DC1_CityGrind_Cash",140);
            DC1Preferences.SetInt(CityGrindStartPreset.AmmoPrefPrefix+0,200);
            DC1Preferences.SetFloat(CityGrindStartPreset.HealthPrefKey,70);DC1Preferences.Save();
            yield return LoadCity();
            core.TestHarness_SetCashTotal(777);core.TestHarness_GrantAmmo(0,17-core.TestHarness_AmmoFor(0));
            core.TestHarness_DebugInstantDeath();float deadline=Time.realtimeSinceStartup+8;
            while(!core.TestHarness_DeathUIActive && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(core.TestHarness_DeathUIActive,Is.True);
            Assert.That(core.TestHarness_CashTotal,Is.EqualTo(140));
            Assert.That(core.TestHarness_AmmoFor(0),Is.EqualTo(200));
            Assert.That(core.TestHarness_DisplayedHud,Is.EqualTo(new Vector4(0,777,17,0)),
                "Native retained controls show the dying scene's values, independently of restored globals.");
            var old=core;
            while(old!=null && Time.realtimeSinceStartup<deadline)
            {
                Assert.That(old.TestHarness_DisplayedHud,Is.EqualTo(new Vector4(0,777,17,0)));
                yield return null;
            }
            Assert.That(old==null,Is.True);yield return null;
            core=Object.FindFirstObjectByType<CityGridController>();
            Assert.That(core.TestHarness_DisplayedHud,Is.EqualTo(new Vector4(100,140,200,0)));
            Assert.That(DC1Preferences.GetFloat(CityGrindStartPreset.HealthPrefKey),Is.EqualTo(70),"Full respawn health is not a rewritten checkpoint.");
        }
        [UnityTest] public IEnumerator ShopFadeStartsAtThresholdAndWalkContinuesPastRootEdge()
        {
            yield return LoadCity();core.enabled=false;
            foreach(float aspect in new[]{4f/3f,3f/2f,16f/10f,16f/9f})
            {
                core.TestHarness_SetCameraAspect(aspect);core.TestHarness_TriggerLevelClear();
                core.TestHarness_SetPlayerX(CityGridController.ShopFinishX+.01f);core.TestHarness_UpdateCamera(.01f);
                typeof(CityGridController).GetField("forcedWalkActive",Private).SetValue(core,false);
                core.TestHarness_TickForcedWalkExit(.01f);
                Assert.That(core.TestHarness_ForcedWalkFading,Is.True,"7334C enters fade at the native finish threshold, not at a guessed screen cutoff.");
                float right=core.TestHarness_CameraLeftX+core.TestHarness_CurrentViewWidth;
                core.TestHarness_SetPlayerX(right+1f);core.TestHarness_TickForcedWalkExit(.1f);
                Assert.That(core.TestHarness_PlayerX,Is.EqualTo(right+21.5f).Within(.01),"Walking continues during fade even after root crosses the screen edge.");
                core.TestHarness_SetPlayerX(right+51f);core.TestHarness_TickForcedWalkExit(.1f);
                Assert.That(core.TestHarness_PlayerX,Is.EqualTo(right+51f).Within(.01),"65E2C stops at the proven root>cameraRight+50 test.");
                Assert.That(core.TestHarness_ForcedWalkFadeTimer,Is.EqualTo(.21f).Within(.001));
                core.TestHarness_SkipForcedWalkExit();
            }
        }
        [Test] public void TwoXIsFreshAndResetDefaultButManualOneXPersists()
        {
            Assert.That(GameplayRenderTarget.Supersample2xEnabled,Is.True);
            GameplayRenderTarget.TogglePersistentSupersample();
            DC1Preferences.TestHarness_Reload();GameplayRenderTarget.ReloadPreferences();
            Assert.That(GameplayRenderTarget.Supersample2xEnabled,Is.False);
            DC1Preferences.ResetAll();GameplayRenderTarget.ReloadPreferences();
            Assert.That(GameplayRenderTarget.Supersample2xEnabled,Is.True);
            GameplayRenderTarget.TogglePersistentSupersample();GameplayRenderTarget.TogglePersistentSupersample();
            DC1Preferences.TestHarness_Reload();GameplayRenderTarget.ReloadPreferences();
            Assert.That(GameplayRenderTarget.Supersample2xEnabled,Is.True);
        }
    }
}
#endif
