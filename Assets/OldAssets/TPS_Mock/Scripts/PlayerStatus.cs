using System;
using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーのステータスクラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PlayerStatus), menuName = "Mock/TPS/" + nameof(PlayerStatus), order = 0)]
    public class PlayerStatus : ScriptableObject
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