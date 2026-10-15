#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N254JungleSceneRepro
    {
        DC1MenuPreferenceScope preferences;
        CityGridController core;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            preferences=new DC1MenuPreferenceScope();
            DC1Preferences.SetInt("DC1_CityGrind_Cash",9876);
            DC1Preferences.SetInt("DC1_CityGrind_Level",42);
            DC1Preferences.SetInt("DC1_CityGrind_Tier_0",0);
            DC1Preferences.SetInt(CityGrindStartPreset.AmmoPrefPrefix+0,12);
            SceneManager.sceneLoaded+=AddJungleOwner;
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            SceneManager.sceneLoaded-=AddJungleOwner;
            core=Object.FindFirstObjectByType<CityGridController>();
        }
        static void AddJungleOwner(Scene scene,LoadSceneMode mode)
        {
            var c=Object.FindFirstObjectByType<CityGridController>();
            Assert.That(c,Is.Not.Null);
            c.gameObject.AddComponent<JungleBlitzController>().SetCarriedSession(321,2);
        }
        [TearDown]
        public void TearDown()
        {
            SceneManager.sceneLoaded-=AddJungleOwner;
            preferences?.Dispose();preferences=null;
        }
        [Test]
        public void NativeFreshInventoryIsIndependentOfCommittedCitySave()
        {
            Assert.That(core.TestHarness_IsJungle,Is.True);
            Assert.That(core.TestHarness_RangedFamilyTier(0),Is.EqualTo(3));
            Assert.That(core.TestHarness_AmmoFor(0),Is.EqualTo(999));
            for(int i=1;i<6;i++) {
                Assert.That(core.TestHarness_RangedFamilyTier(i),Is.EqualTo(-1));
                Assert.That(core.TestHarness_AmmoFor(i),Is.EqualTo(0));
            }
            Assert.That(core.TestHarness_PlayerHP,Is.EqualTo(100));
            Assert.That(core.TestHarness_JungleCash,Is.EqualTo(321));
            Assert.That(core.TestHarness_JungleLives,Is.EqualTo(2));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_OwnershipModelMigratedV1",-99),Is.EqualTo(-99));
        }
        [Test]
        public void JungleSceneBuildsOnlyNativeWorldAndStart()
        {
            Assert.That(core.TestHarness_JungleWorld,Is.Not.Null);
            Assert.That(core.TestHarness_JungleWorld.TileCount,Is.GreaterThanOrEqualTo(2));
            Assert.That(core.TestHarness_GameplayWorldRight,Is.EqualTo(2000000000f));
            Assert.That(core.TestHarness_LevelIntroActive,Is.True);
            Assert.That(Object.FindObjectsByType<JungleWorldLayers>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            Assert.That(GameObject.Find("CityLevelWorld_1"),Is.Null);
        }
        [UnityTest] public IEnumerator NativeStartForcesRightWalkUntilStartCompletes()
        {
            float initial=core.TestHarness_PlayerX;
            yield return null;yield return null;
            Assert.That(core.TestHarness_LevelIntroActive,Is.True);
            Assert.That(core.TestHarness_PlayerX,Is.GreaterThan(initial));
            float deadline=Time.realtimeSinceStartup+5f;
            while(core.TestHarness_LevelIntroActive && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(core.TestHarness_LevelIntroActive,Is.False);
            float stopped=core.TestHarness_PlayerX;
            yield return null;yield return null;
            Assert.That(core.TestHarness_PlayerX,Is.EqualTo(stopped));
        }
        [Test]
        public void JungleSaveOwnerCannotOverwriteCampaign()
        {
            core.TestHarness_SetCash(555);
            core.TestHarness_SaveProgression();
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.EqualTo(9876));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Level"),Is.EqualTo(42));
            Assert.That(DC1Preferences.GetInt(CityGrindStartPreset.AmmoPrefPrefix+0),Is.EqualTo(12));
        }
        [Test]
        public void NativeTimerWaitsForStartAndKeepsOneSecondPerLateCallback()
        {
            core.enabled=false;
            core.TestHarness_JungleTickTimers(10);
            Assert.That(core.TestHarness_JungleSeconds,Is.Zero);
            Assert.That(core.TestHarness_DinoCount,Is.Zero);
            core.TestHarness_SkipLevelIntro();
            core.TestHarness_JungleTickTimers(0);
            core.TestHarness_JungleTickTimers(.5f);
            Assert.That(core.TestHarness_JungleSeconds,Is.Zero);
            core.TestHarness_JungleTickTimers(.5f);
            Assert.That(core.TestHarness_JungleSeconds,Is.EqualTo(1));
            core.TestHarness_JungleTickTimers(5);
            Assert.That(core.TestHarness_JungleSeconds,Is.EqualTo(2));
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void SpawnedActorUsesNativeJungleRowWithoutCityLevelScaling(int type)
        {
            core.enabled=false;
            core.TestHarness_JungleSpawn(type);
            Assert.That(core.TestHarness_DinoCount,Is.EqualTo(1));
            var expected=JungleBlitzProgression.Load().QuerySpawn(0,type,core.TestHarness_Difficulty);
            Assert.That(core.TestHarness_DinoHP(0),Is.EqualTo(expected.hp));
            Assert.That(core.TestHarness_BuildingCount,Is.Zero);
        }
        [Test]
        public void FatalOwnerDropsAtNativeGroundOffsetAndCreditsOnlyOnce()
        {
            core.enabled=false;core.TestHarness_JungleRawDraw(()=>0);
            core.TestHarness_JungleSpawn(1);
            float x=core.TestHarness_DinoX(0);
            core.TestHarness_ApplyRangedHitToDino(0,1000);
            Assert.That(core.TestHarness_JungleKills,Is.EqualTo(1));
            Assert.That(core.TestHarness_CashTotal,Is.EqualTo(341));
            Assert.That(core.TestHarness_JunglePickupCount,Is.EqualTo(1));
            var pickup=core.TestHarness_JunglePickup(0);
            Assert.That(pickup.Decision.frame,Is.EqualTo("Lb1Frame"));
            Assert.That(pickup.State.X,Is.EqualTo(x));Assert.That(pickup.ItemY,Is.EqualTo(-74));
            core.TestHarness_ApplyRangedHitToDino(0,1000);
            Assert.That(core.TestHarness_JungleKills,Is.EqualTo(1));
            Assert.That(core.TestHarness_JunglePickupCount,Is.EqualTo(1));
        }
        [Test]
        public void HealthCollectionUsesStrictXRangeAndIndependentFiveSecondIndicator()
        {
            core.enabled=false;core.TestHarness_SetPlayerHP(60);
            float x=core.TestHarness_PlayerX;
            var d=new JungleDropSelection.Decision {frame="LxuepingFrame",kind=JunglePickupState.Kind.Health,family=-1,tier=-1};
            var pickup=core.TestHarness_JungleCreatePickup(d,x+80,-94);
            core.TestHarness_JungleTickPickups(0);
            Assert.That(core.TestHarness_PlayerHP,Is.EqualTo(60));
            core.TestHarness_SetPlayerX(x+1);core.TestHarness_JungleTickPickups(0);
            Assert.That(core.TestHarness_PlayerHP,Is.EqualTo(85));
            Assert.That(pickup.State.HealIndicator,Is.True);Assert.That(pickup.BeamActive,Is.False);
            core.TestHarness_JungleTickPickups(.2f);
            Assert.That(pickup.ItemY,Is.EqualTo(-54));Assert.That(core.TestHarness_PlayerHP,Is.EqualTo(85));
            core.TestHarness_JungleTickTimers(4.99f);Assert.That(pickup.State.Active,Is.True);
            core.TestHarness_JungleTickTimers(.02f);Assert.That(pickup.State.Active,Is.False);
        }
        [Test]
        public void WeaponPickupRebindsHigherFamilyAndRetiresOwnedBeamImmediately()
        {
            core.enabled=false;
            var d=new JungleDropSelection.Decision {frame="Lb1Frame",kind=JunglePickupState.Kind.Weapon,family=1,tier=0};
            var pickup=core.TestHarness_JungleCreatePickup(d,core.TestHarness_PlayerX,-94);
            core.TestHarness_JungleTickPickups(0);
            Assert.That(core.TestHarness_RangedFamilyTier(1),Is.EqualTo(0));
            Assert.That(core.TestHarness_AmmoFor(1),Is.EqualTo(50));
            Assert.That(pickup.State.Active,Is.False);Assert.That(pickup.BeamActive,Is.False);
            core.TestHarness_JungleTickTimers(9.99f);Assert.That(pickup.BeamActive,Is.False);
            core.TestHarness_JungleTickTimers(.02f);Assert.That(pickup.BeamActive,Is.False);
        }
        [Test]
        public void AmmoCollectionUsesRealSceneNotificationOwner()
        {
            core.enabled=false;core.TestHarness_JungleRawDraw(()=>0);
            var d=new JungleDropSelection.Decision {frame="Lbullet1Frame",kind=JunglePickupState.Kind.Ammo,family=1,tier=-1};
            core.TestHarness_JungleCreatePickup(d,core.TestHarness_PlayerX,-94);
            core.TestHarness_JungleTickPickups(0);
            Assert.That(core.TestHarness_AmmoFor(1),Is.EqualTo(8));
            Assert.That(core.TestHarness_JungleNotification.Displaying,Is.True);
            Assert.That(core.TestHarness_JungleNotification.Family,Is.EqualTo(1));
            Assert.That(core.TestHarness_JungleNotification.Amount,Is.EqualTo(8));
            Assert.That(core.TestHarness_JungleNotification.transform.position.y,Is.EqualTo(-74));
        }
        [Test]
        public void K4UsesOriginalAttackFrame10ProjectileAndCannotBite()
        {
            core.enabled=false;core.TestHarness_JungleRawDraw(()=>0);
            int i=core.TestHarness_DinoCount;
            core.TestHarness_JungleSpawn(4);
            Assert.That(core.TestHarness_DinoCount,Is.EqualTo(i+1));
            core.TestHarness_ForceDinoClip(i,"_attack");
            core.TestHarness_FireDinoPrimaryFrame(i,9);
            Assert.That(Object.FindObjectsByType<JungleProjectileActor>(FindObjectsSortMode.None).Length,Is.Zero);
            core.TestHarness_FireDinoPrimaryFrame(i,10);
            var p=Object.FindFirstObjectByType<JungleProjectileActor>();Assert.That(p,Is.Not.Null);
            Assert.That(p.State.Position.y,Is.EqualTo(-9));
            float hp=core.TestHarness_PlayerHP;
            core.TestHarness_FireDinoPrimaryFrame(i,12);
            Assert.That(core.TestHarness_PlayerHP,Is.EqualTo(hp));
        }
        [Test]
        public void ProjectileDamageReadsUnscaledCurrentTableWithoutBiteStun()
        {
            core.enabled=false;core.TestHarness_GodMode=false;core.TestHarness_SetDifficulty(3);
            float hp=core.TestHarness_PlayerHP;
            Assert.That(core.TestHarness_JungleProjectileHit(),Is.True);
            Assert.That(core.TestHarness_PlayerHP,Is.EqualTo(hp-10));
            Assert.That(core.TestHarness_PlayerDead,Is.False);
        }
        [Test]
        public void JungleTimerDebugChangesOnlyGTimeAndLeavesCampaignAndPickupClockNormal()
        {
            core.enabled=false;core.TestHarness_SkipLevelIntro();
            core.TestHarness_JungleTickTimers(0);
            var d=new JungleDropSelection.Decision {frame="Lbullet1Frame",kind=JunglePickupState.Kind.Ammo,family=1,tier=-1};
            var pickup=core.TestHarness_JungleCreatePickup(d,core.TestHarness_PlayerX+1000,-94);
            core.TestHarness_JungleSetTimerSpeed(10);
            core.TestHarness_JungleTickTimers(.1f);
            Assert.That(core.TestHarness_JungleSeconds,Is.EqualTo(1));
            Assert.That(pickup.State.Active,Is.True);
            core.TestHarness_JungleSetTime(10150);
            Assert.That(core.TestHarness_JungleSeconds,Is.EqualTo(10150));
            core.TestHarness_JungleResetDebug();
            Assert.That(core.TestHarness_JungleSeconds,Is.Zero);
            Assert.That(core.TestHarness_JungleTimerSpeed,Is.EqualTo(1));
            Assert.That(core.TestHarness_GodMode,Is.False);Assert.That(core.TestHarness_InfiniteAmmo,Is.False);
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.EqualTo(9876));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Level"),Is.EqualTo(42));
        }
        [Test]
        public void JungleDebugPreferencesAreSeparateAndTimeIsRuntimeOnly()
        {
            core.TestHarness_JungleSetTimerSpeed(1000);
            Assert.That(core.TestHarness_JungleTimerSpeed,Is.EqualTo(100));
            Assert.That(DC1Preferences.GetFloat("DC1_Debug_Jungle_TimerSpeed"),Is.EqualTo(100));
            core.TestHarness_JungleSetTime(-1);Assert.That(core.TestHarness_JungleSeconds,Is.Zero);
            core.TestHarness_JungleSetTime(999999);Assert.That(core.TestHarness_JungleSeconds,Is.EqualTo(86400));
            Assert.That(DC1Preferences.HasKey("DC1_Debug_Jungle_Time"),Is.False);
            core.TestHarness_JungleSetTimerSpeed(float.NaN);
            Assert.That(core.TestHarness_JungleTimerSpeed,Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator ActualDeathShowsJungleResultsFreezesTimeAndReturnsWithoutCitySave()
        {
            DC1Preferences.SetInt(CityGridController.LivesPrefKey,7);
            core.TestHarness_GodMode=false;
            core.TestHarness_SkipLevelIntro();core.TestHarness_ForcePlayerInvulnExpired();
            core.TestHarness_DamagePlayer(1000);
            Assert.That(core.TestHarness_PlayerDead,Is.True);
            float deadline=Time.realtimeSinceStartup+8;
            while(!core.TestHarness_JungleResultsVisible && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(core.TestHarness_JungleResultsVisible,Is.True);
            Assert.That(core.TestHarness_JungleLives,Is.EqualTo(2));
            Assert.That(DC1Preferences.GetInt(CityGridController.LivesPrefKey),Is.EqualTo(7));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.EqualTo(9876));
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Level"),Is.EqualTo(42));
            int seconds=core.TestHarness_JungleSeconds;
            yield return new WaitForSecondsRealtime(1.2f);
            Assert.That(core.TestHarness_JungleSeconds,Is.EqualTo(seconds));
            core.TestHarness_JungleRequestDone();
            deadline=Time.realtimeSinceStartup+4;
            while(Object.FindFirstObjectByType<MainMenuController>()==null && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(Object.FindFirstObjectByType<MainMenuController>(),Is.Not.Null);
            Assert.That(Power2DSourceTruthRenderer.GlobalPaused,Is.False);
        }
    }
}
#endif
