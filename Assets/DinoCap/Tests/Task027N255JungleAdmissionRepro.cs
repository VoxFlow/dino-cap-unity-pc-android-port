#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N255JungleAdmissionRepro
    {
        CityGridController core;DC1MenuPreferenceScope preferences;
        [UnitySetUp] public IEnumerator SetUp()
        {
            preferences=new DC1MenuPreferenceScope();
            SceneManager.sceneLoaded+=AddJungle;
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;SceneManager.sceneLoaded-=AddJungle;
            core=UnityEngine.Object.FindFirstObjectByType<CityGridController>();core.enabled=false;
        }
        static void AddJungle(Scene s,LoadSceneMode m)=>UnityEngine.Object.FindFirstObjectByType<CityGridController>().gameObject.AddComponent<JungleBlitzController>();
        [TearDown] public void TearDown(){SceneManager.sceneLoaded-=AddJungle;preferences?.Dispose();}
        [Test] public void FutureFilterRetainsExistingUntilExplicitRemovalAndConsumesPlacementDraws()
        {
            // This case exercises the stationary placement branch. Native
            // Jungle intro now correctly starts with forced right movement.
            core.TestHarness_SkipLevelIntro();
            typeof(CityGridController).GetField("junglePlayerPhysicsVelocity",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(core,0f);
            int draws=0;core.TestHarness_JungleRawDraw(()=>{draws++;return 0;});
            core.TestHarness_JungleSpawn(1);
            Assert.That(core.TestHarness_DinoCount,Is.EqualTo(1));
            core.TestHarness_JungleSpeciesFilter(8,false);
            Assert.That(core.TestHarness_DinoMarkedForRemoval(0),Is.False);
            int before=draws;core.TestHarness_JungleSpawn(1);
            Assert.That(draws-before,Is.EqualTo(2)); // Walk decision and stationary side.
            Assert.That(core.TestHarness_DinoCount,Is.EqualTo(1));
            core.TestHarness_JungleSpawn(4);
            Assert.That(core.TestHarness_DinoCount,Is.EqualTo(2));
            core.TestHarness_JungleSpeciesFilter(8,true);
            Assert.That(core.TestHarness_DinoMarkedForRemoval(0),Is.True);
            Assert.That(core.TestHarness_DinoMarkedForRemoval(1),Is.False);
            core.TestHarness_ReloadDebugSettings();
            Assert.That(core.TestHarness_JungleSpeciesMask,Is.EqualTo(8));
        }
        [Test] public void NativeTimerQueueNaturallyAdmitsK4At10150NotDebug999()
        {
            var queue=new NativeSceneTimerQueue();JungleSpawnSchedule schedule=null;
            int k4=0;
            schedule=new JungleSpawnSchedule(JungleBlitzProgression.Load(),(type,delay)=>queue.Arm(delay,handle=>schedule.OnDinoTimer(handle,t=>{if(t==4)k4++;})));
            Action<int> second=null;second=_=>{schedule.OnSecond();queue.Arm(1,second);};
            queue.Arm(1,second);schedule.Start();
            for(int i=1;i<=10149;i++)
            {
                queue.Tick(1);
                if(i==999)Assert.That(k4,Is.Zero);
            }
            Assert.That(k4,Is.Zero);Assert.That(schedule.ElapsedSeconds,Is.EqualTo(10149));
            queue.Tick(1);
            Assert.That(k4,Is.EqualTo(1),"Real timer dispatch, not a direct staged K4 call.");
        }
        [Test] public void JungleDebugAmmoDepletionUsesSharedInventoryWithoutPublishingCarryover()
        {
            var before=NativeModeSession.Capture();
            core.TestHarness_DebugDepleteAllAmmo();
            for(int i=0;i<6;i++)Assert.That(core.TestHarness_AmmoFor(i),Is.Zero);
            core.TestHarness_SaveProgression();
            Assert.That(NativeModeSession.Capture().cash,Is.EqualTo(before.cash));
        }
    }
}
#endif
