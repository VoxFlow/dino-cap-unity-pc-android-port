#if UNITY_EDITOR
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    // TASK027N.2.40 Primary Goals 4/5 - minimal render-capture harness proof.
    // Captures actual rendered PNGs at named checkpoints so Claude (or a
    // human) can inspect real pixels instead of only state assertions -
    // closing the gap N.2.39's own false-confidence audit identified
    // (headless state tests cannot prove visual fidelity). No pixel-diff
    // system is built here, per this task's own explicit scope limit -
    // capture and one-time visual inspection only.
    public class Task027N240RenderCaptureRepro
    {
        const string CityScenePath = "Assets/DinoCap/Scenes/CityGrindScene.unity";

        CityGridController city;
        string captureDir;
        string runId;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            NativeAudioSessionGate.StopAll();
            EditorSceneManager.LoadSceneInPlayMode(
                CityScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            yield return null;
            city = Object.FindFirstObjectByType<CityGridController>();
            Assert.IsNotNull(city, "CityGridController not found after loading " + CityScenePath);
            city.TestHarness_SkipLevelIntro(); // Scene captures exercise established gameplay, not intro.
            city.TestHarness_ClearDinos();
            city.TestHarness_SetLevel(1);
            city.TestHarness_SetDifficulty(1);
            city.TestHarness_SetPlayerX(0f);

            captureDir = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "Dev", "Reports",
                "Task027N_2_40_SpawnLifecycle_RenderCaptureClosure", "Captures"));

            // TASK027N.2.41 Part A fix: a unique per-test-run ID is now
            // REQUIRED in every capture filename (this test file's own
            // 2-runs-in-1-task history silently overwrote its own fixed
            // filenames and produced a stale-vs-actual mismatch - see
            // n240_capture_identity_audit_n241.json). System.DateTime
            // ticks gives a distinct ID per test invocation, not just per
            // test-run process.
            runId = System.DateTime.UtcNow.Ticks.ToString();

            yield return null;
        }

        IEnumerator WaitSeconds(float seconds)
        {
            float start = Time.time;
            while (Time.time - start < seconds) yield return null;
        }

        [UnityTest]
        public IEnumerator Capture01_SceneStart()
        {
            yield return null;
            string path = city.TestHarness_CaptureScreenshot("01_scene_start", captureDir, runId);
            Assert.IsNotNull(path, "Capture must produce a file path.");
            Assert.IsTrue(File.Exists(path), "Captured PNG must exist on disk at " + path);
            Assert.Greater(new FileInfo(path).Length, 0, "Captured PNG must be non-empty.");
        }

        // TASK027N.2.49R Part O - proves the new runtime-truth snapshot
        // tool (DEBUG/TEST only) actually produces a real PNG + JSON pair
        // with genuine, non-placeholder gameplay state in it.
        [UnityTest]
        public IEnumerator CaptureTruthSnapshot_Basic()
        {
            city.TestHarness_ForceSpawn(0);
            yield return null;
            string jsonPath = city.TestHarness_CaptureRuntimeTruthSnapshot("truth_snapshot_basic", captureDir, runId);
            Assert.IsNotNull(jsonPath, "Snapshot must produce a JSON file path.");
            Assert.IsTrue(File.Exists(jsonPath), "Snapshot JSON must exist on disk at " + jsonPath);
            string contents = File.ReadAllText(jsonPath);
            Assert.IsTrue(contents.Contains("\"dinos\":[{"), "Snapshot must include the just-spawned dino's own state.");
            Assert.IsTrue(contents.Contains("\"player\":{"), "Snapshot must include player state.");
            Assert.IsTrue(contents.Contains("pngPath"), "Snapshot must reference its own PNG capture.");
        }

        [UnityTest]
        public IEnumerator Capture02_After3sStationary()
        {
            yield return WaitSeconds(3f);
            string path = city.TestHarness_CaptureScreenshot("02_after_3s_stationary", captureDir, runId);
            Assert.IsTrue(File.Exists(path));
        }

        [UnityTest]
        public IEnumerator Capture03_AfterFirstWalkIn()
        {
            // Force a walk-in specifically (entryMode==WalkIn) - retry a
            // bounded number of times since jump-vs-walk is an RNG roll
            // at spawn (see CityGridController.SpawnCityDino).
            bool gotWalkIn = false;
            for (int attempt = 0; attempt < 30 && !gotWalkIn; attempt++)
            {
                city.TestHarness_ClearDinos();
                city.TestHarness_RequestMoveRight();
                city.TestHarness_ForceSpawn(0);
                yield return null;
                if (city.TestHarness_ActiveDinoCount > 0 && !city.TestHarness_DinoIsJumpIn(0))
                    gotWalkIn = true;
            }
            Assert.IsTrue(gotWalkIn, "Could not obtain a walk-in spawn within 30 attempts.");
            yield return WaitSeconds(0.5f);   // let a few walk frames render
            string path = city.TestHarness_CaptureScreenshot("03_after_first_walk_in", captureDir, runId);
            Assert.IsTrue(File.Exists(path));
        }

        [UnityTest]
        public IEnumerator Capture04_AfterFirstJumpIn()
        {
            bool gotJumpIn = false;
            for (int attempt = 0; attempt < 60 && !gotJumpIn; attempt++)
            {
                city.TestHarness_ClearDinos();
                city.TestHarness_RequestMoveRight();
                city.TestHarness_ForceSpawn(0);
                yield return null;
                if (city.TestHarness_ActiveDinoCount > 0 && city.TestHarness_DinoIsJumpIn(0))
                    gotJumpIn = true;
            }
            Assert.IsTrue(gotJumpIn, "Could not obtain a jump-in spawn within 60 attempts.");
            // capture mid-jump (still entering) for the requested checkpoint
            string path = city.TestHarness_CaptureScreenshot("04_after_first_jump_in", captureDir, runId);
            Assert.IsTrue(File.Exists(path));
        }

        [UnityTest]
        public IEnumerator Capture05_BuildingEntry()
        {
            city.TestHarness_SetPlayerNear(0);
            city.TestHarness_PressDoor();
            yield return WaitSeconds(0.3f);
            string path = city.TestHarness_CaptureScreenshot("05_building_entry", captureDir, runId);
            Assert.IsTrue(File.Exists(path));
        }

        [UnityTest]
        public IEnumerator Capture06_RewardPopup()
        {
            city.TestHarness_SetPlayerNear(0);
            city.TestHarness_PressDoor();
            yield return WaitSeconds(0.3f);
            city.TestHarness_ForceLootType(0, 6);
            yield return null;
            Assert.IsTrue(city.TestHarness_HasLastPopup, "Loot popup must exist before capturing this checkpoint.");
            string path = city.TestHarness_CaptureScreenshot("06_reward_popup", captureDir, runId);
            Assert.IsTrue(File.Exists(path));
        }
    }
}
#endif
