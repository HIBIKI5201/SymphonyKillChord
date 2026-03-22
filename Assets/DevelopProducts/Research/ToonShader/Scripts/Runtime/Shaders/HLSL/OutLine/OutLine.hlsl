#ifndef OUTLINE_INCLUDED
#define OUTLINE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\OutLine\UVToSmoothNormal.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\OutLine\ZeroZ.hlsl"
#include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\OutLine\ZOffset.hlsl"


float _ZOffset;
float _IsSmoothNormal;
float _OutlineWidth;

float4 _OutlineColor = float4(0, 0, 0, 1);

struct appdata
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv3 : TEXCOORD3;//SmoothNormal
};
struct v2f
{
    float4 pos : SV_POSITION;
};

v2f vert(appdata v)
{
    v2f o;
    
    float3 smoothNormalOS = GetSmoothNormalFromUV(v.uv3, v.normalOS, v.tangentOS);
    
    float3 normalOS = _IsSmoothNormal ? smoothNormalOS : v.normalOS;
    normalOS = GetViewZeroZ_OS(normalOS);
    
    float3 pushedOS = v.positionOS.xyz + normalOS * _OutlineWidth;
    pushedOS = IncreaseZOffset(pushedOS, -_ZOffset);
    
    o.pos = TransformObjectToHClip(float4(pushedOS, 1.0));
    return o;
}

float4 frag(v2f i) : SV_Target
{
    return _OutlineColor;
}

#endif