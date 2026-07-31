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
        /// <param name="stageEffectIds"> Wave開始時のステージ演出IDです。 </param>
        /// <param name="spawnPointCandidateHashes"> 敵生成の候補スポーンポイントIDです。空の場合は全スポーンポイントが対象です。 </param>
        public EnemyWaveDefinition(
            EnemyWaveDetail[] details,
            float waveDuration,
            IReadOnlyList<int> stageEffectIds = null,
            IReadOnlyList<int> spawnPointCandidateHashes = null)
        {
            _details = details ?? Array.Empty<EnemyWaveDetail>();
            _waveDuration = waveDuration;
            _stageEffectIds = stageEffectIds ?? Array.Empty<int>();
            _spawnPointCandidateHashes = spawnPointCandidateHashes ?? Array.Empty<int>();
        }
        /// <summary> 1Wave分の中身 </summary>
        public EnemyWaveDetail[] Details => _details;
        /// <summary> 1Waveの継続時間 </summary>
        public float WaveDuration => _waveDuration;
        /// <summary> Wave開始時に予約するステージ演出です。 </summary>
        public IReadOnlyList<int> StageEffectIds => _stageEffectIds;
        /// <summary> 敵生成の候補スポーンポイントIDです。空の場合は全スポーンポイントが対象です。 </summary>
        public IReadOnlyList<int> SpawnPointCandidateHashes => _spawnPointCandidateHashes;

        private readonly EnemyWaveDetail[] _details;
        private readonly float _waveDuration;
        private readonly IReadOnlyList<int> _stageEffectIds;
        private readonly IReadOnlyList<int> _spawnPointCandidateHashes;
    }
}
