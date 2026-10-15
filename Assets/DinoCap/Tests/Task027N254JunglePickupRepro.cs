using NUnit.Framework;
using DinoCap.PortV2;

public sealed class Task027N254JunglePickupRepro
{
    [TestCase(-80,false)] [TestCase(-79.99f,true)] [TestCase(0,true)]
    [TestCase(79.99f,true)] [TestCase(80,false)]
    public void N254_PickupUsesStrictNativeHorizontalRange(float offset,bool expected)
    {
        var p=new JunglePickupState(JunglePickupState.Kind.Ammo,100,50);
        Assert.That(p.InPickupRange(100+offset),Is.EqualTo(expected));
        Assert.That(p.SpawnY,Is.EqualTo(70));
    }
    [Test]
    public void N254_BobDiscardsOvershootAndRetainsVelocity()
    {
        var p=new JunglePickupState(JunglePickupState.Kind.Weapon,0,0);
        p.Tick(.01f);Assert.That(p.BobPhase,Is.Zero);Assert.That(p.VelocityY,Is.Zero);
        p.Tick(.5f);Assert.That(p.VelocityY,Is.EqualTo(20));
        p.Tick(.25f);Assert.That(p.VelocityY,Is.EqualTo(-20));
        p.Tick(.25f);Assert.That(p.BobPhase,Is.EqualTo(1));Assert.That(p.VelocityY,Is.EqualTo(-20));
        p.Tick(.1f);Assert.That(p.BobPhase,Is.Zero);Assert.That(p.VelocityY,Is.EqualTo(-20));
        p.Tick(.01f);Assert.That(p.VelocityY,Is.EqualTo(20));
    }
    [TestCase(JunglePickupState.Kind.Ammo)]
    [TestCase(JunglePickupState.Kind.Weapon)]
    [TestCase(JunglePickupState.Kind.Health)]
    public void N254_UncollectedFirstTimerIsTenSeconds(JunglePickupState.Kind kind)
    {
        var p=new JunglePickupState(kind,0,0);p.Tick(9);
        Assert.That(p.Active,Is.True);p.Tick(1);Assert.That(p.Active,Is.False);
        Assert.That(p.InPickupRange(0),Is.False);
    }
    [TestCase(20,45)] [TestCase(90,100)] [TestCase(100,100)]
    public void N254_HealthAdds25OnceThenRisesForFiveSeconds(float hp,float expected)
    {
        var p=new JunglePickupState(JunglePickupState.Kind.Health,0,0);
        Assert.That(p.TryCollectHealth(0,ref hp),Is.True);Assert.That(hp,Is.EqualTo(expected));
        Assert.That(p.TryCollectHealth(0,ref hp),Is.False);
        Assert.That(p.HealIndicator,Is.True);Assert.That(p.VelocityY,Is.EqualTo(100));
        p.Tick(4);Assert.That(p.Active,Is.True);p.Tick(1);Assert.That(p.Active,Is.False);
    }
    [TestCase(JunglePickupState.Kind.Ammo)] [TestCase(JunglePickupState.Kind.Weapon)]
    public void N254_InventoryCollectionRetiresWorldPickupOnce(JunglePickupState.Kind kind)
    {
        var p=new JunglePickupState(kind,0,0);
        Assert.That(p.FinishInventoryCollection(80),Is.False);
        Assert.That(p.FinishInventoryCollection(0),Is.True);
        Assert.That(p.FinishInventoryCollection(0),Is.False);
    }
}
