#ifndef SMEARS_INCLUDED
#define SMEARS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


// UV座標からハッシュ値を生成
float Hash21(float2 uv)
{
    uv = frac(uv * 1.1234567f);
    uv += dot(uv, uv + 2.1234f);
    return frac(uv.x * uv.y);
}

// UV座標からValue Noiseを生成
float ValueNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f); // Cubic smoothstep

    float a = Hash21(i);
    float b = Hash21(i + float2(1, 0));
    float c = Hash21(i + float2(0, 1));
    float d = Hash21(i + float2(1, 1));
    float result = lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
    
    result *= result;
    return result;
}

// UV座標からノイズを生成し、vertex座標に加算する
void ApplySmear(
    float3 positionOS,
    float2 uv,
    float3 normalOS,
    float3 smearDirectionWS,
    float smearsPower,
    out float3 smearOS,
    out float alpha)
{
    float3 smearDirectionOS =  TransformWorldToObject(smearDirectionWS);
    
    float offset = frac(_Time.y / 50) * 100;
    float noise = ValueNoise(uv * 100 + offset) * ValueNoise(uv * 10);
    noise = saturate(noise * 1.1);
    noise *= noise;
    float3 direction = noise * smearDirectionOS * saturate(dot(normalOS, smearDirectionOS));

    smearOS = positionOS + direction * smearsPower;
    
    alpha = saturate(1 - noise * 2);
    alpha *= alpha * alpha * alpha * alpha * alpha;
    alpha = max(0.01f, alpha);
}

#endif
