#ifndef CHARACTER_SHADOW_INCLUDED
#define CHARACTER_SHADOW_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// SilToonShadowRenderPassがセットするグローバル定数。
// マテリアル定数ではないため UnityPerMaterial には入れないこと(SRP Batcher互換維持)。
TEXTURE2D_SHADOW(_CharShadowmap);
SAMPLER_CMP(sampler_CharShadowmap);
float4x4 _CharWorldToShadow;
float4 _CharShadowParams; // x = 影の強度(0で無効), y = 1/解像度
float3 _CharShadowLightDirection;

half SampleCharacterShadow(float3 positionWS,half3 normalWS)
{
    // 正射影なのでw除算は不要
    float3 coord = mul(_CharWorldToShadow, float4(positionWS, 1.0)).xyz * round(saturate(dot(normalWS, _CharShadowLightDirection)));

    // マップ範囲外は影なし扱い
    if (any(coord != saturate(coord)))
        return 1.0h;

    // 比較サンプラーの1タップでハードウェア2x2 PCFが効く
    half atten = half(SAMPLE_TEXTURE2D_SHADOW(_CharShadowmap, sampler_CharShadowmap, coord));

    return lerp(1.0h, atten, half(_CharShadowParams.x));
}

#endif
