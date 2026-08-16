#ifndef OUTLINE_INCLUDED
#define OUTLINE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/OutLine/UVToSmoothNormal.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/OutLine/ZeroZ.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/OutLine/ZOffset.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/OutLine/LuminanceToOutlineThickness.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/PerspectiveRemoval/PerspectiveRemoval.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Dither/Dither.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/OutLine/Smears.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/SilToonInput.hlsl"

struct appdata
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv3 : TEXCOORD3; //SmoothNormal
};
struct v2f
{
    float4 pos : SV_POSITION;
#ifdef SMEARS_ON
    float smearsAlpha : TEXCOORD0;
#endif
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
    
#ifdef _PERSPECTIVE_REMOVAL_ON
    pushedOS = GetPerspectiveRemoval(_Head, pushedOS, v.normalOS, _PerspectiveRemovalRadius, _PerspectiveRemovalRatio);
#endif

#ifdef SMEARS_ON
    ApplySmear(pushedOS, v.uv3, normalOS, _SmearsDirection, _SmearsPower,
        pushedOS,
        o.smearsAlpha);
#endif

    // IncreaseZOffsetは詳細なアウトラインをフラグメントに埋め込むためのZOffset
    pushedOS = IncreaseZOffset(pushedOS, -_ZOffset);
    
    
    // オブジェクト空間の位置をクリップ空間（HClip）へ変換し、描画用の位置に設定する。
    o.pos = TransformObjectToHClip(float4(pushedOS, 1.0));
    return o;
}

float4 frag(v2f i) : SV_Target
{
#if defined(FADE_ON) || defined(SMEARS_ON)
    // FADE_OFF時は_FadeAlphaが0でもスメア単体で機能するよう1を基準にする
    half fadeAlpha = 1.0h;
#ifdef FADE_ON
    fadeAlpha = _FadeAlpha;
#endif
#ifdef SMEARS_ON
    fadeAlpha *= i.smearsAlpha;
#endif
    clip(fadeAlpha - BayerDither(i.pos.xy) - 0.0001);
#endif
    return _OutlineColor;
}

#endif