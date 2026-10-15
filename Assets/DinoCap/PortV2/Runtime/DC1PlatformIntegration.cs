using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DinoCap.PortV2
{
    // PLATFORM_ADAPTER (user-approved QoL, 2026-10-13). No gameplay semantics: windowing, fullscreen, focus, immersive mode,
    // screen-awake, cutout insets and a one-time shader warmup. Never touches the original menu "Loading..." sequence:
    // the warmup runs once at launch (before the first menu frame is drawn) and the sequence's timing is untouched.
    //   PC:      F11 or Alt+Enter toggles borderless fullscreen (FullScreenWindow) / windowed; fullscreen state and the
    //            windowed size + position are remembered (DC1Preferences) and restored at launch.
    //   Android: sticky immersive mode, screen kept awake, cutout/notch insets published to NativeViewport.
    // Auto-pause on focus loss / backgrounding lives in CityGridController (gameplay scenes only, never in the menu).
    // REVERSIBILITY: delete this file and the NativeViewport inset lines.
    public sealed class DC1PlatformIntegration : MonoBehaviour
    {
        public const string FullscreenKey = "DC1_Fullscreen";
        public const string WindowWKey = "DC1_Window_W", WindowHKey = "DC1_Window_H", WindowXKey = "DC1_Window_X", WindowYKey = "DC1_Window_Y";
        static DC1PlatformIntegration instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (instance != null) return;
            var go = new GameObject("DC1PlatformIntegration");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy;
            instance = go.AddComponent<DC1PlatformIntegration>();
        }

        void Awake()
        {
            instance = this;
            WarmUpShaders();
            if (Application.isMobilePlatform)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                ApplyImmersive();
            }
            else if (!Application.isEditor)
                RestoreWindow();
        }

        // One synchronous warmup at launch so first use of a shader in a level does not hitch. Measured at launch
        // (logged); it happens before the original menu sequence begins, so no native timing is affected.
        static void WarmUpShaders()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try { Shader.WarmupAllShaders(); } catch (Exception e) { Debug.LogWarning("[DC1-PLATFORM] shader warmup skipped: " + e.Message); }
            Debug.Log("[DC1-PLATFORM] shader warmup " + sw.ElapsedMilliseconds + " ms");
        }

        void Update()
        {
            if (Application.isMobilePlatform) { PublishSafeInsets(); return; }
            if (Application.isEditor) return;
            var kb = ReadToggle();
            if (kb) ToggleFullscreen();
            if (!Screen.fullScreen) RememberWindow();
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus && Application.isMobilePlatform) ApplyImmersive();
        }

        void OnApplicationQuit()
        {
            if (!Application.isMobilePlatform && !Application.isEditor) { SaveFullscreen(); if (!Screen.fullScreen) RememberWindow(true); }
        }

        // ---------------------------------------------------------------- PC
        static bool ReadToggle()
        {
#if ENABLE_INPUT_SYSTEM
            var k = UnityEngine.InputSystem.Keyboard.current;
            if (k == null) return false;
            bool alt = k.leftAltKey.isPressed || k.rightAltKey.isPressed;
            return k.f11Key.wasPressedThisFrame || (alt && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame));
#else
            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            return Input.GetKeyDown(KeyCode.F11) || (alt && Input.GetKeyDown(KeyCode.Return));
