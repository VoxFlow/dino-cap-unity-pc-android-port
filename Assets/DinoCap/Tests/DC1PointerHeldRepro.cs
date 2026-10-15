#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    // Native: touchesMoved fires only for touches that actually moved (EventEAGLView 0x188680), so a resting finger sends
    // nothing; the shop's single-slot handler (sub_6F594) ignores touch ids and cancels on any move outside the icon.
    // A resting finger on a move button therefore must NOT look like a move to the shop (user report: mobile shop press
    // while holding a move button was cancelled every frame).
    public class DC1PointerHeldRepro
    {
        static readonly Rect Zone = new Rect(100,100,100,100);

        SyntheticPointerLayer Make()
        {
            var layer = new SyntheticPointerLayer();
            layer.RegisterControl("a", Zone);
            return layer;
        }

        [Test]
        public void RestingPointerProducesHeldNotMove()
        {
            var layer = Make();
            int id = layer.InjectDown(new Vector2(150,150));
            var down = layer.Update(new Rect(0,0,1024,768),1024,768,false);
            Assert.AreEqual(SyntheticPointerLayer.Phase.Down, down[0].phase);
            var held = layer.Update(new Rect(0,0,1024,768),1024,768,false);
            Assert.AreEqual(1, held.Count);
            Assert.AreEqual(SyntheticPointerLayer.Phase.Held, held[0].phase, "A resting pointer is Held, never a fabricated Move.");
            Assert.IsTrue(layer.IsControlPressed("a"), "Held keeps the control pressed.");
            layer.InjectMove(id,new Vector2(160,150));
            var moved = layer.Update(new Rect(0,0,1024,768),1024,768,false);
            Assert.AreEqual(SyntheticPointerLayer.Phase.Move, moved[0].phase);
            layer.InjectUp(id);
            var up = layer.Update(new Rect(0,0,1024,768),1024,768,false);
            Assert.AreEqual(SyntheticPointerLayer.Phase.Up, up[0].phase);
            Assert.IsFalse(layer.IsControlPressed("a"));
            Assert.AreEqual(0, layer.Update(new Rect(0,0,1024,768),1024,768,false).Count);
        }
    }
}
#endif
