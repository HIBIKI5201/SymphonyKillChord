using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションを識別するためのIDを表す値オブジェクト。
    /// </summary>
    public readonly struct MissionId : IEquatable<MissionId>
    {
        public MissionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("MissionIdが空文字列またはnullです。", nameof(value));
            }

            _value = value;
        }

        public string Value => _value;

        public bool Equals(MissionId other) => _value == other._value;
        public override bool Equals(object obj) => obj is MissionId other && Equals(other);
        public override int GetHashCode() => _value != null ? _value.GetHashCode() : 0;
        public override string ToString() => _value;

        private readonly string _value;
    }
}
