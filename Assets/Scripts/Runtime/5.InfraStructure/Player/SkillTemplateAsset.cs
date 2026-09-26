using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    /// <summary>
    ///     スキルデータの設定を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTemplate", menuName = "Game/SkillTemplate")]
    public class SkillTemplateAsset : ScriptableObject
    {
        /// <summary> スキルIDです。 </summary>
        public SkillId Id => new SkillId(_id.Id);

        /// <summary> 表示名です。 </summary>
        public string DisplayName => _displayName;

        /// <summary> スキル詳細です。 </summary>
        public string SkillDetail => _skillDetail;

        /// <summary> 改造画面に表示するスキルTipsです。 </summary>
        public string Tips => _tips;

        /// <summary> アウトゲーム画面で表示するスキルアイコンです。 </summary>
        public Sprite Icon => _icon;

        /// <summary> アウトゲーム画面でのスキル効果表示モードです。 </summary>
        public SkillEffectDisplayMode EffectDisplayMode => _effectDisplayMode;

        /// <summary> 入力パターンです。 </summary>
        public BeatType[] Pattern => _pattern;

        /// <summary> クールダウンの分子です。 </summary>
        public int CooldownNumerator => _cooldownNumerator;

        /// <summary> クールダウンの分母です。 </summary>
        public int CooldownDenomimator => _cooldownDenomimator;

        /// <summary> スキルの種類です。 </summary>
        public SkillType[] SkillType => _skillType;

        /// <summary> スキルのレベルです。 </summary>
        public SkillLevel Level => new SkillLevel(_level);

        /// <summary> 効果定義です。 </summary>
        public SkillEffectSpec EffectSpec => new SkillEffectSpec(
            _skillEffectType,
            _skillTargetingType,
            BuildEffectParameters(),
            _statusEffectReapplyPolicy,
            _skillNormalAttackDamagePolicy);

        /// <summary> レベルアップ時の効果パラメータ成長設定一覧です。 </summary>
        public SkillEffectParameterGrowth[] EffectParameterGrowths => BuildEffectParameterGrowths();

        /// <summary> アニメーションキーです。 </summary>
        public string AnimationKey => _animationKey;

        /// <summary>
        ///     Domain層のSkillDataに変換する。
        /// </summary>
        /// <returns> Domain層のテンプレートです。 </returns>
        public SkillTemplate ToDomain()
        {
            return new SkillTemplate(
                Id, _pattern, _skillType, Level, _cooldownNumerator, _cooldownDenomimator,
                EffectSpec, EffectParameterGrowths, _animationKey, _displayName, _skillDetail, _tips, _icon, _effectDisplayMode);
        }

        private const string EFFECT_PARAMETER_PLACEHOLDER_PATTERN = "\\{([^{}]+)\\}";
        private const int PLACEHOLDER_ID_GROUP = 1;

        [SerializeField, Tooltip("スキルIDです。")]
        [SourceDataCollection("Skill")]
        private DataID _id;

        [SerializeField, Tooltip("スキル表示名です。")] private string _displayName;

        [SerializeField, Tooltip("入力パターンです。")] private BeatType[] _pattern;

        [SerializeField, Tooltip("アウトゲーム画面で表示するスキルアイコンです。")]
        private Sprite _icon;

        [SerializeField, TextArea, Tooltip("スキル詳細です。")] private string _skillDetail;

        [SerializeField, TextArea, Tooltip("改造画面に表示するスキルTipsです。空欄の場合は表示しません。")]
        private string _tips;

        [SerializeField, Tooltip("アウトゲーム画面でのスキル効果表示モードです。")]
        private SkillEffectDisplayMode _effectDisplayMode;

        [SerializeField, Tooltip("効果処理と説明文で共有する数値パラメータです。")]
        private SkillEffectParameterSetting[] _effectParameters = Array.Empty<SkillEffectParameterSetting>();

        [SerializeField, Tooltip("レベルアップ時に成長させるパラメータのみ設定してください。未設定のパラメータは変化しません。")]
        private SkillEffectParameterGrowthSetting[] _effectParameterGrowths = Array.Empty<SkillEffectParameterGrowthSetting>();

        [SerializeField, Tooltip("スキルの種類です。")]
        private SkillType[] _skillType;

        [SerializeField, Min(0), Tooltip("スキルのレベルです。")]
        private int _level;

        [SerializeField, Min(0), Tooltip("小節単位で表すクールダウン時間の分子です。")]
        private int _cooldownNumerator = 1;

        [SerializeField, Min(1), Tooltip("小節単位で表すクールダウン時間の分母です。")]
        private int _cooldownDenomimator = 1;

        [SerializeField, Tooltip("スキル効果の識別子です。")]
        private SkillEffectType _skillEffectType;

        [SerializeField, Tooltip("スキル対象の解決ルールです。")]
        private SkillTargetingType _skillTargetingType;

        [SerializeField, Tooltip("状態効果の再付与ポリシーです。")]
        private StatusEffectReapplyPolicy _statusEffectReapplyPolicy = StatusEffectReapplyPolicy.Ignore;

        [SerializeField, Tooltip("スキル発動時の通常攻撃ダメージの計算ポリシーです。")]
        private SkillNormalAttackDamagePolicy _skillNormalAttackDamagePolicy = SkillNormalAttackDamagePolicy.Apply;

        [SerializeField, Tooltip("スキル発動時に再生するアニメーションキー。空なら通常攻撃アニメーションを使う。")]
        private string _animationKey;

        /// <summary>
        ///     Inspector 設定からドメイン用パラメータ一覧を構築します。
        /// </summary>
        /// <returns> ドメイン用パラメータ一覧です。 </returns>
        private SkillEffectParameter[] BuildEffectParameters()
        {
            if (_effectParameters == null || _effectParameters.Length == 0)
            {
                return Array.Empty<SkillEffectParameter>();
            }

            SkillEffectParameter[] result = new SkillEffectParameter[_effectParameters.Length];
            for (int i = 0; i < _effectParameters.Length; i++)
            {
                result[i] = _effectParameters[i].ToDomain();
            }

            return result;
        }

        /// <summary>
        ///     Inspector 設定からドメイン用パラメータ成長設定一覧を構築します。
        /// </summary>
        /// <returns> ドメイン用パラメータ成長設定一覧です。 </returns>
        private SkillEffectParameterGrowth[] BuildEffectParameterGrowths()
        {
            if (_effectParameterGrowths == null || _effectParameterGrowths.Length == 0)
            {
                return Array.Empty<SkillEffectParameterGrowth>();
            }

            SkillEffectParameterGrowth[] result = new SkillEffectParameterGrowth[_effectParameterGrowths.Length];
            for (int i = 0; i < _effectParameterGrowths.Length; i++)
            {
                result[i] = _effectParameterGrowths[i].ToDomain();
            }

            return result;
        }

        /// <summary>
        ///     Inspector に設定された表示情報を検証します。
        /// </summary>
        private void OnValidate()
        {
            ValidateDisplaySettings();
            ValidateGrowthSettings();
        }

        /// <summary>
        ///     表示名、説明テンプレート、数値パラメータの整合性を検証します。
        /// </summary>
        private void ValidateDisplaySettings()
        {
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                Debug.LogWarning($"[{nameof(SkillTemplateAsset)}] 表示名が設定されていません。", this);
            }

            if (_effectDisplayMode == SkillEffectDisplayMode.ComboOnly)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_skillDetail))
            {
                Debug.LogWarning($"[{nameof(SkillTemplateAsset)}] 効果説明テンプレートが設定されていません。", this);
                return;
            }

            HashSet<SkillEffectParameterId> ids = new();
            SkillEffectParameterSetting[] parameters = _effectParameters ?? Array.Empty<SkillEffectParameterSetting>();
            for (int i = 0; i < parameters.Length; i++)
            {
                SkillEffectParameterSetting parameter = parameters[i];
                if (!ids.Add(parameter.Id))
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] 効果パラメータが重複しています。ParameterId: {parameter.Id}",
                        this);
                }

                if (double.IsNaN(parameter.Value) || double.IsInfinity(parameter.Value))
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] 効果パラメータには有限値を指定してください。ParameterId: {parameter.Id}",
                        this);
                }

                if ((parameter.DisplayFormat == SkillEffectValueFormat.Integer ||
                     parameter.DisplayFormat == SkillEffectValueFormat.Count) &&
                    parameter.Value != Math.Truncate(parameter.Value))
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] 整数表示のパラメータに小数が設定されています。ParameterId: {parameter.Id}",
                        this);
                }

                string placeholder = $"{{{parameter.Id}}}";
                if (!_skillDetail.Contains(placeholder))
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] 説明文で使用されていないパラメータです。ParameterId: {parameter.Id}",
                        this);
                }
            }

            MatchCollection placeholderMatches = Regex.Matches(
                _skillDetail,
                EFFECT_PARAMETER_PLACEHOLDER_PATTERN);
            for (int i = 0; i < placeholderMatches.Count; i++)
            {
                string idText = placeholderMatches[i].Groups[PLACEHOLDER_ID_GROUP].Value;
                if (!Enum.TryParse(idText, out SkillEffectParameterId id) || !ids.Contains(id))
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] プレースホルダーに対応するパラメータがありません。ParameterId: {idText}",
                        this);
                }
            }
