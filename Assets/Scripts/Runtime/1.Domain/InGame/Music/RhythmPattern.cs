using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Domain
{
    public readonly struct RhythmPattern : IEquatable<RhythmPattern>
    {
        public RhythmPattern(BeatType[] signatures)
        {
            _signatures = signatures;
        }

        public BeatType[] Signatures => _signatures;

        private readonly BeatType[] _signatures;

        public bool Equals(RhythmPattern other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (_signatures.Length != other._signatures.Length)
            {
                return false;
            }

            for (int i = 0; i < _signatures.Length; i++)
            {
                if (_signatures[i] != other._signatures[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is RhythmPattern other && Equals(other);
        }

        public ReadOnlySpan<BeatType> AsSpan()
        {
            return _signatures;
        }
    }
}
