using UnityEngine;

namespace Mock.TPS
{
    public class EnemyManager : MonoBehaviour, ICharacter
    {
        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);

        [SerializeField]
        private float _maxHealth = 100f;
        [SerializeField]
        private HealthbarManager _healthbarManager;

        private HealthEntity _healthEntity;

        private void Awake()
        {
            _healthEntity = new HealthEntity(_maxHealth);
            _healthEntity.OnHealthChanged += _healthbarManager.SetHealthBar;
        }

        private void Update()
        {
            _healthbarManager.MovePosition(transform.position);
        }
    }
}
