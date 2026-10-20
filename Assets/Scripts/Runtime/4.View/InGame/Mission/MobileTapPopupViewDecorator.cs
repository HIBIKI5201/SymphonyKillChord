using KillChord.Runtime.Adaptor.InGame.Mission;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     説明ポップアップの表示に合わせて、スマートフォンの画面タップ入力を切り替えるデコレータです。
    ///     スマートフォンでは種類を問わず画面タップで閉じるため、閉じる案内もタップに差し替えます。
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
        public void Show(GameObject popupPrefab, MissionStepPopupCloseInput closeInput)
        {
            _inner.Show(popupPrefab, MissionStepPopupCloseInput.ScreenTap);

            // Prefabが無いステップでは何も表示されないため、タップも有効にしない。
            if (popupPrefab != null)
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
