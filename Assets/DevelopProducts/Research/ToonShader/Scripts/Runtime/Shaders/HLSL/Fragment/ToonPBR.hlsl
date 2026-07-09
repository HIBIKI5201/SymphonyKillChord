#ifndef TOON_PBR_INCLUDED
#define TOON_PBR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/SilToonInput.hlsl"
#include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/Lights.hlsl"

// トゥーンランプ(リム込み)の上にPBRのスペキュラ/環境反射を合成する。
// 拡散はトゥーン側の結果を brdfData.diffuse (アルベド×非金属率) で減衰させることで
// 金属部のエネルギー保存を成立させる。
half3 ApplyToonPBR(
    half3 toonLight,
    half3 albedo,
    float2 uv,
    float3 positionWS,
    half3 normalWS,
    half3 viewDirWS,
    float2 normalizedScreenSpaceUV)
{
    half metallic = half(SAMPLE_TEXTURE2D(_MetallicMap, sampler_BaseMap, uv).r) * _Metallic;
    half roughness = half(SAMPLE_TEXTURE2D(_RoughnessMap, sampler_BaseMap, uv).r) * _Roughness;
    half smoothness = 1.0h - roughness;

    BRDFData brdfData;
    half alpha = 1.0h;
    InitializeBRDFData(albedo, metallic, half3(0.0h, 0.0h, 0.0h), smoothness, alpha, brdfData);

    half3 color = toonLight * brdfData.diffuse;

    // 直接スペキュラ(メインライト)。影の受け取りはトゥーン側と同じ
    // (受け取りバイアス+キャラ専用シャドウ)に揃える
    half3 lightDir, lightColor;
    half shadowAtten, characterShadowAtten;
    GetToonMainLight(positionWS, normalWS, lightDir, lightColor, shadowAtten, characterShadowAtten);

    half NdotL = saturate(dot(normalWS, lightDir));
    // スペキュラは通常影・キャラ影のどちらでも抑制する
    half3 radiance = lightColor * (NdotL * min(shadowAtten, characterShadowAtten));
    color += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDir, viewDirWS)
             * radiance * _SpecularIntensity;

    // 環境反射(リフレクションプローブ)。金属・革の「置かれた場所の色を拾う」質感はここで出る
    half3 reflectVector = reflect(-viewDirWS, normalWS);
    half NdotV = saturate(dot(normalWS, viewDirWS));
    half fresnelTerm = half(Pow4(1.0h - NdotV));
    half3 indirectSpecular = GlossyEnvironmentReflection(
        reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h, normalizedScreenSpaceUV);
    color += EnvironmentBRDFSpecular(brdfData, fresnelTerm) * indirectSpecular * _EnvReflectionIntensity;

    return color;
}

#endif
