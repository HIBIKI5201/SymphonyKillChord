using KillChord.Runtime.Adaptor.InGame.Animation;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     キャラクターアニメーションの連続状態を保持するViewModel。
    /// </summary>
    public sealed class CharacterAnimationViewModel : ICharacterAnimationViewModel
    {
        /// <summary> 現在の移動速度ベクトルです。 </summary>
        public Vector2 Velocity { get; private set; }

        /// <summary>
        ///     移動速度ベクトルを更新する。
        /// </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        public void SetVelocity(Vector2 velocity)
        {
            Velocity = velocity;
        }
    }
}
