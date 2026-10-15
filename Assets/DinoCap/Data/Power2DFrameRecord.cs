using System;

namespace DinoCap.Data
{
    [Serializable]
    public sealed class Power2DFrameRecord
    {
        public int sourceOrdinal;
        public bool hasEvent;
        public Power2DPartRecord[] parts;
    }
}
