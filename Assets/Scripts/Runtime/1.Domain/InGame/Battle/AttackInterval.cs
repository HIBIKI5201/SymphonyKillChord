using System;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    /// 攻撃時の硬直時間を表すVO。
    /// </summary>
    public readonly struct AttackInterval
    {
        /// <summary>
        /// 攻撃の硬直時間を指定して初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        public AttackInterval(float value)
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
        
        /// <summary>
        /// 攻撃の硬直時間を取得するプロパティ。
        /// </summary>
        public float Value { get; }
        
        /// <summary>
        /// floatへの型変換を行う。
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static implicit operator float(AttackInterval interval) => interval.Value;
    }
}