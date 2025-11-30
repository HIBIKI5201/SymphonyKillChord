using Mock.MusicBattle.Basis;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    [CreateAssetMenu(fileName = nameof(PlayerStatus), menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(PlayerStatus))]
    public class PlayerStatus : ScriptableObject
    {
        public float MoveSpeed => _moveSpeed;
        public float WalkAccelerationDuration => _walkAccelerationDuration;
        public float StopAccelerationDuration => _stopAccelerationDuration;
        public float RotationDamping => _rotationDamping;
        public float AttackPower => _attackPower;
        public float AttackRange => _attackRange;
        public float MaxHealth => _maxHealth;
        public float GroundNormalVerticalThreshold => _groundNormalVerticalThreshold;
        [Header("Movement Status")]
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _walkAccelerationDuration;
        [SerializeField] private float _stopAccelerationDuration;
        [SerializeField] private float _rotationDamping;
        [Header("Battle Status")]
        [SerializeField] private float _attackPower;
        [SerializeField] private float _attackRange;
        [SerializeField] private float _maxHealth;
        [Header("Ground Check")]
        [SerializeField, Range(0, 1)] private float _groundNormalVerticalThreshold = 0.5f;
    }
}
