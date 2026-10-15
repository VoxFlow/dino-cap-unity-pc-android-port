// Task027M.8.1 - F3 X-ray visual reconstruction (PLATFORM_ADAPTER / VISUAL_RECONSTRUCTION).
//
// V30.21 shipped a shader that failed on the live project (bright magenta =
// Unity's internal-error shader). V30.22 replaces it with the canonical Unity
// Built-in-RP sprite shader skeleton + a 5-line fragment change - the reference
// pattern that cannot fail to compile on BiRP (Unity 6000.3.8f1). The C# side
// additionally refuses to assign this material unless Shader.isSupported is true
// and the material did NOT fall back to Hidden/InternalErrorShader, so a shader
// failure can never turn the dinosaur magenta again - it just disables the
// X-ray and keeps normal rendering.
//
// NOT a recovered native renderer. Presentation only - no damage / audio / AI /
// HP / collision / F2 state. Driven by CombatSandboxV2 on the existing state-4
// k*_be BE frame-3 reaction cadence.
//
// Blend One OneMinusSrcAlpha (premultiplied) - identical to Unity's stock
// Sprites/Default, so it composites consistently with every other sprite.
// Point-filter compatible (no screen-space UVs, no blur, no bloom); 1x/2x
// identical (pure per-fragment math).

Shader "DinoCap/F3XraySprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _XrayTint ("X-ray tint", Color) = (0.72, 0.87, 1.0, 1.0)
        _XrayAlpha ("X-ray alpha", Range(0,1)) = 0.62
        _XrayBrightness ("Internal brightness", Range(0,3)) = 1.35
        _XrayFloor ("Silhouette floor", Range(0,1)) = 0.12
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _XrayTint;
            float  _XrayAlpha;
            float  _XrayBrightness;
            float  _XrayFloor;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color    = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // authored body texel, masked by the k*_be pose alpha (vertex colour)
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // dark interior/outline -> dense "bone" (bright); highlight -> thin (dim)
                fixed lum     = dot(c.rgb, fixed3(0.299, 0.587, 0.114));
                fixed density = saturate((1.0 - lum) * _XrayBrightness + _XrayFloor);

                fixed3 rgb = _XrayTint.rgb * density;   // pale blue-white
                fixed  a   = c.a * _XrayAlpha;          // keep silhouette, translucent phase
                rgb *= a;                               // premultiply for Blend One OneMinusSrcAlpha
                return fixed4(rgb, a);
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
