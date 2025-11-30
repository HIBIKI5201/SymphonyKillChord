
using CriWare;
using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Develop;
using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.Player;
using System;
using System.Collections;
using UnityEngine;
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

        [Header("MusicSync")]
        [SerializeField, Tooltip("音楽同期マネージャー")]
        private MusicSyncManager _musicSyncManager;
        [Header("音楽同期システム初期化パラメータ")]
        [SerializeField] private MusicSystemInitSO _musicSystemInitSO;
        [SerializeField] private CriAtomSource _source;

        private float _xrange = 5f;
        private float _yrange = 1f;
        private float _zrange = 5f;

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
            _playerManager.Init(_inputBuffer);
            _factory = new EnemyFactory(_enemyContainer,
               _playerManager.transform, _enemyManager,
               _musicSyncManager, _lockOnManager);
        }


        private void PlayerInit()
        {
            _lockOnManager = new LockOnManager(_cameraManager.transform,
                _enemyContainer, _inputBuffer);
            _cameraManager.Init(_inputBuffer, _lockOnManager);
            _playerManager.Init(_inputBuffer);
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
               // Debug.Log($"Enemy Count: {_enemyContainer.Targets.Count}");
                if (_enemyContainer.Targets.Count < 3)
                {

                    Vector3 RandamPos = new Vector3(
                        Random.Range(-_xrange, _xrange),
                        _yrange,
                        Random.Range(-_zrange, _zrange));

                    _factory.Spawn(_enemystatus, RandamPos);
                }


                yield return new WaitForSeconds(_enemySpawnTime);

            }
        }
    }
}