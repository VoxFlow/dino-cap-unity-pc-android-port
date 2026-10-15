using System.Collections.Generic;
using NUnit.Framework;
using DinoCap.PortV2;

public sealed class Task027N254JungleScheduleRepro
{
    [Test]
    public void StartAndFirstSecondRearmAreDistinctNativeAdmissions()
    {
        int serial=0;var arms=new List<float>();
        var schedule=new JungleSpawnSchedule(JungleBlitzProgression.Load(),(type,delay)=>{arms.Add(delay);return ++serial;});
        schedule.OnSecond();Assert.That(schedule.ElapsedSeconds,Is.EqualTo(0));
        schedule.Start();int old=schedule.HandleForType(1);
        Assert.That(arms.Count,Is.EqualTo(4));Assert.That(arms[0],Is.EqualTo(1.3f));
        schedule.OnSecond();Assert.That(arms.Count,Is.EqualTo(8));
        int calls=0;Assert.That(schedule.OnDinoTimer(old,_=>calls++),Is.False);
        Assert.That(calls,Is.EqualTo(0));
        Assert.That(schedule.OnDinoTimer(schedule.HandleForType(1),_=>calls++),Is.True);
        Assert.That(calls,Is.EqualTo(1));Assert.That(arms.Count,Is.EqualTo(9));
    }
    [TestCase(30,8)] [TestCase(31,12)] [TestCase(60,12)]
    [TestCase(61,16)] [TestCase(150,16)] [TestCase(151,20)]
    [TestCase(190,20)] [TestCase(201,20)]
    public void StrictIntervalTransitionsRearmOnceInOriginalFileOrder(int seconds,int expectedArms)
    {
        int serial=0;
        var schedule=new JungleSpawnSchedule(JungleBlitzProgression.Load(),(_,__)=>++serial);
        schedule.Start();for(int i=0;i<seconds;i++)schedule.OnSecond();
        Assert.That(serial,Is.EqualTo(expectedArms));
    }
    [Test]
    public void K4Latest9999TimerStartsAt151Not150()
    {
        int serial=0,clock=0;var due=new Dictionary<int,int>();
        var schedule=new JungleSpawnSchedule(JungleBlitzProgression.Load(),(type,delay)=>
            {int handle=++serial;if(type==4)due[handle]=clock+(int)delay;return handle;});
        schedule.Start();int startup=schedule.HandleForType(4);
        for(clock=1;clock<=151;clock++)schedule.OnSecond();
        int current=schedule.HandleForType(4);
        Assert.That(due[startup],Is.EqualTo(9999));
        Assert.That(due[current],Is.EqualTo(10150));
        Assert.That(schedule.OnDinoTimer(startup,_=>Assert.Fail("Superseded K4 timer")),Is.False);
    }
    [Test]
    public void ExplicitStopPreventsSpawnAndSecondsWithoutMutatingDifficultyTables()
    {
        int serial=0;var schedule=new JungleSpawnSchedule(JungleBlitzProgression.Load(),(_,__)=>++serial);
        schedule.Start();schedule.OnSecond();int handle=schedule.HandleForType(1);
        schedule.Stop();schedule.OnSecond();
        Assert.That(schedule.ElapsedSeconds,Is.EqualTo(1));
        Assert.That(schedule.OnDinoTimer(handle,_=>Assert.Fail("Stopped spawn")),Is.False);
        Assert.That(serial,Is.EqualTo(8));
    }
}
