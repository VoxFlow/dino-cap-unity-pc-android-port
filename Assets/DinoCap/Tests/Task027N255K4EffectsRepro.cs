#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N255K4EffectsRepro
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        IDisposable storage;
        [SetUp] public void SetUp()=>storage=DC1Preferences.TestHarness_UseDirectory(Path.GetFullPath(Path.Combine(
            Application.dataPath,"../../Dev/Operator/DC1-MASTER/TestSaves",Guid.NewGuid().ToString("N"))));
        [TearDown] public void TearDown(){storage?.Dispose();GameplayRenderTarget.ReloadPreferences();}
        static void Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,Private).Invoke(target,args);
        static void Set(object target,string field,object value)=>target.GetType().GetField(field,Private).SetValue(target,value);
        static T Get<T>(object target,string field)=>(T)target.GetType().GetField(field,Private).GetValue(target);
        static Power2DSourceTruthRenderer SandboxVisual(CombatSandboxV2 sandbox)=>(Power2DSourceTruthRenderer)
            typeof(CombatSandboxV2).GetProperty("enemyVisual",Private).GetValue(sandbox);
        IEnumerator Load(string name)
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/"+name+".unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }
        [Test] public void DamageFlashMatchesDecodedNativeRateAndBoundaryClamps()
        {
            float rgb=1,rate=4;NativePlayerDamageFlash.Tick(ref rgb,ref rate,.025f);
            Assert.That(rgb,Is.EqualTo(1));Assert.That(rate,Is.EqualTo(-4));
            NativePlayerDamageFlash.Tick(ref rgb,ref rate,.1f);Assert.That(rgb,Is.EqualTo(.6f).Within(.0001f));
            NativePlayerDamageFlash.Tick(ref rgb,ref rate,.025f);Assert.That(rgb,Is.EqualTo(.5f).Within(.0001f));Assert.That(rate,Is.EqualTo(4));
            NativePlayerDamageFlash.Tick(ref rgb,ref rate,.125f);Assert.That(rgb,Is.EqualTo(1));Assert.That(rate,Is.EqualTo(-4));
        }
        [UnityTest] public IEnumerator BothSpitHitOwnersPreservePulsePhaseAndExpiryPreservesRate()
        {
            foreach(string scene in new[]{"CombatSandboxV2","JungleBlitzScene"})
            {
                yield return Load(scene);
                MonoBehaviour owner=scene=="CombatSandboxV2"?(MonoBehaviour)Object.FindFirstObjectByType<CombatSandboxV2>():Object.FindFirstObjectByType<CityGridController>();
                owner.enabled=false;Set(owner,"godMode",false);Set(owner,"playerHP",100f);Set(owner,"playerInvulnColourScale",.75f);Set(owner,"playerInvulnColourRate",-4f);
                Call(owner,scene=="CombatSandboxV2"?"DebugK4ProjectileHit":"JungleProjectileHit");
                Assert.That(Get<float>(owner,"playerHP"),Is.EqualTo(90));
                Assert.That(Get<float>(owner,"playerInvulnColourScale"),Is.EqualTo(.75f));
                Assert.That(Get<float>(owner,"playerInvulnColourRate"),Is.EqualTo(-4f));
                Set(owner,"invulnUntil",scene=="CombatSandboxV2"?Time.time-1f:Get<float>(owner,"sceneTime")-1f);Call(owner,"UpdatePlayerInvulnerabilityVisual");
                Assert.That(Get<float>(owner,"playerInvulnColourScale"),Is.EqualTo(1));
                Assert.That(Get<float>(owner,"playerInvulnColourRate"),Is.EqualTo(-4f));
            }
        }
        [UnityTest] public IEnumerator SpecialDeathSelectsActualK4AssetRatherThanRusherAlias()
        {
            yield return Load("CombatSandboxV2");var sandbox=Object.FindFirstObjectByType<CombatSandboxV2>();sandbox.enabled=false;
            sandbox.TestHarness_ClearDinos();sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);
            Call(sandbox,"BeginGSpecialFatalDeath");var animation=SandboxVisual(sandbox).primary.Clip;
            Assert.That(animation.name,Is.EqualTo("k4njz_die2"));Assert.That(animation.frames.Length,Is.EqualTo(31));
            Assert.That(Array.Exists(animation.frames[0].parts,p=>p.name=="njz"),Is.True);
            Assert.That(Array.Exists(animation.frames[0].parts,p=>p.name=="shen"),Is.False);
            Assert.That(DinoFatalDeathCore.GetGSpecialPresentation(DinoFatalDeathCore.Species.K1Rusher).die2ClipName,Is.EqualTo("k4_die2"));
            Assert.That(sandbox.TestHarness_EnemyRuntimeAdisValue,Is.EqualTo(190));
        }
        [UnityTest] public IEnumerator BothK4LaserOwnersContinueWithOriginalBonesAndPreservePoseAndMarkedDamage()
        {
            foreach(string scene in new[]{"CombatSandboxV2","JungleBlitzScene"})
            {
                yield return Load(scene);Power2DSourceTruthRenderer visual;object owner,dino=null;
                if(scene=="CombatSandboxV2")
                {
                    var sandbox=Object.FindFirstObjectByType<CombatSandboxV2>();sandbox.enabled=false;sandbox.TestHarness_ClearDinos();
                    sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);sandbox.TestHarness_SetDebugEnemyHP(1000);
                    owner=sandbox;visual=SandboxVisual(sandbox);Call(owner,"BeginF3");
                }
                else
                {
                    var city=Object.FindFirstObjectByType<CityGridController>();city.enabled=false;city.TestHarness_ClearDinos();city.TestHarness_JungleSpawn(4);city.TestHarness_SetDinoHP(0,1000);
                    dino=((IList)typeof(CityGridController).GetField("dinos",Private).GetValue(city))[0];owner=city;
                    visual=(Power2DSourceTruthRenderer)dino.GetType().GetField("visual").GetValue(dino);Call(owner,"BeginF3",dino);
                }
                visual.enabled=false;Call(visual,"Render");
                Assert.That(Application.isPlaying,Is.True);Assert.That(visual.xraySkeleton,Is.False,"Existing F3 phase starts with the authored hurt pose.");
                for(int i=0;i<3;i++)visual.primary.Tick(visual.primary.Clip.frameDuration);
                float hp=dino==null?(float)typeof(CombatSandboxV2).GetProperty("enemyHP",Private).GetValue(owner):(float)dino.GetType().GetField("hp").GetValue(dino);
                Assert.That(hp,Is.LessThan(1000),"Native marked3 combat event still applies damage.");Assert.That(visual.xraySkeleton,Is.True);
                visual.xraySkeleton=false;Call(visual,"Render");var parts=visual.GetComponentsInChildren<SpriteRenderer>();
                var position=new Vector3[parts.Length];var rotation=new Quaternion[parts.Length];var size=new Vector3[parts.Length];var colour=new Color[parts.Length];
                for(int i=0;i<parts.Length;i++){position[i]=parts[i].transform.localPosition;rotation[i]=parts[i].transform.localRotation;size[i]=parts[i].bounds.size;colour[i]=parts[i].color;}
                visual.xraySkeleton=true;Call(visual,"Render");int bones=0;
                for(int i=0;i<parts.Length;i++)
                {
                    var sprite=parts[i];if(!sprite.enabled)continue;
                    Assert.That(sprite.transform.localPosition,Is.EqualTo(position[i]));Assert.That(sprite.transform.localRotation,Is.EqualTo(rotation[i]));
                    Assert.That(Vector3.Distance(sprite.bounds.size,size[i]),Is.LessThan(.001f));Assert.That(sprite.color,Is.EqualTo(colour[i]));
                    if(sprite.sprite.name=="njzs" || sprite.sprite.name=="ntxs" || sprite.sprite.name=="ntss" || sprite.sprite.name=="nss" || sprite.sprite.name=="sts" || sprite.sprite.name=="wjzs" || sprite.sprite.name=="wtxs" || sprite.sprite.name=="wtss" || sprite.sprite.name=="wss" || sprite.sprite.name=="ts" || sprite.sprite.name=="zbs")bones++;
                }
                Assert.That(bones,Is.EqualTo(11),"All eleven original skeletal atlas images must render, without invented art.");Assert.That(Application.isPlaying,Is.True);
            }
        }
        [UnityTest] public IEnumerator BothK4BurnAndAshOwnersPreserveSpeciesFourGeometry()
        {
            foreach(string scene in new[]{"CombatSandboxV2","JungleBlitzScene"})
            {
                yield return Load(scene);Power2DSourceTruthRenderer visual;F2EnemyBigFireVisual fire;object owner,dino=null;
                if(scene=="CombatSandboxV2")
                {
                    var sandbox=Object.FindFirstObjectByType<CombatSandboxV2>();sandbox.enabled=false;sandbox.TestHarness_ClearDinos();
                    sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);owner=sandbox;visual=SandboxVisual(sandbox);
                    Call(owner,"BeginF2Burn");fire=(F2EnemyBigFireVisual)typeof(CombatSandboxV2).GetProperty("f2BigFireVisual",Private).GetValue(owner);
                }
                else
                {
                    var city=Object.FindFirstObjectByType<CityGridController>();city.enabled=false;city.TestHarness_ClearDinos();city.TestHarness_JungleSpawn(4);
                    dino=((IList)typeof(CityGridController).GetField("dinos",Private).GetValue(city))[0];owner=city;
                    visual=(Power2DSourceTruthRenderer)dino.GetType().GetField("visual").GetValue(dino);
                    Call(owner,"BeginF2Burn",dino);fire=(F2EnemyBigFireVisual)dino.GetType().GetField("f2BigFireVisual").GetValue(dino);
                }
                Assert.That(fire.transform.position,Is.EqualTo(visual.transform.position));
                Assert.That(fire.GetComponent<Power2DSourceTruthRenderer>().actorScale,Is.EqualTo(1));
                if(dino==null)Call(owner,"BeginF2BurnDeath");else Call(owner,"BeginF2BurnDeath",dino);
                Assert.That(visual.actorScale,Is.EqualTo(1));Assert.That(visual.drawOffset,Is.EqualTo(Vector2.zero));
                //12884 species4 skips scale, including when a child retains a previous small-species scale.
                fire.BeginNative(visual.transform,visual,DinoFatalDeathCore.Species.K3Blue);
                fire.BeginNative(visual.transform,visual,DinoFatalDeathCore.Species.K4Raptor);
                Assert.That(fire.GetComponent<Power2DSourceTruthRenderer>().actorScale,Is.EqualTo(.8f));
                Assert.That(fire.transform.position,Is.EqualTo(visual.transform.position));
            }
        }
        [UnityTest] public IEnumerator UnassignedK4HeadAndDebrisReuseGraphicsAcrossOneFortySlotRing()
        {
            yield return Load("CombatSandboxV2");var sandbox=Object.FindFirstObjectByType<CombatSandboxV2>();sandbox.enabled=false;
            sandbox.TestHarness_ClearDinos();sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);var source=SandboxVisual(sandbox);
            var first=DinoCapDamageParticle.Spawn(source,DinoCapDamageParticle.Kind.Blood,Vector3.zero,Vector2.zero,0,-250,new Vector2(28,13),100);
            Sprite sprite=first.GetComponent<SpriteRenderer>().sprite;Vector3 size=first.transform.localScale;
            for(int i=1;i<40;i++)
                if(i%2==0)WeaponSuirouParticle.Spawn(source,"suirou2",Vector3.zero,new Vector2(35,19),-250,100);
                else DinoCapDamageParticle.Spawn(source,DinoCapDamageParticle.Kind.ChunkS3,Vector3.zero,Vector2.zero,0,-250,new Vector2(61,51),100);
            var head=FlyingHeadParticle.Spawn(source,null,Vector3.zero,Vector2.zero,-250,true,FlyingHeadParticle.Species.K4Raptor,100);
            Assert.That(head.gameObject,Is.EqualTo(first.gameObject));Assert.That(head.GetComponent<SpriteRenderer>().sprite,Is.EqualTo(sprite));
            Assert.That(head.GetComponent<SpriteRenderer>().enabled,Is.True);Assert.That(head.transform.localScale,Is.EqualTo(size));Assert.That(first.enabled,Is.False);
            for(int i=1;i<40;i++)DinoCapDamageParticle.Spawn(source,DinoCapDamageParticle.Kind.Blood,Vector3.zero,Vector2.zero,0,-250,new Vector2(28,13),100);
            var chunk=DinoCapDamageParticle.Spawn(source,DinoCapDamageParticle.Kind.BlankChunkS1,Vector3.zero,Vector2.zero,0,-250,Vector2.zero,100);
            Assert.That(chunk.gameObject,Is.EqualTo(head.gameObject));Assert.That(head.enabled,Is.False);
            Assert.That(chunk.GetComponent<SpriteRenderer>().sprite,Is.EqualTo(sprite));Assert.That(chunk.transform.localScale,Is.EqualTo(size));
            var pool=chunk.GetComponentInParent<NativeDamageParticlePool>();Assert.That(pool.GetComponentsInChildren<SpriteRenderer>(true).Length,Is.EqualTo(40));
            Call(chunk,"Settle");Assert.That(chunk.GetComponent<SpriteRenderer>().enabled,Is.True);
        }
        [UnityTest] public IEnumerator BothExplosionOwnersPreserveMissingNativeSocketOrigin()
        {
            foreach(string scene in new[]{"CombatSandboxV2","JungleBlitzScene"})
            {
                yield return Load(scene);
                if(scene=="CombatSandboxV2")
                {
                    var sandbox=Object.FindFirstObjectByType<CombatSandboxV2>();sandbox.enabled=false;sandbox.TestHarness_ClearDinos();
                    sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);Call(sandbox,"BeginEWeaponDeath");
                }
                else
                {
                    var city=Object.FindFirstObjectByType<CityGridController>();city.enabled=false;city.TestHarness_ClearDinos();city.TestHarness_JungleSpawn(4);
                    city.TestHarness_ApplyEExplosionHitToDino(0,10000);
                }
                int count=0;
                foreach(var piece in Object.FindObjectsByType<WeaponSuirouParticle>(FindObjectsSortMode.None))if(piece.enabled)
                {count++;Assert.That(piece.transform.position,Is.EqualTo(scene=="CombatSandboxV2"?Vector3.down*136:Vector3.zero));}
                Assert.That(count,Is.EqualTo(4));
            }
        }
    }
}
#endif
