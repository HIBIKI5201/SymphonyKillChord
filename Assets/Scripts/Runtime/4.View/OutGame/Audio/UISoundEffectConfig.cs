using KillChord.Runtime.Adaptor.OutGame.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Audio
{
    /// <summary>
    ///     UI Toolkit操作に対応するUI操作音のCue設定を保持する。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(UISoundEffectConfig),
        menuName = "KillChord/OutGame/Audio/UI Sound Effect Config")]
    public sealed class UISoundEffectConfig : ScriptableObject
    {
        /// <summary> Buttonに使用する既定のClick Cue。 </summary>
        public string DefaultButtonActivationCue => _defaultButtonActivationCue;

        /// <summary>
        ///     UI操作の意味に対応するCueを取得する。
        /// </summary>
        /// <param name="kind"> UI操作音の種類。 </param>
        /// <param name="cueName"> 解決したCue名。 </param>
        /// <returns> Cueを解決できた場合はtrue。 </returns>
        public bool TryGetCue(UISoundEffectKind kind, out string cueName)
        {
            string ussClassName = kind switch
            {
                UISoundEffectKind.Select => SELECT_USS_CLASS_NAME,
                UISoundEffectKind.SkillSet => SKILL_SET_USS_CLASS_NAME,
                _ => string.Empty,
            };

            return TryFindCueByUssClass(_clickMappings, ussClassName, out cueName);
        }

        private const string USS_CLASS_PREFIX = "ui-se-";

        // UISoundEffectKindからCueを解決するUSSクラス名は、既定対応との不整合を防ぐため定数化する。
        private const string SELECT_USS_CLASS_NAME = "ui-se-select";
        private const string SKILL_SET_USS_CLASS_NAME = "ui-se-skill-set";

        // Validate時の列挙値配列生成を防ぐため、型初期化時に一度だけ取得する。
        private static readonly UISoundEffectKind[] UI_SOUND_EFFECT_KINDS =
            (UISoundEffectKind[])Enum.GetValues(typeof(UISoundEffectKind));

        [SerializeField, Tooltip("明示的な音用USSクラスがないButtonに使用するClick Cue。")]
        private string _defaultButtonActivationCue = "SE_Click";

        [SerializeField, Tooltip("ClickEvent用のUSSクラスとCue名の対応。")]
        private List<SoundMapping> _clickMappings = new()
        {
            new(SELECT_USS_CLASS_NAME, "SE_Select"),
            new("ui-se-cancel", "SE_Cancel"),
            new("ui-se-window", "SE_Window"),
            new("ui-se-sortie", "SE_Sortie"),
            new(SKILL_SET_USS_CLASS_NAME, "SE_SkillSet")
        };

        [SerializeField, Tooltip("PointerDownEvent用のUSSクラスとCue名の対応。")]
        private List<SoundMapping> _pointerDownMappings = new()
        {
            new("ui-se-title-start", "SE_Gamestart")
        };

        /// <summary>
        ///     必須CueとUSSクラス対応が有効か確認する。
        /// </summary>
        /// <param name="errorMessage"> 検証失敗時の理由。 </param>
        /// <returns> 設定が有効な場合はtrue。 </returns>
        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(_defaultButtonActivationCue))
            {
                errorMessage = "Buttonの既定Click Cueが設定されていません。";
                return false;
            }

            if (!ValidateMappings(_clickMappings, "ClickEvent", out errorMessage))
            {
                return false;
            }

            for (int i = 0; i < UI_SOUND_EFFECT_KINDS.Length; i++)
            {
                UISoundEffectKind kind = UI_SOUND_EFFECT_KINDS[i];
                if (!TryGetCue(kind, out _))
                {
                    errorMessage = $"{kind}に対応するCueが設定されていません。";
                    return false;
                }
            }

            return ValidateMappings(_pointerDownMappings, "PointerDownEvent", out errorMessage);
        }

        /// <summary>
        ///     要素に設定されたClickEvent用のCueを解決する。
        /// </summary>
        /// <param name="element"> 調べるUI要素。 </param>
        /// <param name="cueName"> 解決したCue名。 </param>
        /// <param name="hasMultipleMatches"> 同じ要素に複数の音用クラスがある場合はtrue。 </param>
        /// <returns> Cueを1つ解決できた場合はtrue。 </returns>
        public bool TryResolveActivationCue(
            VisualElement element,
            out string cueName,
            out bool hasMultipleMatches)
        {
            return TryResolveCue(element, _clickMappings, out cueName, out hasMultipleMatches);
        }

        /// <summary>
        ///     要素に設定されたPointerDownEvent用のCueを解決する。
        /// </summary>
        /// <param name="element"> 調べるUI要素。 </param>
        /// <param name="cueName"> 解決したCue名。 </param>
        /// <param name="hasMultipleMatches"> 同じ要素に複数の音用クラスがある場合はtrue。 </param>
        /// <returns> Cueを1つ解決できた場合はtrue。 </returns>
        public bool TryResolvePointerDownActivationCue(
            VisualElement element,
            out string cueName,
            out bool hasMultipleMatches)
        {
            return TryResolveCue(element, _pointerDownMappings, out cueName, out hasMultipleMatches);
        }

        /// <summary>
        ///     指定されたイベント種別の対応を検証する。
        /// </summary>
        /// <param name="mappings"> 検証対象の対応一覧。 </param>
        /// <param name="eventName"> ログ用のイベント名。 </param>
        /// <param name="errorMessage"> 検証失敗時の理由。 </param>
        /// <returns> 対応一覧が有効な場合はtrue。 </returns>
        private static bool ValidateMappings(
            IReadOnlyList<SoundMapping> mappings,
            string eventName,
            out string errorMessage)
        {
            if (mappings == null)
            {
                errorMessage = $"{eventName}の対応一覧が設定されていません。";
                return false;
            }

            HashSet<string> registeredClassNames = new(StringComparer.Ordinal);
            for (int i = 0; i < mappings.Count; i++)
            {
                SoundMapping mapping = mappings[i];
                if (mapping == null
                    || string.IsNullOrWhiteSpace(mapping.UssClassName)
                    || string.IsNullOrWhiteSpace(mapping.CueName))
                {
                    errorMessage = $"{eventName}のUSSクラス名またはCue名が空です。";
                    return false;
                }

                if (!mapping.UssClassName.StartsWith(USS_CLASS_PREFIX, StringComparison.Ordinal))
                {
                    errorMessage = $"{eventName}の音用USSクラスは{USS_CLASS_PREFIX}で始めてください。";
                    return false;
                }

                if (!registeredClassNames.Add(mapping.UssClassName))
                {
                    errorMessage = $"{eventName}のUSSクラス{mapping.UssClassName}が重複しています。";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        ///     要素のUSSクラスからCueを1つ解決する。
        /// </summary>
        /// <param name="element"> 調べるUI要素。 </param>
        /// <param name="mappings"> 使用する対応一覧。 </param>
        /// <param name="cueName"> 解決したCue名。 </param>
        /// <param name="hasMultipleMatches"> 複数の音用クラスが一致した場合はtrue。 </param>
        /// <returns> Cueを1つ解決できた場合はtrue。 </returns>
        private static bool TryResolveCue(
            VisualElement element,
            IReadOnlyList<SoundMapping> mappings,
            out string cueName,
            out bool hasMultipleMatches)
        {
            cueName = string.Empty;
            hasMultipleMatches = false;
            if (element == null || mappings == null)
            {
                return false;
            }

            int matchCount = 0;
            for (int i = 0; i < mappings.Count; i++)
            {
                SoundMapping mapping = mappings[i];
                if (mapping == null || !element.ClassListContains(mapping.UssClassName))
                {
                    continue;
                }

                matchCount++;
                cueName = mapping.CueName;
            }

            hasMultipleMatches = matchCount > 1;
            if (hasMultipleMatches)
            {
                cueName = string.Empty;
                return false;
            }

            return matchCount == 1;
        }

        /// <summary>
        ///     USSクラス名に対応するCueを取得する。
        /// </summary>
        /// <param name="mappings"> 検索対象の対応一覧。 </param>
        /// <param name="ussClassName"> 検索するUSSクラス名。 </param>
        /// <param name="cueName"> 解決したCue名。 </param>
        /// <returns> Cueを解決できた場合はtrue。 </returns>
        private static bool TryFindCueByUssClass(
            IReadOnlyList<SoundMapping> mappings,
            string ussClassName,
            out string cueName)
        {
            cueName = string.Empty;
            if (mappings == null || string.IsNullOrWhiteSpace(ussClassName))
            {
                return false;
            }

            for (int i = 0; i < mappings.Count; i++)
            {
                SoundMapping mapping = mappings[i];
                if (mapping != null
                    && string.Equals(mapping.UssClassName, ussClassName, StringComparison.Ordinal))
                {
                    cueName = mapping.CueName;
                    return !string.IsNullOrWhiteSpace(cueName);
                }
            }

            return false;
        }

        [Serializable]
        private sealed class SoundMapping
        {
            /// <summary>
            ///     USSクラスとCue名の対応を初期化する。
            /// </summary>
            /// <param name="ussClassName"> 音の意味を表すUSSクラス名。 </param>
            /// <param name="cueName"> 再生するCue名。 </param>
            public SoundMapping(string ussClassName, string cueName)
            {
                _ussClassName = ussClassName;
                _cueName = cueName;
            }

            /// <summary> 音の意味を表すUSSクラス名。 </summary>
            public string UssClassName => _ussClassName;

            /// <summary> 再生するCue名。 </summary>
            public string CueName => _cueName;

            [SerializeField, Tooltip("音の意味を表すui-se-で始まるUSSクラス名。")]
            private string _ussClassName;

            [SerializeField, Tooltip("USSクラスに対応して再生するCue名。")]
            private string _cueName;
        }
    }
}
