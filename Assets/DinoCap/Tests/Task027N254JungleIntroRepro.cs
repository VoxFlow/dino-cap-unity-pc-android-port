using NUnit.Framework;
using DinoCap.PortV2;

public sealed class Task027N254JungleIntroRepro
{
    [Test]
    public void JungleStartUsesNativeInitializerRectAndMoveControlClamp()
    {
        var intro=NativeLevelIntro.CreateJungle();
        intro.Tick(.2f);intro.Tick(.05f);
        Assert.That(intro.CurrentStage,Is.EqualTo(NativeLevelIntro.Stage.LabelEnter));
        Assert.That(intro.Label.Value.x,Is.EqualTo(1534));
        Assert.That(intro.Label.Value.y,Is.EqualTo(347.5f));
        Assert.That(intro.Label.Value.z,Is.EqualTo(300));
        Assert.That(intro.Label.Value.w,Is.EqualTo(101));
        intro.Tick(.3f);
        Assert.That(intro.Label.Value.x,Is.EqualTo(408));
        intro.Tick(.01f);
        Assert.That(intro.CurrentStage,Is.EqualTo(NativeLevelIntro.Stage.Hold));
        Assert.That(intro.Label.Rect.x,Is.EqualTo(360));
        Assert.That(intro.Label.Rect.y,Is.EqualTo(297));
    }
    [Test]
    public void JungleControlsWaitForBandLeaveCompletion()
    {
        var intro=NativeLevelIntro.CreateJungle();
        for(int i=0;i<1000 && intro.CurrentStage!=NativeLevelIntro.Stage.BandLeave;i++)intro.Tick(.01f);
        Assert.That(intro.CurrentStage,Is.EqualTo(NativeLevelIntro.Stage.BandLeave));
        Assert.That(intro.Active,Is.True);
        for(int i=0;i<1000 && intro.Active;i++)intro.Tick(.01f);
        Assert.That(intro.CurrentStage,Is.EqualTo(NativeLevelIntro.Stage.Complete));
        Assert.That(intro.Band.Value.w,Is.EqualTo(0));
    }
    [Test]
    public void CityRetainsItsNativeLevelControlExtrapolationAndGeometry()
    {
        var intro=new NativeLevelIntro();
        intro.Tick(.2f);intro.Tick(.05f);
        Assert.That(intro.Label.Value.x,Is.EqualTo(1547));
        Assert.That(intro.Label.Value.z,Is.EqualTo(446));
        intro.Tick(.3f);
        Assert.That(intro.Label.Value.x,Is.LessThan(421));
    }
}
