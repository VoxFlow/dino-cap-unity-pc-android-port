using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.Data
{
    [CreateAssetMenu(menuName = "Dino Cap/Sprite Registry", fileName = "SpriteRegistry")]
    public sealed class SpriteRegistryAsset : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string key;
            public Sprite sprite;
        }

        public Entry[] entries;
        private Dictionary<string, Sprite> _lookup;

        public bool TryGet(string sourceRef, out Sprite sprite)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<string, Sprite>(StringComparer.Ordinal);
                if (entries != null)
                {
                    foreach (var e in entries)
                        if (!string.IsNullOrEmpty(e.key) && e.sprite != null)
                            _lookup[e.key] = e.sprite;
                }
            }
            return _lookup.TryGetValue(sourceRef, out sprite);
        }
    }
}
