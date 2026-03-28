using System;

namespace KillChord.Runtime.Domain
{
    public readonly struct SkillPattern  : IEquatable<SkillPattern>
    {
        public ReadOnlySpan<int> Signatures => _signatures;
        private readonly int[] _signatures;

        public SkillPattern(int[] signatures)
        {
            _signatures = signatures;
        }

        public static bool operator ==(SkillPattern left, SkillPattern right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SkillPattern left, SkillPattern right)
        {
            return !(left == right);
        }

        public bool Equals(SkillPattern other)
        {
            return Equals(_signatures[^1], other._signatures[^1]);
        }

        public override bool Equals(object obj)
        {
            return obj is SkillPattern other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (_signatures != null ? _signatures.GetHashCode() : 0);
        }
    }
}