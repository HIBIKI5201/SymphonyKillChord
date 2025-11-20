using System;
using System.Collections.Generic;
using UnityEngine;
using Mock.MusicBattle.Camera;
using Unity.Collections;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    /// 敵の主要な処理を一括して管理するクラス。
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        public void Init()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            _healthEntity = new HealthEntity(_enemyStatus.MaxHealth);
            _enemyController = new EnemyController(_target, _enemy, _enemyStatus, rb);
        }

        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);

        private void FixedUpdate()
        {
            if (_enemyController == null) return;
            _enemyController.MoveTo();
        }


        [SerializeField] private EnemyStatus _enemyStatus;
        private HealthEntity _healthEntity;
        private EnemyController _enemyController;
        private Transform _enemy; //どうやってエネミーとプレイヤーを取得しようかな。
        private Transform _target;
    }
}