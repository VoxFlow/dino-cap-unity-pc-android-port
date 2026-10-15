using System;
using UnityEngine;

namespace DinoCap.PortV2
{
    // Native GO: sub_7EEC8 -> E9E2C/E8E48, then one upgrade pass per
    // floor(level/5). This reconstructs a preset, not a historical save.
    public sealed class CityGrindStartPreset
    {
        public readonly int[] tiers = {0,-1,-1,-1,-1,-1,0};
        public readonly int[] ammo = {200,0,0,0,0,0};
        public const float Health = 100f;
        public const int Lives = 5;
        public const string HealthPrefKey = "DC1_CityGrind_HP";
        public const string AmmoPrefPrefix = "DC1_CityGrind_Ammo_";
        public const string EquippedPrefKey = "DC1_CityGrind_Equipped";
        public const string BonusPrefPrefix = "DC1_CityGrind_Bonus_";

        public static CityGrindStartPreset ForLevel(int level)
        {
            if(level<1) throw new ArgumentOutOfRangeException(nameof(level));
            var result=new CityGrindStartPreset();
            // Seven passes saturate every real weapon.prop family. Further
            // native passes leave both tiers and stocks unchanged; this bound
            // limits computation, not selectable/continuing gameplay levels.
            int passes=Math.Min(level/5,7);
            for(int pass=0;pass<passes;pass++) {
                bool unlockedFirstLocked=false;
                for(int family=0;family<result.tiers.Length;family++) {
                    int tier=result.tiers[family];
                    if(tier>=0) tier++;
                    else {
                        if(!unlockedFirstLocked) tier++;
                        unlockedFirstLocked=true;
                    }
                    if(tier>=WeaponFamilyTable.TierCount[family])
                        result.tiers[family]=WeaponFamilyTable.TierCount[family]-1;
                    else {
                        result.tiers[family]=tier;
                        if(tier>=0 && family<result.ammo.Length)
                            result.ammo[family]=WeaponFamilyTable.AmmoCaps[family];
                    }
                }
            }
            return result;
        }

        public void WriteCommittedRun(int level)
        {
            DC1Preferences.SetInt("DC1_CityGrind_Level",level);
            DC1Preferences.SetInt(CityGridController.LivesPrefKey,Lives);
            DC1Preferences.SetInt("DC1_CityGrind_Cash",0);
            DC1Preferences.SetFloat(HealthPrefKey,Health);
            DC1Preferences.SetInt(EquippedPrefKey,0);
            for(int family=0;family<ammo.Length;family++) {
                DC1Preferences.SetInt("DC1_CityGrind_Tier_"+family,tiers[family]);
                DC1Preferences.SetInt(AmmoPrefPrefix+family,ammo[family]);
                DC1Preferences.SetInt(BonusPrefPrefix+family,0); //7EEC8 resets21BE18.
            }
            DC1Preferences.SetInt("DC1_CityGrind_Tier_G",tiers[6]);
            DC1Preferences.SetInt("DC1_CityGrind_OwnershipModelMigratedV1",1);
        }
    }
}
