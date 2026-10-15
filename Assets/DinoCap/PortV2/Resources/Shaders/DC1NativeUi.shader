Shader "Hidden/DinoCap/DC1NativeUi"
{
    Properties {
        _MainTex("Texture",2D)="white"{}
        _Tint("Tint",Color)=(1,1,1,1)
        _RectClip("Clip rectangle",Vector)=(0,0,1,1)
        _UvRect("UV rectangle",Vector)=(0,0,1,1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        UNITY_DECLARE_TEX2D(_MainTex);
        #if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
            SamplerState native_point_clamp_sampler;
            SamplerState display_linear_clamp_sampler;
        #endif
        float4 _Tint;
        float4 _RectClip;
        float4 _UvRect;
        float _UseRasterNearest;
        float4 _RasterRect;
        float4 _TexelUvRect;
        float4 _TextureDimensions;
        struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
        v2f vert(appdata_base v) {v2f o;o.vertex=float4(_RectClip.xy+v.vertex.xy*_RectClip.zw,0,1);o.uv=_UvRect.xy+v.texcoord.xy*_UvRect.zw;return o;}
        fixed4 frag(v2f i):SV_Target {
            #if defined(DC1_DIAGNOSTIC_SOLID)
                return _Tint;
            #else
                #if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
                    #if defined(DC1_PRESENT_LINEAR)
                        return _MainTex.Sample(display_linear_clamp_sampler,i.uv)*_Tint;
                    #else
                        float2 pointUv=i.uv;
                        #if defined(DC1_TEXEL_SPACE_NEAREST)
                            if(_UseRasterNearest>0.5) {
                                // GLES1.1 nearest is floor(texelCoordinate). Evaluate
                                // the same affine mapping in texel units to preserve
                                // exact boundaries without varying-UV rounding drift.
                                float2 pixel=float2(i.vertex.x,_TextureDimensions.z-i.vertex.y);
                                float2 texel=_TexelUvRect.xy+(pixel-_RasterRect.xy)*(_TexelUvRect.zw/_RasterRect.zw);
                                pointUv=(floor(texel)+0.5)/_TextureDimensions.xy;
                            }
                        #endif
                        return _MainTex.Sample(native_point_clamp_sampler,pointUv)*_Tint;
                    #endif
                #else
                    return UNITY_SAMPLE_TEX2D(_MainTex,i.uv)*_Tint;
                #endif
            #endif
        }
        ENDCG
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.5
            #pragma multi_compile_local _ DC1_DIAGNOSTIC_SOLID
            #pragma multi_compile_local _ DC1_PRESENT_LINEAR
            #pragma multi_compile_local _ DC1_TEXEL_SPACE_NEAREST
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
        Pass
        {
            Blend Off
            CGPROGRAM
            #pragma target 3.5
            #pragma multi_compile_local _ DC1_DIAGNOSTIC_SOLID
            #pragma multi_compile_local _ DC1_PRESENT_LINEAR
            #pragma multi_compile_local _ DC1_TEXEL_SPACE_NEAREST
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
    Fallback Off
}
