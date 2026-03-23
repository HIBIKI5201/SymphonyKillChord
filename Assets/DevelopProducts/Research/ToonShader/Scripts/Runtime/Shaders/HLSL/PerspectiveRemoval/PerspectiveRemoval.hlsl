#ifndef PERSPECTIVE_REMOVAL_INCLUDED
#define PERSPECTIVE_REMOVAL_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

float GetCameraDot(float3 headWS)
{
    float3 cameraPos = _WorldSpaceCameraPos;
    float3 cameraForward = normalize(-UNITY_MATRIX_V[2].xyz);
    
    float result = saturate(dot(normalize(headWS - cameraPos), cameraForward));
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
    
    
    float4 worldPos = mul(UNITY_MATRIX_I_V, posVS);
    float4 posOS = mul(UNITY_MATRIX_I_M, worldPos);
    positionOS_out = posOS.xyz;
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
    
    float t = GetCameraDot(headWS) * ratio * (1 - saturate(distance(headOS, positionOS) / radius));
    
    return lerp(positionOS, positionOS_out, t);
}
#endif