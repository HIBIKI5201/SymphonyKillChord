using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    public readonly struct RhythmGuideRange : IEquatable<RhythmGuideRange>
    {
        public RhythmGuideRange(BeatType beatType, float startNormalized, float endNormalized)
        {
            if (startNormalized < 0f || startNormalized > 1f) throw new ArgumentOutOfRangeException(nameof(startNormalized));
            if (endNormalized < startNormalized || endNormalized > 1f) throw new ArgumentOutOfRangeException(nameof(endNormalized));

            _beatType = beatType;
            _startNormalized = startNormalized;
            _endNormalized = endNormalized;
        }

        public BeatType BeatType => _beatType;
        public float StartNormalized => _startNormalized;
        public float EndNormalized => _endNormalized;

        public bool Contains(float elapsedBeat)
        {
            return elapsedBeat >= _startNormalized && elapsedBeat <= _endNormalized;
        }

        public bool Equals(RhythmGuideRange other)
        {
            return _beatType == other._beatType &&
                     _startNormalized.Equals(other._startNormalized) &&
                     _endNormalized.Equals(other._endNormalized);
        }

        private readonly BeatType _beatType;
        private readonly float _startNormalized;
        private readonly float _endNormalized;
    }
}
