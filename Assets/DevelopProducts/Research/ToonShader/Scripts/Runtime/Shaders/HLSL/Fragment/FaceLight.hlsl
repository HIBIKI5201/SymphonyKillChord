#ifndef FACE_LIGHT_INCLUDED
#define FACE_LIGHT_INCLUDED

float3 GetFaceNormal(float3 faceUpWS, float3 normalWS)
{
    return normalize(-dot(faceUpWS, normalWS) * faceUpWS + normalWS);
}

#endif