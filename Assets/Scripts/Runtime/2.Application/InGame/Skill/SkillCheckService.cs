using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary> 入力履歴に基づいて発動可能なスキルを判定するサービス。 </summary>
    public class SkillCheckService
    {
        /// <summary>
        ///     入力がスキル発動できるかチェックする。
        /// </summary>
        /// <param name="skillDefinition"></param>
        /// <param name="inputHistory"></param>
        /// <returns></returns>
        public bool CheckInput(in SkillDefinition skillDefinition,in ReadOnlySpan<BeatType> inputHistory)
        {
            Span<BeatType> reversedInput = stackalloc BeatType[inputHistory.Length];

            for (int i = 0; i < inputHistory.Length; i++)
            {
                reversedInput[i] = inputHistory[^(i + 1)];
            }

            if (skillDefinition.IsMatch(reversedInput))
            {
                Debug.Log($"[SkillCheckService({skillDefinition.Id.Value})] SKILL TRIGGERED. SKILL ID: {skillDefinition.Id.Value}");
                return true;
            }
            return false;
        }
    }
}