#endif
        }

        /// <summary>
        ///     効果パラメータ成長設定の整合性を検証します。
        /// </summary>
        private void ValidateGrowthSettings()
        {
#if UNITY_EDITOR
            SkillEffectParameterSetting[] parameters = _effectParameters ?? Array.Empty<SkillEffectParameterSetting>();
            HashSet<SkillEffectParameterId> parameterIds = new();
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterIds.Add(parameters[i].Id);
            }

            SkillEffectParameterGrowthSetting[] growths =
                _effectParameterGrowths ?? Array.Empty<SkillEffectParameterGrowthSetting>();
            HashSet<SkillEffectParameterId> growthIds = new();
            int expectedStepCount = -1;
            for (int i = 0; i < growths.Length; i++)
            {
                SkillEffectParameterGrowthSetting growth = growths[i];
                if (!growthIds.Add(growth.Id))
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] 効果パラメータ成長設定が重複しています。ParameterId: {growth.Id}",
                        this);
                }

                if (!parameterIds.Contains(growth.Id))
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] 効果パラメータに存在しないIDへ成長設定がされています。ParameterId: {growth.Id}",
                        this);
                }

                ValidateGrowthSteps(growth);

                int stepCount = growth.Steps?.Length ?? 0;
                if (stepCount == 0)
                {
                    continue;
                }

                if (expectedStepCount < 0)
                {
                    expectedStepCount = stepCount;
                }
                else if (stepCount != expectedStepCount)
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] パラメータ間でStep数が一致していません。" +
                        $"ParameterId: {growth.Id} ({stepCount}件) が他のパラメータ({expectedStepCount}件)と異なります。" +
                        "Step数は全パラメータで揃えることを推奨します。",
                        this);
                }
            }
