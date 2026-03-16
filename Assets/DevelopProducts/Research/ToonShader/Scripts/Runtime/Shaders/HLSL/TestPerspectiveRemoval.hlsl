#ifndef CUSTOM_PERSPECTIVE_REMOVAL_INCLUDED

#define CUSTOM_PERSPECTIVE_REMOVAL_INCLUDED

void PerspectiveRemoval_float(
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

#endif