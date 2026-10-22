#ifndef CUSTOM_OUTLINE_INCLUDED
#define CUSTOM_OUTLINE_INCLUDED

void AdvancedOutline_float(
    float3 PositionOS,
    float3 NormalOS,
    float Thickness,
    float ThicknessShadow,
    float FadeDistance,
    float3 LightDirWS,
    out float3 OutPositionOS
)
{
    // ===== 空間変換 =====
    float3 positionWS = mul(unity_ObjectToWorld, float4(PositionOS, 1.0)).xyz;
    float3 normalWS = normalize(mul((float3x3)unity_ObjectToWorld, NormalOS));

    // ===== ライト方向による強弱 =====
    float ndl = saturate(-dot(normalWS, normalize(LightDirWS)));
    float lightFade = pow(1 - ndl,2.2); // 光が当たっている側は細く、影側は太く

    // ===== 距離フェード（任意） =====
    float camDistance = distance(positionWS, _WorldSpaceCameraPos);
    float distanceFade = saturate(1.0 - camDistance / FadeDistance);
    
    
    // ===== アウトラインオフセット =====
    // 法線方向で押し出す方式に変更（カメラ距離に依存しない）
    float3 offsetOS = NormalOS * lerp(Thickness, ThicknessShadow, lightFade) * distanceFade;
    OutPositionOS = PositionOS + offsetOS;
}


#endif