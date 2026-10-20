using KillChord.Runtime.Adaptor.InGame.Mission;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     説明ポップアップの表示に合わせて、スマートフォンの画面タップ入力を切り替えるデコレータです。
    /// </summary>
    public sealed class MobileTapPopupViewDecorator : IMissionStepPopupView
    {
        /// <summary>
        ///     ポップアップViewとタップ入力を結合します。
        /// </summary>
        /// <param name="inner"> 実際にポップアップを表示するViewです。 </param>
        /// <param name="tapInput"> ポップアップ表示中に有効化するタップ入力です。 </param>
        public MobileTapPopupViewDecorator(
            IMissionStepPopupView inner,
            MobileTapPopupConfirmInput tapInput)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _tapInput = tapInput != null
                ? tapInput
                : throw new ArgumentNullException(nameof(tapInput));
        }

        /// <inheritdoc />
        public void Show(string imageEntryKey, Sprite fallbackImage)
        {
            _inner.Show(imageEntryKey, fallbackImage);

            // 画像キーもフォールバック画像も無いステップでは何も表示されないため、タップも有効にしない。
            if (!string.IsNullOrWhiteSpace(imageEntryKey) || fallbackImage != null)
            {
                _tapInput.Activate();
            }
            else
            {
                _tapInput.Deactivate();
            }
        }

        /// <inheritdoc />
        public void Hide()
        {
            _inner.Hide();
            _tapInput.Deactivate();
        }

        private readonly IMissionStepPopupView _inner;
        private readonly MobileTapPopupConfirmInput _tapInput;
    }
}
