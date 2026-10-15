#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    // The pause zone must pause on a touch BEGIN even while another finger already holds a control (Android: pause while moving).
    public class DC1PausePointerRepro
    {
        CityGridController city;
        SyntheticPointerLayer layer;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            city = Object.FindFirstObjectByType<CityGridController>();
            city.TestHarness_SkipLevelIntro();
            city.TestHarness_ClearDinos();
            layer = city.TestHarness_PointerLayer;
            yield return null; yield return null;   // controls are registered during Update
        }

        Vector2 Center(string id)
        {
            Rect r;
            Assert.IsTrue(layer.TryGetControlRect(id, out r), "control registered: " + id);
            return r.center;
        }

        [UnityTest]
        public IEnumerator SinglePointerInThePauseZonePauses()
        {
            int p = layer.InjectDown(Center("pause"), 0);
            yield return null; yield return null;
            Assert.IsTrue(city.TestHarness_Paused, "a touch beginning in the pause zone must pause");
            layer.InjectUp(p);
        }

        [UnityTest]
        public IEnumerator PauseTouchWhileAnotherFingerHoldsMoveStillPauses()
        {
            int move = layer.InjectDown(Center("moveRight"), 0);
            yield return null; yield return null;
            Assert.IsFalse(city.TestHarness_Paused, "precondition: moving, not paused");
            int pause = layer.InjectDown(Center("pause"), 1);
            yield return null; yield return null;
            Assert.IsTrue(city.TestHarness_Paused, "pause must work while another finger holds the move control");
            layer.InjectUp(pause); layer.InjectUp(move);
        }

        [UnityTest]
        public IEnumerator PauseZoneTouchWhilePausedDoesNotUnpause()
        {
            city.TestHarness_RequestPause();
            yield return null; yield return null;
            Assert.IsTrue(city.TestHarness_Paused);
            int p = layer.InjectDown(Center("pause"), 0);
            yield return null; yield return null;
            Assert.IsTrue(city.TestHarness_Paused, "the pause zone is not a resume control (the pause menu's Back button resumes)");
            layer.InjectUp(p);
        }
    }
}
#endif
