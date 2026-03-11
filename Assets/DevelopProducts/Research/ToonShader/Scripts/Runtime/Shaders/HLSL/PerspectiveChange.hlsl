#ifndef CUSTOM_PERSPECTIVE_CHANGE_INCLUDED
#define CUSTOM_PERSPECTIVE_CHANGE_INCLUDED

void PerspectiveChange_float(
    float3 Position,
    float PerspectiveAmount,
    out float3 Out)
{
    float3 viewPos = TransformWorldToView(TransformObjectToWorld(Position));
    float3 pivotViewPos = TransformWorldToView(TransformObjectToWorld(float3(0, 0, 0)));

    // ✅ 比率を逆に（ピボットのzで頂点のzを割る）
    float2 orthoXY = viewPos.xy * (pivotViewPos.z / viewPos.z);

    // 0=透視投影 / 1=正投影（直感的な方向に合わせて反転）
    float2 blendedXY = lerp(viewPos.xy, orthoXY, PerspectiveAmount);

    float3 blendedViewPos = float3(blendedXY, viewPos.z);

    float4 worldPos = mul(UNITY_MATRIX_I_V, float4(blendedViewPos, 1.0));
    Out = mul(UNITY_MATRIX_I_M, worldPos).xyz;
}
#endif