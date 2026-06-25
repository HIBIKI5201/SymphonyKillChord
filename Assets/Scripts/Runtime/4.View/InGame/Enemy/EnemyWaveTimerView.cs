using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    public class EnemyWaveTimerView : MonoBehaviour, IGameplayControllable, IEnemyWaveTimerView
    {

        public float WaveTimer => _waveTimer;
        public void StartGameplay()
        {
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

        public void SetTimer(float time)
        {
            _waveTimer = time;
            _timerActive = true;
        }

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
    }
}
