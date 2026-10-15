#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoCap.PortV2
{
    public sealed partial class DC1ConvergenceCapture
    {
        IEnumerator CaptureDebugControls()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-save-directory")<0)
                throw new InvalidOperationException("Debug UI capture requires an isolated save directory.");
            DC1DebugSettings.Enabled=true;DC1DebugSettings.AffectsSaves=false;
            yield return SceneManager.LoadSceneAsync("MainMenuScene");
            yield return null;yield return null;
            FindFirstObjectByType<MainMenuController>().TestHarness_GoOptions();
            yield return new WaitForSeconds(.6f);yield return Save("debug_menu_settings");
            foreach(string scene in new[]{"CityGrindScene","JungleBlitzScene"})
            {
                yield return SceneManager.LoadSceneAsync(scene);yield return null;yield return null;
                var city=FindFirstObjectByType<CityGridController>();
                city.TestHarness_SkipLevelIntro();city.TestHarness_DebugUiHidden=false;
                yield return new WaitForSeconds(.1f);yield return Save("debug_"+scene);
                DC1DebugSettings.Enabled=false;
                yield return null;yield return Save("debug_off_"+scene);
                DC1DebugSettings.Enabled=true;
            }
            yield return SceneManager.LoadSceneAsync("CombatSandboxV2");yield return null;yield return null;
            var sandbox=FindFirstObjectByType<CombatSandboxV2>();
            typeof(CombatSandboxV2).GetField("debugHidden",BindingFlags.Instance|BindingFlags.NonPublic)?.SetValue(sandbox,false);
            yield return Save("debug_sandbox_player");
            typeof(CombatSandboxV2).GetField("debugPanelTab",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(sandbox,1);
            yield return null;yield return Save("debug_sandbox_dinosaur");
            File.WriteAllText(Path.Combine(output,"debug_ui_dimensions.json"),
                "{\"scope\":\"Real player OnGUI screenshots in isolated storage; actual host window dimensions, no physical spanning claim\",\"width\":"+Screen.width+",\"height\":"+Screen.height+",\"panel_width\":"+DC1DebugPanel.CurrentViewport(460).width+",\"panel_height\":"+DC1DebugPanel.CurrentViewport(460).height+"}");
        }
    }
}
#endif
