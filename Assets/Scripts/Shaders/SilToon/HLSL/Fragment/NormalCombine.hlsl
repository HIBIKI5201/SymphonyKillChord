#ifndef NORMAL_COMBINE_INCLUDED
#define NORMAL_COMBINE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

half3 GetNormalCombine(
    TEXTURE2D_PARAM( normalMap, samplerNormalMap),
    float2 uv,
    half3 normalWS,
    half3 tangentWS,
    half3 bitangentWS,
    half strength)
{
    half4 normalSample = SAMPLE_TEXTURE2D(normalMap, samplerNormalMap, uv);
    half3 normalTS = UnpackNormal(normalSample);

    normalTS.xy *= strength;

    half3x3 TBN = half3x3(tangentWS, bitangentWS, normalWS);
    return  strength <= 0 ? normalWS : normalize(mul(normalTS, TBN));
}
#endif