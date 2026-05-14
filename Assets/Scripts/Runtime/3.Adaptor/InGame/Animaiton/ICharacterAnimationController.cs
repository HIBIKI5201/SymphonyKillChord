using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     キャラクターアニメーション操作のAdaptorインターフェース。
    /// </summary>
    public interface ICharacterAnimationController
    {
        /// <summary> アニメーション再生速度（bpm / 60f）。 </summary>
        float AnimationSpeed { get; }

        /// <summary> アイドルから歩きへのブレンドウェイト（0〜1）。 </summary>
        float BlendWeight { get; }

        /// <summary> プレイヤーの速度ベクトルを設定する。 </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        void SetVelocity(Vector2 velocity);
    }
}
