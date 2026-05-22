using KillChord.Runtime.View.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     テスト用の歩兵スポナー。
    /// </summary>
    public class EnemyInfantrySpawner : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize()
        {
            _spawnPositions = new Vector3[_spawnBatchCount];
            _spawnCount = 0;
        }
        /// <summary>
        ///     歩兵インスタンスが回収された時のcallback処理。
        /// </summary>
        public void HandleInfantryDeactivated()
        {
            _spawnCount--;
        }

        [SerializeField] private EnemyPools _enemyPools;
        [SerializeField, Tooltip("生成位置")] private Transform _spawnPoint;
        [SerializeField, Tooltip("生成距離")] private float _spawnDistance;
        [SerializeField, Tooltip("生成間隔")] private float _spawnInterval;
        [SerializeField, Tooltip("一度の生成数")] private int _spawnBatchCount = 4;
        [SerializeField, Tooltip("敵の最大数。-1は無限")] private int _maxSpawnCount;
        [SerializeField, Tooltip("敵の生成位置を探索するコンポーネント")]
        private EnemySpawnPositionSearcher _spawnPositionSearcher;

        private float _timer;
        private int _spawnCount;
        private Vector3[] _spawnPositions;

        private void Update()
        {
            if (_spawnCount >= _maxSpawnCount && _maxSpawnCount != -1) return;

            _timer += Time.deltaTime;

            if (_timer >= _spawnInterval)
            {
                _timer = 0f;
                SpawnEnemy();
            }
        }

        /// <summary>
        ///     敵生成処理。
        /// </summary>
        private void SpawnEnemy()
        {
            _spawnPositionSearcher.FindSpawnPositions(_spawnDistance, _spawnPositions);
            for (int i = 0; i < _spawnPositions.Length; i++)
            {
                if (_spawnCount >= _maxSpawnCount && _maxSpawnCount != -1) break;
                EnemyLifeCycle lifeCycle = _enemyPools.GetInfantry().GetComponent<EnemyLifeCycle>();
                lifeCycle.Activate(_spawnPositions[i], HandleInfantryDeactivated);
                _spawnCount++;
            }
        }
    }
}
