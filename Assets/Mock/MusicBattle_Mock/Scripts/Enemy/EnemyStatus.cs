using System;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>　エネミーのステータスクラス。　</summary>
    [CreateAssetMenu(fileName = nameof(EnemyStatus),menuName = "Mock/MusicBattle/Enemy/" + nameof(EnemyStatus), order = 0)]
    public class EnemyStatus : ScriptableObject
    {
        public float MoveSpeed => _moveSpeed;
        public float AttackPower => _attackPower;
        public float AttackRange => _attackRange;
        public float MaxHealth => _maxHealth;
        
        [Header("Movement Status")]
        [SerializeField]
        private float _moveSpeed;

        [Header("Battle Status")] 
        [SerializeField]
        private float _attackPower = 20f;
        [SerializeField]
        private float _attackRange = 2f;
        [SerializeField]
        private float _maxHealth = 100f;


    }
}
