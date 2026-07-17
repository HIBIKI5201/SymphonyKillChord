using KillChord.Runtime.Domain.InGame.Enemy;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵生成処理
    /// </summary>
    public class EnemyWaveSpawnerState
    {
        public EnemyWaveSpawnerState()
        {
            _enemyCount = 0;
            _isLastWave = false;
        }

        /// <summary> 敵がいなくなった時のイベント </summary>
        public event Action OnWaveCleared;

        /// <summary> これ以上敵が生成されない、かつ敵がいなくなった時のイベント </summary>
        public event Action OnWaveAllCleared;

        /// <summary> Waveが開始されたときにWave番号と定義を通知します。 </summary>
        public event Action<int, EnemyWaveDefinition> OnWaveStarted;

        /// <summary>
        ///     敵数を加算する。
        /// </summary>
        /// <param name="count"></param>
        public void AddEnemyCount(int count)
        {
            _enemyCount += count;
        }

        /// <summary>
        ///     敵死亡時に呼び出す処理。
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void OnEnemyDeath()
        {
            _enemyCount--;
            if( _enemyCount < 0)
            {
                throw new Exception($"[EnemyWaveSpawnerState] 敵の数管理に異常が発生しました。敵数：{_enemyCount}");
            }
            if (_enemyCount == 0)
            {
                if (_isLastWave)
                {
                    Debug.Log("[EnemyWaveSpawnerState] All Wave Cleared.");
                    OnWaveAllCleared?.Invoke();
                }
                else
                {
                    Debug.Log("[EnemyWaveSpawnerState] Wave Cleared.");
                    OnWaveCleared?.Invoke();
                }
            }
        }

        /// <summary>
        ///     最終Waveフラグを設定する。
        /// </summary>
        public void SetLastWave()
        {
            _isLastWave = true;
        }

        /// <summary>
        ///     Wave開始を通知します。
        /// </summary>
        /// <param name="waveIndex"> 開始したWaveインデックスです。 </param>
        /// <param name="definition"> 開始したWave定義です。 </param>
        public void NotifyWaveStarted(
            int waveIndex,
            EnemyWaveDefinition definition)
        {
            OnWaveStarted?.Invoke(waveIndex, definition);
        }

        private int _enemyCount;
        private bool _isLastWave;
    }
}
