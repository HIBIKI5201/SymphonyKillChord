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
        /// <summary>  現在、アニメーションの予約状態かどうかを示す値です。 </summary>
        public bool IsReserving { get; private set; }

        /// <summary>
        ///     移動速度ベクトルを更新する。
        /// </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        public void SetVelocity(Vector2 velocity)
        {
            Velocity = velocity;
        }
        /// <summary>
        ///    アニメーションの予約状態を更新する。
        /// </summary>
        /// <param name="reserving"></param>
        public void SetReserving(bool reserving)
        {
            IsReserving = reserving;
        }
    }
}
