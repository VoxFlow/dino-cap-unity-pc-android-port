using NUnit.Framework;
using System.Collections.Generic;
using DinoCap.PortV2;

public sealed class Task027N254JungleTimerRepro
{
    [Test]
    public void TimerBoundaryIsInclusiveAndClockUsesF32()
    {
        var q=new NativeSceneTimerQueue();int hits=0;
        q.Arm(.5f,_=>hits++);q.Tick(.25f);Assert.That(hits,Is.Zero);
        q.Tick(.25f);Assert.That(hits,Is.EqualTo(1));Assert.That(q.Count,Is.Zero);
    }
    [Test]
    public void LateFrameRearmStartsAtCurrentClockAndDoesNotCatchUp()
    {
        var q=new NativeSceneTimerQueue();int hits=0;
        System.Action<int> callback=null;callback=_=>{hits++;q.Arm(1,callback);};
        q.Arm(1,callback);q.Tick(3.5f);
        Assert.That(hits,Is.EqualTo(1));Assert.That(q.Clock,Is.EqualTo(3.5f));
        q.Tick(.5f);Assert.That(hits,Is.EqualTo(1));
        q.Tick(.5f);Assert.That(hits,Is.EqualTo(2));
    }
    [Test]
    public void CallbackAdditionsWaitForNextTickEvenWhenAlreadyDue()
    {
        var q=new NativeSceneTimerQueue();var events=new List<int>();
        q.Arm(0,_=>{events.Add(1);q.Arm(0,__=>events.Add(2));});
        q.Tick(.1f);Assert.That(events,Is.EqualTo(new[]{1}));
        q.Tick(0);Assert.That(events,Is.EqualTo(new[]{1}));
        q.Tick(.1f);Assert.That(events,Is.EqualTo(new[]{1,2}));
    }
    [Test]
    public void EqualDeadlinesFollowNativeHeapReplacementOrder()
    {
        var q=new NativeSceneTimerQueue();var events=new List<int>();
        for(int i=1;i<=4;i++){int n=i;q.Arm(1,_=>events.Add(n));}
        q.Tick(1);Assert.That(events,Is.EqualTo(new[]{1,4,3,2}));
    }
    [Test]
    public void EarlierDeadlinesPercolateAheadOfInsertionOrder()
    {
        var q=new NativeSceneTimerQueue();var events=new List<int>();
        q.Arm(3,_=>events.Add(3));q.Arm(1,_=>events.Add(1));q.Arm(2,_=>events.Add(2));
        q.Tick(3);Assert.That(events,Is.EqualTo(new[]{1,2,3}));
    }
    [Test]
    public void CancellationIsProcessedAfterAdmissionBeforeClockAdvance()
    {
        var q=new NativeSceneTimerQueue();int hits=0;
        int handle=q.Arm(1,_=>hits++);q.Cancel(handle);q.Tick(1);
        Assert.That(hits,Is.Zero);Assert.That(q.Count,Is.Zero);
    }
    [Test]
    public void CallbackCancellationWaitsUntilFollowingFrame()
    {
        var q=new NativeSceneTimerQueue();var events=new List<int>();int second=0;
        q.Arm(1,_=>{events.Add(1);q.Cancel(second);});second=q.Arm(1,_=>events.Add(2));
        q.Tick(1);Assert.That(events,Is.EqualTo(new[]{1,2}));
    }
    [TestCase(0)] [TestCase(-1)]
    public void NonpositiveDeltaDoesNotAdvanceOrDispatch(float dt)
    {
        var q=new NativeSceneTimerQueue();int hits=0;q.Arm(0,_=>hits++);q.Tick(dt);
        Assert.That(q.Clock,Is.Zero);Assert.That(hits,Is.Zero);Assert.That(q.Count,Is.EqualTo(1));
    }
}
