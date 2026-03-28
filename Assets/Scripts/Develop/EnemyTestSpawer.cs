using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Develop
{
    /// <summary>
    ///     テスト用のスポナー。
    /// </summary>
    public class EnemyTestSpawer : MonoBehaviour
    {
        [SerializeField] private EnemyMoveDebugInitializer _enemyPrefab;
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _spawnInterval;
        [SerializeField] private int _maxSpawnCount;

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

            enemyInstance.Initialize(_target);

            _spawnCount++;
        }
    }
}
