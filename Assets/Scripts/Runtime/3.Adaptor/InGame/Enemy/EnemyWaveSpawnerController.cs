using KillChord.Runtime.Domain.InGame.Enemy;
using System;
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
        /// <param name="infantrySpawner">歩兵の生成処理。</param>
        /// <param name="artillerySpawner">砲兵の生成処理。</param>
        /// <param name="waveTimer">Waveタイマー表示。</param>
        /// <param name="autoAdvanceWaves">Waveクリア時に次のWaveへ自動進行する場合はtrue。</param>
        public EnemyWaveSpawnerController(
            EnemyWaves waves,
            EnemyWaveSpawnerState state,
            IEnemySpawner infantrySpawner,
            IEnemySpawner artillerySpawner,
            IEnemyWaveTimerView waveTimer,
            bool autoAdvanceWaves)
        {
            _waves = waves;
            _state = state;
            _infantrySpawner = infantrySpawner;
            _artillerySpawner = artillerySpawner;
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

                // これ以上Wave定義がない時、stateクラスの最終Waveフラグを設定する
                if (_waves.IsLastWave)
                {
                    _state.SetLastWave();
                }

                _state.NotifyWaveStarted(waveIndex, waveDefinition);

                for (int i = 0; i < waveDefinition.Details.Length; i++)
                {
                    switch (waveDefinition.Details[i].EnemyType)
                    {
                        case EnemyType.Infantry:
                            SpawnEnemies(_infantrySpawner, waveDefinition.Details[i].EnemyAmount);
                            break;
                        case EnemyType.Artillery:
                            SpawnEnemies(_artillerySpawner, waveDefinition.Details[i].EnemyAmount);
                            break;
                        default:
                            throw new Exception("[EnemyWaveSpawnerController] 敵種類が不正です。");
                    }
                }

                // Waveのタイマーを設定する
                _waveTimer.SetTimer(waveDefinition.WaveDuration);
            }
            finally
            {
                _isSpawningWave = false;
            }
        }

        public void Dispose()
        {
            if (_autoAdvanceWaves)
            {
                _state.OnWaveCleared -= SpawnNextWave;
            }
        }

        private EnemyWaves _waves;
        private EnemyWaveSpawnerState _state;
        private IEnemySpawner _infantrySpawner;
        private IEnemySpawner _artillerySpawner;
        private IEnemyWaveTimerView _waveTimer;
        private bool _isSpawningWave;
        private int _lastSpawnFrame = -1;
        private readonly bool _autoAdvanceWaves;

        /// <summary>
        ///     spawnerと数を指定して、敵生成処理を呼び出す。
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="amount"></param>
        private void SpawnEnemies(IEnemySpawner spawner, int amount)
        {
            spawner.SpawnEnemy(amount, AddStateEnemyCount);
        }

        private void AddStateEnemyCount()
        {
            _state.AddEnemyCount(1);
        }
    }
}
