using KillChord.Runtime.Adaptor.InGame.Mission;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     説明ポップアップの表示に合わせて、スマートフォンのタップ攻撃入力を切り替えるデコレータです。
    /// </summary>
    public sealed class MobileTapAttackPopupViewDecorator : IMissionStepPopupView
    {
        /// <summary>
        ///     ポップアップViewとタップ攻撃入力を結合します。
        /// </summary>
        /// <param name="inner"> 実際にポップアップを表示するViewです。 </param>
        /// <param name="tapAttackInput"> ポップアップ表示中に有効化するタップ攻撃入力です。 </param>
        public MobileTapAttackPopupViewDecorator(
            IMissionStepPopupView inner,
            MobileTapAttackInput tapAttackInput)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _tapAttackInput = tapAttackInput != null
                ? tapAttackInput
                : throw new ArgumentNullException(nameof(tapAttackInput));
        }

        /// <inheritdoc />
        public void Show(string imageEntryKey, Sprite fallbackImage)
        {
            _inner.Show(imageEntryKey, fallbackImage);

            // 画像キーもフォールバック画像も無いステップでは何も表示されないため、タップ攻撃も有効にしない。
            if (!string.IsNullOrWhiteSpace(imageEntryKey) || fallbackImage != null)
            {
                _tapAttackInput.Activate();
            }
            else
            {
                _tapAttackInput.Deactivate();
            }
        }

        /// <inheritdoc />
        public void Hide()
        {
            _inner.Hide();
            _tapAttackInput.Deactivate();
        }

        private readonly IMissionStepPopupView _inner;
        private readonly MobileTapAttackInput _tapAttackInput;
    }
}
