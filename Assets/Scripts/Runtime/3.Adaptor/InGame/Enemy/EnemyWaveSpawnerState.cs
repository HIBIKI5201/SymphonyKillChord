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
        /// <summary>
        ///     敵の数を0にした初期状態で生成する。
        /// </summary>
        public EnemyWaveSpawnerState()
        {
            _enemyCount = 0;
            _pendingEnemyCount = 0;
            _isLastWave = false;
        }

        /// <summary> 敵がいなくなった時のイベント </summary>
        public event Action OnWaveCleared;

        /// <summary> これ以上敵が生成されない、かつ敵がいなくなった時のイベント </summary>
        public event Action OnWaveAllCleared;

        /// <summary> Waveが開始されたときにWave番号と定義を通知します。 </summary>
        public event Action<int, EnemyWaveDefinition> OnWaveStarted;

        /// <summary>
        ///     非同期生成を開始する前に、Wave全体の生成予定数を予約します。
        /// </summary>
        /// <param name="count"> 生成予定の敵数です。 </param>
        public void ReserveEnemySpawns(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            _pendingEnemyCount = checked(_pendingEnemyCount + count);
        }

        /// <summary>
        ///     入場が完了した敵を生成待ちから生存数へ移します。
        /// </summary>
        /// <param name="count"> 入場が完了した敵数です。 </param>
        public void AddEnemyCount(int count)
        {
            if (count <= 0 || count > _pendingEnemyCount)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            _pendingEnemyCount -= count;
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
            if (_enemyCount == 0 && _pendingEnemyCount == 0)
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
        private int _pendingEnemyCount;
        private bool _isLastWave;
    }
}
