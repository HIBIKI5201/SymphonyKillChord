#ifndef Z_OFFSET_INCLUED
#define Z_OFFSET_INCLUED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

float3 IncreaseZOffset(float3 positionOS, float value)
{
    float3 positionWS = TransformObjectToWorld(positionOS);
    float3 toCameraWS = normalize(_WorldSpaceCameraPos - positionWS);
    positionWS += toCameraWS * value;

    // WS → HClip
    return mul(unity_WorldToObject, float4(positionWS, 1.0)).xyz;
}

#endif