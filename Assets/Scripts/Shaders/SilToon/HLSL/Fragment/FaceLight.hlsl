#ifndef FACE_LIGHT_INCLUDED
#define FACE_LIGHT_INCLUDED

half3 GetFaceNormal(half3 faceUpWS, half3 normalWS)
{
    return normalize(-dot(faceUpWS, normalWS) * faceUpWS + normalWS);
}

#endif