#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    // Native arbitration (sub_66550 state 3 = primary-track completion during melee, sub_667B0 fire gate state<=2, sub_666BC melee entry,
    // touch begin/end sub_56E94 setting/clearing byte_21BE74 melee flag and byte_21BE78 fire flag):
    //  - a shoot press while the swing runs is rejected (state 3) and is NOT queued;
    //  - at swing completion: melee flag still held -> the swing re-arms (melee never yields); else fire flag held -> sub_667B0 fires;
    //  - melee has no cooldown (Task023).
    public class DC1MeleeRangedArbitrationRepro
    {
        CityGridController city;
        static readonly Vector2 MeleeCentre = new Vector2(829.5f, 625f);
        static readonly Vector2 ShootCentre = new Vector2(960.5f, 594f);
        readonly System.Collections.Generic.Dictionary<string,int?> savedDebugPrefs = new System.Collections.Generic.Dictionary<string,int?>();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savedDebugPrefs.Clear();
            foreach(string key in new[]{"DC1_Debug_CityGrind_GodMode","DC1_Debug_CityGrind_InfiniteAmmo",NativeAudioSessionGate.FxMutedPrefKey,"DC1_MainMenu_SfxOn"}) {
                savedDebugPrefs[key]=DC1Preferences.HasKey(key)?(int?)DC1Preferences.GetInt(key):null;
                DC1Preferences.DeleteKey(key);
            }
            NativeAudioSessionGate.StopAll();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            city = Object.FindFirstObjectByType<CityGridController>();
            Assert.IsNotNull(city);
            city.TestHarness_SkipLevelIntro();
            city.TestHarness_ClearDinos();
            city.TestHarness_DebugUiHidden = true;
            city.TestHarness_SetRangedFamily(0);
            city.TestHarness_SetPlayerX(0f);
            city.TestHarness_GrantAmmo(0, 20);
            city.TestHarness_ResetShootCooldown();
        }

        [TearDown]
        public void Restore()
        {
            foreach(var entry in savedDebugPrefs)
                if(entry.Value.HasValue) DC1Preferences.SetInt(entry.Key, entry.Value.Value);
        }

        IEnumerator WaitMeleeActive()
        {
            float until = Time.realtimeSinceStartup + 2f;
            while(!city.TestHarness_PlayerMeleeActive && Time.realtimeSinceStartup < until) yield return null;
            Assert.IsTrue(city.TestHarness_PlayerMeleeActive, "Melee press must start a swing.");
        }

        IEnumerator Wait(float seconds)
        {
            float until = Time.realtimeSinceStartup + seconds;
            while(Time.realtimeSinceStartup < until) yield return null;
        }

        // sub_666BC clears byte_21BE74 when the swing starts, so a finger that merely stays down does not re-arm the swing.
        [UnityTest]
        public IEnumerator HoldingMeleeFromTheStartDoesNotRepeatTheSwing()
        {
            int ammo = city.TestHarness_AmmoFor(0);
            var layer = city.TestHarness_PointerLayer;
            int melee = layer.InjectDown(MeleeCentre);
            yield return WaitMeleeActive();
            yield return Wait(1.2f);                          // > two swing lengths (19 frames x 0.025 s)
            Assert.IsFalse(city.TestHarness_PlayerMeleeActive, "Native clears the melee flag on swing start: a held finger does not re-swing.");
            Assert.AreEqual(ammo, city.TestHarness_AmmoFor(0));
            layer.InjectUp(melee);
        }

        // A second melee press during the swing sets the flag again (the entry is rejected, state 3); at completion the swing re-arms and keeps
        // re-arming while that touch is down. A fire control held the whole time never gets to shoot.
        [UnityTest]
        public IEnumerator MeleeRetapDuringSwingRearmsAndRangedNeverAppearsWhileItIsDown()
        {
            int ammo = city.TestHarness_AmmoFor(0);
            var layer = city.TestHarness_PointerLayer;
            int first = layer.InjectDown(MeleeCentre);
            yield return WaitMeleeActive();
            yield return null;
            layer.InjectUp(first);
            int fire = layer.InjectDown(ShootCentre);         // fire held through everything below
            yield return null;
            int second = layer.InjectDown(MeleeCentre);       // re-press mid-swing, stays down
            yield return Wait(1.4f);
            Assert.IsTrue(city.TestHarness_PlayerMeleeActive, "Melee flag set at completion: the swing re-arms (melee never yields).");
            Assert.AreEqual(ammo, city.TestHarness_AmmoFor(0), "No ranged shot while the melee flag keeps being set.");
            Assert.IsFalse(city.TestHarness_DeferredFireQueued, "Native has no press queue.");
            layer.InjectUp(second);                           // flag cleared by the matching touch end
            float until = Time.realtimeSinceStartup + 2.5f;
            while(city.TestHarness_AmmoFor(0) == ammo && Time.realtimeSinceStartup < until) yield return null;
            layer.InjectUp(fire);
            Assert.Less(city.TestHarness_AmmoFor(0), ammo, "Only once the melee flag is clear does the held fire control shoot.");
        }

        // Native end handler compares the touch id with the LATEST recorded melee touch only.
        [UnityTest]
        public IEnumerator EndingTheLatestMeleeTouchClearsTheFlagEvenIfAnEarlierFingerIsStillDown()
        {
            var layer = city.TestHarness_PointerLayer;
            int a = layer.InjectDown(MeleeCentre);            // starts the swing, flag cleared
            yield return WaitMeleeActive();
            yield return null;
            int b = layer.InjectDown(MeleeCentre);            // flag set, id = b
            yield return null;
            layer.InjectUp(b);                                // matching end clears the flag; finger a is still physically down
            yield return Wait(1.2f);
            Assert.IsFalse(city.TestHarness_PlayerMeleeActive, "Flag was cleared by the latest touch's end: no re-arm although the first finger is still down.");
            layer.InjectUp(a);
        }

        // An earlier touch ending does not clear a flag owned by a later touch.
        [UnityTest]
        public IEnumerator EndingAnEarlierMeleeTouchDoesNotClearTheLaterTouchFlag()
        {
            var layer = city.TestHarness_PointerLayer;
            int a = layer.InjectDown(MeleeCentre);
            yield return WaitMeleeActive();
            yield return null;
            int b = layer.InjectDown(MeleeCentre);            // flag set, id = b
            yield return null;
            layer.InjectUp(a);                                // id mismatch: flag stays set
            yield return Wait(1.0f);
            Assert.IsTrue(city.TestHarness_PlayerMeleeActive, "Flag still owned by the later touch: the swing re-arms.");
            layer.InjectUp(b);
        }

        [UnityTest]
        public IEnumerator ShootTapInsideSwingIsDroppedWhenMeleeReleased()
        {
            int ammo = city.TestHarness_AmmoFor(0);
            var layer = city.TestHarness_PointerLayer;
            int melee = layer.InjectDown(MeleeCentre);
            yield return WaitMeleeActive();
            yield return null;
            layer.InjectUp(melee);
            int tap = layer.InjectDown(ShootCentre);
            yield return null;
            layer.InjectUp(tap);                              // fire flag cleared before the swing finishes
            yield return Wait(1.5f);
            Assert.IsFalse(city.TestHarness_PlayerMeleeActive, "Melee released: the swing ends.");
            Assert.AreEqual(ammo, city.TestHarness_AmmoFor(0), "A shoot tap that began and ended inside the swing is lost, not replayed afterwards.");
        }

        // Regression: Android/pointer ids are reused (one thumb is always id 0). Native touch identities are unique per touch, so a tap that
        // starts and ends inside the swing must clear the fire flag even when its raw id equals an earlier melee tap's id.
        [UnityTest]
        public IEnumerator SingleThumbMeleeTapThenShootTapInsideSwingDoesNotFireAfterwards()
        {
            int ammo = city.TestHarness_AmmoFor(0);
            var layer = city.TestHarness_PointerLayer;
            int melee = layer.InjectDown(MeleeCentre, 0);
            yield return WaitMeleeActive();
            yield return null;
            layer.InjectUp(melee);                            // melee tap released, id 0 free again
            yield return null;
            int tap = layer.InjectDown(ShootCentre, 0);       // same thumb, same raw id
            yield return null;
            layer.InjectUp(tap);
            yield return Wait(1.5f);
            Assert.IsFalse(city.TestHarness_PlayerMeleeActive);
            Assert.AreEqual(ammo, city.TestHarness_AmmoFor(0), "A shoot tap inside the swing must not fire after it, whatever its raw pointer id.");
        }

        [UnityTest]
        public IEnumerator RapidSingleThumbShootTapsInsideSwingNeverFireAfterwards()
        {
            int ammo = city.TestHarness_AmmoFor(0);
            var layer = city.TestHarness_PointerLayer;
            int melee = layer.InjectDown(MeleeCentre, 0);
            yield return WaitMeleeActive();
            yield return null;
            layer.InjectUp(melee);
            for(int i = 0; i < 4; i++)
            {
                yield return null;
                int tap = layer.InjectDown(ShootCentre, 0);
                yield return null;
                layer.InjectUp(tap);
            }
            yield return Wait(1.5f);
            Assert.AreEqual(ammo, city.TestHarness_AmmoFor(0), "Several rapid taps inside the swing: none may fire afterwards.");
        }

        // User-described iPad behaviour: holding shoot through the swing (and past it) gives exactly one shot for a semi-auto weapon; the player must
        // tap again for the next one.
        [UnityTest]
        public IEnumerator SemiAutoShootHeldThroughAndPastTheSwingFiresExactlyOnce()
        {
            int ammo = city.TestHarness_AmmoFor(0);
            var layer = city.TestHarness_PointerLayer;
            int melee = layer.InjectDown(MeleeCentre, 0);
            yield return WaitMeleeActive();
            yield return null;
            layer.InjectUp(melee);
            int hold = layer.InjectDown(ShootCentre, 1);      // second finger held through the swing
            yield return Wait(2.2f);                          // swing (~0.48 s) plus well beyond it, still holding
            Assert.AreEqual(ammo - 1, city.TestHarness_AmmoFor(0), "Semi-auto: exactly one shot for a press held through the swing.");
            layer.InjectUp(hold);
        }

        // C/D keep their native automatic fire when the fire control is held during and after the swing.
        [UnityTest]
        public IEnumerator AutomaticCdHeldThroughAndPastTheSwingKeepsFiringAutomatically()
        {
            city.TestHarness_SetRangedFamily(2);
            city.TestHarness_GrantAmmo(2, 100);
            int ammo = city.TestHarness_AmmoFor(2);
            var layer = city.TestHarness_PointerLayer;
            int melee = layer.InjectDown(MeleeCentre, 0);
            yield return WaitMeleeActive();
            yield return null;
            layer.InjectUp(melee);
            int hold = layer.InjectDown(ShootCentre, 1);
            yield return Wait(2.2f);
            Assert.LessOrEqual(city.TestHarness_AmmoFor(2), ammo - 4, "C/D: held fire after the swing keeps shooting automatically.");
            layer.InjectUp(hold);
        }

        // Native sub_66550 case 5: when the hurt clip ends, a still-set melee flag starts a melee (sub_666BC), clearing the flag.
        // Double tap then hold = tap 2 sets the flag while the swing rejects it.
        [UnityTest]
        public IEnumerator BiteWhileMeleeRepressIsHeldStartsAnotherMeleeAfterTheHurtClip()
        {
            var layer = city.TestHarness_PointerLayer;
            int tap1 = layer.InjectDown(MeleeCentre, 0);
            yield return WaitMeleeActive();
            yield return null;
            layer.InjectUp(tap1);
            int hold = layer.InjectDown(MeleeCentre, 1);      // tap 2, held: flag set, entry rejected
            yield return null;
            city.TestHarness_DamagePlayer(1f);                // bite
            Assert.IsFalse(city.TestHarness_PlayerMeleeActive, "The bite cancels the swing.");
            float until = Time.realtimeSinceStartup + 3f;
            while(!city.TestHarness_PlayerMeleeActive && Time.realtimeSinceStartup < until) yield return null;
            Assert.IsTrue(city.TestHarness_PlayerMeleeActive, "Hurt clip over, melee flag still set: a new melee starts.");
            layer.InjectUp(hold);
        }

        // Holding from the very start: the swing start cleared the flag, so a bite is not followed by a melee.
        [UnityTest]
        public IEnumerator BiteWhileBlueHeldFromTheStartDoesNotStartAnotherMelee()
        {
            var layer = city.TestHarness_PointerLayer;
            int hold = layer.InjectDown(MeleeCentre, 0);
            yield return WaitMeleeActive();
            yield return null;
            city.TestHarness_DamagePlayer(1f);
            Assert.IsFalse(city.TestHarness_PlayerMeleeActive);
            yield return Wait(2.5f);                          // far longer than the hurt clip
            Assert.IsFalse(city.TestHarness_PlayerMeleeActive, "Flag was cleared at the swing start: nothing restarts after the bite.");
            layer.InjectUp(hold);
        }

        // A fire control still held when the hurt clip ends fires (sub_66550 case 5 -> sub_667B0).
        [UnityTest]
        public IEnumerator BiteWhileShootIsHeldFiresAfterTheHurtClip()
        {
            var layer = city.TestHarness_PointerLayer;
            int ammo = city.TestHarness_AmmoFor(0);
            int hold = layer.InjectDown(ShootCentre, 0);
            float until = Time.realtimeSinceStartup + 2f;
            while(city.TestHarness_AmmoFor(0) == ammo && Time.realtimeSinceStartup < until) yield return null;
            Assert.AreEqual(ammo - 1, city.TestHarness_AmmoFor(0), "First shot on the press.");
            yield return Wait(0.5f);
            city.TestHarness_DamagePlayer(1f);
            yield return Wait(2.5f);
            Assert.LessOrEqual(city.TestHarness_AmmoFor(0), ammo - 2, "Fire flag still set when the hurt clip ended: a second shot.");
            layer.InjectUp(hold);
        }

        // Native sub_68080 (manual weapon switch) clears byte_21BE74/byte_21BE78 before advancing the family: the swing in progress finishes but
        // the automatic repeat (flag re-arm in sub_66550 case 3) ends, and the new weapon is equipped.
        [UnityTest]
        public IEnumerator SwitchingWeaponDuringAutomaticMeleeRepeatCancelsTheRepeat()
        {
            city.TestHarness_SetRangedFamilyTierRaw(1, 0);
            city.TestHarness_GrantAmmo(1, 20);
            var layer = city.TestHarness_PointerLayer;
            int tap1 = layer.InjectDown(MeleeCentre, 0);
            yield return WaitMeleeActive();
            yield return null;
            layer.InjectUp(tap1);
            int hold = layer.InjectDown(MeleeCentre, 1);      // tap 2 held: automatic repeat armed
            yield return Wait(1.2f);                          // more than two swings
            Assert.IsTrue(city.TestHarness_PlayerMeleeActive, "Precondition: the repeat is running.");
            int before = city.TestHarness_EquippedRangedFamily;
            city.TestHarness_RequestNextWeapon();
            Assert.AreNotEqual(before, city.TestHarness_EquippedRangedFamily, "The new weapon is equipped.");
            yield return Wait(1.0f);                          // the current swing finishes, then nothing re-arms
            Assert.IsFalse(city.TestHarness_PlayerMeleeActive, "Flag cleared by the switch: the automatic melee stops.");
            layer.InjectUp(hold);
        }

        [UnityTest]
        public IEnumerator ShootHeldThroughSwingFiresAtCompletionWhenMeleeFlagClear()
        {
            int ammo = city.TestHarness_AmmoFor(0);
            var layer = city.TestHarness_PointerLayer;
            int melee = layer.InjectDown(MeleeCentre);
            yield return WaitMeleeActive();
            yield return null;
            layer.InjectUp(melee);
            int shoot = layer.InjectDown(ShootCentre);        // still held when the swing completes
            float until = Time.realtimeSinceStartup + 2.5f;
            while(city.TestHarness_AmmoFor(0) == ammo && Time.realtimeSinceStartup < until) yield return null;
            layer.InjectUp(shoot);
            Assert.Less(city.TestHarness_AmmoFor(0), ammo, "Fire control held at swing completion with the melee flag clear: sub_667B0 starts the shot.");
        }
    }
}
#endif
