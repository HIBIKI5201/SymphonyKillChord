using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    public interface IEnemyWaveTimerView
    {
        public float WaveTimer { get; }
        public void SetTimer(float time);
        public void StopTimer();
    }
}
