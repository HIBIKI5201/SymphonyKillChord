using System;
using System.Collections.Generic;
using UnityEngine;
using Mock.MusicBattle.Camera;
using Unity.Collections;
using UnityEngine.InputSystem.iOS;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    /// 敵の主要な処理を一括して管理するクラス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyManager : MonoBehaviour
    {
        public void Init(Transform target, Transform enemy)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            _healthEntity = new HealthEntity(_enemyStatus.MaxHealth);
            _enemyMover = new EnemyMover(target, enemy, _enemyStatus, rb);
        }

        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);

        private void FixedUpdate()
        {
            if (_enemyMover == null) return;
            _enemyMover.MoveTo();
        }


        [SerializeField] private EnemyStatus _enemyStatus;
        private HealthEntity _healthEntity;
        private EnemyMover _enemyMover;
    }
}