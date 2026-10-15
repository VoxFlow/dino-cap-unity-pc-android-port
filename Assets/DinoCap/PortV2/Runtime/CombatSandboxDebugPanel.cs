using UnityEngine;

namespace DinoCap.PortV2
{
    public sealed partial class CombatSandboxV2
    {
        void DrawSandboxQuickControls()
        {
            float scale=DC1DebugPanel.ScaleForHeight(Screen.height);
            float width=Mathf.Min(780,DC1DebugPanel.CurrentViewport(600).width/scale-24);
            DC1DebugPanel.Begin(ref debugPanelScroll,width,820,600,0,44);
            GUILayout.BeginArea(new Rect(8,8,width-16,800));
            var text=new GUIStyle(GUI.skin.label){wordWrap=true};
            var button=new GUIStyle(GUI.skin.button){wordWrap=true};
            GUILayout.Label(debugPanelTab==0?"PLAYER AND WEAPONS":"DINOSAUR AND SPAWNING",text);
            GUILayout.Label("HP "+playerHP.ToString("0")+" / "+data.combat.playerHP.ToString("0")+
                "   "+CurrentWeaponName()+"   Ammo "+familyAmmo[equippedFamily]+"   Cash "+money,text);
            GUILayout.Space(6);
            if(debugPanelTab==0)
            {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("GOD MODE: "+(godMode?"ON":"OFF"),button,GUILayout.Height(40)))godMode=!godMode;
                if(GUILayout.Button("INFINITE AMMO: "+(infiniteAmmo?"ON":"OFF"),button,GUILayout.Height(40)))infiniteAmmo=!infiniteAmmo;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("WEAPON FAMILY: "+WeaponFamilyTable.FamilyLetter(debugAbcdFamilySelect),button,GUILayout.Height(40)))debugAbcdFamilySelect=(debugAbcdFamilySelect+1)%4;
                if(GUILayout.Button("TIER: "+(debugAbcdTierSelect+1),button,GUILayout.Height(40)))debugAbcdTierSelect=(debugAbcdTierSelect+1)%WeaponFamilyTable.TierCount[debugAbcdFamilySelect];
                if(GUILayout.Button("EQUIP A-D",button,GUILayout.Height(40)))DebugApplyAbcdSelection(debugAbcdFamilySelect,debugAbcdTierSelect);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("F MODE: F"+(familyTier[5]+1),button,GUILayout.Height(40)))CycleFMode();
                if(GUILayout.Button("G TIER: G"+(meleeTier+1),button,GUILayout.Height(40)))CycleMeleeTier();
                GUILayout.EndHorizontal();
                GUILayout.Label("Use Advanced for hit reactions, X-ray tuning, frame timing and detailed combat telemetry.",text);
            }
            else
            {
                manualTypeIndex=GUILayout.SelectionGrid(manualTypeIndex,new[]{"T1 K3 BLUE","T2 K1 RUSHER","T3 K2 TREX","T4 K4 RAPTOR"},2,button,GUILayout.Height(80));
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("SIDE: "+spawnSide,button,GUILayout.Height(40)))spawnSide=spawnSide==SpawnSide.Left?SpawnSide.Right:SpawnSide.Left;
                if(GUILayout.Button("SPAWN DINOSAUR",button,GUILayout.Height(40)))SpawnDino(DinoSpecies[manualTypeIndex].kind,spawnSide);
                if(GUILayout.Button("CLEAR DINOSAURS",button,GUILayout.Height(40)))ClearDinos();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();GUILayout.Label("Selected dinosaur HP",text);
                debugEnemyHPText=DC1DebugTextInput.FieldLayout(debugEnemyHPText,new[]{GUILayout.Width(Mathf.Min(100,width*.25f)),GUILayout.Height(32)});
                if(GUILayout.Button("SET HP",button,GUILayout.Height(32)))SetDebugEnemyHPFromText();GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("AUTO SPAWN: "+(autoSpawn?"ON":"OFF"),button,GUILayout.Height(40)))autoSpawn=!autoSpawn;
                if(GUILayout.Button("AUTO T1-T3: "+(autoSpawn123?"ON":"OFF"),button,GUILayout.Height(40)))autoSpawn123=!autoSpawn123;
                GUILayout.EndHorizontal();
                GUILayout.Label("Active dinosaurs: "+ActiveDinoCount+". K4 remains a manual Sandbox choice; native Jungle scheduling is unchanged.",text);
            }
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("CORPSE CLEANUP: "+(DebugCorpseSettings.DespawnEnabled?"ON":"OFF"),button,GUILayout.Height(40)))DebugCorpseSettings.SetDespawnEnabled(!DebugCorpseSettings.DespawnEnabled);
            if(GUILayout.Button("PRESENT: "+GameplayPresentationCamera.GlobalModeName,button,GUILayout.Height(40)))GameplayPresentationCamera.CycleGlobalMode();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("INTERNAL: "+GameplayRenderTarget.InternalScaleName,button,GUILayout.Height(40)))GameplayRenderTarget.TogglePersistentSupersample();
            if(GUILayout.Button("FPS: "+DC1FramePacing.LimitName,button,GUILayout.Height(40)))DC1FramePacing.CycleLimit();
            if(GUILayout.Button("VIEW: "+(GameplayRenderTarget.IsSourceFaithful4x3?"4:3":"WIDESCREEN"),button,GUILayout.Height(40)))GameplayRenderTarget.TogglePersistentAspectMode();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("ADVANCED DIAGNOSTICS",button,GUILayout.Height(40))){debugPanelTab=2;debugPanelScroll=Vector2.zero;}
            GUILayout.EndHorizontal();
            GUILayout.Label("FULL GAME MODES",text);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("CITY GRIND",button,GUILayout.Height(40)))SandboxModeSession.OpenCity();
            if(GUILayout.Button("JUNGLE BLITZ",button,GUILayout.Height(40)))SandboxModeSession.OpenJungle();
            GUILayout.EndHorizontal();
            GUILayout.Label("Use the mode's pause menu to return to Main Menu, then select Combat Workbench. City GO starts a new run; Resume keeps the saved run.",text);
            GUILayout.Label(status,text);
            GUILayout.EndArea();DC1DebugPanel.End();
        }
    }
}
