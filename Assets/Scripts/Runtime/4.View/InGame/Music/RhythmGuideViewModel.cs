using KillChord.Runtime.Adaptor.InGame.Music;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View.InGame.Music
{
    public class RhythmGuideViewModel
    {
        public float IndicatorNormalized { get; private set; }
        public int? CurrentBeatType { get; private set; }
        public IReadOnlyList<RhythmGuideZoneDto> Zones { get; private set; } = Array.Empty<RhythmGuideZoneDto>();
        public bool HasTarget { get; private set; }

        public void Apply(in RhythmGuideDto dto)
        {
            IndicatorNormalized = dto.IndicatorNormalized;
            CurrentBeatType = dto.CurrentBeatCount;
            Zones = dto.Zones ?? Array.Empty<RhythmGuideZoneDto>();
            HasTarget = dto.HasTarget;
        }
    }
}
