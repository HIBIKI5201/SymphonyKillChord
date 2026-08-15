#ifndef FAKE_SHADOW_INCLUDED
#define FAKE_SHADOW_INCLUDED

// 髪ポリゴンをライト方向へオフセットして描き、顔領域(ステンシル bit1)にのみ落とす擬似影。
// 深度ではなくステンシルで「顔かどうか」を判定するため、髪と顔の距離に依らず安定した形が出る。

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
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
    float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);

    // _MainLightPositionは「面からライトへ向かう」方向。影は光の進行方向へ伸ばすため反転する。
    float3 lightDirWS = normalize(_MainLightPosition.xyz);
    positionWS -= lightDirWS * _FakeShadowDistance;

    // ZTest LEqual で顔より奥の髪を弾いているが、オフセットで顔の裏に潜った分まで
    // 削れてしまうことがあるため、カメラ側へ少し引き戻して調整できるようにする
    positionWS += normalize(GetCameraPositionWS() - positionWS) * _FakeShadowDepthBias;

    o.pos = TransformWorldToHClip(positionWS);
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