#endif
        }

        /// <summary>
        ///     成長パラメータ1件分の成長ステップを検証します。
        /// </summary>
        /// <param name="growth"> 検証対象の成長設定です。 </param>
        private void ValidateGrowthSteps(SkillEffectParameterGrowthSetting growth)
        {
#if UNITY_EDITOR
            SkillEffectParameterGrowthStepSetting[] steps =
                growth.Steps ?? Array.Empty<SkillEffectParameterGrowthStepSetting>();
            for (int i = 0; i < steps.Length; i++)
            {
                SkillEffectParameterGrowthStepSetting step = steps[i];
                if (double.IsNaN(step.GrowthValue) || double.IsInfinity(step.GrowthValue))
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] 成長値には有限値を指定してください。" +
                        $"ParameterId: {growth.Id}, StepIndex: {i}",
                        this);
                }

                if (step.GrowthValue == 0)
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] 成長値が0のため、このステップでは値が変化しません。" +
                        $"ParameterId: {growth.Id}, StepIndex: {i}",
                        this);
                }

                if (step.GrowthType == SkillEffectParameterGrowthType.Multiplicative && step.GrowthValue <= 0)
                {
                    Debug.LogWarning(
                        $"[{nameof(SkillTemplateAsset)}] 乗算方式の成長値は0より大きい値を指定してください。" +
                        $"ParameterId: {growth.Id}, StepIndex: {i}",
                        this);
                }
            }
