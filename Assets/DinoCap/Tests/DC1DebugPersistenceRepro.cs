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
    // User decisions 2026-10-13: (1) one global shared persisted debug-panel hidden/shown flag that
    // is read at scene start (no auto-open on level start); (2) the Sandbox always has its debug UI
    // enabled regardless of the saved debug switch. Preferences are isolated by DC1FilePreferencesScope.
    public class DC1DebugPersistenceRepro
    {
        const string CityScenePath = "Assets/DinoCap/Scenes/CityGrindScene.unity";
        const string SandboxScenePath = "Assets/DinoCap/Scenes/CombatSandboxV2.unity";

        [TearDown]
        public void RestoreDefaults()
        {
            DC1DebugSettings.UiHidden = false;
            DC1DebugSettings.Enabled = true;   // the fixture scope's own baseline
        }

        static IEnumerator Load(string path)
        {
            EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null; yield return null;
        }

        [Test]
        public void HiddenFlagDefaultsToShownAndPersistsAcrossReload()
        {
            DC1Preferences.DeleteKey(DC1DebugSettings.UiHiddenKey);
            Assert.IsFalse(DC1DebugSettings.UiHidden, "Never-set default must be shown (current behavior).");
            DC1DebugSettings.UiHidden = true;
            DC1Preferences.TestHarness_Reload();
            Assert.IsTrue(DC1DebugSettings.UiHidden, "Hidden state must survive a preference reload (app restart).");
            DC1DebugSettings.UiHidden = false;
            DC1Preferences.TestHarness_Reload();
            Assert.IsFalse(DC1DebugSettings.UiHidden, "Shown state must survive too - either state persists.");
        }

        // Touch/resolution QoL: the hidden-panel restore chip scales with the panel scale and is never smaller than the old 60x18,
        // and the mobile scale rule only applies to the live screen height (explicit-height callers keep the 1080 rule).
        [Test]
        public void RestoreChipScalesWithResolutionAndStaysTappable()
        {
            Rect chip=DC1DebugPanel.RestoreChipRect();
            Assert.GreaterOrEqual(chip.width,60f);
            Assert.GreaterOrEqual(chip.height,18f);
            Assert.AreEqual(1f,DC1DebugPanel.ScaleForHeight(720f),0.001f);
            Assert.AreEqual(1.5f,DC1DebugPanel.ScaleForHeight(1620f),0.001f);
            Assert.AreEqual(2f,DC1DebugPanel.ScaleForHeight(4000f),0.001f);
        }

        [UnityTest]
        public IEnumerator CityGrindStartHonoursTheSavedHiddenState()
        {
            DC1DebugSettings.UiHidden = true;
            yield return Load(CityScenePath);
            var city = Object.FindFirstObjectByType<CityGridController>();
            Assert.IsTrue(city.TestHarness_DebugUiHidden, "A hidden panel must stay hidden when the scene (level) starts.");

            DC1DebugSettings.UiHidden = false;
            yield return Load(CityScenePath);
            city = Object.FindFirstObjectByType<CityGridController>();
            Assert.IsFalse(city.TestHarness_DebugUiHidden, "A shown panel must stay shown when the scene starts.");
        }

        [UnityTest]
        public IEnumerator SandboxStartHonoursTheSharedHiddenState()
        {
            DC1DebugSettings.UiHidden = true;
            yield return Load(SandboxScenePath);
            var sandbox = Object.FindFirstObjectByType<CombatSandboxV2>();
            var field = typeof(CombatSandboxV2).GetField("debugUiHidden",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, "debugUiHidden must stay a field (capture diagnostics set it by reflection).");
            Assert.IsTrue((bool)field.GetValue(sandbox), "Sandbox must read the same global flag City Grind uses.");
        }

        [UnityTest]
        public IEnumerator SandboxDebugIsAlwaysEnabledButOtherScenesStillFollowTheSwitch()
        {
            DC1DebugSettings.Enabled = false;
            yield return Load(SandboxScenePath);
            Assert.IsFalse(DC1DebugSettings.Enabled, "The saved switch itself is untouched.");
            Assert.IsTrue(DC1DebugSettings.EffectiveEnabled, "Sandbox debug UI/hotkeys are enabled regardless of the saved switch.");
            Assert.IsFalse(DebugCorpseSettings.DespawnEnabled == false && !DC1DebugSettings.EffectiveEnabled);

            yield return Load(CityScenePath);
            Assert.IsFalse(DC1DebugSettings.EffectiveEnabled, "City Grind still follows the saved switch.");
        }
    }
}
#endif
