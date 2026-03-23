#ifndef LUMINANCE_TO_OUTLINE_THICKNESS_INCLUDED
#define LUMINANCE_TO_OUTLINE_THICKNESS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _SHADOWS_SOFT


void MainLight_float(
    float3 positionWS,
    out float3 Direction,
    out float ShadowAtten)
{
    Direction = _MainLightPosition.xyz;

    half cascadeIndex = ComputeCascadeIndex(positionWS);
    float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));

    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    half4 shadowParams = GetMainLightShadowParams();
    shadowParams.x = 1.0;

    // SampleShadowmapを使わず直接フィルタリング関数を呼ぶ
    ShadowAtten = SampleShadowmapFiltered(
        TEXTURE2D_SHADOW_ARGS(_MainLightShadowmapTexture, sampler_LinearClampCompare),
        shadowCoord,
        shadowSamplingData
    );

    // ShadowStrengthを適用
    ShadowAtten = LerpWhiteTo(ShadowAtten, shadowParams.x);
}

void GetLights_float(
    float3 positionWS,
    float3 normalWS,
    out float bright
)
{
    float3 sunNormal;
    float sunShadowAtten;
    MainLight_float(positionWS, sunNormal, sunShadowAtten);
    bright = saturate(dot(sunNormal, normalWS)) * sunShadowAtten;
    
    int lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, positionWS, 1);

        float atten = light.shadowAttenuation * light.distanceAttenuation;
        float NdotL = saturate(dot(normalWS, light.direction));
        
        bright += min(NdotL * atten, 1);
    }
}

float GetOutlineThicknessRatio(float3 positionOS,float3 normalOS)
{
    float3 positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
    float3 normalWS = normalize(mul((float3x3) unity_ObjectToWorld, normalOS));
    
    float result;
    GetLights_float(positionWS, normalWS, result);
    return (saturate(result) + 1) / 2;
}
#endif