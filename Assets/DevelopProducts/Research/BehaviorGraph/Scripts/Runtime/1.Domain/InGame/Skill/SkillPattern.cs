using System;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキルの発動に必要な入力パターンを表す構造体。
    /// </summary>
    public readonly struct SkillPattern : IEquatable<SkillPattern>
    {
        public ReadOnlySpan<int> Signatures => _signatures;
        private readonly int[] _signatures;

        public SkillPattern(int[] signatures)
        {
            if (signatures == null) { throw new ArgumentNullException(nameof(signatures)); }
            if (signatures.Length == 0)
                { throw new ArgumentException("signatures must not be empty.", nameof(signatures)); }
            _signatures = signatures;//受け取ったデータは変わらない為そのまま利用する
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
            return (_signatures != null ? _signatures[^1].GetHashCode() : 0);
        }
    }
}