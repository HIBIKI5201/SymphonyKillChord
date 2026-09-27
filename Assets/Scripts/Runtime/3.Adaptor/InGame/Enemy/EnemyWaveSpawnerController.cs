using KillChord.Runtime.Domain.InGame.Enemy;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     Wave単位で敵を生成するController。
    /// </summary>
    public class EnemyWaveSpawnerController : IDisposable
    {
        /// <summary>
        ///     Wave生成を制御するControllerを生成する。
        /// </summary>
        /// <param name="waves">Wave定義。</param>
        /// <param name="state">Wave進行状態。</param>
        /// <param name="enemySpawner">個別の敵定義を解決する生成処理。</param>
        /// <param name="waveTimer">Waveタイマー表示。</param>
        /// <param name="autoAdvanceWaves">Waveクリア時に次のWaveへ自動進行する場合はtrue。</param>
        public EnemyWaveSpawnerController(
            EnemyWaves waves,
            EnemyWaveSpawnerState state,
            IEnemySpawner enemySpawner,
            IEnemyWaveTimerView waveTimer,
            bool autoAdvanceWaves)
        {
            _waves = waves;
            _state = state;
            _enemySpawner = enemySpawner;
            _waveTimer = waveTimer;
            _autoAdvanceWaves = autoAdvanceWaves;

            if (_autoAdvanceWaves)
            {
                _state.OnWaveCleared += SpawnNextWave;
            }
        }

        /// <summary>
        ///     次のWaveの生成を行う。
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void SpawnNextWave()
        {
            if (_isSpawningWave || _lastSpawnFrame == Time.frameCount)
            {
                return;
            }

            _isSpawningWave = true;
            _lastSpawnFrame = Time.frameCount;

            try
            {
                if (!_waves.TryGetNextWave(out int waveIndex, out EnemyWaveDefinition waveDefinition))
                {
                    Debug.Log("[EnemyWaveSpawnerController] これ以上のWaveがない。");
                    _waveTimer.StopTimer();
                    return;
                }

                // 入場待ちの敵が未加算の間に、旧Waveの最後の敵が死んでも全滅扱いにしない。
                // 生成失敗・取消は撃破ではないため、成功callbackが来るまで予約を残す。
                for (int i = 0; i < waveDefinition.Details.Length; i++)
                {
                    _state.ReserveEnemySpawns(waveDefinition.Details[i].EnemyAmount);
                }

                // これ以上Wave定義がない時、stateクラスの最終Waveフラグを設定する
                if (_waves.IsLastWave)
                {
                    _state.SetLastWave();
                }

                _state.NotifyWaveStarted(waveIndex, waveDefinition);

                for (int i = 0; i < waveDefinition.Details.Length; i++)
                {
                    SpawnEnemies(
                        waveDefinition.Details[i].EnemyDefinitionId,
                        waveDefinition.Details[i].EnemyAmount,
                        waveDefinition.SpawnPointCandidateHashes);
                }

                // Waveのタイマーを設定する
                _waveTimer.SetTimer(waveDefinition.WaveDuration);
            }
            finally
            {
                _isSpawningWave = false;
            }
        }

        /// <summary>
        ///     自動進行時に購読したウェーブクリアイベントを解除する。
        /// </summary>
        public void Dispose()
        {
            if (_autoAdvanceWaves)
            {
                _state.OnWaveCleared -= SpawnNextWave;
            }
        }

        private EnemyWaves _waves;
        private EnemyWaveSpawnerState _state;
        private IEnemySpawner _enemySpawner;
        private IEnemyWaveTimerView _waveTimer;
        private bool _isSpawningWave;
        private int _lastSpawnFrame = -1;
        private readonly bool _autoAdvanceWaves;

        /// <summary>
        ///     spawnerと数を指定して、敵生成処理を呼び出す。
        /// </summary>
        /// <param name="enemyDefinitionId"> 生成する個別の敵定義IDです。 </param>
        /// <param name="amount"></param>
        /// <param name="candidateSpawnPointHashes"> 候補とするスポーンポイントIDです。 </param>
        private void SpawnEnemies(
            EnemyDefinitionId enemyDefinitionId,
            int amount,
            IReadOnlyList<int> candidateSpawnPointHashes)
        {
            _enemySpawner.SpawnEnemy(
                enemyDefinitionId,
                amount,
                candidateSpawnPointHashes,
                AddStateEnemyCount);
        }

        /// <summary>
        ///     生存中の敵の数を1増やす。
        /// </summary>
        private void AddStateEnemyCount()
        {
            _state.AddEnemyCount(1);
        }
    }
}
