#ifndef NORMAL_COMBINE_INCLUDED
#define NORMAL_COMBINE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

float3 GetNormalCombine(
    TEXTURE2D_PARAM( normalMap, samplerNormalMap),
    float2 uv,
    float3 normalWS,
    float3 tangentWS,
    float3 bitangentWS,
    float strength)
{
    float4 normalSample = SAMPLE_TEXTURE2D(normalMap, samplerNormalMap, uv);
    float3 normalTS = UnpackNormal(normalSample);

    normalTS.xy *= strength;

    float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);
    return normalize(mul(normalTS, TBN));
}
#endif