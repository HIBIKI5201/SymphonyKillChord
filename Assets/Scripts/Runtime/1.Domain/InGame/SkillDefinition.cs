using System;
using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public class SkillDefinition : IEquatable<SkillDefinition>
    {
        public readonly SkillId Id;
        public readonly SkillPattern SkillPattern;

        #region 定数

        private const int MIN_PATTERN_LENGTH = 1;

        #endregion

        public SkillDefinition(SkillId id, SkillPattern skillPattern)
        {
            Id = id;
            SkillPattern = skillPattern;
        }

        public bool IsMatch(ReadOnlySpan<int> input)
        {
            if (SkillPattern.Signatures.Length < MIN_PATTERN_LENGTH) return false;
            if (input.Length < SkillPattern.Signatures.Length) return false;

            ReadOnlySpan<int> pattern = input.Slice(0, SkillPattern.Signatures.Length);
            return pattern.SequenceEqual(SkillPattern.Signatures);
        }

        public bool Equals(SkillDefinition other)
        {
            if (other == null || Id != other.Id) return false;
            return SkillPattern == other.SkillPattern;
        }
    }
}