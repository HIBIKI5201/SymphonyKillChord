
using CriWare;
using System;
using System.Collections;
using UnityEngine;
using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Develop;
using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.Player;
using Random = UnityEngine.Random;
using Unity.Cinemachine;

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
        private CinemachineCamera _camera;
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

        [SerializeField, Tooltip("音楽同期マネージャー")]
        private MusicSyncManager _musicSyncManager;
        [SerializeField] private MusicSystemInitSO _musicSystemInitSO;
        [SerializeField] private CriAtomSource _source;

        private float _xrange = 50f;
        private float _yrange = 1f;
        private float _zrange = 50f;


        private EnemyFactory _factory;
        private LockOnManager _lockOnManager;
        private EnemyContainer _enemyContainer;

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            _musicSyncManager.Init(_source, _musicSystemInitSO.Bgm, _musicSystemInitSO.BgmProperTime, _musicSystemInitSO.StartOffset);
            StartCoroutine(SpawnLoop());
        }
        private void Init()
        {
            _enemyContainer = new EnemyContainer();
            _lockOnManager = new LockOnManager(_cameraManager.transform,
              _enemyContainer, _inputBuffer);
            _cameraManager.Init(_inputBuffer, _lockOnManager);
            _playerManager.Init(_inputBuffer, _camera);
            _factory = new EnemyFactory(_enemyContainer,
               _playerManager.transform, _enemyManager,
               _musicSyncManager, _lockOnManager);
        }


        private void PlayerInit()
        {
            _lockOnManager = new LockOnManager(_cameraManager.transform,
                _enemyContainer, _inputBuffer);
            _cameraManager.Init(_inputBuffer, _lockOnManager);
            _playerManager.Init(_inputBuffer, _camera);

        }

        private void EnemyInit()
        {
            _enemyContainer = new EnemyContainer();
            _factory = new EnemyFactory(_enemyContainer,
                _playerManager.transform, _enemyManager,
                _musicSyncManager, _lockOnManager);
        }
        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                Vector3 RandamPos = new Vector3(
                    Random.Range(-_xrange, _xrange),
                    _yrange,
                    Random.Range(-_zrange, _zrange));

                _factory.Spawn(_enemystatus, RandamPos);

                yield return new WaitForSeconds(_enemySpawnTime);

            }
        }
    }
}