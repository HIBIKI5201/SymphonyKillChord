
using System;
using System.Collections;
using UnityEngine;using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Develop;
using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.Player;
using Random = UnityEngine.Random;

namespace Mock.MusicBattle.Basis
{
    /// <summary>ゲーム全体の管理クラス</summary>
    [DefaultExecutionOrder(-900)]
    public class GameManager : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] 
        private PlayerManager _playerManager;
        [SerializeField] 
        private InputBuffer _inputBuffer;
        [SerializeField]
        private CameraManager _cameraManager;
        
        [Header("Enemy")]
        [SerializeField] 
        private EnemyManager _enemyManager;
        [SerializeField]
        private EnemyStatus _enemystatus;
        [SerializeField] 
        private Transform _player;
        [SerializeField]
        private float _enemySpawnTime = 1f;
        
        private float _xrange = 50f;
        private float _yrange = 1f;
        private float _zrange = 50f;
        
        private EnemyFactory _factory;
        private LockOnManager _lockOnManager;
        private EnemyContainer _enemyContainer;

        private void Awake()
        {
            EnemyInit();
            PlayerInit();
        }

        private void Start()
        {
            StartCoroutine(SpawnLoop());
        }

        private void PlayerInit()
        {
            _lockOnManager = new LockOnManager(_cameraManager.transform,
                _enemyContainer, _inputBuffer);
            _cameraManager.Init(_inputBuffer,_lockOnManager);
            _playerManager.Init(_inputBuffer);
        }

        private void EnemyInit()
        {
            _enemyContainer = new EnemyContainer();
            _factory = new EnemyFactory(_enemyContainer,
                _playerManager.transform, _enemyManager);
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