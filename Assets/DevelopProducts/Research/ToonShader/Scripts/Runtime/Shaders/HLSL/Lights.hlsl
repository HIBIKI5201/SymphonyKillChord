#ifndef LIGHTS_INCLUDED
#define LIGHTS_INCLUDED


#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


#endif

void MainLight_float(
    float3 positionWS,
    out float3 Direction,
    out float3 Color,
    out float ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction   = float3(0.5, 0.5, 0);
    Color       = float3(1, 1, 1);
    ShadowAtten = 1.0;
#else

    Light mainLight = GetMainLight();
    Direction = mainLight.direction;
    Color = mainLight.color;

#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)

    ShadowAtten = MainLightRealtimeShadow(TransformWorldToShadowCoord(positionWS));

#else
    ShadowAtten = 1.0;
#endif

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

#ifndef SHADERGRAPH_PREVIEW
float3 GetAdditionalToonLight(float3 mainColor, float3 normalWS, Light light)
{
    float atten = light.shadowAttenuation * light.distanceAttenuation;
    float NdotL = saturate(dot(normalWS, light.direction));

    return min(light.color * GetToonColorAdditional(mainColor, NdotL, atten), float3(1, 1, 1)) / 2;
}
#endif

void GetLights_float(
    float3 mainColor,
    float3 outerColor,
    float3 shadowColor,
    float3 positionWS,
    float3 normalWS,
    float2 normalizedScreenSpaceUV,
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

#if USE_CLUSTER_LIGHT_LOOP
    // Forward+(Cluster)ではメインライト以外の平行光源はクラスタに含まれないため先に別ループで処理する
    [loop] for (uint dirIndex = 0u; dirIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); dirIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
        Light light = GetAdditionalLight(dirIndex, positionWS, half4(1, 1, 1, 1));
        color += GetAdditionalToonLight(mainColor, normalWS, light);
    }
#endif

    // LIGHT_LOOP_BEGIN のクラスタ版は inputData という名前のローカル変数を参照する
    InputData inputData = (InputData) 0;
    inputData.positionWS = positionWS;
    inputData.normalizedScreenSpaceUV = normalizedScreenSpaceUV;

    uint lightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(lightCount)
        Light light = GetAdditionalLight(lightIndex, positionWS, half4(1, 1, 1, 1));
        color += GetAdditionalToonLight(mainColor, normalWS, light);
    LIGHT_LOOP_END
#endif
}
#endif
