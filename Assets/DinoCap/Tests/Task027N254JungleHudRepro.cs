using NUnit.Framework;
using UnityEngine;
using DinoCap.PortV2;

public sealed class Task027N254JungleHudRepro
{
    [TestCase(0,"0 0 : 0 0")] [TestCase(59,"0 0 : 5 9")]
    [TestCase(60,"0 1 : 0 0")] [TestCase(600,"1 0 : 0 0")]
    [TestCase(6000,"10 0 : 0 0")]
    public void NativeTimeUsesFourDecimalFieldsWithoutHoursCap(int seconds,string expected)
        =>Assert.That(NativeJungleHud.TimeText(seconds),Is.EqualTo(expected));
    [Test]
    public void NativeKillDigitsUseExactWidthsAndNegativeFiveTracking()
    {
        var digits=NativeJungleHud.KillDigits(140);
        Assert.That(NativeJungleHud.KillIcon,Is.EqualTo(new Rect(450,9,91,72)));
        Assert.That(NativeJungleHud.TimeLabel,Is.EqualTo(new Rect(715,15,200,60)));
        Assert.That(digits[0].rect,Is.EqualTo(new Rect(539,17,23,53)));
        Assert.That(digits[1].rect,Is.EqualTo(new Rect(557,17,40,53)));
        Assert.That(digits[2].rect,Is.EqualTo(new Rect(592,17,40,53)));
    }
    [Test]
    public void OriginalNotificationGrowsAndRisesWithNativeGeometry()
    {
        var go=new GameObject("Native notification fixture");
        try {
            var n=go.AddComponent<JungleAmmoNotification>();n.Show(0,12,new Vector2(100,-74));
            var icon=go.transform.Find("icon_item0Frame").GetComponent<SpriteRenderer>();
            Assert.That(icon.bounds.size.x,Is.EqualTo(19).Within(.001f));
            n.Tick(.1f);
            Assert.That(n.Progress,Is.EqualTo(1));Assert.That(go.transform.position.y,Is.EqualTo(-69));
            Assert.That(icon.transform.position.x,Is.EqualTo(75));Assert.That(icon.transform.position.y,Is.EqualTo(-49));
            Assert.That(icon.bounds.size.x,Is.EqualTo(38).Within(.001f));Assert.That(icon.bounds.size.y,Is.EqualTo(44).Within(.001f));
            var first=go.transform.Find("icon_num1Frame").GetComponent<SpriteRenderer>();
            Assert.That(first.transform.position.x,Is.EqualTo(115));
            Assert.That(first.bounds.size.x,Is.EqualTo(11.5f).Within(.001f));
            Assert.That(first.bounds.size.y,Is.EqualTo(26.5f).Within(.001f));
            n.Show(3,72,new Vector2(200,-20));
            Assert.That(n.Progress,Is.EqualTo(.5f));Assert.That(n.Family,Is.EqualTo(3));
            Assert.That(go.GetComponentsInChildren<SpriteRenderer>().Length,Is.EqualTo(4));
            Assert.That(go.transform.position.x,Is.EqualTo(200));
        } finally {Object.DestroyImmediate(go);}
    }
    [Test]
    public void OriginalTimeGlyphsIncludeColonAndZeroInkSpace()
    {
        var raw=Resources.Load<TextAsset>("Fonts/JungleTime/metrics");
        Assert.That(raw,Is.Not.Null);
        StringAssert.Contains("\"character\": \":\"",raw.text);
        StringAssert.Contains("\"character\": \" \"",raw.text);
        Assert.That(Resources.Load<Texture2D>("Fonts/JungleTime/colon"),Is.Not.Null);
    }
}
