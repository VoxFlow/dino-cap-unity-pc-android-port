using System.Collections.Generic;
using NUnit.Framework;
using DinoCap.PortV2;

public sealed class Task027N254JungleDropRepro
{
    const int Half=1073741824, Top=int.MaxValue;
    // Golden label destinations from19E60, including endpoint-only choices.
    [TestCase(0,1,0,0,"Lb1Frame",1,0)]
    [TestCase(30,1,0,Half,"Lb2Frame",1,1)]
    [TestCase(30,1,0,Top,"Lc1Frame",2,0)]
    [TestCase(31,1,0,0,"Lb3Frame",1,2)]
    [TestCase(60,1,0,Top,"Lg2Frame",6,1)]
    [TestCase(61,1,0,0,"Lb4Frame",1,3)]
    [TestCase(150,1,0,Half,"Lc3Frame",2,2)]
    [TestCase(150,1,0,Top,"Lg3Frame",6,2)]
    [TestCase(61,2,0,0,"Lc3Frame",2,2)]
    [TestCase(150,2,0,Half,"Lc4Frame",2,3)]
    [TestCase(150,2,0,Top,"Ld2Frame",3,1)]
    [TestCase(151,1,0,0,"Le1Frame",4,0)]
    [TestCase(151,1,0,Top,"Lf1Frame",5,0)]
    [TestCase(151,2,0,0,"Ld4Frame",3,3)]
    [TestCase(151,2,0,Top,"Le2Frame",4,1)]
    [TestCase(151,3,0,0,"Le2Frame",4,1)]
    [TestCase(151,3,0,Top,"Lf3Frame",5,2)]
    [TestCase(151,4,0,Top,"Lf2Frame",5,1)]
    [TestCase(0,1,Half,0,"Lbullet1Frame",1,-1)]
    [TestCase(0,1,Half,Top,"Lbullet2Frame",2,-1)]
    [TestCase(31,1,Half,Top,"Lbullet3Frame",3,-1)]
    [TestCase(61,1,Half,Top,"Lbullet4Frame",4,-1)]
    [TestCase(61,2,Half,0,"Lbullet1Frame",1,-1)]
    [TestCase(151,1,Half,0,"Lbullet4Frame",4,-1)]
    [TestCase(151,1,Half,Half,"Lbullet3Frame",3,-1)]
    [TestCase(151,2,Half,0,"Lbullet3Frame",3,-1)]
    [TestCase(151,2,Half,Top,"Lbullet5Frame",5,-1)]
    [TestCase(151,3,Half,Top,"Lbullet4Frame",4,-1)]
    [TestCase(151,4,Half,Top,"Lbullet4Frame",4,-1)]
    public void N254_NativeDropBranchGoldenDestinations(int time,int type,int category,int nested,string frame,int family,int tier)
    {
        var draws=new Queue<int>(new[]{0,category,nested});
        var d=JungleDropSelection.Roll(time,type,()=>draws.Dequeue());
        Assert.That(d.frame,Is.EqualTo(frame));Assert.That(d.family,Is.EqualTo(family));Assert.That(d.tier,Is.EqualTo(tier));
        Assert.That(draws,Is.Empty,"Consume original native rand calls, including fixed-destination ammo branches.");
    }
    [TestCase(0,2)] [TestCase(31,2)] [TestCase(61,3)] [TestCase(150,4)]
    public void N254_IneligibleNativeDinoDropDoesNotDraw(int time,int type)
    {
        Assert.That(JungleDropSelection.Roll(time,type,()=>throw new AssertionException("Native branch does not call rand")).HasDrop,Is.False);
    }
    [TestCase(0,1)] [TestCase(31,1)] [TestCase(61,1)] [TestCase(61,2)]
    [TestCase(151,1)] [TestCase(151,2)] [TestCase(151,3)] [TestCase(151,4)]
    public void N254_NativeFailedChanceConsumesOnlyOneDraw(int time,int type)
    {
        int calls=0;var d=JungleDropSelection.Roll(time,type,()=>{calls++;return Top;});
        Assert.That(d.HasDrop,Is.False);Assert.That(calls,Is.EqualTo(1));
    }
    [Test]
    public void N254_HealthEndpointBranchHasNoThirdDraw()
    {
        var draws=new Queue<int>(new[]{0,Top});
        var d=JungleDropSelection.Roll(0,1,()=>draws.Dequeue());
        Assert.That(d.frame,Is.EqualTo("LxuepingFrame"));Assert.That(d.kind,Is.EqualTo(JunglePickupState.Kind.Health));
        Assert.That(draws,Is.Empty);
    }
}
