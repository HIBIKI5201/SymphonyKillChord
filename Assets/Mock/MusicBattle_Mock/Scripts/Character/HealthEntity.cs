using System;
using UnityEngine;

namespace Mock.MusicBattle.Character
{
    public class HealthEntity
    {
        /// <summary>
        ///     ヘルス量を初期化する。
        /// </summary>
        /// <param name="maxhealth"> マックスヘルス量。 </param>
        public HealthEntity(float maxhealth)
        {
            _maxHealth = maxhealth;
            _currentHealth = maxhealth;
        }

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        /// <summary>
        ///     自分にダメージを与える。
        ///     イベントを通知する。
        /// </summary>
        /// <param name="damage"> 与えるダメージ量。 </param>
        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            if (_currentHealth < 0)
            {
                _currentHealth = 0;
                if (_isAlive)
                {
                    OnDeath?.Invoke();
                    _isAlive = false;
                }
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>
        ///     ヘルスを回復させる。
        ///     イベントを通知する。
        /// </summary>
        /// <param name="heal"> 回復量。 </param>
        public void Heal(float heal)
        {
            _currentHealth += heal;
            if (_currentHealth > _maxHealth)
            {
                _currentHealth = _maxHealth;
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        private bool _isAlive = true;
        private readonly float _maxHealth;
        private float _currentHealth;
    }
}