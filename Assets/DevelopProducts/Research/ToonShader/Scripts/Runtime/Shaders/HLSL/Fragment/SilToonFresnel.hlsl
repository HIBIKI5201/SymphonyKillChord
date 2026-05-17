#ifndef SILTOON_FRESNEL_INCLUDED
#define SILTOON_FRESNEL_INCLUDED

half Pow5(half x) { half x2 = x * x; return x2 * x2 * x; }
half Pow10(half x) { half x2 = x * x; half x5 = x2 * x2 * x; return x5 * x5; }
half Pow20(half x) { half x10 = Pow10(x); return x10 * x10; }

half FresnelEffect(half3 normal, half3 viewDir, half power)
{
    half fresnel = 1.0h - saturate(abs(dot(normal, viewDir)));
    return pow(fresnel, power);
}

//逆光表現
half BackLight(half3 normalWS, half3 cameraFwdWS, half3 viewDirWS, half3 mainLightDirWS)
{
    //ToonFresnel
    half fresnel = 1 - abs(dot(normalWS, viewDirWS));
    fresnel = round(fresnel * fresnel * fresnel);
    
    //MainLight & Normal Dot
    half mainLightDotNormal = 1 - 1.5 * saturate(dot(mainLightDirWS, normalWS));

    //MainLight & CameraFwd Dot
    half mainLightDotCamera = saturate(dot(mainLightDirWS, cameraFwdWS));
    mainLightDotCamera *= mainLightDotCamera;

    return saturate(mainLightDotCamera * mainLightDotNormal * fresnel);
}

half FrontRimFresnel(half3 normalWS, half3 viewDirWS)
{
    half3 lightDirWS = half3(_MainLightPosition.xyz);
    half f20 = FresnelEffect(normalWS, viewDirWS, 20.0h);
    half rim = saturate(pow(f20 * 10.0h, 10.0h));
    
    return rim * saturate(dot(lightDirWS, normalWS));
}

half BackRimFresnel(half3 normalWS, half3 viewDirWS)
{
    half3 lightDirWS = half3(_MainLightPosition.xyz);
    half f2 = FresnelEffect(normalWS, viewDirWS, 2.0h);
    
    half ldn = saturate(-dot(lightDirWS, normalWS) + 0.4h);
    return f2 * Pow20(ldn);
}

void GetFresnel(
    half3 normalWS,
    half3 viewDirWS,
    out float backLight,
    out float rimLightFront,
    out float rimLightBack)
{   
    half3 n = normalWS;
    half3 v = viewDirWS;
    
    backLight = float(BackLight(n, half3(GetViewForwardDir().xyz), v, half3(_MainLightPosition.xyz)));
    
    rimLightFront = float(FrontRimFresnel(n, v));
    rimLightBack = float(BackRimFresnel(n, v));
}
#endif