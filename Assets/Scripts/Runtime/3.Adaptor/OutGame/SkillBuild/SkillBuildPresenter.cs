using System;
using System.Collections.Generic;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル編成状態を ViewModel 用 DTO に変換して反映するプレゼンター。
    /// </summary>
    public sealed class SkillBuildPresenter
    {
        /// <summary>
        ///     プレゼンターを初期化する。
        /// </summary>
        /// <param name="viewModel"> 出力先 ViewModel。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillBuildPresenter(ISkillBuildViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        /// <summary>
        ///     スキル編成状態を ViewModel に反映する。
        /// </summary>
        /// <param name="equippedSkills"> 現在装備中のスキル一覧。 </param>
        /// <param name="ownedSkills"> 入手済みスキル一覧。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Push(IReadOnlyList<EquippedSkill> equippedSkills, IReadOnlyList<SkillTemplate> ownedSkills)
        {
            if (equippedSkills == null)
            {
                throw new ArgumentNullException(nameof(equippedSkills));
            }

            if (ownedSkills == null)
            {
                throw new ArgumentNullException(nameof(ownedSkills));
            }

            SkillBuildSlotDTO[] slots = new SkillBuildSlotDTO[equippedSkills.Count];
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                EquippedSkill equippedSkill = equippedSkills[i];
                if (!equippedSkill.HasSkill)
                {
                    slots[i] = new SkillBuildSlotDTO(i, EMPTY_SKILL_ID, EMPTY_SKILL_LABEL);
                    continue;
                }

                int skillId = equippedSkill.SkillTemplate.Id.Value;
                slots[i] = new SkillBuildSlotDTO(i, skillId, equippedSkill.SkillTemplate.DisplayName);
            }

            (int skillId, string skillLabel)[] skills = new (int, string)[ownedSkills.Count];
            for (int i = 0; i < ownedSkills.Count; i++)
            {
                int skillId = ownedSkills[i].Id.Value;
                skills[i] = (skillId, ownedSkills[i].DisplayName);
            }

            SkillBuildViewDTO dto = new(slots, skills);
            _viewModel.Apply(in dto);
        }

        private const int EMPTY_SKILL_ID = -1;
        private const string EMPTY_SKILL_LABEL = "未設定";

        private readonly ISkillBuildViewModel _viewModel;
    }
}