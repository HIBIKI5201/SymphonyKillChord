using Codice.Client.Common.GameUI;
using KillChord.Runtime.View.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     テスト用の砲兵スポナー。
    /// </summary>
    public class EnemyArtillerySpawner : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(in Transform[] assignedPositions)
        {
            _spawnPositions = new Vector3[_spawnBatchCount];
            _spawnCount = 0;
            _initialized = true;
            if (assignedPositions != null)
            {
                SpawnAssignedEnemy(assignedPositions);
            }
        }

        /// <summary>
        ///     砲兵インスタンスが回収された時のcallback処理。
        /// </summary>
        public void HandleArtilleryDeactivated()
        {
            if (_spawnCount > 0) _spawnCount--;
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
        private bool _initialized = false;

        private void Update()
        {
            if (!_initialized) return;
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
                EnemyLifeCycle lifeCycle = _enemyPools.GetArtillery().GetComponent<EnemyLifeCycle>();
                lifeCycle.Activate(_spawnPositions[i], HandleArtilleryDeactivated);
                _spawnCount++;
            }
        }

        /// <summary>
        ///     事前配置の位置で敵を生成する。
        /// </summary>
        /// <param name="assignedPositions"></param>
        private void SpawnAssignedEnemy(in Transform[] assignedPositions)
        {
            for (int i = 0; i < assignedPositions.Length; i++)
            {
                EnemyLifeCycle lifeCycle = _enemyPools.GetArtillery().GetComponent<EnemyLifeCycle>();
                lifeCycle.Activate(assignedPositions[i].position, HandleArtilleryDeactivated);
                _spawnCount++;
            }
        }
    }
}
