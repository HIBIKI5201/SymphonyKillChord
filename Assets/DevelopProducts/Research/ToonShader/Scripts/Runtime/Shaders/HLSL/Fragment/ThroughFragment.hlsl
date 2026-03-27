#ifndef FRAGMENT_INCLUDED
#define FRAGMENT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\Lights.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\Fragment\SilToonFresnel.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\Fragment\FaceLight.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\PerspectiveRemoval\PerspectiveRemoval.hlsl"
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
    float3 positionOS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    float3 tangentWS : TEXCOORD3;
    float3 bitangentWS : TEXCOORD4;
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
float4 _BaseMap_ST;

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
float4 _NormalMap_ST;

float _NormalMapIntensity;

float _PerspectiveRemovalRatio;
float _PerspectiveRemovalRadius;
float3 _Head;

float _IsForFace;
float3 _FaceUp;

half4 _ColorLit;
half4 _ColorMiddle;
half4 _ColorShadow;

float _FresnelBackLight;
float _FresnelFrontRimLight;
float _FresnelBackRimLight;

float _Alpha;

#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\Fragment\NormalCombine.hlsl"

Varyings vert(Attributes IN)
{
    Varyings OUT;
    float3 perspectiveRemoval = GetPerspectiveRemoval(_Head, IN.positionOS.xyz, IN.normalOS, _PerspectiveRemovalRadius, _PerspectiveRemovalRatio);
    
    OUT.positionHCS = TransformObjectToHClip(perspectiveRemoval);
    OUT.positionOS = IN.positionOS;
    OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
    
    
    
    OUT.normalWS = normalize(mul(float4(IN.normalOS, 0.0), unity_WorldToObject).xyz);
    OUT.tangentWS = normalize(mul((float3x3) unity_ObjectToWorld, IN.tangentOS.xyz));
    float sign = IN.tangentOS.w * unity_WorldTransformParams.w;
    OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * sign;
    
    return OUT;
}

half4 frag(Varyings IN) : SV_Target
{
    float3 positionWS = mul(unity_ObjectToWorld, float4(IN.positionOS, 1.0)).xyz;
    float3 normalWS = GetNormalCombine(
        TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap), // ← マクロで渡す
        IN.uv,
        IN.normalWS,
        IN.tangentWS,
        IN.bitangentWS,
        _NormalMapIntensity
    );
    
    normalWS = _IsForFace ? GetFaceNormal(_FaceUp, normalWS) : normalWS;
    
    float3 color;
    GetLights_float(_ColorLit, _ColorMiddle, _ColorShadow, positionWS, normalWS, color);
    
    float backLight, rimLightFront, rimLightBack;
    GetFresnel(IN.normalWS, GetWorldSpaceNormalizeViewDir(positionWS), backLight, rimLightFront, rimLightBack);;
    color += backLight * _FresnelBackLight;
    color += rimLightBack * _FresnelBackRimLight;
    color += rimLightFront * _FresnelFrontRimLight;
    
    half4 result = (half4) (float4(color, 1)) * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
    result.w = _Alpha;
    return result;
}
#endif