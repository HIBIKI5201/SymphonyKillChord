#ifndef ZERO_Z_INCLUDED
#define ZERO_Z_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

half3 GetViewZeroZ(half3 normalVS)
{
    return SafeNormalize(half3(normalVS.xy, max(0.0h, normalVS.z)));
}

float3 GetViewZeroZ_OS(float3 normalOS)
{
    float3 normalWS = TransformObjectToWorldNormal(normalOS);
    float3 normalVS = TransformWorldToViewNormal(normalWS);
    
    half3 correctedVS = GetViewZeroZ(half3(normalVS));
    
    float3 correctedWS = mul((float3x3)UNITY_MATRIX_I_V, float3(correctedVS));
    return TransformWorldToObjectNormal(correctedWS);
}

#endif