#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N255SavePolicyRepro
    {
        IDisposable storage;
        CityGridController core;
        [UnitySetUp] public IEnumerator SetUp()
        {
            storage=DC1Preferences.TestHarness_UseDirectory(System.IO.Path.GetFullPath(System.IO.Path.Combine(
                Application.dataPath,"../../Dev/Operator/DC1-MASTER/TestSaves",Guid.NewGuid().ToString("N"))));
            DC1DebugSettings.Enabled=true;DC1DebugSettings.AffectsSaves=true;
            CityGrindStartPreset.ForLevel(1).WriteCommittedRun(1);DC1Preferences.Save();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            core=UnityEngine.Object.FindFirstObjectByType<CityGridController>();
            core.enabled=false;core.TestHarness_SkipLevelIntro();
        }
        [TearDown] public void TearDown()=>storage.Dispose();
        [Test] public void CashAmmoEditsCommitAtNativeCheckpointAndSurviveStorageReload()
        {
            int initial=DC1Preferences.GetInt(CityGrindStartPreset.AmmoPrefPrefix+0);
            core.TestHarness_DebugCashSet(321);core.TestHarness_DebugDepleteAllAmmo();
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.Zero);
            Assert.That(DC1Preferences.GetInt(CityGrindStartPreset.AmmoPrefPrefix+0),Is.EqualTo(initial));
            core.TestHarness_SaveProgressionState();DC1Preferences.TestHarness_Reload();
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.EqualTo(321));
            Assert.That(DC1Preferences.GetInt(CityGrindStartPreset.AmmoPrefPrefix+0),Is.Zero);
        }
        [Test] public void IsolatedPolicyDoesNotCommitDebugCashAmmo()
        {
            DC1DebugSettings.AffectsSaves=false;
            core.TestHarness_DebugCashSet(321);core.TestHarness_DebugDepleteAllAmmo();
            core.TestHarness_SaveProgressionState();DC1Preferences.TestHarness_Reload();
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.Zero);
            Assert.That(DC1Preferences.GetInt(CityGrindStartPreset.AmmoPrefPrefix+0),Is.EqualTo(200));
        }
        [Test] public void DebugSkipPersistsProgressionAndPreservesConsumedLives()
        {
            core.TestHarness_SetLives(2);
            core.TestHarness_SetLevelInputText("999999");core.TestHarness_CommitLevelInputViaUI();
            Assert.That(core.TestHarness_CityLevel,Is.EqualTo(999999));
            Assert.That(core.TestHarness_Lives,Is.EqualTo(2));
            DC1Preferences.TestHarness_Reload();
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Level"),Is.EqualTo(999999));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_MaxLevel"),Is.EqualTo(999999));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Lives"),Is.EqualTo(2));
        }
    }
}
#endif
