using UnityEngine;

namespace DinoCap.PortV2
{
    public static class NativeDinoLandingAudio
    {
        // Native76A20 primary event -> dino vtable+44 ->10294 case3.
        // iPad preload marks die frame17 / die2 frame18 (Task018;
        // k2 marks independently verified at3D536/3D56A). This is an
        // animation event, not an extra physics-contact sound. Ash death
        // has no such mark and must not receive this cue.
        public static bool IsMarkedDeathFrame(Power2DTrack track,int frame,bool dead,bool speciesHasDeathMarks=true)
        {
            // K4's own21BBCC/21BBE4 clips are loaded without78BC8 death
            // marks at42F76/42F98. Select by semantic species, since the
            // other species' special death can share a k4-prefixed asset.
            if(!speciesHasDeathMarks || !dead || track?.Clip==null) return false;
            string name=track.Clip.name;
            return (name.EndsWith("_die") && frame==17) ||
                   (name.EndsWith("_die2") && frame==18);
        }

        public static void Play(AudioSource template)
        {
            var clip=Resources.Load<AudioClip>("Audio/Audio_death3");
            NativeAudioSessionGate.Play(template,clip);
        }
    }
}
