using System.Collections.Generic;
using DinoCap.Data;
using UnityEngine;

namespace DinoCap.Power2DCompat
{
    [RequireComponent(typeof(Power2DAnimationPlayer))]
    public sealed class Power2DActorRenderer : MonoBehaviour
    {
        [Header("Generated source-truth data")]
        public SpriteRegistryAsset spriteRegistry;
        public Power2DPartPatchAsset equippedWeaponPatch;
        public string weaponPartName = "com1";

        [Header("Recovered transform contract")]
        [Tooltip("Source-space actor scale. Keep 1 unless original behavior proves otherwise.")]
        public float actorScale = 1f;
        [Tooltip("Current best mapping for native screen-CW positive C into Unity's CCW-positive Z.")]
        public float unityRotationSign = -1f;
        public bool facingLeft;
        public int baseSortingOrder;

        private Power2DAnimationPlayer _player;
        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();

        private void Awake()
        {
            _player = GetComponent<Power2DAnimationPlayer>();
            _player.FrameEntered += _ => RenderCurrentFrame();
        }

        private void OnEnable()
        {
            if (_player != null) RenderCurrentFrame();
        }

        public void SetWeaponPatch(Power2DPartPatchAsset patch)
        {
            equippedWeaponPatch = patch;
            RenderCurrentFrame();
        }

        public void RenderCurrentFrame()
        {
            if (_player == null || spriteRegistry == null) return;
            var frame = _player.GetCurrentFrame();
            if (frame == null || frame.parts == null) return;
            EnsureRendererCount(frame.parts.Length);

            for (int i = 0; i < _renderers.Count; i++)
                _renderers[i].gameObject.SetActive(i < frame.parts.Length);

            for (int i = 0; i < frame.parts.Length; i++)
            {
                Power2DPartRecord part = frame.parts[i];
                if (part.partName == weaponPartName && equippedWeaponPatch != null)
                    part = Power2DC2BMerge.Merge(part, equippedWeaponPatch.patch);

                ApplyPart(_renderers[i], part, i);
            }
        }

        private void EnsureRendererCount(int count)
        {
            while (_renderers.Count < count)
            {
                int index = _renderers.Count;
                var go = new GameObject("Part_" + index.ToString("00"));
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                _renderers.Add(sr);
            }
        }

        private void ApplyPart(SpriteRenderer sr, Power2DPartRecord part, int partIndex)
        {
            if (string.IsNullOrEmpty(part.spriteRef) || !spriteRegistry.TryGet(part.spriteRef, out var sprite))
            {
                sr.sprite = null;
                return;
            }

            sr.sprite = sprite;
            sr.sortingOrder = baseSortingOrder + partIndex;
            sr.color = Power2DMath.DecodePackedRgba(part.packedRgba);

            float localX = part.x * actorScale;
            float localY = part.y * actorScale;
            float rotation = part.rotationRadians;
            float pivotX = part.a * (part.width * actorScale) * 0.5f;
            float pivotY = part.b * (part.height * actorScale) * 0.5f;

            if (facingLeft)
            {
                localX = -localX;
                pivotX = -pivotX;
                rotation = -rotation;
            }

            Vector2 position = new Vector2(localX, localY);
            Vector2 pivotOffset = new Vector2(pivotX, pivotY);

            // Task021B model C: authored weapon pivot/grip P = position + pivotOffset.
            // For ordinary body parts A=B=0, this collapses exactly to centre=position.
            Vector2 gripPoint = position + pivotOffset;
            Vector2 centre = gripPoint - Power2DMath.Rotate(pivotOffset, rotation);

            sr.transform.localPosition = new Vector3(centre.x, centre.y, 0f);
            sr.transform.localRotation = Quaternion.Euler(0f, 0f, unityRotationSign * rotation * Mathf.Rad2Deg);

            // Source W/H are authoritative requested display dimensions. Generated Sprite pixelsPerUnit
            // should normally be 1 so this is a direct source-unit scale.
            Vector2 nativeSize = sprite.rect.size;
            float sx = nativeSize.x > 0f ? (part.width * actorScale) / nativeSize.x : 1f;
            float sy = nativeSize.y > 0f ? (part.height * actorScale) / nativeSize.y : 1f;
            sr.transform.localScale = new Vector3(sx, sy, 1f);
            sr.flipX = facingLeft;
        }
    }
}
