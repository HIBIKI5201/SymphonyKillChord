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
            _enemyController = new EnemyController(target, enemy, _enemyStatus, rb);
            _lockOnController = new LockOnController();
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
        private LockOnController _lockOnController;
    }
}