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
            EnemyWaveDefinition waveDefinition = _waves.GetNextWave();
            if(waveDefinition == null)
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
            int enemyCount = 0;
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
                enemyCount += waveDefinition.Details[i].EnemyAmount;
            }
            // Waveのタイマーを設定する
            _waveTimer.SetTimer(waveDefinition.WaveDuration);
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
