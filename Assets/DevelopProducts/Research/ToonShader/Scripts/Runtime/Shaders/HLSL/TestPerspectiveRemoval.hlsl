#ifndef CUSTOM_PERSPECTIVE_REMOVAL_INCLUDED
#define CUSTOM_PERSPECTIVE_REMOVAL_INCLUDED

void PerspectiveRemoval_float(
    float3 positionOS,
    float3 objectCenterOS,
    out float3 positionOS_out, // Object Space で返す
    out float fovHalfAngleDeg
)
{
    // Object Space → View Space
    float4 posVS = mul(UNITY_MATRIX_MV, float4(positionOS, 1.0));
    float4 centerVS = mul(UNITY_MATRIX_MV, float4(objectCenterOS, 1.0));

    // FOV半角計算
    float AC = -centerVS.z;
    float BC = length(posVS.xy - centerVS.xy);
    fovHalfAngleDeg = degrees(atan2(BC, AC));

    // Perspective Removal 補正（View Space上で）
    float scale = posVS.z / centerVS.z;
    posVS.xy *= scale;

    // View Space → World Space → Object Space へ戻す
    float4 worldPos = mul(UNITY_MATRIX_I_V, posVS);
    float4 posOS = mul(UNITY_MATRIX_I_M, worldPos);

    positionOS_out = posOS.xyz;
}
#endif