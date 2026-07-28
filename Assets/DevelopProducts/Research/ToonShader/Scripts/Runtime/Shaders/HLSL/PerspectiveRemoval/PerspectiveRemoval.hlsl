#ifndef PERSPECTIVE_REMOVAL_INCLUDED
#define PERSPECTIVE_REMOVAL_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

float GetCameraDot(float3 headWS)
{
    float3 cameraPos = _WorldSpaceCameraPos;
    half3 cameraForward = SafeNormalize(half3(-UNITY_MATRIX_V[2].xyz));
    
    half3 headDirWS = SafeNormalize(half3(headWS - cameraPos));
    
    float result = saturate(dot(headDirWS, cameraForward));
    return result * result;
}

void CorePerspectiveRemoval(
    float3 positionOS,
    float3 objectCenterOS,
    out float3 positionOS_out
)
{
    float4 posVS = mul(UNITY_MATRIX_MV, float4(positionOS, 1.0));
    float4 centerVS = mul(UNITY_MATRIX_MV, float4(objectCenterOS, 1.0));
    
    float scale = posVS.z / min(centerVS.z, -0.001);
    posVS.xy *= scale;
    
    float4 posWS = mul(UNITY_MATRIX_I_V, posVS);
    positionOS_out = mul(UNITY_MATRIX_I_M, posWS).xyz;
}

float3 GetPerspectiveRemoval(
    float3 headWS,
    float3 positionOS,
    float3 normalOS,
    float radius,
    float ratio)
{
    float3 headOS = TransformWorldToObject(headWS);
    
    float3 positionOS_out;
    CorePerspectiveRemoval(positionOS, headOS, positionOS_out);
        
    half dist = half(distance(headOS, positionOS));
    half influence = saturate(1.0h - dist / half(max(radius, 0.0001f)));
    
    float t = GetCameraDot(headWS) * ratio * influence;
    
    return lerp(positionOS, positionOS_out, t);
}
#endif