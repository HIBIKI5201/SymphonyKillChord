using UnityEngine;

namespace Mock.MusicBattle
{
    [CreateAssetMenu(fileName = nameof(PlayerStatus), menuName = "MusicBattle/" + nameof(PlayerStatus))]
    public class PlayerStatus : ScriptableObject
    {
        public float MoveSpeed => _moveSpeed;
        public float WalkAccelerationDuration => _walkAccelerationDuration;
        public float RotationDamping => _rotationDamping;
        public float AttackPower => _attackPower;
        public float AttackRange => _attackRange;
        public float MaxHealth => _maxHealth;
        [Header("Movement Status")]
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _walkAccelerationDuration;
        [SerializeField] private float _rotationDamping;
        [Header("Battle Status")]
        [SerializeField] private float _attackPower;
        [SerializeField] private float _attackRange;
        [SerializeField] private float _maxHealth;
    }
}
