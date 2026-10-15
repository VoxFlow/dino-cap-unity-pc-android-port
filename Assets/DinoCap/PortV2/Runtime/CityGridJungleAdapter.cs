using System;
using UnityEngine;

namespace DinoCap.PortV2
{
    public sealed partial class CityGridController
    {
        JungleBlitzController jungle;
        JungleWorldLayers jungleWorld;
        NativeSceneTimerQueue jungleTimers;
        JungleSpawnSchedule jungleSchedule;
        JungleBlitzProgression jungleProgression;
        JungleSpawnPlacement.JumpBounds jungleJumpBounds;
        readonly System.Random jungleRandom=new System.Random();
        readonly byte[] jungleRandomBytes=new byte[4];
        bool jungleTimerStarted;
        float junglePlayerPhysicsVelocity;
        readonly System.Collections.Generic.List<JunglePickupActor> junglePickups=new System.Collections.Generic.List<JunglePickupActor>();
        int jungleKills;
        JungleAmmoNotification jungleNotification;
        NativeAmmoMetrics jungleTimeMetrics;
        int jungleSecondTimer;
        bool jungleResultsVisible,jungleDoneRequested;
        NativeUiRectTween jungleDoneTween;
        float jungleTimerSpeed=1f;
        string jungleDebugTime="0";
        readonly System.Collections.Generic.List<JungleProjectileActor> jungleProjectiles=new System.Collections.Generic.List<JungleProjectileActor>();
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
        Func<int> jungleRawOverride;
        const string JungleDebugSpeciesKey="DC1_Debug_Jungle_FutureSpeciesMask";
        int jungleDebugSpeciesMask=15;
        bool jungleDebugRemoveExcluded;
#endif
        float GameplayWorldRight=>jungle!=null?JungleBlitzController.NativeWorldRight:WorldRight;

