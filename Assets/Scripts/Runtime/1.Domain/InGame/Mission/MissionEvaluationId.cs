using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    public readonly struct MissionEvaluationId : IEquatable<MissionEvaluationId>
    {
        public MissionEvaluationId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("ミッションの評価IDが空です。", nameof(value));
            }

            _value = value;
        }

        public string Value => _value;

        /// <inheritdoc/>
        public bool Equals(MissionEvaluationId other)
        {
            return string.Equals(
                _value,
                other._value,
                StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is MissionEvaluationId other
                && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value ?? string.Empty;
        }

        public static bool operator ==(MissionEvaluationId left, MissionEvaluationId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissionEvaluationId left, MissionEvaluationId right)
        {
            return !left.Equals(right);
        }

        private readonly string _value;
    }
}
