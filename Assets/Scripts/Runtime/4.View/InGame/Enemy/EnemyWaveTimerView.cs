using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵生成用のタイマーを管理するクラス。
    /// </summary>
    public class EnemyWaveTimerView : MonoBehaviour, IGameplayControllable, IEnemyWaveTimerView
    {
        public void StartGameplay()
        {
            if (_autoSpawnSuppressed)
            {
                return;
            }

            _waveSpawnerController.SpawnNextWave();
        }

        public void StopGameplay()
        {
            StopTimer();
        }

        public void Initialize(EnemyWaveSpawnerController controller)
        {
            _waveSpawnerController = controller;
            _timerActive = false;
            _waveTimer = 0f;
        }

        /// <summary>
        ///     ゲームプレイ開始時のWave自動生成を抑制するかどうかを設定する。
        ///     チュートリアル等、任意のタイミングまで敵を生成させたくない場合に使用する。
        /// </summary>
        /// <param name="suppressed"> 抑制する場合はtrue。 </param>
        public void SetAutoSpawnSuppressed(bool suppressed)
        {
            _autoSpawnSuppressed = suppressed;
        }

        /// <summary>
        ///     タイマーを設定する。
        /// </summary>
        /// <param name="time"></param>
        public void SetTimer(float time)
        {
            _waveTimer = time;
            _timerActive = true;
        }

        /// <summary>
        ///     タイマーを停止する。
        /// </summary>
        public void StopTimer()
        {
            _timerActive = false;
        }

        private void FixedUpdate()
        {
            if (_timerActive)
            {
                if(_waveTimer <= 0f)
                {
                    Debug.Log("[EnemyWaveTimerView] Wave Timeout.");
                    _waveSpawnerController.SpawnNextWave();
                    return;
                }
                _waveTimer -= Time.deltaTime;
            }
        }

        private EnemyWaveSpawnerController _waveSpawnerController;
        private bool _timerActive;
        private float _waveTimer;
        private bool _autoSpawnSuppressed;
    }
}
