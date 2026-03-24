using DevelopProducts.Architecture.Adaptor;
using System;
using UnityEngine;

namespace DevelopProducts.Architecture.View
{
    public class CharacterViewModel : ICharacterViewModel
    {
        public event Action<Vector2> OnMove;
        public event Action<float, float> OnUpdateHealth;

        public Vector2 Dir => _dir;
        public float Health => _health;
        public float MaxHealth => _maxHealth;

        void ICharacterViewModel.Move(Vector2 dir)
        {
            _dir = dir;
            OnMove?.Invoke(dir);
        }

        void ICharacterViewModel.UpdateHealth(float currentHealth, float maxHealth)
        {
            _health = currentHealth;
            _maxHealth = maxHealth;
            OnUpdateHealth?.Invoke(currentHealth, maxHealth);
        }

        private Vector2 _dir;
        private float _health;
        private float _maxHealth;
    }
}
