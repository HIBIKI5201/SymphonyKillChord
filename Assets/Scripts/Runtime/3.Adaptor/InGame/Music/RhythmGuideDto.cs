using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    public readonly ref struct RhythmGuideDto
    {
        public RhythmGuideDto(float indicatorNormalized, int? currentBeatCount, IReadOnlyList<RhythmGuideZoneDto> zones, bool hasTarget)
        {
            IndicatorNormalized = indicatorNormalized;
            CurrentBeatCount = currentBeatCount;
            Zones = zones;
            HasTarget = hasTarget;
        }

        public float IndicatorNormalized { get; }
        public int? CurrentBeatCount { get; }
        public IReadOnlyList<RhythmGuideZoneDto> Zones { get; }
        public bool HasTarget { get; }
    }
}
