using NUnit.Framework;
using DinoCap.PortV2;

public sealed class Task027N254JunglePlacementRepro
{
    [Test]
    public void OriginalJumpBoundsAreNativeFrameAggregates()
    {
        var b=JungleSpawnPlacement.LoadNativeJumpBounds();
        Assert.That(b.minX,Is.EqualTo(-139.5f));Assert.That(b.maxX,Is.EqualTo(140.5f));
    }
    // Native: player +0x60 == 1 means FACING LEFT (sub_65D1C: move-left sets it with a negative x velocity), and jump-ins spawn on the
    // side the player FACES (left edge moving +1024 when facing left; right edge moving -1024 when facing right).
    [TestCase(1,true,1163.5f,-1024)] [TestCase(1,false,-140.5f,1024)]
    [TestCase(2,true,1163.5f,-1024)] [TestCase(2,false,-140.5f,1024)]
    public void JumpSideUsesFacingWithExactNativeBounds(int type,bool right,float x,float velocity)
    {
        int draws=0;
        var p=JungleSpawnPlacement.Decide(type,0,0,1024,0,right,50,JungleSpawnPlacement.LoadNativeJumpBounds(),()=>{draws++;return int.MaxValue;});
        Assert.That(p.jump,Is.True);Assert.That(p.x,Is.EqualTo(x));Assert.That(p.y,Is.EqualTo(136));
        Assert.That(p.velocityX,Is.EqualTo(velocity));Assert.That(draws,Is.EqualTo(1));
    }
    [TestCase(3,-10,true,-200)] [TestCase(3,10,false,1224)]
    [TestCase(4,-10,true,-200)] [TestCase(4,10,false,1224)]
    public void WalkOnlyTypesUsePhysicsVelocityWithoutEntryRoll(int type,float playerVelocity,bool facing,float x)
    {
        var p=JungleSpawnPlacement.Decide(type,0,0,1024,playerVelocity,false,30,default,()=>throw new System.Exception("Unexpected draw"));
        Assert.That(p.jump,Is.False);Assert.That(p.facingRight,Is.EqualTo(facing));Assert.That(p.x,Is.EqualTo(x));
        Assert.That(p.velocityX,Is.EqualTo(facing?30:-30));
    }
    [TestCase(1610612736,false)] [TestCase(1626896768,true)]
    public void EntryUses99ScaleBeforeTruncation(int raw,bool jump)
    {
        var p=JungleSpawnPlacement.Decide(1,0,0,1024,10,true,50,JungleSpawnPlacement.LoadNativeJumpBounds(),()=>raw);
        Assert.That(p.jump,Is.EqualTo(jump));
    }
    [TestCase(0,false)] [TestCase(1073741824,true)] [TestCase(2147483647,true)]
    public void StationaryWalkTiebreakHasNative50Boundary(int raw,bool left)
    {
        var p=JungleSpawnPlacement.Decide(3,0,100,1365,0,true,30,default,()=>raw);
        Assert.That(p.x,Is.EqualTo(left?-100:1665));
    }
    [TestCase(4)] [TestCase(6)]
    public void NativePlayerStatesRejectSpawnWithoutRandomDraw(int state)
    {
        var p=JungleSpawnPlacement.Decide(1,state,0,1024,0,true,50,default,()=>throw new System.Exception("Unexpected draw"));
        Assert.That(p.admitted,Is.False);
    }
}
