using KillChord.Runtime.View.Persistent.Input;
using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace KillChord.Runtime.View.Persistent.Localization
{
    /// <summary>
    ///     入力機器の種類をLocalizationのグローバル変数へ反映します。
    ///     Smart Stringから <c>{input.device:choose(keyboard|xbox|playstation|switch):...}</c> で参照します。
    /// </summary>
    public sealed class InputDeviceLocalizationVariable : IDisposable
    {
        /// <summary>
        ///     入力機器の変化を購読し、Localizationの初期化後に現在の種類を反映します。
        /// </summary>
        /// <param name="observer"> 入力機器の種類の監視元です。 </param>
        /// <exception cref="ArgumentNullException"> 監視元がnullの場合に発生します。 </exception>
        public InputDeviceLocalizationVariable(InputDeviceKindObserver observer)
        {
            _observer = observer ?? throw new ArgumentNullException(nameof(observer));
            _observer.OnKindChanged += HandleKindChanged;
            LocalizationInitializer.RunWhenInitialized(HandleLocalizationInitialized);
        }

        /// <summary>
        ///     入力機器の変化の購読を解除します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _observer.OnKindChanged -= HandleKindChanged;
            _variable = null;
            _isDisposed = true;
        }

        private const string GROUP_NAME = "input";
        private const string VARIABLE_NAME = "device";

        private readonly InputDeviceKindObserver _observer;
        private StringVariable _variable;
        private bool _isDisposed;

        /// <summary>
        ///     グローバル変数を取得し、現在の入力機器の種類を反映します。
        /// </summary>
        /// <param name="isReady"> Localeを取得できたかです。 </param>
        private void HandleLocalizationInitialized(bool isReady)
        {
            if (_isDisposed || !isReady)
            {
                return;
            }

            if (!TryFindVariable(out _variable))
            {
                Debug.LogError($"[{nameof(InputDeviceLocalizationVariable)}] グローバル変数 {GROUP_NAME}.{VARIABLE_NAME} が見つかりませんでした。");
                return;
            }

            Apply(_observer.CurrentKind);
        }

        /// <summary>
        ///     入力機器の種類の変化をグローバル変数へ反映します。
        /// </summary>
        /// <param name="kind"> 変化後の種類です。 </param>
        private void HandleKindChanged(InputDeviceKind kind)
        {
            Apply(kind);
        }

        /// <summary>
        ///     値が変わるときだけグローバル変数を更新し、参照する文字列の再生成を抑えます。
        /// </summary>
        /// <param name="kind"> 反映する種類です。 </param>
        private void Apply(InputDeviceKind kind)
        {
            if (_variable == null)
            {
                return;
            }

            string value = ToVariableValue(kind);
            if (_variable.Value != value)
            {
                _variable.Value = value;
            }
        }

        /// <summary>
        ///     Localization設定に登録されたグローバル変数を取得します。
        /// </summary>
        /// <param name="variable"> 取得した変数です。 </param>
        /// <returns> 取得できた場合はtrueです。 </returns>
        private static bool TryFindVariable(out StringVariable variable)
        {
            variable = null;
            PersistentVariablesSource source =
                LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
            if (source == null
                || !source.TryGetValue(GROUP_NAME, out VariablesGroupAsset group)
                || !group.TryGetValue(VARIABLE_NAME, out IVariable found))
            {
                return false;
            }

            variable = found as StringVariable;
            return variable != null;
        }

        /// <summary>
        ///     種類をSmart Stringのchooseで使う値へ変換します。
        /// </summary>
        /// <param name="kind"> 変換する種類です。 </param>
        /// <returns> グローバル変数に入れる値です。 </returns>
        private static string ToVariableValue(InputDeviceKind kind)
        {
            return kind switch
            {
                InputDeviceKind.Keyboard => "keyboard",
                InputDeviceKind.PlayStation => "playstation",
                InputDeviceKind.Switch => "switch",
                _ => "xbox",
            };
        }
    }
}
