using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッション中に使用した武器種類数を表す値オブジェクトです。
    /// </summary>
    public readonly struct MissionWeaponVariety : IEquatable<MissionWeaponVariety>
    {
        /// <summary>
        ///     武器種類数を生成します。
        /// </summary>
        /// <param name="value"> 武器種類数です。 </param>
        public MissionWeaponVariety(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        /// <summary> 使用した武器種類数です。 </summary>
        public int Value { get; }

        /// <summary>
        ///     値が等しいか判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrueです。 </returns>
        public bool Equals(MissionWeaponVariety other)
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
            return obj is MissionWeaponVariety other && Equals(other);
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
