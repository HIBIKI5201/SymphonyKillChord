#ifndef UV_TO_SMOOTH_NORMAL_INCLUDED
#define UV_TO_SMOOTH_NORMAL_INCLUDED

float3 GetSmoothNormalFromUV(float2 uv)
{
    float z = sqrt(max(0, 1 - uv.x * uv.x - uv.y * uv.y));
    return float3(uv.x, uv.y, z);
}

float3 GetViewZeroZ(float3 Position_View)
{
    return normalize(float3(Position_View.x, Position_View.y, max(0, Position_View.z)));
}

#endif