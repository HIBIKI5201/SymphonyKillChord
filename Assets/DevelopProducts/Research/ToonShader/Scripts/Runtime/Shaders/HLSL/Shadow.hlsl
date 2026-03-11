#ifndef CUSTOM_SHADOW_INCLUDED
#define CUSTOM_SHADOW_INCLUDED

void MainLight_float(
    float3 positionWS,
    out float3 Direction,
    out float3 Color,
    out float DistanceAtten,
    out float ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction     = float3(0.5, 0.5, 0);
    Color         = float3(1, 1, 1);
    DistanceAtten = 1.0;
    ShadowAtten   = 1.0;
#else
    Direction = _MainLightPosition.xyz;
    Color = _MainLightColor.rgb;
    DistanceAtten = 1.0;

    half cascadeIndex = ComputeCascadeIndex(positionWS);
    float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));

    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    half4 shadowParams = GetMainLightShadowParams();
    shadowParams.x = 1.0;

    // SampleShadowmapを使わず直接フィルタリング関数を呼ぶ
    ShadowAtten = SampleShadowmapFiltered(
        TEXTURE2D_SHADOW_ARGS(_MainLightShadowmapTexture, sampler_LinearClampCompare),
        shadowCoord,
        shadowSamplingData
    );

    // ShadowStrengthを適用
    ShadowAtten = LerpWhiteTo(ShadowAtten, shadowParams.x);
#endif
}

#endif