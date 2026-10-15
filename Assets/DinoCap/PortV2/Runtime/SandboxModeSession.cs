using UnityEngine.SceneManagement;

namespace DinoCap.PortV2
{
    // User-requested Sandbox mode adapter. Real scene owners retain sole
    // ownership of spawning, combat, shops, death, progression and saves.
    public static class SandboxModeSession
    {
        internal enum Entry { None, City, Jungle }
        static Entry pending;
        public static bool Active { get; private set; }

        public static void OpenCity() => Open(Entry.City);
        public static void OpenJungle() => Open(Entry.Jungle);
        static void Open(Entry entry)
        {
            Active=true;pending=entry;
            SceneManager.LoadScene("MainMenuScene");
        }
        internal static Entry ConsumeEntry()
        {
            var entry=pending;pending=Entry.None;return entry;
        }
        public static void ReturnToWorkbench()
        {
            Reset();
            SceneManager.LoadScene("CombatSandboxV2");
        }
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Reset(){Active=false;pending=Entry.None;}
    }
}
