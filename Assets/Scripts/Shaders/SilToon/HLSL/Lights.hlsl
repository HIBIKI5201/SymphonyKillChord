#ifndef LIGHTS_INCLUDED
#define LIGHTS_INCLUDED


#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/CharacterShadow.hlsl"


#endif

void GetToonMainLight(
    float3 positionWS,
    half3 normalWS,
    out half3 direction,
    out half3 color,
    out half shadowAtten,
    out half characterShadowAtten
)
{
    characterShadowAtten = 1.0h;
    direction   = half3(0.5, 0.5, 0);
    color       = half3(1, 1, 1);
    shadowAtten = 1.0h;
    return;

    Light mainLight = GetMainLight();
    direction = mainLight.direction;
    color = mainLight.color;

#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)

    shadowAtten = MainLightRealtimeShadow(TransformWorldToShadowCoord(positionWS + mainLight.direction * 0.5));

#else
    shadowAtten = 1.0h;
#endif

#if defined(_CHAR_SHADOW_ON)
    // キャラ専用シャドウマップによるセルフシャドウ。
    // メインライト側の受け取りバイアス(自己投影の棄却)はこちらには適用しない
    //shadowAtten = saturate(max(0.2, shadowAtten) * min(SampleCharacterShadow(positionWS),0.7));
    characterShadowAtten = SampleCharacterShadow(positionWS, normalWS);
#endif
}

void GetMainLightShadowAtten(
    Light light,
    float3 positionWS,
    half3 normalWS,
    out half shadowAtten,
    out half characterShadowAtten
)
{
    shadowAtten = 1.0h;
    characterShadowAtten = 1.0h;
#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    shadowAtten = MainLightRealtimeShadow(TransformWorldToShadowCoord(positionWS + light.direction * 0.5));
#endif

#if defined(_CHAR_SHADOW_ON)
    characterShadowAtten = SampleCharacterShadow(positionWS, normalWS);
#endif
}

half3 GetToonColor(
    half3 mainColor,
    half3 outerColor,
    half3 shadowColor,
    half bright,
    half shadowAtten,
    half characterShadowAtten
)
{
    half atten = saturate(1.0 - ((1.0 - shadowAtten) * 0.5 + (1.0 - characterShadowAtten) * 0.7));
    bright = max(0.2, saturate(bright));
    half main = saturate( smoothstep(0.0h, 0.5h, bright) * atten);
    half outer = saturate( smoothstep(0.0h, 0.1h, bright) * atten);

    return lerp(lerp(shadowColor, outerColor, outer), mainColor, main);
}

void AddSumLight(inout half3 color, inout half3 direction, Light light, half3 normalWS)
{
    half factor = saturate(dot(normalWS, light.direction) + 0.5);
    half3 lightColor = light.color * saturate(light.shadowAttenuation * light.distanceAttenuation);
    direction += light.direction * dot(lightColor,0.333333) * factor;
    color += lightColor * factor;
}
half easeOutQuad(half x)
{
    return 1.0 - (1.0 - x) * (1.0 - x);
}

void GetToonLights(
    half3 mainColor,
    half3 outerColor,
    half3 shadowColor,
    float3 positionWS,
    half3 normalWS,
    float2 normalizedScreenSpaceUV,
    out half3 color
)
{
#ifdef SHADERGRAPH_PREVIEW
    color = half3(0, 0, 0);
#else
    color = SampleSH(normalWS);
    half3 dir = half3(0, 0, 0);
    Light mainLight = GetMainLight();
    AddSumLight(color, dir, mainLight, normalWS);
    half sunShadowAtten, characterShadowAtten;
    GetMainLightShadowAtten(mainLight, positionWS, normalWS, sunShadowAtten, characterShadowAtten);
    
#if USE_CLUSTER_LIGHT_LOOP
    // Forward+(Cluster)ではメインライト以外の平行光源はクラスタに含まれないため先に別ループで処理する
    [loop] for (uint dirIndex = 0u; dirIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); dirIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
        Light light = GetAdditionalLight(dirIndex, positionWS, half4(1, 1, 1, 1));
        AddSumLight(color, dir, light, normalWS);
    }
#endif

    // LIGHT_LOOP_BEGIN のクラスタ版は inputData という名前のローカル変数を参照する
    InputData inputData = (InputData) 0;
    inputData.positionWS = positionWS;
    inputData.normalizedScreenSpaceUV = normalizedScreenSpaceUV;

    uint lightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(lightCount)
        Light light = GetAdditionalLight(lightIndex, positionWS, half4(1, 1, 1, 1));
        AddSumLight(color, dir, light, normalWS);
    LIGHT_LOOP_END
    
    
    dir = SafeNormalize(dir);

    //color *= saturate(sunShadowAtten * min(0.2, characterShadowAtten));
    half bright = saturate((dot(dir, normalWS))) * easeOutQuad(saturate(sunShadowAtten * characterShadowAtten));
    color *= GetToonColor(mainColor, outerColor, shadowColor, bright, 1.0h, 1.0h);
    color = clamp(color, shadowColor, 5.0h);
#endif
}
#endif
