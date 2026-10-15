#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    // Native rage face (sub_66DCC counter +132 / suffix +108, "fireup" named timers sub_66110, sub_663A0, sub_65A08, sub_666BC, sub_68148, sub_68CD8):
    //  - face re-set per C/D shot (10 / 30 / 50), neutral again via a 2.0 s "fireup" timer scheduled at C/D fire end and on every weapon equip;
    //  - erased immediately by being hurt, entering/exiting a house, and starting a melee swing while fire is active.
    public class DC1RageFaceNativeRepro
    {
        CityGridController city;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            foreach(string key in new[]{"DC1_Debug_CityGrind_GodMode","DC1_Debug_CityGrind_InfiniteAmmo"}) DC1Preferences.DeleteKey(key);
            NativeAudioSessionGate.StopAll();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            city = Object.FindFirstObjectByType<CityGridController>();
            city.TestHarness_SkipLevelIntro();
            city.TestHarness_ClearDinos();
            city.TestHarness_DebugUiHidden = true;
            city.TestHarness_SetRangedFamily(2);
            city.TestHarness_GrantAmmo(2, 100);
            city.TestHarness_GrantAmmo(0, 100);
        }

        void RageToFace3()
        {
            city.TestHarness_Shoot();                                   // C/D fire start: fire-active, counter 0
            for(int i = 0; i < 50; i++) city.TestHarness_AdvanceSustainedFireFace();
            Assert.AreEqual("tou_face3", city.TestHarness_HeadRef);
        }

        IEnumerator WaitSeconds(float s)
        {
            float t0 = Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup - t0 < s) yield return null;
        }

        // The reported bug: switching weapon while raging left the face permanently on.
        [UnityTest]
        public IEnumerator SwitchingWeaponWhileRagingStillDecaysToNeutralAfterTwoSeconds()
        {
            RageToFace3();
            city.TestHarness_RequestNextWeapon();
            yield return null;
            Assert.AreEqual("tou_face3", city.TestHarness_HeadRef, "Native does not erase the face at the switch itself.");
            yield return WaitSeconds(2.4f);
            Assert.AreEqual("", city.TestHarness_HeadRef, "sub_65A08 schedules fireup (2 s) on every equip: the face must return to neutral.");
        }

        [UnityTest]
        public IEnumerator MeleeStartWhileFireActiveErasesTheFaceAtOnce()
        {
            RageToFace3();
            city.TestHarness_Melee();
            Assert.AreEqual("", city.TestHarness_HeadRef, "sub_666BC: fire byte set -> face suffix erased immediately.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FireEndSchedulesTwoSecondDecay()
        {
            RageToFace3();
            // end the fire run: the secondary clip completes without a held request
            float until = Time.realtimeSinceStartup + 3f;
            while(city.TestHarness_PlayerSecondaryActive && Time.realtimeSinceStartup < until) yield return null;
            Assert.AreEqual("tou_face3", city.TestHarness_HeadRef, "Just after the run ends the face is still shown (timer pending).");
            yield return WaitSeconds(2.4f);
            Assert.AreEqual("", city.TestHarness_HeadRef, "sub_663A0 final branch schedules fireup: neutral after 2 s.");
        }

        // Native sub_68CD8 entry: fire byte set -> face erased AND the byte cleared, so the next fire start resets the counter to 0.
        [UnityTest]
        public IEnumerator EnteringAHouseWhileFiringErasesFaceAndTheNextFireRestartsFromZero()
        {
            RageToFace3();
            city.TestHarness_SetPlayerNear(0);
            yield return null;
            city.TestHarness_PressDoor();
            Assert.AreEqual("", city.TestHarness_HeadRef, "sub_68CD8 erases the face when fire was active.");
            yield return WaitSeconds(0.7f);
            city.TestHarness_PressDoor();                               // exit
            yield return WaitSeconds(1.0f);
            Assert.IsFalse(city.TestHarness_PlayerControlSuspended, "Exit completed.");
            city.TestHarness_Shoot();
            Assert.AreEqual(0, city.TestHarness_SustainedShotCounter, "Fire-active byte was cleared on entry, so the new fire run restarts the counter at 0 (not from rage 3).");
        }

        // Entry while fire is NOT active (inside the 2 s decay window) leaves the face alone.
        [UnityTest]
        public IEnumerator EnteringAHouseAfterFireEndedKeepsTheFace()
        {
            RageToFace3();
            float until = Time.realtimeSinceStartup + 3f;
            while(city.TestHarness_PlayerSecondaryActive && Time.realtimeSinceStartup < until) yield return null;
            city.TestHarness_SetPlayerNear(0);
            yield return null;
            city.TestHarness_PressDoor();
            Assert.AreEqual("tou_face3", city.TestHarness_HeadRef, "sub_68CD8 only erases when the fire byte was set.");
        }

        // Native sub_68148 erases the face only when the fire byte was set; a bite during the 2 s decay window leaves it to the timer.
        [UnityTest]
        public IEnumerator HurtAfterFireEndedKeepsTheFace()
        {
            RageToFace3();
            float until = Time.realtimeSinceStartup + 3f;
            while(city.TestHarness_PlayerSecondaryActive && Time.realtimeSinceStartup < until) yield return null;
            city.TestHarness_DamagePlayer(1f);
            Assert.AreEqual("tou_face3", city.TestHarness_HeadRef, "No fire active at the bite: the face is not erased by it.");
        }

        // Native sub_76A20 draws each part as ref + face suffix + "Frame" if that sprite exists: the hurt clip's head ref is "tou 1", so the hurt face
        // shows while the suffix (still held until its "fireup" timer) waits; afterwards the stand clip's "tou" shows the rage face again.
        [UnityTest]
        public IEnumerator BiteAfterFireEndedShowsTheHurtFaceThenTheRageFaceAgainUntilTheTimer()
        {
            RageToFace3();
            float until = Time.realtimeSinceStartup + 3f;
            while(city.TestHarness_PlayerSecondaryActive && Time.realtimeSinceStartup < until) yield return null;
            yield return null; yield return null;
            Assert.AreEqual("tou_face3", city.TestHarness_RenderedHeadRef, "Sanity: rage face rendered after the fire run ended.");
            city.TestHarness_DamagePlayer(1f);
            yield return null; yield return null;
            Assert.AreEqual("tou 1", city.TestHarness_RenderedHeadRef, "The hurt clip shows its own hurt face, not the rage face.");
            Assert.AreEqual("tou_face3", city.TestHarness_HeadRef, "The face suffix is still held (timer pending).");
            float t0 = Time.realtimeSinceStartup;
            while(city.TestHarness_RenderedHeadRef != "tou_face3" && Time.realtimeSinceStartup - t0 < 1.2f) yield return null;
            Assert.AreEqual("tou_face3", city.TestHarness_RenderedHeadRef, "Hurt clip over: the rage face returns while the 2 s timer is still pending.");
            yield return WaitSeconds(2.4f);
            Assert.AreEqual("tou", city.TestHarness_RenderedHeadRef, "After the timer the head is neutral.");
        }

        [UnityTest]
        public IEnumerator HurtErasesTheFaceImmediately()
        {
            RageToFace3();
            city.TestHarness_DamagePlayer(1f);
            Assert.AreEqual("", city.TestHarness_HeadRef);
            yield return null;
        }
    }
}
#endif
