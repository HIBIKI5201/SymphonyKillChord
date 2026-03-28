using System;
using System.Collections.Generic;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public class SkillCheckService
    {
        public static bool TryCheckSkills(
            IReadOnlyList<SkillDefinition> equipmentSkills,
            ReadOnlySpan<int> history,
            out SkillId skillId)
        {
            Span<int> reversInput = stackalloc int[history.Length];
            for (int i = 0; i < history.Length; i++)
            {
                reversInput[i] = history[^(i + 1)];
            }

            foreach (var skillDefinition in equipmentSkills)
            {
                if (skillDefinition.IsMatch(reversInput))
                {
                    skillId = skillDefinition.Id;
                    return true;
                }
            }

            skillId = default;
            return false;
        }
    }
}