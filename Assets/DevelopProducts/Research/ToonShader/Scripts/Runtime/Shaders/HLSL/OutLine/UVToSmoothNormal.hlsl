#ifndef UV_TO_SMOOTH_NORMAL_INCLUDED
#define UV_TO_SMOOTH_NORMAL_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

float3 UVToSmoothNormal(float2 uv)
{
    float z = sqrt(max(0, 1 - saturate(dot(uv.xy, uv.xy))));
    return float3(uv.x, uv.y, z);
}
float3 NormalTSToOS(float3 smoothNormalTS, float3 normalOS, float4 tangentOS)
{
    float3 normalWS = TransformObjectToWorldNormal(normalOS);
    float3 tangentWS = TransformObjectToWorldDir(tangentOS.xyz);
    float3 bitangentWS = cross(normalWS,tangentWS) * tangentOS.w;
    
    // ShaderGraphと同じ：World空間のTBNを使う
    float3x3 tangentTransform = float3x3(tangentWS, bitangentWS, normalWS);
    float3 smoothNormalWS = TransformTangentToWorld(smoothNormalTS, tangentTransform, false);

    // World → Object
    return TransformWorldToObjectNormal(smoothNormalWS, true);
}


float3 GetSmoothNormalFromUV(float2 uv, float3 normalOS, float4 tangentOS)
{
    float3 smoothNormalTS = UVToSmoothNormal(uv);
    return NormalTSToOS(smoothNormalTS, normalOS, tangentOS);
}

#endif