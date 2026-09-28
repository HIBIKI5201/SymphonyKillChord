#ifndef FAKE_SHADOW_INCLUDED
#define FAKE_SHADOW_INCLUDED

// 髪ポリゴンをカメラから見た上下左右へ一定量オフセットして描き、顔領域(ステンシル bit1)にのみ落とす擬似影。
// 深度ではなくステンシルで「顔かどうか」を判定するため、髪と顔の距離に依らず安定した形が出る。

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/Dither/Dither.hlsl"
#include "Assets/Scripts/Shaders/SilToon/HLSL/SilToonInput.hlsl"

struct appdata
{
    float4 positionOS : POSITION;
};

struct v2f
{
    float4 pos : SV_POSITION;
};

v2f vert(appdata v)
{
    v2f o;

#ifdef _FAKE_SHADOW_ON
    float3 positionVS = TransformWorldToView(TransformObjectToWorld(v.positionOS.xyz));
    
    positionVS.xy += _FakeShadowOffset;
    
    o.pos = TransformWViewToHClip(positionVS);
    
    float3 towardCameraVS =  -normalize(positionVS);
    float depthBias = _FakeShadowDepthBias;
        float distToNear = length(positionVS) * saturate(1.0 - _ProjectionParams.y * rcp(max(-positionVS.z, 1e-5)));
        depthBias = min(depthBias, distToNear * 0.5);
    float4 biasedCS = TransformWViewToHClip(positionVS + towardCameraVS * depthBias);
    o.pos.z = biasedCS.z * rcp(biasedCS.w) * o.pos.w;
    
#if UNITY_REVERSED_Z
    o.pos.z = min(o.pos.z, o.pos.w);
#else
    o.pos.z = max(o.pos.z, -o.pos.w);
#endif
#else
    // パス自体はSilToonの全マテリアルに存在するため、無効時は縮退させて破棄する
    o.pos = (float4)0;
#endif
    return o;
}

half4 frag(v2f i) : SV_Target
{
#ifdef FADE_ON
    clip(_FadeAlpha - BayerDither(i.pos.xy) - 0.0001);
#endif

    // Blend DstColor Zero (乗算) で描くため、出力色がそのまま減衰率になる
    return _FakeShadowColor;
}

#endif
