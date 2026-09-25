using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KillChord.Runtime.View.Persistent.Localization
{
    /// <summary>
    ///     String Tableを通らない表示文の <c>{input.attack}</c> などを、
    ///     UICommonの <c>ui.input.glyph.attack</c> などの現在の入力機器のアイコンへ展開します。
    ///     入力機器が変わったときは、表示し直せるように通知します。
    /// </summary>
    public sealed class InputGlyphTextFormatter : IDisposable
    {
        /// <summary>
        ///     Localizationの初期化後に入力機器の種類の変化を購読します。
        /// </summary>
        /// <param name="onGlyphChanged"> 入力機器が変わり、アイコンを表示し直す必要があるときの処理です。 </param>
        /// <exception cref="ArgumentNullException"> 通知先がnullの場合に発生します。 </exception>
        public InputGlyphTextFormatter(Action onGlyphChanged)
        {
            _onGlyphChanged = onGlyphChanged ?? throw new ArgumentNullException(nameof(onGlyphChanged));
            LocalizationInitializer.RunWhenInitialized(HandleLocalizationInitialized);
        }

        /// <summary>
        ///     表示文の <c>{input.キー}</c> を入力アイコンへ展開します。
        ///     Localizationの初期化前は、元の文をそのまま返します。
        /// </summary>
        /// <param name="text"> 展開する表示文です。 </param>
        /// <returns> 展開後の表示文です。 </returns>
        public string Format(string text)
        {
            // 変数を含まない文とLocalization初期化前は、置換を行わない。
            if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0 || _deviceVariable == null)
            {
                return text;
            }

            return GLYPH_PATTERN.Replace(text, ResolveGlyph);
        }

        /// <summary>
        ///     入力機器の種類の変化の購読を解除します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_deviceVariable != null)
            {
                _deviceVariable.ValueChanged -= HandleDeviceChanged;
                _deviceVariable = null;
            }

            _isDisposed = true;
        }

        private const string TABLE_NAME = "UICommon";
        private const string GLYPH_ENTRY_PREFIX = "ui.input.glyph.";
        private static readonly Regex GLYPH_PATTERN = new(@"\{input\.([a-z_]+)\}", RegexOptions.Compiled);

        private readonly Action _onGlyphChanged;
        private StringVariable _deviceVariable;
        private bool _isDisposed;

        /// <summary>
        ///     入力機器の種類の変数を購読し、展開できるようになったことを通知します。
        /// </summary>
        /// <param name="isReady"> Localeを取得できたかです。 </param>
        private void HandleLocalizationInitialized(bool isReady)
        {
            if (_isDisposed || !isReady)
            {
                return;
            }

            if (!InputDeviceLocalizationVariable.TryFindDeviceVariable(out _deviceVariable))
            {
                Debug.LogError($"[{nameof(InputGlyphTextFormatter)}] 入力機器の種類のグローバル変数が見つかりませんでした。");
                return;
            }

            _deviceVariable.ValueChanged += HandleDeviceChanged;
            _onGlyphChanged();
        }

        /// <summary>
        ///     1つの変数を、現在の入力機器のアイコンへ置き換えます。
        ///     テーブルの読み込みが終わっていない場合は空にし、完了後に表示し直すよう通知します。
        /// </summary>
        /// <param name="match"> 変数の一致結果です。 </param>
        /// <returns> 置き換え後の文字列です。 </returns>
        private string ResolveGlyph(Match match)
        {
            AsyncOperationHandle<string> operation = LocalizationSettings.StringDatabase
                .GetLocalizedStringAsync(TABLE_NAME, GLYPH_ENTRY_PREFIX + match.Groups[1].Value);
            if (operation.IsDone)
            {
                return operation.Result ?? string.Empty;
            }

            operation.Completed += HandleGlyphLoaded;
            return string.Empty;
        }

        /// <summary>
        ///     アイコンの読み込み完了後に表示し直すよう通知します。
        /// </summary>
        /// <param name="operation"> 完了した読み込みです。 </param>
        private void HandleGlyphLoaded(AsyncOperationHandle<string> operation)
        {
            if (!_isDisposed)
            {
                _onGlyphChanged();
            }
        }

        /// <summary>
        ///     入力機器が変わったことを通知します。
        /// </summary>
        /// <param name="variable"> 変化した変数です。 </param>
        private void HandleDeviceChanged(IVariable variable)
        {
            _onGlyphChanged();
        }
    }
}
