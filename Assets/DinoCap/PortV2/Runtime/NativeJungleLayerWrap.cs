using System;

namespace DinoCap.PortV2
{
    // Shared native D308, Jungle13FD4 supplies origin(-512,y),size1023.
    // Preserve negative pre-subtraction, truncation and retained fraction;
    // replacing this with floor/modulo would change the shipped boundary.
    public sealed class NativeJungleLayerWrap
    {
        public const float TileWidth=1023f;
        public float OriginX { get; private set; }=-512f;
        public float Fraction { get; private set; }
        public void Tick(float cameraLeft)
        {
            float delta=cameraLeft-OriginX;
            if(delta<0f)delta-=TileWidth;
            float quotient=delta/TileWidth;
            int steps=(int)quotient;
            if(steps==0)return;
            OriginX+=steps*TileWidth;
            Fraction=quotient-steps;
        }
        public float CentreX(int index)
        {
            if(index<0)throw new ArgumentOutOfRangeException(nameof(index));
            return OriginX+TileWidth*(index+Fraction+0.5f);
        }
        public static int TileCountForView(float viewWidth)
        {
            // Native13FD4 creates three children per layer. Extra copies serve expanded
            // desktop viewports; this adaptation is not native behavior.
            return 3+(int)Math.Ceiling(Math.Max(0f,viewWidth-1024f)/TileWidth);
        }
    }
}
