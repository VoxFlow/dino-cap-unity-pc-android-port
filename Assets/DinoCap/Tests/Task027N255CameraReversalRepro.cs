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
    public sealed class Task027N255CameraReversalRepro
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        DC1MenuPreferenceScope preferences;
        CityGridController core;
        Transform player;
        Power2DSourceTruthRenderer visual;
        FieldInfo cameraLeft;

        [UnitySetUp] public IEnumerator SetUp()
        {
            preferences=new DC1MenuPreferenceScope();
            yield return null;
        }
        [TearDown] public void TearDown()
        {
            core?.TestHarness_ResetCameraAspect();
            preferences?.Dispose();
            NativeAudioSessionGate.StopAll();
        }
        IEnumerator Load(bool jungle,float aspect)
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/"+
                (jungle?"JungleBlitzScene":"CityGrindScene")+".unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            core=Object.FindFirstObjectByType<CityGridController>();
            core.TestHarness_SkipLevelIntro();core.TestHarness_ClearDinos();core.enabled=false;
            core.TestHarness_SetDebugMovementSpeed(1f);core.TestHarness_SetCameraAspect(aspect);
            player=(Transform)typeof(CityGridController).GetField("player",Private).GetValue(core);
            visual=(Power2DSourceTruthRenderer)typeof(CityGridController).GetField("playerVisual",Private).GetValue(core);
            cameraLeft=typeof(CityGridController).GetField("cameraLeftX",Private);
        }
        float ScreenFraction=>(player.position.x-(float)cameraLeft.GetValue(core))/core.TestHarness_CurrentViewWidth;
        void Settle(bool facingRight)
        {
            player.position=new Vector3(3500,player.position.y,0);visual.facingRight=facingRight;
            typeof(CityGridController).GetMethod("InitializeCamera",Private).Invoke(core,null);
            core.TestHarness_UpdateCamera(1f/60f);
        }
        void Walk(bool right,int frames)
        {
            visual.facingRight=right;
            for(int i=0;i<frames;i++)
            {
                player.position+=new Vector3((right?1:-1)*205f/60f,0,0);
                core.TestHarness_UpdateCamera(1f/60f);
            }
        }
        [UnityTest] public IEnumerator SustainedReversalsCatchBothDirectionsInCityAndJungle()
        {
            foreach(bool jungle in new[]{false,true})
            foreach(float aspect in new[]{4f/3f,3f/2f,16f/10f,16f/9f,21f/9f,32f/9f,48f/9f})
            {
                yield return Load(jungle,aspect);
                // Start from the opposite settled offset; hold movement long
                // enough for native315/205 to close a full reversal.
                Settle(false);Walk(true,60);
                Assert.That(ScreenFraction,Is.EqualTo(.45f).Within(.001f),$"Right hold: jungle={jungle}, aspect={aspect}");
                Walk(false,60);
                Assert.That(ScreenFraction,Is.EqualTo(.55f).Within(.001f),$"Left hold: jungle={jungle}, aspect={aspect}");
                Walk(true,60);
                Assert.That(ScreenFraction,Is.EqualTo(.45f).Within(.001f),"Second held reversal must also settle without stopping.");
            }
        }
        [UnityTest] public IEnumerator NativeFourByThreeMatchesRegisteredIpadPanAndMeasuredClosingTime()
        {
            yield return Load(false,4f/3f);
            Settle(false);Walk(true,30);
            Assert.That(ScreenFraction,Is.EqualTo(.55f-110f*.5f/1024f).Within(.001f),
                "Native315 camera minus205 walking closes110 units per second.");
            Walk(true,30);
            Assert.That(ScreenFraction,Is.EqualTo(.45f).Within(.001f),"A102.4-unit reversal must settle in about0.93 seconds while walking.");
        }
    }
}
#endif
