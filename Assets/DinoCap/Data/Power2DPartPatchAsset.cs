using UnityEngine;

namespace DinoCap.Data
{
    [CreateAssetMenu(menuName = "Dino Cap/C2B Part Patch", fileName = "C2BPartPatch")]
    public sealed class Power2DPartPatchAsset : ScriptableObject
    {
        public string sourceName;
        public Power2DPartRecord patch;
    }
}
