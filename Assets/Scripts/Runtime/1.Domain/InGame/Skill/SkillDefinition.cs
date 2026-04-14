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

        public void SkillExecute()
        {
            Effect.Execute();
            Visual.Execute();
        }

        /// <summary>
        /// 逆順にソートした入力を利用してスキルの成立をチェックする
        /// </summary>
        /// <param name="reversInput"></param>
        /// <returns></returns>
        public bool IsMatch(ReadOnlySpan<BeatType> reversInput)
        {
            if (SkillPattern.Signatures.Length < MIN_PATTERN_LENGTH) return false;
            if (reversInput.Length < SkillPattern.Signatures.Length) return false;

            ReadOnlySpan<BeatType> pattern = reversInput.Slice(0, SkillPattern.Signatures.Length);
            return pattern.SequenceEqual(SkillPattern.Signatures);
        }

        public bool Equals(SkillDefinition other)
        {
            if (other == null || Id != other.Id) return false;
            return SkillPattern == other.SkillPattern;
        }
    }
}