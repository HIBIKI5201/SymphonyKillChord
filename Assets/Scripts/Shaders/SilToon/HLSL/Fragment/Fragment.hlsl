
#ifndef FRAGMENT_INCLUDED
#define FRAGMENT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/SilToonInput.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Lights.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/SimplifiedSSS.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/ToonPBR.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/SilToonFresnel.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/FaceLight.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/NormalCombine.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/PerspectiveRemoval/PerspectiveRemoval.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Dither/Dither.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
#ifdef _NORMALMAP
    float4 tangentOS : TANGENT;
#endif
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
#ifdef _NORMALMAP
    half3 tangentWS : TEXCOORD3;
    half3 bitangentWS : TEXCOORD4;
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

#ifdef _NORMALMAP
    OUT.tangentWS = (half3) TransformObjectToWorldDir(IN.tangentOS.xyz);

    half sign = (half) (IN.tangentOS.w * unity_WorldTransformParams.w);
    OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * sign;
#endif

    return OUT;
}

half4 frag(Varyings IN) : SV_Target
{
#ifdef _NORMALMAP
    half3 normalWS = (half3) GetNormalCombine(
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
    normalWS = (half3) GetFaceNormal(_FaceUp, (float3) normalWS);
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

#ifdef _PBR_ON
    color = ApplyToonPBR(color, baseColor.rgb, IN.uv, IN.positionWS, normalWS, viewDirWS,
                         GetNormalizedScreenSpaceUV(IN.positionHCS));
#else
    color *= baseColor.rgb;
#endif

    return half4(color, baseColor.a);
}
#endif
