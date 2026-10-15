using NUnit.Framework;
using DinoCap.PortV2;

public sealed class Task027N254JungleInventoryRepro
{
    static JungleDropSelection.Decision Weapon(int family,int tier)=>new JungleDropSelection.Decision
        {frame="native-fixture",kind=JunglePickupState.Kind.Weapon,family=family,tier=tier};
    static int[] Classes()=>new[]{3,-1,-1,-1,-1,-1,0};
    [TestCase(1,0,50)] [TestCase(1,1,36)] [TestCase(2,2,135)]
    [TestCase(3,3,120)] [TestCase(4,1,22)] [TestCase(5,2,75)]
    public void HigherFamilyUsesIncomingClassStockAndEquips(int family,int tier,int expected)
    {
        var classes=Classes();var stock=new int[6];
        var result=JunglePickupInventory.Apply(Weapon(family,tier),classes,stock,0,()=>throw new System.Exception("Unexpected random draw"));
        Assert.That(classes[family],Is.EqualTo(tier));Assert.That(stock[family],Is.EqualTo(expected));
        Assert.That(result.selectedFamily,Is.EqualTo(family));Assert.That(result.rebindPlayer,Is.True);
        Assert.That(result.itemAudio && result.buttonAudio,Is.True);Assert.That(result.ammoNotification,Is.False);
    }
    [Test]
    public void HigherFamilyAlreadyOwnedStillEquipsAndUsesIncomingClassZeroSet()
    {
        var classes=Classes();classes[1]=3;var stock=new int[6];stock[1]=700;
        var result=JunglePickupInventory.Apply(Weapon(1,0),classes,stock,0,null);
        Assert.That(classes[1],Is.EqualTo(3));Assert.That(stock[1],Is.EqualTo(50));
        Assert.That(result.selectedFamily,Is.EqualTo(1));Assert.That(result.classChanged,Is.False);
    }
    [Test]
    public void SameFamilyUpgradeRebindsWithoutAddingStockOrDrawingRandom()
    {
        var classes=Classes();classes[2]=0;var stock=new int[6];stock[2]=17;
        var result=JunglePickupInventory.Apply(Weapon(2,3),classes,stock,2,null);
        Assert.That(classes[2],Is.EqualTo(3));Assert.That(stock[2],Is.EqualTo(17));
        Assert.That(result.rebindPlayer,Is.True);Assert.That(result.buttonAudio,Is.False);
    }
    [Test]
    public void LowerFamilyUpgradePreservesSelectionAndAddsTruncatedStock()
    {
        var classes=Classes();classes[4]=0;var stock=new int[6];stock[4]=10;
        var result=JunglePickupInventory.Apply(Weapon(4,1),classes,stock,5,null);
        Assert.That(stock[4],Is.EqualTo(32));Assert.That(result.selectedFamily,Is.EqualTo(5));
        Assert.That(result.rebindPlayer,Is.False);Assert.That(result.buttonAudio,Is.True);
    }
    [TestCase(1)] [TestCase(3)]
    public void RedundantWeaponUsesAmmoRollWithoutNotification(int selected)
    {
        var classes=Classes();classes[1]=3;var stock=new int[6];int draws=0;
        var result=JunglePickupInventory.Apply(Weapon(1,1),classes,stock,selected,()=>{draws++;return int.MaxValue;});
        Assert.That(draws,Is.EqualTo(1));Assert.That(stock[1],Is.EqualTo(32));
        Assert.That(result.selectedFamily,Is.EqualTo(selected));Assert.That(result.ammoNotification,Is.False);
        Assert.That(result.rebindPlayer || result.buttonAudio,Is.False);
    }
    [Test]
    public void ActualAmmoPickupEmitsNotificationAndCapsStock()
    {
        var classes=Classes();var stock=new int[6];stock[3]=980;
        var decision=new JungleDropSelection.Decision {frame="Lbullet3Frame",kind=JunglePickupState.Kind.Ammo,family=3,tier=-1};
        var result=JunglePickupInventory.Apply(decision,classes,stock,0,()=>int.MaxValue);
        Assert.That(stock[3],Is.EqualTo(999));Assert.That(result.ammoAdded,Is.EqualTo(106));
        Assert.That(result.ammoNotification,Is.True);Assert.That(result.buttonAudio,Is.False);
    }
    [TestCase(0)] [TestCase(3)]
    public void MeleePickupOnlyUpgradesItsClass(int tier)
    {
        var classes=Classes();var stock=new int[6];stock[0]=999;
        var result=JunglePickupInventory.Apply(Weapon(6,tier),classes,stock,0,null);
        Assert.That(classes[6],Is.EqualTo(tier));Assert.That(result.selectedFamily,Is.EqualTo(0));
        Assert.That(stock[0],Is.EqualTo(999));Assert.That(result.buttonAudio || result.rebindPlayer,Is.False);
    }
}
