using NUnit.Framework;
using UnityEngine;
using DinoCap.PortV2;

public sealed class Task027N254JungleResultsRepro
{
    [Test]
    public void JungleDeathDispatchCompletesAtTextRiseWithoutCityLifeEntry()
    {
        var p=new NativeDeathPresentation(0,0,false);
        for(int i=0;i<1000 && p.Active;i++)p.Tick(.01f);
        Assert.That(p.Active,Is.False);Assert.That(p.LivesVisible,Is.False);
        Assert.That(p.Text.Rect,Is.EqualTo(new Rect(306,30,412,156)));
    }
    [Test]
    public void DefaultSharedDeathRetainsCityLifeOwner()
    {
        var p=new NativeDeathPresentation(0,40);
        for(int i=0;i<1000 && !p.LivesVisible;i++)p.Tick(.01f);
        Assert.That(p.LivesVisible,Is.True);
        Assert.That(p.CurrentStage,Is.EqualTo(NativeDeathPresentation.Stage.LivesEnter));
    }
    [Test]
    public void NativeResultsAndDoneHaveOriginalRectangles()
    {
        Assert.That(NativeJungleResults.TimeTitle,Is.EqualTo(new Rect(150,399,306,36)));
        Assert.That(NativeJungleResults.KillValue,Is.EqualTo(new Rect(485,489,200,60)));
        Assert.That(NativeJungleResults.MoneyValue,Is.EqualTo(new Rect(485,590,200,60)));
        var done=NativeJungleResults.CreateDone();
        for(int i=0;i<40;i++)done.Tick(.01f);
        Assert.That(done.Active,Is.False);
        Assert.That(done.Rect,Is.EqualTo(new Rect(768.5f,630,183,60)));
    }
}
