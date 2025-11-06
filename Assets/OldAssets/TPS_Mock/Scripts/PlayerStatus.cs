using System;
using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーのステータスクラス。
    /// </summary>
    [Serializable]
    public class PlayerStatus
    {
        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public float RotationSpeed => _rotationSpeed;

        [SerializeField]
        private float _moveSpeed = 5f;
        [SerializeField]
        private float _jumpForce = 5f;

        [SerializeField]
        private float _rotationSpeed = 3f;
    }
}