using KillChord.Runtime.Domain.InGame.Music;

namespace KillChord.Runtime.Domain.InGame.Stage
{
    /// <summary>
    ///     Wave開始時に予約するステージ演出の定義です。
    /// </summary>
    public interface IStageEffectDefinition
    {
        /// <summary> 演出を識別するIDです。 </summary>
        int EffectId { get; }

        /// <summary> 演出の種類です。 </summary>
        StageEffectKind Kind { get; }

        /// <summary> 音楽同期タイミングです。 </summary>
        MusicSyncSpec MusicSpec { get; }
    }
}
