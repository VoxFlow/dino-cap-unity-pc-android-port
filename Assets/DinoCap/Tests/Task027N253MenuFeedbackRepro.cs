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
    public class Task027N253MenuFeedbackRepro
    {
        MainMenuController menu;
        DC1MenuPreferenceScope preferences;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            preferences=new DC1MenuPreferenceScope();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            menu=Object.FindFirstObjectByType<MainMenuController>();
            menu.enabled=false;
        }
        [TearDown]
        public void TearDown(){preferences?.Dispose();preferences=null;}

        [TestCase(0,false,0)]
        [TestCase(1,false,4)]
        [TestCase(1,true,0)]
        public void NativeMenuGaitAudioFollowsFlaggedFramesAcrossTwoLoops(int variant,bool mute,int expected)
        {
            var random=Random.state;
            try {
                for(int seed=0;seed<100;seed++) {
                    Random.InitState(seed);
                    if((Random.value*100f>=50f?1:0)!=variant)continue;
                    Random.InitState(seed);
                    typeof(MainMenuController).GetMethod("InitBackgroundDino",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(menu,null);
                    break;
                }
                Assert.AreEqual(variant,menu.TestHarness_DinoVariant);
                var track=menu.TestHarness_DinoVisual.primary;
                Assert.AreEqual(variant==1?"k2_move":"k3_move",track.Clip.name);
                if(menu.TestHarness_SfxOn==mute)menu.TestHarness_PressSfxToggle();
                NativeAudioSessionGate.StopAll();
                NativeAudioSessionGate.TestHarness_ResetClipCounters();
                for(int i=0;i<track.Clip.frames.Length*2;i++)track.Tick(0.025f);
                Assert.AreEqual(expected,NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Dino"),
                    "5D00C flags only k2_move40/80;59AB0 plays Audio_DinoAudio through the shared FX gate. No timer, roar or unflagged clip can substitute.");
            } finally {Random.state=random;}
        }

        [Test]
        public void MenuGaitDoesNotPlayBetweenNativeMarks()
        {
            var random=Random.state;
            try {
                for(int seed=0;seed<100;seed++) {
                    Random.InitState(seed);
                    if(Random.value*100f<50f)continue;
                    Random.InitState(seed);
                    typeof(MainMenuController).GetMethod("InitBackgroundDino",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(menu,null);
                    break;
                }
                NativeAudioSessionGate.StopAll();NativeAudioSessionGate.TestHarness_ResetClipCounters();
                var track=menu.TestHarness_DinoVisual.primary;
                for(int i=0;i<39;i++)track.Tick(0.025f);
                Assert.AreEqual(0,NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Dino"));
                track.Tick(0.025f);
                Assert.AreEqual(1,NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Dino"));
                for(int i=0;i<39;i++)track.Tick(0.025f);
                Assert.AreEqual(1,NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Dino"));
                track.Tick(0.025f);
                Assert.AreEqual(2,NativeAudioSessionGate.TestHarness_AdmittedCount("Audio_Dino"));
            } finally {Random.state=random;}
        }
    }
}
#endif
