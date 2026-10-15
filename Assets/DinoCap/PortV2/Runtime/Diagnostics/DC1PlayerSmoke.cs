#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoCap.PortV2
{
    // Development-player smoke test only; inert without -dc1-smoke.
    // Loads every build scene, dwells, and records platform identity plus any
    // Error/Exception/Assert log. Proves "builds, launches, scenes load and run
    // without logged errors" on the actual player (Windows Mono / Android IL2CPP).
    // It does NOT prove gameplay correctness or visual fidelity.
    // Args: -dc1-smoke [-dc1-smoke-out <file>] [-dc1-smoke-dwell <seconds>]
    // Android: adb shell am start -n <pkg>/<activity> -e unity "-dc1-smoke"
    public sealed class DC1PlayerSmoke : MonoBehaviour
    {
        const float SceneTimeoutSeconds = 30f;
        const float TotalTimeoutSeconds = 240f;
        const int MinDwellFrames = 120;

        sealed class SceneResult
        {
            public string scene; public bool loaded; public int frames; public float seconds;
            public int errors; public int spriteRenderers; public int cameras;
            public float avgFps, p50Ms, p95Ms, p99Ms, maxMs; public int over33Ms;
            public int gcCollections; public float allocKbPerFrame;
            public float cpuMainMs = -1f, cpuRenderMs = -1f, gpuMs = -1f;
            public int rtWidth, rtHeight;
            public string colorProbe;   // optional: rendered-frame RGB samples (-dc1-smoke-colorprobe), MainMenuScene only
        }

        static bool started;
        readonly List<SceneResult> results = new List<SceneResult>();
        readonly List<string> problems = new List<string>();
        string outputPath;
        float dwellSeconds = 4f;
        bool colorProbe;
        // Measurement-only experiment switches (dev harness; never touches production code paths):
        // -dc1-smoke-scale 1|2 (internal framebuffer), -dc1-smoke-vsync 0|1, -dc1-smoke-fps N
        int expScale = 0, expVsync = -1, expFps = 0;
        bool restoreScale; bool originalScale2x;
        int currentErrors;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartIfRequested()
        {
            if (started) return;
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-dc1-smoke") < 0) return;
            started = true;
            var go = new GameObject("DC1 Player Smoke");
            DontDestroyOnLoad(go);
            var smoke = go.AddComponent<DC1PlayerSmoke>();
            int o = Array.IndexOf(args, "-dc1-smoke-out");
            smoke.outputPath = (o >= 0 && o + 1 < args.Length)
                ? Path.GetFullPath(args[o + 1])
                : Path.Combine(Application.persistentDataPath, "dc1_smoke_result.json");
            int d = Array.IndexOf(args, "-dc1-smoke-dwell");
            float parsed;
            if (d >= 0 && d + 1 < args.Length && float.TryParse(args[d + 1],
                System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsed))
                smoke.dwellSeconds = Mathf.Max(0.5f, parsed);
            smoke.expScale = IntArg(args, "-dc1-smoke-scale", 0);
            smoke.expVsync = IntArg(args, "-dc1-smoke-vsync", -1);
            smoke.expFps = IntArg(args, "-dc1-smoke-fps", 0);
            smoke.colorProbe = Array.IndexOf(args, "-dc1-smoke-colorprobe") >= 0;
            GameplayRenderTarget.DisplayCapOverride = IntArg(args, "-dc1-smoke-displaycap", -1);
            smoke.StartCoroutine(smoke.Run());
        }

        static int IntArg(string[] args, string name, int fallback)
        {
            int i = Array.IndexOf(args, name);
            int v;
            return (i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], out v)) ? v : fallback;
        }

        void OnEnable() { Application.logMessageReceived += OnLog; }
        void OnDisable() { Application.logMessageReceived -= OnLog; }

        void OnLog(string condition, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            currentErrors++;
            if (problems.Count < 40)
                problems.Add(type + ": " + (condition.Length > 300 ? condition.Substring(0, 300) : condition));
        }

        IEnumerator Run()
        {
            float startRealtime = Time.realtimeSinceStartup;
            Debug.Log("DC1_SMOKE start platform=" + Application.platform + " scenes=" + SceneManager.sceneCountInBuildSettings);
            if (expScale == 1 || expScale == 2)
            {
                originalScale2x = GameplayRenderTarget.Supersample2xEnabled;
                if (originalScale2x != (expScale == 2)) { GameplayRenderTarget.TogglePersistentSupersample(); restoreScale = true; }
            }
            if (expVsync >= 0) QualitySettings.vSyncCount = expVsync;
            if (expFps > 0) Application.targetFrameRate = expFps;
            int count = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < count; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                var r = new SceneResult { scene = Path.GetFileNameWithoutExtension(path) };
                currentErrors = 0;
                float t0 = Time.realtimeSinceStartup;
                var op = SceneManager.LoadSceneAsync(i, LoadSceneMode.Single);
                if (op == null)
                {
                    problems.Add("LoadSceneAsync returned null for " + r.scene);
                    results.Add(r);
                    continue;
                }
                while (!op.isDone && Time.realtimeSinceStartup - t0 < SceneTimeoutSeconds) yield return null;
                r.loaded = op.isDone;
                if (!r.loaded) problems.Add("scene load timed out: " + r.scene);
                else
                {
                    int frames = 0;
                    var dts = new List<float>(1024);
                    int gc0 = GC.CollectionCount(0);
                    long alloc0 = GC.GetAllocatedBytesForCurrentThread();
                    double cpuMain = 0, cpuRender = 0, gpu = 0; int ftN = 0;
                    var ft = new FrameTiming[1];
                    float d0 = Time.realtimeSinceStartup;
                    while ((frames < MinDwellFrames || Time.realtimeSinceStartup - d0 < dwellSeconds)
                           && Time.realtimeSinceStartup - d0 < SceneTimeoutSeconds)
                    {
                        frames++;
                        if (frames > 5) dts.Add(Time.unscaledDeltaTime * 1000f);
                        FrameTimingManager.CaptureFrameTimings();
                        if (FrameTimingManager.GetLatestTimings(1, ft) > 0 && ft[0].cpuFrameTime > 0)
                        { cpuMain += ft[0].cpuMainThreadFrameTime; cpuRender += ft[0].cpuRenderThreadFrameTime; gpu += ft[0].gpuFrameTime; ftN++; }
                        yield return null;
                    }
                    if (colorProbe && r.scene == "MainMenuScene") yield return CaptureColorProbe(r);
                    r.frames = frames;
                    r.gcCollections = GC.CollectionCount(0) - gc0;
                    r.allocKbPerFrame = (GC.GetAllocatedBytesForCurrentThread() - alloc0) / 1024f / Mathf.Max(1, frames);
                    if (ftN > 0) { r.cpuMainMs = (float)(cpuMain / ftN); r.cpuRenderMs = (float)(cpuRender / ftN); r.gpuMs = (float)(gpu / ftN); }
                    if (dts.Count > 0)
                    {
                        dts.Sort();
                        float sum = 0; foreach (var d in dts) { sum += d; if (d > 33.4f) r.over33Ms++; }
                        r.avgFps = 1000f / (sum / dts.Count);
                        r.p50Ms = dts[dts.Count / 2]; r.p95Ms = dts[(int)(dts.Count * 0.95f)];
                        r.p99Ms = dts[Mathf.Min(dts.Count - 1, (int)(dts.Count * 0.99f))]; r.maxMs = dts[dts.Count - 1];
                    }
                    var grt = FindFirstObjectByType<GameplayRenderTarget>();
                    if (grt != null && grt.Target != null) { r.rtWidth = grt.Target.width; r.rtHeight = grt.Target.height; }
                    r.spriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length;
                    r.cameras = Camera.allCamerasCount;
                    if (frames < MinDwellFrames) problems.Add("too few frames in " + r.scene + ": " + frames);
                    if (r.cameras < 1) problems.Add("no active camera in " + r.scene);
                }
                r.seconds = Time.realtimeSinceStartup - t0;
                r.errors = currentErrors;
                results.Add(r);
                Debug.Log("DC1_SMOKE scene=" + r.scene + " loaded=" + r.loaded + " frames=" + r.frames +
                          " errors=" + r.errors + " sprites=" + r.spriteRenderers + " seconds=" + r.seconds.ToString("0.0"));
                if (Time.realtimeSinceStartup - startRealtime > TotalTimeoutSeconds)
                {
                    problems.Add("total smoke timeout");
                    break;
                }
            }
            Finish();
        }

        // Dev-only: reads back the real rendered frame (what the player actually presents) and samples RGB at fixed
        // normalised positions on the menu background away from UI sprites. The read-back is bounded by a hard 3 s timeout
        // and fails loudly into `problems` (project rule: no unbounded end-of-frame waits).
        IEnumerator CaptureColorProbe(SceneResult r)
        {
            Texture2D tex = null; bool done = false;
            StartCoroutine(CaptureAtEndOfFrame(t => { tex = t; done = true; }));
            float t0 = Time.realtimeSinceStartup;
            while (!done && Time.realtimeSinceStartup - t0 < 3f) yield return null;
            if (tex == null) { problems.Add("colour probe: screen read-back timed out"); yield break; }
            float[][] pts = { new[] { 0.1f, 0.05f }, new[] { 0.5f, 0.05f }, new[] { 0.9f, 0.05f }, new[] { 0.1f, 0.30f }, new[] { 0.9f, 0.32f }, new[] { 0.5f, 0.46f }, new[] { 0.1f, 0.70f } };
            var sb = new StringBuilder("{\"screen\":\"" + tex.width + "x" + tex.height + "\",\"samples\":[");
            for (int i = 0; i < pts.Length; i++)
            {
                int x = Mathf.Clamp(Mathf.RoundToInt(pts[i][0] * tex.width), 4, tex.width - 5);
                int y = Mathf.Clamp(Mathf.RoundToInt((1f - pts[i][1]) * tex.height), 4, tex.height - 5);   // texture origin is bottom-left
                Color acc = Color.black; int n = 0;
                for (int dy = -3; dy <= 3; dy++) for (int dx = -3; dx <= 3; dx++) { acc += tex.GetPixel(x + dx, y + dy); n++; }
                acc /= n;
                if (i > 0) sb.Append(",");
                sb.Append("{\"fx\":" + F(pts[i][0]) + ",\"fy\":" + F(pts[i][1]) + ",\"rgb\":[" +
                          Mathf.RoundToInt(acc.r * 255f) + "," + Mathf.RoundToInt(acc.g * 255f) + "," + Mathf.RoundToInt(acc.b * 255f) + "]}");
            }
            sb.Append("]}");
            r.colorProbe = sb.ToString();
            Destroy(tex);
        }

        static IEnumerator CaptureAtEndOfFrame(Action<Texture2D> done)
        {
            yield return new WaitForEndOfFrame();
            done(ScreenCapture.CaptureScreenshotAsTexture());
        }

        void Finish()
        {
            if (restoreScale) GameplayRenderTarget.TogglePersistentSupersample();
            bool pass = problems.Count == 0 && results.Count > 0;
            foreach (var r in results) if (!r.loaded || r.errors > 0) pass = false;

            var sb = new StringBuilder();
            sb.Append("{\"result\":\"").Append(pass ? "PASS" : "FAIL").Append("\",");
            sb.Append("\"platform\":\"").Append(Application.platform).Append("\",");
            sb.Append("\"pointerBits\":").Append(IntPtr.Size * 8).Append(",");
            sb.Append("\"scriptingBackend\":\"")
#if ENABLE_IL2CPP
              .Append("IL2CPP")
#else
              .Append("Mono")
#endif
              .Append("\",");
            sb.Append("\"unity\":\"").Append(Application.unityVersion).Append("\",");
            sb.Append("\"graphicsApi\":\"").Append(SystemInfo.graphicsDeviceType).Append("\",");
            sb.Append("\"cpu\":\"").Append(Escape(SystemInfo.processorType)).Append("\",");
            sb.Append("\"os\":\"").Append(Escape(SystemInfo.operatingSystem)).Append("\",");
            sb.Append("\"config\":{\"targetFrameRate\":").Append(Application.targetFrameRate)
              .Append(",\"vSyncCount\":").Append(QualitySettings.vSyncCount)
              .Append(",\"quality\":\"").Append(QualitySettings.names[QualitySettings.GetQualityLevel()]).Append("\"")
              .Append(",\"refreshHz\":").Append(Screen.currentResolution.refreshRateRatio.value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture))
              .Append(",\"screen\":\"").Append(Screen.width).Append("x").Append(Screen.height).Append("\"")
              .Append(",\"internalScale\":").Append(GameplayRenderTarget.InternalScale)
              .Append(",\"displayCapOverride\":").Append(GameplayRenderTarget.DisplayCapOverride)
              .Append(",\"expScale\":").Append(expScale).Append(",\"expVsync\":").Append(expVsync).Append(",\"expFps\":").Append(expFps)
              .Append(",\"frameTimingEnabled\":").Append(FrameTimingManager.IsFeatureEnabled() ? "true" : "false").Append("},");
            sb.Append("\"scenes\":[");
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                if (i > 0) sb.Append(",");
                sb.Append("{\"scene\":\"").Append(r.scene).Append("\",\"loaded\":").Append(r.loaded ? "true" : "false")
                  .Append(",\"frames\":").Append(r.frames).Append(",\"errors\":").Append(r.errors)
                  .Append(",\"spriteRenderers\":").Append(r.spriteRenderers).Append(",\"cameras\":").Append(r.cameras)
                  .Append(",\"seconds\":").Append(r.seconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))
                  .Append(",\"avgFps\":").Append(F(r.avgFps)).Append(",\"p50Ms\":").Append(F(r.p50Ms)).Append(",\"p95Ms\":").Append(F(r.p95Ms))
                  .Append(",\"p99Ms\":").Append(F(r.p99Ms)).Append(",\"maxMs\":").Append(F(r.maxMs)).Append(",\"over33Ms\":").Append(r.over33Ms)
                  .Append(",\"gcCollections\":").Append(r.gcCollections).Append(",\"allocKbPerFrame\":").Append(F(r.allocKbPerFrame))
                  .Append(",\"cpuMainMs\":").Append(F(r.cpuMainMs)).Append(",\"cpuRenderMs\":").Append(F(r.cpuRenderMs)).Append(",\"gpuMs\":").Append(F(r.gpuMs))
                  .Append(",\"rt\":\"").Append(r.rtWidth).Append("x").Append(r.rtHeight).Append("\"")
                  .Append(r.colorProbe != null ? ",\"colorProbe\":" + r.colorProbe : "").Append("}");
            }
            sb.Append("],\"problems\":[");
            for (int i = 0; i < problems.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("\"").Append(Escape(problems[i])).Append("\"");
            }
            sb.Append("]}");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllText(outputPath, sb.ToString());
            }
            catch (Exception e) { Debug.Log("DC1_SMOKE could not write result file: " + e.Message); }

            Debug.Log("DC1_SMOKE_RESULT " + (pass ? "PASS" : "FAIL") + " " + sb);
            Application.Quit(pass ? 0 : 1);
        }

        static string F(float v) { return v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture); }

        static string Escape(string s)
        {
            return (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", " ");
        }
    }
}
#endif
