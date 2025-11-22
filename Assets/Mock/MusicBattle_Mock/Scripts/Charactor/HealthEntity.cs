using System;
using UnityEngine;

namespace Mock.MusicBattle.Character
{
    public class HealthEntity
    {
        public HealthEntity(float maxhealth)
        {
            _maxHealth = maxhealth;
            _currentHealth = maxhealth;
        }
        public event Action<float,float> OnHealthChanged;
        public event Action OnDeath;

        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            if (_currentHealth < 0)
            {
                _currentHealth = 0;
                OnDeath?.Invoke();
            }
            OnHealthChanged?.Invoke(_currentHealth,_maxHealth);
        }

        public void Heal(float heal)
        {
            _currentHealth += heal;
            if (_currentHealth > _maxHealth)
            {
                _currentHealth = _maxHealth;
            }
            OnHealthChanged?.Invoke(_currentHealth,_maxHealth);
        }

        private readonly float _maxHealth;
        private float _currentHealth;
    }
}