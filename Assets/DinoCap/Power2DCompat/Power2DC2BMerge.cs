using DinoCap.Data;

namespace DinoCap.Power2DCompat
{
    public static class Power2DC2BMerge
    {
        // Recovered sub_78A38 contract:
        // ADD X/Y/C ; COPY W/H/A/B/ref. One raw merge, then evaluate once.
        public static Power2DPartRecord Merge(Power2DPartRecord source, Power2DPartRecord patch)
        {
            source.x += patch.x;
            source.y += patch.y;
            source.rotationRadians += patch.rotationRadians;
            source.width = patch.width;
            source.height = patch.height;
            source.a = patch.a;
            source.b = patch.b;
            source.spriteRef = patch.spriteRef;
            source.sourceFlag = patch.sourceFlag;
            return source;
        }
    }
}
