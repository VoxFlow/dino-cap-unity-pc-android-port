using System;

namespace DinoCap.PortV2
{
    // iPad dinosaur.prop EXP +57BD8 level/jitter/difficulty pass.
    // ARM58198 VCVT.S32.F32 truncates;10B90 common fatal tail pays once.
    public static class NativeCityDinoReward
    {
        public const int CashCap=999999;
        public static int Compute(int speciesIndex,int level,int difficulty,int jitter)
        {
            if(speciesIndex<0 || speciesIndex>2)throw new ArgumentOutOfRangeException(nameof(speciesIndex));
            if(difficulty<1 || difficulty>3)throw new ArgumentOutOfRangeException(nameof(difficulty));
            if(jitter<0 || jitter>9)throw new ArgumentOutOfRangeException(nameof(jitter));
            int reward=speciesIndex==0 ? 7+(int)((level-1)*3f)+jitter
                : speciesIndex==1 ? 7+3*level-12+jitter : 11+4*level-28+jitter;
            return (int)(reward*(1f-0.05f*(difficulty-1)));
        }
    }
}
