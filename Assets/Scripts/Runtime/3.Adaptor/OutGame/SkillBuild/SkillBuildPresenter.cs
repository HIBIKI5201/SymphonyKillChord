using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;

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
        /// <param name="textFormatter"> 共通表示文字列フォーマッター。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillBuildPresenter(
            ISkillBuildViewModelWriter viewModel,
            SkillDisplayTextFormatter textFormatter)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _textFormatter = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
        }

        /// <summary>
        ///     スキル編成状態を ViewModel に反映する。
        /// </summary>
        /// <param name="equippedSkills"> 現在装備中のスキル一覧。 </param>
        /// <param name="ownedSkills"> 入手済みスキル一覧。 </param>
        /// <param name="ownedPoints"> 所持ポイント。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Push(
            IReadOnlyList<EquippedSkill> equippedSkills,
            IReadOnlyList<SkillTemplate> ownedSkills,
            int ownedPoints)
        {
            if (equippedSkills == null)
            {
                throw new ArgumentNullException(nameof(equippedSkills));
            }

            if (ownedSkills == null)
            {
                throw new ArgumentNullException(nameof(ownedSkills));
            }

            SkillBuildSlotData[] slots = new SkillBuildSlotData[equippedSkills.Count];
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                EquippedSkill equippedSkill = equippedSkills[i];
                int skillId = equippedSkill.HasSkill
                    ? equippedSkill.SkillTemplate.Id.Value
                    : EMPTY_SKILL_ID;
                slots[i] = new SkillBuildSlotData(i, skillId);
            }

            SkillViewData[] skills = new SkillViewData[ownedSkills.Count];
            for (int i = 0; i < ownedSkills.Count; i++)
            {
                SkillTemplate skillTemplate = ownedSkills[i];
                if (skillTemplate == null)
                {
                    throw new ArgumentException($"入手済みスキル一覧に null が存在します。 index={i}", nameof(ownedSkills));
                }

                SkillDisplayText text = GetOrCreateText(skillTemplate);
                skills[i] = new SkillViewData(
                    skillTemplate.Id.Value,
                    skillTemplate.DisplayName,
                    skillTemplate.Icon,
                    text.ComboLabel,
                    text.SkillTypeLabel,
                    text.HasEffectDescription,
                    text.EffectDescription,
                    skillTemplate.Level.Value);
            }

            SkillBuildViewDTO dto = new(slots, skills, ownedPoints);
            _viewModel.Apply(in dto);
        }

        private const int EMPTY_SKILL_ID = -1;

        private readonly ISkillBuildViewModelWriter _viewModel;
        private readonly SkillDisplayTextFormatter _textFormatter;
        private readonly Dictionary<SkillTemplate, SkillDisplayText> _textCache = new();

        /// <summary>
        ///     スキルテンプレートの共通表示文字列を取得する。
        /// </summary>
        /// <param name="skillTemplate"> スキルテンプレート。 </param>
        /// <returns> 表示文字列。 </returns>
        private SkillDisplayText GetOrCreateText(SkillTemplate skillTemplate)
        {
            if (_textCache.TryGetValue(skillTemplate, out SkillDisplayText cachedText))
            {
                return cachedText;
            }

            SkillDisplayText text = _textFormatter.Format(skillTemplate);
            _textCache.Add(skillTemplate, text);
            return text;
        }
    }
}
