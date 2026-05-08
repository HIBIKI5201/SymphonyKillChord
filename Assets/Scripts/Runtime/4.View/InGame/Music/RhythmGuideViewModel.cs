using KillChord.Runtime.Adaptor.InGame.Music;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     リズムガイドの表示状態を管理するビューモデル。
    /// </summary>
    public class RhythmGuideViewModel
    {
        /// <summary> インジケーターの正規化位置。 </summary>
        public float IndicatorNormalized { get; private set; }
        /// <summary> 現在の拍の種類。 </summary>
        public int? CurrentBeatType { get; private set; }
        /// <summary> 判定ゾーンのリスト。 </summary>
        public IReadOnlyList<RhythmGuideZoneDto> Zones { get; private set; } = Array.Empty<RhythmGuideZoneDto>();
        /// <summary> ターゲットの有無。 </summary>
        public bool HasTarget { get; private set; }

        /// <summary>
        ///     DTOから状態を適用する。
        /// </summary>
        /// <param name="dto"> リズムガイドDTO。 </param>
        public void Apply(in RhythmGuideDto dto)
        {
            IndicatorNormalized = dto.IndicatorNormalized;
            CurrentBeatType = dto.CurrentBeatCount;
            Zones = dto.Zones ?? Array.Empty<RhythmGuideZoneDto>();
            HasTarget = dto.HasTarget;
        }
    }
}
