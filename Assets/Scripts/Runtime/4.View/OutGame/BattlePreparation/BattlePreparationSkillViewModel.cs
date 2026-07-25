using KillChord.Runtime.Adaptor.OutGame.BattlePreparation;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View.OutGame.BattlePreparation
{
    /// <summary>
    ///     戦闘準備画面の装備スキル表示状態を保持します。
    /// </summary>
    public sealed class BattlePreparationSkillViewModel : IBattlePreparationSkillViewModel
    {
        /// <summary> 装備スキル一覧が更新されたときに通知します。 </summary>
        public event Action<IReadOnlyList<BattlePreparationSkillDTO>> OnSkillsChanged;

        /// <summary> 現在の装備スキル表示一覧です。 </summary>
        public IReadOnlyList<BattlePreparationSkillDTO> Skills => _skills;

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

            if (_skills.Length != dto.Skills.Length)
            {
                _skills = new BattlePreparationSkillDTO[dto.Skills.Length];
            }

            dto.Skills.CopyTo(_skills);
            OnSkillsChanged?.Invoke(_skills);
        }

        private BattlePreparationSkillDTO[] _skills =
            Array.Empty<BattlePreparationSkillDTO>();

        /// <summary>
        ///     現在の表示状態と受信した表示状態が同一か判定します。
        ///     同一の場合は配列コピーとUI再構築通知を省略します。
        /// </summary>
        /// <param name="skills"> 比較する装備スキル一覧です。 </param>
        /// <returns> 全項目が同一の場合はtrue。 </returns>
        private bool HasSameSkills(ReadOnlySpan<BattlePreparationSkillDTO> skills)
        {
            if (_skills.Length != skills.Length)
            {
                return false;
            }

            for (int i = 0; i < skills.Length; i++)
            {
                BattlePreparationSkillDTO current = _skills[i];
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
