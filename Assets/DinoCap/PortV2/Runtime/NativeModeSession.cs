using UnityEngine;

namespace DinoCap.PortV2
{
    //Native global cash/lives survive Jungle's E8E48/13FD4 local reset.
    //In-memory adapter only: Jungle does not write a City checkpoint.
    public static class NativeModeSession
    {
        public struct Snapshot { public bool initialized;public int cash,lives; }
        static Snapshot current;
        public static Snapshot Capture()=>current;
        public static void Restore(Snapshot snapshot)=>current=snapshot;
        public static Snapshot Read()
        {
            if(!current.initialized)Publish(DC1Preferences.GetInt("DC1_CityGrind_Cash",0),
                DC1Preferences.GetInt(CityGridController.LivesPrefKey,0));
            return current;
        }
        public static void Publish(int cash,int lives)
            =>current=new Snapshot {initialized=true,cash=cash,lives=lives};
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()=>current=default;
    }
}
