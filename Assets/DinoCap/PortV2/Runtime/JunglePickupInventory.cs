using System;

namespace DinoCap.PortV2
{
    // iPad1B13C branch graph, with raw call arguments at1B4B4/1B500/
    // 1B50E and1B6A0. Operates on the mode-owned native inventory arrays.
    public static class JunglePickupInventory
    {
        static readonly int[] InitialStock={200,50,150,200,30,250};
        static readonly int[] WeaponStock={48,24,90,80,15,50};
        public struct Outcome
        {
            public int selectedFamily, ammoAdded;
            public bool classChanged, rebindPlayer, itemAudio, buttonAudio, ammoNotification;
        }
        public static Outcome Apply(JungleDropSelection.Decision pickup,int[] classes,int[] stock,int selectedFamily,Func<int> nextRaw31)
        {
            if(classes==null || classes.Length!=7 || stock==null || stock.Length<6)
                throw new ArgumentException("Native inventory requires seven classes and six ranged stocks.");
            if(selectedFamily<0 || selectedFamily>6)throw new ArgumentOutOfRangeException(nameof(selectedFamily));
            if(!pickup.HasDrop || pickup.kind==JunglePickupState.Kind.Health)return new Outcome {selectedFamily=selectedFamily};
            int f=pickup.family;
            if(f<0 || f>6 || pickup.kind==JunglePickupState.Kind.Ammo && f==6)
                throw new ArgumentOutOfRangeException(nameof(pickup));
            var result=new Outcome {selectedFamily=selectedFamily,itemAudio=true};
            bool upgrade=pickup.tier>classes[f];
            if(pickup.kind==JunglePickupState.Kind.Weapon)
            {
                if(f==6)
                {
                    if(upgrade){classes[f]=pickup.tier;result.classChanged=true;}
                    return result;
                }
                if(f>selectedFamily)
                {
                    if(upgrade){classes[f]=pickup.tier;result.classChanged=true;}
                    ApplyWeaponStock(stock,f,pickup.tier,ref result);
                    result.selectedFamily=f;result.rebindPlayer=true;
                    return result;
                }
                if(upgrade)
                {
                    classes[f]=pickup.tier;result.classChanged=true;
                    if(f==selectedFamily)result.rebindPlayer=true;
                    else ApplyWeaponStock(stock,f,pickup.tier,ref result);
                    return result;
                }
            }
            if(nextRaw31==null)throw new ArgumentNullException(nameof(nextRaw31));
            result.ammoAdded=JunglePickupRewards.AmmoPickupAmount(f,nextRaw31());
            stock[f]=Math.Min(999,stock[f]+result.ammoAdded);
            // R3=1 for a floating ammo pickup;0 for redundant weapon ammo.
            result.ammoNotification=pickup.kind==JunglePickupState.Kind.Ammo;
            return result;
        }
        static void ApplyWeaponStock(int[] stock,int family,int tier,ref Outcome result)
        {
            int previous=stock[family];
            // Native1B780 R2=incoming class, R3=1: class0 SET; other
            // classes ADD trunc(base*1.5). Never use old class as predicate.
            stock[family]=tier==0 ? InitialStock[family]
                : Math.Min(999,stock[family]+(int)(WeaponStock[family]*1.5f));
            result.ammoAdded=stock[family]-previous;
            result.buttonAudio=true;
        }
    }
}
