using System;

namespace DinoCap.PortV2
{
    // Native17A08 arms first-row timers without setting interval flags.
    // 1802C checks/rearms a newly admitted interval BEFORE comparing the
    // incoming handle; callbacks from superseded timers cannot spawn.
    // Timer dispatch/physics belong to the scene, not this decision owner.
    public sealed class JungleSpawnSchedule
    {
        readonly JungleBlitzProgression progression;
        readonly Func<int,float,int> arm;
        readonly int[] handles=new int[4];
        readonly bool[] admitted=new bool[5];
        public int ElapsedSeconds { get; private set; }
        public bool Playing { get; private set; }
        public JungleSpawnSchedule(JungleBlitzProgression progression,Func<int,float,int> arm)
        {
            this.progression=progression ?? throw new ArgumentNullException(nameof(progression));
            this.arm=arm ?? throw new ArgumentNullException(nameof(arm));
        }
        public void Start()
        {
            Playing=true;
            // The initial values come from the first row even on the old
            // native continuation path; normal fresh runs begin at0.
            ArmAll(0);
        }
        public void Stop()=>Playing=false;
#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
        public void DebugSetElapsed(int seconds)
        {
            ElapsedSeconds=Math.Max(0,Math.Min(86400,seconds));
            Array.Clear(admitted,0,admitted.Length);
            if(Playing){ArmAll(ElapsedSeconds);admitted[progression.IntervalIndex(ElapsedSeconds)]=true;}
        }
#endif
        public void OnSecond()
        {
            if(!Playing)return;
            ElapsedSeconds++;
            AdmitElapsedInterval();
        }
        public bool OnDinoTimer(int handle,Action<int> spawn)
        {
            if(!Playing)return false;
            AdmitElapsedInterval();
            for(int i=0;i<4;i++)
                if(handles[i]==handle)
                {
                    spawn?.Invoke(i+1);
                    handles[i]=arm(i+1,progression.Query(ElapsedSeconds,i+1).frequency);
                    return true;
                }
            return false;
        }
        public int HandleForType(int type)
        {
            if(type<1 || type>4)throw new ArgumentOutOfRangeException(nameof(type));
            return handles[type-1];
        }
        void AdmitElapsedInterval()
        {
            int interval=progression.IntervalIndex(ElapsedSeconds);
            if(admitted[interval])return;
            ArmAll(ElapsedSeconds);
            admitted[interval]=true;
        }
        void ArmAll(int seconds)
        {
            for(int i=0;i<4;i++)handles[i]=arm(i+1,progression.Query(seconds,i+1).frequency);
        }
    }
}
