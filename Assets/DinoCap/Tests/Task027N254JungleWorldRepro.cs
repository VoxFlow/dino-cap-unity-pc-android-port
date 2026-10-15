using NUnit.Framework;
using UnityEngine;
using DinoCap.PortV2;

public sealed class Task027N254JungleWorldRepro
{
    [Test]
    public void InitialNativeCentresAndDrawGeometry()
    {
        var go=new GameObject("Native Jungle world fixture");
        try
        {
            var world=go.AddComponent<JungleWorldLayers>();world.Initialize(0,12);world.Tick(-512,1024);
            Assert.That(world.TileCount,Is.EqualTo(3));
            var b=go.transform.Find("JungleBackground_0").GetComponent<SpriteRenderer>();
            var f=go.transform.Find("JungleForeground_0").GetComponent<SpriteRenderer>();
            Assert.That(b.sprite.name,Is.EqualTo("back_endFrame"));Assert.That(b.sprite.rect.width,Is.EqualTo(1021));
            Assert.That(b.transform.position.x,Is.EqualTo(-.5f));
            Assert.That(b.bounds.size.x,Is.EqualTo(1023).Within(.001f));Assert.That(b.bounds.size.y,Is.EqualTo(768));
            Assert.That(f.bounds.size.x,Is.EqualTo(1023));Assert.That(f.bounds.size.y,Is.EqualTo(312));
            Assert.That(f.transform.position.y,Is.EqualTo(-250));Assert.That(f.bounds.max.y,Is.EqualTo(-94));
            Assert.That(b.sortingOrder,Is.EqualTo(0));Assert.That(f.sortingOrder,Is.EqualTo(12));
        }
        finally{Object.DestroyImmediate(go);}
    }
    [TestCase(510,-512,0,-.5f)] [TestCase(511,511,0,1022.5f)]
    [TestCase(-513,-1535,-1f/1023f,-1024.5f)]
    [TestCase(-1535,-2558,0,-2046.5f)]
    public void SignedNativeBoundaryVectors(float camera,float origin,float fraction,float centre)
    {
        var wrap=new NativeJungleLayerWrap();wrap.Tick(camera);
        Assert.That(wrap.OriginX,Is.EqualTo(origin));Assert.That(wrap.Fraction,Is.EqualTo(fraction).Within(.000001f));
        Assert.That(wrap.CentreX(0),Is.EqualTo(centre).Within(.001f));
    }
    [Test]
    public void FractionRetainedUntilAnotherNativeSnap()
    {
        var wrap=new NativeJungleLayerWrap();wrap.Tick(521);
        float centre=wrap.CentreX(0),fraction=wrap.Fraction;
        wrap.Tick(600);
        Assert.That(wrap.CentreX(0),Is.EqualTo(centre));Assert.That(wrap.Fraction,Is.EqualTo(fraction));
    }
    [TestCase(1024,3)] [TestCase(1365,4)] [TestCase(2730,5)]
    public void ViewportAdaptationRetainsNativeThreeCopiesAtReferenceWidth(float width,int count)
        =>Assert.That(NativeJungleLayerWrap.TileCountForView(width),Is.EqualTo(count));
    [TestCase(2560,1440)] [TestCase(3840,2160)] [TestCase(3440,1440)]
    [TestCase(5120,1440)] [TestCase(5760,1080)]
    public void ExpandedLayoutsAddLeftCoverageWithoutChangingNativeSnap(int pixelsX,int pixelsY)
    {
        var go=new GameObject("Expanded Jungle coverage fixture");
        try
        {
            float width=768f*pixelsX/pixelsY;
            var world=go.AddComponent<JungleWorldLayers>();world.Initialize(0,12);
            world.Tick(-513,width);
            Assert.That(world.Layout.OriginX,Is.EqualTo(-1535));
            Assert.That(world.ExpandedLeftCoverageActive,Is.True);
            var left=go.transform.Find("JungleBackground_ExpandedLeftCoverage");
            Assert.That(left,Is.Not.Null);
            Assert.That(left.position.x,Is.EqualTo(world.Layout.CentreX(0)-1023));
            world.Tick(-513,1024);
            Assert.That(world.ExpandedLeftCoverageActive,Is.False,"Reference4:3 retains native geometry.");
        }
        finally{Object.DestroyImmediate(go);}
    }
}
