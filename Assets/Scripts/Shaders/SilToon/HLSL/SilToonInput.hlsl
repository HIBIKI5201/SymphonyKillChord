#ifndef SILTOON_INPUT_INCLUDED
#define SILTOON_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);

// PBR用(sampler_BaseMapを共用)
TEXTURE2D(_MetallicMap);
TEXTURE2D(_RoughnessMap);

// SRP Batcher対応:
// マテリアルプロパティは全パスで同一レイアウトの UnityPerMaterial に置く必要がある。
// SilToon / SilToonFaceOverlay の全パスがこのファイルをincludeすること。
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;

    half4 _ColorLit;
    half4 _ColorMiddle;
    half4 _ColorShadow;
    half4 _OutlineColor;
    half4 _SSSColor;

    half3 _FaceUp;
    half _FadeAlpha;

    float3 _Head;
    half _Alpha;

    float _PerspectiveRemovalRatio;
    float _PerspectiveRemovalRadius;
    float _ZOffset;
    float _IsSmoothNormal;

    float _OutlineWidthLit;
    float _OutlineWidthShadow;

    float3 _SmearsDirection;
    float _SmearsPower;

    half _NormalMapIntensity;
    half _FresnelBackLight;
    half _FresnelFrontRimLight;
    half _FresnelBackRimLight;

    half _SSSWrap;
    half _SSSIntensity;
    half _SSSThickness;
    half _SSSTransmissionPower;

    half4 _FakeShadowColor;
    float2 _FakeShadowOffset;
    float _FakeShadowDepthBias;

    half _Metallic;
    half _Roughness;
    half _SpecularIntensity;
    half _EnvReflectionIntensity;
CBUFFER_END

#endif
