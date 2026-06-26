using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵の1Wave分の定義。
    /// </summary>
    public class EnemyWaveDefinition
    {
        public EnemyWaveDefinition(EnemyWaveDetail[] details, float waveDuration)
        {
            _details = details;
            _waveDuration = waveDuration;
        }
        /// <summary> 1Wave分の中身 </summary>
        public EnemyWaveDetail[] Details => _details;
        /// <summary> 1Waveの継続時間 </summary>
        public float WaveDuration => _waveDuration;

        private readonly EnemyWaveDetail[] _details;
        private readonly float _waveDuration;
    }
}
