#ifndef OUTLINE_INCLUDED
#define OUTLINE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\OutLine\UVToSmoothNormal.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\OutLine\ZeroZ.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\OutLine\ZOffset.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\OutLine\LuminanceToOutlineThickness.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\PerspectiveRemoval\PerspectiveRemoval.hlsl"


float _PerspectiveRemovalRatio;
float _PerspectiveRemovalRadius;
float3 _Head;

float _ZOffset;
float _IsSmoothNormal;
float _OutlineWidthLit;
float _OutlineWidthShadow;

float4 _OutlineColor = float4(0, 0, 0, 1);

struct appdata
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv3 : TEXCOORD3;//SmoothNormal
};
struct v2f
{
    float4 pos : SV_POSITION;
};

v2f vert(appdata v)
{
    // vert: 頂点シェーダーの主要処理ブロック
    v2f o;
    
    // UV を用いて滑らかな法線を取得。必要に応じて頂点法線の代わりに使う。
    float3 smoothNormalOS = GetSmoothNormalFromUV(v.uv3, v.normalOS, v.tangentOS);
    
    float3 normalOS = _IsSmoothNormal ? smoothNormalOS : v.normalOS;
    
    // ビュー方向に依存する Z 成分の補正を行うユーティリティ。
    normalOS = GetViewZeroZ_OS(normalOS);
    
    // 頂点位置を法線方向に押し出してアウトライン幅を作る。
    float3 pushedOS = v.positionOS.xyz + normalOS * lerp(_OutlineWidthShadow, _OutlineWidthLit, GetOutlineThicknessRatio(v.positionOS, v.normalOS));
    
    // IncreaseZOffsetは詳細なアウトラインをフラグメントに埋め込むためのZOffset
    pushedOS = GetPerspectiveRemoval(_Head, pushedOS, v.normalOS, _PerspectiveRemovalRadius, _PerspectiveRemovalRatio);
    pushedOS = IncreaseZOffset(pushedOS, -_ZOffset);
    
    
    // オブジェクト空間の位置をクリップ空間（HClip）へ変換し、描画用の位置に設定する。
    o.pos = TransformObjectToHClip(float4(pushedOS, 1.0));
    return o;
}

float4 frag(v2f i) : SV_Target
{
    return _OutlineColor;
}

#endif