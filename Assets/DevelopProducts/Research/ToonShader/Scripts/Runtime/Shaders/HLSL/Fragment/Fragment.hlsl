#ifndef FRAGMENT_INCLUDED
#define FRAGMENT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\Lights.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionOS : TEXCOORD1;
    float3 normalOS : TEXCOORD2;
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
float4 _BaseMap_ST;

half4 _ColorLit;
half4 _ColorMiddle;
half4 _ColorShadow;

Varyings vert(Attributes IN)
{
    Varyings OUT;
    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
    OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
    OUT.positionOS = IN.positionOS;
    OUT.normalOS = IN.normalOS;
    return OUT;
}

half4 frag(Varyings IN) : SV_Target
{
    float3 positionWS = mul(unity_ObjectToWorld, float4(IN.positionOS, 1.0)).xyz;
    float3 normalWS = normalize(mul((float3x3) unity_ObjectToWorld, IN.normalOS));
    
    float3 color;
    GetLights_float(_ColorLit, _ColorMiddle, _ColorShadow, positionWS, normalWS, color);
    return (half4) (float4(color, 1.0));
}
#endif