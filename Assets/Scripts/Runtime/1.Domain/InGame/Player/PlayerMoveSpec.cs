using System;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Player
{
    /// <summary>
    ///     プレイヤー移動に関するパラメータを保持するドメインクラス。
    /// </summary>
    public sealed class PlayerMoveSpec
    {
        /// <summary> プレイヤー移動パラメータを初期化する。 </summary>
        public PlayerMoveSpec(MoveSpeed moveSpeed, AttackRotationSpeed attackRotationSpeed, DodgeSpeed dodgeSpeed, DodgeDuration dodgeDuration, DodgeCooldown dodgeCooldown, AttackCooldown attackCooldown, AttackRecoveryBeats attackRecoveryBeats)
        {
            MoveSpeed = moveSpeed;
            AttackRotationSpeed = attackRotationSpeed;
            DodgeSpeed = dodgeSpeed;
            DodgeDuration = dodgeDuration;
            DodgeCooldown = dodgeCooldown;
            AttackCooldown = attackCooldown;
            AttackRecoveryBeats = attackRecoveryBeats;
        }

        /// <summary> 通常移動速度。 </summary>
        public MoveSpeed MoveSpeed;

        /// <summary> 攻撃した際の回転速度。 </summary>
        public AttackRotationSpeed AttackRotationSpeed;

        /// <summary> 回避移動速度。 </summary>
        public DodgeSpeed DodgeSpeed;

        /// <summary> 回避時間。 </summary>
        public DodgeDuration DodgeDuration;

        /// <summary> 回避クールダウン時間。 </summary>
        public DodgeCooldown DodgeCooldown;

        /// <summary> 回避の後に攻撃できない時間（拍数）。 </summary>
        public AttackCooldown AttackCooldown;

        /// <summary> 通常攻撃の攻撃硬直（拍数）。 </summary>
        public AttackRecoveryBeats AttackRecoveryBeats;
    }
}

