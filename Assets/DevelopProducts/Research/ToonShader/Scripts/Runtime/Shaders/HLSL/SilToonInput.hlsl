#ifndef SILTOON_INPUT_INCLUDED
#define SILTOON_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);

// SRP Batcher対応:
// マテリアルプロパティは全パスで同一レイアウトの UnityPerMaterial に置く必要がある。
// SilToon / SilToonFaceOverlay の全パスがこのファイルをincludeすること。
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;

    half4 _ColorLit;
    half4 _ColorMiddle;
    half4 _ColorShadow;
    float4 _OutlineColor;

    float3 _FaceUp;
    float _FadeAlpha;

    float3 _Head;
    float _Alpha;

    float _PerspectiveRemovalRatio;
    float _PerspectiveRemovalRadius;
    float _ZOffset;
    float _IsSmoothNormal;

    float _OutlineWidthLit;
    float _OutlineWidthShadow;

    half _NormalMapIntensity;
    half _FresnelBackLight;
    half _FresnelFrontRimLight;
    half _FresnelBackRimLight;
CBUFFER_END

#endif
