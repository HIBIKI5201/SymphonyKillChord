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
        /// <summary> 移動速度。 </summary>
        public float MoveSpeed => _moveSpeed;
        /// <summary> 攻撃力。 </summary>
        public float AttackPower => _attackPower;
        /// <summary> 攻撃範囲。 </summary>
        public float AttackRange => _attackRange;
        /// <summary> 最大ヘルス。 </summary>
        public float MaxHealth => _maxHealth;
        
        /// <summary> 移動速度。 </summary>
        [SerializeField, Tooltip("移動速度。")]
        private float _moveSpeed;

        /// <summary> 攻撃力。 </summary> 
        [SerializeField, Tooltip("攻撃力。")]
        private float _attackPower = 20f;
        /// <summary> 攻撃範囲。 </summary>
        [SerializeField, Tooltip("攻撃範囲。")]
        private float _attackRange = 2f;
        /// <summary> 最大ヘルス。 </summary>
        [SerializeField, Tooltip("最大ヘルス。")]
        private float _maxHealth = 100f;
    }
}

