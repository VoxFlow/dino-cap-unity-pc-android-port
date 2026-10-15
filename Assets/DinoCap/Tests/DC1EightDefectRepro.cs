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
    public class DC1EightDefectRepro
    {
        CityGridController city;
        GameplayRenderTarget.PresentationAspectMode savedAspect;
        readonly System.Collections.Generic.Dictionary<string,int?> savedDebugPrefs=new System.Collections.Generic.Dictionary<string,int?>();
        const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savedDebugPrefs.Clear();
            foreach(string key in new[]{"DC1_Debug_CityGrind_GodMode","DC1_Debug_CityGrind_InfiniteAmmo",NativeAudioSessionGate.FxMutedPrefKey,"DC1_MainMenu_SfxOn"}) {
                savedDebugPrefs[key]=DC1Preferences.HasKey(key)?(int?)DC1Preferences.GetInt(key):null;
                DC1Preferences.DeleteKey(key);
            }
            savedAspect=GameplayRenderTarget.AspectMode;
            GameplayRenderTarget.SetPersistentAspectMode(GameplayRenderTarget.PresentationAspectMode.SourceFaithful4x3);
            NativeAudioSessionGate.StopAll();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            city = Object.FindFirstObjectByType<CityGridController>();
            Assert.IsNotNull(city);
            city.TestHarness_SkipLevelIntro(); // Combat/door cases start in native playing state4.
            city.TestHarness_ClearDinos();
            city.TestHarness_DebugUiHidden = true;
        }

        [TearDown]
        public void RestorePresentation()
        {
            GameplayRenderTarget.SetPersistentAspectMode(savedAspect);
            foreach(var entry in savedDebugPrefs)
                if(entry.Value.HasValue)DC1Preferences.SetInt(entry.Key,entry.Value.Value);
                else DC1Preferences.DeleteKey(entry.Key);
        }

        [TestCase(1024,768,640,480)]
        [TestCase(1365,768,960,540)]
        [TestCase(1792,768,1280,548)]
        [TestCase(1024,768,1280,900)]
        public void CG05_HudViewportMatchesPresentedSurface(int rw,int rh,int sw,int sh)
        {
            Rect rect = GameplayPresentationCamera.ComputeGameViewScreenRect(rw,rh,sw,sh);
            Vector2 fit = GameplayPresentationCamera.ComputeFitScale(rw,rh,sw,sh);
            Assert.AreEqual(sw*fit.x/((float)sw/sh),rect.width,0.01f);
            Assert.AreEqual(sh*fit.y,rect.height,0.01f);
            Assert.GreaterOrEqual(rect.xMin,-0.01f);
            Assert.GreaterOrEqual(rect.yMin,-0.01f);
            Assert.LessOrEqual(rect.xMax,sw+0.01f);
            Assert.LessOrEqual(rect.yMax,sh+0.01f);
        }

        [UnityTest]
        public IEnumerator CG06_MeleeUiPressDoesNotSpendRangedAmmo()
        {
            city.TestHarness_SetRangedFamily(0);
            city.TestHarness_SetPlayerX(0f);
            city.TestHarness_GrantAmmo(0,20);
            int before = city.TestHarness_AmmoFor(0);
            var presenter = Object.FindFirstObjectByType<GameplayPresentationCamera>();
            Rect view = presenter.GetCurrentGameViewScreenRect();
            Vector2 logical = new Vector2(830.5f,600.5f);
            Vector2 screen = new Vector2(view.x+view.width*logical.x/1024f,
                view.y+view.height*logical.y/768f);
            // Optional seam lets this regression compile and fail on the
            // baseline too, where the fallback ignores control coordinates.
            typeof(CityGridController).GetField("TestOverrideMouseGuiPosition",PrivateInstance)
                ?.SetValue(city,(Vector2?)screen);
            city.TestHarness_SetMouseOverPauseButton(false);
            city.TestHarness_SetRawMouseLeft(true,true);
            typeof(CityGridController).GetMethod("PollKeyboard",PrivateInstance).Invoke(city,null);
            Assert.IsNull(city.TestHarness_LastFireClip,"A control click cannot start fallback fire audio/animation.");
            int id = city.TestHarness_PointerLayer.InjectDown(logical);
            yield return null;
            Assert.AreEqual(before,city.TestHarness_AmmoFor(0),
                "A melee UI click must not first commit the fallback ranged shot.");
            Assert.IsTrue(city.TestHarness_PlayerMeleeActive);
            Assert.IsFalse((bool)typeof(CityGridController).GetField("deferredFirePressQueued",PrivateInstance).GetValue(city),
                "UI melee cannot queue a fallback ranged shot for after the swing.");
            city.TestHarness_SetRawMouseLeft(false,false);
            city.TestHarness_PointerLayer.InjectUp(id);
            yield return null;
            float until=Time.realtimeSinceStartup+1.5f;
            while(Time.realtimeSinceStartup<until) yield return null;
            Assert.AreEqual(before,city.TestHarness_AmmoFor(0));
        }

        [UnityTest]
        public IEnumerator CG07_CompletedForcedWalkEntersInteractiveStore()
        {
            typeof(CityGridController).GetMethod("BeginForcedWalkExit",PrivateInstance)
                .Invoke(city,new object[]{1});
            Assert.IsTrue(city.TestHarness_PlayerControlSuspended);
            city.TestHarness_SkipForcedWalkExit();
            Assert.IsTrue(city.TestHarness_ShopScreenActive);
            Assert.IsFalse(city.TestHarness_PlayerControlSuspended,
                "The completed cutscene must release its own lock on upgrade-scene handoff.");
            float start = city.TestHarness_PlayerX;
            int id = city.TestHarness_PointerLayer.InjectDown(new Vector2(198f,590f));
            for(int frame=0;frame<5;frame++) yield return null;
            city.TestHarness_PointerLayer.InjectUp(id);
            Assert.Greater(city.TestHarness_PlayerX,start);
        }

        [UnityTest]
        public IEnumerator CG02_AllSixSkinsOwnNativeDoorsAndMd()
        {
            bool[] seen=new bool[6];
            // Exercise actual production construction, independent of random initial skin selection.
            Random.State saved=Random.state;
            for(int seed=0;seed<40;seed++) {
                RebuildBuildings(seed);
                for(int i=0;i<city.TestHarness_BuildingCount;i++) {
                    int skin=city.TestHarness_BuildingSkinIndex(i); seen[skin]=true;
                    Assert.IsTrue(city.TestHarness_BuildingHasDoor(i),"Native loader clones populated house2 into house4-6.");
                    int template=skin<3?skin:1;
                    Assert.AreEqual("fangzi"+(template+1)+"_mdFrame",city.TestHarness_BuildingMdOverlaySpriteName(i));
                    if(skin>=3) {
                        Assert.AreEqual(-46.11109924316406f,city.TestHarness_BuildingDoorLocalX(i),0.001f);   // raw native com3 x from the native object origin (body offset is applied to the body)
                        Assert.AreEqual(38.556f,city.TestHarness_Door1LocalY(i),0.001f);
                    }
                }
                if(System.Array.TrueForAll(seen,x=>x)) break;
                yield return null;
            }
            Random.state=saved;
            Assert.IsTrue(System.Array.TrueForAll(seen,x=>x),"Must actually exercise every skin.");
        }

        // Diagnostic (explicit): render the Deli with the nearest decorations so the placement can be compared with the original
        // footage. Writes only to the system temp folder (or DC1_CAPTURE_DIR), never under Reports.
        [NUnit.Framework.Explicit("Render capture for manual comparison")]
        [UnityTest]
        public IEnumerator DIAG_DeliAndNearbyDecorationsCapture()
        {
            string dir=System.Environment.GetEnvironmentVariable("DC1_CAPTURE_DIR");
            if(string.IsNullOrEmpty(dir)) dir=System.IO.Path.GetTempPath();
            for(int seed=0;seed<200;seed++)
            {
                RebuildBuildings(seed);
                for(int i=0;i<city.TestHarness_BuildingCount;i++)
                {
                    if(city.TestHarness_BuildingSkinIndex(i)!=2) continue;
                    float origin=city.TestHarness_BuildingOriginX(i);
                    city.TestHarness_SetPlayerX(origin-60f);
                    for(int f=0;f<8;f++) yield return null;
                    string path=System.IO.Path.Combine(dir,"dc1_deli_seed"+seed+".png");
                    string result=city.TestHarness_CaptureRenderTarget(path);
                    UnityEngine.Debug.Log("[DELI_DIAG] seed="+seed+" building="+i+" origin="+origin+" bodyX="+city.TestHarness_BuildingBodyX(i)+" capture="+result);
                    if(seed>=2) yield break;
                    break;
                }
            }
        }

        void RebuildBuildings(int seed)
        {
            var list=(System.Collections.IList)typeof(CityGridController).GetField("buildings",PrivateInstance).GetValue(city);
            foreach(object building in list) {
                var transform=(Transform)building.GetType().GetField("transform").GetValue(building);
                Object.DestroyImmediate(transform.gameObject);
            }
            list.Clear();
            Random.InitState(seed);
            typeof(CityGridController).GetMethod("BuildBuildings",PrivateInstance).Invoke(city,null);
        }

        [UnityTest]
        public IEnumerator CG02_AllSkinsCompleteLitDarkDoorLifecycle()
        {
            string directory=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../Dev/Operator/DC1-MASTER/Captures"));
            System.IO.Directory.CreateDirectory(directory);
            for(int skin=0;skin<6;skin++) {
                int index=-1;
                for(int seed=0;seed<40 && index<0;seed++) {
                    RebuildBuildings(seed);
                    for(int b=0;b<city.TestHarness_BuildingCount;b++)
                        if(city.TestHarness_BuildingSkinIndex(b)==skin) { index=b; break; }
                }
                Assert.GreaterOrEqual(index,0);
                city.TestHarness_SetPlayerNear(index);
                city.TestHarness_SetRangedFamily(1);
                city.TestHarness_SetRangedTier(1,0);
                for(int frame=0;frame<3;frame++) yield return null;
                Assert.IsNotNull(city.TestHarness_CaptureRenderTarget(System.IO.Path.Combine(directory,"skin"+skin+"_lit.png")));
                city.TestHarness_PressDoor();
                yield return null;
                Assert.IsNotNull(city.TestHarness_CaptureRenderTarget(System.IO.Path.Combine(directory,"skin"+skin+"_enter.png")));
                float until=Time.realtimeSinceStartup+3f;
                while(!city.TestHarness_BuildingLootConsumed(index) && Time.realtimeSinceStartup<until) yield return null;
                Assert.IsTrue(city.TestHarness_BuildingLootConsumed(index));
                int template=skin<3?skin:1;
                Assert.AreEqual("fangzi"+(template+1)+"_md_aFrame",city.TestHarness_BuildingMdOverlaySpriteName(index));
                Assert.AreEqual("fangzi"+(skin+1)+"_aFrame",city.TestHarness_BuildingBodySpriteName(index));
                // ReadPixels sees the last rendered target, so let this
                // newly applied dark state reach the camera before capture.
                yield return null;
                Assert.IsNotNull(city.TestHarness_CaptureRenderTarget(System.IO.Path.Combine(directory,"skin"+skin+"_dark.png")));
                city.TestHarness_PressDoor();
                yield return null;
                Assert.IsNotNull(city.TestHarness_CaptureRenderTarget(System.IO.Path.Combine(directory,"skin"+skin+"_exit.png")));
                until=Time.realtimeSinceStartup+1f;
                while(city.TestHarness_BuildingOccupancy(index)!=CityGridController.BuildingOccupancy.Outside && Time.realtimeSinceStartup<until) yield return null;
                Assert.AreEqual(CityGridController.BuildingOccupancy.Outside,city.TestHarness_BuildingOccupancy(index));
            }
        }

        [UnityTest]
        public IEnumerator CG07_StoreActionsAndReturnUseSharedPlayerOwners()
        {
            typeof(CityGridController).GetMethod("BeginForcedWalkExit",PrivateInstance).Invoke(city,new object[]{1});
            city.TestHarness_SkipForcedWalkExit();
            city.TestHarness_ForceShopFadeComplete();
            city.TestHarness_SetPlayerHP(50f);
            city.TestHarness_SetCashTotal(10000);
            Assert.IsTrue(city.TestHarness_TryBuyHpRefill());
            Assert.Greater(city.TestHarness_PlayerHP,50f);
            city.TestHarness_SetRangedFamily(0);
            city.TestHarness_SetRangedTier(0,0);
            city.TestHarness_GrantAmmo(0,20);
            city.TestHarness_RequestMelee();
            Assert.IsTrue(city.TestHarness_PlayerMeleeActive);
            float until=Time.realtimeSinceStartup+2f;
            while(city.TestHarness_PlayerMeleeActive && Time.realtimeSinceStartup<until) yield return null;
            int ammo=city.TestHarness_AmmoFor(0);
            city.TestHarness_RequestShoot();
            until=Time.realtimeSinceStartup+2f;
            while(city.TestHarness_AmmoFor(0)>=ammo && Time.realtimeSinceStartup<until) yield return null;
            Assert.Less(city.TestHarness_AmmoFor(0),ammo);
            string directory=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../Dev/Operator/DC1-MASTER/Captures"));
            city.TestHarness_CaptureRenderTarget(System.IO.Path.Combine(directory,"store_world.png"));
            // Unity Test Framework batch mode cannot capture the presented IMGUI frame.
            // Full-display captures are made by the development player diagnostic.
            city.TestHarness_ExitShopAndContinue();
            city.TestHarness_SkipForcedWalkExit();
            Assert.IsFalse(city.TestHarness_ShopScreenActive);
            Assert.IsFalse(city.TestHarness_PlayerControlSuspended);
        }

        [Test]
        public void CG03_CompositeBuildingLayerKeepsPropsBehind()
        {
            for(int i=0;i<city.TestHarness_BuildingCount;i++) {
                Assert.AreEqual(10,city.TestHarness_BuildingBodySortingOrder(i),"CAObj +0x5c copied to child sprite +36.");
                for(int j=0;j<city.TestHarness_DecorationCount;j++) {
                    Assert.AreEqual(4,city.TestHarness_DecorationSortingOrder(j));
                    Assert.Less(city.TestHarness_DecorationSortingOrder(j),city.TestHarness_BuildingBodySortingOrder(i));
                }
            }
        }

        [Test]
        public void CG04_EveryHouseRigUsesFullNativeWeaponPatch()
        {
            DCManifest manifest=SourceTruthLoader.Load();
            var visual=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",PrivateInstance).GetValue(city);
            MethodInfo build=typeof(Power2DSourceTruthRenderer).GetMethod("FindWeaponHouseClip");
            Assert.IsNotNull(build,"Baseline ref-only substitution lacks native C2B geometry.");
            foreach(DCPatch patch in manifest.patches) {
                DCPart q=patch.part;
                DCAnimation raw=visual.FindClip(q.name=="com1"?"p_house":"pd_house");
                DCAnimation clip=(DCAnimation)build.Invoke(visual,new object[]{patch.name});
                for(int frame=0;frame<raw.frames.Length;frame++) {
                    DCPart source=System.Array.Find(raw.frames[frame].parts,p=>p.name==q.name);
                    DCPart actual=System.Array.Find(clip.frames[frame].parts,p=>p.name==q.name);
                    // Native7D81C case5 modes1/2 both apply the complete
                    // patch to pd_house.7DCAE is F1 nested animation.
                    Assert.IsNotNull(actual,patch.name);
                    Assert.AreEqual(source.x+q.x,actual.x,0.0001f,patch.name);
                    Assert.AreEqual(source.y+q.y,actual.y,0.0001f,patch.name);
                    Assert.AreEqual(q.w,actual.w); Assert.AreEqual(q.h,actual.h);
                    Assert.AreEqual(q.a,actual.a); Assert.AreEqual(q.b,actual.b);
                    Assert.AreEqual(source.c+q.c,actual.c,0.0001f);
                    Assert.AreEqual(q.Ref,actual.Ref);
                    Assert.AreEqual(source.dBits,actual.dBits);
                }
                Assert.AreSame(raw,visual.FindClip(raw.name),"Raw clip ownership must stay immutable.");
            }
        }

        [TestCase(640,480)]
        [TestCase(1920,1080)]
        [TestCase(768,1365)]
        public void CG08_FadeCoversWholeDisplay(int width,int height)
        {
            var method=typeof(CityGridController).GetMethod("FullScreenFadeRect",BindingFlags.Static|BindingFlags.NonPublic);
            Assert.IsNotNull(method,"Both fade paths need one display-space owner.");
            Rect rect=(Rect)method.Invoke(null,new object[]{width,height});
            Assert.AreEqual(Vector2.zero,rect.position);
            Assert.AreEqual(new Vector2(width,height),rect.size);
        }

        [UnityTest]
        public IEnumerator CG01_OneContactDamagesEveryEligibleDino() => CheckMelee(0);

        [UnityTest]
        public IEnumerator CG01_NativeType2GetsDamageBloodAndHeavyReaction() => CheckMelee(1);

        [UnityTest]
        public IEnumerator CG01_NativeType3GetsDamageBloodWithoutHeavyReaction() => CheckMelee(2);

        IEnumerator CheckMelee(int kind)
        {
            city.TestHarness_JungleRawDraw(()=>0); // Contact-cardinality fixture requires targetable ground actors.
            city.TestHarness_SetPlayerX(0f);
            typeof(CityGridController).GetField("godMode",PrivateInstance).SetValue(city,true);
            city.TestHarness_ForceSpawn(kind);
            city.TestHarness_ForceSpawn(kind);
            var playerVisual = (Power2DSourceTruthRenderer)typeof(CityGridController)
                .GetField("playerVisual",PrivateInstance).GetValue(city);
            playerVisual.facingRight = true;
            var targets = (System.Collections.IList)typeof(CityGridController)
                .GetField("dinos",PrivateInstance).GetValue(city);
            // This verifies contact cardinality/reaction, not AI motion. Hold
            // both target poses inside the native strict forward window until
            // the player's actual frame10 event. Random jump/attack animations
            // otherwise move a head outside that window during the swing.
            city.enabled=false;
            for(int i=0;i<2;i++) {
                object target=targets[i];
                var targetType=target.GetType();
                targetType.GetField("facingRight").SetValue(target,false);
                var visual=(Power2DSourceTruthRenderer)targetType.GetField("visual").GetValue(target);
                visual.enabled=false;
                visual.facingRight=true; // Native dino art polarity is opposite the physical facing.
                Vector3 head=Vector3.zero;
                Assert.IsTrue(visual.TryGetPartWorldAnchor("tou",out head));
                city.TestHarness_SetDinoX(i,city.TestHarness_DinoX(i)+(150f+i*20f)-head.x);
                Assert.IsTrue(visual.TryGetPartWorldAnchor("tou",out head));
                Assert.AreEqual(150f+i*20f,head.x-playerVisual.transform.position.x,0.001f);
            }
            city.TestHarness_SetDinoHP(0,10000f);
            city.TestHarness_SetDinoHP(1,10000f);
            yield return null;
            DinoCapDamageParticle.TestHarness_ResetSpawnCounters();
            DinoCapBloodBurst.ResetDebugCounts();
            playerVisual.primary.EnterFrame += (track, frame) => {
                if(frame!=10 || track.Clip==null || !track.Clip.name.StartsWith("p_hit_G")) return;
                for(int i=0;i<2;i++) {
                    object target=targets[i];
                    var visual=(Power2DSourceTruthRenderer)target.GetType().GetField("visual").GetValue(target);
                    visual.TryGetPartWorldAnchor("tou",out Vector3 head);
                    Debug.Log("CG01 eligibility kind="+kind+" target="+i+" dx="+
                        (head.x-playerVisual.transform.position.x)+" facing="+visual.facingRight+
                        " clip="+visual.primary.Clip.name+" frame="+visual.primary.Frame);
                }
            };
            city.TestHarness_RequestMelee();
            float end = Time.realtimeSinceStartup+3f;
            while(city.TestHarness_DinoHP(0)>=10000f && Time.realtimeSinceStartup<end)
                yield return null;
            Assert.Less(city.TestHarness_DinoHP(0),10000f);
            Assert.Less(city.TestHarness_DinoHP(1),10000f,
                "Native group=1 contacts all valid head anchors, rather than the nearest root.");
            Assert.GreaterOrEqual(DinoCapDamageParticle.TestHarness_SpawnCallCount(DinoCapDamageParticle.Kind.Blood),6,"Each nonfatal G contact spawns three native ballistic particles.");
            Assert.GreaterOrEqual(DinoCapBloodBurst.DxSpawnCount,2);
            for(int i=0;i<2;i++)
                if(city.TestHarness_DinoKind(i)==2)
                    Assert.IsFalse(city.TestHarness_DinoHeavyReaction(i),"Native type3 ordinary melee cannot knock back (City art prefix k2).");
                else Assert.IsTrue(city.TestHarness_DinoHeavyReaction(i),"k1/k2 ordinary melee must enter heavy reaction.");
        }
    }
}
#endif
