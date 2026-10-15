#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N255SandboxModesRepro
    {
        IDisposable storage;
        DC1MenuPreferenceScope preferences;
        NativeModeSession.Snapshot session;
        [UnitySetUp] public IEnumerator SetUp()
        {
            storage=DC1Preferences.TestHarness_UseDirectory(Path.GetFullPath(Path.Combine(
                Application.dataPath,"../../Dev/Operator/DC1-MASTER/TestSaves",Guid.NewGuid().ToString("N"))));
            preferences=new DC1MenuPreferenceScope();session=NativeModeSession.Capture();SandboxModeSession.Reset();
            CityGrindStartPreset.ForLevel(10).WriteCommittedRun(10);
            DC1Preferences.SetInt(CityGridController.LivesPrefKey,2);
            DC1Preferences.SetInt("DC1_CityGrind_Cash",140);DC1Preferences.Save();
            yield return null;
        }
        [TearDown] public void TearDown()
        {
            SandboxModeSession.Reset();NativeModeSession.Restore(session);
            preferences?.Dispose();storage?.Dispose();GameplayRenderTarget.ReloadPreferences();
        }
        [UnityTest] public IEnumerator CitySelectionUsesNativeMenuWithoutOverwritingExistingRun()
        {
            SandboxModeSession.OpenCity();yield return null;yield return null;
            var menu=Object.FindFirstObjectByType<MainMenuController>();
            Assert.That(menu.TestHarness_Screen,Is.EqualTo(MainMenuController.MenuScreen.LevelSelect));
            Assert.That(DC1Preferences.GetInt(CityGridController.LivesPrefKey),Is.EqualTo(2));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.EqualTo(140));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Level"),Is.EqualTo(10));
            menu.TestHarness_PressResume();yield return null;yield return null;
            var city=Object.FindFirstObjectByType<CityGridController>();
            Assert.That(city,Is.Not.Null);Assert.That(Object.FindFirstObjectByType<JungleBlitzController>(),Is.Null);
            Assert.That(city.TestHarness_CashTotal,Is.EqualTo(140));
            Assert.That(DC1Preferences.GetInt(CityGridController.LivesPrefKey),Is.EqualTo(2));
            Assert.That(SandboxModeSession.Active,Is.True);
        }
        [UnityTest] public IEnumerator JungleSelectionUsesActualModeAndReturnPreservesCheckpoint()
        {
            NativeModeSession.Publish(321,2);
            SandboxModeSession.OpenJungle();yield return null;
            float deadline=Time.realtimeSinceStartup+8;
            while(Object.FindFirstObjectByType<JungleBlitzController>()==null && Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;
            Assert.That(Object.FindFirstObjectByType<JungleBlitzController>(),Is.Not.Null);
            var jungle=Object.FindFirstObjectByType<CityGridController>();
            Assert.That(jungle,Is.Not.Null);Assert.That(jungle.TestHarness_CashTotal,Is.EqualTo(321));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.EqualTo(140));
            SandboxModeSession.ReturnToWorkbench();yield return null;yield return null;
            Assert.That(Object.FindFirstObjectByType<CombatSandboxV2>(),Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<CityGridController>(),Is.Null);
            Assert.That(SandboxModeSession.Active,Is.False);
            Assert.That(DC1Preferences.GetInt(CityGridController.LivesPrefKey),Is.EqualTo(2));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.EqualTo(140));
        }
    }
}
#endif
