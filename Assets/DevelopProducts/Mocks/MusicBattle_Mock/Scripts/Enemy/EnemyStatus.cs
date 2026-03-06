using System;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     エネミーのステータスを定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(EnemyStatus),menuName = "Mock/MusicBattle/Enemy/" + nameof(EnemyStatus), order = 0)]
    public class EnemyStatus : ScriptableObject
    {
        #region パブリックプロパティ
        /// <summary> 移動速度。 </summary>
        public float MoveSpeed => _moveSpeed;
        /// <summary> 攻撃力。 </summary>
        public float AttackPower => _attackPower;
        /// <summary> 攻撃範囲。 </summary>
        public float AttackRange => _attackRange;
        /// <summary> 最大ヘルス。 </summary>
        public float MaxHealth => _maxHealth;
        #endregion

        #region 定数
        private const float DEFAULT_ATTACK_POWER = 20f;
        private const float DEFAULT_ATTACK_RANGE = 2f;
        private const float DEFAULT_MAX_HEALTH = 100f;
        #endregion

        #region インスペクター表示フィールド
        /// <summary> 移動速度。 </summary>
        [SerializeField, Tooltip("移動速度。")]
        private float _moveSpeed;

        /// <summary> 攻撃力。 </summary>
        [SerializeField, Tooltip("攻撃力。")]
        private float _attackPower = DEFAULT_ATTACK_POWER;
        /// <summary> 攻撃範囲。 </summary>
        [SerializeField, Tooltip("攻撃範囲。")]
        private float _attackRange = DEFAULT_ATTACK_RANGE;
        /// <summary> 最大ヘルス。 </summary>
        [SerializeField, Tooltip("最大ヘルス。")]
        private float _maxHealth = DEFAULT_MAX_HEALTH;
        #endregion
    }}

