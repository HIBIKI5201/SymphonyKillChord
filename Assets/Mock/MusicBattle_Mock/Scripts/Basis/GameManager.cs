using System;
using Mock.MusicBattle.Enemy;
using UnityEngine;

namespace Mock.MusicBattle.Basis
{
    /// <summary>ゲーム全体の管理クラス</summary>
    [DefaultExecutionOrder(-900)]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private EnemyManager _enemyManager;
        [SerializeField] private EnemyStatus _enemystatus;
        [SerializeField] private Transform _player;
        private EnemyContainer _enemyContainer;
        private EnemyFactory _factory;

        private void Awake()
        {
            _enemyContainer = new EnemyContainer();
            _factory = gameObject.AddComponent<EnemyFactory>();
            _factory.Init(_enemyContainer, _player, _enemyManager);
        }

        private void Start()
        {
            _factory.Spawn(_enemystatus, transform.position);
        }
    }
}