using System;
using UnityEngine;

namespace KillChord.Runtime.Domain
{
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
            Effect.Do();
            Visual.Do();
        }

        /// <summary>
        /// 逆順にソートした入力を利用してスキルの成立をチェックする
        /// </summary>
        /// <param name="reversInput"></param>
        /// <returns></returns>
        public bool IsMatch(ReadOnlySpan<int> reversInput)
        {
            if (SkillPattern.Signatures.Length < MIN_PATTERN_LENGTH) return false;
            if (reversInput.Length < SkillPattern.Signatures.Length) return false;

            ReadOnlySpan<int> pattern = reversInput.Slice(0, SkillPattern.Signatures.Length);
            return pattern.SequenceEqual(SkillPattern.Signatures);
        }

        public bool Equals(SkillDefinition other)
        {
            if (other == null || Id != other.Id) return false;
            return SkillPattern == other.SkillPattern;
        }
    }
}