using System;

namespace KillChord.Runtime.Domain.OutGame.Resource
{
    /// <summary>
    ///     ゲーム内リソース（研究ポイント・改造ポイント・強化素材など）を一意に識別するIDです。
    /// </summary>
    public readonly struct GameResourceId : IEquatable<GameResourceId>
    {
        /// <summary>
        ///     GameResourceId の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="value"> DataIDから算出したハッシュ値です。 </param>
        public GameResourceId(int value)
        {
            _value = value;
        }

        /// <summary> IDの値です。 </summary>
        public int Value => _value;

        /// <inheritdoc />
        public bool Equals(GameResourceId other)
        {
            return _value == other._value;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is GameResourceId other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _value.ToString();
        }

        private readonly int _value;
    }
}
