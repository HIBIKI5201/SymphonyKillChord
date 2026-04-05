#ifndef CUSTOM_FACE_SHADOW_INCLUDED
#define CUSTOM_FACE_SHADOW_INCLUDED

void FaceShadow_float(
    float3 HeadForward,
    float3 HeadRight,
    float3 Light,
    out float OutShadow)
{
    float dotF = dot(HeadForward.xz, Light.xz);
    float dotR = dot(HeadRight.xz, Light.xz);
    
    float dotFStep = step(0.0, dotF);
    float dotRAcos = acos(dotR)/ 3.14 * 2;
    float dotRAcosDir = (dotR < 0) ? 1 - dotRAcos : (dotRAcos - 1);
    OutShadow = dotRAcos * dotFStep;
}
#endif