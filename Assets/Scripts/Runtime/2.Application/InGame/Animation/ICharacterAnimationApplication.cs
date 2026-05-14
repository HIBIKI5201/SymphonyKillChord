using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     アニメーション処理のアプリケーションサービスインターフェース。
    /// </summary>
    public interface ICharacterAnimationApplication
    {
        /// <summary> 現在のアニメーション状態。 </summary>
        CharacterAnimationState CurrentState { get; }
        
        /// <summary> BPMから算出したアニメーションの再生速度。 </summary>
        float AnimationSpeed { get; }

        /// <summary> ブレンドウェイト。 </summary>
        float BlendWeight { get; }
        
        /// <summary> キャラクターの速度を設定します。 </summary>
        /// <param name="velocity">速度ベクトル。</param>
        void SetVelocity(Vector2 velocity);

        /// <summary> BPMを設定する。 </summary>
        /// <param name="bpm"> BPM値。 </param>
        void SetBpm(float bpm);
    }
}
