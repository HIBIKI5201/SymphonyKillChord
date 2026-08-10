using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッション中のコンボ数を表す値オブジェクトです。
    /// </summary>
    public readonly struct MissionMaxCombo : IEquatable<MissionMaxCombo>
    {
        /// <summary>
        ///     コンボ数を生成します。
        /// </summary>
        /// <param name="value"> コンボ数です。 </param>
        public MissionMaxCombo(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "現在の最大コンボ数は負の値を取ることができません。");
            }

            Value = value;
        }

        /// <summary> 現在の最大コンボ数です。 </summary>
        public int Value { get; }

        /// <summary>
        ///     値が等しいか判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrueです。 </returns>
        public bool Equals(MissionMaxCombo other)
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
            return obj is MissionMaxCombo other && Equals(other);
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
