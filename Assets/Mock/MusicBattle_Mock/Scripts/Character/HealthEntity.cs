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
            if (_isDead) return;
            _currentHealth -= damage;
            Debug.Log($"{this} は　{damage} を受けた");
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                OnDeath?.Invoke();
                _isDead = true;
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        ///<summary> ヘルスとisDeadをリセットする。 </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
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

        private bool _isDead = false;
        private readonly float _maxHealth;
        private float _currentHealth;
    }
}