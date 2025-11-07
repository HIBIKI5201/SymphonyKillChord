using System;
using UnityEngine;

namespace Mock.TPS
{
    public class EnemyManager : MonoBehaviour, ICharacter
    {
        public event Action OnDeath
        {
            add => _healthEntity.OnDeath += value;
            remove => _healthEntity.OnDeath -= value;
        }

        public Transform LockTarget => _lockTarget;

        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);

        public void Dead() => Destroy(gameObject);

        [SerializeField]
        private float _maxHealth = 100f;
        [SerializeField]
        private HealthbarManager _healthbarManager;

        [SerializeField]
        private Transform _lockTarget;

        private HealthEntity _healthEntity;

        private void Awake()
        {
            _healthEntity = new HealthEntity(_maxHealth);
            _healthEntity.OnHealthChanged += _healthbarManager.SetHealthBar;
            _healthEntity.OnDeath += Dead;
        }

        private void Update()
        {
            _healthbarManager.MovePosition(transform.position);
        }
    }
}
