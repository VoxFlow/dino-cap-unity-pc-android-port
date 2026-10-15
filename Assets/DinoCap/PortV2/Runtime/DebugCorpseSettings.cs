using UnityEngine;

namespace DinoCap.PortV2
{
    // Shared requested debug convenience. The native cleanup is retained unless debug tools
    // are effectively enabled (always inside the Combat Workbench, otherwise the saved switch).
    public static class DebugCorpseSettings
    {
        public const string PreferenceKey="DC1_Debug_DinoCorpseDespawn";
        public static bool DespawnEnabled
        {
            get
            {
                return !DC1DebugSettings.EffectiveEnabled || DC1Preferences.GetInt(PreferenceKey,1)!=0;
            }
        }
        public static void SetDespawnEnabled(bool enabled)
        {
            DC1Preferences.SetInt(PreferenceKey,enabled?1:0);
            DC1Preferences.Save();
        }
    }
}
