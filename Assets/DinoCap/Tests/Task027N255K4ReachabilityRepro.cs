#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N255K4ReachabilityRepro
    {
        IDisposable storage;
        DC1MenuPreferenceScope preferences;
        CityGridController core;
        [UnitySetUp] public IEnumerator SetUp()
        {
            storage=DC1Preferences.TestHarness_UseDirectory(Path.GetFullPath(Path.Combine(
                Application.dataPath,"../../Dev/Operator/DC1-MASTER/TestSaves",Guid.NewGuid().ToString("N"))));
            preferences=new DC1MenuPreferenceScope();CityGrindStartPreset.ForLevel(1).WriteCommittedRun(3);
            DC1Preferences.Save();yield return null;
        }
        [TearDown] public void TearDown()
        {
            preferences?.Dispose();storage?.Dispose();GameplayRenderTarget.ReloadPreferences();
        }
        [UnityTest] public IEnumerator LiveJungleTimersNaturallyDispatchK4WithoutSetTimeOrForceSpawn()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/JungleBlitzScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;core=Object.FindFirstObjectByType<CityGridController>();
            core.TestHarness_SkipLevelIntro();core.TestHarness_ClearDinos();core.enabled=false;
            core.TestHarness_JungleSpeciesFilter(8,false);
            // Suppress other species only in this fixture; their timers still
            // dispatch and consume normal admission draws. Do not SET TIME.
            core.TestHarness_JungleTickTimers(0);
            for(int i=0;i<10149;i++)core.TestHarness_JungleTickTimers(1);
            Assert.That(core.TestHarness_JungleSeconds,Is.EqualTo(10149));
            Assert.That(core.TestHarness_DinoCount,Is.Zero);
            core.TestHarness_JungleTickTimers(1);
            Assert.That(core.TestHarness_DinoCount,Is.EqualTo(1),"151+9999 expires the live type4 timer; no random species selection can turn it into another species.");
            Assert.That(core.TestHarness_DinoKind(0),Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator DebugK4SpawnUsesCurrentJungleStatsWithoutAdvancingTimeOrReplacingTimers()
        {
            DC1DebugSettings.Enabled=true;
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/JungleBlitzScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;core=Object.FindFirstObjectByType<CityGridController>();
            core.TestHarness_SkipLevelIntro();core.TestHarness_ClearDinos();core.enabled=false;
            core.TestHarness_JungleTickTimers(0);
            core.TestHarness_JungleSetTime(10150);
            core.TestHarness_JungleSpeciesFilter(0,false);
            int handle=core.TestHarness_JungleK4TimerHandle;
            core.TestHarness_JungleDebugSpawnK4();
            Assert.That(core.TestHarness_DinoCount,Is.EqualTo(1),"An explicit manual spawn bypasses the future-species filter.");
            Assert.That(core.TestHarness_DinoKind(0),Is.EqualTo(3));
            Assert.That(core.TestHarness_DinoHP(0),Is.EqualTo(549f));
            Assert.That(core.TestHarness_JungleSeconds,Is.EqualTo(10150));
            Assert.That(core.TestHarness_JungleK4TimerHandle,Is.EqualTo(handle));
            Assert.That(core.TestHarness_JungleSpeciesMask,Is.Zero,"Manual admission must not alter the persistent filter.");
            DC1DebugSettings.Enabled=false;
            core.TestHarness_JungleDebugSpawnK4();
            Assert.That(core.TestHarness_DinoCount,Is.EqualTo(1),"Master OFF prohibits debug admission.");
        }
        [UnityTest] public IEnumerator SpitGroundPartsUseNativeTranslationAndRetainedAngleInBothSpaces()
        {
            foreach(bool facingRight in new[]{false,true})
            foreach(float offsetY in new[]{0f,136f})
            {
                var go=new GameObject("Native K4 ground transform fixture");
                try
                {
                    var actor=go.AddComponent<JungleProjectileActor>();
                    actor.Initialize(SourceTruthLoader.Load(),new Vector2(100,136-offsetY),facingRight,Vector2.up*offsetY);
                    int hits=0;
                    for(int i=0;i<180 && actor.State.Flying;i++)actor.Tick(1f/60f,10000,true,()=>{hits++;return true;});
                    Assert.That(actor.State.Flying,Is.False);
                    var visual=go.GetComponent<Power2DSourceTruthRenderer>();visual.enabled=false;
                    typeof(Power2DSourceTruthRenderer).GetMethod("Render",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(visual,null);
                    Assert.That(visual.facingRight,Is.False,"13918 writes native facing byte96=1, regardless of travel side.");
                    Assert.That(go.transform.rotation,Is.EqualTo(Quaternion.identity),"Native root angle must not rotate authored part offsets around the root.");
                    Assert.That(visual.TryGetPartWorldAnchor("dyd1",out Vector3 anchor),Is.True);
                    Assert.That(anchor.y,Is.EqualTo(actor.State.Position.y-offsetY-232.143005f).Within(.01f));
                    Assert.That(anchor.x,Is.EqualTo(actor.State.Position.x+.714279f).Within(.01f));
                    Assert.That(anchor.y,Is.LessThan(-100f-offsetY),"Ground clip must stay at its native ground anchor rather than orbiting above the player.");
                    float angle=visual.nativeRootRotationRadians;
                    actor.Tick(.05f,actor.State.Position.x,false,()=>{hits++;return true;});
                    Assert.That(hits,Is.Zero,"Ground goo has no projectile collision/DOT branch.");
                    Assert.That(visual.nativeRootRotationRadians,Is.EqualTo(angle),"139C8 and its animation bind do not reset native root angle; flattening would invent behavior.");
                    var sprite=go.GetComponentsInChildren<SpriteRenderer>();
                    Assert.That(sprite.Length,Is.GreaterThanOrEqualTo(7));
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(sprite[5].transform.eulerAngles.z,angle*Mathf.Rad2Deg)),Is.LessThan(.01f));
                }
                finally{Object.DestroyImmediate(go);}
            }
            yield return null;
        }
    }
}
#endif
