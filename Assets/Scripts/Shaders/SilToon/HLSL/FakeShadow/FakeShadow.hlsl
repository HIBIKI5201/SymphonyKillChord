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

    // _FakeShadowOffset はカメラの右(+x)・上(+y)方向へのワールド単位の移動量。
    // ビュー空間で一定量ずらすため、投影で顔と同じく遠近に応じて縮む。
    // xyのみ動かすので、深度は元の髪のまま保たれる。
    positionVS.xy += _FakeShadowOffset;

    // ZTest LEqual で顔より奥の髪を弾いているが、顔の輪郭際で少し奥にある髪まで
    // 削れてしまうため、カメラ側へ引き戻して調整できるようにする。
    // 視線に沿って動かすので画面上の位置は変わらない。
    float3 towardCameraVS = IsPerspectiveProjection() ? -normalize(positionVS) : float3(0.0, 0.0, 1.0);
    positionVS += towardCameraVS * _FakeShadowDepthBias;

    o.pos = TransformWViewToHClip(positionVS);
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
