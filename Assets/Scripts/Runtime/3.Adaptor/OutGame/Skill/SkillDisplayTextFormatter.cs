using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// <exception cref="ArgumentNullException"></exception>
        public SkillDisplayTextFormatter(SkillEffectDescriptionFormatter descriptionFormatter)
        {
            _descriptionFormatter = descriptionFormatter ??
                throw new ArgumentNullException(nameof(descriptionFormatter));
        }

        /// <summary>
        ///     スキルテンプレートから共通表示文字列を生成する。
        /// </summary>
        /// <param name="skillTemplate"> スキルテンプレート。 </param>
        /// <returns> 共通表示文字列。 </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillDisplayText Format(SkillTemplate skillTemplate)
        {
            if (skillTemplate == null)
            {
                throw new ArgumentNullException(nameof(skillTemplate));
            }

            string skillTypeLabel = BuildSkillTypeLabel(skillTemplate.Type);
            bool hasFormattedEffect =
                skillTemplate.EffectDisplayMode == SkillEffectDisplayMode.FullDescription &&
                !string.IsNullOrWhiteSpace(skillTemplate.SkillDetail);
            string formattedEffect = hasFormattedEffect
                ? _descriptionFormatter.Format(
                    skillTemplate.SkillDetail,
                    skillTemplate.EffectSpec.Parameters)
                : string.Empty;

            return new SkillDisplayText(
                BuildComboLabel(skillTemplate.Pattern),
                skillTypeLabel,
                hasFormattedEffect,
                formattedEffect);
        }

        private const string EMPTY_COMBO_LABEL = "発動コンボ: —";
        private const string COMBO_LABEL_PREFIX = "発動コンボ: ";
        private const string COMBO_SEPARATOR = " → ";
        private const string SKILL_TYPE_SEPARATOR = " / ";
        private const string EMPTY_VALUE_LABEL = "—";

        private readonly SkillEffectDescriptionFormatter _descriptionFormatter;
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
        ///     スキル種類を日本語の表示名へ変換する。
        /// </summary>
        /// <param name="skillType"> スキル種類。 </param>
        /// <returns> 表示名。 </returns>
        private string GetSkillTypeDisplayName(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.Attack => "攻撃",
                SkillType.Buff => "バフ",
                SkillType.Debuff => "デバフ",
                _ => skillType.ToString(),
            };
        }
    }
}
