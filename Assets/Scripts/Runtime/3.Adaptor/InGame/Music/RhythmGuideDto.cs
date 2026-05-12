using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     リズムガイドの表示用データを保持するDTO。
    /// </summary>
    public readonly ref struct RhythmGuideDto
    {
        /// <summary>
        ///     新しいリズムガイドDTOを生成する。
        /// </summary>
        /// <param name="indicatorNormalized"> インジケーターの正規化位置。 </param>
        /// <param name="currentBeatCount"> 現在の拍数。 </param>
        /// <param name="zones"> 判定ゾーンのリスト。 </param>
        /// <param name="hasTarget"> ターゲットの有無。 </param>
        public RhythmGuideDto(float indicatorNormalized, int? currentBeatCount, IReadOnlyList<RhythmGuideZoneDto> zones, bool hasTarget)
        {
            IndicatorNormalized = indicatorNormalized;
            CurrentBeatCount = currentBeatCount;
            Zones = zones;
            HasTarget = hasTarget;
        }

        /// <summary> インジケーターの正規化位置。 </summary>
        public float IndicatorNormalized { get; }
        /// <summary> 現在の拍数。 </summary>
        public int? CurrentBeatCount { get; }
        /// <summary> 判定ゾーンのリスト。 </summary>
        public IReadOnlyList<RhythmGuideZoneDto> Zones { get; }
        /// <summary> ターゲットの有無。 </summary>
        public bool HasTarget { get; }
    }
}
