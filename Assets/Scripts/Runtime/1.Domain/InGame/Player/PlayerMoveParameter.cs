using System;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Player
{
    /// <summary>
    ///     プレイヤー移動に関するパラメータを保持するドメインクラス。
    /// </summary>
    public sealed class PlayerMoveParameter
    {
        /// <summary> プレイヤー移動パラメータを初期化する。 </summary>
        public PlayerMoveParameter(MoveSpeed moveSpeed, DodgeSpeed dodgeSpeed, DodgeDuration dodgeDuration, DodgeCooldown dodgeCooldown)
        {
            MoveSpeed = moveSpeed;
            DodgeSpeed = dodgeSpeed;
            DodgeDuration = dodgeDuration;
            DodgeCooldown = dodgeCooldown;
        }

        /// <summary> 通常移動速度。 </summary>
        public MoveSpeed MoveSpeed;

        /// <summary> 回避移動速度。 </summary>
        public DodgeSpeed DodgeSpeed;

        /// <summary> 回避時間。 </summary>
        public DodgeDuration DodgeDuration;

        /// <summary> 回避クールダウン時間。 </summary>
        public DodgeCooldown DodgeCooldown;
    }
}
