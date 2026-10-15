using System;

namespace DinoCap.PortV2
{
    public struct DC1WeaponRow
    {
        public string name;
        public float atkMin,atkMax,atkDis;
        public bool group;

        public DC1WeaponRow(string n,float mn,float mx,float dis,bool g)
        {
            name=n; atkMin=mn; atkMax=mx; atkDis=dis; group=g;
        }
    }

    /// <summary>
    /// Task027/027K source rows for implementation-safe A-F gameplay.
    /// iPad A/B/C/D range is the proven 1024 source units.
    /// </summary>
    public static class WeaponFamilyTable
    {
        public static readonly DC1WeaponRow[][] Rows =
        {
            new [] {
                new DC1WeaponRow("A1",1,2,1024,false),
                new DC1WeaponRow("A2",2,3,1024,false),
                new DC1WeaponRow("A3",3,4,1024,false),
                new DC1WeaponRow("A4",4,5,1024,false)
            },
            new [] {
                new DC1WeaponRow("B1",4,7,1024,false),
                new DC1WeaponRow("B2",7,11,1024,false),
                new DC1WeaponRow("B3",13,16,1024,false),
                new DC1WeaponRow("B4",19,25,1024,false)
            },
            new [] {
                new DC1WeaponRow("C1",3,5,1024,false),
                new DC1WeaponRow("C2",4,6,1024,false),
                new DC1WeaponRow("C3",5,7,1024,false),
                new DC1WeaponRow("C4",6,8,1024,false)
            },
            new [] {
                new DC1WeaponRow("D1",5,7,1024,false),
                new DC1WeaponRow("D2",6,9,1024,false),
                new DC1WeaponRow("D3",9,10,1024,false),
                new DC1WeaponRow("D4",11,15,1024,false)
            },
            new [] {
                new DC1WeaponRow("E1",25,35,220,true),
                new DC1WeaponRow("E2",35,40,310,true)
            },
            new [] {
                // Task027K iPad source rows. Family F tier field is a
                // 0/1/2 MODE selector, not a four-tier firearm progression.
                new DC1WeaponRow("F1",9,12,1024,true),
                new DC1WeaponRow("F2",10,14,310,true),
                new DC1WeaponRow("F3",3,6,1024,true)
            }
        };

        public static readonly int[] AmmoCaps={200,50,150,200,30,250};

        public static readonly DC1WeaponRow[] MeleeRows={
            new DC1WeaponRow("G1",2,3,200,true),
            new DC1WeaponRow("G2",3,5,200,true),
            new DC1WeaponRow("G3",4,7,200,true),
            new DC1WeaponRow("G4",6,9,200,true)
        };

        /// <summary>
        /// Task027L.4.7 PHASE B — source-faithful unlock thresholds from
        /// Data/Image/Conf/iPad/weapon.prop (the SubWeapon `exp` attribute).
        /// This is an accumulated-score / EXP gate, NOT a player-level gate.
        /// A1 and G1 are free (exp 0). weapon.prop states no previous-tier
        /// prerequisite; a tier is available when the run/profile EXP total
        /// reaches its threshold. Indexed [family 0..6][tier].
        /// </summary>
        public static readonly int[][] UnlockExp =
        {
            new []{   0, 300,  550,  900},   // A
            new []{ 350, 800, 1600, 2900},   // B
            new []{ 600,1000, 1500, 2300},   // C
            new []{1700,2400, 3300, 4800},   // D
            new []{4000,6000},               // E (2 tiers only)
            new []{2500,3700, 5500},         // F (3 mode selectors)
            new []{   0, 650, 1500, 2600}    // G
        };

        /// <summary>Tier count that actually exists per family (weapon.prop / Task027J1).</summary>
        public static readonly int[] TierCount = {4,4,4,4,2,3,4};

        public static DC1WeaponRow Get(int family,int tier)
        {
            return Rows[family][tier];
        }

        //10554 uses live21BE18 bonus plus weaponClass+28 table tier.
        //Return a value copy: shared authored rows remain immutable.
        public static DC1WeaponRow WithNativeBonus(int family,int tier,int bonus)
        {
            var row=Get(family,tier);
            if(bonus<1)return row;
            int n=bonus+tier;
            float minimum=0,spread=0;
            switch(family) {
                case 0:minimum=n+3;spread=n<10?2:1;break;
                case 1:minimum=1.5f*(n-1)+19;spread=n<8?3:2;break;
                case 2:minimum=n+5;spread=n<10?2:1;break;
                case 3:minimum=3*n+8;spread=n<10?3:2;break;
                case 4:minimum=5*n+30;spread=n<10?5:3;break;
                case 5:minimum=3*n;spread=n<10?3:2;break;
            }
            row.atkMin=minimum;row.atkMax=minimum+spread;return row;
        }

        public static int UnlockExpFor(int family,int tier)
        {
            if(family<0 || family>=UnlockExp.Length) return int.MaxValue;
            int[] fam=UnlockExp[family];
            if(tier<0 || tier>=fam.Length) return int.MaxValue;
            return fam[tier];
        }

        public static string FamilyLetter(int family)
        {
            return ((char)('A'+family)).ToString();
        }
    }
}
