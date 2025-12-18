using CriWare;
using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.Player;
using Mock.MusicBattle.UI;
using Unity.Cinemachine;
using UnityEngine;

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
        [SerializeField] private CriMusicBuffer _criMusicBuffer;

        [SerializeField] private EnemySpawnSO _enemySpawnSO;

        [SerializeField] private IngameHUDManager _hudManager;
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
            StartCoroutine(EnemyUtility.SpawnLoop(
                _enemyContainer,
                _enemySpawnSO,
                _factory,
                _enemystatus,
                _enemySpawnTime));
        }

        private void Init()
        {
            _enemyContainer = new EnemyContainer();
            _lockOnManager = new LockOnManager(_cameraManager.transform,
              _enemyContainer, _inputBuffer);

            PlayerInitUtility.InitPlayer(_playerManager, _inputBuffer,
                _cameraManager, _camera, _lockOnManager, _musicSyncManager);

            HudUtility.Init(_hudManager, _playerManager,
                _criMusicBuffer, _inputBuffer,
                _lockOnManager,
                this.destroyCancellationToken);

            _factory = new EnemyFactory(
                _enemyContainer, _player,
                _enemyManager, _musicSyncManager,
                _lockOnManager, _hudManager);
            EnemyUtility.EnemyContainerInit(_enemyContainer, _playerManager, _cameraManager, _lockOnManager);
        }
    }
}