#ifndef LUMINANCE_TO_OUTLINE_THICKNESS_INCLUDED
#define LUMINANCE_TO_OUTLINE_THICKNESS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _SHADOWS_SOFT


void MainLight_half(
    float3 positionWS,
    out half3 Direction,
    out half ShadowAtten)
{
    Direction = half3(_MainLightPosition.xyz);

    half cascadeIndex = ComputeCascadeIndex(positionWS);
    float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));

    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();

    ShadowAtten = (half) SampleShadowmapFiltered(
        TEXTURE2D_SHADOW_ARGS(_MainLightShadowmapTexture, sampler_LinearClampCompare),
        shadowCoord,
        shadowSamplingData
    );
}

void GetLights_half(
    float3 positionWS,
    half3 normalWS,
    out half bright)
{
    half3 sunDir;
    half sunShadowAtten;
    MainLight_half(positionWS, sunDir, sunShadowAtten);

    bright = saturate(dot(sunDir, normalWS)) * sunShadowAtten;

    int lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, positionWS, half4(1, 1, 1, 1));

        half atten = (half) (light.shadowAttenuation * light.distanceAttenuation);
        half NdotL = saturate(dot(normalWS, half3(light.direction)));
        
        bright = saturate(bright + NdotL * atten);
    }
}

half GetOutlineThicknessRatio(float3 positionOS, half3 normalOS)
{
    float3 positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
    
    half3 normalWS = normalize(half3(mul((half3x3) unity_ObjectToWorld, normalOS)));

    half result;
    GetLights_half(positionWS, normalWS, result);
    
    return mad(saturate(result), 0.5h, 0.5h);
}
#endif