        void InitializeJungleSession()
        {
            // E8E48 then13FD4: reset local weapon vectors, Aclass3/999,
            // Gclass0 and HP100. Do not load/migrate or overwrite City save.
            Array.Clear(itemStock,0,itemStock.Length);
            Array.Clear(weaponBonus,0,weaponBonus.Length);
            for(int i=0;i<rangedFamilyTier.Length;i++)rangedFamilyTier[i]=-1;
            rangedFamilyTier[0]=3;itemStock[0]=999;
            meleeTier=0;equippedRangedFamily=0;ammoInitialized=true;
            playerHP=PlayerMaxHP;
            cashTotal=jungle.CarriedCash;cityLives=jungle.CarriedLives;
            if(!jungle.HasExplicitSession)
            {
                var session=NativeModeSession.Read();cashTotal=session.cash;cityLives=session.lives;
            }
            cityDifficulty=Mathf.Clamp(DC1Preferences.GetInt(DifficultyPrefKey,1),1,3);
            pendingDeathReplay=null;
            jungleProgression=JungleBlitzProgression.Load();
            jungleJumpBounds=JungleSpawnPlacement.LoadNativeJumpBounds();
            jungleTimers=new NativeSceneTimerQueue();
            jungleSchedule=new JungleSpawnSchedule(jungleProgression,
                (type,delay)=>jungleTimers.Arm(delay,handle=>jungleSchedule.OnDinoTimer(handle,SpawnJungleDino)));
        }
        void BuildJungleWorld()
        {
            levelWorldGeneration++;
            levelWorldRoot=new GameObject("JungleWorld_"+levelWorldGeneration).transform;
            jungleWorld=levelWorldRoot.gameObject.AddComponent<JungleWorldLayers>();
            // Port actors use SortingGroups100/200. Native foreground12
            // follows those actors; map it above their groups in this port.
            jungleWorld.Initialize(0,300);
            var notice=new GameObject("JungleAmmoNotification");notice.transform.SetParent(levelWorldRoot,false);
            jungleNotification=notice.AddComponent<JungleAmmoNotification>();
            levelIntro=NativeLevelIntro.CreateJungle();
            levelInitialFade=255f;levelClearTween=null;
            left=right=false;
        }
        void TickJungleWorld()=>jungleWorld?.Tick(cameraLeftX,CurrentViewWidth);
        void TickJungleTimers(float dt)
        {
            //17BA5C calls182058 BEFORE UI/actor/scene updates. Advancing
            // the clock before START completes avoids ticking newly armed
            //17A08 timers in their own admission frame.
            jungleTimers.Tick(dt);
        }
        void AdmitJungleGameplay()
        {
            if(!jungleTimerStarted)
            {
                if(LevelIntroActive)return;
                jungleTimerStarted=true;
                //17A08 arms g_time FIRST, then the four species timers.
                jungleSecondTimer=jungleTimers.Arm(JungleSecondDelay,JungleSecond);
                jungleSchedule.Start();
            }
        }
        void JungleSecond(int handle)
        {
            jungleSchedule.OnSecond();
            jungleSecondTimer=jungleTimers.Arm(JungleSecondDelay,JungleSecond);
        }
        float JungleSecondDelay {
            get {
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
                return DC1DebugSettings.Enabled?1f/jungleTimerSpeed:1f;
#else
                return 1f;
#endif
            }
        }
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
        void LoadJungleDebugSettings()
        {
            jungleTimerSpeed=DC1DebugSettings.Enabled?SanitizeJungleTimerSpeed(DC1Preferences.GetFloat("DC1_Debug_Jungle_TimerSpeed",1)):1f;
            jungleDebugSpeciesMask=DC1DebugSettings.Enabled?(DC1Preferences.GetInt(JungleDebugSpeciesKey,15)&15):15;
            jungleDebugRemoveExcluded=false;
        }
        void SetJungleDebugSpecies(int mask,bool removeExisting)
        {
            jungleDebugSpeciesMask=mask&15;jungleDebugRemoveExcluded=removeExisting;
            DC1Preferences.SetInt(JungleDebugSpeciesKey,jungleDebugSpeciesMask);DC1Preferences.Save();
            if(removeExisting)
                foreach(var d in dinos)
                    if((jungleDebugSpeciesMask&(1<<(int)d.kind))==0)d.markedForRemoval=true;
        }
        static float SanitizeJungleTimerSpeed(float speed)
            =>float.IsNaN(speed)||float.IsInfinity(speed)?1f:Mathf.Clamp(speed,1f,100f);
        void SetJungleTimerSpeed(float speed)
        {
            jungleTimerSpeed=SanitizeJungleTimerSpeed(speed);
            DC1Preferences.SetFloat("DC1_Debug_Jungle_TimerSpeed",jungleTimerSpeed);DC1Preferences.Save();
            if(jungleTimerStarted && !playerDead && !jungleResultsVisible)
            {
                jungleTimers.Cancel(jungleSecondTimer);
                jungleSecondTimer=jungleTimers.Arm(JungleSecondDelay,JungleSecond);
            }
        }
        void SetJungleDebugTime(int seconds)
        {
            if(playerDead || jungleResultsVisible)return;
            jungleSchedule.DebugSetElapsed(seconds);
            jungleDebugTime=jungleSchedule.ElapsedSeconds.ToString();
        }
        void ResetJungleDebug()
        {
            SetDebugGodMode(false);SetDebugInfiniteAmmo(false);SetDebugMovementSpeed(1);
            SetJungleTimerSpeed(1);SetJungleDebugTime(0);
            SetJungleDebugSpecies(15,false);
        }
        void DrawJungleDebugControls()
        {
            GUI.Label(new Rect(10,192,160,26),"DEBUG TIMER x"+jungleTimerSpeed.ToString("0.##"));
            if(GUI.Button(new Rect(170,192,95,26),"TIMER x1"))SetJungleTimerSpeed(1);
            if(GUI.Button(new Rect(270,192,95,26),"TIMER x10"))SetJungleTimerSpeed(10);
            if(GUI.Button(new Rect(370,192,95,26),"TIMER x100"))SetJungleTimerSpeed(100);
            if(GUI.Button(new Rect(470,192,160,26),"RESET JUNGLE DEBUG"))ResetJungleDebug();
            GUI.Label(new Rect(10,222,165,26),"DEBUG TIME (seconds)");
            jungleDebugTime=DC1DebugTextInput.Field(new Rect(175,222,95,26),jungleDebugTime);
            if(GUI.Button(new Rect(280,222,90,26),"SET TIME") && int.TryParse(jungleDebugTime,out int seconds))SetJungleDebugTime(seconds);
            if(GUI.Button(new Rect(375,222,160,26),"SPAWN K4 NOW"))DebugSpawnJungleK4();
            GUI.Label(new Rect(10,252,160,26),"FUTURE DINO TYPES:");
            for(int i=0;i<4;i++)
            {
                bool old=(jungleDebugSpeciesMask&(1<<i))!=0;
                bool selected=GUI.Toggle(new Rect(175+i*80,252,75,26),old,"TYPE "+(i+1));
                if(selected!=old)SetJungleDebugSpecies(jungleDebugSpeciesMask^(1<<i),jungleDebugRemoveExcluded);
            }
            bool remove=GUI.Toggle(new Rect(10,282,440,26),jungleDebugRemoveExcluded,"ALSO DESPAWN EXISTING EXCLUDED TYPES (OPT-IN)");
            if(remove!=jungleDebugRemoveExcluded)SetJungleDebugSpecies(jungleDebugSpeciesMask,remove);
            GUI.Label(new Rect(10,312,680,22),"Debug preference only; skipped spawns consume placement draws and keep timer cadence.");
        }
        internal void TestHarness_JungleSetTimerSpeed(float speed)=>SetJungleTimerSpeed(speed);
        internal float TestHarness_JungleTimerSpeed=>jungleTimerSpeed;
        internal void TestHarness_JungleSetTime(int seconds)=>SetJungleDebugTime(seconds);
        internal void TestHarness_JungleResetDebug()=>ResetJungleDebug();
        internal void TestHarness_JungleSpeciesFilter(int mask,bool remove)=>SetJungleDebugSpecies(mask,remove);
        internal int TestHarness_JungleSpeciesMask=>jungleDebugSpeciesMask;
        void DebugSpawnJungleK4()
        {
            if(!DC1DebugSettings.Enabled || jungle==null || !jungleTimerStarted || playerDead || jungleResultsVisible)return;
            // Explicit debug admission bypasses only the long timer and the
            // future-species filter. Native stats, placement and combat remain
            // owned by the same path; elapsed time/timer handles stay intact.
            SpawnJungleDino(4,false);
            status="DEBUG K4 spawn requested at current Jungle time; normal timers unchanged.";
        }
        internal void TestHarness_JungleDebugSpawnK4()=>DebugSpawnJungleK4();
        internal int TestHarness_JungleK4TimerHandle=>jungleSchedule.HandleForType(4);
#endif
        int NextJungleRaw31()
        {
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            if(jungleRawOverride!=null)return jungleRawOverride();
#endif
            // Uniform31-bit engine adapter; does not claim the original
            // iOS libc seed stream. Native F32 consumption lives in models.
            jungleRandom.NextBytes(jungleRandomBytes);
            return BitConverter.ToInt32(jungleRandomBytes,0)&int.MaxValue;
        }
        void SpawnJungleDino(int type)=>SpawnJungleDino(type,true);
        void SpawnJungleDino(int type,bool applySpeciesFilter)
        {
            var stats=jungleProgression.QuerySpawn(jungleSchedule.ElapsedSeconds,type,cityDifficulty);
            int playerState=playerDead?4:playerControlSuspended?6:0;
            var placement=JungleSpawnPlacement.Decide(type,playerState,cameraLeftX,CurrentViewWidth,
                junglePlayerPhysicsVelocity,playerVisual.facingRight,stats.speed,jungleJumpBounds,NextJungleRaw31);
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
            // Consume normal placement/admission draws first. Filtering does
            // not reroll species, cancel a timer, or affect campaign progress.
            if(applySpeciesFilter && (jungleDebugSpeciesMask&(1<<(type-1)))==0)return;
#endif
            if(placement.admitted)SpawnCityDino(type-1,0,cityDifficulty,stats,placement);
        }
        void CreateJungleProjectile(CityDinoInstance d)
        {
            var go=new GameObject("JungleK4Projectile");go.transform.SetParent(levelWorldRoot,false);
            var actor=go.AddComponent<JungleProjectileActor>();
            actor.Initialize(data,d.transform.position,d.facingRight);
            jungleProjectiles.Add(actor);
        }
        void TickJungleProjectiles(float dt)
        {
            for(int i=jungleProjectiles.Count-1;i>=0;i--)
            {
                var actor=jungleProjectiles[i];
                if(actor==null){jungleProjectiles.RemoveAt(i);continue;}
                actor.Tick(dt,player.position.x,sceneTime<invulnUntil,JungleProjectileHit);
            }
        }
        bool JungleProjectileHit()
        {
            //13B10 reads current interval ATK, not spawn difficulty-scaled
            //ATK. It grants Wudi/RoleAudio with no normal bite knockback,
            //hurt clip, facing change or sustained-fire interruption.
            if(!godMode)playerHP-=jungleProgression.Query(jungleSchedule.ElapsedSeconds,4).attack;
            if(playerHP<=0f && !playerDead)
            {
                playerHP=0;StartCoroutine(PlayerDeathSequence());return false;
            }
            invulnUntil=sceneTime+1.5f;
            //13B10 sets Wudi without restarting65C78 brightness/rate.
            EnsureCombatAudio();NativeAudioSessionGate.Play(combatAudioSource,playerHurtClip);
            return true;
        }
        void JungleDinoKilled(CityDinoInstance d)
        {
            //10B90 raw10C90..10CA8 passes dinosaurY-230 into19E60;
            //1AD48 then adds20. Ground root136 yields pickup centre-74.
            var decision=JungleDropSelection.Roll(jungleSchedule.ElapsedSeconds,(int)d.kind+1,NextJungleRaw31);
            if(decision.HasDrop)CreateJunglePickup(decision,d.transform.position.x,d.transform.position.y-230f);
            jungleKills++;
        }
        JunglePickupActor CreateJunglePickup(JungleDropSelection.Decision decision,float x,float nativeDropY)
        {
            var go=new GameObject("JungleDeathDrop");go.transform.SetParent(levelWorldRoot,false);
            var actor=go.AddComponent<JunglePickupActor>();actor.Initialize(decision,x,nativeDropY,jungleTimers);
            junglePickups.Add(actor);return actor;
        }
        JunglePickupInventory.Outcome ApplyJunglePickup(JungleDropSelection.Decision decision)
        {
            var classes=new int[7];Array.Copy(rangedFamilyTier,classes,6);classes[6]=meleeTier;
            var outcome=JunglePickupInventory.Apply(decision,classes,itemStock,equippedRangedFamily,NextJungleRaw31);
            Array.Copy(classes,rangedFamilyTier,6);meleeTier=classes[6];
            if(outcome.rebindPlayer)EquipCityRangedFamily(outcome.selectedFamily);
            SyncAmmoHudEvent();
            return outcome;
        }
        void JunglePickupCollected(JungleDropSelection.Decision decision,JunglePickupInventory.Outcome outcome,float x,float y)
        {
            EnsureCombatAudio();
            if(outcome.itemAudio)NativeAudioSessionGate.Play(combatAudioSource,rewardClip);
            if(outcome.buttonAudio)NativeAudioSessionGate.Play(combatAudioSource,shopPurchaseClip);
            if(outcome.ammoNotification)jungleNotification.Show(decision.family,outcome.ammoAdded,new Vector2(x,y));
        }
        void TickJunglePickups(float dt)
        {
            jungleNotification.Tick(dt);
            for(int i=junglePickups.Count-1;i>=0;i--)
            {
                var actor=junglePickups[i];
                if(actor==null){junglePickups.RemoveAt(i);continue;}
                actor.Tick(dt,player.position.x,ref playerHP,ApplyJunglePickup,JunglePickupCollected);
            }
        }
        void DrawJungleStart()
        {
            var texture=Resources.Load<Texture2D>("DinoCapGenerated/JungleBlitz/LstartFrame");
            if(texture==null)throw new InvalidOperationException("Missing native Jungle START frame.");
            NativeFramebufferUi.DrawTexture(LogicalToScreenRect(levelIntro.Label.Rect,currentGameViewRect),texture);
        }
        void DrawJungleHud()
        {
            var texture=Resources.Load<Texture2D>("DinoCapGenerated/JungleBlitz/LkillFrame");
            if(texture==null)throw new InvalidOperationException("Missing native Jungle kill icon.");
            NativeFramebufferUi.DrawTexture(LogicalToScreenRect(NativeJungleHud.KillIcon,currentGameViewRect),texture);
            foreach(var digit in NativeJungleHud.KillDigits(jungleKills))
            {
                var sprite=LoadCityGrindSprite("icon_num"+digit.value+"Frame",.5f,.5f);
                if(sprite==null)throw new InvalidOperationException("Missing native Jungle kill digit.");
                NativeFramebufferUi.DrawTexture(LogicalToScreenRect(digit.rect,currentGameViewRect),sprite.texture);
            }
            DrawJungleTime();
        }
        void DrawJungleTime()
            =>DrawJungleNativeText(NativeJungleHud.TimeLabel,NativeJungleHud.TimeText(jungleSchedule.ElapsedSeconds));
        void DrawJungleNativeText(Rect nativeRect,string text)
        {
            if(jungleTimeMetrics==null)
            {
                var raw=Resources.Load<TextAsset>("Fonts/JungleTime/metrics");
                if(raw==null)throw new InvalidOperationException("Missing original Jungle time glyphs.");
                jungleTimeMetrics=JsonUtility.FromJson<NativeAmmoMetrics>(raw.text);
            }
            Rect rect=LogicalToScreenRect(nativeRect,currentGameViewRect);
            float scale=currentGameViewRect.height/DinoCapSourceProfileV11.ViewHeight;
            float cursor=0;var before=GUI.color;
            GUI.color=new Color(250f/255f,205f/255f,0,250f/255f*before.a);
            foreach(char c in text)
            {
                NativeAmmoGlyph glyph=null;
                foreach(var candidate in jungleTimeMetrics.glyphs)if(candidate.character.Length==1 && candidate.character[0]==c){glyph=candidate;break;}
                if(glyph==null || cursor+glyph.advance>nativeRect.width)break;
                if(glyph.texture==null && !string.IsNullOrEmpty(glyph.resource))glyph.texture=Resources.Load<Texture2D>(glyph.resource);
                if(glyph.texture!=null)NativeFramebufferUi.DrawTexture(new Rect(rect.x+(cursor+glyph.offsetX)*scale,
                    rect.y+(jungleTimeMetrics.baseline+glyph.offsetY)*scale,glyph.width*scale,glyph.height*scale),glyph.texture);
                cursor+=glyph.advance+2;
            }
            GUI.color=before;
        }
        System.Collections.IEnumerator RunJungleDeathResults()
        {
            //66550 sceneKind3 bypasses lives/save branch.5905C owns the
            //same pop/fade/rise;17650 cancels g_time after pop; Jungle
            //vtable+84 dispatches16A30, not56ACC City lives/replay.
            deathUIActive=true;
            deathPresentation=new NativeDeathPresentation(levelInitialFade,0,false);
            bool timeCancelled=false;
            while(deathPresentation.Active)
            {
                if(!paused)deathPresentation.Tick(Time.deltaTime);
                if(!timeCancelled && deathPresentation.CurrentStage!=NativeDeathPresentation.Stage.TextPop)
                {
                    jungleTimers.Cancel(jungleSecondTimer);timeCancelled=true;
                }
                yield return null;
            }
            jungleResultsVisible=true;jungleDoneRequested=false;
            jungleDoneTween=NativeJungleResults.CreateDone();
            //16A30 ->7FA5C ->1900D0 ->190254 submits shared cash to the
            //external SubmitScore:score: service (com.trinitigame.dinocap.l1).
            //Offline external services are intentionally inactive.
            while(!jungleDoneRequested)
            {
                if(jungleDoneTween.Active)jungleDoneTween.Tick(Time.deltaTime);
                yield return null;
            }
            Power2DSourceTruthRenderer.GlobalPaused=false;
            if(!debugTestLaunch)NativeModeSession.Publish(cashTotal,cityLives);
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenuScene");
        }
        void DrawJungleResults()
        {
            DrawJungleResultSprite("LtimeFrame",NativeJungleResults.TimeTitle);
            DrawJungleResultSprite("LoverkillFrame",NativeJungleResults.KillTitle);
            DrawJungleResultSprite("LmoneyFrame",NativeJungleResults.MoneyTitle);
            DrawJungleNativeText(NativeJungleResults.TimeValue,NativeJungleHud.TimeText(jungleSchedule.ElapsedSeconds));
            DrawJungleNativeText(NativeJungleResults.KillValue,jungleKills.ToString(System.Globalization.CultureInfo.InvariantCulture));
            DrawJungleNativeText(NativeJungleResults.MoneyValue,cashTotal.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if(jungleDoneTween!=null)
            {
                Rect rect=LogicalToScreenRect(jungleDoneTween.Rect,currentGameViewRect);
                if(GUI.Button(rect,"",GUIStyle.none))jungleDoneRequested=true;
                DrawJungleResultSprite("LdoneFrame",jungleDoneTween.Rect);
            }
        }
        void DrawJungleResultSprite(string frame,Rect rect)
        {
            var texture=Resources.Load<Texture2D>("DinoCapGenerated/JungleBlitz/"+frame);
            if(texture==null)throw new InvalidOperationException("Missing original Jungle result frame "+frame);
            NativeFramebufferUi.DrawTexture(LogicalToScreenRect(rect,currentGameViewRect),texture);
        }
        internal bool TestHarness_IsJungle=>jungle!=null;
        internal JungleWorldLayers TestHarness_JungleWorld=>jungleWorld;
        internal void TestHarness_JungleInitializeForFixture()
        {
            jungle=GetComponent<JungleBlitzController>();
            InitializeJungleSession();
        }
        internal int TestHarness_JungleCash=>cashTotal;
        internal int TestHarness_JungleLives=>cityLives;
        internal float TestHarness_GameplayWorldRight=>GameplayWorldRight;
        internal void TestHarness_SaveProgression()=>SaveProgressionState();
        internal int TestHarness_JungleSeconds=>jungleSchedule?.ElapsedSeconds ?? 0;
        internal void TestHarness_JungleTickTimers(float dt){TickJungleTimers(dt);AdmitJungleGameplay();}
        internal void TestHarness_JungleSpawn(int type)=>SpawnJungleDino(type);
        internal int TestHarness_JungleKills=>jungleKills;
        internal void TestHarness_JungleTickProjectiles(float dt)=>TickJungleProjectiles(dt);
        internal bool TestHarness_JungleProjectileHit()=>JungleProjectileHit();
        internal int TestHarness_JunglePickupCount=>junglePickups.Count;
        internal JunglePickupActor TestHarness_JunglePickup(int i)=>junglePickups[i];
        internal JungleAmmoNotification TestHarness_JungleNotification=>jungleNotification;
        internal bool TestHarness_JungleResultsVisible=>jungleResultsVisible;
        internal void TestHarness_JungleRequestDone()=>jungleDoneRequested=true;
        internal void TestHarness_JungleTickPickups(float dt)=>TickJunglePickups(dt);
        internal JunglePickupActor TestHarness_JungleCreatePickup(JungleDropSelection.Decision d,float x,float y)=>CreateJunglePickup(d,x,y);
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
        internal void TestHarness_JungleRawDraw(Func<int> draw)=>jungleRawOverride=draw;
#endif
    }
}
