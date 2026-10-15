using UnityEngine;

namespace DinoCap.PortV2
{
    public sealed partial class CityGridController
    {
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
        string debugCashInput="0";
        void DebugSetRuntimeCash(int amount)
        {
            debugTestLaunch=!DC1DebugSettings.AffectsSaves;
            cashTotal=Mathf.Clamp(amount,0,NativeCityDinoReward.CashCap);
            status="DEBUG cash="+cashTotal+"; "+(debugTestLaunch?"isolated run":"native checkpoint saves enabled");
        }
        void DebugAddRuntimeCash(int amount)
        {
            long next=(long)cashTotal+amount;
            DebugSetRuntimeCash((int)System.Math.Max(0,System.Math.Min(next,NativeCityDinoReward.CashCap)));
        }
        void DebugInstantPlayerDeath()
        {
            if(playerDead || cityGameOver)return;
            debugTestLaunch=!DC1DebugSettings.AffectsSaves;
            paused=false;Power2DSourceTruthRenderer.GlobalPaused=false;
            playerHP=0;ResetSustainedFireFaceInstant();
            // Same lethal state machine as68148 and Jungle projectile death;
            // explicit debug death bypasses God/invulnerability admission.
            StartCoroutine(PlayerDeathSequence());
        }
        void DebugDepleteAllAmmo()
        {
            debugTestLaunch=!DC1DebugSettings.AffectsSaves;
            EndF2FireImmediate();EndF3FireImmediate();
            for(int family=0;family<6;family++)itemStock[family]=0;
            SyncAmmoHudEvent();
            status="DEBUG all ranged ammo depleted; "+(debugTestLaunch?"isolated run":"native checkpoint saves enabled");
        }
        void DrawRuntimeDebugActions()
        {
            float y=jungle==null?252:342;
            if(GUI.Button(new Rect(10,y,160,26),"DEBUG INSTANT DEATH"))DebugInstantPlayerDeath();
            GUI.Label(new Rect(175,y,50,26),"CASH:");
            debugCashInput=DC1DebugTextInput.Field(new Rect(225,y,95,26),debugCashInput);
            if(GUI.Button(new Rect(325,y,70,26),"SET") && int.TryParse(debugCashInput,out int set))DebugSetRuntimeCash(set);
            if(GUI.Button(new Rect(400,y,70,26),"ADD") && int.TryParse(debugCashInput,out int add))DebugAddRuntimeCash(add);
            if(GUI.Button(new Rect(475,y,100,26),"RESET TO 0"))DebugSetRuntimeCash(0);
            if(GUI.Button(new Rect(580,y,180,26),"DEPLETE ALL AMMO"))DebugDepleteAllAmmo();
            GUI.Label(new Rect(10,y+28,760,22),DC1DebugSettings.AffectsSaves?"Debug edits use normal native checkpoint/death saves.":"Isolated debug run: campaign and cross-mode saves paused.");
            if(GUI.Button(new Rect(10,y+54,285,26),"DEBUG AFFECTS SAVES: "+(DC1DebugSettings.AffectsSaves?"ON":"OFF")))
            {
                DC1DebugSettings.AffectsSaves=!DC1DebugSettings.AffectsSaves;
                debugTestLaunch=!DC1DebugSettings.AffectsSaves;
            }
            DrawDebugResetSave(new Rect(305,y+54,260,26));
        }
        bool confirmResetSave;
        void DrawDebugResetSave(Rect rect)
        {
            if(!GUI.Button(rect,confirmResetSave?"CONFIRM RESET ALL SAVES":"RESET ALL SAVES"))return;
            if(!confirmResetSave){confirmResetSave=true;return;}
            DC1Preferences.ResetAll();
            pendingDeathReplay=null;NativeModeSession.Restore(default);
            GameplayRenderTarget.ReloadPreferences();
            NativeAudioSessionGate.SaveMusicPreference(false);NativeAudioSessionGate.SaveFxPreference(false);DC1Preferences.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
        }
        internal void TestHarness_DebugCashSet(int amount)=>DebugSetRuntimeCash(amount);
        internal void TestHarness_DebugCashAdd(int amount)=>DebugAddRuntimeCash(amount);
        internal void TestHarness_DebugInstantDeath()=>DebugInstantPlayerDeath();
        internal void TestHarness_DebugDepleteAllAmmo()=>DebugDepleteAllAmmo();
#endif
    }
}