#endif
        }

        /// <summary>
        ///     スキル効果のパラメーター1件の設定。
        /// </summary>
        [Serializable]
        private struct SkillEffectParameterSetting
        {
            /// <summary> パラメータ識別子です。 </summary>
            public SkillEffectParameterId Id => _id;

            /// <summary> ゲーム処理で使用する値です。 </summary>
            public double Value => _value;

            /// <summary> 画面表示時の形式です。 </summary>
            public SkillEffectValueFormat DisplayFormat => _displayFormat;

            /// <summary>
            ///     ドメイン用パラメータへ変換します。
            /// </summary>
            /// <returns> ドメイン用パラメータです。 </returns>
            public SkillEffectParameter ToDomain()
            {
                return new SkillEffectParameter(_id, _value, _displayFormat);
            }

            [SerializeField, Tooltip("効果パラメータの識別子です。")]
            private SkillEffectParameterId _id;

            [SerializeField, Tooltip("ゲーム処理で使用する値です。")]
            private double _value;

            [SerializeField, Tooltip("アウトゲーム画面での表示形式です。")]
            private SkillEffectValueFormat _displayFormat;
        }

        /// <summary>
        ///     スキルレベルに応じたパラメーターの成長設定。
        /// </summary>
        [Serializable]
        private struct SkillEffectParameterGrowthSetting
        {
            /// <summary> 成長対象のパラメータ識別子です。 </summary>
            public SkillEffectParameterId Id => _id;

            /// <summary> レベル遷移ごとの成長ステップ一覧です。 </summary>
            public SkillEffectParameterGrowthStepSetting[] Steps => _steps;

            /// <summary>
            ///     ドメイン用パラメータ成長設定へ変換します。
            /// </summary>
            /// <returns> ドメイン用パラメータ成長設定です。 </returns>
            public SkillEffectParameterGrowth ToDomain()
            {
                return new SkillEffectParameterGrowth(_id, BuildSteps());
            }

            [SerializeField, Tooltip("成長対象の効果パラメータの識別子です。")]
            private SkillEffectParameterId _id;

            [SerializeField, Tooltip("レベル遷移ごとの成長ステップです。未設定の遷移には既定値(乗算×1.1)が適用されます。")]
            private SkillEffectParameterGrowthStepSetting[] _steps;

            /// <summary>
            ///     ドメイン用成長ステップ一覧を構築します。
            /// </summary>
            /// <returns> ドメイン用成長ステップ一覧です。 </returns>
            private SkillEffectParameterGrowthStep[] BuildSteps()
            {
                if (_steps == null || _steps.Length == 0)
                {
                    return Array.Empty<SkillEffectParameterGrowthStep>();
                }

                SkillEffectParameterGrowthStep[] result = new SkillEffectParameterGrowthStep[_steps.Length];
                for (int i = 0; i < _steps.Length; i++)
                {
                    result[i] = _steps[i].ToDomain();
                }

                return result;
            }
        }

        /// <summary>
        ///     パラメーターの成長1段階分の設定。
        /// </summary>
        [Serializable]
        public struct SkillEffectParameterGrowthStepSetting
        {
            /// <summary> 成長方式です。 </summary>
            public SkillEffectParameterGrowthType GrowthType => _growthType;

            /// <summary> 加算量または乗算倍率です。 </summary>
            public double GrowthValue => _growthValue;

            /// <summary>
            ///     ドメイン用成長ステップへ変換します。
            /// </summary>
            /// <returns> ドメイン用成長ステップです。 </returns>
            public SkillEffectParameterGrowthStep ToDomain()
            {
                return new SkillEffectParameterGrowthStep(_growthType, _growthValue);
            }

            [SerializeField, Tooltip("成長方式です(加算/乗算)。")]
            private SkillEffectParameterGrowthType _growthType;

            [SerializeField, Tooltip(
                "この1回分のレベルアップでの加算量(加算方式)または乗算倍率(乗算方式)です。" +
                "新規追加時に既定値が反映されない場合は、乗算なら1.1、加算なら25を目安に設定してください。")]
            private double _growthValue;
        }
    }
}
