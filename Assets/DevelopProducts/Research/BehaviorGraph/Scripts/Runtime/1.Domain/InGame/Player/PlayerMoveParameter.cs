using UnityEngine;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;
using System;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Player
{
    /// <summary>
    ///     プレイヤーの移動に関するパラメータを保持するドメインクラス。
    /// </summary>
    [Serializable]
    public sealed class PlayerMoveParameter
    {
        public PlayerMoveParameter(float moveSpeed, float dodgeSpeed, float dodgeDuration, float dodgeCooldown)
        {
            _moveSpeed = moveSpeed;
            _dodgeSpeed = dodgeSpeed;
            _dodgeDuration = dodgeDuration;
            _dodgeCooldown = dodgeCooldown;
        }

        public MoveSpeed MoveSpeed => new(_moveSpeed);

        public MoveSpeed DodgeSpeed => new(_dodgeSpeed);
        public float DodgeDuration => _dodgeDuration;
        public float DodgeCooldown => _dodgeCooldown;

        [SerializeField] private float _moveSpeed;
        [Space]
        [SerializeField] private float _dodgeSpeed;
        [SerializeField] private float _dodgeDuration;
        [SerializeField] private float _dodgeCooldown;
    }
}
