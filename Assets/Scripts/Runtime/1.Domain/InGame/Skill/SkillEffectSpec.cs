using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキル効果の定義データです。
    /// </summary>
    public readonly struct SkillEffectSpec
    {
        /// <summary>
        ///     効果定義を初期化します。
        /// </summary>
        /// <param name="effectType"> 効果種別です。 </param>
        /// <param name="targetingType"> 対象解決ルールです。 </param>
        /// <param name="parameters"> 効果処理と表示で共有する数値パラメータです。 </param>
        public SkillEffectSpec(
            SkillEffectType effectType,
            SkillTargetingType targetingType,
            IReadOnlyList<SkillEffectParameter> parameters = null)
        {
            EffectType = effectType;
            TargetingType = targetingType;
            _parameters = CopyParameters(parameters);
        }

        /// <summary> 効果種別です。 </summary>
        public SkillEffectType EffectType { get; }

        /// <summary> 対象解決ルールです。 </summary>
        public SkillTargetingType TargetingType { get; }

        /// <summary> 効果処理と表示で共有する数値パラメータです。 </summary>
        public IReadOnlyList<SkillEffectParameter> Parameters => _parameters ?? Array.Empty<SkillEffectParameter>();

        /// <summary>
        ///     指定したパラメータの取得を試みます。
        /// </summary>
        /// <param name="id"> パラメータ識別子です。 </param>
        /// <param name="parameter"> 取得したパラメータです。 </param>
        /// <returns> パラメータが存在する場合はtrue。 </returns>
        public bool TryGetParameter(SkillEffectParameterId id, out SkillEffectParameter parameter)
        {
            IReadOnlyList<SkillEffectParameter> parameters = Parameters;
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Id != id)
                {
                    continue;
                }

                parameter = parameters[i];
                return true;
            }

            parameter = default;
            return false;
        }

        /// <summary>
        ///     指定したパラメータの値を取得します。
        /// </summary>
        /// <param name="id"> パラメータ識別子です。 </param>
        /// <returns> パラメータ値です。 </returns>
        /// <exception cref="InvalidOperationException"></exception>
        public double GetRequiredValue(SkillEffectParameterId id)
        {
            if (TryGetParameter(id, out SkillEffectParameter parameter))
            {
                return parameter.Value;
            }

            throw new InvalidOperationException(
                $"スキル効果パラメータが設定されていません。EffectType: {EffectType}, ParameterId: {id}");
        }

        private readonly SkillEffectParameter[] _parameters;

        /// <summary>
        ///     パラメータ一覧を複製し、重複を検証します。
        /// </summary>
        /// <param name="parameters"> 複製元です。 </param>
        /// <returns> 複製した配列です。 </returns>
        /// <exception cref="ArgumentException"></exception>
        private static SkillEffectParameter[] CopyParameters(IReadOnlyList<SkillEffectParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return Array.Empty<SkillEffectParameter>();
            }

            SkillEffectParameter[] result = new SkillEffectParameter[parameters.Count];
            HashSet<SkillEffectParameterId> ids = new();
            for (int i = 0; i < parameters.Count; i++)
            {
                SkillEffectParameter parameter = parameters[i];
                if (!ids.Add(parameter.Id))
                {
                    throw new ArgumentException(
                        $"スキル効果パラメータが重複しています。ParameterId: {parameter.Id}",
                        nameof(parameters));
                }

                result[i] = parameter;
            }

            return result;
        }
    }
}
