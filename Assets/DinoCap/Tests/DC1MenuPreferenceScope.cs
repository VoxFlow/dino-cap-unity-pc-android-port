#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2.Tests
{
    // Menu GO tests reset the committed run. Isolate/restore every affected
    // campaign field and the shared FX flag, rather than overwriting a save.
    internal sealed class DC1MenuPreferenceScope : IDisposable
    {
        readonly Dictionary<string,int?> saved=new Dictionary<string,int?>();
        readonly float? health;
        readonly bool fxMuted;
        readonly NativeModeSession.Snapshot session;
        readonly Dictionary<string,float?> debugFloats=new Dictionary<string,float?>();
        public DC1MenuPreferenceScope()
        {
            session=NativeModeSession.Capture();NativeModeSession.Restore(default);
            fxMuted=NativeAudioSessionGate.FxMuted;
            var keys=new List<string> {"DC1_CityGrind_Level","DC1_CityGrind_MaxLevel",
                "DC1_CityGrind_Lives","DC1_CityGrind_Cash","DC1_CityGrind_Difficulty",
                "DC1_CityGrind_OwnershipModelMigratedV1","DC1_CityGrind_Tier_G",CityGrindStartPreset.EquippedPrefKey,
                "DC1_MainMenu_MusicOn",NativeAudioSessionGate.MusicMutedPrefKey,"DC1_MainMenu_SfxOn",NativeAudioSessionGate.FxMutedPrefKey,DebugCorpseSettings.PreferenceKey,
                "DC1_Debug_Jungle_GodMode","DC1_Debug_Jungle_InfiniteAmmo",
                "DC1_Debug_CityGrind_GodMode","DC1_Debug_Jungle_FutureSpeciesMask"};
            for(int i=0;i<6;i++){keys.Add("DC1_CityGrind_Tier_"+i);keys.Add(CityGrindStartPreset.AmmoPrefPrefix+i);keys.Add(CityGrindStartPreset.BonusPrefPrefix+i);}
            foreach(string key in keys) {
                saved[key]=DC1Preferences.HasKey(key)?(int?)DC1Preferences.GetInt(key):null;
                DC1Preferences.DeleteKey(key);
            }
            health=DC1Preferences.HasKey(CityGrindStartPreset.HealthPrefKey)?(float?)DC1Preferences.GetFloat(CityGrindStartPreset.HealthPrefKey):null;
            foreach(string key in new[]{"DC1_Debug_Jungle_MoveSpeed","DC1_Debug_Jungle_TimerSpeed"})
            {
                debugFloats[key]=DC1Preferences.HasKey(key)?(float?)DC1Preferences.GetFloat(key):null;
                DC1Preferences.DeleteKey(key);
            }
            DC1Preferences.DeleteKey(CityGrindStartPreset.HealthPrefKey);
            NativeAudioSessionGate.FxMuted=false;
            NativeAudioSessionGate.StopAll();
            DC1Preferences.Save();
        }
        public void Dispose()
        {
            NativeModeSession.Restore(session);
            foreach(var pair in debugFloats)
                if(pair.Value.HasValue)DC1Preferences.SetFloat(pair.Key,pair.Value.Value);else DC1Preferences.DeleteKey(pair.Key);
            foreach(var pair in saved)
                if(pair.Value.HasValue)DC1Preferences.SetInt(pair.Key,pair.Value.Value);else DC1Preferences.DeleteKey(pair.Key);
            if(health.HasValue)DC1Preferences.SetFloat(CityGrindStartPreset.HealthPrefKey,health.Value);
            else DC1Preferences.DeleteKey(CityGrindStartPreset.HealthPrefKey);
            NativeAudioSessionGate.FxMuted=fxMuted;
            NativeAudioSessionGate.StopAll();
            DC1Preferences.Save();
        }
    }
}
#endif
