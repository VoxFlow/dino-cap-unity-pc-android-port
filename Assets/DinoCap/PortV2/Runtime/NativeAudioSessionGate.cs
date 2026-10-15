using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    public sealed class NativeAudioSessionGate : MonoBehaviour
    {
        public const int NativeMaxSessions = 28;
        const float NativeGcSeconds = 2.0f;

        sealed class Session
        {
            public GameObject owner;
            public AudioSource source;
        }

        static NativeAudioSessionGate instance;
        readonly List<Session> sessions = new List<Session>(NativeMaxSessions);
        float gcAccumulator;

        static int totalRequests;
        static int totalAdmitted;
        static int totalDropped;
        static int att3Requests;
        static int att3Admitted;
        static int att3Dropped;

        // TASK027N.2.2 Part D: DEBUG_ONLY per-clip-name admitted-play
        // counter, generic (unlike the att3-specific counters above) so a
        // test can distinguish e.g. Attack_G / Audio_G4 (swing) from
        // Audio_att1 / Audio_att2 (contact) admitted-play events without
        // adding a new named counter pair per clip. Does not alter Play()'s
        // own admission/drop logic in any way.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        static readonly Dictionary<string,int> byClipAdmitted = new Dictionary<string,int>();
        public static void TestHarness_ResetClipCounters() => byClipAdmitted.Clear();
        public static int TestHarness_AdmittedCount(string clipName) =>
            byClipAdmitted.TryGetValue(clipName,out int n) ? n : 0;
#endif

        public static int TotalRequests { get { return totalRequests; } }
        public static int TotalAdmitted { get { return totalAdmitted; } }
        public static int TotalDropped { get { return totalDropped; } }
        public static int Att3Requests { get { return att3Requests; } }
        public static int Att3Admitted { get { return att3Admitted; } }
        public static int Att3Dropped { get { return att3Dropped; } }
        public static int CountedSessions { get { return instance != null ? instance.sessions.Count : 0; } }

        // TASK027N.2.51 Part 4.16: native's own pause menu exposes a
        // player-toggleable FX mute control (sub_64694, 'text_fxFrame' +
        // on/off state graphics - see pause_contract.json). This is the ONE
        // shared SFX-playback gate for the whole game, so the mute flag
        // lives here rather than being duplicated per caller.
        public static bool FxMuted;
        public const string FxMutedPrefKey="DC1_CityGrind_FxMuted";
        const string LegacyMenuSfxPrefKey="DC1_MainMenu_SfxOn";

        // Native byte_214FB8 is shared by menu/gameplay. Retain the existing
        // City key and migrate menu-only saves when that key is absent.
        public const string MusicMutedPrefKey="DC1_CityGrind_MusicMuted";
        const string LegacyMenuMusicPrefKey="DC1_MainMenu_MusicOn";
        // Conflicting old per-screen saves have no timestamp. Preserve an
        // explicit OFF in either screen; the next toggle aligns both keys.
        public static bool LoadMusicPreference() => DC1Preferences.GetInt(MusicMutedPrefKey,0)!=0
            || DC1Preferences.GetInt(LegacyMenuMusicPrefKey,1)==0;

        public static void SaveMusicPreference(bool muted)
        {
            DC1Preferences.SetInt(MusicMutedPrefKey,muted?1:0);
            DC1Preferences.SetInt(LegacyMenuMusicPrefKey,muted?0:1);
            DC1Preferences.Save();
        }

        // Native7EB04 consumes one shared byte_214FBC in menu and City.
        // Retain the existing City key as the canonical host preference;
        // legacy menu-only saves supply the fallback when it is absent.
        public static bool LoadFxPreference()
        {
            FxMuted=DC1Preferences.HasKey(FxMutedPrefKey)
                ? DC1Preferences.GetInt(FxMutedPrefKey)!=0
                : DC1Preferences.GetInt(LegacyMenuSfxPrefKey,1)==0;
            return FxMuted;
        }

        public static void SaveFxPreference(bool muted)
        {
            FxMuted=muted;
            DC1Preferences.SetInt(FxMutedPrefKey,muted?1:0);
            DC1Preferences.SetInt(LegacyMenuSfxPrefKey,muted?0:1);
        }

        public static void ResetDiagnostics()
        {
            totalRequests = 0;
            totalAdmitted = 0;
            totalDropped = 0;
            att3Requests = 0;
            att3Admitted = 0;
            att3Dropped = 0;
        }

        public static bool Play(AudioSource template, AudioClip clip)
        {
            if(template == null || clip == null)
                return false;

            NativeAudioSessionGate gate = Ensure();

            bool isAtt3 = (clip.name == "Audio_att3");
            totalRequests++;
            if(isAtt3) att3Requests++;

            // TASK027N.2.51 Part 4.16: player-toggled FX mute (native's own
            // pause-menu control) - request is counted (a genuine Play()
            // call occurred) but no session/AudioSource is created, matching
            // the same 'return true, create nothing' shape as the cap check
            // immediately below.
            if(FxMuted)
                return true;

            // Native returns success at cap but creates nothing.
            if(gate.sessions.Count >= NativeMaxSessions)
            {
                totalDropped++;
                if(isAtt3) att3Dropped++;
                return true;
            }

            GameObject go = new GameObject("NativeAudioSession_" + clip.name);
            go.transform.SetParent(gate.transform, false);
            go.transform.position = template.transform.position;

            AudioSource src = go.AddComponent<AudioSource>();
            CopySourceContract(template, src);
            src.clip = clip;
            src.loop = false;

            Session session = new Session
            {
                owner = go,
                source = src
            };

            // Native count increments before playback starts.
            gate.sessions.Add(session);
            totalAdmitted++;
            if(isAtt3) att3Admitted++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            byClipAdmitted.TryGetValue(clip.name,out int curClip);
            byClipAdmitted[clip.name]=curClip+1;
#endif
            src.Play();
            return true;
        }

        public static void StopAll()
        {
            if(instance == null)
                return;

            for(int i=0;i<instance.sessions.Count;i++)
            {
                Session s = instance.sessions[i];
                if(s != null && s.source != null)
                    s.source.Stop();
                if(s != null && s.owner != null)
                    Destroy(s.owner);
            }

            instance.sessions.Clear();
            instance.gcAccumulator = 0f;
        }

        static NativeAudioSessionGate Ensure()
        {
            if(instance != null)
                return instance;

            GameObject go = new GameObject("NativeAudioSessionGate");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<NativeAudioSessionGate>();
            return instance;
        }

        static void CopySourceContract(AudioSource src, AudioSource dst)
        {
            dst.outputAudioMixerGroup = src.outputAudioMixerGroup;
            dst.mute = src.mute;
            dst.bypassEffects = src.bypassEffects;
            dst.bypassListenerEffects = src.bypassListenerEffects;
            dst.bypassReverbZones = src.bypassReverbZones;
            dst.playOnAwake = false;
            dst.loop = false;
            dst.priority = src.priority;
            dst.volume = src.volume;
            dst.pitch = src.pitch;
            dst.panStereo = src.panStereo;
            dst.spatialBlend = src.spatialBlend;
            dst.reverbZoneMix = src.reverbZoneMix;
            dst.dopplerLevel = src.dopplerLevel;
            dst.spread = src.spread;
            dst.rolloffMode = src.rolloffMode;
            dst.minDistance = src.minDistance;
            dst.maxDistance = src.maxDistance;
        }

        void Update()
        {
            gcAccumulator += Time.unscaledDeltaTime;
            if(gcAccumulator <= NativeGcSeconds)
                return;

            // Native discards overshoot and resets to exactly zero.
            gcAccumulator = 0f;

            // Reap every finished session in this batch.
            for(int i=sessions.Count-1;i>=0;i--)
            {
                Session s = sessions[i];
                if(s == null || s.source == null || !s.source.isPlaying)
                {
                    if(s != null && s.owner != null)
                        Destroy(s.owner);
                    sessions.RemoveAt(i);
                }
            }
        }

        void OnDestroy()
        {
            if(instance == this)
                instance = null;
        }
    }
}
