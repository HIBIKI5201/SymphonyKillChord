#ifndef CUSTOM_MANUAL_PBR_INCLUDED
#define CUSTOM_MANUAL_PBR_INCLUDED


void ManualPBR_float(
    float3 albedo,
    float3 normalWS,
    float3 viewDirWS,
    float metallic,
    float smoothness,
    out float3 result
)
{
    result = albedo;
    half perceptualRoughness = 1.0h - (half) smoothness;
    half roughness = max(perceptualRoughness * perceptualRoughness, 0.0001h);
    half roughness2 = roughness * roughness;
    half normalizationTerm = roughness * 4.0h + 2.0h;
    half roughness2MinusOne = roughness2 - 1.0h;

    half3 specularColor = lerp(half3(0.04h, 0.04h, 0.04h), (half3) albedo, (half) metallic);
    half3 diffuseColor = (half3) albedo * (1.0h - (half) metallic) * (1.0h - 0.04h);

    Light mainLight = GetMainLight();

    half NdotL = saturate(dot(normalWS, mainLight.direction));
    half3 halfDir = SafeNormalize(mainLight.direction + viewDirWS);
    half NdotH = saturate(dot(normalWS, halfDir));
    half LdotH = saturate(dot(mainLight.direction, halfDir));

    half d = NdotH * NdotH * roughness2MinusOne + 1.00001h;
    half specTerm = roughness2 / ((d * d) * max(0.1h, LdotH * LdotH) * normalizationTerm);

    half3 spec = specularColor * specTerm * mainLight.color * NdotL;
    half3 diff = diffuseColor * mainLight.color * NdotL;
    half3 ambient = SampleSH(normalWS) * diffuseColor;

    result = diff + spec + ambient;
}
#endif