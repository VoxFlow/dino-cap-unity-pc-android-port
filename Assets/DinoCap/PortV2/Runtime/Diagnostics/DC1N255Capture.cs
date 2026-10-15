#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoCap.PortV2
{
    public sealed partial class DC1ConvergenceCapture
    {
        [Serializable] sealed class N255Frame
        {
            public string stage,scene,clip;public int frame;public float rootX,headX,headY,cameraLeft,time;
            public bool entering,deathUi;public int blood,pickups,beams,dinos,projectiles,f2Events;public float hp,dinoHp;
        }
        [Serializable] sealed class N255Output
        {
            public string scope="Owned real development player; explicit staged jump admission, real animation/movement/death/scene owners. Not human acceptance.";
            public List<N255Frame> frames=new List<N255Frame>();
        }
        IEnumerator SaveN255(string label,N255Output evidence,CityGridController city,int index=-1,bool screenshot=true)
        {
            yield return new WaitForEndOfFrame();
            var row=new N255Frame {stage=label,scene=SceneManager.GetActiveScene().name,time=Time.realtimeSinceStartup,
                deathUi=city!=null && city.TestHarness_DeathUIActive,
                blood=FindObjectsByType<DinoCapDamageParticle>(FindObjectsSortMode.None).Length};
            row.projectiles=FindObjectsByType<JungleProjectileActor>(FindObjectsSortMode.None).Length;
            row.f2Events=NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_att3");
            if(city!=null){row.cameraLeft=city.TestHarness_CameraLeftX;row.hp=city.TestHarness_PlayerHP;row.dinos=city.TestHarness_DinoCount;}
            foreach(var pickup in FindObjectsByType<JunglePickupActor>(FindObjectsSortMode.None))
            {row.pickups++;if(pickup.BeamActive)row.beams++;}
            if(city!=null && index>=0 && index<city.TestHarness_DinoCount)
            {
                var list=(IList)typeof(CityGridController).GetField("dinos",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
                var visual=(Power2DSourceTruthRenderer)list[index].GetType().GetField("visual").GetValue(list[index]);
                visual.TryGetPartWorldAnchor("tou",out Vector3 head);
                row.clip=visual.primary.Clip?.name;row.frame=visual.primary.Frame;
                row.rootX=city.TestHarness_DinoX(index);row.headX=head.x;row.headY=head.y;row.entering=city.TestHarness_DinoEntering(index);
                row.dinoHp=city.TestHarness_DinoHP(index);
            }
            if(screenshot)
            {
                var image=ScreenCapture.CaptureScreenshotAsTexture();
                try{File.WriteAllBytes(Path.Combine(output,label+".png"),image.EncodeToPNG());}
                finally{Destroy(image);}
            }
            evidence.frames.Add(row);
        }
        IEnumerator CaptureN255()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-k4-laser-repair")>=0)
            {
                yield return CaptureK4LaserRepair();yield break;
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-k4-parity")>=0)
            {
                yield return CaptureK4Parity();yield break;
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-boot-death-shop")>=0)
            {
                yield return CaptureBootDeathShop();yield break;
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-camera-reversal")>=0)
            {
                yield return CaptureCameraReversal();yield break;
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-debug-ui")>=0)
            {
                yield return CaptureDebugControls();yield break;
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dc1-wide-smoke")>=0)
            {
                yield return CaptureWideSmoke();yield break;
            }
            var evidence=new N255Output();
            for(int mode=0;mode<2;mode++)
            {
                yield return SceneManager.LoadSceneAsync(mode==0?"CityGrindScene":"JungleBlitzScene");
                yield return null;yield return null;
                var city=FindFirstObjectByType<CityGridController>();city.TestHarness_DebugUiHidden=true;
                city.TestHarness_DebugCashSet(321);city.TestHarness_GodMode=true;
                city.TestHarness_SkipLevelIntro();city.TestHarness_ClearDinos();
                city.TestHarness_SetPlayerX(200);
                city.TestHarness_JungleSpeciesFilter(15,false);
                yield return new WaitForSeconds(.2f);
                Set(city,"cgT1Next",-1f);Set(city,"cgT2Next",-1f);Set(city,"cgT3Next",-1f);
                if(mode==1)city.TestHarness_JungleSpeciesFilter(0,false);
                for(int type=1;type<=2;type++)
                {
                    city.TestHarness_ClearDinos();city.TestHarness_JungleRawDraw(()=>int.MaxValue);
                    if(mode==1)city.TestHarness_JungleSpeciesFilter(1<<(type-1),false);
                    if(mode==0)city.TestHarness_ForceSpawn(type-1);else city.TestHarness_JungleSpawn(type);
                    if(mode==1)city.TestHarness_JungleSpeciesFilter(0,false);
                    int[] frames={0,5,10,15,20};int next=0;
                    float timeout=Time.realtimeSinceStartup+3;
                    while(next<frames.Length && Time.realtimeSinceStartup<timeout)
                    {
                        var list=(IList)typeof(CityGridController).GetField("dinos",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
                        var visual=(Power2DSourceTruthRenderer)list[0].GetType().GetField("visual").GetValue(list[0]);
                        if(visual.primary.Frame>=frames[next])
                        {yield return SaveN255((mode==0?"city":"jungle")+"_type"+type+"_jump_"+frames[next],evidence,city,0);next++;}
                        else yield return null;
                    }
                    if(next!=5)throw new InvalidOperationException("Missing live jump frames for type"+type+" mode"+mode);
                    yield return new WaitForSeconds(.2f);
                    yield return SaveN255((mode==0?"city":"jungle")+"_type"+type+"_landed",evidence,city,0);
                }
                city.TestHarness_ClearDinos();city.TestHarness_JungleRawDraw(null);
                if(mode==1)
                {
                    var pickup=new JungleDropSelection.Decision {frame="Lb1Frame",kind=JunglePickupState.Kind.Weapon,family=1,tier=0};
                    city.TestHarness_JungleCreatePickup(pickup,city.TestHarness_PlayerX+300,-94);
                    yield return SaveN255("jungle_pickup_before",evidence,city);
                    city.TestHarness_SetPlayerX(city.TestHarness_PlayerX+300);
                    yield return null;yield return SaveN255("jungle_pickup_collected_child_retired",evidence,city);
                    city.TestHarness_JungleCreatePickup(pickup,city.TestHarness_PlayerX,-94);
                    yield return null;yield return SaveN255("jungle_repeated_pickup_beams",evidence,city);
                    yield return new WaitForSeconds(10.3f);
                    yield return SaveN255("jungle_pickup_beams_expired",evidence,city);
                    city.TestHarness_JungleRawDraw(()=>0);
                    city.TestHarness_JungleSpeciesFilter(15,false);city.TestHarness_JungleSpawn(1);city.TestHarness_JungleSpawn(4);
                    city.TestHarness_SetDinoX(0,city.TestHarness_PlayerX+200);city.TestHarness_SetDinoX(1,city.TestHarness_PlayerX+330);
                    yield return null;
                    city.TestHarness_JungleSpeciesFilter(8,false);
                    yield return SaveN255("jungle_future_filter_keeps_existing",evidence,city);
                    city.TestHarness_JungleSpeciesFilter(8,true);yield return null;yield return null;
                    yield return SaveN255("jungle_filter_optin_retired_excluded",evidence,city);
                    city.TestHarness_ClearDinos();city.TestHarness_JungleSpeciesFilter(0,false);city.TestHarness_JungleRawDraw(null);
                    city.TestHarness_SetPlayerX(2000);yield return new WaitForSeconds(1);
                    yield return SaveN255("jungle_camera_right_before_reversal",evidence,city);
                    for(int step=0;step<12;step++)
                    {
                        city.TestHarness_SetPlayerX(city.TestHarness_PlayerX-10);yield return null;
                        yield return SaveN255("jungle_left_reversal_"+step,evidence,city);
                    }
                }
                if(mode==0)
                {
                    var listener=FindFirstObjectByType<AudioListener>();var pcm=listener.gameObject.AddComponent<DC1DiagnosticPcm>();
                    for(int f=0;f<6;f++)city.TestHarness_SetRangedFamilyTierRaw(f,f<2?0:-1);
                    city.TestHarness_SetRangedFamily(0);city.TestHarness_GrantAmmo(1,50);
                    city.TestHarness_SwitchWeapon();yield return new WaitForSeconds(.3f);
                    pcm.Write(Path.Combine(output,"switch_button_audio.wav"));
                    for(int trial=0;trial<4;trial++)
                    {
                        city.TestHarness_ClearDinos();city.TestHarness_JungleRawDraw(()=>0);city.TestHarness_ForceSpawn(2);
                        city.TestHarness_SetDinoHP(0,10000);city.TestHarness_SetPlayerInvulnerableForSeconds(10);
                        var list=(IList)typeof(CityGridController).GetField("dinos",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
                        var d=list[0];var v=(Power2DSourceTruthRenderer)d.GetType().GetField("visual").GetValue(d);
                        v.TryGetPartWorldAnchor("tou",out Vector3 head);
                        city.TestHarness_SetDinoX(0,city.TestHarness_DinoX(0)+city.TestHarness_PlayerX+100-head.x);
                        city.TestHarness_SetRangedFamilyTierRaw(5,trial==3?2:1);city.TestHarness_SetRangedFamily(5);city.TestHarness_GrantAmmo(5,250);
                        NativeAudioSessionGate.TestHarness_ResetClipCounters();
                        string protocol=trial==0?"f2_first_tap":trial==1?"f2_repeat_tap":trial==2?"f2_15frame_hold":"f3_first_tap";
                        for(int tick=0;tick<72;tick++)
                        {
                            if(tick==0 || trial==2 && tick<15)city.TestHarness_RequestShoot();
                            yield return null;
                            if(tick%6==0)yield return SaveN255("city_"+protocol+"_"+tick,evidence,city,0,tick==6 || tick==30 || tick==66);
                        }
                    }
                    city.TestHarness_ClearDinos();city.TestHarness_JungleRawDraw(null);
                }
                city.TestHarness_SetLives(2);city.TestHarness_DebugInstantDeath();
                yield return new WaitForSeconds(.15f);yield return SaveN255((mode==0?"city":"jungle")+"_ballistic_death",evidence,city);
                if(mode==0)
                {
                    yield return WaitJungle(()=>city==null || city.TestHarness_DeathUIActive,"City death UI");
                    while(city!=null && city.TestHarness_DeathUIActive)
                    {
                        var p=(NativeDeathPresentation)typeof(CityGridController).GetField("deathPresentation",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
                        if(p!=null && p.CurrentStage==NativeDeathPresentation.Stage.DieEndWait)break;
                        yield return null;
                    }
                    yield return SaveN255("city_lives_end_wait",evidence,city);
                    var old=city;float deadline=Time.realtimeSinceStartup+5;int sampled=0;
                    while(old!=null && Time.realtimeSinceStartup<deadline)
                    {
                        yield return SaveN255("city_replay_transition_"+sampled++,evidence,old);
                        if(sampled>120)throw new InvalidOperationException("Transition capture frame limit");
                    }
                    if(old!=null)throw new InvalidOperationException("City scene replacement failed");
                    city=FindFirstObjectByType<CityGridController>();city.TestHarness_DebugUiHidden=true;
                    yield return SaveN255("city_replay_new_scene",evidence,city);
                    city.TestHarness_DebugCashSet(321);city.TestHarness_SkipLevelIntro();city.TestHarness_ClearDinos();
                    city.TestHarness_SetLives(0);city.TestHarness_DebugInstantDeath();
                    yield return WaitJungle(()=>city==null || city.TestHarness_DeathUIActive,"City game-over UI");
                    while(city!=null)
                    {
                        var p=(NativeDeathPresentation)typeof(CityGridController).GetField("deathPresentation",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
                        if(p!=null && p.CurrentStage==NativeDeathPresentation.Stage.DieEndWait)break;
                        yield return null;
                    }
                    old=city;deadline=Time.realtimeSinceStartup+5;sampled=0;
                    while(old!=null && Time.realtimeSinceStartup<deadline)
                    {
                        yield return SaveN255("city_menu_transition_"+sampled++,evidence,old);
                        if(sampled>120)throw new InvalidOperationException("Menu transition frame limit");
                    }
                    if(old!=null || SceneManager.GetActiveScene().name!="MainMenuScene")throw new InvalidOperationException("Game over did not reach MainMenu");
                    yield return SaveN255("city_gameover_mainmenu",evidence,null);
                    yield return new WaitForSeconds(1);
                    var menu=FindFirstObjectByType<MainMenuController>();
                    menu.TestHarness_GoNewGame();
                    yield return new WaitForSeconds(1);
                    menu.TestHarness_SelectCity();
                    yield return new WaitForSeconds(1);
                    if(menu.TestHarness_LevelPresentation==null)throw new InvalidOperationException("City level selection did not open");
                    yield return SaveN255("city_level_select_options_hidden",evidence,null);
                    menu.TestHarness_BackFromLevelSelect();
                    yield return new WaitForSeconds(1);
                    yield return SaveN255("city_level_select_back_options_restored",evidence,null);
                    yield return SceneManager.LoadSceneAsync("CityGrindScene");yield return null;yield return null;
                    city=FindFirstObjectByType<CityGridController>();city.TestHarness_DebugUiHidden=true;
                    foreach(int level in new[]{25,35})
                    {
                        city.TestHarness_DebugJumpLevel(level-city.TestHarness_CityLevel);
                        city.TestHarness_TriggerLevelClear();yield return new WaitForSeconds(1.5f);
                        yield return SaveN255("city_native_debug_preset_shop_"+level,evidence,city);
                    }
                }
                else
                {
                    yield return WaitJungle(()=>city.TestHarness_JungleResultsVisible,"Jungle results");
                    yield return SaveN255("jungle_results_current",evidence,city);
                }
            }
            yield return SceneManager.LoadSceneAsync("CombatSandboxV2");yield return null;yield return null;
            var sandbox=FindFirstObjectByType<CombatSandboxV2>();sandbox.TestHarness_ClearDinos();sandbox.TestHarness_SetGodMode(true);
            sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);
            sandbox.TestHarness_SetEnemyX(sandbox.TestHarness_PlayerX+250);sandbox.TestHarness_ForceEnemyAttack();
            yield return new WaitForSeconds(.36f);yield return SaveN255("sandbox_debug_k4_projectile",evidence,null);
            File.WriteAllText(Path.Combine(output,"n255_capture.json"),JsonUtility.ToJson(evidence,true));
        }
        IEnumerator CaptureWideSmoke()
        {
            for(int mode=0;mode<2;mode++)
            {
                yield return SceneManager.LoadSceneAsync(mode==0?"CityGrindScene":"JungleBlitzScene");
                yield return null;yield return null;
                var city=FindFirstObjectByType<CityGridController>();
                city.TestHarness_DebugUiHidden=true;city.TestHarness_DebugCashSet(321);
                city.TestHarness_GodMode=true;city.TestHarness_SkipLevelIntro();city.TestHarness_ClearDinos();
                Set(city,"cgT1Next",-1f);Set(city,"cgT2Next",-1f);Set(city,"cgT3Next",-1f);
                if(mode==1)city.TestHarness_JungleSpeciesFilter(0,false);
                city.TestHarness_SetPlayerX(2000);yield return new WaitForSeconds(1);
                yield return Save(mode==0?"city_wide_hud":"jungle_before_reversal");
                if(mode==1)
                {
                    for(int step=0;step<12;step++){city.TestHarness_SetPlayerX(city.TestHarness_PlayerX-10);yield return null;}
                    yield return Save("jungle_left_reversal_covered");
                    var decision=new JungleDropSelection.Decision {frame="Lbullet1Frame",kind=JunglePickupState.Kind.Ammo,family=1,tier=-1};
                    city.TestHarness_JungleCreatePickup(decision,city.TestHarness_PlayerX+300,-94);
                    yield return Save("jungle_halo_before_collection");
                    city.TestHarness_SetPlayerX(city.TestHarness_PlayerX+300);yield return null;
                    yield return Save("jungle_halo_after_collection");
                    foreach(var pickup in FindObjectsByType<JunglePickupActor>(FindObjectsSortMode.None))
                        if(!pickup.State.Active && pickup.BeamActive)throw new InvalidOperationException("Collected pickup retained its child halo");
                }
            }
            File.WriteAllText(Path.Combine(output,"wide_smoke_complete.json"),"{\"status\":\"PASS\",\"scope\":\"Staged production City/Jungle rendering; virtual viewport export, not physical multi-monitor acceptance\"}");
        }

        IEnumerator CaptureCameraReversal()
        {
            var trace=new System.Text.StringBuilder("mode,phase,time,root_x,camera_left,view_width,screen_fraction\n");
            for(int mode=0;mode<2;mode++)
            {
                yield return SceneManager.LoadSceneAsync(mode==0?"CityGrindScene":"JungleBlitzScene");
                yield return null;yield return null;
                var city=FindFirstObjectByType<CityGridController>();
                city.TestHarness_DebugUiHidden=true;city.TestHarness_GodMode=true;
                city.TestHarness_SetDebugMovementSpeed(1f);city.TestHarness_SkipLevelIntro();city.TestHarness_ClearDinos();
                Set(city,"cgT1Next",-1f);Set(city,"cgT2Next",-1f);Set(city,"cgT3Next",-1f);
                if(mode==1)city.TestHarness_JungleSpeciesFilter(0,false);
                city.TestHarness_SetPlayerX(3500);
                var visual=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(city);
                visual.facingRight=false;
                typeof(CityGridController).GetMethod("InitializeCamera",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(city,null);
                yield return new WaitForSeconds(.2f);
                for(int phase=0;phase<3;phase++)
                {
                    bool right=phase!=1;float start=Time.realtimeSinceStartup;
                    while(Time.realtimeSinceStartup-start<1.15f)
                    {
                        if(right)city.TestHarness_RequestMoveRight();else city.TestHarness_RequestMoveLeft();
                        yield return null;
                        float fraction=(city.TestHarness_PlayerX-city.TestHarness_CameraLeftX)/city.TestHarness_CurrentViewWidth;
                        trace.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"{0},{1},{2:F5},{3:F5},{4:F5},{5:F5},{6:F5}\n",
                            mode,phase,Time.realtimeSinceStartup-start,city.TestHarness_PlayerX,city.TestHarness_CameraLeftX,city.TestHarness_CurrentViewWidth,fraction);
                    }
                    yield return Save((mode==0?"city":"jungle")+"_held_"+(right?"right":"left")+"_"+phase);
                    float final=(city.TestHarness_PlayerX-city.TestHarness_CameraLeftX)/city.TestHarness_CurrentViewWidth;
                    if(Mathf.Abs(final-(right?.45f:.55f))>.005f)throw new InvalidOperationException("Held camera reversal did not settle: "+final);
                }
            }
            File.WriteAllText(Path.Combine(output,"camera_reversal_trace.csv"),trace.ToString());
            File.WriteAllText(Path.Combine(output,"camera_reversal_complete.json"),"{\"status\":\"PASS\",\"scope\":\"Actual development player movement and camera; 1x held reversals, isolated save; not device acceptance\"}");
        }
    }
}
#endif
