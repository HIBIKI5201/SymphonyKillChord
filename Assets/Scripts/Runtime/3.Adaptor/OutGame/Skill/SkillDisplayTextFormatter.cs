using KillChord.Runtime.Adaptor.Persistent.Localization;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.Skill
{
    /// <summary>
    ///     アウトゲーム画面で共通利用するスキル表示文字列を生成する。
    /// </summary>
    public sealed class SkillDisplayTextFormatter
    {
        /// <summary>
        ///     フォーマッターを初期化する。
        /// </summary>
        /// <param name="descriptionFormatter"> 効果説明フォーマッター。 </param>
        /// <param name="beatColors"> 発動コマンドの拍子(BeatType)と色の対応表。 </param>
        /// <param name="textProvider"> 発動コンボ・スキル種類の文言の取得元。nullなら日本語で表示する。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillDisplayTextFormatter(
            SkillEffectDescriptionFormatter descriptionFormatter,
            IReadOnlyDictionary<int, Color> beatColors = null,
            IUiTextProvider textProvider = null)
        {
            _descriptionFormatter = descriptionFormatter ??
                throw new ArgumentNullException(nameof(descriptionFormatter));
            _beatColors = beatColors;
            _textProvider = textProvider;
        }

        /// <summary>
        ///     スキルテンプレートから共通表示文字列を生成する。
        /// </summary>
        /// <param name="skillTemplate"> スキルテンプレート。 </param>
        /// <param name="currentLevel"> 反映する現在のスキルレベルです。未指定時は基準レベルのまま表示します。 </param>
        /// <returns> 共通表示文字列。 </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillDisplayText Format(SkillTemplate skillTemplate, int? currentLevel = null)
        {
            if (skillTemplate == null)
            {
                throw new ArgumentNullException(nameof(skillTemplate));
            }

            string skillTypeLabel = BuildSkillTypeLabel(skillTemplate.Type);
            bool hasFormattedEffect =
                skillTemplate.EffectDisplayMode == SkillEffectDisplayMode.FullDescription &&
                !string.IsNullOrWhiteSpace(skillTemplate.SkillDetail);
            bool isGrown = currentLevel.HasValue && currentLevel.Value > skillTemplate.Level.Value;
            string formattedEffect = hasFormattedEffect
                ? _descriptionFormatter.Format(
                    skillTemplate.SkillDetail,
                    skillTemplate.GetEffectSpec(currentLevel).Parameters,
                    isGrown ? skillTemplate.EffectParameterGrowths : null)
                : string.Empty;

            return new SkillDisplayText(
                BuildComboLabel(skillTemplate.Pattern),
                skillTypeLabel,
                hasFormattedEffect,
                formattedEffect,
                BuildComboStepColors(skillTemplate.Pattern));
        }

        private const string COMBO_LABEL_FORMAT_KEY = "ui.skill.combo_label_format";
        private const string COMBO_LABEL_FORMAT_FALLBACK = "発動コンボ: {0}";
        private const string SKILL_TYPE_ATTACK_KEY = "ui.skill.type_attack";
        private const string SKILL_TYPE_BUFF_KEY = "ui.skill.type_buff";
        private const string SKILL_TYPE_DEBUFF_KEY = "ui.skill.type_debuff";
        private const string SKILL_TYPE_ATTACK_FALLBACK = "攻撃";
        private const string SKILL_TYPE_BUFF_FALLBACK = "バフ";
        private const string SKILL_TYPE_DEBUFF_FALLBACK = "デバフ";
        private const string COMBO_SEPARATOR = " → ";
        private const string SKILL_TYPE_SEPARATOR = " / ";
        private const string EMPTY_VALUE_LABEL = "—";
        private static readonly Color DEFAULT_COMBO_STEP_COLOR = Color.gray;

        private readonly SkillEffectDescriptionFormatter _descriptionFormatter;
        private readonly IReadOnlyDictionary<int, Color> _beatColors;
        private readonly IUiTextProvider _textProvider;
        private readonly StringBuilder _comboBuilder = new();
        private readonly StringBuilder _skillTypeBuilder = new();

        /// <summary>
        ///     入力パターンから発動コンボ表示を構築する。
        /// </summary>
        /// <param name="pattern"> 入力パターン。 </param>
        /// <returns> 発動コンボ表示。 </returns>
        private string BuildComboLabel(IReadOnlyList<BeatType> pattern)
        {
            if (pattern == null || pattern.Count == 0)
            {
                return UiText.Format(
                    _textProvider, COMBO_LABEL_FORMAT_KEY, COMBO_LABEL_FORMAT_FALLBACK, EMPTY_VALUE_LABEL);
            }

            // 拍子の並びだけを組み立て、「発動コンボ: 」などの文言は選択中の言語の書式で付ける。
            _comboBuilder.Clear();
            for (int i = 0; i < pattern.Count; i++)
            {
                if (i > 0)
                {
                    _comboBuilder.Append(COMBO_SEPARATOR);
                }

                _comboBuilder.Append((int)pattern[i]);
            }

            return UiText.Format(
                _textProvider, COMBO_LABEL_FORMAT_KEY, COMBO_LABEL_FORMAT_FALLBACK, _comboBuilder.ToString());
        }

        /// <summary>
        ///     入力パターンから、発動コマンドの入力順に並んだ拍子ごとの色一覧を構築する。
        /// </summary>
        /// <param name="pattern"> 入力パターン。 </param>
        /// <returns> 発動コマンドの入力順に並んだ色一覧。 </returns>
        private Color[] BuildComboStepColors(IReadOnlyList<BeatType> pattern)
        {
            if (pattern == null || pattern.Count == 0)
            {
                return Array.Empty<Color>();
            }

            Color[] result = new Color[pattern.Count];
            for (int i = 0; i < pattern.Count; i++)
            {
                result[i] = _beatColors != null && _beatColors.TryGetValue((int)pattern[i], out Color color)
                    ? color
                    : DEFAULT_COMBO_STEP_COLOR;
            }

            return result;
        }

        /// <summary>
        ///     スキル種類の表示文字列を構築する。
        /// </summary>
        /// <param name="skillTypes"> スキル種類一覧。 </param>
        /// <returns> スキル種類の表示文字列。 </returns>
        private string BuildSkillTypeLabel(IReadOnlyList<SkillType> skillTypes)
        {
            if (skillTypes == null || skillTypes.Count == 0)
            {
                return EMPTY_VALUE_LABEL;
            }

            _skillTypeBuilder.Clear();
            for (int i = 0; i < skillTypes.Count; i++)
            {
                if (i > 0)
                {
                    _skillTypeBuilder.Append(SKILL_TYPE_SEPARATOR);
                }

                _skillTypeBuilder.Append(GetSkillTypeDisplayName(skillTypes[i]));
            }

            return _skillTypeBuilder.ToString();
        }

        /// <summary>
        ///     スキル種類を選択中の言語の表示名へ変換する。
        /// </summary>
        /// <param name="skillType"> スキル種類。 </param>
        /// <returns> 表示名。 </returns>
        private string GetSkillTypeDisplayName(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.Attack => UiText.Get(_textProvider, SKILL_TYPE_ATTACK_KEY, SKILL_TYPE_ATTACK_FALLBACK),
                SkillType.Buff => UiText.Get(_textProvider, SKILL_TYPE_BUFF_KEY, SKILL_TYPE_BUFF_FALLBACK),
                SkillType.Debuff => UiText.Get(_textProvider, SKILL_TYPE_DEBUFF_KEY, SKILL_TYPE_DEBUFF_FALLBACK),
                _ => skillType.ToString(),
            };
        }
    }
}
