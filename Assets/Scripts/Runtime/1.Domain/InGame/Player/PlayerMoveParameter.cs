using UnityEngine;
using KillChord.Runtime.Domain.InGame.Character;
using System;

namespace KillChord.Runtime.Domain.InGame.Player
{
    /// <summary>
    ///     プレイヤーの移動に関するパラメータを保持するドメインクラス。
    /// </summary>
    public sealed class PlayerMoveParameter
    {
        public PlayerMoveParameter(MoveSpeed moveSpeed, DodgeSpeed dodgeSpeed, DodgeDuration dodgeDuration, DodgeCooldown dodgeCooldown)
        {
            MoveSpeed = moveSpeed;
            DodgeSpeed = dodgeSpeed;
            DodgeDuration = dodgeDuration;
            DodgeCooldown = dodgeCooldown;
        }

        public MoveSpeed MoveSpeed;
        public DodgeSpeed DodgeSpeed;
        public DodgeDuration DodgeDuration;
        public DodgeCooldown DodgeCooldown;
    }
}
