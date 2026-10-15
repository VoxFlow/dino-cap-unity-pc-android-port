#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N255FirstOwnersRepro
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CityGridController core;
        DC1MenuPreferenceScope preferences;
        [UnitySetUp] public IEnumerator SetUp()
        {
            preferences=new DC1MenuPreferenceScope();
            DC1Preferences.SetInt("DC1_Debug_CityGrind_GodMode",0);
            NativeAudioSessionGate.FxMuted=false;
            DC1Preferences.SetInt(NativeAudioSessionGate.FxMutedPrefKey,0);
            NativeAudioSessionGate.StopAll();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            core=Object.FindFirstObjectByType<CityGridController>();
            Assert.That(core,Is.Not.Null);
            core.TestHarness_SkipLevelIntro();core.TestHarness_ClearDinos();core.enabled=false;
        }
        [TearDown] public void TearDown(){preferences?.Dispose();NativeAudioSessionGate.StopAll();}
        [TestCase(4f/3f,1f)] [TestCase(16f/9f,1f)] [TestCase(32f/9f,1f)]
        [TestCase(4f/3f,8f)] [TestCase(16f/9f,8f)] [TestCase(32f/9f,8f)]
        public void FacingReversalUsesBoundedPanIncludingAcceleratedDebugWalk(float aspect,float speed)
        {
            core.TestHarness_SetCameraAspect(aspect);core.TestHarness_SetDebugMovementSpeed(speed);
            try
            {
                var player=(Transform)typeof(CityGridController).GetField("player",Private).GetValue(core);
                var visual=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",Private).GetValue(core);
                player.position=new Vector3(3500,player.position.y,0);
                visual.facingRight=true;
                typeof(CityGridController).GetMethod("InitializeCamera",Private).Invoke(core,null);
                typeof(CityGridController).GetMethod("UpdateCamera",Private).Invoke(core,null);
                var camera=typeof(CityGridController).GetField("cameraLeftX",Private);
                float before=(float)camera.GetValue(core);
                visual.facingRight=false;
                typeof(CityGridController).GetMethod("UpdateCamera",Private).Invoke(core,null);
                float after=(float)camera.GetValue(core);
                float closingSpeed=(315f-205f)*(core.TestHarness_CurrentViewWidth/1024f);
                float pan=205f*speed+closingSpeed;
                Assert.That(before-after,Is.EqualTo(pan*Time.deltaTime).Within(.01),"Reversal must pan rather than jump straight to its new target.");
                if(speed>1)Assert.That(pan,Is.GreaterThan(DinoCapSourceProfileV11.PlayerSpeed*speed),"Debug pan must catch accelerated walking.");
            }
            finally{core.TestHarness_ResetCameraAspect();}
        }
        // ORIGINAL BUG KEPT (user decision 2026-10-14, mark for writeup): native sub_65E2C steps the camera by pan*dt before clamping, so turning
        // away from a pinned world bound moves the camera one step inside the bound for one frame and the next frame snaps it back. The step is
        // pan*dt with the widescreen adapter's width-proportional closing margin (equal to native 315 u/s at 1024 wide).
        [TestCase(4f/3f)] [TestCase(16f/9f)] [TestCase(20f/9f)]
        public void TurningAwayFromThePinnedLeftBoundBlipsByTheNativePanStepThenSnapsBack(float aspect)
        {
            core.TestHarness_SetCameraAspect(aspect);core.TestHarness_SetDebugMovementSpeed(1f);
            try
            {
                var player=(Transform)typeof(CityGridController).GetField("player",Private).GetValue(core);
                var visual=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",Private).GetValue(core);
                player.position=new Vector3(-500,player.position.y,0);
                visual.facingRight=false;
                typeof(CityGridController).GetMethod("InitializeCamera",Private).Invoke(core,null);
                typeof(CityGridController).GetMethod("UpdateCamera",Private).Invoke(core,null);
                var camera=typeof(CityGridController).GetField("cameraLeftX",Private);
                float pinned=(float)camera.GetValue(core);
                Assert.AreEqual(CityGridController.WorldLeft,pinned,.01f,"Precondition: camera pinned at the left bound.");
                visual.facingRight=true;
                typeof(CityGridController).GetMethod("UpdateCamera",Private).Invoke(core,null);
                float blip=(float)camera.GetValue(core);
                float expectedPan=205f+(315f-205f)*(core.TestHarness_CurrentViewWidth/1024f);
                Assert.That(blip-pinned,Is.EqualTo(expectedPan*Time.deltaTime).Within(.01),"One pan step inside the bound (native 315 u/s at 1024 wide, width-scaled closing margin on wider views).");
                typeof(CityGridController).GetMethod("UpdateCamera",Private).Invoke(core,null);
                Assert.AreEqual(pinned,(float)camera.GetValue(core),.01f,"The next frame snaps back to the clamped target (native quirk).");
            }
            finally{core.TestHarness_ResetCameraAspect();}
        }
        [Test] public void F3AudioWaitsForMarkedFrameAndKeepsFreshCycleVoices()
        {
            core.TestHarness_SetRangedFamilyTierRaw(5,2);core.TestHarness_SetRangedFamily(5);core.TestHarness_SetFMode(2);
            core.TestHarness_GrantAmmo(5,20-core.TestHarness_AmmoFor(5));
            var phase=typeof(CityGridController).GetField("f3ClipPhase",Private);
            phase.SetValue(core,System.Enum.Parse(phase.FieldType,"Hold"));
            var visual=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",Private).GetValue(core);
            NativeAudioSessionGate.StopAll();NativeAudioSessionGate.TestHarness_ResetClipCounters();
            visual.secondary.PlayRange(visual.FindClip("p_fire_F3_hold"),3,4);
            string sound=core.TestHarness_LastFireClip.name;
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount(sound),Is.EqualTo(1));
            Assert.That(core.TestHarness_AmmoFor(5),Is.EqualTo(19));
            visual.secondary.Tick(.2f);
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount(sound),Is.EqualTo(1),"Unmarked frame4 cannot request sound.");
            Assert.That(core.TestHarness_AmmoFor(5),Is.EqualTo(19),"Frame4 has no discharge or ammo decrement.");
            visual.secondary.PlayRange(visual.FindClip("p_fire_F3_hold"),3,4);
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount(sound),Is.EqualTo(2));
            Assert.That(NativeAudioSessionGate.CountedSessions,Is.EqualTo(2),"Every actual marked request admits a fresh voice.");
            Assert.That(core.TestHarness_AmmoFor(5),Is.EqualTo(18));
        }
        [TestCase(10,0)] [TestCase(0,1)]
        public void SwitchRequestsNativeButtonAndIndependentEmptySound(int stock,int noBullet)
        {
            for(int i=0;i<6;i++)core.TestHarness_SetRangedFamilyTierRaw(i,i<2?0:-1);
            core.TestHarness_SetRangedFamily(0);
            core.TestHarness_GrantAmmo(1,stock-core.TestHarness_AmmoFor(1));
            NativeAudioSessionGate.TestHarness_ResetClipCounters();
            NativeAudioSessionGate.StopAll();
            core.TestHarness_SwitchWeapon();
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Button1"),Is.EqualTo(1));
            Assert.That(NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Nobullet"),Is.EqualTo(noBullet));
            Assert.That(core.TestHarness_AmmoFor(1),Is.EqualTo(stock),"Switch cannot fire a shot.");
        }
        [Test] public void F1LastVictimMarkerAllowsInterveningProjectileAndClearsOnRetirement()
        {
            var a=new GameObject("F1 marker A").AddComponent<FProjectileParticle>();
            var b=new GameObject("F1 marker B").AddComponent<FProjectileParticle>();
            var victim=new GameObject("F1 marker victim").transform;
            try
            {
                a.MarkHit(victim);Assert.That(a.HasHit(victim),Is.True);
                b.MarkHit(victim);Assert.That(b.HasHit(victim),Is.True);
                Assert.That(a.HasHit(victim),Is.False,"Native13250 admits A again after B replaces victim+180.");
                a.MarkHit(victim);
                typeof(FProjectileParticle).GetMethod("Recycle",Private).Invoke(b,null);
                Assert.That(a.HasHit(victim),Is.False,"Native13420 clears all scene victims, including another projectile's marker.");
            }
            finally {Object.DestroyImmediate(a.gameObject);Object.DestroyImmediate(b.gameObject);Object.DestroyImmediate(victim.gameObject);}
        }
        [TestCase(1,"k3_jump")] [TestCase(2,"k1_jump")]
        public void ActualJumpSpawnBindsNativeAssetAndCompletes(int type,string clip)
        {
            // Exercise the shared City/Jungle spawn owner with native jump
            // placement; do not derive the expected name from its walk prefix.
            var stats=JungleBlitzProgression.Load().QuerySpawn(0,type,1);
            var placement=new JungleSpawnPlacement.Placement {admitted=true,jump=true,x=-900,y=136,velocityX=1024,facingRight=true};
            typeof(CityGridController).GetMethod("SpawnCityDino",Private).Invoke(core,new object[]{type-1,0,1,stats,placement});
            var list=(IList)typeof(CityGridController).GetField("dinos",Private).GetValue(core);
            var visual=(Power2DSourceTruthRenderer)list[0].GetType().GetField("visual").GetValue(list[0]);
            Assert.That(visual.primary.Clip,Is.Not.Null);
            Assert.That(visual.primary.Clip.name,Is.EqualTo(clip));
            Assert.That(visual.primary.Clip.frames.Length,Is.EqualTo(21));
            for(int i=0;i<9;i++)visual.primary.Tick(.025f);
            Assert.That(visual.primary.Frame,Is.EqualTo(9));
            Assert.That((float)list[0].GetType().GetField("velocityX").GetValue(list[0]),Is.Zero,
                "10294's landing mark stops jump velocity before the clip ends.");
            Assert.That((bool)list[0].GetType().GetProperty("JumpUntargetable").GetValue(list[0]),Is.False);
            Assert.That(visual.primary.Active,Is.True);
            for(int i=9;i<21;i++)visual.primary.Tick(.025f);
            Assert.That(visual.primary.Active,Is.False,"Both shipped jump clips complete their authored frames.");
        }
        [Test] public void RealPlayerLethalPathEmitsSixBallisticParticlesWithoutMeleePulse()
        {
            foreach(var p in Object.FindObjectsByType<DinoCapDamageParticle>(FindObjectsSortMode.None))Object.DestroyImmediate(p.gameObject);
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();DinoCapBloodBurst.ResetDebugCounts();
            core.TestHarness_DamagePlayer(1000);
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood),Is.EqualTo(6));
            Assert.That(DinoCapBloodBurst.DxSpawnCount,Is.Zero);
            var particles=Object.FindObjectsByType<DinoCapDamageParticle>(FindObjectsSortMode.None);
            Assert.That(particles.Length,Is.EqualTo(6));
            foreach(var p in particles)
            {
                Assert.That((float)typeof(DinoCapDamageParticle).GetField("gravity",Private).GetValue(p),Is.EqualTo(-2000));
                var sr=p.GetComponent<SpriteRenderer>();
                Assert.That(sr.sortingOrder,Is.EqualTo(260),"Native layer11 is above both actor groups.");
                Assert.That(sr.sprite.name,Does.Contain("xue"));
                Assert.That(sr.bounds.size.x,Is.InRange(28.5f,57f));
                Assert.That(sr.bounds.size.y,Is.InRange(13f,26f));
            }
        }
        [Test] public void DebugCashControlsCannotWriteCampaignOrModeCarryover()
        {
            DC1Preferences.SetInt("DC1_CityGrind_Cash",123);
            var before=NativeModeSession.Capture();
            core.TestHarness_DebugCashSet(700);core.TestHarness_DebugCashAdd(50);
            Assert.That(core.TestHarness_CashTotal,Is.EqualTo(750));
            core.TestHarness_SaveProgression();
            Assert.That(DC1Preferences.GetInt("DC1_CityGrind_Cash"),Is.EqualTo(123));
            Assert.That(NativeModeSession.Capture().cash,Is.EqualTo(before.cash));
            core.TestHarness_DebugCashSet(0);
            Assert.That(core.TestHarness_CashTotal,Is.Zero);
        }
        [Test] public void RapidWeaponSwitchPreservesMovementFrameAndCadence()
        {
            var visual=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",Private).GetValue(core);
            for(int i=0;i<6;i++)core.TestHarness_SetRangedFamilyTierRaw(i,0);
            core.TestHarness_SetRangedFamily(0);
            var moving=visual.FindClip("p_move_A1");
            Assert.That(moving,Is.Not.Null);
            // Reach frame 2 naturally from frame 0: native clip starts at frame>0 seed the accumulator (sub_76980), so Play(clip,2)
            // would advance on the first tick and this test would no longer measure "selection keeps accumulated time".
            visual.primary.Play(moving,0);
            visual.primary.Tick(moving.frameDuration);
            visual.primary.Tick(moving.frameDuration);
            visual.primary.Tick(moving.frameDuration*.75f);
            for(int i=0;i<12;i++)core.TestHarness_SwitchWeapon();
            Assert.That(visual.primary.Clip.name,Does.StartWith("p_move_"));
            Assert.That(visual.primary.Frame,Is.EqualTo(2));
            visual.primary.Tick(moving.frameDuration*.3f);
            Assert.That(visual.primary.Frame,Is.EqualTo(3),"Selection must not reset the accumulated animation time.");
        }
        [Test] public void DebugAmmoDepletionIsRuntimeOnly()
        {
            var before=NativeModeSession.Capture();
            core.TestHarness_DebugDepleteAllAmmo();
            for(int i=0;i<6;i++)Assert.That(core.TestHarness_AmmoFor(i),Is.Zero);
            core.TestHarness_SaveProgression();
            Assert.That(NativeModeSession.Capture().cash,Is.EqualTo(before.cash));
        }
        [Test] public void EngulfingBurnStaysBelowPlayerWhileDinoBodyStaysInFront()
        {
            core.TestHarness_ForceSpawn(0);
            var list=(IList)typeof(CityGridController).GetField("dinos",Private).GetValue(core);
            var d=list[0];
            typeof(CityGridController).GetMethod("BeginF2Burn",Private).Invoke(core,new[]{d});
            var burn=(F2EnemyBigFireVisual)d.GetType().GetField("f2BigFireVisual").GetValue(d);
            var body=(Power2DSourceTruthRenderer)d.GetType().GetField("visual").GetValue(d);
            var player=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",Private).GetValue(core);
            Assert.That(burn.GetComponentInChildren<UnityEngine.Rendering.SortingGroup>().sortingOrder,Is.LessThan(player.baseSortingOrder));
            Assert.That(body.baseSortingOrder,Is.GreaterThan(player.baseSortingOrder));
        }
        [UnityTest] public IEnumerator LaunchImageIsShownAtApplicationBootButNotAfterGameplayReturn()
        {
            typeof(MainMenuController).GetMethod("BeginApplicationBoot",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            yield return SceneManager.LoadSceneAsync("MainMenuScene");yield return null;
            Assert.That(Object.FindFirstObjectByType<MainMenuController>().TestHarness_BootSplashActive,Is.True);
            yield return SceneManager.LoadSceneAsync("CityGrindScene");yield return null;
            yield return SceneManager.LoadSceneAsync("MainMenuScene");yield return null;
            Assert.That(Object.FindFirstObjectByType<MainMenuController>().TestHarness_BootSplashActive,Is.False);
        }
        [TestCase(2560,1440)] [TestCase(3840,2160)] [TestCase(3440,1440)]
        [TestCase(5120,1440)] [TestCase(5760,1080)]
        public void CityLayerAllocationCoversExpandedViewportThroughoutRecycling(int width,int height)
        {
            core.TestHarness_SetCameraAspect((float)width/height);
            try
            {
                typeof(CityGridController).GetMethod("RecycleBackground",Private).Invoke(core,null);
                Assert.That(core.TestHarness_CameraFullyCovered(),Is.True);
                for(int i=0;i<15;i++)
                {
                    typeof(CityGridController).GetField("cameraLeftX",Private).SetValue(core,2000f-i*100);
                    typeof(CityGridController).GetMethod("RecycleBackground",Private).Invoke(core,null);
                    Assert.That(core.TestHarness_CameraFullyCovered(),Is.True,"Camera reversal step "+i);
                }
            }
            finally{core.TestHarness_ResetCameraAspect();}
        }
        [Test] public void DebugDeathUsesRealEffectsEvenWithGodEnabled()
        {
            DC1Preferences.SetInt("DC1_Debug_CityGrind_GodMode",1);core.TestHarness_ReloadDebugSettings();
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            core.TestHarness_DebugInstantDeath();
            Assert.That(core.TestHarness_PlayerHP,Is.Zero);
            Assert.That(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood),Is.EqualTo(6));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void F1ContactCannotEnterOrdinaryHeavyKnockback(int type)
        {
            core.TestHarness_JungleRawDraw(()=>0);core.TestHarness_ForceSpawn(type);
            core.TestHarness_SetDinoHP(0,1000);
            var go=new GameObject("F1 contact fixture");
            try
            {
                go.transform.position=new Vector3(core.TestHarness_DinoX(0),136,0);
                var projectile=go.AddComponent<FProjectileParticle>();
                core.TryF1ProjectileContact(projectile);
                Assert.That(core.TestHarness_DinoHP(0),Is.LessThan(1000));
                Assert.That(core.TestHarness_DinoHeavyReaction(0),Is.False);
            }
            finally{Object.DestroyImmediate(go);}
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void OriginalFShopIconsResolveForEveryNativeTier(int tier)
            =>Assert.That(core.TestHarness_ShopWeaponIconLoads(5,tier),Is.True);
        [Test] public void F1CanContactAirborneActorAndInterveningProjectileReopensContact()
        {
            core.TestHarness_JungleRawDraw(()=>int.MaxValue);core.TestHarness_ForceSpawn(0);
            core.TestHarness_SetDinoHP(0,1000);
            var a=new GameObject("F1 airborne A").AddComponent<FProjectileParticle>();
            var b=new GameObject("F1 airborne B").AddComponent<FProjectileParticle>();
            try
            {
                a.transform.position=b.transform.position=new Vector3(core.TestHarness_DinoX(0),136,0);
                core.TryF1ProjectileContact(a);float first=core.TestHarness_DinoHP(0);
                Assert.That(first,Is.LessThan(1000),"13250 has no airborne targetable-byte gate.");
                core.TryF1ProjectileContact(a);Assert.That(core.TestHarness_DinoHP(0),Is.EqualTo(first));
                core.TryF1ProjectileContact(b);float second=core.TestHarness_DinoHP(0);
                Assert.That(second,Is.LessThan(first));
                core.TryF1ProjectileContact(a);Assert.That(core.TestHarness_DinoHP(0),Is.LessThan(second));
            }
            finally{Object.DestroyImmediate(a.gameObject);Object.DestroyImmediate(b.gameObject);}
        }
        [TestCase(1,"k3_jump")] [TestCase(2,"k1_jump")]
        public void CityUsesNativeJumpPlacementAnd1024RootVelocity(int type,string clip)
        {
            core.TestHarness_JungleRawDraw(()=>int.MaxValue);
            core.TestHarness_ForceSpawn(type-1);
            Assert.That(core.TestHarness_DinoEntering(0),Is.True);
            var list=(IList)typeof(CityGridController).GetField("dinos",Private).GetValue(core);
            Assert.That((float)list[0].GetType().GetField("velocityX").GetValue(list[0]),Is.EqualTo(1024).Or.EqualTo(-1024));
            var visual=(Power2DSourceTruthRenderer)list[0].GetType().GetField("visual").GetValue(list[0]);
            Assert.That(visual.primary.Clip.name,Is.EqualTo(clip));
        }
        [TestCase(1)] [TestCase(2)]
        public void SustainedFWeaponsRejectAirborneTargetBeforeReplacingJump(int tier)
        {
            core.TestHarness_JungleRawDraw(()=>int.MaxValue);core.TestHarness_ForceSpawn(0);
            var list=(IList)typeof(CityGridController).GetField("dinos",Private).GetValue(core);
            var d=list[0];var visual=(Power2DSourceTruthRenderer)d.GetType().GetField("visual").GetValue(d);
            var root=(Transform)d.GetType().GetField("transform").GetValue(d);
            visual.TryGetPartWorldAnchor("tou",out Vector3 head);
            var pv=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",Private).GetValue(core);
            pv.facingRight=true;root.position+=Vector3.right*(core.TestHarness_PlayerX+100-head.x);
            typeof(CityGridController).GetMethod(tier==1?"DischargeF2":"DischargeF3",Private).Invoke(core,null);
            Assert.That(visual.primary.Clip.name,Is.EqualTo("k3_jump"));
            Assert.That(core.TestHarness_DinoBurningF2(0),Is.False);
            Assert.That(core.TestHarness_DinoF3State(0),Is.False);
            d.GetType().GetField("entering").SetValue(d,false);
            typeof(CityGridController).GetMethod(tier==1?"DischargeF2":"DischargeF3",Private).Invoke(core,null);
            Assert.That(tier==1?core.TestHarness_DinoBurningF2(0):core.TestHarness_DinoF3State(0),Is.True,
                "The same in-range actor becomes eligible after native landing.");
        }
        [Test] public void OrdinaryGunSelectsNearestForwardHeadRegardlessOfPainterOrder()
        {
            core.TestHarness_JungleRawDraw(()=>0);core.TestHarness_ForceSpawn(0);core.TestHarness_ForceSpawn(1);
            var list=(IList)typeof(CityGridController).GetField("dinos",Private).GetValue(core);
            var pv=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",Private).GetValue(core);
            pv.facingRight=true;
            for(int i=0;i<2;i++)
            {
                var d=list[i];var visual=(Power2DSourceTruthRenderer)d.GetType().GetField("visual").GetValue(d);
                d.GetType().GetField("entering").SetValue(d,false);
                visual.TryGetPartWorldAnchor("tou",out Vector3 head);
                var root=(Transform)d.GetType().GetField("transform").GetValue(d);
                root.position+=Vector3.right*(core.TestHarness_PlayerX+100+50*i-head.x);
                visual.baseSortingOrder=200+500*i;
            }
            var select=typeof(CityGridController).GetMethod("FindNativeRangedTarget",Private);
            Assert.That(select.Invoke(core,new object[]{200f}),Is.SameAs(list[0]),"Higher painter order cannot override nearer head.");
            list[0].GetType().GetField("entering").SetValue(list[0],true);
            Assert.That(select.Invoke(core,new object[]{200f}),Is.SameAs(list[1]),"Airborne nearest actor remains untargetable.");
            Assert.That(select.Invoke(core,new object[]{149f}),Is.Null,"Head range is strict and cannot extend to a farther target.");
            pv.facingRight=false;
            Assert.That(select.Invoke(core,new object[]{200f}),Is.Null,"Both heads are behind the facing direction.");
        }
        [Test] public void F1CannotOverwriteAnExistingPrimaryHeavyReaction()
        {
            core.TestHarness_JungleRawDraw(()=>0);core.TestHarness_ForceSpawn(0);core.TestHarness_SetDinoHP(0,1000);
            var list=(IList)typeof(CityGridController).GetField("dinos",Private).GetValue(core);var d=list[0];var type=d.GetType();
            var visual=(Power2DSourceTruthRenderer)type.GetField("visual").GetValue(d);
            type.GetField("heavyReaction").SetValue(d,true);type.GetField("velocityX").SetValue(d,123f);
            visual.primary.Play(visual.FindClip("k3_behited"),0);visual.secondary.Stop();
            var go=new GameObject("F1 existing-reaction contact");
            try
            {
                go.transform.position=new Vector3(core.TestHarness_DinoX(0),136,0);core.TryF1ProjectileContact(go.AddComponent<FProjectileParticle>());
                Assert.That(core.TestHarness_DinoHP(0),Is.LessThan(1000));
                Assert.That((float)type.GetField("velocityX").GetValue(d),Is.EqualTo(123));
                Assert.That(visual.primary.Clip.name,Is.EqualTo("k3_behited"));Assert.That(visual.secondary.Clip,Is.Null);
            }
            finally{Object.DestroyImmediate(go);}
        }
    }
}
#endif
