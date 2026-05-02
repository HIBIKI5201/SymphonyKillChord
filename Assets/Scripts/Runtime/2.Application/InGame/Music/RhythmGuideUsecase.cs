using KillChord.Runtime.Domain.InGame.Music;

namespace KillChord.Runtime.Application.InGame.Music
{
    public class RhythmGuideUsecase
    {
        public RhythmGuideUsecase(RhythmGuideDefinition rhythmGuideDefinition)
        {
            _rhythmGuideDefinition = rhythmGuideDefinition;
        }

        public RhythmGuideDefinition RhythmGuideDefinition => _rhythmGuideDefinition;

        public float CalculateIndicatorNormalized(float barProgress)
        {
            if (barProgress <= 0f) return 0f;
            if (barProgress >= 1f) return 1f;

            return barProgress;
        }

        public BeatType? CalculateCurrentBeatType(float elapsedBeat)
        {
            foreach (RhythmGuideRange range in _rhythmGuideDefinition.GuideRanges)
            {
                if (range.Contains(elapsedBeat))
                {
                    return range.BeatType;
                }
            }
            return null;
        }

        private readonly RhythmGuideDefinition _rhythmGuideDefinition;
    }
}
