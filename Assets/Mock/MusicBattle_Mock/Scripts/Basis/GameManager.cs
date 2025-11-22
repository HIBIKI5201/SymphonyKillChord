using System;
using Mock.MusicBattle.Enemy;
using UnityEngine;

namespace Mock.MusicBattle.Basis
{
    /// <summary>ゲーム全体の管理クラス</summary>
    [DefaultExecutionOrder(-900)]
    public class GameManager : MonoBehaviour
    {
         private EnemyContainer _enemyContainer;
        [SerializeField] private Transform _target;
        [SerializeField] private EnemyManager _enemyManager;
        [SerializeField] private EnemyStatus _enemystatus;
        private EnemyFactory _factory;
        private void Awake()
        {
            _enemyContainer = new EnemyContainer();
            _factory = gameObject.AddComponent<EnemyFactory>();
            _factory.Init(_enemyContainer,transform,_enemyManager);
        }

        private void Start()
        {
            _factory.Spawn(_enemystatus,transform.position);
        }
    }
    
}
