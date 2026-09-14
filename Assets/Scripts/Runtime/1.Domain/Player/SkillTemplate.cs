using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;
using System.Collections.Generic;
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

        /// <summary> レベルアップ時の効果パラメータ成長設定一覧です。 </summary>
        public IReadOnlyList<SkillEffectParameterGrowth> EffectParameterGrowths { get; }

        /// <summary> このスキルのレベル上限です。成長ステップが未設定の場合は既定値になります。 </summary>
        public int MaxLevel => ComputeMaxLevel();

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
        /// <param name="effectParameterGrowths"> レベルアップ時の効果パラメータ成長設定一覧です。未指定時は成長しません。 </param>
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
            IReadOnlyList<SkillEffectParameterGrowth> effectParameterGrowths,
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
            EffectParameterGrowths = CopyEffectParameterGrowths(effectParameterGrowths);
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
        /// <param name="currentLevel"> 反映する現在のスキルレベルです。未指定時はテンプレートの基準レベルのまま変化しません。 </param>
        /// <returns> 変換後のスキル定義です。 </returns>
        public SkillDefinition ToSkillDefinition(double bpm, int? currentLevel = null)
        {
            return new SkillDefinition(
                Id,
                new SkillPattern(new(Pattern)),
                Type,
                currentLevel.HasValue ? new SkillLevel(currentLevel.Value) : Level,
                CooldownBarRatio,
                BuildScaledEffectSpec(currentLevel),
                bpm,
                AnimationKey);
        }

        /// <summary> 成長ステップが未設定の場合に適用するレベル上限の既定値です。 </summary>
        private const int DEFAULT_MAX_LEVEL = 10;

        /// <summary>
        ///     現在レベルに応じて効果パラメータを成長させた効果定義を構築する。
        ///     <para> 成長設定があるパラメータは、基準レベルからのレベルアップ回数分だけStepsを先頭から順に適用する。 </para>
        ///     <para> レベルアップ回数がそのパラメータのStep数を超える場合、超えた分は値を据え置く。 </para>
        /// </summary>
        /// <param name="currentLevel"> 現在のスキルレベルです。 </param>
        /// <returns> 成長後の効果定義です。基準レベル以下の場合は元の効果定義をそのまま返します。 </returns>
        private SkillEffectSpec BuildScaledEffectSpec(int? currentLevel)
        {
            if (!currentLevel.HasValue || currentLevel.Value <= Level.Value)
            {
                return EffectSpec;
            }

            IReadOnlyList<SkillEffectParameter> parameters = EffectSpec.Parameters;
            SkillEffectParameter[] scaledParameters = new SkillEffectParameter[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                SkillEffectParameter parameter = parameters[i];
                double value = TryGetGrowth(parameter.Id, out SkillEffectParameterGrowth growth)
                    ? ApplyGrowth(growth, parameter.Value, currentLevel.Value)
                    : parameter.Value;
                scaledParameters[i] = new SkillEffectParameter(parameter.Id, value, parameter.DisplayFormat);
            }

            return new SkillEffectSpec(
                EffectSpec.EffectType,
                EffectSpec.TargetingType,
                scaledParameters,
                EffectSpec.ReapplyPolicy,
                EffectSpec.SkillNormalAttackDamagePolicy);
        }

        /// <summary>
        ///     レベルアップ回数分だけ、成長ステップを先頭から順に適用する。
        /// </summary>
        /// <param name="growth"> 対象パラメータの成長設定です。 </param>
        /// <param name="baseValue"> 基準レベルでの値です。 </param>
        /// <param name="currentLevel"> 現在のスキルレベルです。 </param>
        /// <returns> 成長後の値です。 </returns>
        private double ApplyGrowth(SkillEffectParameterGrowth growth, double baseValue, int currentLevel)
        {
            int levelUpCount = currentLevel - Level.Value;
            IReadOnlyList<SkillEffectParameterGrowthStep> steps = growth.Steps;
            int appliedCount = Math.Min(levelUpCount, steps.Count);

            double value = baseValue;
            for (int i = 0; i < appliedCount; i++)
            {
                value = steps[i].ApplyTo(value);
            }

            return value;
        }

        /// <summary>
        ///     指定したパラメータの成長設定の取得を試みる。
        /// </summary>
        /// <param name="parameterId"> パラメータ識別子です。 </param>
        /// <param name="growth"> 取得した成長設定です。 </param>
        /// <returns> 成長設定が存在する場合はtrue。 </returns>
        private bool TryGetGrowth(SkillEffectParameterId parameterId, out SkillEffectParameterGrowth growth)
        {
            for (int i = 0; i < EffectParameterGrowths.Count; i++)
            {
                if (EffectParameterGrowths[i].ParameterId != parameterId)
                {
                    continue;
                }

                growth = EffectParameterGrowths[i];
                return true;
            }

            growth = default;
            return false;
        }

        /// <summary>
        ///     成長ステップの最大件数からこのスキルのレベル上限を算出する。
        /// </summary>
        /// <returns> レベル上限です。成長ステップが1件も無ければ既定値を返します。 </returns>
        private int ComputeMaxLevel()
        {
            int maxStepCount = 0;
            for (int i = 0; i < EffectParameterGrowths.Count; i++)
            {
                int stepCount = EffectParameterGrowths[i].Steps.Count;
                if (stepCount > maxStepCount)
                {
                    maxStepCount = stepCount;
                }
            }

            return maxStepCount > 0 ? Level.Value + maxStepCount : DEFAULT_MAX_LEVEL;
        }

        /// <summary>
        ///     成長設定一覧を複製し、重複を検証する。
        /// </summary>
        /// <param name="effectParameterGrowths"> 複製元です。 </param>
        /// <returns> 複製した配列です。 </returns>
        /// <exception cref="ArgumentException"></exception>
        private static SkillEffectParameterGrowth[] CopyEffectParameterGrowths(
            IReadOnlyList<SkillEffectParameterGrowth> effectParameterGrowths)
        {
            if (effectParameterGrowths == null || effectParameterGrowths.Count == 0)
            {
                return Array.Empty<SkillEffectParameterGrowth>();
            }

            SkillEffectParameterGrowth[] result = new SkillEffectParameterGrowth[effectParameterGrowths.Count];
            HashSet<SkillEffectParameterId> ids = new();
            for (int i = 0; i < effectParameterGrowths.Count; i++)
            {
                SkillEffectParameterGrowth growth = effectParameterGrowths[i];
                if (!ids.Add(growth.ParameterId))
                {
                    throw new ArgumentException(
                        $"効果パラメータ成長設定が重複しています。ParameterId: {growth.ParameterId}",
                        nameof(effectParameterGrowths));
                }

                result[i] = growth;
            }

            return result;
        }
    }
}
