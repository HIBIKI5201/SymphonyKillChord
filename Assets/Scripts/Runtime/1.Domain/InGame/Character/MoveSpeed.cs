using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     移動速度を表す値オブジェクト。
    /// </summary>
    public readonly struct MoveSpeed
    {
        /// <summary>
        ///     移動速度を初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        public MoveSpeed(float value)
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

        /// <summary> 移動速度の値。 </summary>
        public readonly float Value;

        /// <summary>
        ///     floatへの明示的型変換を行う。
        /// </summary>
        public static explicit operator float(MoveSpeed value)
            => value.Value;

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public static bool operator ==(MoveSpeed left, MoveSpeed right)
            => left.Value == right.Value;

        /// <summary>
        ///     不等価比較を行う。
        /// </summary>
        public static bool operator !=(MoveSpeed left, MoveSpeed right)
            => left.Value != right.Value;

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is MoveSpeed moveSpeed && moveSpeed.Value == Value;
        }

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public bool Equals(MoveSpeed other)
        {
            return other.Value == Value;
        }

        /// <summary>
        ///     ハッシュ値を計算する。
        /// </summary>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
