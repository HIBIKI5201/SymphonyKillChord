#ifndef OUTLINE_INCLUDED
#define OUTLINE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


float _IsForFace;

float4 _OutlineColor = float4(0, 0, 0, 1);

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 pos : SV_POSITION;
};

v2f vert(appdata v)
{
    v2f o;
    float3 pushed = v.vertex.xyz + v.normal * 0.1;
    o.pos = TransformObjectToHClip(float4(pushed, 1.0));
    o.pos.z -= _IsForFace ? 0.0005 * o.pos.w : 0;
    return o;
}

float4 frag(v2f i) : SV_Target
{
    return _OutlineColor;
}

#endif