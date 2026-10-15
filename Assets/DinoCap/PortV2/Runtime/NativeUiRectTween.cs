using UnityEngine;

namespace DinoCap.PortV2
{
    // Native DA87C/DA3C0. UILevelCtrliPad 69B48/69444 has the same
    // two phases but deliberately extrapolates, with strict >1 tests.
    public sealed class NativeUiRectTween
    {
        readonly Vector4 from, via, to;
        readonly float firstSeconds, secondSeconds;
        readonly bool levelControl;
        float elapsed;
        int phase=1;
        public Vector4 Value { get; private set; }
        public bool Active { get; private set; } = true;
        public Rect Rect => new Rect(Value.x-Value.z*0.5f, Value.y-Value.w*0.5f, Value.z, Value.w);

        public NativeUiRectTween(Vector4 from, Vector4 via, Vector4 to,
            float firstSeconds, float secondSeconds, bool levelControl=false)
        {
            this.from=from; this.via=via; this.to=to;
            this.firstSeconds=firstSeconds; this.secondSeconds=secondSeconds;
            this.levelControl=levelControl;
            Value=from;
        }

        public void Tick(float dt)
        {
            if(!Active) return;
            elapsed+=dt;
            float duration=phase==1?firstSeconds:secondSeconds;
            float ratio=elapsed/duration;
            bool end=levelControl?ratio>1f:ratio>=1f;
            float t=levelControl?ratio:Mathf.Min(1f,ratio);
            Value=phase==1?from+(via-from)*t:via+(to-via)*t;
            if(end)
            {
                if(phase==1) { phase=2; elapsed-=firstSeconds; }
                else Active=false;
            }
        }
    }
}
