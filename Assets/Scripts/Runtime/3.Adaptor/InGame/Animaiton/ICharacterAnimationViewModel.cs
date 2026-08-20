using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Animation
{
    /// <summary>
    ///     キャラクターアニメーションの連続状態を扱うViewModelインターフェース。
    /// </summary>
    public interface ICharacterAnimationViewModel
    {
        /// <summary> 現在の移動速度ベクトルです。 </summary>
        public Vector2 Velocity { get; }

        /// <summary>
        ///     移動速度ベクトルを更新する。
        /// </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        public void SetVelocity(Vector2 velocity);
    }
}
