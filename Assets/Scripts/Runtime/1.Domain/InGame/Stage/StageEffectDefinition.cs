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
            int effectId,
            StageEffectKind kind,
            EnemyMusicSpec musicSpec)
        {
            if (effectId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(effectId), "演出IDに0は使用できません。");
            }

            EffectId = effectId;
            Kind = kind;
            MusicSpec = musicSpec;
        }

        /// <summary> 演出を識別するIDです。 </summary>
        public int EffectId { get; }

        /// <summary> 演出の種類です。 </summary>
        public StageEffectKind Kind { get; }

        /// <summary> 音楽同期タイミングです。 </summary>
        public EnemyMusicSpec MusicSpec { get; }
    }
}
