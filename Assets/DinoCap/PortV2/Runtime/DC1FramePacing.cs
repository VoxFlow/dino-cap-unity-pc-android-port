using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    // PLATFORM_ADAPTER + MODERN_VISUAL_ENHANCEMENT (presentation pacing only; no gameplay semantics).
    // ORIGINAL_CONTRACT: the shipped engine drives its update from an NSTimer whose interval double is
    //   0x3F91111111111111 = 1/60 s (PROVEN, Task027L.4.10). 60 is therefore the default limit.
    // WHY REQUIRED: production code never set Application.targetFrameRate, and Android paces at 30 fps
    //   by default even with vSync 1 (measured on a 60 Hz Redmi Note 9S: locked 30.0 fps in every scene;
    //   with a 60 target 59.6-59.9). 30 fps presents the 60 Hz native tick at half rate.
    // USER SETTING (approved 2026-10-13, persisted): frame limit + (desktop only) V-Sync.
    //   Mobile offers only 60 and the multiples of 60 the panel really supports (display modes), no
    //   Unlimited, no V-Sync toggle. Desktop offers 60 / 120 / 240 / Unlimited and V-Sync on/off; with V-Sync
    //   ON the monitor rate rules and the limit applies once V-Sync is off (best-practice split).
    //   Default: limit 60, V-Sync ON (= previous desktop behavior, no tearing).
    // Game logic and the fixed 1/60 s animation tick adapter (Task027L.4.11) are untouched: values above 60
    //   only smooth continuous motion. REVERSIBILITY: delete this file and its three UI call sites.
    public static class DC1FramePacing
    {
        public const string LimitKey = "DC1_FrameLimit";   // 0 = unlimited
        public const string VSyncKey = "DC1_VSync";
        const int NativeTickHz = 60;
        static readonly int[] DesktopLimits = { 60, 120, 240, 0 };

        public static bool IsMobile => Application.isMobilePlatform;
        public static bool VSyncSelectable => !IsMobile;

        public static int Limit
        {
            get => AvailableLimits().Contains(DC1Preferences.GetInt(LimitKey, NativeTickHz))
                ? DC1Preferences.GetInt(LimitKey, NativeTickHz) : NativeTickHz;
        }
        public static bool VSync => IsMobile || DC1Preferences.GetInt(VSyncKey, 1) != 0;

        public static List<int> AvailableLimits()
        {
            var list = new List<int>();
            if (!IsMobile) { list.AddRange(DesktopLimits); return list; }
            double max = NativeTickHz;
            foreach (var r in Screen.resolutions) max = System.Math.Max(max, r.refreshRateRatio.value);
            list.Add(NativeTickHz);
            for (int hz = NativeTickHz * 2; hz <= max + 1.0; hz += NativeTickHz) list.Add(hz);
            return list;
        }

        public static string LimitName => Limit == 0 ? "UNLIMITED" : Limit.ToString();

        public static void CycleLimit()
        {
            var l = AvailableLimits();
            int next = l[(l.IndexOf(Limit) + 1) % l.Count];
            DC1Preferences.SetInt(LimitKey, next); DC1Preferences.Save(); Apply();
        }

        public static void ToggleVSync()
        {
            if (!VSyncSelectable) return;
            DC1Preferences.SetInt(VSyncKey, VSync ? 0 : 1); DC1Preferences.Save(); Apply();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // Headless/batch runs (test runner, CI) have no display to pace and the test suite is written against uncapped frames, so the
        // launch-time pacing is skipped there. Explicit Apply() calls (settings changes, pacing tests) always work.
        static void Startup() { if (!Application.isBatchMode) Apply(); }

        public static void Apply()
        {
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            Application.targetFrameRate = Limit == 0 ? -1 : Limit;
        }
    }
}
