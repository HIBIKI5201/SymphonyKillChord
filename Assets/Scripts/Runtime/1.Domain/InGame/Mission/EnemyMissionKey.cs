using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションに関連する敵を識別するためのキーを表す値オブジェクト。
    /// </summary>
    public readonly struct EnemyMissionKey : IEquatable<EnemyMissionKey>
    {
        public EnemyMissionKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("EnemyMissionKeyが空文字列またはnullです。", nameof(value));
            }

            _value = value;
        }

        public string Value => _value;

        public bool Equals(EnemyMissionKey other) => _value == other._value;
        public override bool Equals(object obj) => obj is EnemyMissionKey other && Equals(other);
        public override int GetHashCode() => _value != null ? _value.GetHashCode() : 0;
        public override string ToString() => _value;

        private readonly string _value;
    }
}
