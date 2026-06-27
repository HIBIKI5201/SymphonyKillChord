#ifndef SILTOON_INPUT_INCLUDED
#define SILTOON_INPUT_INCLUDED

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
float4 _BaseMap_ST;

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
float4 _NormalMap_ST;

half _NormalMapIntensity;

float _PerspectiveRemovalRatio;
float _PerspectiveRemovalRadius;
float3 _Head;

half _IsForFace;
float3 _FaceUp;

half4 _ColorLit;
half4 _ColorMiddle;
half4 _ColorShadow;

half _FresnelBackLight;
half _FresnelFrontRimLight;
half _FresnelBackRimLight;

#endif
