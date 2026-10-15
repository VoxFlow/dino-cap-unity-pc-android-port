Shader "Hidden/DinoCap/DC1Present"
{
    Properties
    {
        _MainTex ("Source Framebuffer", 2D) = "black" {}
        _Mode ("Mode", Float) = 1
        _SourceSize ("Source Size", Vector) = (1024,768,0.0009765625,0.0013020833)
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _SourceSize;
            float _Mode;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 pointSample(float2 uv)
            {
                float2 size = _SourceSize.xy;
                float2 p = floor(uv * size);
                p = clamp(p, 0.0, size - 1.0);
                float2 centre = (p + 0.5) * _SourceSize.zw;
                return tex2D(_MainTex, centre);
            }

            fixed4 linear4(float2 uv)
            {
                // Task027G: source-equivalent presentation kernel.
                //
                // Native iPad path:
                //   completed 1024x768 backing
                //      -> CALayer default kCAFilterLinear
                //      -> physical display.
                //
                // The source RT intentionally remains FilterMode.Point.
                // Fetch the four neighbouring source texel centres explicitly
                // and reconstruct the same separable tent/linear kernel here.
                float2 size = _SourceSize.xy;
                float2 pos = uv * size - 0.5;
                float2 basePos = floor(pos);
                float2 fracPos = pos - basePos;

                float2 p00 = clamp(basePos,                         0.0, size - 1.0);
                float2 p10 = clamp(basePos + float2(1.0, 0.0),     0.0, size - 1.0);
                float2 p01 = clamp(basePos + float2(0.0, 1.0),     0.0, size - 1.0);
                float2 p11 = clamp(basePos + float2(1.0, 1.0),     0.0, size - 1.0);

                fixed4 c00 = tex2D(_MainTex, (p00 + 0.5) * _SourceSize.zw);
                fixed4 c10 = tex2D(_MainTex, (p10 + 0.5) * _SourceSize.zw);
                fixed4 c01 = tex2D(_MainTex, (p01 + 0.5) * _SourceSize.zw);
                fixed4 c11 = tex2D(_MainTex, (p11 + 0.5) * _SourceSize.zw);

                fixed4 cx0 = lerp(c00, c10, fracPos.x);
                fixed4 cx1 = lerp(c01, c11, fracPos.x);

                // Linear has no negative lobes, so this clamp is only a
                // floating-point safety bound and does not alter ideal output.
                return saturate(lerp(cx0, cx1, fracPos.y));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_Mode < 0.5)
                    return pointSample(i.uv);

                // Smooth Modern mode: Task027G source-closed present stage.
                // This matches the native iPad CALayer linear magnification
                // operator while leaving source gameplay rasterization intact.
                return linear4(i.uv);
            }
            ENDCG
        }
    }

    Fallback Off
}
