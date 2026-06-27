#ifndef SIMPLIFIED_SSS_INCLUDED
#define SIMPLIFIED_SSS_INCLUDED

half3 _SSSWrappedDiffuse(half NdotL, half sssWrap, half3 lightColor, half shadowAtten)
{
    half wrappedNdotL = saturate((NdotL + sssWrap) / (1.0h + sssWrap));
    return wrappedNdotL * shadowAtten * lightColor;
}

half3 _SSSScatter(half NdotL, half sssWrap, half3 sssColor, half3 lightColor, half shadowAtten, half sssIntensity)
{
    half wrappedNdotL = saturate((NdotL + sssWrap) / (1.0h + sssWrap));
    half sssTerm = wrappedNdotL - saturate(NdotL);
    return sssTerm * shadowAtten * sssColor * lightColor * sssIntensity;
}

half3 _SSSTransmission(half3 viewDir, half3 lightDir, half3 sssColor, half3 lightColor, half thickness, half power, half sssIntensity)
{
    half VdotL = dot(viewDir, -lightDir);
    return pow(saturate(VdotL), power) * thickness * sssColor * lightColor * sssIntensity;
}

void GetSSSLights_float(
    float3 normalWS,
    float3 positionWS,
    float3 viewDirWS,
    half3 sssColor,
    half sssWrap,
    half sssIntensity,
    half thickness,
    half transmissionPower,
    out half3 color
)
{
#ifdef SHADERGRAPH_PREVIEW
    color = half3(0, 0, 0);
#else
    float4 shadowCoord = float4(0, 0, 0, 0);
#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    shadowCoord = TransformWorldToShadowCoord(positionWS);
#endif
    Light mainLight = GetMainLight(shadowCoord);

    half3 lightDir   = (half3) mainLight.direction;
    half3 lightColor = (half3) mainLight.color;
    half shadowAtten = (half)  mainLight.shadowAttenuation;
    half NdotL       = dot((half3) normalWS, lightDir);

    color  = _SSSWrappedDiffuse(NdotL, sssWrap, lightColor, shadowAtten);
    color += _SSSScatter(NdotL, sssWrap, sssColor, lightColor, shadowAtten, sssIntensity);
    color += _SSSTransmission((half3) viewDirWS, lightDir, sssColor, lightColor, thickness, transmissionPower, sssIntensity);

    int lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, positionWS, 1);
        half atten = (half) (light.shadowAttenuation * light.distanceAttenuation);
        half nDotL = saturate(dot((half3) normalWS, (half3) light.direction));
        color += min((half3) light.color * nDotL * atten, half3(1, 1, 1)) / 2;
    }
#endif
}

#endif
