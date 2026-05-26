using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     アニメーション処理のアプリケーションサービスインターフェース。
    /// </summary>
    public interface ICharacterAnimationApplication
    {
        /// <summary> BPMから算出したアニメーション再生速度（bpm / 60f）。 </summary>
        float AnimationSpeed { get; }

        /// <summary> アニメーションのブレンド結果。 </summary>
        CharacterAnimationBlendData BlendData { get; }

        /// <summary> プレイヤーの速度ベクトルを設定する。 </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        void SetVelocity(Vector2 velocity);

        /// <summary> BPMを設定する。 </summary>
        /// <param name="bpm"> BPM値。 </param>
        void SetBpm(float bpm);
    }
}
