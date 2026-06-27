#ifndef FRAGMENT_INCLUDED
#define FRAGMENT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\SilToon\Lights.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\SilToon\Fragment\SilToonFresnel.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\SilToon\Fragment\FaceLight.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\SilToon\PerspectiveRemoval\PerspectiveRemoval.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\SilToon\SilToonInput.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\SilToon\Fragment\NormalCombine.hlsl"

float _Alpha;

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
    half3 tangentWS : TEXCOORD3;
    half3 bitangentWS : TEXCOORD4;
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
    OUT.tangentWS = (half3) TransformObjectToWorldDir(IN.tangentOS.xyz);
    
    half sign = (half) (IN.tangentOS.w * unity_WorldTransformParams.w);
    OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * sign;
    
    return OUT;
}

half4 frag(Varyings IN) : SV_Target
{
    half3 normalWS = (half3) GetNormalCombine(
        TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap),
        IN.uv,
        IN.normalWS,
        IN.tangentWS,
        IN.bitangentWS,
        _NormalMapIntensity
    );

    normalWS = _IsForFace ? (half3) GetFaceNormal(_FaceUp, (float3) normalWS) : normalWS;

    half3 color;
    GetLights_float(_ColorLit, _ColorMiddle, _ColorShadow, IN.positionWS, (float3) normalWS, color);
    
    half backLight, rimLightFront, rimLightBack;
    GetFresnel(IN.normalWS, (half3) GetWorldSpaceNormalizeViewDir(IN.positionWS),
               backLight, rimLightFront, rimLightBack);

    color += backLight * _FresnelBackLight;
    color += rimLightBack * _FresnelBackRimLight;
    color += rimLightFront * _FresnelFrontRimLight;
    
    return half4(color, _Alpha) * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
}
#endif