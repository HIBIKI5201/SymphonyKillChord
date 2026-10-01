using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            Shutdown();
            _spawnCancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
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

        /// <summary>
        ///     未完了の生成枠をキャンセルし、初期化世代の所有権を解放する。
        /// </summary>
        public void Shutdown()
        {
            _initialized = false;
            _isPlaying = false;
            CancellationTokenSource cancellation = _spawnCancellation;
            _spawnCancellation = null;
            if (cancellation != null)
            {
                cancellation.Cancel();
                cancellation.Dispose();
            }
        }

        private const int SPAWN_RETRY_DELAY_MILLISECONDS = 1000;
        private const int SPAWN_RETRY_LOG_INTERVAL = 10;

        [SerializeField, Tooltip("敵の種類ごとのオブジェクトプール。")] private EnemyPools _enemyPools;
        [SerializeField, Tooltip("敵の生成位置を探索するコンポーネント")]
        private EnemySpawnPositionSearcher _spawnPositionSearcher;

        private readonly List<EnemyLifeCycle> _activeEnemies = new();
        private bool _initialized = false;
        private bool _isPlaying;
        private CancellationTokenSource _spawnCancellation;

        /// <summary>
        ///     敵生成処理。
        /// </summary>
        /// <param name="enemyDefinitionId"> 生成する個別の敵定義IDです。 </param>
        /// <param name="amount"> 生成数です。 </param>
        /// <param name="candidateSpawnPointHashes"> 候補とするスポーンポイントIDです。 </param>
        /// <param name="callback"> 生成完了時に呼ばれます。 </param>
        public void SpawnEnemy(
            EnemyDefinitionId enemyDefinitionId,
            int amount,
            IReadOnlyList<int> candidateSpawnPointHashes,
            Action callback)
        {
            if (!_initialized || _spawnCancellation == null)
            {
                return;
            }

            CancellationToken cancellationToken = _spawnCancellation.Token;
            for (int i = 0; i < amount; i++)
            {
                _ = SpawnEnemyAsync(enemyDefinitionId, candidateSpawnPointHashes, callback, cancellationToken);
            }
        }

        /// <summary>
        ///     シーン破棄時に未完了の生成を取り消す。
        /// </summary>
        private void OnDestroy()
        {
            Shutdown();
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
        ///     予約済みの1体分を入場成功まで再試行し、成功通知を一度だけ行う。
        /// </summary>
        /// <param name="enemyDefinitionId">生成する敵定義ID。</param>
        /// <param name="candidateSpawnPointHashes">再抽選の対象となる生成位置ID。</param>
        /// <param name="callback">入場成功時の通知。</param>
        /// <param name="cancellationToken">この初期化世代のキャンセルトークン。</param>
        private async Task SpawnEnemyAsync(
            EnemyDefinitionId enemyDefinitionId,
            IReadOnlyList<int> candidateSpawnPointHashes,
            Action callback,
            CancellationToken cancellationToken)
        {
            int failedAttempts = 0;
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    SpawnPositionPair positionPair = null;
                    EnemyLifeCycle lifeCycle = null;
                    bool activateSuccess = false;
                    bool hasReservedPosition = false;
                    string failureReason = "入場移動に失敗しました。";
                    try
                    {
                        positionPair = await _spawnPositionSearcher.GetRandomSpawnPositionAsync(
                            candidateSpawnPointHashes,
                            cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();

                        // 探索待機中に停止された場合も、再試行の入場開始は再開まで待つ。
                        if (failedAttempts > 0 && (!_isPlaying || Time.timeScale <= 0f))
                        {
                            await WaitForSpawnRetryAsync(cancellationToken);
                            continue;
                        }

                        positionPair.SetInUse(true);
                        hasReservedPosition = true;
                        lifeCycle = _enemyPools.GetEnemy(enemyDefinitionId);
                        activateSuccess = await lifeCycle.EnterFromOutsideAsync(
                            positionPair,
                            HandleArtilleryDeactivated,
                            cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        failureReason = exception.Message;
                    }
                    finally
                    {
                        if (hasReservedPosition && positionPair != null)
                        {
                            positionPair.SetInUse(false);
                        }
                    }

                    if (activateSuccess)
                    {
                        // 再予約せず、元の生成枠だけを確定する。通知後の例外では再生成しない。
                        callback.Invoke();
                        _activeEnemies.Add(lifeCycle);
                        StartOrStopSpawnedEnemy(lifeCycle);
                        return;
                    }

                    failedAttempts++;
                    if (failedAttempts == 1 || failedAttempts % SPAWN_RETRY_LOG_INTERVAL == 0)
                    {
                        string candidateIds = candidateSpawnPointHashes == null
                            ? "全候補"
                            : string.Join(", ", candidateSpawnPointHashes);
                        Debug.LogWarning(
                            $"[{nameof(EnemyArtillerySpawner)}] 生成枠を保持して再試行します。"
                            + $" 敵ID: {enemyDefinitionId}, 候補ID: {candidateIds}, 失敗回数: {failedAttempts}, 原因: {failureReason}",
                            this);
                    }

                    await WaitForSpawnRetryAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // 終了・再初期化で取り消された生成枠は、次の世代へ持ち越さない。
            }
            catch (Exception exception)
            {
                if (this != null)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        /// <summary>
        ///     再試行の間隔を空け、ゲームプレイ停止・戦闘ポーズ中は生成を再開しない。
        /// </summary>
        /// <param name="cancellationToken">この初期化世代のキャンセルトークン。</param>
        private async Task WaitForSpawnRetryAsync(CancellationToken cancellationToken)
        {
            do
            {
                await Task.Delay(SPAWN_RETRY_DELAY_MILLISECONDS, cancellationToken);
            }
            while (!_isPlaying || Time.timeScale <= 0f);
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
