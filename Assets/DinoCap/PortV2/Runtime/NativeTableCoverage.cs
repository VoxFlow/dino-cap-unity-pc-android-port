using UnityEngine;

namespace DinoCap.PortV2
{
    // N252 measured iPad native1x table coverage. Host rendering adapter,
    // not a claim about the original PowerVR rounding implementation.
    // Bound is derived from the original sprite vertices; no row number,
    // offset, size reduction, UV inset or texture-pixel edit is involved.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class NativeTableCoverage : MonoBehaviour
    {
        static Material shared;
        SpriteRenderer spriteRenderer;
        Vector2[] vertices;
        MaterialPropertyBlock properties;

        void Awake()
        {
            spriteRenderer=GetComponent<SpriteRenderer>();
            var shader=Resources.Load<Shader>("Shaders/DC1NativeTableCoverage");
            if(shader==null || !shader.isSupported)return;
            if(shared==null)shared=new Material(shader){name="DC1 measured native table coverage"};
            spriteRenderer.sharedMaterial=shared;
            vertices=spriteRenderer.sprite.vertices;
            properties=new MaterialPropertyBlock();
        }

        void OnWillRenderObject()
        {
            if(properties==null)return;
            var camera=Camera.current;
            bool backing=camera!=null && camera.targetTexture!=null;
            float bottom=float.PositiveInfinity;
            if(backing)
                foreach(var vertex in vertices)
                    bottom=Mathf.Min(bottom,camera.WorldToScreenPoint(transform.TransformPoint(vertex)).y);
            spriteRenderer.GetPropertyBlock(properties);
            properties.SetFloat("_NativeBottomY",backing?bottom:0f);
            properties.SetFloat("_ExclusiveBottom",backing?1f:0f);
            spriteRenderer.SetPropertyBlock(properties);
        }
    }
}
