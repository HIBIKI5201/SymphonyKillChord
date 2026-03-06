using UnityEngine;
using System;

namespace DevelopProducts.Architecture.Domain
{
    public class CharacterEntity
    {
        public CharacterEntity(string name, float health)
        {
            if (health <= 0) { throw new Exception($"{health}. Health must be greater than zero."); }

            _name = name;
            _maxHealth = health;
            _currentHealth = health;
        }

        public void TakeDamage(float damage)
        {
            _currentHealth = Mathf.Max(_currentHealth - damage, 0);
        }

        private readonly string _name;
        private readonly float _maxHealth;
        private float _currentHealth;
    }
}
