using UnityEngine;
using System;

namespace DevelopProducts.Architecture.Domain
{
    public class CharacterEntity
    {
        public CharacterEntity(string name, float health, float speed)
        {
            if (health <= 0) { throw new Exception($"{health}. Health must be greater than zero."); }

            _name = name;
            _maxHealth = health;
            _currentHealth = health;
            _speed = speed;
        }

        public void TakeDamage(float damage)
        {
            _currentHealth = Mathf.Max(_currentHealth - damage, 0);
        }

        public float Speed => _speed;

        private readonly string _name;
        private readonly float _maxHealth;
        private float _currentHealth;
        private readonly float _speed;
    }
}
