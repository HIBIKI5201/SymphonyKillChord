using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     歩兵のスポナークラス。
    /// </summary>
    public class EnemyInfantrySpawner : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(in Transform[] assignedPositions)
        {
            _assignedPositions = assignedPositions;
            _spawnPositions = new Vector3[_spawnBatchCount];
            _spawnCount = 0;
            _initialized = true;
            _timer = 0f;
            _isPlaying = false;
            _spawnedAssignedEnemies = false;
            _activeEnemies.Clear();
        }
        /// <summary>
        ///   ゲームプレイの開始処理を行います。
        /// </summary>
        public void StartGameplay()
        {
            if (!_initialized)
            {
                return;
            }

            if (_assignedPositions != null)
            {
                SpawnAssignedEnemy(_assignedPositions);
                _spawnedAssignedEnemies = true;
            }

            // 非アクティブな敵をリストから削除し、残りの敵のゲームプレイを開始する。
            ReMoveInactiveEnemies();

            for (int i = 0; i < _activeEnemies.Count; i++)
            {
                _activeEnemies[i]?.StartGameplay();
            }

            _isPlaying = true;
        }

        /// <summary>
        ///   ゲームプレイの停止処理を行います。
        /// </summary>
        public void StopGameplay()
        {
            // 非アクティブな敵をリストから削除し、残りの敵のゲームプレイを停止する。
            _isPlaying = false;

            ReMoveInactiveEnemies();

            for (int i = 0; i < _activeEnemies.Count; i++)
            {
                _activeEnemies[i]?.StopGameplay();
            }
        }

        /// <summary>
        ///     歩兵インスタンスが回収された時のcallback処理。
        /// </summary>
        public void HandleInfantryDeactivated()
        {
            if (_spawnCount > 0) _spawnCount--;

            ReMoveInactiveEnemies();
        }

        [SerializeField] private EnemyPools _enemyPools;
        [SerializeField, Tooltip("生成位置")] private Transform _spawnPoint;
        [SerializeField, Tooltip("生成距離")] private float _spawnDistance;
        [SerializeField, Tooltip("生成間隔")] private float _spawnInterval;
        [SerializeField, Tooltip("一度の生成数")] private int _spawnBatchCount = 4;
        [SerializeField, Tooltip("敵の最大数。-1は無限")] private int _maxSpawnCount;
        [SerializeField, Tooltip("敵の生成位置を探索するコンポーネント")]
        private EnemySpawnPositionSearcher _spawnPositionSearcher;

        private readonly List<EnemyLifeCycle> _activeEnemies = new();
        private bool _spawnedAssignedEnemies;
        private float _timer;
        private int _spawnCount;
        private Vector3[] _spawnPositions;
        private bool _initialized = false;
        private Transform[] _assignedPositions;
        private bool _isPlaying = false;

        private void Update()
        {
            if (!_initialized || !_isPlaying) return;
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
                EnemyLifeCycle lifeCycle = _enemyPools.GetInfantry();
                lifeCycle.Activate(_spawnPositions[i], HandleInfantryDeactivated);
                lifeCycle.StartGameplay();
                _activeEnemies.Add(lifeCycle);
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
                if (assignedPositions[i] == null)
                {
                    Debug.LogError("[EnemyInfantrySpawner] 事前配置の位置情報がNULL。");
                    continue;
                }
                EnemyLifeCycle lifeCycle = _enemyPools.GetInfantry();
                lifeCycle.Activate(assignedPositions[i].position, HandleInfantryDeactivated);
                lifeCycle.StartGameplay();
                _activeEnemies.Add(lifeCycle);
                _spawnCount++;
            }
        }

        /// <summary>
        ///    非アクティブな敵をリストから削除する。
        /// </summary>
        private void ReMoveInactiveEnemies()
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                if (_activeEnemies[i] == null || !_activeEnemies[i].gameObject.activeSelf)
                {
                    _activeEnemies.RemoveAt(i);
                }
            }
        }
    }
}
