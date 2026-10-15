using System;

namespace DinoCap.PortV2
{
    // iPad19E60 actual branch/label graph. Uses raw rand31 draws with the
    // native F32 conversion, including rare selector==scale endpoints.
    // No equal-weight replacement pool or unreachable190 interval.
    public static class JungleDropSelection
    {
        public struct Decision
        {
            public string frame;
            public JunglePickupState.Kind kind;
            public int family, tier;
            public bool HasDrop => frame!=null;
        }
        static Decision Frame(string name)
        {
            if(name=="LxuepingFrame")return new Decision {frame=name,kind=JunglePickupState.Kind.Health,family=-1,tier=-1};
            if(name.StartsWith("Lbullet"))return new Decision {frame=name,kind=JunglePickupState.Kind.Ammo,family=name[7]-'0',tier=-1};
            return new Decision {frame=name,kind=JunglePickupState.Kind.Weapon,family=name[1]-'a',tier=name[2]-'1'};
        }
        public static Decision Roll(int seconds,int dinoType,Func<int> nextRaw31)
        {
            if(nextRaw31==null)throw new ArgumentNullException(nameof(nextRaw31));
            if(dinoType<1 || dinoType>4 || seconds<0)return default;
            int tier=seconds>150 ? 3 : seconds>60 ? 2 : seconds>30 ? 1 : 0;
            if(tier<=1 && dinoType!=1 || tier==2 && dinoType>2)return default;
            int chance=tier==0 ? 22 : tier==1 ? 23 : tier==2 ? (dinoType==1 ? 20 : 23)
                : dinoType==1 ? 28 : dinoType==2 ? 35 : dinoType==3 ? 40 : 32;
            int Draw(float scale)=>JunglePickupRewards.TruncateNativeScaledRand(nextRaw31(),scale);
            if(Draw(100)>=chance)return default;
            int category=Draw(2);
            if(category==2)return Frame("LxuepingFrame");
            if(category==1)
            {
                if(tier==0 || tier==2 && dinoType==2)
                    return Frame(Draw(1)==0 ? "Lbullet1Frame" : "Lbullet2Frame");
                if(tier==1)return Frame(new[]{"Lbullet1Frame","Lbullet2Frame","Lbullet3Frame"}[Draw(2)]);
                if(tier==2)return Frame(new[]{"Lbullet1Frame","Lbullet2Frame","Lbullet3Frame","Lbullet4Frame"}[Draw(3)]);
                if(dinoType>=3)
                {
                    // LABEL54 compares a nonnegative normalized int to−1;
                    // the native result is always bullet4, but draw consumed.
                    Draw(1);return Frame("Lbullet4Frame");
                }
                int ammo=Draw(2);
                return Frame(dinoType==1 ? new[]{"Lbullet4Frame","Lbullet3Frame","Lbullet5Frame"}[ammo]
                    : new[]{"Lbullet3Frame","Lbullet4Frame","Lbullet5Frame"}[ammo]);
            }
            if(tier==0)return Frame(new[]{"Lb1Frame","Lb2Frame","Lc1Frame"}[Draw(2)]);
            if(tier==1)return Frame(new[]{"Lb3Frame","Lc2Frame","Lg2Frame"}[Draw(2)]);
            if(tier==2)
                return Frame(dinoType==1 ? new[]{"Lb4Frame","Lc3Frame","Ld1Frame","Lg3Frame"}[Draw(3)]
                    : new[]{"Lc3Frame","Lc4Frame","Ld2Frame"}[Draw(2)]);
            if(dinoType==1)return Frame(Draw(1)==0 ? "Le1Frame" : "Lf1Frame");
            if(dinoType==2)return Frame(new[]{"Ld4Frame","Le1Frame","Le2Frame"}[Draw(2)]);
            if(dinoType==3)return Frame(new[]{"Le2Frame","Lf2Frame","Lf3Frame"}[Draw(2)]);
            return Frame(new[]{"Le1Frame","Le2Frame","Lf1Frame","Lf2Frame"}[Draw(3)]);
        }
    }
}
