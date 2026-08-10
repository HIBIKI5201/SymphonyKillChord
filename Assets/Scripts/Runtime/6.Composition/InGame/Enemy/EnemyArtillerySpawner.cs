using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     砲兵のスポナークラス。
    /// </summary>
    public class EnemyArtillerySpawner : MonoBehaviour, IGameplayControllable, IEnemySpawner
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize()
        {
            _initialized = true;
            _isPlaying = false;
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
            ReMoveInactiveEnemies();
        }

        [SerializeField] private EnemyPools _enemyPools;
        [SerializeField, Tooltip("敵の生成位置を探索するコンポーネント")]
        private EnemySpawnPositionSearcher _spawnPositionSearcher;

        private readonly List<EnemyLifeCycle> _activeEnemies = new();
        private bool _initialized = false;
        private bool _isPlaying;

        /// <summary>
        ///     敵生成処理。
        /// </summary>
        /// <param name="enemyDefinitionId"> 生成する個別の敵定義IDです。 </param>
        /// <param name="amount"> 生成数です。 </param>
        /// <param name="candidateSpawnPointHashes"> 候補とするスポーンポイントIDです。 </param>
        /// <param name="callback"> 生成完了時に呼ばれます。 </param>
        public async void SpawnEnemy(
            EnemyDefinitionId enemyDefinitionId,
            int amount,
            IReadOnlyList<int> candidateSpawnPointHashes,
            Action callback)
        {
            for (int i = 0;  i < amount; i++)
            {
                try
                {
                    SpawnPositionPair positionPair =
                        await _spawnPositionSearcher.GetRandomSpawnPositionAsync(candidateSpawnPointHashes);
                    EnemyLifeCycle lifeCycle = _enemyPools.GetEnemy(enemyDefinitionId);
                    SpawnEnemyAsync(lifeCycle, positionPair, callback);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
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
        /// <param name="positionPair">選定された生成位置。</param>
        private async void SpawnEnemyAsync(EnemyLifeCycle lifeCycle, SpawnPositionPair positionPair, Action callback)
        {
            CancellationToken cancellationToken = destroyCancellationToken;
            positionPair.SetInUse(true);
            try
            {
                bool activateSuccess =
                    await lifeCycle.EnterFromOutsideAsync(
                        positionPair,
                        HandleArtilleryDeactivated,
                        cancellationToken);

                if (!activateSuccess)
                {
                    positionPair.SetInUse(true);
                    return;
                }
                else
                {
                    callback.Invoke();
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
                return;
            }
            finally
            {
                positionPair.SetInUse(false);
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
