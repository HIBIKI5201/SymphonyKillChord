using KillChord.Runtime.Domain.InGame.Enemy;
using System;

namespace KillChord.Runtime.Domain.InGame.Stage
{
    /// <summary>
    ///     Wave開始時に予約するステージ演出を表します。
    /// </summary>
    public sealed class StageEffectDefinition : IStageEffectDefinition
    {
        /// <summary>
        ///     ステージ演出定義を生成します。
        /// </summary>
        /// <param name="effectId"> 演出IDです。 </param>
        /// <param name="kind"> 演出種類です。 </param>
        /// <param name="musicSpec"> 音楽同期タイミングです。 </param>
        public StageEffectDefinition(
            string effectId,
            StageEffectKind kind,
            EnemyMusicSpec musicSpec)
        {
            if (string.IsNullOrWhiteSpace(effectId))
            {
                throw new ArgumentException("演出IDが未設定です。", nameof(effectId));
            }

            EffectId = effectId.Trim();
            Kind = kind;
            MusicSpec = musicSpec;
        }

        /// <summary> 演出を識別するIDです。 </summary>
        public string EffectId { get; }

        /// <summary> 演出の種類です。 </summary>
        public StageEffectKind Kind { get; }

        /// <summary> 音楽同期タイミングです。 </summary>
        public EnemyMusicSpec MusicSpec { get; }
    }
}
