using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    public class EnemyWaveSpawnerState
    {
        public EnemyWaveSpawnerState()
        {
            _enemyCount = 0;
        }

        /// <summary> 敵がいなくなった時のイベント </summary>
        public event Action OnWaveCleared;

        /// <summary> これ以上敵が生成されない、かつ敵がいなくなった時のイベント </summary>
        public event Action OnWaveAllCleared;

        public void AddEnemyCount(int count)
        {
            Debug.Log($"[EnemyWaveSpawnerState] Add Enemy Count. Current: {_enemyCount}, Add:{count}");
            _enemyCount += count;
        }

        public void OnEnemyDeath()
        {
            _enemyCount--;
            Debug.Log("[EnemyWaveSpawnerState] Enemy Died. Count -1.");
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
                Debug.Log("[EnemyWaveSpawnerState] Wave Cleared.");
                OnWaveCleared?.Invoke();
            }
        }

        public void SetLastWave()
        {
            _isLastWave = true;
        }

        private int _enemyCount;
        private bool _isLastWave;
    }
}
