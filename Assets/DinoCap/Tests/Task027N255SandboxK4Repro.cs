#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N255SandboxK4Repro
    {
        CombatSandboxV2 sandbox;DC1MenuPreferenceScope preferences;
        [UnitySetUp] public IEnumerator SetUp()
        {
            preferences=new DC1MenuPreferenceScope();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/DinoCap/Scenes/CombatSandboxV2.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            yield return null;sandbox=Object.FindFirstObjectByType<CombatSandboxV2>();
            sandbox.enabled=false;sandbox.TestHarness_ClearDinos();sandbox.TestHarness_SetGodMode(false);
        }
        [TearDown] public void TearDown()=>preferences.Dispose();
        Power2DSourceTruthRenderer SelectedVisual()=>
            (Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetProperty("enemyVisual",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(sandbox);
        [TestCase(DinoFatalDeathCore.Species.K3Blue,116,120)]
        [TestCase(DinoFatalDeathCore.Species.K1Rusher,116,133)]
        [TestCase(DinoFatalDeathCore.Species.K2Trex,170,164)]
        public void SpecialDeathHeadSizesMatchNativeFloatLiterals(DinoFatalDeathCore.Species species,float width,float height)
        {
            var p=DinoFatalDeathCore.GetGSpecialPresentation(species);
            Assert.That(p.flyingHeadWidth,Is.EqualTo(width));Assert.That(p.flyingHeadHeight,Is.EqualTo(height));
        }
        [TestCase("BeginF2Burn","k4_shao")] [TestCase("BeginF3","k4_be")]
        public void K4UsesItsOwnNativeBurnAndElectrocutionBindings(string method,string clip)
        {
            sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);sandbox.TestHarness_SetDebugEnemyHP(1000);
            typeof(CombatSandboxV2).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(sandbox,null);
            Assert.That(SelectedVisual().primary.Clip.name,Is.EqualTo(clip));
            Assert.That(sandbox.TestHarness_DinoBurningF2(0),Is.EqualTo(method=="BeginF2Burn"));
            Assert.That(sandbox.TestHarness_DinoF3State(0),Is.EqualTo(method=="BeginF3"));
        }
        [Test] public void K4StandardDeathDoesNotInventAnUnmarkedCorpseLandingSound()
        {
            sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);
            typeof(CombatSandboxV2).GetProperty("enemyHP",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(sandbox,0f);
            NativeAudioSessionGate.FxMuted=false;NativeAudioSessionGate.StopAll();NativeAudioSessionGate.TestHarness_ResetClipCounters();
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            typeof(CombatSandboxV2).GetMethod("BeginStandardEnemyDeath",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(sandbox,null);
            var visual=SelectedVisual();Assert.That(visual.primary.Clip.name,Is.EqualTo("k4_die"));
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood),Is.EqualTo(3));
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.BlankChunkS1),Is.EqualTo(1));
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.BlankChunkS2),Is.EqualTo(1));
            while(visual.primary.Active && visual.primary.Frame<17)visual.primary.Tick(.025f);
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_death1"),Is.EqualTo(1));
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_death3"),Is.Zero);
        }
        [Test] public void StandardDeathKeepsK2SpecificDebrisAndDistinctNativePhysics()
        {
            sandbox.TestHarness_SpawnDino(2,true);sandbox.TestHarness_SelectDino(0);
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            typeof(CombatSandboxV2).GetMethod("BeginStandardEnemyDeath",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(sandbox,null);
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.ChunkK2S1),Is.EqualTo(1));
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.ChunkK2S2),Is.EqualTo(1));
            const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
            foreach(var p in Object.FindObjectsByType<DinoCapDamageParticle>(FindObjectsSortMode.None))
            {
                var kind=(DinoCapDamageParticle.Kind)typeof(DinoCapDamageParticle).GetField("kind",flags).GetValue(p);
                if(kind==DinoCapDamageParticle.Kind.Blood)continue;
                var velocity=(Vector2)typeof(DinoCapDamageParticle).GetField("velocity",flags).GetValue(p);
                Assert.That(velocity.x,Is.InRange(-200f,200f));Assert.That(velocity.y,Is.InRange(500f,700f));
                Assert.That((float)typeof(DinoCapDamageParticle).GetField("gravity",flags).GetValue(p),Is.EqualTo(-2000f));
            }
        }
        [Test] public void K4SpecialFatalKeepsDie2AudioAndBlankHeadLandingOwner()
        {
            sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);
            NativeAudioSessionGate.FxMuted=false;NativeAudioSessionGate.StopAll();NativeAudioSessionGate.TestHarness_ResetClipCounters();
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            typeof(CombatSandboxV2).GetMethod("BeginGSpecialFatalDeath",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(sandbox,null);
            Assert.That(SelectedVisual().primary.Clip.name,Is.EqualTo("k4njz_die2"));
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_death2"),Is.EqualTo(1));
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_death1"),Is.Zero);
            var head=Object.FindFirstObjectByType<FlyingHeadParticle>();
            Assert.That(head,Is.Not.Null);Assert.That(head.GetComponent<SpriteRenderer>().enabled,Is.False);
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            bool facing=(bool)typeof(CombatSandboxV2).GetProperty("enemyFacingRight",flags).GetValue(sandbox);
            Assert.That((Vector2)typeof(FlyingHeadParticle).GetField("velocity",flags).GetValue(head),Is.EqualTo(new Vector2(facing?100:-100,300)));
            typeof(FlyingHeadParticle).GetMethod("Land",flags).Invoke(head,null);
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.ChunkS3),Is.EqualTo(1));
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.ChunkS4),Is.EqualTo(1));
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.BlankChunkS1),Is.EqualTo(1));
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.BlankChunkS2),Is.EqualTo(1));
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.ChunkK3S1),Is.Zero);
            foreach(var particle in Object.FindObjectsByType<DinoCapDamageParticle>(FindObjectsSortMode.None))
            {
                var kind=(DinoCapDamageParticle.Kind)typeof(DinoCapDamageParticle).GetField("kind",flags).GetValue(particle);
                if(kind!=DinoCapDamageParticle.Kind.BlankChunkS1 && kind!=DinoCapDamageParticle.Kind.BlankChunkS2)continue;
                Assert.That(particle.GetComponent<SpriteRenderer>().enabled,Is.False);
                typeof(DinoCapDamageParticle).GetMethod("Settle",flags).Invoke(particle,null);
                Assert.That(particle.GetComponent<SpriteRenderer>().enabled,Is.True,"Unconfigured airborne slots still acquire native floor decals.");
            }
        }
        [TestCase("BeginF2BurnDeath")] [TestCase("BeginF3Death")]
        public void K4ThermalAndLaserFatalUseSharedAshWithoutInventedDeathAudio(string method)
        {
            sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);
            NativeAudioSessionGate.FxMuted=false;NativeAudioSessionGate.StopAll();NativeAudioSessionGate.TestHarness_ResetClipCounters();
            typeof(CombatSandboxV2).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(sandbox,null);
            Assert.That(SelectedVisual().primary.Clip.name,Is.EqualTo("ShaoSiAnim"));
            Assert.That(SelectedVisual().actorScale,Is.EqualTo(1f),"118E8 species4 skips the small-species scale write.");
            Assert.That(SelectedVisual().drawOffset,Is.EqualTo(Vector2.zero));
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_death1"),Is.Zero);
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_death2"),Is.Zero);
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_death3"),Is.Zero);
        }
        [TestCase(6,true,3,1)] [TestCase(4,true,3,0)] [TestCase(5,false,0,1)]
        public void K4HeavyNonfatalPreservesWeaponSpecificBloodAndReaction(int family,bool blood,int ballistic,int burst)
        {
            sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);sandbox.TestHarness_SetDebugEnemyHP(100000);
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();DinoCapBloodBurst.ResetDebugCounts();
            var method=typeof(CombatSandboxV2).GetMethod("ApplyHeavyWeaponHit",BindingFlags.Instance|BindingFlags.NonPublic);
            var mode=System.Enum.Parse(method.GetParameters()[1].ParameterType,family==4?"EExplosion":family==5?"GSpecial":"Standard");
            var weapon=family==6?WeaponFamilyTable.MeleeRows[0]:WeaponFamilyTable.Rows[family][0];
            method.Invoke(sandbox,new object[]{weapon,mode,blood});
            Assert.That(sandbox.TestHarness_DinoHP(0),Is.LessThan(100000));
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood),Is.EqualTo(ballistic));
            Assert.That(DinoCapBloodBurst.DxSpawnCount,Is.EqualTo(burst));
            Assert.That(DinoCapBloodBurst.QxSpawnCount,Is.Zero);
            Assert.That(family==5?SelectedVisual().secondary.Clip.name:SelectedVisual().primary.Clip.name,Is.EqualTo(family==5?"k4_befired":"k4_behited"));
        }
        [UnityTest] public IEnumerator ActualK4Frame10CreatesSharedProjectileWithoutFrame12Bite()
        {
            sandbox.TestHarness_SpawnDino(3,true);sandbox.TestHarness_SelectDino(0);
            sandbox.TestHarness_SetEnemyX(sandbox.TestHarness_PlayerX+30);
            sandbox.TestHarness_ForceEnemyAttack();
            var visual=(Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetField("enemyVisual",BindingFlags.Instance|BindingFlags.NonPublic)?.GetValue(sandbox);
            // enemyVisual is a per-instance property shim.
            if(visual==null)visual=(Power2DSourceTruthRenderer)typeof(CombatSandboxV2).GetProperty("enemyVisual",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(sandbox);
            float hp=sandbox.TestHarness_PlayerHP;
            while(visual.primary.Frame<12)yield return null;
            Assert.That(sandbox.TestHarness_DebugK4ProjectileCount,Is.EqualTo(1));
            Assert.That(sandbox.TestHarness_PlayerHP,Is.EqualTo(hp),"K4 cannot also inflict the ordinary frame12 bite.");
            var projectile=Object.FindFirstObjectByType<JungleProjectileActor>();
            Assert.That(projectile,Is.Not.Null);Assert.That(projectile.State.Flying,Is.True);
            Assert.That(projectile.State.Position.y,Is.EqualTo(-9).Within(.01));
        }
        [Test] public void SharedProjectileNonfatalHitDoesNotClaimBiteHurtOrFacing()
        {
            sandbox.TestHarness_SetPlayerHP(1000);sandbox.TestHarness_ForceInvulnExpired();
            bool facing=sandbox.TestHarness_PlayerFacingRight;
            Assert.That(sandbox.TestHarness_DebugK4Hit(),Is.True);
            Assert.That(sandbox.TestHarness_PlayerHP,Is.EqualTo(1000-JungleBlitzProgression.Load().Query(0,4).attack));
            Assert.That((bool)typeof(CombatSandboxV2).GetField("playerHitStun",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(sandbox),Is.False);
            Assert.That(sandbox.TestHarness_PlayerFacingRight,Is.EqualTo(facing));
        }
        [Test] public void SandboxF1PreservesExistingPrimaryReactionVelocity()
        {
            sandbox.TestHarness_SpawnDino(0,true);sandbox.TestHarness_SelectDino(0);
            var type=typeof(CombatSandboxV2);const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
            type.GetProperty("enemyHP",flags).SetValue(sandbox,1000f);
            type.GetProperty("enemyHeavyReaction",flags).SetValue(sandbox,true);
            type.GetProperty("enemyVelocityX",flags).SetValue(sandbox,123f);
            var visual=(Power2DSourceTruthRenderer)type.GetProperty("enemyVisual",flags).GetValue(sandbox);
            visual.primary.Play(visual.FindClip("k3_behited"),0);visual.secondary.Stop();
            var go=new GameObject("SandboxF1 existing reaction");
            try
            {
                go.transform.position=new Vector3(sandbox.TestHarness_DinoX(0),0,0);
                sandbox.TryF1ProjectileContact(go.AddComponent<FProjectileParticle>());
                Assert.That(sandbox.TestHarness_DinoHP(0),Is.LessThan(1000));
                Assert.That((float)type.GetProperty("enemyVelocityX",flags).GetValue(sandbox),Is.EqualTo(123));
                Assert.That(visual.primary.Clip.name,Is.EqualTo("k3_behited"));Assert.That(visual.secondary.Clip,Is.Null);
            }
            finally{Object.DestroyImmediate(go);}
        }
    }
}
#endif
