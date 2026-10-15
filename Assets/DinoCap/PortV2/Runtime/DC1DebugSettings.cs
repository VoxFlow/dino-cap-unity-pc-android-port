using UnityEngine;

namespace DinoCap.PortV2
{
    public static class DC1DebugSettings
    {
        public const string EnabledKey="DC1_Debug_Enabled";
        public const string AffectsSavesKey="DC1_Debug_AffectsSaves";
        public const string UiHiddenKey="DC1_Debug_UiHidden";

        // User decision 2026-10-13: the debug switch works in every build (default OFF),
        // so Options -> DEBUG TOOLS -> COMBAT WORKBENCH is reachable in the shipped
        // Dino Cap app too. DEBUG_ONLY tooling, not production gameplay.
        public static bool Enabled
        {
            get=>DC1Preferences.GetInt(EnabledKey,0)!=0;
            set {DC1Preferences.SetInt(EnabledKey,value?1:0);DC1Preferences.Save();}
        }

        // The Combat Workbench (Sandbox scene) always exposes its debug UI,
        // regardless of the saved switch above. Other scenes still follow Enabled.
        public static bool EffectiveEnabled=>Enabled || CombatSandboxV2.Instance!=null;

        // One global shared preference for the debug panel's hidden/shown state:
        // set by HIDE / F1 / the restore chip in any scene, read when a scene starts,
        // so it never auto-opens just because a level (re)started. Defaults to shown.
        public static bool UiHidden
        {
            get=>DC1Preferences.GetInt(UiHiddenKey,0)!=0;
            set {DC1Preferences.SetInt(UiHiddenKey,value?1:0);DC1Preferences.Save();}
        }

        public static bool AffectsSaves
        {
            get=>DC1Preferences.GetInt(AffectsSavesKey,1)!=0;
            set {DC1Preferences.SetInt(AffectsSavesKey,value?1:0);DC1Preferences.Save();}
        }
    }
}
