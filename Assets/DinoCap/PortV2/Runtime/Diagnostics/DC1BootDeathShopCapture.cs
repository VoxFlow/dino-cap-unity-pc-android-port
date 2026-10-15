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
        IEnumerator CaptureBootDeathShop()
        {
            var trace=new System.Text.StringBuilder("stage,time,root_x,live_hp,live_cash,live_ammo,hud_hp,hud_cash,hud_ammo,fade\n");
            float start=Time.realtimeSinceStartup;
            // Existing capture entry may skip logo for other protocols; force
            // a new boot lifecycle only for this explicit isolated diagnostic.
            typeof(MainMenuController).GetField("initialApplicationScene",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);
            yield return SceneManager.LoadSceneAsync("MainMenuScene");yield return null;
            var menu=FindFirstObjectByType<MainMenuController>();
            if(!menu.TestHarness_BootSplashActive || menu.TestHarness_MenuMusicSource!=null)
                throw new InvalidOperationException("Boot logo admitted menu music");
            yield return Save("boot_logo_silent");
            while(menu.TestHarness_BootSplashActive)yield return null;
            yield return null;
            if(menu.TestHarness_MenuMusicSource==null || !menu.TestHarness_MenuMusicSource.isPlaying)
                throw new InvalidOperationException("Menu handoff did not start music");
            yield return Save("mainmenu_music_started");
            CityGrindStartPreset.ForLevel(1).WriteCommittedRun(3);
            DC1Preferences.SetInt("DC1_CityGrind_Cash",140);
            DC1Preferences.SetInt(CityGrindStartPreset.AmmoPrefPrefix+0,200);
            DC1Preferences.SetFloat(CityGrindStartPreset.HealthPrefKey,70);DC1Preferences.Save();
            DC1DebugSettings.AffectsSaves=true;
            yield return SceneManager.LoadSceneAsync("CityGrindScene");yield return null;yield return null;
            var city=FindFirstObjectByType<CityGridController>();city.TestHarness_DebugUiHidden=true;
            city.TestHarness_SkipLevelIntro();city.TestHarness_ClearDinos();city.TestHarness_GodMode=true;
            Set(city,"cgT1Next",-1f);Set(city,"cgT2Next",-1f);Set(city,"cgT3Next",-1f);
            city.TestHarness_SetCashTotal(777);city.TestHarness_GrantAmmo(0,17-city.TestHarness_AmmoFor(0));
            city.TestHarness_DebugInstantDeath();
            while(!city.TestHarness_DeathUIActive)yield return null;
            if(city.TestHarness_DisplayedHud!=new Vector4(0,777,17,0) || city.TestHarness_CashTotal!=140)
                throw new InvalidOperationException("Death HUD did not retain its displayed values independently of rollback");
            yield return Save("death_rollback_retained_hud");
            var old=city;int frame=0;
            while(old!=null)
            {
                var hud=old.TestHarness_DisplayedHud;
                trace.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"death,{0:F5},{1:F5},{2},{3},{4},{5},{6},{7},0\n",
                    Time.realtimeSinceStartup-start,old.TestHarness_PlayerX,old.TestHarness_PlayerHP,old.TestHarness_CashTotal,old.TestHarness_AmmoFor(0),hud.x,hud.y,hud.z);
                if(hud!=new Vector4(0,777,17,0))throw new InvalidOperationException("Old HUD changed during death composite");
                if(++frame%30==0)yield return Save("death_stage_"+frame);
                else yield return null;
            }
            yield return null;city=FindFirstObjectByType<CityGridController>();city.TestHarness_DebugUiHidden=true;
            if(city.TestHarness_DisplayedHud!=new Vector4(100,140,200,0))throw new InvalidOperationException("Replay values did not appear on the new scene");
            yield return Save("reloaded_checkpoint_full_health");
            city.TestHarness_SkipLevelIntro();city.TestHarness_ClearDinos();city.TestHarness_GodMode=true;
            city.TestHarness_TriggerLevelClear();city.TestHarness_ForceShopFadeComplete();
            city.TestHarness_SetPlayerX(CityGridController.ShopFinishX+.01f);
            while(!city.TestHarness_ForcedWalkFading)yield return null;
            yield return Save("shop_native_threshold_fade");
            frame=0;
            while(city.TestHarness_ShopScreenActive)
            {
                trace.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"shop,{0:F5},{1:F5},0,0,0,0,0,0,{2:F5}\n",
                    Time.realtimeSinceStartup-start,city.TestHarness_PlayerX,city.TestHarness_ForcedWalkFadeTimer);
                if(++frame%10==0)yield return Save("shop_exit_"+frame);
                else yield return null;
            }
            yield return Save("city_after_shop_exit");
            File.WriteAllText(Path.Combine(output,"presentation_trace.csv"),trace.ToString());
            File.WriteAllText(Path.Combine(output,"presentation_complete.json"),"{\"status\":\"PASS\",\"scope\":\"Staged production boot/death/shop owners; isolated saves; user acceptance pending\"}");
        }
    }
}
#endif
