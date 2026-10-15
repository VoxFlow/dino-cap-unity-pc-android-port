using NUnit.Framework;
using DinoCap.PortV2;

public sealed class Task027N254JungleProgressionRepro
{
    // Golden strict-boundary vectors from18858's nested comparisons, not a
    // sorted interpretation of the original XML's0,30,60,190,150,200 order.
    [TestCase(0,0)] [TestCase(30,0)] [TestCase(31,1)]
    [TestCase(60,1)] [TestCase(61,2)] [TestCase(150,2)]
    [TestCase(151,4)] [TestCase(190,4)] [TestCase(191,4)] [TestCase(201,4)]
    public void N254_NativeFileOrderStrictIntervals(int time,int expected)
    {
        Assert.That(JungleBlitzProgression.Load().IntervalIndex(time),Is.EqualTo(expected));
    }
    // Native magic signed division88888889/ASR4 yields q5 at151..179,
    // q6 at180; these vectors distinguish the superseded /9 formula.
    [TestCase(151,1,92,128,30,50,190)]
    [TestCase(179,1,92,128,30,50,190)]
    [TestCase(180,1,99,133,33,50,190)]
    [TestCase(151,2,57,140,22.5f,200,190)]
    [TestCase(180,2,62,150,25,200,190)]
    [TestCase(151,3,127,220,45,30,280)]
    [TestCase(180,3,139,240,50,30,280)]
    [TestCase(151,4,49.5f,97,10,50,200)]
    [TestCase(180,4,51,99,10,50,200)]
    public void N254_Post150NativeIPadStatVectors(int time,int type,float hp,int reward,float attack,float speed,float distance)
    {
        var s=JungleBlitzProgression.Load().Query(time,type);
        Assert.That(s.hp,Is.EqualTo(hp));Assert.That(s.reward,Is.EqualTo(reward));
        Assert.That(s.attack,Is.EqualTo(attack));Assert.That(s.speed,Is.EqualTo(speed));
        Assert.That(s.attackDistance,Is.EqualTo(distance));
    }
    [TestCase(0)] [TestCase(31)] [TestCase(61)] [TestCase(151)] [TestCase(10150)]
    public void N254_K4RetainsNativeLongTimerRatherThanDisabledFlag(int time)
    {
        Assert.That(JungleBlitzProgression.Load().Query(time,4).frequency,Is.EqualTo(9999));
    }
    [Test]
    public void N254_InitialTableIsOriginalIPadNotCityScaling()
    {
        var s=JungleBlitzProgression.Load().Query(0,1);
        Assert.That(s.hp,Is.EqualTo(13));Assert.That(s.reward,Is.EqualTo(20));
        Assert.That(s.speed,Is.EqualTo(50));Assert.That(s.attack,Is.EqualTo(8));
        Assert.That(s.attackDistance,Is.EqualTo(200));Assert.That(s.frequency,Is.EqualTo(1.3f));
    }
}
