using UnityEngine;
using System;

namespace DevelopProducts.Architecture.Domain
{
    /// <summary>
    ///     キャラクターのドメインモデルを表すエンティティ。
    /// </summary>
    public class CharacterEntity
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="name"> 名前。 </param>
        /// <param name="health"> 初期体力。 </param>
        /// <param name="speed"> 移動速度。 </param>
        /// <param name="attackPower"> 攻撃力。 </param>
        public CharacterEntity(string name, float health, float speed, float attackPower)
        {
            if (float.IsNaN(health) || health <= 0) { throw new Exception($"{health}. Health must be greater than zero."); }

            _name = name;
            _maxHealth = health;
            _currentHealth = health;
            _speed = speed;
            _attackPower = attackPower;
        }

        /// <summary>
        ///     体力の変更を通知するイベント。
        /// </summary>
        public event Action<float> OnHealthChanged;

        /// <summary> 現在の体力。 </summary>
        public float CurrentHealth => _currentHealth;

        /// <summary> 最大体力。 </summary>
        public float MaxHealth => _maxHealth;

        /// <summary> 移動速度。 </summary>
        public float Speed => _speed;

        /// <summary> 攻撃力。 </summary>
        public float AttackPower => _attackPower;

        /// <summary>
        ///     ダメージを受ける。
        /// </summary>
        /// <param name="damage"> ダメージ量。 </param>
        public void TakeDamage(DamageContext damage)
        {
            _currentHealth = Mathf.Max(_currentHealth - damage.Value, 0);
            OnHealthChanged?.Invoke(_currentHealth);
        }

        private readonly string _name;
        private readonly float _maxHealth;
        private float _currentHealth;
        private readonly float _speed;
        private readonly float _attackPower;
    }
}
