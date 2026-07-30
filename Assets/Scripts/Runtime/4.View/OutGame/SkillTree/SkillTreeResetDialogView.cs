using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリーリセットの確認ダイアログを表示する。
    /// </summary>
    public sealed class SkillTreeResetDialogView : IDisposable
    {
        /// <summary>
        ///     リセットボタンと確認ダイアログを初期化する。
        /// </summary>
        /// <param name="rootElement"> スキルツリー画面のルート。 </param>
        /// <param name="outGameUIEvent"> アウトゲームUIイベント。 </param>
        public SkillTreeResetDialogView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
        {
            if (rootElement == null)
            {
                throw new ArgumentNullException(nameof(rootElement));
            }

            _outGameUIEvent = outGameUIEvent ?? throw new ArgumentNullException(nameof(outGameUIEvent));
            _resetButton = rootElement.Q<Button>(RESET_BUTTON_NAME)
                ?? throw new InvalidOperationException($"{RESET_BUTTON_NAME} が見つかりません。");
            _dialog = rootElement.Q<VisualElement>(RESET_DIALOG_NAME)
                ?? throw new InvalidOperationException($"{RESET_DIALOG_NAME} が見つかりません。");
            _messageLabel = _dialog.Q<Label>(RESET_MESSAGE_NAME)
                ?? throw new InvalidOperationException($"{RESET_MESSAGE_NAME} が見つかりません。");
            _confirmButton = _dialog.Q<Button>(RESET_CONFIRM_BUTTON_NAME)
                ?? throw new InvalidOperationException($"{RESET_CONFIRM_BUTTON_NAME} が見つかりません。");
            _cancelButton = _dialog.Q<Button>(RESET_CANCEL_BUTTON_NAME)
                ?? throw new InvalidOperationException($"{RESET_CANCEL_BUTTON_NAME} が見つかりません。");

            _resetButton.clicked += HandleResetButtonClickedHandler;
            _confirmButton.clicked += HandleConfirmButtonClickedHandler;
            _cancelButton.clicked += HandleCancelButtonClickedHandler;
            Hide();
        }

        /// <summary>
        ///     返却ポイントを表示して確認ダイアログを開く。
        /// </summary>
        /// <param name="refundPoints"> 返却予定の研究ポイント。 </param>
        public void Show(int refundPoints)
        {
            _messageLabel.text = $"スキルツリーをリセットしますか？\n返却される研究ポイント：{refundPoints}";
            _confirmButton.SetEnabled(refundPoints > 0);
            _dialog.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        ///     確認ダイアログを閉じる。
        /// </summary>
        public void Hide()
        {
            _dialog.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     リセット処理中の入力可否を設定する。
        /// </summary>
        /// <param name="isEnabled"> 入力を許可する場合はtrue。 </param>
        public void SetInteractionEnabled(bool isEnabled)
        {
            _resetButton.SetEnabled(isEnabled);
            _confirmButton.SetEnabled(isEnabled);
            _cancelButton.SetEnabled(isEnabled);
        }

        /// <summary>
        ///     登録済みイベントを解除する。
        /// </summary>
        public void Dispose()
        {
            _resetButton.clicked -= HandleResetButtonClickedHandler;
            _confirmButton.clicked -= HandleConfirmButtonClickedHandler;
            _cancelButton.clicked -= HandleCancelButtonClickedHandler;
        }

        private const string RESET_BUTTON_NAME = "ResetButton";
        private const string RESET_DIALOG_NAME = "SkillTreeResetDialog";
        private const string RESET_MESSAGE_NAME = "ResetMessage";
        private const string RESET_CONFIRM_BUTTON_NAME = "ResetConfirmButton";
        private const string RESET_CANCEL_BUTTON_NAME = "ResetCancelButton";

        private readonly OutGameUIEvent _outGameUIEvent;
        private readonly Button _resetButton;
        private readonly VisualElement _dialog;
        private readonly Label _messageLabel;
        private readonly Button _confirmButton;
        private readonly Button _cancelButton;

        /// <summary>
        ///     リセットボタン押下を通知する。
        /// </summary>
        private void HandleResetButtonClickedHandler()
        {
            _outGameUIEvent.OnSkillTreeResetRequested?.Invoke();
        }

        /// <summary>
        ///     リセット確定を通知する。
        /// </summary>
        private void HandleConfirmButtonClickedHandler()
        {
            _outGameUIEvent.OnSkillTreeResetConfirmed?.Invoke();
        }

        /// <summary>
        ///     リセットキャンセルを通知する。
        /// </summary>
        private void HandleCancelButtonClickedHandler()
        {
            _outGameUIEvent.OnSkillTreeResetCancelled?.Invoke();
        }
    }
}
