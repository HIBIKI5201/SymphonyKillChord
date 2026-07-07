#ifndef SIMPLIFIED_SSS_INCLUDED
#define SIMPLIFIED_SSS_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/CharacterShadow.hlsl"
#endif

half3 _SSSWrappedDiffuse(half NdotL, half sssWrap, half3 lightColor, half shadowAtten)
{
    half wrappedNdotL = saturate((NdotL + sssWrap) / (1.0h + sssWrap));
    return wrappedNdotL * shadowAtten * lightColor;
}

half3 _SSSScatter(half NdotL, half sssWrap, half3 sssColor, half3 lightColor, half shadowAtten, half sssIntensity)
{
    half wrappedNdotL = saturate((NdotL + sssWrap) / (1.0h + sssWrap));
    half sssTerm = wrappedNdotL - saturate(NdotL);
    return sssTerm * shadowAtten * sssColor * lightColor * sssIntensity;
}

half3 _SSSTransmission(half3 viewDir, half3 lightDir, half3 sssColor, half3 lightColor, half thickness, half power, half sssIntensity)
{
    half VdotL = dot(viewDir, -lightDir);
    return pow(saturate(VdotL), power) * thickness * sssColor * lightColor * sssIntensity;
}

#ifndef SHADERGRAPH_PREVIEW
half3 GetSSSAdditionalLight(half3 normalWS, Light light)
{
    half atten = half(light.shadowAttenuation * light.distanceAttenuation);
    half NdotL = saturate(dot(normalWS, half3(light.direction)));

    return min(half3(light.color) * NdotL * atten, half3(1, 1, 1)) / 2.0h;
}
#endif

void GetSSSLights(
    half3 normalWS,
    float3 positionWS,
    half3 viewDirWS,
    half3 sssColor,
    half sssWrap,
    half sssIntensity,
    half thickness,
    half transmissionPower,
    float2 normalizedScreenSpaceUV,
    out half3 color
)
{
#ifdef SHADERGRAPH_PREVIEW
    color = half3(0, 0, 0);
#else
    Light mainLight = GetMainLight();
    half3 lightDir = mainLight.direction;
    half3 lightColor = mainLight.color;

    // シャドウの受け取りはGetToonMainLightと同じ流儀に揃える
    // (メインマップは受け取りバイアス0.15で自己投影を棄却、セルフシャドウは専用マップ)
    half shadowAtten = 1.0h;
#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    shadowAtten = MainLightRealtimeShadow(TransformWorldToShadowCoord(positionWS + mainLight.direction * 0.15));
#endif
#if defined(_CHAR_SHADOW_ON)
    shadowAtten = min(shadowAtten, SampleCharacterShadow(positionWS));
#endif

    half NdotL = dot(normalWS, lightDir);

    color  = _SSSWrappedDiffuse(NdotL, sssWrap, lightColor, shadowAtten);
    color += _SSSScatter(NdotL, sssWrap, sssColor, lightColor, shadowAtten, sssIntensity);
    color += _SSSTransmission(viewDirWS, lightDir, sssColor, lightColor, thickness, transmissionPower, sssIntensity);

#if USE_CLUSTER_LIGHT_LOOP
    // Forward+(Cluster)ではメインライト以外の平行光源はクラスタに含まれないため先に別ループで処理する
    [loop] for (uint dirIndex = 0u; dirIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); dirIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
        Light light = GetAdditionalLight(dirIndex, positionWS, half4(1, 1, 1, 1));
        color += GetSSSAdditionalLight(normalWS, light);
    }
#endif

    // LIGHT_LOOP_BEGIN のクラスタ版は inputData という名前のローカル変数を参照する
    InputData inputData = (InputData) 0;
    inputData.positionWS = positionWS;
    inputData.normalizedScreenSpaceUV = normalizedScreenSpaceUV;

    uint lightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(lightCount)
        Light light = GetAdditionalLight(lightIndex, positionWS, half4(1, 1, 1, 1));
        color += GetSSSAdditionalLight(normalWS, light);
    LIGHT_LOOP_END
#endif
}

#endif
