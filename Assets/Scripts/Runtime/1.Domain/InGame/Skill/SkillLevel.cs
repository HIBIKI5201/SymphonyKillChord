using System;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキルのレベルを表す値型オブジェクト。
    /// </summary>
    public readonly struct SkillLevel : IEquatable<SkillLevel>
    {
        /// <summary>
        ///     スキルレベルを初期化します。
        /// </summary>
        /// <param name="value">スキルレベルの値。</param>
        /// <exception cref="ArgumentOutOfRangeException">スキルレベルが0未満の場合にスローされます。</exception>
        public SkillLevel(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "スキルレベルは0以上の整数で設定してください。");
            }
            _value = value;
        }

        /// <summary> スキルレベルの値を取得します。 </summary>
        public int Value => _value;

        /// <summary> 指定したスキルレベルと等しいか判定します。 </summary>
        public bool Equals(SkillLevel other) => _value == other._value;
        /// <summary> 指定したオブジェクトと等しいか判定します。 </summary>
        public override bool Equals(object obj) => obj is SkillLevel other && Equals(other);
        /// <summary> 現在のスキルレベルのハッシュコードを取得します。 </summary>
        public override int GetHashCode() => _value.GetHashCode();

        private readonly int _value;
    }
}
