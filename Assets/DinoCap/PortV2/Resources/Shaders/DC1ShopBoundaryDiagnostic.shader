// Development capture experiment only. Not selected by production gameplay.
// Tests exclusive lower-edge coverage from the measured geometric boundary;
// changes no frame texels, UV rectangle, size, placement or other sprites.
Shader "Hidden/DinoCap/ShopBoundaryDiagnostic"
{
 Properties {
  [PerRendererData] _MainTex("Sprite",2D)="white"{}
  _NativeBottomY("Projected lower edge",Float)=0
  _TargetHeight("Target height",Float)=768
  _StrictBottom("Exclusive lower-edge experiment",Float)=0
 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;
   float _NativeBottomY,_TargetHeight,_StrictBottom;
   struct input {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   struct output {float4 position:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   output vert(input v) {output o;o.position=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   float4 frag(output i):SV_Target {
    float y=floor(i.position.y)+0.5;
    // This experiment renders into Unity's flipped render-target projection.
    // Raw SV_POSITION here matches the camera's bottom-left target coordinates.
    if(_StrictBottom>1.5)return float4(y/_TargetHeight,_NativeBottomY/_TargetHeight,0,1);
    if(_StrictBottom>0.5 && y<=_NativeBottomY)discard;
    return tex2D(_MainTex,i.uv)*i.color;
   }
   ENDCG
  }
 }
 Fallback Off
}
