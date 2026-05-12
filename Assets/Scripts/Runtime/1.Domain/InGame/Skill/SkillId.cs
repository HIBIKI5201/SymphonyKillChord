using System;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    /// スキルを一意に識別するIDを表す構造体。
    /// </summary>
    public readonly struct SkillId : IEquatable<SkillId>
    {
        public int Value => _value;
        private readonly int _value;

        /// <summary>
        /// コンストラクタ。整数値で初期化する。
        /// </summary>
        public SkillId(int value)
        {
            _value = value;
        }

        public static bool operator ==(SkillId left, SkillId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SkillId left, SkillId right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 別のSkillIdと等価か判定する。
        /// </summary>
        public bool Equals(SkillId other)
        {
            return _value == other._value;
        }

        /// <summary>
        /// オブジェクト等価性の判定を行う。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is SkillId other && Equals(other);
        }

        /// <summary>
        /// ハッシュコードを取得する。
        /// </summary>
        public override int GetHashCode()
        {
            return _value;
        }
    }
}