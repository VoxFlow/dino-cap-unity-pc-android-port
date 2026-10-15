using System;
using UnityEngine;

namespace DinoCap.PortV2
{
    // Native BEE60 loads five blocks in file order, then1802C/18858 scan
    // backwards with strict elapsed>time. The190 block is shadowed by150.
    // Fresh N254 ARM189F4..18A02 (and phone8614C..8615A) proves /30,
    // correcting Task003's earlier /9 interpretation. Independent of City.
    public sealed class JungleBlitzProgression
    {
        [Serializable] public struct DinoStats
        {
            public int type;
            public float hp, speed, frequency, attack, attackDistance;
            public int reward;
        }
        [Serializable] public sealed class Interval
        {
            public int time;
            public DinoStats[] dinos;
        }
        [Serializable] public sealed class NativeTable
        {
            public string sourceSha256;
            public Interval[] intervals;
        }
        readonly NativeTable table;
        public string SourceSha256 => table.sourceSha256;
        public JungleBlitzProgression(NativeTable nativeTable)
        {
            if(nativeTable==null || nativeTable.intervals==null || nativeTable.intervals.Length<5)
                throw new ArgumentException("Jungle native table requires the first five file-order intervals.");
            for(int i=0;i<5;i++)
                if(nativeTable.intervals[i].dinos==null || nativeTable.intervals[i].dinos.Length!=4)
                    throw new ArgumentException("Each Jungle native interval requires four dinosaur rows.");
            table=nativeTable;
        }
        public static JungleBlitzProgression Load()
        {
            var raw=Resources.Load<TextAsset>("DinoCapGenerated/jungle_progression");
            if(raw==null)throw new InvalidOperationException("Missing exported native Jungle progression table.");
            return new JungleBlitzProgression(JsonUtility.FromJson<NativeTable>(raw.text));
        }
        public int IntervalIndex(int elapsedSeconds)
        {
            for(int i=4;i>0;i--)
                if(elapsedSeconds>table.intervals[i].time)return i;
            return 0;
        }
        public DinoStats Query(int elapsedSeconds,int type)
        {
            if(type<1 || type>4)throw new ArgumentOutOfRangeException(nameof(type));
            DinoStats s=table.intervals[IntervalIndex(elapsedSeconds)].dinos[type-1];
            if(elapsedSeconds<=table.intervals[4].time)return s;
            int q=elapsedSeconds/30;
            switch(type)
            {
                case 1:s.hp=7*q+57;s.reward=5*q+103;s.attack=3*q+15;break;
                case 2:s.hp=5*q+32;s.reward=10*q+90;s.attack=2.5f*q+10;break;
                case 3:s.hp=12*q+67;s.reward=20*q+120;s.attack=5*q+20;break;
                case 4:s.hp=1.5f*q+42;s.reward=2*q+87;break;
            }
            // SPD, ADIS and FRQ retain the selected original150-block values;
            // K4 ATK is copied here. Query returns the elapsed-table stage;
            // native18858 applies the shared difficulty pass afterwards.
            return s;
        }
        public DinoStats QuerySpawn(int elapsedSeconds,int type,int difficulty)
        {
            var s=Query(elapsedSeconds,type);
            // iPad18858 raw1900A onward reads214FC0 AFTER every species
            // branch. Preserve F32 arithmetic and reward truncation.
            float adjustment=(difficulty-1)*0.1f;
            s.hp*=1f+adjustment;
            s.attack*=1f+adjustment;
            // Raw18FFC loads -.05, not -.1. Explicit F32 conversion before
            // truncation matches VCVT.S32.F32 even on Mono intermediates.
            s.reward=(int)(float)(s.reward*(1f+(difficulty-1)*-0.05f));
            return s;
        }
    }
}
