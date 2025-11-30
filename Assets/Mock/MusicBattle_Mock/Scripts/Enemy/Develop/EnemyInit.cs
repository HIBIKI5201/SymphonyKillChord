using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Mock.MusicBattle.MusicSync;

namespace Mock.MusicBattle.Enemy
{
    /// <summary> エネミー全体の初期化テストクラス。 </summary>
    [DefaultExecutionOrder(-800)]
    public class EnemyInit : MonoBehaviour
    {
        [SerializeField] private EnemyManager _enemyManager;
        [SerializeField] private EnemyStatus _enemystatus;
        [SerializeField] private Transform _player;
        [SerializeField]private float _enemySpawnTime = 1f;
        private EnemyContainer _enemyContainer;
        private float _xrange = 50f;
        private float _yrange = 1f;
        private float _zrange = 50f;
        private EnemyFactory _factory;
        private MusicSyncManager musicSyncManager;

        private void Awake()
        {
            _enemyContainer = new EnemyContainer();
       
        }

        private void Start()
        {
          StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                Vector3 RandamPos = new Vector3(
                    Random.Range(-_xrange, _xrange),
                    _yrange,
                    Random.Range(-_zrange, _zrange));
                
                _factory.Spawn(_enemystatus,RandamPos);

                yield return new WaitForSeconds(_enemySpawnTime);
            }
        }
    }
}