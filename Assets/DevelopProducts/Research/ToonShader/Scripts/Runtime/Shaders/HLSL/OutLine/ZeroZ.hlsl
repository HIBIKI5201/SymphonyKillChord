#ifndef ZERO_Z_INCLUDED
#define ZERO_Z_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


float3 GetViewZeroZ(float3 normalVS)
{
    return normalize(float3(normalVS.x, normalVS.y, max(0, normalVS.z)));
}

float3 GetViewZeroZ_OS(float3 normalOS)
{
    float3 normalWS = TransformObjectToWorldNormal(normalOS);
    float3 normalVS = TransformWorldToViewNormal(normalWS);
    
    
    normalWS = mul((float3x3) UNITY_MATRIX_I_V, normalVS); // VS→WS
    return TransformWorldToObjectNormal(normalWS);
}

#endif