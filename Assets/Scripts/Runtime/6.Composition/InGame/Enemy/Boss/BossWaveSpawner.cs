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
    ///     ウェーブの定義に置かれたボスを、シーンに配置したボスを有効にして出現させるスポナー。
    ///     ボスはシーンに1体だけ置くため、1ステージで1回だけ出現させる。
    /// </summary>
    public sealed class BossWaveSpawner : IEnemySpawner, IGameplayControllable, IDisposable
    {
        /// <summary>
        ///     シーンのボスと、出現に使う依存で初期化します。
        /// </summary>
        /// <param name="bossInitializer"> シーンに配置したボスの初期化ドライバです。 </param>
        /// <param name="spawnPositionSearcher"> 出現位置を探すコンポーネントです。 </param>
        /// <param name="waveSpawnerState"> ウェーブの進行状態です。 </param>
        /// <param name="initializeBoss"> ボスを初期化する処理です。成功した場合はtrueを返します。 </param>
        public BossWaveSpawner(
            BossInitializer bossInitializer,
            EnemySpawnPositionSearcher spawnPositionSearcher,
            EnemyWaveSpawnerState waveSpawnerState,
            Func<bool> initializeBoss)
        {
            _bossInitializer = bossInitializer;
            _spawnPositionSearcher = spawnPositionSearcher;
            _waveSpawnerState = waveSpawnerState;
            _initializeBoss = initializeBoss;

            // ウェーブで出現するまでは、シーンのボスを隠しておく。
            _bossInitializer.gameObject.SetActive(false);
        }

        /// <summary>
        ///     ボスを出現させます。2体目以降は出現させません。
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
            if (amount <= 0)
            {
                return;
            }

            if (_hasSpawnStarted || amount > 1)
            {
                Debug.LogError(
                    $"[{nameof(BossWaveSpawner)}] ボスは1ステージに1体だけ出現できます。"
                    + $" 敵ID: {enemyDefinitionId}, 生成数: {amount}");
                if (_hasSpawnStarted)
                {
                    return;
                }
            }

            _hasSpawnStarted = true;
            _ = SpawnBossAsync(candidateSpawnPointHashes, callback, _spawnCancellation.Token);
        }

        /// <summary>
        ///     ゲームプレイを開始します。出現済みのボスがいれば動かします。
        /// </summary>
        public void StartGameplay()
        {
            _isPlaying = true;
            if (_isSpawned && !_isDefeated)
            {
                StartBoss();
            }
        }

        /// <summary>
        ///     ゲームプレイを停止します。出現済みのボスがいれば止めます。
        /// </summary>
        public void StopGameplay()
        {
            _isPlaying = false;
            if (_isBossActive && !_isDefeated)
            {
                _isBossActive = false;
                _bossInitializer.LifeCycle.StopGameplay();
            }
        }

        /// <summary>
        ///     未完了の出現を取り消し、ボスの撃破の購読を解除します。
        /// </summary>
        public void Dispose()
        {
            _spawnCancellation.Cancel();
            _spawnCancellation.Dispose();
            if (_bossInitializer != null && _bossInitializer.LifeCycle != null)
            {
                _bossInitializer.LifeCycle.OnDefeated -= HandleBossDefeated;
            }
        }

        private readonly BossInitializer _bossInitializer;
        private readonly EnemySpawnPositionSearcher _spawnPositionSearcher;
        private readonly EnemyWaveSpawnerState _waveSpawnerState;
        private readonly Func<bool> _initializeBoss;
        private readonly CancellationTokenSource _spawnCancellation = new();
        private bool _hasSpawnStarted;
        private bool _isSpawned;
        private bool _isDefeated;
        private bool _isPlaying;
        private bool _isBossActive;

        /// <summary>
        ///     ボスの撃破をウェーブの進行状態へ通知します。
        /// </summary>
        private void HandleBossDefeated()
        {
            if (_isDefeated)
            {
                return;
            }

            _isDefeated = true;
            _isBossActive = false;

            // ボスは死亡演出を待たずに消えるため、撃破と除去を続けて通知する。
            _waveSpawnerState.NotifyEnemyDefeated();
            _waveSpawnerState.NotifyEnemyRemoved();
        }

        /// <summary>
        ///     出現位置を決めてボスを有効にし、初期化してから出現を確定します。
        /// </summary>
        /// <param name="candidateSpawnPointHashes"> 候補とするスポーンポイントIDです。 </param>
        /// <param name="callback"> 出現を確定したときに呼ばれます。 </param>
        /// <param name="cancellationToken"> 出現を取り消すトークンです。 </param>
        private async Task SpawnBossAsync(
            IReadOnlyList<int> candidateSpawnPointHashes,
            Action callback,
            CancellationToken cancellationToken)
        {
            try
            {
                SpawnPositionPair positionPair = await _spawnPositionSearcher.GetRandomSpawnPositionAsync(
                    candidateSpawnPointHashes,
                    cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                // 歩兵のように外から歩いて入らず、戦場の中の入場位置に直接出現する。
                GameObject bossObject = _bossInitializer.gameObject;
                bossObject.transform.position = positionPair.EntryPosition.position;
                bossObject.SetActive(true);
                if (!_initializeBoss.Invoke())
                {
                    Debug.LogError($"[{nameof(BossWaveSpawner)}] ボスの初期化に失敗しました。", _bossInitializer);
                    bossObject.SetActive(false);
                    return;
                }

                _bossInitializer.LifeCycle.OnDefeated += HandleBossDefeated;
                callback?.Invoke();
                _isSpawned = true;

                // ゲームプレイの開始前に出現した場合は、開始まで隠しておく。
                if (_isPlaying)
                {
                    StartBoss();
                }
                else
                {
                    bossObject.SetActive(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // ステージの終了で取り消された出現は行わない。
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, _bossInitializer);
            }
        }

        /// <summary>
        ///     出現済みのボスのゲームプレイを開始します。
        /// </summary>
        private void StartBoss()
        {
            if (_isBossActive)
            {
                return;
            }

            _isBossActive = true;
            _bossInitializer.LifeCycle.StartGameplay();
        }
    }
}