#endif
        }

        static void ToggleFullscreen()
        {
            if (Screen.fullScreen)
            {
                int w = DC1Preferences.GetInt(WindowWKey, 1280), h = DC1Preferences.GetInt(WindowHKey, 720);
                Screen.SetResolution(w, h, FullScreenMode.Windowed);
                DC1Preferences.SetInt(FullscreenKey, 0);
            }
            else
            {
                SaveWindowRect();
                var res = Screen.currentResolution;
                Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow);   // borderless fullscreen
                DC1Preferences.SetInt(FullscreenKey, 1);
            }
            DC1Preferences.Save();
        }

        static void SaveFullscreen() { DC1Preferences.SetInt(FullscreenKey, Screen.fullScreen ? 1 : 0); DC1Preferences.Save(); }

        static int lastW, lastH, lastX, lastY, stableFrames;
        static void RememberWindow(bool force = false)
        {
            int x = lastX, y = lastY;
            WindowPos(out x, out y);
            if (Screen.width == lastW && Screen.height == lastH && x == lastX && y == lastY)
            {
                if (++stableFrames == 30 || force) SaveWindowRect();   // persist once the window has been stable for 30 frames
                return;
            }
            lastW = Screen.width; lastH = Screen.height; lastX = x; lastY = y; stableFrames = 0;
        }

        static void SaveWindowRect()
        {
            if (Screen.fullScreen) return;
            WindowPos(out int x, out int y);
            DC1Preferences.SetInt(WindowWKey, Screen.width); DC1Preferences.SetInt(WindowHKey, Screen.height);
            DC1Preferences.SetInt(WindowXKey, x); DC1Preferences.SetInt(WindowYKey, y);
            DC1Preferences.Save();
        }

        static void RestoreWindow()
        {
            // Command-line size arguments (smoke/test harness, -screen-width ...) always win over the remembered state.
            foreach (var a in Environment.GetCommandLineArgs())
                if (a == "-screen-width" || a == "-screen-height" || a == "-screen-fullscreen" || a == "-dc1-smoke") return;
            if (DC1Preferences.GetInt(FullscreenKey, 1) != 0)
            {
                // First launch and "fullscreen" both start borderless fullscreen (the project's configured default mode).
                var res = Screen.currentResolution;
                Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow);
                return;
            }
            int w = DC1Preferences.GetInt(WindowWKey, 0), h = DC1Preferences.GetInt(WindowHKey, 0);
            if (w < 640 || h < 360) return;
            Screen.SetResolution(w, h, FullScreenMode.Windowed);
            if (DC1Preferences.HasKey(WindowXKey)) instance.StartCoroutine(MoveWindowSoon(DC1Preferences.GetInt(WindowXKey), DC1Preferences.GetInt(WindowYKey)));
        }

        static System.Collections.IEnumerator MoveWindowSoon(int x, int y)
        {
            yield return null; yield return null;           // let the resolution change settle first
            SetWindowPos(x, y);
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [StructLayout(LayoutKind.Sequential)] struct RECT { public int left, top, right, bottom; }
        [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
        static void WindowPos(out int x, out int y)
        {
            x = 0; y = 0;
            try { var h = GetActiveWindow(); if (h != IntPtr.Zero && GetWindowRect(h, out var r)) { x = r.left; y = r.top; } } catch { }
        }
        static void SetWindowPos(int x, int y)
        {
            try
            {
                var h = GetActiveWindow();
                if (h == IntPtr.Zero) return;
                // Only restore a position that is still on a connected display (sanity: left/top inside the virtual desktop).
                if (x < -32000 || y < -32000 || x > 32000 || y > 32000) return;
                SetWindowPos(h, IntPtr.Zero, x, y, 0, 0, 0x0001 | 0x0004);   // SWP_NOSIZE | SWP_NOZORDER
            }
            catch { }
        }
#else
        static void WindowPos(out int x, out int y) { x = 0; y = 0; }
        static void SetWindowPos(int x, int y) { }
#endif

        // ------------------------------------------------------------ Android
        static void ApplyImmersive()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                // The runnable executes later on the UI thread: re-use the activity/player handles inside it and never
                // dispose them before it has run (disposing here made them null inside the callback).
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    try
                    {
                        var window = activity.Call<AndroidJavaObject>("getWindow");
                        var decor = window.Call<AndroidJavaObject>("getDecorView");
                        // IMMERSIVE_STICKY | HIDE_NAVIGATION | FULLSCREEN | LAYOUT_STABLE | LAYOUT_HIDE_NAVIGATION | LAYOUT_FULLSCREEN
                        const int flags = 0x1000 | 0x2 | 0x4 | 0x100 | 0x200 | 0x400;
                        decor.Call("setSystemUiVisibility", flags);
                    }
                    catch (Exception e) { Debug.LogWarning("[DC1-PLATFORM] immersive failed: " + e.Message); }
                }));
            }
            catch (Exception e) { Debug.LogWarning("[DC1-PLATFORM] immersive failed: " + e.Message); }
#endif
        }

        static void PublishSafeInsets()
        {
            Rect safe = Screen.safeArea;
            if (Screen.width <= 0) return;
            NativeViewport.SafeInsetLeft = Mathf.Max(0f, safe.xMin);
            NativeViewport.SafeInsetRight = Mathf.Max(0f, Screen.width - safe.xMax);
        }
    }
}
