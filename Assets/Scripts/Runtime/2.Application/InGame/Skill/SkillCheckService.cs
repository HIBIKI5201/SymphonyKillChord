using System;
using System.Collections.Generic;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    /// 入力履歴に基づいて発動可能なスキルを判定するサービス。
    /// </summary>
    public class SkillCheckService
    {
        /// <summary>
        /// 装備中のスキル群と入力履歴から、発動したスキルのインデックスと最後の攻撃タイプを取得する。
        /// </summary>
        public bool TryCheckSkills(
            IReadOnlyList<SkillDefinition> equipmentSkills,
            ReadOnlySpan<BeatType> history,
            out int skillIndex, out BeatType lastAttackType)
        {
            const int MAX_STACKALLOC_SIZE = 256; //入力履歴の最大長　
            if (history.Length > MAX_STACKALLOC_SIZE)
            {
                throw new Exception("入力履歴が長すぎます");
            }

            Span<BeatType> reversInput = stackalloc BeatType[history.Length];
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