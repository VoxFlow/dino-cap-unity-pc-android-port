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
        [Serializable]sealed class JungleCaptureStage
        {
            public string stage,scene;public int seconds,kills,cash,lives,ammo,width,height,internalScale;
            public bool results,paused,hitStun;public float hp;
            public int projectiles;public Vector2 projectilePosition,projectileVelocity;
        }
        [Serializable]sealed class JungleCaptureEvidence
        {
            public string scope="Owned development player; native1x; production menu/scene/death/relaunch owners, staged enemies and pickups. Not original-device acceptance.";
            public List<JungleCaptureStage> stages=new List<JungleCaptureStage>();
        }
        IEnumerator CaptureN254Jungle()
        {
            var evidence=new JungleCaptureEvidence();
            NativeModeSession.Publish(321,2);
            yield return SceneManager.LoadSceneAsync("MainMenuScene");yield return null;
            var menu=FindFirstObjectByType<MainMenuController>();menu.TestHarness_SkipBootSplash();
            yield return new WaitForSeconds(1.2f);yield return Save("jungle_menu_before_launch");
            menu.TestHarness_GoLifeMode();
            yield return WaitJungle(()=>FindFirstObjectByType<JungleBlitzController>()!=null,"launch");
            yield return null;yield return null;
            var city=FindFirstObjectByType<CityGridController>();city.TestHarness_DebugUiHidden=true;
            city.TestHarness_GodMode=true;
            var listener=FindFirstObjectByType<AudioListener>();
            var pcm=listener.gameObject.AddComponent<DC1DiagnosticPcm>();
            yield return SaveJungle(city,evidence,"jungle_start");
            yield return WaitJungle(()=>!city.TestHarness_LevelIntroActive,"START admission");
            city.TestHarness_SetPlayerX(200);city.TestHarness_ClearDinos();
            city.TestHarness_JungleSetTime(151);
            for(int type=1;type<=4;type++)
            {
                int i=city.TestHarness_DinoCount;city.TestHarness_JungleSpawn(type);
                city.TestHarness_SetDinoX(i,city.TestHarness_PlayerX+100+type*120);
                city.TestHarness_ForceDinoClip(i,"_move");
            }
            yield return SaveJungle(city,evidence,"jungle_four_species");
            city.TestHarness_ClearDinos();city.TestHarness_JungleSpawn(4);
            int k4=city.TestHarness_DinoCount-1;
            city.TestHarness_SetDinoX(k4,city.TestHarness_PlayerX+160);
            city.TestHarness_ForceDinoClip(k4,"_attack");
            // Native track advances at most one frame/update, so .025*10
            // is not a reliable wall-clock deadline. Observe actual producer.
            yield return WaitJungle(()=>FindFirstObjectByType<JungleProjectileActor>()!=null,"K4 projectile frame10");
            yield return new WaitForSeconds(.10f);
            yield return SaveJungle(city,evidence,"jungle_k4_projectile");
            city.TestHarness_RequestShoot();yield return new WaitForSeconds(.3f);
            var decision=new JungleDropSelection.Decision {frame="Lbullet1Frame",kind=JunglePickupState.Kind.Ammo,family=1,tier=-1};
            city.TestHarness_JungleCreatePickup(decision,city.TestHarness_PlayerX,-94);
            yield return new WaitForSeconds(.12f);yield return SaveJungle(city,evidence,"jungle_pickup_notice");
            city.TestHarness_TogglePause();yield return SaveJungle(city,evidence,"jungle_pause");
            city.TestHarness_TogglePause();city.TestHarness_ClearDinos();
            city.TestHarness_GodMode=false;city.TestHarness_ForceClearPlayerHitStun();city.TestHarness_ForcePlayerInvulnExpired();
            city.TestHarness_DamagePlayer(1000);
            yield return new WaitForSeconds(.5f);yield return SaveJungle(city,evidence,"jungle_death");
            yield return WaitJungle(()=>city.TestHarness_JungleResultsVisible,"results");
            yield return new WaitForSeconds(.3f);yield return SaveJungle(city,evidence,"jungle_results");
            pcm.Write(Path.Combine(output,"jungle_audio.wav"));
            int frozen=city.TestHarness_JungleSeconds;
            yield return new WaitForSeconds(1.2f);
            if(city.TestHarness_JungleSeconds!=frozen)throw new InvalidOperationException("Jungle result time did not freeze.");
            city.TestHarness_JungleRequestDone();
            yield return WaitJungle(()=>FindFirstObjectByType<MainMenuController>()!=null,"Done menu");
            menu=FindFirstObjectByType<MainMenuController>();menu.TestHarness_SkipBootSplash();
            yield return new WaitForSeconds(1.1f);yield return Save("jungle_menu_after_done");
            menu.TestHarness_GoLifeMode();
            yield return WaitJungle(()=>FindFirstObjectByType<JungleBlitzController>()!=null,"relaunch");
            yield return null;yield return null;
            city=FindFirstObjectByType<CityGridController>();city.TestHarness_DebugUiHidden=true;
            if(city.TestHarness_JungleSeconds!=0 || city.TestHarness_JungleKills!=0 || city.TestHarness_AmmoFor(0)!=999)
                throw new InvalidOperationException("Jungle relaunch did not reset local state.");
            yield return SaveJungle(city,evidence,"jungle_relaunch_start");
            File.WriteAllText(Path.Combine(output,"jungle_capture.json"),JsonUtility.ToJson(evidence,true));
        }
        IEnumerator WaitJungle(Func<bool> predicate,string stage)
        {
            float until=Time.realtimeSinceStartup+10;
            while(!predicate() && Time.realtimeSinceStartup<until)yield return null;
            if(!predicate())throw new InvalidOperationException("Jungle capture timed out: "+stage);
        }
        IEnumerator SaveJungle(CityGridController city,JungleCaptureEvidence evidence,string stage)
        {
            yield return Save(stage);
            var projectiles=FindObjectsByType<JungleProjectileActor>(FindObjectsSortMode.None);
            var projectile=projectiles.Length>0?projectiles[0].State:null;
            evidence.stages.Add(new JungleCaptureStage {stage=stage,scene=SceneManager.GetActiveScene().name,
                seconds=city.TestHarness_JungleSeconds,kills=city.TestHarness_JungleKills,cash=city.TestHarness_JungleCash,
                lives=city.TestHarness_JungleLives,ammo=city.TestHarness_AmmoFor(0),width=Screen.width,height=Screen.height,
                internalScale=GameplayRenderTarget.InternalScale,hp=city.TestHarness_PlayerHP,results=city.TestHarness_JungleResultsVisible,
                paused=city.TestHarness_Paused,hitStun=city.TestHarness_PlayerHitStun,
                projectiles=projectiles.Length,projectilePosition=projectile?.Position??Vector2.zero,
                projectileVelocity=projectile?.Velocity??Vector2.zero});
        }
    }
}
#endif
