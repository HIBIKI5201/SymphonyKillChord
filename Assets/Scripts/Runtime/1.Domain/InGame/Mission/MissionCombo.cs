using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッション中のコンボ数を表す値オブジェクトです。
    /// </summary>
    public readonly struct MissionCombo : IEquatable<MissionCombo>
    {
        /// <summary>
        ///     コンボ数を生成します。
        /// </summary>
        /// <param name="value"> コンボ数です。 </param>
        public MissionCombo(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        /// <summary> コンボ数です。 </summary>
        public int Value { get; }

        /// <summary>
        ///     値が等しいか判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrueです。 </returns>
        public bool Equals(MissionCombo other)
        {
            return Value == other.Value;
        }

        /// <summary>
        ///     値が等しいか判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrueです。 </returns>
        public override bool Equals(object obj)
        {
            return obj is MissionCombo other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを返します。
        /// </summary>
        /// <returns> ハッシュコードです。 </returns>
        public override int GetHashCode()
        {
            return Value;
        }
    }
}
