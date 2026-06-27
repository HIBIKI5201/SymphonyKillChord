using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     砲兵のスポナークラス。
    /// </summary>
    public class EnemyArtillerySpawner : MonoBehaviour, IGameplayControllable
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

            if (!_spawnedAssignedEnemies && _assignedPositions != null)
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
        ///    ゲームプレイの停止処理を行います。
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
        ///     砲兵インスタンスが回収された時のcallback処理。
        /// </summary>
        public void HandleArtilleryDeactivated()
        {
            if (_spawnCount > 0) _spawnCount--;

            ReMoveInactiveEnemies();
        }

        [SerializeField] private EnemyPools _enemyPools;
        [FormerlySerializedAs("_spawnPoint")]
        [SerializeField, Tooltip("マップ外側の実生成地点")]
        private Transform _outsideSpawnPoint;
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
        private bool _isPlaying;

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
                EnemyLifeCycle lifeCycle = _enemyPools.GetArtillery();
                SpawnEnemyAsync(lifeCycle, _spawnPositions[i]);
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
                    Debug.LogError("[EnemyArtillerySpawner] 事前配置の位置情報がNULL。");
                    continue;
                }
                EnemyLifeCycle lifeCycle = _enemyPools.GetArtillery();
                lifeCycle.Activate(assignedPositions[i].position, HandleArtilleryDeactivated);
                lifeCycle.StartGameplay();
                _activeEnemies.Add(lifeCycle);
                _spawnCount++;
            }
        }

        /// <summary>
        ///     非アクティブな敵をリストから削除する。
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

        /// <summary>
        ///     マップ外側から生成地点まで歩かせてから、敵を戦闘状態にする。
        /// </summary>
        /// <param name="lifeCycle">生成する敵のライフサイクル。</param>
        /// <param name="activePosition">戦闘を開始する生成地点。</param>
        private async void SpawnEnemyAsync(EnemyLifeCycle lifeCycle, Vector3 activePosition)
        {
            if (_outsideSpawnPoint == null)
            {
                Debug.LogError("[EnemyArtillerySpawner] マップ外側の実生成地点が未設定。");
                lifeCycle.Activate(activePosition, HandleArtilleryDeactivated);
                _activeEnemies.Add(lifeCycle);
                StartOrStopSpawnedEnemy(lifeCycle);
                return;
            }

            CancellationToken cancellationToken = destroyCancellationToken;

            try
            {
                bool activateSuccess =
                    await lifeCycle.EnterFromOutsideAsync(
                        _outsideSpawnPoint.position,
                        activePosition,
                        HandleArtilleryDeactivated,
                        cancellationToken);

                if (!activateSuccess)
                {
                    if (_spawnCount > 0)
                    {
                        _spawnCount--;
                    }

                    return;
                }
            }
            catch (OperationCanceledException)
            {
                // BattleSceneのアンロードに伴うキャンセルは正常終了として扱う。
                return;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                if (_spawnCount > 0) _spawnCount--;
                return;
            }

            if (cancellationToken.IsCancellationRequested
                || this == null
                || lifeCycle == null)
            {
                return;
            }

            _activeEnemies.Add(lifeCycle);
            StartOrStopSpawnedEnemy(lifeCycle);
        }

        /// <summary>
        ///     スポナーの再生状態に合わせて、生成完了した敵の処理を切り替える。
        /// </summary>
        /// <param name="lifeCycle">生成完了した敵。</param>
        private void StartOrStopSpawnedEnemy(EnemyLifeCycle lifeCycle)
        {
            if (_isPlaying)
            {
                lifeCycle.StartGameplay();
                return;
            }

            lifeCycle.StopGameplay();
        }
    }
}
