using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキルの定義（条件や効果、演出）を管理するドメインクラス。
    /// </summary>
    public class SkillDefinition : IEquatable<SkillDefinition>
    {
        public readonly SkillId Id;
        public readonly SkillPattern SkillPattern;
        public readonly ISkillEffect Effect;
        public readonly ISkillVisual Visual;

        #region 定数

        private const int MIN_PATTERN_LENGTH = 1;

        #endregion

        public SkillDefinition(SkillId id, SkillPattern skillPattern, ISkillEffect effect, ISkillVisual visual)
        {
            Id = id;
            SkillPattern = skillPattern;
            Effect = effect;
            Visual = visual;
        }

        /// <summary>
        /// 逆順にソートした入力を利用してスキルの成立をチェックする
        /// </summary>
        /// <param name="reversInput"></param>
        /// <returns></returns>
        public bool IsMatch(ReadOnlySpan<BeatType> reversInput)
        {
            var length = SkillPattern.Signatures.Length;
            if (length < MIN_PATTERN_LENGTH) return false;
            if (reversInput.Length < length) return false;

            ReadOnlySpan<BeatType> pattern = reversInput.Slice(0, length);
            return SkillPattern.Equals(pattern);
        }

        public bool Equals(SkillDefinition other)
        {
            if (other == null || Id != other.Id) return false;
            return SkillPattern == other.SkillPattern;
        }
    }
}