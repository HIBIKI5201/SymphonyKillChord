using System.Collections.Generic;
using Codice.Client.BaseCommands;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>エネミー(EnemyManager)生成を一元管理するファクトリ。</summary>
    public class EnemyFactory : MonoBehaviour
    {
        public void Init(EnemyContainer enemyContainer, Transform target,EnemyManager enemyManager)
        {
            _enemyContainer = enemyContainer;
            _target = target;
            _enemyPrefab = enemyManager;
        }


        public EnemyManager Spawn(EnemyStatus status, Vector3 position)
        {
            EnemyManager enemy;

            if (_pool.Count > 0)
            {
                enemy = _pool.Dequeue();
                enemy.gameObject.SetActive(true);
            }
            else
            {
                enemy = Instantiate(_enemyPrefab);
                enemy.OnDeath += () =>
                {
                    enemy.gameObject.SetActive(false);
                    _pool.Enqueue(enemy);
                };
            }

            enemy.SetStatus(status);
            enemy.SetTarget(_target);
            enemy.transform.position = position;
            _enemyContainer.Register(enemy);
            return enemy;
        }

        private readonly Queue<EnemyManager> _pool = new();
        private EnemyManager _enemyPrefab;
        private EnemyContainer _enemyContainer;
        private Transform _target;
    }
}