#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoCap.PortV2
{
    // DEBUG_ONLY (development player only; inert without -dc1-pauseprobe). Drives the real City scene in the real player: walk right + shoot
    // through the game's own input methods, pause, and save real screenshots before the pause, one frame into it and 0.5 s into it, then logs
    // how much the player's screen region changed. Used to check that pausing freezes the player's pose (iPad reference behaviour).
    // Args: -dc1-pauseprobe [-dc1-pauseprobe-out <dir>] [-dc1-pauseprobe-family N] [-dc1-pauseprobe-fmode N]
    public sealed class DC1PausePoseProbe : MonoBehaviour
    {
        static bool started;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartIfRequested()
        {
            if (started) return;
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-dc1-pauseprobe") < 0) return;
            started = true;
            var go = new GameObject("DC1 Pause Pose Probe");
            DontDestroyOnLoad(go);
            var probe = go.AddComponent<DC1PausePoseProbe>();
            int o = Array.IndexOf(args, "-dc1-pauseprobe-out");
            probe.outDir = (o >= 0 && o + 1 < args.Length) ? Path.GetFullPath(args[o + 1]) : Application.persistentDataPath;
            probe.family = IntArg(args, "-dc1-pauseprobe-family", 0);
            probe.fmode = IntArg(args, "-dc1-pauseprobe-fmode", 1);
            probe.StartCoroutine(probe.Run());
            probe.StartCoroutine(probe.Watchdog());
        }

        static int IntArg(string[] a, string n, int d) { int i = Array.IndexOf(a, n); int v; return (i >= 0 && i + 1 < a.Length && int.TryParse(a[i + 1], out v)) ? v : d; }

        string outDir; int family, fmode;
        readonly StringBuilder log = new StringBuilder();

        IEnumerator Watchdog() { yield return new WaitForSecondsRealtime(90f); Write("WATCHDOG timeout"); Application.Quit(2); }

        void Write(string line)
        {
            log.AppendLine(line);
            try { Directory.CreateDirectory(outDir); File.WriteAllText(Path.Combine(outDir, "pauseprobe.txt"), log.ToString()); } catch { }
        }

        Texture2D Grab() => ScreenCapture.CaptureScreenshotAsTexture();

        static Rect PlayerRegion(CityGridController city)
        {
            var cam = Camera.main;
            Vector3 sp = cam != null ? cam.WorldToScreenPoint(city.TestHarness_PlayerTransform.position) : new Vector3(Screen.width * 0.3f, Screen.height * 0.4f);
            float w = Screen.height * 0.30f, h = Screen.height * 0.30f;
            return new Rect(Mathf.Clamp(sp.x - w * 0.5f, 0, Screen.width - w), Mathf.Clamp(sp.y - h * 0.15f, 0, Screen.height - h), w, h);
        }

        static float RegionDiff(Texture2D a, Texture2D b, Rect r)
        {
            var pa = a.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
            var pb = b.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
            double sum = 0; for (int i = 0; i < pa.Length; i++) sum += Mathf.Abs(pa[i].r - pb[i].r) + Mathf.Abs(pa[i].g - pb[i].g) + Mathf.Abs(pa[i].b - pb[i].b);
            return (float)(sum / (pa.Length * 3.0));
        }

        IEnumerator Run()
        {
            Write("start platform=" + Application.platform + " family=" + family + " fmode=" + fmode);
            var op = SceneManager.LoadSceneAsync("CityGrindScene");
            while (op != null && !op.isDone) yield return null;
            CityGridController city = null; float t0 = Time.realtimeSinceStartup;
            while (city == null && Time.realtimeSinceStartup - t0 < 20f) { yield return null; city = FindFirstObjectByType<CityGridController>(); }
            if (city == null) { Write("FAIL no CityGridController"); Application.Quit(3); yield break; }
            for (int i = 0; i < 5; i++) yield return null;
            city.TestHarness_SkipLevelIntro(); city.TestHarness_ClearDinos(); city.TestHarness_SetPlayerX(2000f);
            city.TestHarness_SetRangedFamily(family); if (family == 5) city.TestHarness_SetFMode(fmode);
            city.TestHarness_GrantAmmo(family, 5000);
            for (int i = 0; i < 30; i++) yield return null;
            t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 0.9f) { city.TestHarness_RequestMoveRight(); city.TestHarness_Shoot(); yield return null; }
            city.TestHarness_RequestMoveRight(); city.TestHarness_Shoot();
            yield return new WaitForEndOfFrame();
            var rect = PlayerRegion(city);
            var p0 = Grab();
            File.WriteAllBytes(Path.Combine(outDir, "p0_before_pause.png"), p0.EncodeToPNG());
            city.TestHarness_RequestPause();                       // exactly what the pause icon press calls
            yield return null; yield return new WaitForEndOfFrame();
            var a = Grab(); File.WriteAllBytes(Path.Combine(outDir, "a_pause_plus1.png"), a.EncodeToPNG());
            t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 0.5f) yield return null;
            yield return new WaitForEndOfFrame();
            var b = Grab(); File.WriteAllBytes(Path.Combine(outDir, "b_pause_plus500ms.png"), b.EncodeToPNG());
            Write("paused=" + city.TestHarness_Paused + " region=" + rect);
            Write("diff p0->a (1 frame after pause press) = " + RegionDiff(p0, a, rect).ToString("0.0000"));
            Write("diff a->b (paused for 0.5 s)          = " + RegionDiff(a, b, rect).ToString("0.0000"));
            Write("DONE");
            Application.Quit(0);
        }
    }
}
#endif
