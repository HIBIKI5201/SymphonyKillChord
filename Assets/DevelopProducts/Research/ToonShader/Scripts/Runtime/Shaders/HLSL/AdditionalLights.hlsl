#ifndef ADDITIONAL_LIGHTS_INCLUDED
#define ADDITIONAL_LIGHTS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void GetAdditionalLights_float(
    float3 WorldPos,
    float3 WorldNormal,
    float3 WorldView,
    out float3 DiffuseLight)
{
    DiffuseLight = float3(0, 0, 0);

#ifndef SHADERGRAPH_PREVIEW
    InputData inputData = (InputData) 0;
    inputData.positionWS = WorldPos;
    inputData.normalWS = normalize(WorldNormal);
    inputData.viewDirectionWS = normalize(WorldView);
    inputData.shadowMask = unity_ProbesOcclusion;
    inputData.bakedGI = float3(0, 0, 0);

    int lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, WorldPos, inputData.shadowMask);

        float atten = light.distanceAttenuation * light.shadowAttenuation;
        float NdotL = saturate(dot(WorldNormal, light.direction));
        DiffuseLight += light.color * atten * NdotL;
    }
#endif
}

#endif