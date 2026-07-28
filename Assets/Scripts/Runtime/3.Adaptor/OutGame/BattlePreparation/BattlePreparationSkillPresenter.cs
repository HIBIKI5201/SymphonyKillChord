using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.BattlePreparation
{
    /// <summary>
    ///     装備スキルを ViewModel 用 DTO に変換して反映するプレゼンターです。
    /// </summary>
    public sealed class BattlePreparationSkillPresenter
    {
        /// <summary>
        ///     プレゼンターを初期化します。
        /// </summary>
        /// <param name="viewModel"> 出力先 ViewModel です。 </param>
        /// <param name="textFormatter"> 共通表示文字列フォーマッターです。 </param>
        public BattlePreparationSkillPresenter(
            IBattlePreparationSkillViewModel viewModel,
            SkillDisplayTextFormatter textFormatter)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _textFormatter = textFormatter ??
                throw new ArgumentNullException(nameof(textFormatter));
        }

        /// <summary>
        ///     現在の装備スキルを画面へ反映します。
        /// </summary>
        /// <param name="equippedSkills"> スロット順の装備スキル一覧です。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Push(IReadOnlyList<EquippedSkill> equippedSkills)
        {
            if (equippedSkills == null)
            {
                throw new ArgumentNullException(nameof(equippedSkills));
            }

            EnsureSkillBufferSize(equippedSkills.Count);
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                EquippedSkill equippedSkill = equippedSkills[i];
                _skillBuffer[i] = equippedSkill.HasSkill
                    ? BuildSkill(i, equippedSkill.SkillTemplate)
                    : BuildEmptySlot(i);
            }

            BattlePreparationSkillViewDTO dto =
                new(_skillBuffer.AsSpan(0, equippedSkills.Count));
            _viewModel.Apply(in dto);
        }

        private const string EMPTY_SKILL_LABEL = "未設定";
        private const string EMPTY_COMBO_LABEL = "発動コンボ: —";
        private const string EMPTY_VALUE_LABEL = "—";

        private readonly IBattlePreparationSkillViewModel _viewModel;
        private readonly SkillDisplayTextFormatter _textFormatter;
        private readonly Dictionary<SkillTemplate, SkillDisplayText> _textCache = new();
        private BattlePreparationSkillDTO[] _skillBuffer =
            Array.Empty<BattlePreparationSkillDTO>();

        /// <summary>
        ///     装備中スキルの表示情報を構築します。
        /// </summary>
        /// <param name="slotIndex"> スロット番号です。 </param>
        /// <param name="skillTemplate"> スキルテンプレートです。 </param>
        /// <returns> 表示情報です。 </returns>
        private BattlePreparationSkillDTO BuildSkill(
            int slotIndex,
            SkillTemplate skillTemplate)
        {
            SkillDisplayText text = GetOrCreateText(skillTemplate);

            return new BattlePreparationSkillDTO(
                slotIndex,
                true,
                skillTemplate.Icon,
                skillTemplate.DisplayName,
                text.ComboLabel,
                text.SkillTypeLabel,
                text.HasEffectDescription,
                text.EffectDescription);
        }

        /// <summary>
        ///     空スロットの表示情報を構築します。
        /// </summary>
        /// <param name="slotIndex"> スロット番号です。 </param>
        /// <returns> 空スロットの表示情報です。 </returns>
        private BattlePreparationSkillDTO BuildEmptySlot(int slotIndex)
        {
            return new BattlePreparationSkillDTO(
                slotIndex,
                false,
                null,
                EMPTY_SKILL_LABEL,
                EMPTY_COMBO_LABEL,
                EMPTY_VALUE_LABEL,
                false,
                string.Empty);
        }

        /// <summary>
        ///     マスターデータから生成した表示文字列を取得します。
        ///     同じスキルでは生成結果を再利用し、画面表示ごとの文字列割り当てを抑えます。
        /// </summary>
        /// <param name="skillTemplate"> 表示対象のスキルテンプレートです。 </param>
        /// <returns> キャッシュされた表示文字列です。 </returns>
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

        /// <summary>
        ///     DTO構築用配列を必要な要素数へ調整します。
        ///     スロット数が変わらない限り既存配列を再利用します。
        /// </summary>
        /// <param name="requiredLength"> 必要な要素数です。 </param>
        private void EnsureSkillBufferSize(int requiredLength)
        {
            if (_skillBuffer.Length != requiredLength)
            {
                _skillBuffer = new BattlePreparationSkillDTO[requiredLength];
            }
        }

    }
}
