using NUnit.Framework;
using DinoCap.PortV2;

public sealed class Task027N254JungleDifficultyRepro
{
    // Native18858 common tail: HP/ATK*(1+.1*(d-1));
    // EXP truncated after F32*(1-.05*(d-1)), SPD/FRQ/ADIS untouched.
    [TestCase(0,1,2,14.3f,8.8f,19)]
    [TestCase(0,1,3,15.6f,9.6f,18)]
    [TestCase(151,1,2,101.2f,33,121)]
    [TestCase(151,2,3,68.4f,27,126)]
    [TestCase(151,3,2,139.7f,49.5f,209)]
    [TestCase(151,4,3,59.4f,12,87)]
    public void NativeCommonTailScalesJungleSpawn(int time,int type,int difficulty,float hp,float attack,int reward)
    {
        var p=JungleBlitzProgression.Load();
        var s=p.QuerySpawn(time,type,difficulty);
        var b=p.Query(time,type);
        Assert.That(s.hp,Is.EqualTo(hp).Within(.0001f));
        Assert.That(s.attack,Is.EqualTo(attack).Within(.0001f));
        Assert.That(s.reward,Is.EqualTo(reward));
        Assert.That(s.speed,Is.EqualTo(b.speed));
        Assert.That(s.frequency,Is.EqualTo(b.frequency));
        Assert.That(s.attackDistance,Is.EqualTo(b.attackDistance));
    }
    [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
    public void DifficultyOnePreservesElapsedStage(int type)
    {
        var p=JungleBlitzProgression.Load();
        var s=p.QuerySpawn(180,type,1);var b=p.Query(180,type);
        Assert.That(s.hp,Is.EqualTo(b.hp));
        Assert.That(s.attack,Is.EqualTo(b.attack));
        Assert.That(s.reward,Is.EqualTo(b.reward));
    }
}
