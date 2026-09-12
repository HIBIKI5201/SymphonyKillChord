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
        /// <summary> 現在、アニメーションの予約状態かどうかを示す値です。 </summary>
        bool IsReserving { get; }
        /// <summary>
        ///     移動速度ベクトルを更新する。
        /// </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        public void SetVelocity(Vector2 velocity);
        /// <summary>
        ///    アニメーションの予約状態を更新する。
        /// </summary>
        /// <param name="reserving"></param>
        void SetReserving(bool reserving);
    }
}
