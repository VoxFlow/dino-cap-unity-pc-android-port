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
    // TASK027N.2.41 Parts C/D/J. See report.md for full context.
    //
    // SAFETY NOTE: an earlier version of this file drove
    // TestHarness_CaptureFinalFrame via `yield return new
    // WaitForEndOfFrame()`, which hung the whole Unity batchmode process for
    // over 1h30m (WaitForEndOfFrame never resumes without a real
    // present/vsync cycle - a documented Unity Test Framework batchmode
    // incompatibility). That method was rewritten to use only bounded-safe
    // primitives (fixed frame-count + wall-clock timeout, no
    // WaitForEndOfFrame) and empirically re-verified NOT to hang - but
    // ScreenCapture.CaptureScreenshotAsTexture() itself still fails fast
    // with "Was method called before the 'end of frame' state was reached?"
    // in this exact environment, confirming a TRUE OnGUI-inclusive capture
    // is structurally blocked here (see final_framebuffer_capture_n241.json
    // for the full evidence chain). This file therefore uses ONLY the safe
    // camera-only capture (TestHarness_CaptureScreenshot) plus direct
    // renderer-bounds measurement for Part D, rather than depending on the
    // blocked final-frame path.
    public class Task027N241FinalFrameRepro
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
                "Task027N_2_41_L1Survivability_FinalFramebufferCaptureClosure", "Captures"));

            runId = System.DateTime.UtcNow.Ticks.ToString();

            yield return null;
        }

        // Part D root cause: direct, empirical, synchronous measurement of
        // the player's own real Unity-computed renderer bounds against the
        // camera's own real orthographic Y range - proves/disproves a
        // genuine runtime placement defect independent of any capture
        // mechanism (no WaitForEndOfFrame, no ScreenCapture - just reads
        // already-live Unity state, so this cannot hang).
        [UnityTest]
        public IEnumerator DiagnosticD_PlayerBoundsVsCameraRange()
        {
            yield return null;

            float camY = city.TestHarness_CameraY;
            float camHalf = city.TestHarness_CameraOrthographicSize;
            float visibleBottom = camY - camHalf;
            float visibleTop = camY + camHalf;
            float playerMinY = city.TestHarness_PlayerRendererBoundsMinY;
            float playerMaxY = city.TestHarness_PlayerRendererBoundsMaxY;

            string diag =
                "camera.Y=" + camY + " camera.orthographicSize=" + camHalf +
                " visibleY_range=[" + visibleBottom + "," + visibleTop + "]" +
                " player.position.y=" + city.TestHarness_PlayerY +
                " player.renderer.bounds.minY=" + playerMinY +
                " player.renderer.bounds.maxY=" + playerMaxY +
                " margin_below_visibleBottom=" + (playerMinY - visibleBottom) +
                " sprite_extends_below_visible_area=" + (playerMinY < visibleBottom);

            // Also measure a dino's own bounds (same Power2DSourceTruthRenderer
            // component as the player) - tests whether the pivot-offset issue
            // is shared (a rendering-system-wide bug) or player-specific
            // (per-rig pivot data differs).
            city.TestHarness_RequestMoveRight();
            city.TestHarness_ForceSpawn(0);
            yield return null;
            string dinoDiag = "";
            if (city.TestHarness_ActiveDinoCount > 0)
            {
                float dinoY = city.TestHarness_DinoY(0);
                float dinoMinY = city.TestHarness_DinoRendererBoundsMinY(0);
                float dinoMaxY = city.TestHarness_DinoRendererBoundsMaxY(0);
                dinoDiag = " | dino.position.y=" + dinoY +
                    " dino.renderer.bounds.minY=" + dinoMinY +
                    " dino.renderer.bounds.maxY=" + dinoMaxY +
                    " dino_offset_min=" + (dinoMinY - dinoY) +
                    " dino_extends_below_visible_area=" + (dinoMinY < visibleBottom);
            }
            diag += dinoDiag;

            Debug.Log("[N241 Part D diagnostic] " + diag);
            File.WriteAllText(Path.Combine(captureDir, "D_bounds_diagnostic__" + runId + ".txt"), diag);

            // camera-only capture too, for a visual cross-check alongside the numbers
            string camPath = city.TestHarness_CaptureScreenshot("D_bounds_check", captureDir, runId);
            Assert.IsTrue(File.Exists(camPath), "Camera-only capture must exist.");

            Assert.Pass(diag);
        }

        // Part J: L1 stationary-survival timeline, using ONLY the safe
        // camera-only capture (world-space content; HUD/on-screen controls
        // are NOT included - see render_capture_layer_audit_n241.json /
        // final_framebuffer_capture_n241.json for why that layer remains
        // uncapturable in this batchmode environment). Player never
        // moves/attacks; dinos spawn/approach/bite via the normal per-frame
        // Update loop (not forced), matching Part H's own model scenario.
        [UnityTest]
        public IEnumerator TimelineJ_L1StationarySurvival()
        {
            float horizon = 60f;
            float start = Time.time;
            int lastVisible = -1;
            bool firstBiteCaptured = false;
            bool beforeDeathCaptured = false;
            bool deathCaptured = false;
            bool afterRetryCaptured = false;

            city.TestHarness_CaptureScreenshot("J_00_scene_start", captureDir, runId);
            yield return null;

            while (Time.time - start < horizon)
            {
                int visible = city.TestHarness_ActiveDinoCount;
                if (visible >= 1 && lastVisible < 1)
                    city.TestHarness_CaptureScreenshot("J_01_first_dino_arrival", captureDir, runId);
                if (visible >= 2 && lastVisible < 2)
                    city.TestHarness_CaptureScreenshot("J_02_second_dino_arrival", captureDir, runId);
                lastVisible = visible;

                if (!firstBiteCaptured && city.TestHarness_PlayerHP < 100f)
                {
                    city.TestHarness_CaptureScreenshot("J_03_first_bite", captureDir, runId);
                    firstBiteCaptured = true;
                }
                if (!beforeDeathCaptured && firstBiteCaptured && city.TestHarness_PlayerHP <= 10f && city.TestHarness_PlayerHP > 0f)
                {
                    city.TestHarness_CaptureScreenshot("J_04_before_death", captureDir, runId);
                    beforeDeathCaptured = true;
                }
                if (!deathCaptured && city.TestHarness_PlayerHP <= 0f)
                {
                    city.TestHarness_CaptureScreenshot("J_05_death", captureDir, runId);
                    deathCaptured = true;
                }
                if (deathCaptured && !afterRetryCaptured && city.TestHarness_PlayerHP >= 100f)
                {
                    city.TestHarness_CaptureScreenshot("J_06_after_retry", captureDir, runId);
                    afterRetryCaptured = true;
                    break;
                }

                yield return null;
            }

            if (!deathCaptured)
                city.TestHarness_CaptureScreenshot("J_fallback_no_death_within_horizon", captureDir, runId);

            Assert.Pass("Timeline capture complete. firstBite=" + firstBiteCaptured + " death=" + deathCaptured + " retry=" + afterRetryCaptured);
        }
    }
}
#endif
