#if UNITY_EDITOR || DEVELOPMENT_BUILD   // compiled out of shipped builds
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DinoCap.PortV2
{
    // DEBUG_ONLY: measures frame-time jitter and per-frame scroll steps of the gameplay camera so editor-vs-build judder can be
    // compared with numbers instead of eyesight. F10 starts a 12 s recording (press again to stop); the summary goes to the log and a
    // CSV is written next to the save file. No gameplay, rendering or timing behaviour is touched.
    public sealed class DC1FrameStepRecorder : MonoBehaviour
    {
        const float Seconds = 12f;
        bool recording;
        float startRealtime;
        readonly List<float> dts = new List<float>(2048);
        readonly List<float> udts = new List<float>(2048);
        readonly List<float> camX = new List<float>(2048);
        readonly List<float> playerX = new List<float>(2048);
        Camera worldCamera;
        CityGridController city;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Application.isBatchMode) return;
            var go = new GameObject("DC1FrameStepRecorder");
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
            go.AddComponent<DC1FrameStepRecorder>();
        }

        void LateUpdate()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if(kb != null && kb.f10Key.wasPressedThisFrame)
            {
                if(recording) Finish(); else Begin();
                return;
            }
#endif
            if(!recording) return;
            if(worldCamera == null)
            {
                foreach(var c in Camera.allCameras) if(c.targetTexture != null) { worldCamera = c; break; }
                city = Object.FindFirstObjectByType<CityGridController>();
            }
            dts.Add(Time.deltaTime);
            udts.Add(Time.unscaledDeltaTime);
            camX.Add(worldCamera != null ? worldCamera.transform.position.x : float.NaN);
            playerX.Add(city != null ? city.TestHarness_PlayerX : float.NaN);
            if(Time.realtimeSinceStartup - startRealtime >= Seconds) Finish();
        }

        void Begin()
        {
            recording = true; startRealtime = Time.realtimeSinceStartup;
            dts.Clear(); udts.Clear(); camX.Clear(); playerX.Clear(); worldCamera = null;
            Debug.Log("[DC1-FRAMESTEP] recording " + Seconds + " s: walk back and forth around a building");
        }

        static float Median(List<float> v)
        {
            var c = new List<float>(v); c.Sort(); return c.Count == 0 ? 0f : c[c.Count / 2];
        }

        static void Stats(List<float> v, out float mean, out float sd, out float min, out float max)
        {
            mean = 0f; min = float.MaxValue; max = float.MinValue;
            foreach(float x in v) { mean += x; if(x < min) min = x; if(x > max) max = x; }
            mean = v.Count > 0 ? mean / v.Count : 0f;
            float s = 0f; foreach(float x in v) s += (x - mean) * (x - mean);
            sd = v.Count > 1 ? Mathf.Sqrt(s / (v.Count - 1)) : 0f;
        }

        void Finish()
        {
            recording = false;
            int n = dts.Count;
            if(n < 10) { Debug.Log("[DC1-FRAMESTEP] too few frames"); return; }
            var steps = new List<float>(n); var stepsPerSecond = new List<float>(n);
            for(int i = 1; i < n; i++)
            {
                float dx = Mathf.Abs(camX[i] - camX[i - 1]);
                steps.Add(dx);
                stepsPerSecond.Add(dts[i] > 1e-6f ? dx / dts[i] : 0f);
            }
            float dm, dsd, dmin, dmax; Stats(dts, out dm, out dsd, out dmin, out dmax);
            float med = Median(dts); int off20 = 0; foreach(float d in dts) if(Mathf.Abs(d - med) > 0.2f * med) off20++;
            // Moving frames only: scroll uniformity is meaningful while the camera actually moves.
            var movingSteps = new List<float>(); var movingVel = new List<float>();
            for(int i = 0; i < steps.Count; i++) if(steps[i] > 0.01f) { movingSteps.Add(steps[i]); movingVel.Add(stepsPerSecond[i]); }
            float sm = 0, ssd = 0, smin = 0, smax = 0, vm = 0, vsd = 0, vmin = 0, vmax = 0;
            if(movingSteps.Count > 5) { Stats(movingSteps, out sm, out ssd, out smin, out smax); Stats(movingVel, out vm, out vsd, out vmin, out vmax); }
            int zeroSteps = 0; for(int i = 1; i < n; i++) if(Mathf.Abs(camX[i] - camX[i - 1]) < 0.01f && Mathf.Abs(playerX[i] - playerX[i - 1]) > 0.01f) zeroSteps++;
            var sb = new StringBuilder();
            sb.AppendLine("frame,deltaTime,unscaledDeltaTime,cameraX,playerX");
            for(int i = 0; i < n; i++) sb.AppendLine(i + "," + dts[i].ToString("F6") + "," + udts[i].ToString("F6") + "," + camX[i].ToString("F4") + "," + playerX[i].ToString("F4"));
            string path = Path.Combine(Application.persistentDataPath, "framestep_" + (Application.isEditor ? "editor" : "player") + ".csv");
            try { File.WriteAllText(path, sb.ToString()); } catch { path = "(csv write failed)"; }
            Debug.Log("[DC1-FRAMESTEP] " + (Application.isEditor ? "EDITOR" : "PLAYER") + " frames=" + n +
                " dt mean=" + (dm * 1000f).ToString("F2") + "ms sd=" + (dsd * 1000f).ToString("F2") + "ms min=" + (dmin * 1000f).ToString("F2") + " max=" + (dmax * 1000f).ToString("F2") +
                " | frames off median by >20%: " + off20 + " (" + (100f * off20 / n).ToString("F1") + "%)" +
                " | moving camera steps(units/frame) n=" + movingSteps.Count + " mean=" + sm.ToString("F3") + " sd=" + ssd.ToString("F3") + " min=" + smin.ToString("F3") + " max=" + smax.ToString("F3") +
                " | velocity(units/s) mean=" + vm.ToString("F1") + " sd=" + vsd.ToString("F2") +
                " | frames where the player moved but the camera did not: " + zeroSteps +
                " | vSync=" + QualitySettings.vSyncCount + " targetFps=" + Application.targetFrameRate + " csv=" + path);
        }
    }
}
#endif
