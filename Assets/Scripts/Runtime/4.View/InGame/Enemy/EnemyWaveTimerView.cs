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
        /// <summary>
        ///     ゲームプレイ開始時にウェーブタイマーを動かし始める。
        ///     自動スポーンが抑止されている場合は何もしない。
        /// </summary>
        public void StartGameplay()
        {
            if (_autoSpawnSuppressed)
            {
                return;
            }

            _waveSpawnerController.SpawnNextWave();
        }

        /// <summary>
        ///     ゲームプレイ停止時にタイマーを止める。
        /// </summary>
        public void StopGameplay()
        {
            StopTimer();
        }

        /// <summary>
        ///     ウェーブの生成を制御するコントローラーを設定し、タイマーを初期化する。
        /// </summary>
        public void Initialize(EnemyWaveSpawnerController controller)
        {
            _waveSpawnerController = controller;
            _timerActive = false;
            _waveTimer = 0f;
        }

        /// <summary>
        ///     ゲームプレイ開始時のWave自動生成を抑制するかどうかを設定する。
        ///     Mission等から明示的にWaveを開始する場合に使用する。
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

        /// <summary>
        ///     タイマーを減らし、時間切れになったら次のウェーブを生成する。
        ///     自動スポーンが抑止されている場合はタイマーを止める。
        /// </summary>
        private void FixedUpdate()
        {
            if (_timerActive)
            {
                if(_waveTimer <= 0f)
                {
                    Debug.Log("[EnemyWaveTimerView] Wave Timeout.");

                    if (_autoSpawnSuppressed)
                    {
                        StopTimer();
                        return;
                    }

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
