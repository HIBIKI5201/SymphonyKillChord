
#ifndef FRAGMENT_INCLUDED
#define FRAGMENT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/SilToonInput.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Lights.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/SimplifiedSSS.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/ToonPBR.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/SilToonFresnel.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/Sheen.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/AnisotropicHighlight.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/OutLine/UVToSmoothNormal.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/FaceLight.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/NormalCombine.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/PerspectiveRemoval/PerspectiveRemoval.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Dither/Dither.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
#if defined(_NORMALMAP) || defined(_ANISO_ON)
    float4 tangentOS : TANGENT;
#endif
    float2 uv : TEXCOORD0;
#ifdef _ANISO_ON
    float2 uv3 : TEXCOORD3; // SmoothNormal (アウトラインと共用)
#endif
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
#if defined(_NORMALMAP) || defined(_ANISO_ON)
    half3 tangentWS : TEXCOORD3;
    half3 bitangentWS : TEXCOORD4;
#endif
#ifdef _ANISO_ON
    // 髪カードごとに法線が割れていると異方性ハイライトが途切れるため、平滑化法線で帯を繋げる
    half3 smoothNormalWS : TEXCOORD5;
#endif
};

Varyings vert(Attributes IN)
{
    Varyings OUT;
#ifdef _PERSPECTIVE_REMOVAL_ON
    float3 positionOS = GetPerspectiveRemoval(
        _Head, IN.positionOS.xyz, IN.normalOS,
        _PerspectiveRemovalRadius, _PerspectiveRemovalRatio);
#else
    float3 positionOS = IN.positionOS.xyz;
#endif

    OUT.positionHCS = TransformObjectToHClip(positionOS);

    OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);

    OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

    OUT.normalWS = (half3) TransformObjectToWorldNormal(IN.normalOS);

#if defined(_NORMALMAP) || defined(_ANISO_ON)
    OUT.tangentWS = (half3) TransformObjectToWorldDir(IN.tangentOS.xyz);

    half sign = (half) (IN.tangentOS.w * unity_WorldTransformParams.w);
    OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * sign;
#endif

#ifdef _ANISO_ON
    // 平滑化法線が焼かれていないメッシュでは uv3 = 0 となり、頂点法線と同じ向きに戻る
    OUT.smoothNormalWS = (half3) TransformObjectToWorldNormal(
        GetSmoothNormalFromUV((half2) IN.uv3, (half3) IN.normalOS, (half4) IN.tangentOS));
#endif

    return OUT;
}

half4 frag(Varyings IN) : SV_Target
{
#ifdef _NORMALMAP
    half3 normalWS = GetNormalCombine(
        TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap),
        IN.uv,
        IN.normalWS,
        IN.tangentWS,
        IN.bitangentWS,
        _NormalMapIntensity
    );
#else
    half3 normalWS = IN.normalWS;
#endif

#ifdef _ISFORFACE_ON
    normalWS = GetFaceNormal(_FaceUp, normalWS);
#endif

    half3 color;
#ifdef SSS_ON
    GetSSSLights(normalWS, IN.positionWS, (half3) GetWorldSpaceNormalizeViewDir(IN.positionWS),
                 _SSSColor.rgb, _SSSWrap, _SSSIntensity, _SSSThickness, _SSSTransmissionPower,
                 GetNormalizedScreenSpaceUV(IN.positionHCS), color);
#else
    GetToonLights(_ColorLit.rgb, _ColorMiddle.rgb, _ColorShadow.rgb, IN.positionWS, normalWS,
                  GetNormalizedScreenSpaceUV(IN.positionHCS), color);
#endif

    half3 viewDirWS = (half3) GetWorldSpaceNormalizeViewDir(IN.positionWS);

    half backLight, rimLightFront, rimLightBack;
    GetFresnel(IN.normalWS, viewDirWS,
               backLight, rimLightFront, rimLightBack);

    color += backLight * _FresnelBackLight;
    color += rimLightBack * _FresnelBackRimLight;
    color += rimLightFront * _FresnelFrontRimLight;

#ifdef FADE_ON
    clip(_FadeAlpha - BayerDither(IN.positionHCS.xy * 0.5) - 0.0001);
#endif

    half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

#if defined(_SHEEN_ON) || defined(_ANISO_ON)
    half NdotV = saturate(dot(normalWS, viewDirWS));
#endif

#ifdef _SHEEN_ON
    baseColor.rgb = ApplySheer(baseColor.rgb, _SheerColor.rgb, _SheerAmount, _SheerPower, NdotV);
#endif

#ifdef _PBR_ON
    color = ApplyToonPBR(color, baseColor.rgb, IN.uv, IN.positionWS, normalWS, viewDirWS,
                         GetNormalizedScreenSpaceUV(IN.positionHCS));
#else
    color *= baseColor.rgb;
#endif

#if defined(_SHEEN_ON) || defined(_ANISO_ON)
    // シーン/異方性ハイライトは光の反射なのでアルベドを掛けた後に加算する
    half3 mainLightDir, mainLightColor;
    half mainShadowAtten, mainCharShadowAtten;
    GetToonMainLight(IN.positionWS, normalWS, mainLightDir, mainLightColor, mainShadowAtten, mainCharShadowAtten);
    half mainShadow = min(mainShadowAtten, mainCharShadowAtten);
#endif

#ifdef _SHEEN_ON
    color += GetSheen(mainLightColor, mainShadow, _SheenColor.rgb, _SheenPower,
                      dot(normalWS, mainLightDir), NdotV);
#endif

#ifdef _ANISO_ON
    color += GetAnisotropicHighlight(normalize(IN.smoothNormalWS), normalize(IN.tangentWS), normalize(IN.bitangentWS),
                                     viewDirWS, mainLightDir, mainLightColor, mainShadow,
                                     _AnisoColor.rgb, _AnisoExponent, _AnisoShift, _AnisoAround);
#endif

    return half4(color, baseColor.a);
}
#endif
