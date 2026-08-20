using KillChord.Runtime.Adaptor.OutGame.BattlePreparation;
using R3;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View.OutGame.BattlePreparation
{
    /// <summary>
    ///     戦闘準備画面の装備スキル表示状態を保持します。
    /// </summary>
    public sealed class BattlePreparationSkillViewModel :
        IBattlePreparationSkillViewModel,
        IDisposable
    {
        /// <summary> 現在の装備スキル表示一覧です。 </summary>
        public ReadOnlyReactiveProperty<IReadOnlyList<BattlePreparationSkillDTO>> Skills =>
            _skills;

        /// <summary>
        ///     DTO から装備スキル表示状態を反映します。
        /// </summary>
        /// <param name="dto"> 装備スキル一覧です。 </param>
        public void Apply(in BattlePreparationSkillViewDTO dto)
        {
            if (HasSameSkills(dto.Skills))
            {
                return;
            }

            BattlePreparationSkillDTO[] skills =
                new BattlePreparationSkillDTO[dto.Skills.Length];
            dto.Skills.CopyTo(skills);
            _skills.Value = skills;
        }

        /// <summary>
        ///     ReactivePropertyを解放します。
        /// </summary>
        public void Dispose()
        {
            _skills.Dispose();
        }

        private readonly ReactiveProperty<IReadOnlyList<BattlePreparationSkillDTO>> _skills =
            new(Array.Empty<BattlePreparationSkillDTO>());

        /// <summary>
        ///     現在の表示状態と受信した表示状態が同一か判定します。
        ///     同一の場合は配列コピーとUI再構築通知を省略します。
        /// </summary>
        /// <param name="skills"> 比較する装備スキル一覧です。 </param>
        /// <returns> 全項目が同一の場合はtrue。 </returns>
        private bool HasSameSkills(ReadOnlySpan<BattlePreparationSkillDTO> skills)
        {
            IReadOnlyList<BattlePreparationSkillDTO> currentSkills = _skills.Value;
            if (currentSkills.Count != skills.Length)
            {
                return false;
            }

            for (int i = 0; i < skills.Length; i++)
            {
                BattlePreparationSkillDTO current = currentSkills[i];
                BattlePreparationSkillDTO next = skills[i];
                if (!current.Equals(next))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
