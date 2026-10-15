using NUnit.Framework;
using UnityEngine;
using DinoCap.PortV2;

public sealed class Task027N254JungleProjectileRepro
{
    [TestCase(true,125f,280f)]
    [TestCase(false,75f,-300f)]
    public void LaunchAndFirstPhysicsMatchNativeConstructor(bool right,float x,float vx)
    {
        var p=new NativeJungleProjectile(new Vector2(100,136),right);
        Assert.That(p.Position,Is.EqualTo(new Vector2(x,-9)));
        p.AdvancePhysics(.1f);Assert.That(p.Position.x,Is.EqualTo(x));
        p.UpdateVelocity(.1f);Assert.That(p.Velocity,Is.EqualTo(new Vector2(vx,-5)));
        p.AdvancePhysics(.1f);Assert.That(p.Position.y,Is.EqualTo(-9.5f));
        p.UpdateVelocity(.1f);Assert.That(p.Velocity.y,Is.EqualTo(-15));
    }
    [Test]
    public void CollisionHasStrictHorizontalGateAndInvulnerabilityButNoPlayerY()
    {
        var p=new NativeJungleProjectile(new Vector2(100,136),true);
        Assert.That(p.InHitRange(145,false),Is.False);
        Assert.That(p.InHitRange(144.999f,false),Is.True);
        Assert.That(p.InHitRange(125,true),Is.False);
        p.Hit();Assert.That(p.InHitRange(125,false),Is.False);
    }
    [Test]
    public void GroundBoundaryAdds180AndMarkedFrame18Retires()
    {
        var p=new NativeJungleProjectile(new Vector2(100,85),true);
        Assert.That(p.Position.y,Is.EqualTo(-60));
        Assert.That(p.InHitRange(125,false),Is.False);
        Assert.That(p.TryGround(),Is.True);
        Assert.That(p.Position.y,Is.EqualTo(120));
        Assert.That(p.Velocity,Is.EqualTo(Vector2.zero));
        p.GroundFrame(17);Assert.That(p.Retired,Is.False);
        p.GroundFrame(18);Assert.That(p.Retired,Is.True);
    }
}
