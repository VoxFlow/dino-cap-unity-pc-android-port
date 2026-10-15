using UnityEngine;

namespace DinoCap.PortV2
{
    // DEBUG_ONLY / PLATFORM_ADAPTER: the debug numeric fields (cash, Jungle timer, level, enemy HP ...) are ordinary IMGUI text fields that use the
    // OS soft keyboard. IMGUI keeps a field "focused" after the keyboard is dismissed in some ways, so the keyboard could not be reopened
    // or stayed attached. Rule: once the soft keyboard has been shown for a focused field, the moment it is no longer visible (Back, the
    // keyboard's hide key, Done/OK/Enter, anything else) the field's focus is dropped. Escape (desktop, or Back with no keyboard) also drops focus.
    // No gameplay semantics. REVERSIBILITY: delete this file and replace Field/FieldLayout with GUI.TextField / GUILayout.TextField.
    public sealed class DC1DebugTextInput : MonoBehaviour
    {
        static DC1DebugTextInput instance;
        bool keyboardWasVisible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (instance != null) return;
            var go = new GameObject("DC1DebugTextInput");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy;
            instance = go.AddComponent<DC1DebugTextInput>();
        }

        public static string Field(Rect rect, string text) => GUI.TextField(rect, text);

        public static string FieldLayout(string text, GUILayoutOption[] options) => GUILayout.TextField(text, options);

        // Back handlers (City pause, main menu) call this FIRST. If a debug field is focused or any soft keyboard is visible, it is closed and
        // the Back press is consumed (returns true) so it cannot also pause the game. Independent of which keyboard app is installed.
        public static bool ConsumeBack()
        {
            bool open = GUIUtility.keyboardControl != 0 || (Application.isMobilePlatform && TouchScreenKeyboard.visible);
            if (!open) return false;
            DropFocus();
            if (instance != null) instance.keyboardWasVisible = false;
            return true;
        }

        static void DropFocus()
        {
            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
        }

        // LateUpdate: after every Update-time Back handler had the chance to call ConsumeBack(); this only closes the field when nothing else used Back.
        void LateUpdate()
        {
            if (GUIUtility.keyboardControl == 0) { keyboardWasVisible = false; return; }

            bool escape;
#if ENABLE_INPUT_SYSTEM
            var k = UnityEngine.InputSystem.Keyboard.current;
            escape = k != null && k.escapeKey.wasPressedThisFrame;
#else
            escape = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (escape) { DropFocus(); keyboardWasVisible = false; return; }

            if (!Application.isMobilePlatform) return;
            if (TouchScreenKeyboard.visible) keyboardWasVisible = true;
            else if (keyboardWasVisible) { DropFocus(); keyboardWasVisible = false; }   // keyboard was shown and is now gone, by any route
        }
    }
}
