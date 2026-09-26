using System;

namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///    コンボ数を表す値オブジェクト。
    /// </summary>
    public readonly struct ComboCount : IEquatable<ComboCount>
    {
        /// <summary>
        ///   コンボ数を表す値オブジェクトを作成する。
        /// </summary>
        /// <param name="value"> コンボ数です。 </param>
        /// <exception cref="System.ArgumentOutOfRangeException"> コンボ数が負の値の場合にスローされます。 </exception>
        public ComboCount(int value)
        {
            if (value < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(value), "コンボ数は負の値を取ることができません。");
            }
            Value = value;
        }
        /// <summary> コンボ数を取得します。 </summary>
        public int Value { get; }
        /// <summary>
        ///     値オブジェクトの等価性を比較します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrueです。 </returns>
        public bool Equals(ComboCount other)
        {
            return Value == other.Value;
        }
        /// <summary>
        ///     値オブジェクトの等価性を比較します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrueです。 </returns>
        public override bool Equals(object obj)
        {
            return obj is ComboCount other && Equals(other);
        }
        /// <summary>
        ///     値オブジェクトのハッシュコードを取得します。
        /// </summary>
        /// <returns> ハッシュコードです。 </returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        /// <summary>
        ///     値オブジェクトの等価性を比較します。
        /// </summary>
        /// <param name="left"> 左辺です。 </param>
        /// <param name="right"> 右辺です。 </param>
        /// <returns> 等しい場合はtrueです。 </returns>
        public static bool operator ==(ComboCount left, ComboCount right) => left.Equals(right);
        /// <summary>
        ///     値オブジェクトの非等価性を比較します。
        /// </summary>
        /// <param name="left"> 左辺です。 </param>
        /// <param name="right"> 右辺です。 </param>
        /// <returns> 等しくない場合はtrueです。 </returns>
        public static bool operator !=(ComboCount left, ComboCount right) => !left.Equals(right);
    }
}
