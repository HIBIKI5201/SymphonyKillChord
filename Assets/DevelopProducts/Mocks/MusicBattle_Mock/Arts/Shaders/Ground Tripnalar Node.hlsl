void WorldTriplanar_float(
    float3 worldPos,
    float3 worldNormal,
    float tileSize,
    UnityTexture2D tex,
    SamplerState sampler_tex,
    out float4 color
)
{
    float3 n = normalize(worldNormal);

    float2 uvX = worldPos.yz / tileSize;
    float2 uvY = worldPos.xz / tileSize;
    float2 uvZ = worldPos.xy / tileSize;
    
    float4 cx = tex.Sample(sampler_tex, uvX);
    float4 cy = tex.Sample(sampler_tex, uvY);
    float4 cz = tex.Sample(sampler_tex, uvZ);

    float3 blend = abs(n);
    blend = blend / (blend.x + blend.y + blend.z + 1e-5);

    color = cx * blend.x + cy * blend.y + cz * blend.z;
}