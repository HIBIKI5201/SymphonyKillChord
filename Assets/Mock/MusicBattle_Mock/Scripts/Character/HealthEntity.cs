using System;
using UnityEngine;

namespace Mock.MusicBattle.Character
{
    /// <summary>
    ///     キャラクターのヘルス（体力）を管理するエンティティクラス。
    /// </summary>
    public class HealthEntity
    {
        /// <summary>
        ///     <see cref="HealthEntity"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="maxHealth">最大ヘルス量。</param>
        public HealthEntity(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        #region Publicイベント
        /// <summary> ヘルスが変更されたときに発火するイベント。 </summary>
        public event Action<float, float> OnHealthChanged;
        /// <summary> ヘルスが0になったときに発火するイベント。 </summary>
        public event Action OnDeath;
        #endregion

        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        #region Publicメソッド
        /// <summary>
        ///     自分にダメージを与え、ヘルスを減少させます。
        ///     ヘルスが0以下になった場合、OnDeathイベントを発火します。
        /// </summary>
        /// <param name="damage">与えるダメージ量。</param>
        public void TakeDamage(float damage)
        {
            if (_isDead) return;
            _currentHealth -= damage;
            Debug.Log($"{this.GetType().Name} は {damage} のダメージを受けました。現在のヘルス: {_currentHealth}");
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                OnDeath?.Invoke();
                _isDead = true;
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>
        ///     ヘルスと死亡状態をリセットします。
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>
        ///     ヘルスを回復させます。
        ///     ヘルスが最大値を超えないように制限します。
        /// </summary>
        /// <param name="heal">回復量。</param>
        public void Heal(float heal)
        {
            _currentHealth += heal;
            if (_currentHealth > _maxHealth)
            {
                _currentHealth = _maxHealth;
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        #endregion

        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> 死亡状態を示すフラグ。 </summary>
        private bool _isDead = false;
        /// <summary> 最大ヘルス量。 </summary>
        private readonly float _maxHealth;
        /// <summary> 現在のヘルス量。 </summary>
        private float _currentHealth;
        #endregion

        // UNITY_LIFECYCLE_METHODS
        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}