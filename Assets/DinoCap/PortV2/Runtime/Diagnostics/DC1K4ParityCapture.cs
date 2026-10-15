#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoCap.PortV2
{
    public sealed partial class DC1ConvergenceCapture
    {
        [Serializable] sealed class K4ParityRows { public List<K4ParityRow> rows=new List<K4ParityRow>(); }
        [Serializable] sealed class K4ParityRow
        {
            public string stage;public Vector2 physicsPosition;public Vector3 groundPart;
            public float retainedAngle;public bool facingRight;public string scene;
        }
        IEnumerator CaptureK4Parity()
        {
            DC1DebugSettings.Enabled=true;
            var rows=new K4ParityRows();
            yield return SceneManager.LoadSceneAsync("MainMenuScene");yield return null;
            var settingsMenu=FindFirstObjectByType<MainMenuController>();settingsMenu.TestHarness_SkipBootSplash();
            settingsMenu.TestHarness_GoOptions();yield return new WaitForSeconds(.5f);
            yield return Save("mainmenu_settings_workbench_entry");
            yield return SceneManager.LoadSceneAsync("CombatSandboxV2");yield return null;yield return null;
            var sandbox=FindFirstObjectByType<CombatSandboxV2>();sandbox.TestHarness_ClearDinos();
            yield return Save("sandbox_full_mode_controls");
            sandbox.enabled=false;
            foreach(bool facingRight in new[]{false,true})
            {
                var go=new GameObject("Staged shared K4 ground projectile");
                var actor=go.AddComponent<JungleProjectileActor>();
                actor.Initialize(SourceTruthLoader.Load(),new Vector2(500,0),facingRight,Vector2.up*136);
                for(int i=0;i<180 && actor.State.Flying;i++)actor.Tick(1f/60f,10000,true,()=>true);
                if(actor.State.Flying)throw new InvalidOperationException("Ground projectile fixture did not land");
                yield return null;
                var visual=go.GetComponent<Power2DSourceTruthRenderer>();visual.enabled=false;
                visual.TryGetPartWorldAnchor("dyd1",out Vector3 anchor);
                rows.rows.Add(new K4ParityRow {stage=facingRight?"right":"left",physicsPosition=actor.State.Position,
                    groundPart=anchor,retainedAngle=visual.nativeRootRotationRadians,facingRight=visual.facingRight,
                    scene=SceneManager.GetActiveScene().name});
                if(anchor.y>=-236)throw new InvalidOperationException("Sandbox goo orbited above its native ground offset");
                yield return Save(facingRight?"sandbox_k4_ground_right":"sandbox_k4_ground_left");
                Destroy(go);yield return null;
            }
            SandboxModeSession.OpenCity();yield return null;yield return null;
            var menu=FindFirstObjectByType<MainMenuController>();
            if(menu.TestHarness_Screen!=MainMenuController.MenuScreen.LevelSelect)throw new InvalidOperationException("Sandbox City entry did not open native level selection");
            yield return new WaitForSeconds(.5f);yield return Save("sandbox_city_native_level_selection");
            menu.TestHarness_PressGo();yield return null;yield return null;
            if(FindFirstObjectByType<CityGridController>()==null)throw new InvalidOperationException("City mode owner missing");
            yield return Save("sandbox_full_city_mode");
            // Entry here starts from City only for the isolated diagnostic;
            // the user path leaves gameplay through its native pause menu.
            SandboxModeSession.OpenJungle();yield return null;
            float deadline=Time.realtimeSinceStartup+8;
            while(FindFirstObjectByType<JungleBlitzController>()==null && Time.realtimeSinceStartup<deadline)yield return null;
            if(FindFirstObjectByType<JungleBlitzController>()==null)throw new InvalidOperationException("Jungle mode owner missing");
            yield return null;yield return Save("sandbox_full_jungle_mode");
            SandboxModeSession.ReturnToWorkbench();yield return null;yield return null;
            if(FindFirstObjectByType<CombatSandboxV2>()==null)throw new InvalidOperationException("Workbench return failed");
            File.WriteAllText(Path.Combine(output,"k4_parity.json"),JsonUtility.ToJson(rows,true));
            File.WriteAllText(Path.Combine(output,"k4_parity_complete.json"),"{\"status\":\"PASS\",\"scope\":\"Staged shared projectile and real Sandbox mode-entry owners; isolated saves; native retained tilt\"}");
        }
    }
}
#endif
