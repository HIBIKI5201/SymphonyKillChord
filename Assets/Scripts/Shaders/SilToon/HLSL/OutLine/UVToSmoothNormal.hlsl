#ifndef UV_TO_SMOOTH_NORMAL_INCLUDED
#define UV_TO_SMOOTH_NORMAL_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

half3 UVToSmoothNormal(half2 uv)
{
    half z = sqrt(max(0.0h, 1.0h - saturate(dot(uv, uv))));
    return half3(uv.x, uv.y, z);
}

half3 NormalTSToOS(half3 smoothNormalTS, half3 normalOS, half4 tangentOS)
{
    half3 normalWS = half3(TransformObjectToWorldNormal(float3(normalOS)));
    half3 tangentWS = half3(TransformObjectToWorldDir(float3(tangentOS.xyz)));
    half3 bitangentWS = cross(normalWS, tangentWS) * tangentOS.w;
    
    half3x3 tangentTransform = half3x3(tangentWS, bitangentWS, normalWS);
    
    half3 smoothNormalWS = mul(smoothNormalTS, tangentTransform);
    
    return half3(TransformWorldToObjectNormal(float3(smoothNormalWS)));
}

half3 GetSmoothNormalFromUV(half2 uv, half3 normalOS, half4 tangentOS)
{
    half3 smoothNormalTS = UVToSmoothNormal(uv);
    return NormalTSToOS(smoothNormalTS, normalOS, tangentOS);
}

#endif