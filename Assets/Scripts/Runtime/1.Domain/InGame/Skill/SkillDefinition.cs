using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    /// スキルの定義（条件・効果・演出）を保持するドメインクラス。
    /// </summary>
    public class SkillDefinition : IEquatable<SkillDefinition>
    {
        public readonly SkillId Id;
        public readonly SkillPattern SkillPattern;
        public readonly ISkillEffect Effect;

        #region 定数

        private const int MIN_PATTERN_LENGTH = 1;

        #endregion

        /// <summary>
        /// コンストラクタ。ID・パターン・効果を指定して初期化する。
        /// </summary>
        public SkillDefinition(SkillId id, SkillPattern skillPattern, ISkillEffect effect)
        {
            Id = id;
            SkillPattern = skillPattern;
            Effect = effect;
        }

        /// <summary>
        /// 逆順の入力履歴からこのスキルの発動条件に一致するか判定する。
        /// </summary>
        /// <param name="reversInput">逆順に並べた入力履歴</param>
        /// <returns>一致する場合はtrue</returns>
        public bool IsMatch(ReadOnlySpan<BeatType> reversInput)
        {
            var length = SkillPattern.Signatures.Length;
            if (length < MIN_PATTERN_LENGTH) return false;
            if (reversInput.Length < length) return false;

            ReadOnlySpan<BeatType> pattern = reversInput.Slice(0, length);
            return SkillPattern.Equals(pattern);
        }

        /// <summary>
        /// 指定したSkillDefinitionと等価か判定する。
        /// </summary>
        /// <param name="other">比較対象のSkillDefinition</param>
        /// <returns>等しい場合はtrue</returns>
        public bool Equals(SkillDefinition other)
        {
            if (other == null || Id != other.Id) return false;
            return SkillPattern == other.SkillPattern;
        }
    }
}