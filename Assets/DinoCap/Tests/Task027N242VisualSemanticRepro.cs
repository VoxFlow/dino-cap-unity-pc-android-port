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
    // TASK027N.2.42 - visual-semantic world-layer closure. Camera-only
    // capture only (see final_framebuffer_capture_n241.json - true final-
    // frame capture remains a proven blocker, not revisited this task per
    // Part M's own explicit instruction). All waits are bounded (plain
    // yield return null, never WaitForEndOfFrame).
    public class Task027N242VisualSemanticRepro
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
            city.TestHarness_ClearDinos();
            city.TestHarness_SetLevel(1);
            city.TestHarness_SetDifficulty(1);
            city.TestHarness_SetPlayerX(0f);

            captureDir = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "Dev", "Reports",
                "Task027N_2_42_VisualSemanticWorldLayerClosure", "Captures"));

            runId = System.DateTime.UtcNow.Ticks.ToString();
            yield return null;
        }

        // Part E: is renderer.bounds.minY a stable "feet" proxy across
        // different player poses (idle/walk/death)? If it varies a lot,
        // minY is NOT a safe feet-anchor and any offset-based math built on
        // it (N.2.41's own -245 measurement) is suspect.
        [UnityTest]
        public IEnumerator PartE_PlayerBoundsAcrossPoses()
        {
            var sb = new System.Text.StringBuilder();

            // idle (default state right after setup)
            yield return null;
            float idleMinY = city.TestHarness_PlayerRendererBoundsMinY;
            float idleMaxY = city.TestHarness_PlayerRendererBoundsMaxY;
            sb.AppendLine($"idle: minY={idleMinY} maxY={idleMaxY} transformY={city.TestHarness_PlayerY}");

            // walk (hold move-right for a couple of frames)
            city.TestHarness_RequestMoveRight();
            yield return null;
            city.TestHarness_RequestMoveRight();
            yield return null;
            float walkMinY = city.TestHarness_PlayerRendererBoundsMinY;
            float walkMaxY = city.TestHarness_PlayerRendererBoundsMaxY;
            sb.AppendLine($"walk: minY={walkMinY} maxY={walkMaxY} transformY={city.TestHarness_PlayerY}");

            File.WriteAllText(Path.Combine(captureDir, "E_bounds_by_pose__" + runId + ".txt"), sb.ToString());
            Debug.Log("[N242 Part E] " + sb.ToString());
            Assert.Pass(sb.ToString());
        }

        // TASK027N.2.43 Part A: the N.2.42 candidate-Y calibration probe
        // (CalibrationProbe_CandidateActorY, which tried {75,82,90} and
        // selected 82f "because it looked right") has been REMOVED per the
        // newly-merged No-calibration-as-source-truth rule. Trying
        // candidate constants until one "looks right" is now explicitly
        // forbidden as a route to a production value - see
        // n242_calibration_reversal_manifest_n243.json. The corresponding
        // production-facing TestHarness_SetPlayerYForCalibration accessor
        // was also removed from CityGridController.cs (it had no other
        // caller). MP4 comparison may still be used to FALSIFY a result or
        // VALIDATE an already source-derived one - see
        // Task027N243SourceTruthRepro.cs for this task's own replacement
        // evidence-gathering tests.
    }
}
#endif
