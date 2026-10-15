using System;
using UnityEngine;

namespace DinoCap.Data
{
    [Serializable]
    public struct Power2DPartRecord
    {
        public string partName;
        public string spriteRef;
        public float x;
        public float y;
        public float width;
        public float height;
        public float a;
        public float b;
        public float rotationRadians;
        public int packedRgba;
        public int sourceFlag;
    }
}
