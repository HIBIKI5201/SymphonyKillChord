using KillChord.Runtime.Domain.InGame.Stage;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵の1Wave分の定義。
    /// </summary>
    public class EnemyWaveDefinition
    {
        /// <summary>
        ///     1Wave分の定義を生成します。
        /// </summary>
        /// <param name="details"> 出現する敵の定義です。 </param>
        /// <param name="waveDuration"> Waveの継続時間です。 </param>
        /// <param name="stageEffects"> Wave開始時のステージ演出です。 </param>
        public EnemyWaveDefinition(
            EnemyWaveDetail[] details,
            float waveDuration,
            IReadOnlyList<IStageEffectDefinition> stageEffects = null)
        {
            _details = details ?? Array.Empty<EnemyWaveDetail>();
            _waveDuration = waveDuration;
            _stageEffects = stageEffects ?? Array.Empty<IStageEffectDefinition>();
        }
        /// <summary> 1Wave分の中身 </summary>
        public EnemyWaveDetail[] Details => _details;
        /// <summary> 1Waveの継続時間 </summary>
        public float WaveDuration => _waveDuration;
        /// <summary> Wave開始時に予約するステージ演出です。 </summary>
        public IReadOnlyList<IStageEffectDefinition> StageEffects => _stageEffects;

        private readonly EnemyWaveDetail[] _details;
        private readonly float _waveDuration;
        private readonly IReadOnlyList<IStageEffectDefinition> _stageEffects;
    }
}
