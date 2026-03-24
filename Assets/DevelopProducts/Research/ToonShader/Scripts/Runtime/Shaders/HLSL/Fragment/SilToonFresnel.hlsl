#ifndef SILTOON_FRESNEL_INCLUDED
#define SILTOON_FRESNEL_INCLUDED

float FresnelEffect(float3 normal, float3 viewDir, float power)
{
    return pow(1 - abs(dot(normal, viewDir)), power);
}

float BackLight(float3 normalWS, float3 viewDirWS)
{
    float3 lightDirWS = _MainLightPosition.xyz;
    return 
    pow(saturate(-dot(viewDirWS, lightDirWS)), 5) 
    * 
    saturate(20 * FresnelEffect(normalWS, viewDirWS, 9));
}
float FrontRimFresnel(float3 normalWS, float3 viewDirWS)
{
    float3 lightDirWS = _MainLightPosition.xyz;
    return
    saturate(pow(FresnelEffect(normalWS, viewDirWS, 10) * 10, 10))
    *
    saturate(dot(lightDirWS, normalWS));
}
float BackRimFresnel(float3 normalWS, float3 viewDirWS)
{
    float3 lightDirWS = _MainLightPosition.xyz;
    return
    FresnelEffect(normalWS, viewDirWS, 2)
    *
    pow(saturate(-dot(lightDirWS, normalWS) + 0.4), 20);
}

void GetFresnel(
float3 normalWS,
float3 viewDirWS,
out float backLight,
out float rimLightFront,
out float rimLightBack)
{   
    backLight = BackLight(normalWS, viewDirWS);
    rimLightFront = FrontRimFresnel(normalWS, viewDirWS);
    rimLightBack = BackRimFresnel(normalWS, viewDirWS);
}
#endif