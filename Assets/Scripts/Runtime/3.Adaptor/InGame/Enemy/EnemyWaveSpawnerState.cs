using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Utility.Diagnostics;
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
            _dyingEnemyCount = 0;
            _isLastWave = false;
            _isWaveAnnihilated = false;
        }

        /// <summary>
        ///     Waveの全滅後、撃破した敵の死亡演出がすべて終わった時のイベント。
        ///     次のWaveへの切り替えに使う。
        /// </summary>
        public event Action OnWaveCleared;

        /// <summary> これ以上敵が生成されない状態で、最後の敵のHPが0になった時のイベント </summary>
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

            // 死亡演出の終了待ちの間に次のWaveが始まった場合、前のWaveの全滅で二重に切り替えない。
            if (count > 0)
            {
                _isWaveAnnihilated = false;
            }
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
        ///     敵のHPが0になった時に呼び出す処理。
        ///     キル数の計上と同じ瞬間にWaveの全滅を判定する。
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void NotifyEnemyDefeated()
        {
            _enemyCount--;
            if (_enemyCount < 0)
            {
                throw new Exception($"[EnemyWaveSpawnerState] 敵の数管理に異常が発生しました。敵数：{_enemyCount}");
            }

            // 死亡演出が終わるまでは、次のWaveへの切り替えを待たせる。
            _dyingEnemyCount++;

            if (_enemyCount != 0 || _pendingEnemyCount != 0)
            {
                return;
            }

            if (_isLastWave)
            {
                DevLog.Log("[EnemyWaveSpawnerState] All Wave Cleared.");
                OnWaveAllCleared?.Invoke();
                return;
            }

            DevLog.Log("[EnemyWaveSpawnerState] Wave Annihilated.");
            _isWaveAnnihilated = true;
        }

        /// <summary>
        ///     撃破した敵の死亡演出が終わり、場から消えた時に呼び出す処理。
        ///     全滅済みのWaveで死亡演出がすべて終わったら、次のWaveへ切り替える。
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void NotifyEnemyRemoved()
        {
            _dyingEnemyCount--;
            if (_dyingEnemyCount < 0)
            {
                throw new Exception($"[EnemyWaveSpawnerState] 死亡演出中の敵の数管理に異常が発生しました。敵数：{_dyingEnemyCount}");
            }

            if (!_isWaveAnnihilated || _dyingEnemyCount != 0)
            {
                return;
            }

            _isWaveAnnihilated = false;
            DevLog.Log("[EnemyWaveSpawnerState] Wave Cleared.");
            OnWaveCleared?.Invoke();
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
        private int _dyingEnemyCount;
        private bool _isLastWave;
        private bool _isWaveAnnihilated;
    }
}
