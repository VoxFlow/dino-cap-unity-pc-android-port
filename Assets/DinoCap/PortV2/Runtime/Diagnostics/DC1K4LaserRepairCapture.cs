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
        [Serializable] sealed class K4LaserRepairRows { public List<K4LaserRepairRow> rows=new List<K4LaserRepairRow>(); }
        [Serializable] sealed class K4LaserRepairRow { public string scene,stage;public int frame,bones;public float hp;public bool xray; }
        IEnumerator CaptureK4LaserRepair()
        {
            const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var rows=new K4LaserRepairRows();DC1DebugSettings.Enabled=true;
            foreach(string scene in new[]{"CombatSandboxV2","JungleBlitzScene"})
            {
                yield return SceneManager.LoadSceneAsync(scene);yield return null;yield return null;
                Power2DSourceTruthRenderer visual;object owner,dino=null;
                if(scene=="CombatSandboxV2")
                {
                    var sandbox=FindFirstObjectByType<CombatSandboxV2>();sandbox.enabled=false;sandbox.TestHarness_ClearDinos();
                    sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);sandbox.TestHarness_SetEnemyX(sandbox.TestHarness_PlayerX+150);sandbox.TestHarness_SetDebugEnemyHP(1000);
                    owner=sandbox;visual=(Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetProperty("enemyVisual",flags).GetValue(owner);
                    visual.enabled=false;typeof(CombatSandboxV2).GetMethod("BeginF3",flags).Invoke(owner,null);
                }
                else
                {
                    var city=FindFirstObjectByType<CityGridController>();city.TestHarness_SkipLevelIntro();city.enabled=false;city.TestHarness_SetPlayerX(200);city.TestHarness_UpdateCamera(0);city.TestHarness_ClearDinos();city.TestHarness_JungleSpawn(4);
                    city.TestHarness_SetDinoX(0,city.TestHarness_PlayerX+150);city.TestHarness_SetDinoHP(0,1000);
                    owner=city;dino=((IList)typeof(CityGridController).GetField("dinos",flags).GetValue(owner))[0];
                    visual=(Power2DSourceTruthRenderer)dino.GetType().GetField("visual").GetValue(dino);
                    visual.enabled=false;typeof(CityGridController).GetMethod("BeginF3",flags).Invoke(owner,new[]{dino});
                }
                foreach(string stage in new[]{"hurt","xray"})
                {
                    if(stage!="hurt")for(int i=0;i<3;i++)visual.primary.Tick(visual.primary.Clip.frameDuration);
                    typeof(Power2DSourceTruthRenderer).GetMethod("Render",flags).Invoke(visual,null);
                    float hp=dino==null?(float)typeof(CombatSandboxV2).GetProperty("enemyHP",flags).GetValue(owner):(float)dino.GetType().GetField("hp").GetValue(dino);
                    int bones=0;
                    foreach(var sprite in visual.GetComponentsInChildren<SpriteRenderer>())
                        if(sprite.enabled && sprite.sprite!=null &&
                           Array.IndexOf(new[]{"njzs","ntxs","ntss","nss","sts","wjzs","wtxs","wtss","wss","ts","zbs"},sprite.sprite.name)>=0)bones++;
                    if(stage=="xray" && (bones!=11 || !visual.xraySkeleton || hp>=1000))throw new InvalidOperationException("K4 original-bone/damage capture failed");
                    if(stage!="xray" && visual.xraySkeleton)throw new InvalidOperationException("K4 hurt-phase capture failed");
                    rows.rows.Add(new K4LaserRepairRow {scene=scene,stage=stage,frame=visual.primary.Frame,bones=bones,hp=hp,xray=visual.xraySkeleton});
                    //The staged owner is disabled to preserve this exact frame.
                    //Its OnGUI framebuffer can consequently be stale. Capture
                    //the real world camera target directly for visual evidence.
                    var camera=Camera.main;
                    if(camera!=null && camera.targetTexture!=null)
                    {
                        camera.Render();var target=camera.targetTexture;var previous=RenderTexture.active;
                        var pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
                        try
                        {
                            RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();
                            File.WriteAllBytes(Path.Combine(output,scene+"_k4_laser_"+stage+"_world.png"),pixels.EncodeToPNG());
                        }
                        finally{RenderTexture.active=previous;Destroy(pixels);}
                    }
                    yield return Save(scene+"_k4_laser_"+stage);
                }
            }
            File.WriteAllText(Path.Combine(output,"k4_laser_repair.json"),JsonUtility.ToJson(rows,true));
            File.WriteAllText(Path.Combine(output,"k4_laser_repair_complete.json"),"{\"status\":\"PASS\",\"scope\":\"Owned development player; staged real Sandbox/Jungle F3 owners; original atlas bones; explicit binding repair\"}");
        }
    }
}
#endif
