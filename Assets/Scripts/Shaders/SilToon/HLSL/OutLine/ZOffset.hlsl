#ifndef Z_OFFSET_INCLUDED
#define Z_OFFSET_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

float3 IncreaseZOffset(float3 positionOS, float offsetValue)
{
    float3 cameraPosOS = TransformWorldToObject(_WorldSpaceCameraPos);
    float3 toCameraOS = SafeNormalize(cameraPosOS - positionOS);
    return positionOS + toCameraOS * offsetValue;
}

#endif