using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     リズムパターンを表す値オブジェクト。
    /// </summary>
    public readonly struct RhythmPattern : IEquatable<RhythmPattern>
    {
        /// <summary>
        ///     コンストラクタ。
        ///     配列が参照型であるため、引数の配列をコピーして保持する。
        /// </summary>
        /// <param name="signatures"> 拍子配列。 </param>
        public RhythmPattern(BeatType[] signatures)
        {
            if (signatures == null)
            {
                throw new ArgumentNullException(nameof(signatures));
            }

            _signatures = new BeatType[signatures.Length];
            signatures.CopyTo(_signatures, 0);
        }

        /// <summary> 拍子配列(読み取り専用)。 </summary>
        public ReadOnlySpan<BeatType> AsSpan()
        {
            return _signatures;
        }

        /// <summary> 等価比較を行う。 </summary>
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

        /// <summary> オブジェクト比較を行う。 </summary>
        public override bool Equals(object obj)
        {
            return obj is RhythmPattern other && Equals(other);
        }

        /// <summary> ハッシュコードを取得する。 </summary>
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

        private readonly BeatType[] _signatures;
    }
}