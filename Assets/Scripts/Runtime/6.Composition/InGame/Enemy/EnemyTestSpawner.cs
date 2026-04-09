using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Music;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     テスト用のスポナー。
    /// </summary>
    public class EnemyTestSpawner : MonoBehaviour
    {
        public void SetTargetManager(TargetManager targetManager)
        {
            _targetManager = targetManager;
        }
        public void SetTargetEntity(IHitTarget targetEntity)
        {
            _targetEntity = targetEntity;
        }

        [SerializeField] private EnemyMoveDebugInitializer _enemyPrefab;
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _spawnInterval;
        [SerializeField] private int _maxSpawnCount;

        private IMusicSyncViewModel _musicSyncViewModel;
        private IMusicSyncService _musicSyncService;
        private IHitTarget _targetEntity;
        private TargetManager _targetManager;
        private TargetManagerController _targetManagerController;

        private float _timer;
        private int _spawnCount;

        private void Start()
        {
            _targetManagerController = new(_targetManager);

            MusicSyncInitializer initializer = FindFirstObjectByType<MusicSyncInitializer>();
            _musicSyncService = initializer.MusicSyncService;
            MusicSyncView view = FindAnyObjectByType<MusicSyncView>();
            _musicSyncViewModel = view.MusicSyncViewModel;
        }

        private void Update()
        {
            if (_enemyPrefab == null || _target == null || _spawnPoint == null) return;
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
            EnemyMoveDebugInitializer enemyInstance =
                Instantiate(_enemyPrefab, _spawnPoint.position, _spawnPoint.rotation);

            enemyInstance.Initialize(_target, _targetEntity, _musicSyncViewModel, _musicSyncService, _targetManagerController);

            _spawnCount++;
        }
    }
}
