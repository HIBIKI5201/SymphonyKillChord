using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     テスト用のスポナー。
    /// </summary>
    public class EnemyTestSpawner : MonoBehaviour
    {
        public void Initialize(IMusicSyncViewModel musicSyncViewModel, IMusicSyncService musicSyncService)
        {
            _musicSyncViewModel = musicSyncViewModel;
            _musicSyncService = musicSyncService;
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

        private float _timer;
        private int _spawnCount;

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

            enemyInstance.Initialize(_target,_targetEntity, _musicSyncViewModel, _musicSyncService);

            _spawnCount++;
        }
    }
}
