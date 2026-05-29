using System;
using System.Collections.Generic;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Skill;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Skill
{
    /// <summary>
    ///     入力履歴に基づいて発動可能なスキルを判定するサービス。
    /// </summary>
    public class SkillCheckService
    {
        public static bool TryCheckSkills(
            IReadOnlyList<SkillDefinition> equipmentSkills,
            ReadOnlySpan<int> history,
            out int skillIndex, out int lastAttackType)
        {
            const int MAX_STACKALLOC_SIZE = 256; //入力履歴の最大長　
            if (history.Length > MAX_STACKALLOC_SIZE)
            {
                throw new Exception("入力履歴が長すぎます");
            }

            Span<int> reversInput = stackalloc int[history.Length];
            for (int i = 0; i < history.Length; i++)
            {
                reversInput[i] = history[^(i + 1)];
            }

            for (int i = 0; i < equipmentSkills.Count; i++)
            {
                if (equipmentSkills[i].IsMatch(reversInput))
                {
                    skillIndex = i;
                    lastAttackType = reversInput[0];
                    return true;
                }
            }

            skillIndex = -1;
            lastAttackType = reversInput[0];
            return false;
        }
    }
}