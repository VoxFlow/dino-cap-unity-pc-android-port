// Measured iPad table coverage adapter (N252 IMG_0029).
// The authored rectangle is unchanged. Exclude the pixel center exactly on
// its lower geometric edge, matching the original native1x capture.
// This does not establish the original GPU's underlying rounding mechanism.
Shader "DinoCap/NativeTableCoverage"
{
 Properties {
  [PerRendererData] _MainTex("Sprite Texture",2D)="white"{}
  _Color("Tint",Color)=(1,1,1,1)
  [HideInInspector] _RendererColor("RendererColor",Color)=(1,1,1,1)
  [HideInInspector] _Flip("Flip",Vector)=(1,1,1,1)
  [PerRendererData] _AlphaTex("External Alpha",2D)="white"{}
  [PerRendererData] _EnableExternalAlpha("Enable External Alpha",Float)=0
  _NativeBottomY("Projected lower edge",Float)=0
  _ExclusiveBottom("Measured target coverage",Float)=0
 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" "IgnoreProjector"="True"}
  Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex SpriteVert
   #pragma fragment frag
   #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
   #include "UnitySprites.cginc"
   float _NativeBottomY,_ExclusiveBottom;
   fixed4 frag(v2f i):SV_Target {
    // Render-target projection yields bottom-left camera coordinates here.
    // Evaluate coverage at an exact pixel center, independent of UV drift.
    float y=floor(i.vertex.y)+0.5;
    if(_ExclusiveBottom>0.5 && y<=_NativeBottomY)discard;
    fixed4 c=SampleSpriteTexture(i.texcoord)*i.color;c.rgb*=c.a;return c;
   }
   ENDCG
  }
 }
 Fallback "Sprites/Default"
}
