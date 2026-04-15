using DevelopProducts.BehaviorGraph.Runtime.Adaptor;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame;
using DevelopProducts.BehaviorGraph.Runtime.Application;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.View.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.View.InGame.Music;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     テスト用のスポナー。
    /// </summary>
    public class EnemyTestSpawner : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.RegisterInstance(this);
        }

        public void SetTargetEntity(IDefender targetEntity)
        {
            _targetEntity = targetEntity;
        }

        public void SetTargetManager(TargetManager targetManager, TargetEntityRegistry targetEntityRegistry)
        {
            _targetManager = targetManager;
            _targetManagerController = new(targetManager);
            _targetEntityRegistryController = new(targetEntityRegistry);
        }

        [SerializeField] private EnemyMoveDebugInitializer _enemyPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _spawnInterval;
        [SerializeField] private int _maxSpawnCount;

        [SerializeField] private CharacterData _enemyData;

        private IMusicSyncViewModel _musicSyncViewModel;
        private IMusicSyncService _musicSyncService;
        private IDefender _targetEntity;
        private TargetManager _targetManager;
        private TargetManagerController _targetManagerController;
        private TargetEntityRegistryController _targetEntityRegistryController;
        private Transform _target;

        private float _timer;
        private int _spawnCount;

        public void Init()
        {
            MusicSyncInitializer initializer = FindFirstObjectByType<MusicSyncInitializer>();
            _musicSyncService = initializer.MusicSyncService;
            MusicSyncView view = FindAnyObjectByType<MusicSyncView>();
            if (view.MusicSyncViewModel == null)
            {
                Debug.LogError("MusicSyncViewが見つかりません。", this);
                return;
            }
            _musicSyncViewModel = view.MusicSyncViewModel;
        }

        private void Update()
        {
            if (_enemyPrefab == null || _spawnPoint == null) return;
            if (_spawnCount >= _maxSpawnCount) return;

            _timer += Time.deltaTime;

            if (_timer >= _spawnInterval)
            {
                _timer = 0f;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            PlayerInitializer playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();
            if (playerInitializer == null)
            {
                Debug.LogError("ターゲットのTransformが見つかりません。", this);
                return;
            }
            _target = playerInitializer.transform;

            if (_targetEntity == null || _musicSyncViewModel == null ||
                _musicSyncService == null || _targetEntityRegistryController == null)
            {
                if (_targetEntity == null) Debug.LogError("ターゲットエンティティが設定されていません。", this);
                if (_musicSyncViewModel == null) Debug.LogError("MusicSyncViewModelが見つかりません。", this);
                if (_musicSyncService == null) Debug.LogError("MusicSyncServiceが見つかりません。", this);
                if (_targetEntityRegistryController == null) Debug.LogError("TargetEntityRegistryControllerが見つかりません。", this);
                return;
            }

            EnemyMoveDebugInitializer enemyInstance =
                Instantiate(_enemyPrefab, _spawnPoint.position, _spawnPoint.rotation);

            enemyInstance.Initialize(_target, (Domain.InGame.Character.CharacterEntity)_targetEntity,
            _musicSyncViewModel, _musicSyncService, _targetManagerController, _targetEntityRegistryController);

            _spawnCount++;
        }
    }
}