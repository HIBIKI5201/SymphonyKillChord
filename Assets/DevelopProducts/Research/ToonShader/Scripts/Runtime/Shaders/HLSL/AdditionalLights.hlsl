#ifndef ADDITIONAL_LIGHTS_INCLUDED
#define ADDITIONAL_LIGHTS_INCLUDED


#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _SHADOWS_SOFT

#endif

void GetAdditionalLights_float(
    float3 WorldPos,
    float3 WorldNormal,
    out float3 DiffuseLight)
{
    DiffuseLight = float3(0, 0, 0);

    int lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, WorldPos, 1);

        float atten = light.shadowAttenuation * light.distanceAttenuation;
        float NdotL = saturate(dot(WorldNormal, light.direction));
        DiffuseLight += light.color * atten * NdotL;
    }
}

void GetReceiveShadow_float(float ShadowAlpha, float3 PositionWS, out half ShadowAttenuation)
{
#ifdef SHADERGRAPH_PREVIEW
    ShadowAttenuation = 1.0;
    
#else
    half4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
    Light mainLight = GetMainLight(shadowCoord);
    half shadow = mainLight.shadowAttenuation;
    int pixelLightCount = GetAdditionalLightsCount();

    for (int i = 0; i < pixelLightCount; i++)
    {
        Light AddLight0 = GetAdditionalLight(i, PositionWS, 1);
        half shadow0 = AddLight0.shadowAttenuation;
        shadow *= shadow0;
    }

    ShadowAttenuation = shadow * ShadowAlpha;

#endif
}


#endif