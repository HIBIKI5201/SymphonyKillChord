using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Player
{
    /// <summary>
    ///     スキルの設定データを保持するドメインクラス。
    /// </summary>
    public class SkillTemplate
    {
        /// <summary> スキルIDです。 </summary>
        public SkillId Id { get; }

        /// <summary> スキル表示名です。 </summary>
        public string DisplayName { get; }

        /// <summary> スキル詳細です。 </summary>
        public string SkillDetail { get; }

        /// <summary> 改造画面に表示するスキルTipsです。 </summary>
        public string Tips { get; }

        /// <summary> アウトゲーム画面で表示するスキルアイコンです。 </summary>
        public Sprite Icon { get; }

        /// <summary> アウトゲーム画面でのスキル効果表示モードです。 </summary>
        public SkillEffectDisplayMode EffectDisplayMode { get; }

        /// <summary> 入力パターンです。 </summary>
        public BeatType[] Pattern { get; }

        /// <summary> クールダウンの小節比率です。 </summary>
        public double CooldownBarRatio { get; }

        /// <summary> スキルの種類です。 </summary>
        public SkillType[] Type { get; }

        /// <summary> スキルのレベルです。 </summary>
        public SkillLevel Level { get; }

        /// <summary> 効果定義です。 </summary>
        public SkillEffectSpec EffectSpec { get; }

        /// <summary> 再生するアニメーションキーです。 </summary>
        public string AnimationKey { get; }

        /// <summary>
        ///     スキルテンプレートを初期化します。
        /// </summary>
        /// <param name="id"> スキルIDです。 </param>
        /// <param name="pattern"> 入力パターンです。 </param>
        /// <param name="type"> スキルの種類です。 </param>
        /// <param name="level"> スキルのレベルです。 </param>
        /// <param name="cooldownNumerator"> クールダウン分子です。 </param>
        /// <param name="cooldownDenomimator"> クールダウン分母です。 </param>
        /// <param name="effectSpec"> 効果定義です。 </param>
        /// <param name="animationKey"> アニメーションキーです。 </param>
        /// <param name="displayName"> スキルの表示名です </param>
        /// <param name="skillDetail"> スキルの詳細です </param>
        /// <param name="tips"> 改造画面に表示するスキルTipsです。 </param>
        /// <param name="icon"> アウトゲーム画面で表示するアイコンです。 </param>
        /// <param name="effectDisplayMode"> アウトゲーム画面でのスキル効果表示モードです。 </param>
        public SkillTemplate(
            SkillId id,
            BeatType[] pattern,
            SkillType[] type,
            SkillLevel level,
            int cooldownNumerator,
            int cooldownDenomimator,
            SkillEffectSpec effectSpec,
            string animationKey,
            string displayName,
            string skillDetail,
            string tips,
            Sprite icon,
            SkillEffectDisplayMode effectDisplayMode)
        {
            if (cooldownDenomimator <= 0)
            {
                throw new ArgumentException("クールダウン時間の分母は0以下では設定できません。");
            }

            if (cooldownNumerator < 0)
            {
                throw new ArgumentException("クールダウンの分子は0未満では設定できません。");
            }

            Id = id;
            Pattern = pattern;
            CooldownBarRatio = (double)cooldownNumerator / cooldownDenomimator;
            Type = type;
            Level = level;
            EffectSpec = effectSpec;
            AnimationKey = animationKey;
            DisplayName = displayName;
            SkillDetail = skillDetail;
            Tips = tips ?? string.Empty;
            Icon = icon;
            EffectDisplayMode = effectDisplayMode;
        }

        /// <summary>
        ///     SkillDefinitionに変換する。
        /// </summary>
        /// <param name="bpm"> 変換時に使用するBPMです。 </param>
        /// <returns> 変換後のスキル定義です。 </returns>
        public SkillDefinition ToSkillDefinition(double bpm)
        {
            return new SkillDefinition(
                Id,
                new SkillPattern(new(Pattern)),
                Type,
                Level,
                CooldownBarRatio,
                EffectSpec,
                bpm,
                AnimationKey);
        }
    }
}
