using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// <param name="descriptionFormatter"> 効果説明フォーマッターです。 </param>
        public BattlePreparationSkillPresenter(
            IBattlePreparationSkillViewModel viewModel,
            SkillEffectDescriptionFormatter descriptionFormatter)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _descriptionFormatter = descriptionFormatter ??
                throw new ArgumentNullException(nameof(descriptionFormatter));
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
        private const string COMBO_LABEL_PREFIX = "発動コンボ: ";
        private const string COMBO_SEPARATOR = " → ";

        private readonly IBattlePreparationSkillViewModel _viewModel;
        private readonly SkillEffectDescriptionFormatter _descriptionFormatter;
        private readonly Dictionary<SkillTemplate, CachedSkillText> _textCache = new();
        private readonly StringBuilder _comboBuilder = new();
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
            CachedSkillText text = GetOrCreateText(skillTemplate);

            return new BattlePreparationSkillDTO(
                slotIndex,
                true,
                skillTemplate.Icon,
                skillTemplate.DisplayName,
                text.ComboLabel,
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
                false,
                string.Empty);
        }

        /// <summary>
        ///     入力パターンから発動コンボ表示を構築します。
        /// </summary>
        /// <param name="pattern"> 入力パターンです。 </param>
        /// <returns> 発動コンボ表示です。 </returns>
        private string BuildComboLabel(IReadOnlyList<BeatType> pattern)
        {
            if (pattern == null || pattern.Count == 0)
            {
                return EMPTY_COMBO_LABEL;
            }

            _comboBuilder.Clear();
            _comboBuilder.Append(COMBO_LABEL_PREFIX);
            for (int i = 0; i < pattern.Count; i++)
            {
                if (i > 0)
                {
                    _comboBuilder.Append(COMBO_SEPARATOR);
                }

                _comboBuilder.Append((int)pattern[i]);
            }

            return _comboBuilder.ToString();
        }

        /// <summary>
        ///     マスターデータから生成した表示文字列を取得します。
        ///     同じスキルでは生成結果を再利用し、画面表示ごとの文字列割り当てを抑えます。
        /// </summary>
        /// <param name="skillTemplate"> 表示対象のスキルテンプレートです。 </param>
        /// <returns> キャッシュされた表示文字列です。 </returns>
        private CachedSkillText GetOrCreateText(SkillTemplate skillTemplate)
        {
            if (_textCache.TryGetValue(skillTemplate, out CachedSkillText cachedText))
            {
                return cachedText;
            }

            bool hasEffectDescription =
                skillTemplate.EffectDisplayMode == SkillEffectDisplayMode.FullDescription &&
                !string.IsNullOrWhiteSpace(skillTemplate.SkillDetail);
            string effectDescription = hasEffectDescription
                ? _descriptionFormatter.Format(
                    skillTemplate.SkillDetail,
                    skillTemplate.EffectSpec.Parameters)
                : string.Empty;

            CachedSkillText text = new(
                BuildComboLabel(skillTemplate.Pattern),
                hasEffectDescription,
                effectDescription);
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

        /// <summary>
        ///     スキルテンプレートから一度だけ生成する表示文字列です。
        /// </summary>
        private readonly struct CachedSkillText
        {
            /// <summary>
            ///     表示文字列を初期化します。
            /// </summary>
            public CachedSkillText(
                string comboLabel,
                bool hasEffectDescription,
                string effectDescription)
            {
                ComboLabel = comboLabel;
                HasEffectDescription = hasEffectDescription;
                EffectDescription = effectDescription;
            }

            public string ComboLabel { get; }
            public bool HasEffectDescription { get; }
            public string EffectDescription { get; }
        }
    }
}
