#ifndef LIGHTS_INCLUDED
#define LIGHTS_INCLUDED


#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


#endif

void MainLight_float(
    float3 positionWS,
    out float3 Direction,
    out float3 Color,
    out float ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction     = float3(0.5, 0.5, 0);
    Color         = float3(1, 1, 1);
    DistanceAtten = 1.0;
    ShadowAtten   = 1.0;
#else
    Direction = _MainLightPosition.xyz;
    Color = _MainLightColor.rgb;

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
#endif
}

float3 GetToonColor(
    float3 mainColor,
    float3 outerColor,
    float3 shadowColor,
    float bright,
    float shadowAtten
)
{
    bright = saturate(bright);
    shadowAtten = saturate(shadowAtten);
    float main = smoothstep(0, 0.5, bright) * shadowAtten;
    float outer = smoothstep(0, 0.1, bright) * shadowAtten;
    
    return lerp(lerp(shadowColor, outerColor, outer.rrr), mainColor, main.rrr);
}
float3 GetToonColorAdditional(
    float3 mainColor,
    float bright,
    float shadowAtten
)
{
    bright = saturate(bright);
    shadowAtten = saturate(shadowAtten);
    float main = smoothstep(0, 0.2, bright) * shadowAtten;
    
    return lerp(float3(0, 0, 0), mainColor, main.rrr);
}

void GetLights_float(
    float3 mainColor,
    float3 outerColor,
    float3 shadowColor,
    float3 positionWS,
    float3 normalWS,
    out float3 color
)
{
#ifdef SHADERGRAPH_PREVIEW
    color = float3(0, 0, 0);
#else
    float3 sunNormal, sunColor;
    float sunShadowAtten;
    MainLight_float(positionWS, sunNormal, sunColor, sunShadowAtten);
    color = sunColor * GetToonColor(mainColor, outerColor, shadowColor, saturate(dot(sunNormal, normalWS)), sunShadowAtten);
    
    int lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, positionWS, 1);

        float atten = light.shadowAttenuation * light.distanceAttenuation;
        float NdotL = saturate(dot(normalWS, light.direction));
        
        //float3 additionalColor = light.color * GetToonColor(mainColor, outerColor, shadowColor, NdotL, atten);
        float3 additionalColor = min(light.color * GetToonColorAdditional(mainColor, NdotL, atten), float3(1,1,1)) / 2;
        
        color += additionalColor;
    }
#endif
}
#endif