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
        /// <param name="value"></param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
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
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ComboCount other)
        {
            return Value == other.Value;
        }
        /// <summary>
        ///     値オブジェクトの等価性を比較します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is ComboCount other && Equals(other);
        }
        /// <summary>
        ///     値オブジェクトのハッシュコードを取得します。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        /// <summary>
        ///     値オブジェクトの等価性を比較します。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ComboCount left, ComboCount right) => left.Equals(right);
        /// <summary>
        ///     値オブジェクトの非等価性を比較します。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ComboCount left, ComboCount right) => !left.Equals(right);
    }
}
