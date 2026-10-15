#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    public class DC1LifecycleConvergenceRepro
    {
        CityGridController city;
        readonly Dictionary<string,int?> savedPrefs = new Dictionary<string,int?>();
        float? savedHealth;
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        object Get(string name) => typeof(CityGridController).GetField(name, Flags).GetValue(city);
        void Set(string name, object value) => typeof(CityGridController).GetField(name, Flags).SetValue(city, value);
        void Call(string name, params object[] args) => typeof(CityGridController).GetMethod(name, Flags).Invoke(city,args);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savedPrefs.Clear();
            savedHealth=DC1Preferences.HasKey(CityGrindStartPreset.HealthPrefKey)
                ? (float?)DC1Preferences.GetFloat(CityGrindStartPreset.HealthPrefKey) : null;
            for(int i=0;i<6;i++) {
                string key=CityGrindStartPreset.AmmoPrefPrefix+i;
                savedPrefs[key]=DC1Preferences.HasKey(key)?(int?)DC1Preferences.GetInt(key):null;
            }
            foreach(string key in new[]{"DC1_Debug_CityGrind_GodMode","DC1_Debug_CityGrind_InfiniteAmmo",NativeAudioSessionGate.FxMutedPrefKey,"DC1_MainMenu_SfxOn"})
            {
                savedPrefs[key]=DC1Preferences.HasKey(key) ? (int?)DC1Preferences.GetInt(key) : null;
                DC1Preferences.DeleteKey(key);
            }
            foreach(string suffix in new[]{"Level","MaxLevel","Lives","Cash","Difficulty",
                "Tier_0","Tier_1","Tier_2","Tier_3","Tier_4","Tier_G","OwnershipModelMigratedV1"})
            {
                string key="DC1_CityGrind_"+suffix;
                savedPrefs[key]=DC1Preferences.HasKey(key) ? (int?)DC1Preferences.GetInt(key) : null;
            }
            NativeAudioSessionGate.StopAll();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            city = Object.FindFirstObjectByType<CityGridController>();
            Assert.IsNotNull(city);
            city.TestHarness_SkipLevelIntro(); // These cases begin in native playing state4.
            city.TestHarness_ClearDinos();
        }

        [TearDown]
        public void TearDown()
        {
            foreach(var item in savedPrefs)
                if(item.Value.HasValue) DC1Preferences.SetInt(item.Key,item.Value.Value);
                else DC1Preferences.DeleteKey(item.Key);
            if(savedHealth.HasValue) DC1Preferences.SetFloat(CityGrindStartPreset.HealthPrefKey,savedHealth.Value);
            else DC1Preferences.DeleteKey(CityGrindStartPreset.HealthPrefKey);
            DC1Preferences.Save();
        }

        [System.Serializable] class NativeHouse { public NativeFrame[] frames; }
        [System.Serializable] class NativeFrame { public NativePart[] parts; }
        [System.Serializable] class NativePart { public string name; public NativeGeometry decoded; }
        [System.Serializable] class NativeGeometry { public float x, w; }

        [UnityTest]
        public IEnumerator B3_AllSixSkinsUseAuthoredLeafWidthsForAllNativeFrames()
        {
            var randomBefore=Random.state;
            var seen=new HashSet<int>();
            var source=new NativeHouse[3];
            for(int i=0;i<3;i++) source[i]=JsonUtility.FromJson<NativeHouse>(File.ReadAllText(
                Path.GetFullPath(Path.Combine(Application.dataPath,
                    "../../Dev/Reports/Task027N_2_36_5_CityGrindA2BTiming_RewardLifecycleClosure/house"+(i+1)+"_a2b_ir_n2365.json"))));
            city.enabled=false;
            try {
                for(int generation=0;generation<12 && seen.Count<6;generation++) {
                    Random.InitState(2510+generation);
                    Call("DisposeLevelContent");
                    Call("BuildLevelContent");
                    var buildings=(IList)Get("buildings");
                    foreach(var building in buildings) {
                        var type=building.GetType();
                        int skin=(int)type.GetField("skinIndex").GetValue(building);
                        int template=skin>=3?1:skin;
                        for(int frame=0;frame<11;frame++) {
                            type.GetField("occupancy").SetValue(building,CityGridController.BuildingOccupancy.Entering);
                            type.GetField("doorFrame").SetValue(building,frame);
                            Call("TickBuildings");
                            for(int leaf=1;leaf<=2;leaf++) {
                                var transform=(Transform)type.GetField("door"+leaf).GetValue(building);
                                var native=System.Array.Find(source[template].frames[frame].parts,p=>p.name=="com"+(leaf+2));
                                if(native==null) { Assert.IsNull(transform); continue; }
                                var sprite=transform.GetComponent<SpriteRenderer>().sprite;
                                Assert.AreEqual(native.decoded.w,sprite.rect.width*transform.localScale.x,0.0001f,
                                    "Skin "+skin+" frame "+frame+" leaf "+leaf+" must use authored signed width, including atlas-trim expansion.");
                            }
                        }
                        type.GetField("occupancy").SetValue(building,CityGridController.BuildingOccupancy.Outside);
                        seen.Add(skin);
                    }
                }
                Assert.AreEqual(6,seen.Count,"All six actual building skins must be observed, not skipped.");
            } finally { Random.state=randomBefore; }
            yield return null;
        }

        Transform FirstBuilding()
        {
            var list=(IList)Get("buildings");
            return (Transform)list[0].GetType().GetField("transform").GetValue(list[0]);
        }

        void CompleteExit(int action)
        {
            Call("BeginForcedWalkExit",action);
            city.TestHarness_SkipForcedWalkExit();
        }

        [UnityTest]
        public IEnumerator A_ShopDisposesCityEntitiesAndDoesNotSpawn()
        {
            Transform oldBuilding=FirstBuilding();
            city.TestHarness_ForceSpawn(0);
            CompleteExit(1);
            Assert.IsTrue(city.TestHarness_ShopScreenActive);
            Assert.IsFalse(city.TestHarness_PlayerControlSuspended);
            yield return null;
            Assert.IsTrue(oldBuilding==null,"Shop must unload level buildings, not hide them behind its board.");
            Assert.AreEqual(0, ((IList)Get("dinos")).Count);
            Set("cgT1Next",(float)Get("sceneTime")-1f);
            Call("CityGrindSpawnerTick");
            Assert.AreEqual(0, ((IList)Get("dinos")).Count,"An overdue city timer cannot construct a Shop dinosaur.");
        }

        [UnityTest]
        public IEnumerator A_RepeatedShopExitsCreateFreshBuildingsAndPreserveCampaignPlayer()
        {
            Set("cityLevel",1);
            Set("cashTotal",1234);
            Set("cityLives",2);
            city.TestHarness_SetPlayerHP(37f);
            city.TestHarness_GrantAmmo(0,24);
            int ammo=city.TestHarness_AmmoFor(0);
            Transform player=(Transform)Get("player");
            for(int level=1;level<=3;level++)
            {
                var list=(IList)Get("buildings");
                var old=list[0];
                old.GetType().GetField("lootConsumed").SetValue(old,true);
                Transform oldBuilding=FirstBuilding();
                int rootId=((Transform)Get("levelWorldRoot")).GetInstanceID();
                CompleteExit(1);
                Assert.AreEqual(level,Get("cityLevel"),"Level increments at Shop exit, not entry.");
                CompleteExit(2);
                yield return null;
                Assert.AreEqual(level+1,Get("cityLevel"));
                Assert.IsTrue(oldBuilding==null);
                var fresh=(IList)Get("buildings");
                Assert.AreEqual(5,fresh.Count);
                foreach(var b in fresh) Assert.IsFalse((bool)b.GetType().GetField("lootConsumed").GetValue(b));
                Assert.AreSame(player,Get("player"));
                Assert.AreNotEqual(rootId,((Transform)Get("levelWorldRoot")).GetInstanceID());
                Assert.AreEqual(1234,Get("cashTotal"));
                Assert.AreEqual(2,city.TestHarness_Lives);
                Assert.AreEqual(37f,city.TestHarness_PlayerHP);
                Assert.AreEqual(ammo,city.TestHarness_AmmoFor(0));
                Assert.AreEqual(0, ((IList)Get("dinos")).Count);
            }
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../Dev/Reports/TASK027N_2_51_CityGrindWholeModeClosure/LifecycleVisualMenu/a_lifecycle_trace.txt"));
            File.WriteAllLines(path,(List<string>)Get("worldLifecycleTrace"));
        }

        [UnityTest]
        public IEnumerator A_CityAndShopEffectsBelongToTheirOwnSceneContent()
        {
            var visual=((Transform)Get("player")).GetComponent<Power2DSourceTruthRenderer>();
            var blood=DinoCapDamageParticle.SpawnOrdinaryBlood(visual,Vector3.zero,-200f,100);
            Assert.IsNotNull(blood);
            // Particles are pooled: slot -> pool container -> scene effect root (NativeDamageParticlePool). Ownership = descendant of the scene root.
            Assert.IsTrue(blood.transform.IsChildOf((Transform)Get("levelWorldRoot")),"City blood must belong to the city world root.");
            CompleteExit(1);
            yield return null;
            Assert.IsTrue(blood==null,"City decals must not survive into Shop.");
            var shopBlood=DinoCapDamageParticle.SpawnOrdinaryBlood(visual,Vector3.zero,-200f,100);
            Assert.IsTrue(shopBlood.transform.IsChildOf((Transform)Get("shopWorldRoot")),"Shop blood must belong to the shop world root.");
            CompleteExit(2);
            yield return null;
            Assert.IsTrue(shopBlood==null,"Shop effects must not survive into the next city.");
        }

        [UnityTest]
        public IEnumerator A_ResumeLoadsFreshWorldAtSavedLevel()
        {
            Set("cityLevel",9);
            city.TestHarness_SaveProgressionState();
            Transform oldRoot=(Transform)Get("levelWorldRoot");
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            city=Object.FindFirstObjectByType<CityGridController>();
            Assert.IsTrue(oldRoot==null);
            Assert.AreEqual(9,Get("cityLevel"));
            Assert.AreEqual(1,Get("levelWorldGeneration"));
            var fresh=(IList)Get("buildings");
            Assert.AreEqual(5,fresh.Count);
            foreach(var b in fresh) Assert.IsFalse((bool)b.GetType().GetField("lootConsumed").GetValue(b));
        }

        [UnityTest]
        public IEnumerator AF_ActualMenuGoReconstructsPresetAndResumePreservesSavedInventory()
        {
            Set("cityLevel",5);
            Set("maxLevelReached",40);
            Set("cashTotal",1234);
            city.TestHarness_SetPlayerHP(37f);
            city.TestHarness_SaveProgressionState();
            Transform oldRoot=(Transform)Get("levelWorldRoot");
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            var menu=Object.FindFirstObjectByType<MainMenuController>();
            menu.TestHarness_PressGo();
            yield return null;
            city=Object.FindFirstObjectByType<CityGridController>();
            Assert.IsTrue(oldRoot==null);
            Assert.AreEqual(5,Get("cityLevel"));
            Assert.AreEqual(100f,city.TestHarness_PlayerHP);
            Assert.AreEqual(5,city.TestHarness_Lives);
            Assert.AreEqual(0,Get("cashTotal"));
            Assert.AreEqual(200,city.TestHarness_AmmoFor(0));
            Assert.AreEqual(50,city.TestHarness_AmmoFor(1));
            Assert.AreEqual(40,city.TestHarness_MaxLevelReached);
            city.TestHarness_SetPlayerHP(37f);
            Set("cashTotal",77);
            ((int[])Get("itemStock"))[0]=11;
            city.TestHarness_SaveProgressionState();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            menu=Object.FindFirstObjectByType<MainMenuController>();
            menu.TestHarness_PressResume();
            yield return null;
            city=Object.FindFirstObjectByType<CityGridController>();
            Assert.AreEqual(37f,city.TestHarness_PlayerHP);
            Assert.AreEqual(77,Get("cashTotal"));
            Assert.AreEqual(11,city.TestHarness_AmmoFor(0));
            Assert.AreEqual(50,city.TestHarness_AmmoFor(1));
        }

        void StepNativeHouse(float dt)
        {
            var visual=(Power2DSourceTruthRenderer)Get("playerVisual");
            Call("TickNativeHouseDoors",dt);
            visual.primary.Tick(dt);
            visual.secondary.Tick(dt);
            // Native draws the current frame every update; a clip started above frame 0 shows its start frame for one tick before advancing
            // (Power2DTrack.DrawSeededStartFrame). A render follows every real tick, so model it here.
            visual.primary.NotifyRendered();
            visual.secondary.NotifyRendered();
            Call("CompleteNativeHousePlayerClip");
        }

        [UnityTest]
        public IEnumerator B1_EntryAndEarlyExitRejectRepeatedActionsAcrossAllSixWeaponFamilies()
        {
            // Independent source oracle: frame6 current-at-tick-entry; player
            // entry [0,7], exit [3,10]; both raw house assets have 11 frames.
            foreach(string rig in new[]{"p_house","pd_house"}) {
                var raw=JsonUtility.FromJson<NativeHouse>(File.ReadAllText(Path.GetFullPath(
                    Path.Combine(Application.dataPath,"../../Dev/Reports/Task017_A2BExtractor/ir/a2b__"+rig+".a2b.ir.json"))));
                Assert.AreEqual(11,raw.frames.Length);
            }
            var trace=new List<string>{"family,phase,tick,action,occupancy,hp,ammo,clip"};
            city.TestHarness_SetPlayerHP(100f);
            for(int f=0;f<5;f++)city.TestHarness_SetRangedFamilyTierRaw(f,0);
            for(int f=0;f<6;f++)city.TestHarness_GrantAmmo(f,200);
            city.TestHarness_ForceSpawn(0);
            string[] actions={"RequestEnterBuilding","RequestMoveLeft","RequestMoveRight","RequestMelee","RequestShoot","RequestNextWeapon"};
            for(int family=0;family<6;family++) {
                city.TestHarness_SetRangedFamily(family);
                int index=family%city.TestHarness_BuildingCount;
                city.TestHarness_SetPlayerNear(index);
                city.TestHarness_PressDoor();
                var b=((IList)Get("buildings"))[index];
                float anchor=city.TestHarness_PlayerX;
                for(int tick=0;tick<8;tick++) {
                    string clip=city.TestHarness_PlayerPrimaryClipName;
                    foreach(string action in actions) {
                        int[] ammo=new int[6];for(int f=0;f<6;f++)ammo[f]=city.TestHarness_AmmoFor(f);
                        Call(action);
                        Assert.AreEqual(CityGridController.BuildingOccupancy.Entering,city.TestHarness_BuildingOccupancy(index));
                        Assert.AreEqual(clip,city.TestHarness_PlayerPrimaryClipName);
                        Assert.AreEqual(anchor,city.TestHarness_PlayerX);
                        for(int f=0;f<6;f++)Assert.AreEqual(ammo[f],city.TestHarness_AmmoFor(f));
                        trace.Add(family+",enter,"+tick+","+action+","+city.TestHarness_BuildingOccupancy(index)+","+city.TestHarness_PlayerHP+","+city.TestHarness_AmmoFor(family)+","+clip);
                    }
                    StepNativeHouse(0.025f);
                }
                Assert.AreEqual(CityGridController.BuildingOccupancy.Inside,city.TestHarness_BuildingOccupancy(index));
                Assert.IsTrue((bool)Get("playerHiddenForEntry"));
                city.TestHarness_PressDoor();
                Assert.AreEqual(CityGridController.BuildingOccupancy.Inside,city.TestHarness_BuildingOccupancy(index),"Native +40 door-completion gate rejects premature reopening.");
                for(int tick=0;tick<3;tick++)StepNativeHouse(0.025f);
                for(int hold=0;hold<3;hold++) Call("RequestShootHeld");
                Assert.AreEqual(CityGridController.BuildingOccupancy.Inside,city.TestHarness_BuildingOccupancy(index),
                    "A held fire update cannot synthesize a new native touch/door press.");
                city.TestHarness_PressDoor();
                // Native sub_76A20 draws THEN advances: the exit clip's start frame (3) is on screen for one tick before the clip advances
                // (registry animation.start_frame_is_drawn_one_tick_before_advance), so the player-clip COMPLETION is one callback later than the
                // old immediate-advance model (within 15 ticks). The reveal/loot-cancel milestone follows the door's frame 6 and is unchanged.
                for(int tick=0;tick<15;tick++) {
                    Assert.IsTrue(city.TestHarness_PlayerControlSuspended,"Control stays gated until player exit completion, including after door completion.");
                    foreach(string action in actions) {
                        Call(action);
                        Assert.AreEqual(CityGridController.BuildingOccupancy.Exiting,city.TestHarness_BuildingOccupancy(index));
                        trace.Add(family+",exit,"+tick+","+action+","+city.TestHarness_BuildingOccupancy(index)+","+city.TestHarness_PlayerHP+","+city.TestHarness_AmmoFor(family)+","+city.TestHarness_PlayerPrimaryClipName);
                    }
                    city.TestHarness_SetDinoX(0,anchor-1f);
                    city.TestHarness_SetDinoFacing(0,true);
                    city.TestHarness_SetDinoAttacking(0,true);
                    float hp=city.TestHarness_PlayerHP;
                    city.TestHarness_FireDinoPrimaryFrame(0,12);
                    Assert.AreEqual(hp,city.TestHarness_PlayerHP);
                    StepNativeHouse(0.025f);
                    Assert.AreEqual(tick<6,(bool)Get("playerHiddenForEntry"),"Reveal occurs on the seventh source callback, while frame6 is current.");
                    if(tick==6)Assert.IsFalse((bool)b.GetType().GetField("lootGrantTimerArmed").GetValue(b),"Native exit cancels pending loot.");
                }
                Assert.AreEqual(CityGridController.BuildingOccupancy.Outside,city.TestHarness_BuildingOccupancy(index));
                Assert.IsFalse(city.TestHarness_PlayerControlSuspended);
            }
            File.WriteAllLines(Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../Dev/Reports/TASK027N_2_51_CityGrindWholeModeClosure/LifecycleVisualMenu/b1_native_interruption_matrix.csv")),trace);
            yield return null;
        }

        [UnityTest]
        public IEnumerator B1_HeldTouchDoesNotReopenButNewTouchDoes()
        {
            city.TestHarness_SetHideOnScreenControls(false);
            city.TestHarness_SetRangedFamilyTierRaw(1,0);
            city.TestHarness_SetRangedFamily(1);
            city.TestHarness_GrantAmmo(1,200);
            foreach(string control in new[]{"moveLeft","moveRight","shoot"}) {
                city.TestHarness_SetPlayerNear(0);
                city.TestHarness_PressDoor();
                Call("UpdateOnScreenControlsPointerLayer");
                var rect=city.TestHarness_PointerControlRect(control);
                Assert.Greater(rect.width,0f);
                int pointer=city.TestHarness_PointerLayer.InjectDown(rect.center);
                for(int tick=0;tick<12;tick++) {
                    Call("UpdateOnScreenControlsPointerLayer");
                    StepNativeHouse(0.025f);
                }
                Assert.AreEqual(CityGridController.BuildingOccupancy.Inside,city.TestHarness_BuildingOccupancy(0),
                    control+" held across entry completion must not synthesize a new touch.");
                city.TestHarness_PointerLayer.InjectUp(pointer);
                Call("UpdateOnScreenControlsPointerLayer");
                pointer=city.TestHarness_PointerLayer.InjectDown(rect.center);
                Call("UpdateOnScreenControlsPointerLayer");
                Assert.AreEqual(CityGridController.BuildingOccupancy.Exiting,city.TestHarness_BuildingOccupancy(0),
                    "A genuinely new touch exits the completed house state.");
                city.TestHarness_PointerLayer.InjectUp(pointer);
                Call("UpdateOnScreenControlsPointerLayer");
                for(int tick=0;tick<15;tick++)StepNativeHouse(0.025f);   // one tick longer than the old model: the start frame is drawn first
                Assert.AreEqual(CityGridController.BuildingOccupancy.Outside,city.TestHarness_BuildingOccupancy(0));
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator B2_ASecondBuildingReplacesTheSingleScenePopupAndShopDisposesIt()
        {
            city.TestHarness_ForceLootType(0,6);
            var first=city.TestHarness_LastPopupGameObject;
            Assert.IsNotNull(first);
            city.TestHarness_ForceLootType(1,0);
            var second=city.TestHarness_LastPopupGameObject;
            Assert.IsNotNull(second);
            Assert.AreNotEqual(first.GetInstanceID(),second.GetInstanceID());
            yield return null;
            Assert.IsTrue(first==null,"Native mainScene+0x84 replaces the previous popup across buildings.");
            Assert.IsTrue(second!=null);
            foreach(var sr in second.GetComponentsInChildren<SpriteRenderer>()) Assert.AreEqual(1f,sr.color.a);
            CompleteExit(1);
            yield return null;
            Assert.IsTrue(second==null,"The last popup belongs to the disposed City scene content.");
            Assert.IsFalse(city.TestHarness_HasLastPopup);
        }

        [UnityTest]
        public IEnumerator B2_LootMovesOutOfViewportAtNativeVelocityWithOpaqueSprites()
        {
            city.TestHarness_GodMode=true;
            city.TestHarness_SetPlayerX(FirstBuilding().position.x);
            yield return null;
            city.TestHarness_ForceLootType(0,6);
            var popup=city.TestHarness_LastPopupGameObject.transform;
            float startTime=Time.time, startY=popup.position.y;
            var samples=new List<string>{"time,worldY,viewportY,alpha"};
            float deadline=Time.realtimeSinceStartup+12f;
            while(Time.time-startTime<8f)
            {
                Assert.Less(Time.realtimeSinceStartup,deadline,"Bounded popup observation timed out.");
                yield return null;
                if(samples.Count==1 || Time.time-startTime>samples.Count*0.5f)
                    samples.Add((Time.time-startTime)+","+popup.position.y+","+
                        Camera.main.WorldToViewportPoint(popup.position).y+","+
                        popup.GetComponentInChildren<SpriteRenderer>().color.a);
            }
            Assert.AreEqual(50f*(Time.time-startTime),popup.position.y-startY,3f);
            Assert.Greater(Camera.main.WorldToViewportPoint(popup.position).y,1f,
                "The actual camera clips the still-live, opaque popup after it rises above the viewport.");
            foreach(var sr in popup.GetComponentsInChildren<SpriteRenderer>()) Assert.AreEqual(1f,sr.color.a);
            File.WriteAllLines(Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../Dev/Reports/TASK027N_2_51_CityGrindWholeModeClosure/LifecycleVisualMenu/b2_screen_trajectory.csv")),samples);
        }

        [UnityTest]
        public IEnumerator F_DebugTogglesPersistAcrossSceneLoadsIndependentlyOfCampaignSave()
        {
            Call("SetDebugGodMode",true);
            Call("SetDebugInfiniteAmmo",true);
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            city=Object.FindFirstObjectByType<CityGridController>();
            Assert.IsTrue(city.TestHarness_GodMode);
            Assert.IsTrue(city.TestHarness_InfiniteAmmo);
            Call("SetDebugGodMode",false);
            Call("SetDebugInfiniteAmmo",false);
            Call("LoadDebugConvenienceSettings");
            Assert.IsFalse(city.TestHarness_GodMode);
            Assert.IsFalse(city.TestHarness_InfiniteAmmo);
        }

        [TestCase(1,"0,-1,-1,-1,-1,-1,0","200,0,0,0,0,0")]
        [TestCase(5,"1,0,-1,-1,-1,-1,1","200,50,0,0,0,0")]
        [TestCase(10,"2,1,0,-1,-1,-1,2","200,50,150,0,0,0")]
        [TestCase(15,"3,2,1,0,-1,-1,3","200,50,150,200,0,0")]
        [TestCase(20,"3,3,2,1,0,-1,3","200,50,150,200,30,0")]
        [TestCase(25,"3,3,3,2,1,0,3","200,50,150,200,30,250")]
        [TestCase(30,"3,3,3,3,1,1,3","200,50,150,200,30,250")]
        [TestCase(35,"3,3,3,3,1,2,3","200,50,150,200,30,250")]
        [TestCase(40,"3,3,3,3,1,2,3","200,50,150,200,30,250")]
        [TestCase(100,"3,3,3,3,1,2,3","200,50,150,200,30,250")]
        public void F_CheckpointPresetMatchesNativeUpgradePasses(int level,string tiers,string ammo)
        {
            var preset=CityGrindStartPreset.ForLevel(level);
            Assert.AreEqual(tiers,string.Join(",",preset.tiers));
            Assert.AreEqual(ammo,string.Join(",",preset.ammo));
        }

        [UnityTest]
        public IEnumerator E_A1QuickRetapsRequestAudioForEveryActualDischarge()
        {
            city.TestHarness_GodMode=true;
            city.TestHarness_SetRangedFamily(0);
            city.TestHarness_SetRangedTier(0,0);
            city.TestHarness_ResetShootCooldown();
            NativeAudioSessionGate.StopAll();
            NativeAudioSessionGate.ResetDiagnostics();
            NativeAudioSessionGate.TestHarness_ResetClipCounters();
            int before=city.TestHarness_AmmoFor(0);
            var records=new List<string>{"gameTime,spentAmmo,admittedAttackA,countedSessions,playingAttackAVoices"};
            for(int press=0;press<8;press++) {
                city.TestHarness_Shoot();
                float until=Time.time+0.11f;
                while(Time.time<until) yield return null;
                int playing=0;
                foreach(var voice in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                    if(voice.clip!=null && voice.clip.name=="Attack_A" && voice.isPlaying) playing++;
                records.Add(Time.time+","+(before-city.TestHarness_AmmoFor(0))+","+
                    NativeAudioSessionGate.TestHarness_AdmittedCount("Attack_A")+","+
                    NativeAudioSessionGate.CountedSessions+","+playing);
            }
            float finish=Time.time+0.2f;
            while(Time.time<finish) yield return null;
            int shots=before-city.TestHarness_AmmoFor(0);
            File.WriteAllLines(Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../Dev/Reports/TASK027N_2_51_CityGrindWholeModeClosure/LifecycleVisualMenu/e_a1_retap_trajectory.csv")),records);
            Assert.GreaterOrEqual(shots,4,"Reproducer must actually discharge repeatedly, not merely press.");
            Assert.AreEqual(0,NativeAudioSessionGate.TotalDropped,"This bounded burst must stay below the native voice cap.");
            Assert.AreEqual(shots,NativeAudioSessionGate.TestHarness_AdmittedCount("Attack_A"),
                "Native sub66DCC requests Attack_A for every discharge, including an A-family frame1 restart.");
        }

        [UnityTest]
        public IEnumerator C_OverlappingActorsCompositeWholeRigsAcrossSpeciesFacingsAndReactions()
        {
            var manifest=SourceTruthLoader.Load();
            var render=typeof(Power2DSourceTruthRenderer).GetMethod("Render",Flags);
            var backGo=new GameObject("C_BackActor");
            var frontGo=new GameObject("C_FrontActor");
            var back=backGo.AddComponent<Power2DSourceTruthRenderer>();
            var front=frontGo.AddComponent<Power2DSourceTruthRenderer>();
            foreach(var visual in new[]{back,front}) {
                visual.Initialize(manifest);
                visual.baseSortingOrder=200;
                visual.EnableActorComposition();
                visual.enabled=false;
            }
            backGo.transform.position=new Vector3(-10f,0f,0f);
            frontGo.transform.position=new Vector3(10f,0f,0f);
            var cameraGo=new GameObject("C_CompositeCamera");
            var camera=cameraGo.AddComponent<Camera>();
            camera.enabled=false;
            camera.orthographic=true;
            camera.orthographicSize=384f;
            camera.transform.position=new Vector3(0f,0f,-10f);
            camera.cullingMask=1<<31;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=Color.clear;
            var target=new RenderTexture(256,256,16,RenderTextureFormat.ARGB32);
            camera.targetTexture=target;
            var readback=new Texture2D(256,256,TextureFormat.RGBA32,false);
            var previous=RenderTexture.active;
            string diagnosticDir=Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../Dev/Reports/TASK027N_2_51_CityGrindWholeModeClosure/LifecycleVisualMenu"));
            var rows=new List<string>{"species,frontSpecies,suffix,backFacing,frontFacing,opaqueOverlapPixels,mismatchedPixels"};
            Color32[] Capture(bool showBack,bool showFront) {
                backGo.SetActive(showBack); frontGo.SetActive(showFront);
                SortingGroup.UpdateAllSortingGroups();
                camera.Render(); RenderTexture.active=target;
                readback.ReadPixels(new Rect(0,0,256,256),0,0); readback.Apply();
                return readback.GetPixels32();
            }
            try {
                for(int species=1;species<=4;species++)
                foreach(string suffix in new[]{"_move","_attack","_befired","_behited","_die"})
                for(int facing=0;facing<4;facing++) {
                    backGo.SetActive(true); frontGo.SetActive(true);
                    back.facingRight=(facing&1)!=0; front.facingRight=(facing&2)!=0;
                    back.primary.Play(back.FindClip("k"+species+suffix),3);
                    front.primary.Play(front.FindClip("k"+(species%4+1)+suffix),3);
                    render.Invoke(back,null); render.Invoke(front,null);
                    foreach(var visual in new[]{back,front})
                        foreach(var sr in visual.GetComponentsInChildren<SpriteRenderer>()) {
                            sr.gameObject.layer=31;
                            Assert.AreSame(visual.transform.Find("ActorParts"),sr.transform.parent);
                        }
                    // Let Unity register newly constructed renderer/group
                    // hierarchy before the independent camera capture.
                    yield return null;
                    var aloneBack=Capture(true,false);
                    var aloneFront=Capture(false,true);
                    var together=Capture(true,true);
                    if(species==1 && suffix=="_move" && facing==0) {
                        foreach(var item in new[]{(name:"c_back",pixels:aloneBack),(name:"c_front",pixels:aloneFront),(name:"c_both",pixels:together)}) {
                            readback.SetPixels32(item.pixels);readback.Apply();
                            File.WriteAllBytes(Path.Combine(diagnosticDir,item.name+".png"),readback.EncodeToPNG());
                        }
                        foreach(var group in new[]{back,front}) {
                            var sorting=group.GetComponentInChildren<SortingGroup>();
                            Debug.Log("C group "+group.name+" order="+sorting.sortingOrder+" pos="+sorting.transform.position+" enabled="+sorting.enabled);
                        }
                    }
                    int overlap=0,mismatched=0;
                    for(int pixel=0;pixel<together.Length;pixel++)
                        if(aloneFront[pixel].a>=254 && aloneBack[pixel].a>=254) {
                            overlap++;
                            if(System.Math.Abs(together[pixel].r-aloneFront[pixel].r)>2 ||
                               System.Math.Abs(together[pixel].g-aloneFront[pixel].g)>2 ||
                               System.Math.Abs(together[pixel].b-aloneFront[pixel].b)>2) mismatched++;
                        }
                    rows.Add(species+","+(species%4+1)+","+suffix+","+back.facingRight+","+front.facingRight+","+overlap+","+mismatched);
                    Assert.Greater(overlap,10,"Every matrix case must actually overlap rendered opaque pixels.");
                    Assert.AreEqual(0,mismatched,"A later native composite must occlude as one rig, without earlier actor parts piercing it: "+rows[rows.Count-1]);
                }
                DinoCapBloodBurst.Spawn(front,manifest,true,false,265);
                var burst=front.transform.Find("Task027N29_BloodBurst_dx");
                Assert.IsNotNull(burst);
                Assert.IsNull(burst.GetComponentInParent<SortingGroup>(),"Actor-attached effects must remain outside the rig group.");
                Assert.AreSame(front.transform,burst.parent);
            } finally {
                File.WriteAllLines(Path.GetFullPath(Path.Combine(Application.dataPath,
                    "../../Dev/Reports/TASK027N_2_51_CityGrindWholeModeClosure/LifecycleVisualMenu/c_actor_composite_pixels.csv")),rows);
                RenderTexture.active=previous;
                camera.targetTexture=null;
                Object.DestroyImmediate(backGo); Object.DestroyImmediate(frontGo);
                Object.DestroyImmediate(cameraGo); Object.DestroyImmediate(target); Object.DestroyImmediate(readback);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator F_DebugPresetAndLaterShopSaveLeaveCampaignUntouched()
        {
            DC1DebugSettings.AffectsSaves=false;
            city.TestHarness_SetLives(2);
            Set("cityLevel",1);
            city.TestHarness_SaveProgressionState();
            int lives=DC1Preferences.GetInt(CityGridController.LivesPrefKey);
            int cash=DC1Preferences.GetInt("DC1_CityGrind_Cash");
            city.TestHarness_DebugJumpLevel(39);
            Assert.AreEqual(40,Get("cityLevel"));
            Assert.AreEqual(100f,city.TestHarness_PlayerHP);
            Assert.AreEqual(lives,city.TestHarness_Lives,"Debug skipping must preserve consumed lives rather than refill them.");
            Assert.AreEqual(250,city.TestHarness_AmmoFor(5));
            CompleteExit(1);
            CompleteExit(2);
            yield return null;
            Assert.AreEqual(41,Get("cityLevel"));
            Assert.AreEqual(1,DC1Preferences.GetInt("DC1_CityGrind_Level"));
            Assert.AreEqual(lives,DC1Preferences.GetInt(CityGridController.LivesPrefKey));
            Assert.AreEqual(cash,DC1Preferences.GetInt("DC1_CityGrind_Cash"));
        }
    }
}
#endif
