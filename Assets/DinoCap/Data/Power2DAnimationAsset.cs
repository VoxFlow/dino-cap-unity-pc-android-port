using UnityEngine;

namespace DinoCap.Data
{
    [CreateAssetMenu(menuName = "Dino Cap/Power2D Animation", fileName = "Power2DAnimation")]
    public sealed class Power2DAnimationAsset : ScriptableObject
    {
        public string sourceName;
        [Min(0.0001f)] public float frameDuration = 0.025f;
        public bool loop = true;
        public Power2DFrameRecord[] frames;
    }
}
