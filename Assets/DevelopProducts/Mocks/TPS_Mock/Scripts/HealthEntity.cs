using System;

namespace Mock.TPS
{
    public class HealthEntity
    {
        public HealthEntity(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            if (_currentHealth < 0)
            {
                _currentHealth = 0;
                OnDeath?.Invoke();
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void Heal(float amount)
        {
            _currentHealth += amount;
            if (_currentHealth > _maxHealth)
            {
                _currentHealth = _maxHealth;
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        private readonly float _maxHealth;
        private float _currentHealth;

    }
}
