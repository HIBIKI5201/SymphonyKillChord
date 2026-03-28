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
            const int MAX_STACKALLOC_SIZE = 256;//入力履歴の最大長　
            if (history.Length > MAX_STACKALLOC_SIZE)
            {
                throw new Exception("入力履歴が長すぎます");
            }
            
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