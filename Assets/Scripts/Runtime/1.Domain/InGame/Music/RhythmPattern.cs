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

        public override int GetHashCode()
        {
            if (_signatures == null)
            {
                return 0;
            }

            HashCode hash = new HashCode();

            for (int i = 0; i < _signatures.Length; i++)
            {
                hash.Add(_signatures[i]);
            }

            return hash.ToHashCode();
        }

        public ReadOnlySpan<BeatType> AsSpan()
        {
            return _signatures;
        }
    }
}