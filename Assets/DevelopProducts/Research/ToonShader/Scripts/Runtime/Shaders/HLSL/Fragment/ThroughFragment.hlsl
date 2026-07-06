#ifndef FRAGMENT_INCLUDED
#define FRAGMENT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/SilToonInput.hlsl"
#include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/Lights.hlsl"
#include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/Fragment/SilToonFresnel.hlsl"
#include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/Fragment/FaceLight.hlsl"
#include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/Fragment/NormalCombine.hlsl"
#include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/PerspectiveRemoval/PerspectiveRemoval.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
#ifdef _NORMALMAP
    float4 tangentOS : TANGENT;
#endif
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
#ifdef _NORMALMAP
    half3 tangentWS : TEXCOORD3;
    half3 bitangentWS : TEXCOORD4;
#endif
};

Varyings vert(Attributes IN)
{
    Varyings OUT;
    float3 perspectiveRemoval = GetPerspectiveRemoval(
        _Head, IN.positionOS.xyz, IN.normalOS,
        _PerspectiveRemovalRadius, _PerspectiveRemovalRatio);

    OUT.positionHCS = TransformObjectToHClip(perspectiveRemoval);

    OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);

    OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

    OUT.normalWS = (half3) TransformObjectToWorldNormal(IN.normalOS);

#ifdef _NORMALMAP
    OUT.tangentWS = (half3) TransformObjectToWorldDir(IN.tangentOS.xyz);

    half sign = (half) (IN.tangentOS.w * unity_WorldTransformParams.w);
    OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * sign;
#endif

    return OUT;
}

half4 frag(Varyings IN) : SV_Target
{
#ifdef _NORMALMAP
    half3 normalWS = (half3) GetNormalCombine(
        TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap),
        IN.uv,
        IN.normalWS,
        IN.tangentWS,
        IN.bitangentWS,
        _NormalMapIntensity
    );
#else
    half3 normalWS = IN.normalWS;
#endif

    normalWS = _IsForFace ? (half3) GetFaceNormal(_FaceUp, (float3) normalWS) : normalWS;

    half3 color;
    GetLights_float(_ColorLit, _ColorMiddle, _ColorShadow, IN.positionWS, (float3) normalWS,
                    GetNormalizedScreenSpaceUV(IN.positionHCS), color);

    half backLight, rimLightFront, rimLightBack;
    GetFresnel(IN.normalWS, (half3) GetWorldSpaceNormalizeViewDir(IN.positionWS),
               backLight, rimLightFront, rimLightBack);

    color += backLight * _FresnelBackLight;
    color += rimLightBack * _FresnelBackRimLight;
    color += rimLightFront * _FresnelFrontRimLight;

    return half4(color, _Alpha) * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
}
#endif
