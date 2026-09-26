using KillChord.Runtime.Domain.InGame.Skill;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace KillChord.Runtime.Adaptor.OutGame.Skill
{
    /// <summary>
    ///     アウトゲームの各画面で共通利用するスキル効果説明フォーマッターです。
    ///     <para>
    ///     説明テンプレートのプレースホルダーを表示用の数値へ置換します。
    ///     </para>
    ///     内部バッファを再利用するため、UIメインスレッドから逐次使用します。
    /// </summary>
    public sealed class SkillEffectDescriptionFormatter
    {
        private const double PERCENT_SCALE = 100d;
        private const int INITIAL_BUILDER_CAPACITY = 256;
        private const string GROWN_VALUE_OPEN = "（";
        private const string GROWN_VALUE_CLOSE = "）";

        private readonly StringBuilder _builder = new(INITIAL_BUILDER_CAPACITY);

        /// <summary>
        ///     効果説明テンプレートへ、ゲーム処理と共有する数値パラメータを埋め込みます。
        /// </summary>
        /// <param name="template"> 効果説明テンプレートです。 </param>
        /// <param name="parameters"> 効果数値パラメータです。 </param>
        /// <param name="growths"> レベル成長が適用されているパラメータ一覧です。未指定時は括弧表示を行いません。 </param>
        /// <returns> 表示用の効果説明です。 </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string Format(
            string template,
            IReadOnlyList<SkillEffectParameter> parameters,
            IReadOnlyList<SkillEffectParameterGrowth> growths = null)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.Count == 0)
            {
                return template;
            }

            _builder.Clear();
            _builder.Append(template);
            for (int i = 0; i < parameters.Count; i++)
            {
                SkillEffectParameter parameter = parameters[i];
                string formattedValue = FormatValue(parameter);
                if (IsGrowable(parameter.Id, growths))
                {
                    formattedValue = $"{GROWN_VALUE_OPEN}{formattedValue}{GROWN_VALUE_CLOSE}";
                }

                _builder.Replace(GetPlaceholder(parameter.Id), formattedValue);
            }

            return _builder.ToString();
        }

        /// <summary>
        ///     指定したパラメータが、現在レベルで実際に成長しているかを判定します。
        /// </summary>
        /// <param name="id"> パラメータ識別子です。 </param>
        /// <param name="growths"> レベル成長が適用されているパラメータ一覧です。 </param>
        /// <returns> 成長ステップが1件以上設定されている場合はtrue。 </returns>
        private static bool IsGrowable(SkillEffectParameterId id, IReadOnlyList<SkillEffectParameterGrowth> growths)
        {
            if (growths == null)
            {
                return false;
            }

            for (int i = 0; i < growths.Count; i++)
            {
                if (growths[i].ParameterId == id && growths[i].Steps.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     パラメータ識別子に対応するプレースホルダーを取得します。
        ///     固定文字列を返し、フォーマットのたびに識別子文字列を生成しません。
        /// </summary>
        /// <param name="id"> パラメータ識別子です。 </param>
        /// <returns> 波括弧を含むプレースホルダーです。 </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string GetPlaceholder(SkillEffectParameterId id)
        {
            return id switch
            {
                SkillEffectParameterId.DamageMultiplier => "{DamageMultiplier}",
                SkillEffectParameterId.CurrentHealthRatio => "{CurrentHealthRatio}",
                SkillEffectParameterId.HealthCostRatio => "{HealthCostRatio}",
                SkillEffectParameterId.HitCount => "{HitCount}",
                SkillEffectParameterId.CriticalMultiplier => "{CriticalMultiplier}",
                SkillEffectParameterId.DamageTakenIncreaseRate => "{DamageTakenIncreaseRate}",
                SkillEffectParameterId.DurationSeconds => "{DurationSeconds}",
                SkillEffectParameterId.LifeStealRate => "{LifeStealRate}",
                SkillEffectParameterId.HealPerHitCap => "{HealPerHitCap}",
                SkillEffectParameterId.DamageReductionRate => "{DamageReductionRate}",
                SkillEffectParameterId.DamageReductionHitCount => "{DamageReductionHitCount}",
                SkillEffectParameterId.BarrierGainRate => "{BarrierGainRate}",
                SkillEffectParameterId.AttackPowerReductionRate => "{AttackPowerReductionRate}",
                SkillEffectParameterId.AttackPowerReductionCap => "{AttackPowerReductionCap}",
                SkillEffectParameterId.SecondaryDamageRate => "{SecondaryDamageRate}",
                SkillEffectParameterId.InfectionRange => "{InfectionRange}",
                SkillEffectParameterId.InfectionTriggerCount => "{InfectionTriggerCount}",
                SkillEffectParameterId.InfectionDamageRate => "{InfectionDamageRate}",
                SkillEffectParameterId.HitDelaySeconds => "{HitDelaySeconds}",
                SkillEffectParameterId.HitIntervalSeconds => "{HitIntervalSeconds}",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(id),
                    id,
                    "未対応のスキル効果パラメータ識別子です。"),
            };
        }

        /// <summary>
        ///     内部値を変更せず、指定された表示形式の文字列へ変換します。
        ///     <para>
        ///     CultureInfo.InvariantCulture を使用することで、常に一貫した形式で数値を表示できます。
        ///     </para>
        /// </summary>
        /// <param name="parameter"> 変換対象です。 </param>
        /// <returns> 表示用文字列です。 </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string FormatValue(SkillEffectParameter parameter)
        {
            return parameter.DisplayFormat switch
            {
                SkillEffectValueFormat.Number => FormatNumber(parameter.Value),
                SkillEffectValueFormat.Integer => Math.Round(parameter.Value).ToString("0", CultureInfo.InvariantCulture),
                SkillEffectValueFormat.Percent =>
                    $"{Math.Round(parameter.Value * PERCENT_SCALE).ToString("0", CultureInfo.InvariantCulture)}%",
                SkillEffectValueFormat.Multiplier => $"{FormatNumber(parameter.Value)}倍",
                SkillEffectValueFormat.Count => $"{Math.Round(parameter.Value).ToString("0", CultureInfo.InvariantCulture)}回",
                SkillEffectValueFormat.Seconds => $"{FormatNumber(parameter.Value)}秒",
                SkillEffectValueFormat.Meter => $"{FormatNumber(parameter.Value)}m",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(parameter),
                    parameter.DisplayFormat,
                    "未対応のスキル効果値表示形式です。"),
            };
        }

        /// <summary>
        ///     小数値を表示用文字列へ変換します。
        ///     <para>
        ///     FormatValue 同様、CultureInfo.InvariantCulture を使用することで、
        ///     常に一貫した形式で数値を表示できます。
        ///     </para>
        /// </summary>
        /// <param name="value"> 変換する値です。 </param>
        /// <returns> 小数点以下2桁までの表示用文字列です。 </returns>
        private string FormatNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
