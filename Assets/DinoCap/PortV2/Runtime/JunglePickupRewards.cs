using System;

namespace DinoCap.PortV2
{
    public static class JunglePickupRewards
    {
        // 1B95C native iPad ammo roll, thresholds40/70/90 and cap999.
        // Preserve D's106 top bucket and the shared C/F middle buckets.
        static readonly int[,] Ammo={{12,24,36,72},{8,16,24,32},
            {15,30,50,100},{14,28,56,106},{5,10,15,30},{10,30,50,100}};
        public static int TruncateNativeScaledRand(int raw31,float scale)
        {
            if(raw31<0)throw new ArgumentOutOfRangeException(nameof(raw31));
            float normalized=raw31*(1f/2147483648f);
            float scaled=normalized*scale;
            return (int)scaled;
        }
        public static int AmmoPickupAmount(int family,int raw31)
        {
            if(family<0 || family>5)throw new ArgumentOutOfRangeException(nameof(family));
            int percent=TruncateNativeScaledRand(raw31,100f);
            int bucket=percent<40 ? 0 : percent<70 ? 1 : percent<90 ? 2 : 3;
            return Ammo[family,bucket];
        }
        public static int ApplyAmmoPickup(int currentStock,int family,int raw31)
            => Math.Min(999,currentStock+AmmoPickupAmount(family,raw31));
    }
}
