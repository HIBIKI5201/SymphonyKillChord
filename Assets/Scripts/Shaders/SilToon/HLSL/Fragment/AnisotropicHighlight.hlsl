#ifndef SILTOON_ANISOTROPIC_HIGHLIGHT_INCLUDED
#define SILTOON_ANISOTROPIC_HIGHLIGHT_INCLUDED

// Kajiya-Kay による異方性ハイライト。
// 繊維方向はメッシュのUV方向から取るため、UVを毛流れに沿って展開したモデルが前提。
//   around = 0 : 繊維 = V方向(髪。毛先が -V を向く短冊展開)   → 毛流れを横切る帯(天使の輪)
//   around = 1 : 繊維 = U方向(タイツ。脚の周方向に U を展開) → 脚に沿った縦筋
half3 GetAnisotropicHighlight(
    half3 normalWS,
    half3 tangentWS,
    half3 bitangentWS,
    half3 viewDirWS,
    half3 lightDirWS,
    half3 lightColor,
    half shadow,
    half3 highlightColor,
    half exponent,
    half shift,
    half around)
{
    // 根元→毛先 (-V) を正の向きに揃える。Shift で傾ける向きが符号で変わるため
    half3 fiber = lerp(-bitangentWS, tangentWS, around);
    // 平滑化法線に対して直交化し、髪カードごとの法線の割れに帯の形が引きずられないようにする
    fiber = SafeNormalize(fiber - normalWS * dot(normalWS, fiber));
    fiber = SafeNormalize(fiber + normalWS * shift);

    half3 halfDir = SafeNormalize(lightDirWS + viewDirWS);
    half TdotH = dot(fiber, halfDir);
    half sinTH = sqrt(saturate(1.0h - TdotH * TdotH));

    // 光の裏側に回った部分で帯が浮かないよう、ラップした NdotL で弱める
    half lit = saturate(dot(normalWS, lightDirWS) * 0.5h + 0.5h);
    return pow(sinTH, exponent) * lit * shadow * lightColor * highlightColor;
}

#endif
