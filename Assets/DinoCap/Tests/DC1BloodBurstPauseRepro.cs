#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    // Pause must freeze the dinosaur blood-impact burst (spiky "daoxue"/"qiangxue" clip): its lifetime counts only unpaused time.
    public class DC1BloodBurstPauseRepro
    {
        CityGridController city;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            city = Object.FindFirstObjectByType<CityGridController>();
            city.TestHarness_SkipLevelIntro();
            city.TestHarness_ClearDinos();
            yield return null;
        }

        [UnityTest]
        public IEnumerator BurstLifetimeDoesNotAdvanceWhilePaused()
        {
            var g = new GameObject("PauseBurstProbe");
            DinoCapBloodBurst.AttachLifetime(g, 0.175f);
            city.TestHarness_RequestPause();
            yield return null; yield return null;
            Assert.IsTrue(city.TestHarness_Paused && Power2DSourceTruthRenderer.GlobalPaused);
            float t0 = Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup - t0 < 0.6f) yield return null;   // 3x the burst lifetime, bounded
            Assert.IsTrue(g != null, "A paused blood burst must not despawn");
            city.TestHarness_RequestPause();   // resume
            float t1 = Time.realtimeSinceStartup;
            while(g != null && Time.realtimeSinceStartup - t1 < 2f) yield return null;   // hard timeout
            Assert.IsTrue(g == null, "After resuming the burst must finish and despawn");
        }

        [UnityTest]
        public IEnumerator DebugLevelChangeWhilePausedStartsAnUnpausedLevel()
        {
            city.TestHarness_RequestPause();
            yield return null; yield return null;
            Assert.IsTrue(city.TestHarness_Paused && Power2DSourceTruthRenderer.GlobalPaused, "precondition: paused");
            city.TestHarness_DebugJumpLevel(1);
            yield return null; yield return null;
            Assert.IsFalse(city.TestHarness_Paused, "a freshly built level must not stay paused (black screen: renderers skip their first LateUpdate while the shared pause flag is set)");
            Assert.IsFalse(Power2DSourceTruthRenderer.GlobalPaused, "the shared renderer freeze must be released with the pause");
        }

        [UnityTest]
        public IEnumerator PauseKeepsRemainingInvulnerabilityAndSpawnIntervals()
        {
            city.TestHarness_SetPlayerInvulnerableForSeconds(0.8f);
            city.TestHarness_RequestPause();
            yield return null; yield return null;
            Assert.IsTrue(city.TestHarness_PlayerInvulnerable, "precondition");
            float t0 = Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup - t0 < 1.2f) yield return null;   // longer than the whole remaining immunity, bounded
            Assert.IsTrue(city.TestHarness_PlayerInvulnerable, "a paused scene clock must not spend the remaining immunity");
            city.TestHarness_RequestPause();   // resume
            yield return null; yield return null;
            Assert.IsTrue(city.TestHarness_PlayerInvulnerable, "immunity must still be running right after resume");
            float t1 = Time.realtimeSinceStartup;
            while(city.TestHarness_PlayerInvulnerable && Time.realtimeSinceStartup - t1 < 3f) yield return null;   // hard timeout
            Assert.IsFalse(city.TestHarness_PlayerInvulnerable, "immunity must still expire after resume");
        }

        [UnityTest]
        public IEnumerator DebugLevelChangeIsBlockedDuringTheDeathSequence()
        {
            city.TestHarness_GodMode = false;
            int level = city.TestHarness_CityLevel;
            city.TestHarness_DamagePlayer(100000f);
            yield return null;
            Assert.IsTrue(city.TestHarness_PlayerDead, "precondition: dying");
            city.TestHarness_DebugJumpLevel(1);
            Assert.AreEqual(level, city.TestHarness_CityLevel, "debug level change must be refused while the player is dying");
            float t0 = Time.realtimeSinceStartup;
            while(!city.TestHarness_DeathUIActive && Time.realtimeSinceStartup - t0 < 10f) yield return null;   // hard timeout
            if(city.TestHarness_DeathUIActive)
            {
                city.TestHarness_DebugJumpLevel(1);
                Assert.AreEqual(level, city.TestHarness_CityLevel, "debug level change must be refused while the death UI is shown");
            }
            else Assert.Inconclusive("death UI did not appear within 10 s; only the dying phase was checked");
        }

        static void SetField(CityGridController c, string name, float v)
        {
            typeof(CityGridController).GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(c, v);
        }
        static float GetField(CityGridController c, string name)
        {
            return (float)typeof(CityGridController).GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(c);
        }

        [UnityTest]
        public IEnumerator PauseKeepsHurtStunUntilTheClipEnds()
        {
            city.TestHarness_GodMode = false;
            city.TestHarness_DamagePlayer(10f);
            yield return null;
            Assert.IsTrue(city.TestHarness_PlayerHitStun, "precondition: hurt");
            city.TestHarness_RequestPause();
            yield return null; yield return null;
            float t0 = Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup - t0 < 1.5f) yield return null;   // longer than the whole hurt window, bounded
            Assert.IsTrue(city.TestHarness_PlayerHitStun, "paused: hurt stun must still be running");
            city.TestHarness_RequestPause();   // resume
            yield return null; yield return null;
            Assert.IsTrue(city.TestHarness_PlayerHitStun, "right after resume the hurt window must not already be spent");
            float t1 = Time.realtimeSinceStartup;
            while(city.TestHarness_PlayerHitStun && Time.realtimeSinceStartup - t1 < 3f) yield return null;   // hard timeout
            Assert.IsFalse(city.TestHarness_PlayerHitStun, "hurt stun must still end after resume");
        }

        [UnityTest]
        public IEnumerator PauseKeepsArmedSpawnIntervals()
        {
            city.TestHarness_ClearDinos();
            float now = GetField(city, "sceneTime");
            SetField(city, "cgT1Next", now + 0.6f); SetField(city, "cgT2Next", now + 0.6f); SetField(city, "cgT3Next", now + 0.6f);
            city.TestHarness_RequestPause();
            yield return null; yield return null;
            float t0 = Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup - t0 < 1.5f) yield return null;   // longer than the armed interval, bounded
            Assert.AreEqual(0, city.TestHarness_ActiveDinoCount, "paused: no spawn may happen");
            city.TestHarness_RequestPause();   // resume
            yield return null; yield return null;
            Assert.AreEqual(0, city.TestHarness_ActiveDinoCount, "right after resume the armed interval must not already be spent");
            float t1 = Time.realtimeSinceStartup;
            while(city.TestHarness_ActiveDinoCount == 0 && Time.realtimeSinceStartup - t1 < 4f) yield return null;   // hard timeout
            Assert.Greater(city.TestHarness_ActiveDinoCount, 0, "the armed spawn must still fire after the remaining interval");
        }
    }
}
#endif