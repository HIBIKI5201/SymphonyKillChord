#ifndef SILTOON_SHEEN_INCLUDED
#define SILTOON_SHEEN_INCLUDED

// 薄手生地(タイツ等)の透け。
// 正面から見るほど下地(肌)が透け、輪郭ほど生地が重なって本来の色が濃く出る。
half3 ApplySheer(half3 albedo, half3 sheerColor, half sheerAmount, half sheerPower, half NdotV)
{
    half sheer = pow(NdotV, sheerPower) * sheerAmount;
    return lerp(albedo, sheerColor, sheer);
}

// 布の毛羽立ちによる輪郭の光沢(シーン)。
// 暗い生地でも形が読めるよう、光の当たる側の輪郭だけを明るくする。
half3 GetSheen(half3 lightColor, half shadow, half3 sheenColor, half sheenPower, half NdotL, half NdotV)
{
    half fresnel = pow(1.0h - NdotV, sheenPower);
    half wrap = saturate(NdotL * 0.5h + 0.5h);
    return fresnel * wrap * wrap * shadow * lightColor * sheenColor;
}

#endif
