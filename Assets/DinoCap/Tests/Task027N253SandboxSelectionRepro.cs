#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public class Task027N253SandboxSelectionRepro
    {
        CombatSandboxV2 sandbox;
        DC1MenuPreferenceScope preferences;
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            preferences=new DC1MenuPreferenceScope();
            NativeAudioSessionGate.StopAll();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CombatSandboxV2.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            sandbox=Object.FindFirstObjectByType<CombatSandboxV2>();
            sandbox.enabled=false;
            sandbox.TestHarness_SetFireHeld(false);
            for(int f=0;f<6;f++) sandbox.TestHarness_SetWeapon(f,-1,0);
        }
        [TearDown]
        public void TearDown(){preferences?.Dispose();preferences=null;}
        [TestCase(2,0,"Attack_D")]
        [TestCase(2,1,"Attack_D")]
        [TestCase(2,2,"Attack_E1")]
        [TestCase(2,3,"Attack_E1")]
        [TestCase(4,0,"Attack_E2")]
        [TestCase(4,1,"Attack_E2")]
        public void NativeCEAudioUsesActualTierBranch(int family,int tier,string expected)
        {
            sandbox.TestHarness_SetWeapon(family,tier,999);
            NativeAudioSessionGate.StopAll();NativeAudioSessionGate.TestHarness_ResetClipCounters();
            typeof(CombatSandboxV2).GetMethod("BeginFireRequest",Private).Invoke(sandbox,null);
            var visual=(Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetField("playerVisual",Private).GetValue(sandbox);
            Assert.AreEqual(0,NativeAudioSessionGate.TestHarness_AdmittedCount(expected));
            for(int i=0;i<3;i++)visual.secondary.Tick(visual.secondary.Clip.frameDuration);
            Assert.AreEqual(1,NativeAudioSessionGate.TestHarness_AdmittedCount(expected),
                "65A08 stores actual family at+144;66DCC Ctier2/3 picksAttack_E1 and67458 always requestsAttack_E2.");
        }

        [TestCase(5,0,4,4)]
        [TestCase(3,0,2,2)]
        [TestCase(2,0,4,4)]
        public void NativeEmptySelectionSearchesHigherThenLowerDescending(int start,int first,int second,int expected)
        {
            sandbox.TestHarness_SetWeapon(first,0,17);
            sandbox.TestHarness_SetWeapon(second,0,23);
            sandbox.TestHarness_SetWeapon(start,0,0);
            bool selected=(bool)typeof(CombatSandboxV2).GetMethod("AutoSwitchToLoaded",Private).Invoke(sandbox,null);
            Assert.IsTrue(selected);
            Assert.AreEqual(expected,sandbox.TestHarness_EquippedFamily,"66BA4 higher ascending,66C18 lower descending; never wrap to family0 before the nearest lower family.");
            Assert.AreEqual(17,sandbox.TestHarness_FamilyAmmo(first));
            Assert.AreEqual(23,sandbox.TestHarness_FamilyAmmo(second));
        }

        [Test]
        public void NoEligibleDestinationDoesNotAlterActiveCurrentTrack()
        {
            sandbox.TestHarness_SetWeapon(5,1,0);
            var visual=(Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetField("playerVisual",Private).GetValue(sandbox);
            visual.secondary.PlayRange(visual.FindClip("p_fire_F2_hold"),3,4);
            typeof(CombatSandboxV2).GetField("f2SessionActive",Private).SetValue(sandbox,true);
            bool selected=(bool)typeof(CombatSandboxV2).GetMethod("AutoSwitchToLoaded",Private).Invoke(sandbox,null);
            Assert.IsFalse(selected);
            Assert.AreEqual(5,sandbox.TestHarness_EquippedFamily);
            Assert.IsTrue(visual.secondary.Active,"No destination means no65A08 cancellation or HUD update.");
            Assert.AreEqual(3,visual.secondary.Frame);
        }

        [Test]
        public void F3UnmarkedFrame4CannotRequestAnotherShot()
        {
            sandbox.TestHarness_SetWeapon(5,2,999);
            var visual=(Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetField("playerVisual",Private).GetValue(sandbox);
            visual.secondary.PlayRange(visual.FindClip("p_fire_F3_hold"),3,4);
            typeof(CombatSandboxV2).GetField("f3SessionActive",Private).SetValue(sandbox,true);
            NativeAudioSessionGate.StopAll();NativeAudioSessionGate.TestHarness_ResetClipCounters();
            typeof(CombatSandboxV2).GetMethod("OnPlayerSecondaryFrame",Private).Invoke(sandbox,new object[]{visual.secondary,4});
            Assert.AreEqual(0,NativeAudioSessionGate.TestHarness_AdmittedCount("Attack_F3"),"76A20 emits only the eventMark3 registered by7D81C.");
        }
        [TestCase(1)]
        [TestCase(2)]
        public void NormalFReleasePlaysBodyTailAndAllowsFreshPress(int mode)
        {
            sandbox.TestHarness_SetWeapon(5,mode,999);
            var visual=(Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetField("playerVisual",Private).GetValue(sandbox);
            visual.secondary.PlayRange(visual.FindClip("p_fire_F"+(mode+1)),4,4);
            visual.secondary.Stop();
            string prefix=mode==1?"f2":"f3";
            typeof(CombatSandboxV2).GetField(prefix+"ReleasePending",Private).SetValue(sandbox,true);
            typeof(CombatSandboxV2).GetMethod("CompletePendingF"+(mode+1)+"Release",Private).Invoke(sandbox,null);
            Assert.AreEqual("p_fire_F"+(mode+1)+"_release",visual.secondary.Clip.name);
            Assert.AreEqual(4,visual.secondary.Frame);
            typeof(CombatSandboxV2).GetMethod("BeginFireRequest",Private).Invoke(sandbox,null);
            Assert.AreEqual("p_fire_F"+(mode+1),visual.secondary.Clip.name,"byte188 is clear while the tail remains active.");
            Assert.AreEqual(0,visual.secondary.Frame);
        }
        [Test]
        public void PlayerHurtAudioOnlyOnAdmittedNonfatalHits()
        {
            sandbox.TestHarness_SetWeapon(0,0,50);
            sandbox.TestHarness_SetGodMode(false);
            typeof(CombatSandboxV2).GetField("playerHP",Private).SetValue(sandbox,100f);
            NativeAudioSessionGate.StopAll();
            NativeAudioSessionGate.TestHarness_ResetClipCounters();
            sandbox.TestHarness_DamagePlayer(1f);
            for(int i=0;i<10;i++)sandbox.TestHarness_DamagePlayer(1f);
            Assert.AreEqual(1,NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Role"),"Shared68148 owner: one nonfatal admission, no repeated blocked-hit sound.");
            sandbox.TestHarness_ForceInvulnExpired();
            typeof(CombatSandboxV2).GetField("playerHitStun",Private).SetValue(sandbox,false);
            typeof(CombatSandboxV2).GetField("playerHP",Private).SetValue(sandbox,1f);
            sandbox.TestHarness_DamagePlayer(5f);
            Assert.AreEqual(1,NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Role"),"Lethal path skips Audio_Role.");
        }

        [Test]
        public void F3ConeExitHasNoInventedMinimumReactionFloor()
        {
            sandbox.TestHarness_ClearDinos();sandbox.TestHarness_SpawnDino(0,true);sandbox.TestHarness_SelectDino(0);
            typeof(CombatSandboxV2).GetMethod("BeginF3",Private).Invoke(sandbox,null);
            typeof(CombatSandboxV2).GetMethod("EndF3",Private).Invoke(sandbox,null);
            var enemyVisual=(Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetProperty("enemyVisual",Private).GetValue(sandbox);
            Assert.IsFalse(sandbox.TestHarness_DinoF3State(0));
            Assert.AreEqual(4,enemyVisual.primary.Frame,"12C64 state4 immediately requests4..end/.025, even immediately after entry.");
            Assert.IsFalse((bool)typeof(CombatSandboxV2).GetProperty("enemyF3TailDeferred",Private).GetValue(sandbox));
        }
        [TestCase(1)]
        [TestCase(2)]
        public void TargetStateEndsThroughOwnedEffectCompletion(int mode)
        {
            sandbox.TestHarness_ClearDinos();sandbox.TestHarness_SpawnDino(0,true);sandbox.TestHarness_SelectDino(0);
            sandbox.TestHarness_SetDinoX(0,100);sandbox.TestHarness_SetWeapon(5,mode,999);
            typeof(CombatSandboxV2).GetMethod("DischargeF"+(mode+1),Private).Invoke(sandbox,new object[]{WeaponFamilyTable.Rows[5][mode]});
            bool active=mode==1?sandbox.TestHarness_DinoBurningF2(0):sandbox.TestHarness_DinoF3State(0);
            Assert.IsTrue(active,"Actual F cone must arm the target before testing completion.");
            var visual=(Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetField("playerVisual",Private).GetValue(sandbox);
            visual.secondary.PlayRange(visual.FindClip("p_fire_F"+(mode+1)),4,4);visual.secondary.Stop();
            typeof(CombatSandboxV2).GetField((mode==1?"f2":"f3")+"ReleasePending",Private).SetValue(sandbox,true);
            typeof(CombatSandboxV2).GetMethod("CompletePendingF"+(mode+1)+"Release",Private).Invoke(sandbox,null);
            active=mode==1?sandbox.TestHarness_DinoBurningF2(0):sandbox.TestHarness_DinoF3State(0);
            Assert.IsTrue(active,"663A0 starts effect wind-down;12F28 completion owns target exit.");
            var effect=(MonoBehaviour)typeof(CombatSandboxV2).GetField(mode==1?"f2FlameVisual":"f3LaserVisual",Private).GetValue(sandbox);
            effect.gameObject.SetActive(false);
            typeof(CombatSandboxV2).GetMethod("TickEnemy",Private).Invoke(sandbox,null);
            active=mode==1?sandbox.TestHarness_DinoBurningF2(0):sandbox.TestHarness_DinoF3State(0);
            Assert.IsFalse(active,"All living affected targets must leave state when their owned effect deactivates.");
        }
    }
}
#endif
