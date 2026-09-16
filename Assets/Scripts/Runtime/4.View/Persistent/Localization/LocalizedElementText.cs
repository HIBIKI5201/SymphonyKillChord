using System;
using UnityEngine.Localization;

namespace KillChord.Runtime.View.Persistent.Localization
{
    /// <summary>
    ///     UI要素のテキストをローカライズされた文字列に連携します。
    /// </summary>
    public sealed class LocalizedElementText : IDisposable
    {
        /// <summary>
        ///     フォールバックを表示し、ローカライズ文字列の変更通知を購読します。
        /// </summary>
        /// <param name="table"> 参照するString Table Collection名です。 </param>
        /// <param name="entry"> 参照するエントリキーです。 </param>
        /// <param name="applyText"> 表示テキストを適用する処理です。 </param>
        /// <param name="fallback"> ローカライズ文字列を使用できない場合の表示テキストです。 </param>
        /// <param name="arguments"> Smart Stringへ渡す引数です。 </param>
        /// <exception cref="ArgumentException"> テーブル名またはエントリキーが空の場合に発生します。 </exception>
        /// <exception cref="ArgumentNullException"> テキスト適用処理がnullの場合に発生します。 </exception>
        public LocalizedElementText(
            string table,
            string entry,
            Action<string> applyText,
            string fallback = null,
            object[] arguments = null)
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentException("テーブル名を指定してください。", nameof(table));
            }

            if (string.IsNullOrEmpty(entry))
            {
                throw new ArgumentException("エントリキーを指定してください。", nameof(entry));
            }

            _applyText = applyText ?? throw new ArgumentNullException(nameof(applyText));
            _fallback = fallback ?? string.Empty;
            _applyText(_fallback);

            _localizedString = new LocalizedString(table, entry);
            if (arguments != null)
            {
                _localizedString.Arguments = arguments;
            }

            _localizedString.StringChanged += HandleStringChanged;
        }

        /// <summary>
        ///     ローカライズ文字列の変更通知を解除します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _localizedString.StringChanged -= HandleStringChanged;
            _isDisposed = true;
        }

        private readonly Action<string> _applyText;
        private readonly string _fallback;
        private readonly LocalizedString _localizedString;
        private bool _isDisposed;

        /// <summary>
        ///     選択中ロケールのテキストを適用します。
        /// </summary>
        /// <param name="localizedText"> ローカライズされたテキストです。 </param>
        private void HandleStringChanged(string localizedText)
        {
            _applyText(string.IsNullOrWhiteSpace(localizedText) ? _fallback : localizedText);
        }
    }
}
