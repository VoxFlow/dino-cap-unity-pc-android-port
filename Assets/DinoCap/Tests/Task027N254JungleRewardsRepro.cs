using NUnit.Framework;
using DinoCap.PortV2;

public sealed class Task027N254JungleRewardsRepro
{
    // Source ARM F32 conversion can round high rand31 values to2^31.
    // Rare selector==scale branches must not be erased as unreachable.
    [TestCase(0,2,0)] [TestCase(1073741824,2,1)]
    [TestCase(2147483583,1,0)] [TestCase(2147483584,1,1)]
    [TestCase(2147483647,2,2)] [TestCase(2147483647,100,100)]
    public void N254_NativeRandomPreservesFloatEndpoint(int raw,float scale,int expected)
    {
        Assert.That(JunglePickupRewards.TruncateNativeScaledRand(raw,scale),Is.EqualTo(expected));
    }
    // Raw31 golden inputs yield native truncated percent0,50,75,100.
    [TestCase(0,12,24,36,72)] [TestCase(1,8,16,24,32)]
    [TestCase(2,15,30,50,100)] [TestCase(3,14,28,56,106)]
    [TestCase(4,5,10,15,30)] [TestCase(5,10,30,50,100)]
    public void N254_NativeAmmoBuckets(int family,int low,int middle,int high,int top)
    {
        Assert.That(JunglePickupRewards.AmmoPickupAmount(family,0),Is.EqualTo(low));
        Assert.That(JunglePickupRewards.AmmoPickupAmount(family,1073741824),Is.EqualTo(middle));
        Assert.That(JunglePickupRewards.AmmoPickupAmount(family,1610612736),Is.EqualTo(high));
        Assert.That(JunglePickupRewards.AmmoPickupAmount(family,int.MaxValue),Is.EqualTo(top));
    }
    [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
    public void N254_NativeAmmoPickupCapsAt999(int family)
    {
        Assert.That(JunglePickupRewards.ApplyAmmoPickup(998,family,int.MaxValue),Is.EqualTo(999));
    }
}
