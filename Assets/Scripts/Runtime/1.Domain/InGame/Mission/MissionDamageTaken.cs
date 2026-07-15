using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッション中に受けた累計ダメージを表す値オブジェクトです。
    /// </summary>
    public readonly struct MissionDamageTaken : IEquatable<MissionDamageTaken>
    {
        /// <summary>
        ///     累計ダメージを生成します。
        /// </summary>
        /// <param name="value"> ダメージ値です。 </param>
        public MissionDamageTaken(float value)
        {
            if (value < 0f || float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        /// <summary> 累計ダメージ値です。 </summary>
        public float Value { get; }

        /// <summary>
        ///     ダメージを加算した値を返します。
        /// </summary>
        /// <param name="damage"> 加算するダメージです。 </param>
        /// <returns> 加算後の累計ダメージです。 </returns>
        public MissionDamageTaken Add(float damage)
        {
            if (damage < 0f || float.IsNaN(damage) || float.IsInfinity(damage))
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            return new MissionDamageTaken(Value + damage);
        }

        /// <summary>
        ///     値が等しいか判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrueです。 </returns>
        public bool Equals(MissionDamageTaken other)
        {
            return Value.Equals(other.Value);
        }

        /// <summary>
        ///     値が等しいか判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrueです。 </returns>
        public override bool Equals(object obj)
        {
            return obj is MissionDamageTaken other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを返します。
        /// </summary>
        /// <returns> ハッシュコードです。 </returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
