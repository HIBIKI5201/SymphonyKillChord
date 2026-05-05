using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキルの発動に必要な入力パターンを表す構造体。
    /// </summary>
    public readonly struct SkillPattern : IEquatable<SkillPattern>
    {
        public ReadOnlySpan<BeatType> Signatures => _signatures.AsSpan();
        private readonly RhythmPattern _signatures;

        public SkillPattern(RhythmPattern signatures)
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

        public static bool operator ==(ReadOnlySpan<BeatType> left, SkillPattern right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(ReadOnlySpan<BeatType> left, SkillPattern right)
        {
            return !(left == right);
        }

        public bool Equals(SkillPattern other)
        {
            return Equals(_signatures, other._signatures);
        }

        public bool Equals(ReadOnlySpan<BeatType> other)
        {
            ReadOnlySpan<BeatType> signatures = _signatures.AsSpan();

            if (signatures.Length != other.Length)
            {
                return false;
            }

            for (int i = 0; i < signatures.Length; i++)
            {
                if (signatures[i] != other[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is SkillPattern other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (_signatures.GetHashCode());
        }
    }
}