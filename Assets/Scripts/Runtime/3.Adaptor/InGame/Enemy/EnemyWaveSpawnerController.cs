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
        public EnemyWaveSpawnerController(EnemyWaves waves, EnemyWaveSpawnerState state, IEnemySpawner infantrySpawner, IEnemySpawner artillerySpawner, IEnemyWaveTimerView waveTimer)
        {
            _waves = waves;
            _state = state;
            _infantrySpawner = infantrySpawner;
            _artillerySpawner = artillerySpawner;
            _waveTimer = waveTimer;
            _state.OnWaveCleared += SpawnNextWave;
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
            _state.OnWaveCleared -= SpawnNextWave;
        }

        private EnemyWaves _waves;
        private EnemyWaveSpawnerState _state;
        private IEnemySpawner _infantrySpawner;
        private IEnemySpawner _artillerySpawner;
        private IEnemyWaveTimerView _waveTimer;
        private bool _isSpawningWave;
        private int _lastSpawnFrame = -1;

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
