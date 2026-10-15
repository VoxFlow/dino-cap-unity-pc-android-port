using UnityEngine;

namespace DinoCap.Power2DCompat
{
    public static class Power2DMath
    {
        public static Vector2 Rotate(Vector2 v, float radians)
        {
            float c = Mathf.Cos(radians);
            float s = Mathf.Sin(radians);
            return new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
        }

        public static Color32 DecodePackedRgba(int packed)
        {
            unchecked
            {
                uint v = (uint)packed;
                return new Color32(
                    (byte)(v & 0xff),
                    (byte)((v >> 8) & 0xff),
                    (byte)((v >> 16) & 0xff),
                    (byte)((v >> 24) & 0xff));
            }
        }
    }
}
