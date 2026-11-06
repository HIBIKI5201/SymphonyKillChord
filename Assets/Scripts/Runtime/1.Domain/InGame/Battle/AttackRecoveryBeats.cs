using System;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃硬直の長さを拍数で表すVO。
    ///     秒数に直すときは、その時点の1拍の長さを掛ける。
    /// </summary>
    public readonly struct AttackRecoveryBeats : IEquatable<AttackRecoveryBeats>
    {
        /// <summary>
        ///     攻撃硬直の拍数を生成する。
        ///     負の値や有限でない値は例外を投げる。
        /// </summary>
        /// <param name="value"> 攻撃硬直の拍数。 </param>
        /// <exception cref="ArgumentException"></exception>
        public AttackRecoveryBeats(float value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value must be non-negative.", nameof(value));
            }

            if (!float.IsFinite(value))
            {
                throw new ArgumentException("value must be finite.", nameof(value));
            }

            Value = value;
        }

        /// <summary> 攻撃硬直の拍数。 </summary>
        public float Value { get; }

        /// <summary>
        ///     1拍の長さから攻撃硬直の秒数を求める。
        /// </summary>
        /// <param name="beatLengthSeconds"> 1拍の長さ（秒）。 </param>
        /// <returns> 攻撃硬直の秒数。1拍の長さが0以下か有限でない場合は0。 </returns>
        public AttackInterval ToInterval(double beatLengthSeconds)
        {
            if (!double.IsFinite(beatLengthSeconds) || beatLengthSeconds <= 0d)
            {
                return new AttackInterval(0f);
            }

            return new AttackInterval((float)(Value * beatLengthSeconds));
        }

        /// <summary>
        ///     他のオブジェクトと値が等しいかを判定する。
        /// </summary>
        public override bool Equals(object obj)
            => obj is AttackRecoveryBeats other && Equals(other);

        /// <summary>
        ///     他の攻撃硬直の拍数と値が等しいかを判定する。
        /// </summary>
        public bool Equals(AttackRecoveryBeats other)
            => other.Value == Value;

        /// <summary>
        ///     値から算出したハッシュコードを返す。
        /// </summary>
        public override int GetHashCode()
            => Value.GetHashCode();
    }
}
