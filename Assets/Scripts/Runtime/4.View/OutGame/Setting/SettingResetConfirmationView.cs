using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.Persistent.Localization;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     環境設定・操作設定パネルの「既定値に戻す」ボタンと確認ダイアログを管理するView。
    ///     確定すると表示中のパネルの項目だけを既定値へ戻す。保存は各パネルの「設定を保存」で行う。
    /// </summary>
    public sealed class SettingResetConfirmationView : IDisposable
    {
        /// <summary>
        ///     ボタンと確認ダイアログのUI要素を取得し、操作を登録する。
        /// </summary>
        public SettingResetConfirmationView(
            VisualElement rootElement,
            IEnvironmentSettingsCommand environmentSettingsCommand)
        {
            // コマンドと画面の各要素を取得する。
            rootElement = rootElement
                ?? throw new ArgumentNullException(nameof(rootElement));
            _environmentSettingsCommand = environmentSettingsCommand
                ?? throw new ArgumentNullException(nameof(environmentSettingsCommand));
            _environmentResetButton = Require<Button>(rootElement, ENVIRONMENT_RESET_BUTTON_NAME);
            _controlResetButton = Require<Button>(rootElement, CONTROL_RESET_BUTTON_NAME);
            _dialog = Require<VisualElement>(rootElement, DIALOG_NAME);
            _messageLabel = Require<Label>(rootElement, MESSAGE_LABEL_NAME);
            _confirmButton = Require<Button>(rootElement, CONFIRM_BUTTON_NAME);
            _cancelButton = Require<Button>(rootElement, CANCEL_BUTTON_NAME);

            // 操作を登録し、ダイアログを閉じた状態にする。
            RegisterCallbacks();
            Hide();

            // ボタンと確認文の文言をローカライズに登録する。
            _localizedTexts = new[]
            {
                new LocalizedElementText(
                    UI_COMMON_TABLE, RESET_BUTTON_ENTRY, text => _environmentResetButton.text = text, "既定値に戻す"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, RESET_BUTTON_ENTRY, text => _controlResetButton.text = text, "既定値に戻す"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, MESSAGE_ENTRY, text => _messageLabel.text = text, "設定を既定値に戻しますか？"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, CONFIRM_ENTRY, text => _confirmButton.text = text, "既定値に戻す"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, CANCEL_ENTRY, text => _cancelButton.text = text, "キャンセル"),
            };
        }

        /// <summary>
        ///     確認ダイアログを表示中であれば閉じる。
        /// </summary>
        /// <returns> ダイアログを閉じた場合はtrue。表示していなかった場合はfalse。 </returns>
        public bool TryHide()
        {
            if (!_isVisible)
            {
                return false;
            }

            HideAndRestoreFocus();
            return true;
        }

        /// <summary>
        ///     フォーカスを戻さずに確認ダイアログを閉じた状態へ初期化する。
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            _dialogNavigationScope.Deactivate();
            _dialog.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public void Dispose()
        {
            _dialogNavigationScope.Deactivate();
            _environmentResetButtonPreset?.Dispose();
            _controlResetButtonPreset?.Dispose();
            _confirmButtonActivation?.Dispose();
            _cancelButtonActivation?.Dispose();
            foreach (LocalizedElementText localizedText in _localizedTexts)
            {
                localizedText.Dispose();
            }
        }

        private const string ENVIRONMENT_RESET_BUTTON_NAME = "EnvironmentPanelResetButton";
        private const string CONTROL_RESET_BUTTON_NAME = "ControlPanelResetButton";
        private const string DIALOG_NAME = "ResetToDefaultsDialog";
        private const string MESSAGE_LABEL_NAME = "ResetToDefaultsMessage";
        private const string CONFIRM_BUTTON_NAME = "ConfirmResetToDefaultsButton";
        private const string CANCEL_BUTTON_NAME = "CancelResetToDefaultsButton";
        private const string UI_COMMON_TABLE = "UICommon";
        private const string RESET_BUTTON_ENTRY = "ui.setting.reset_to_defaults";
        private const string MESSAGE_ENTRY = "ui.setting.reset_to_defaults_message";
        private const string CONFIRM_ENTRY = "ui.setting.confirm_reset_to_defaults";
        private const string CANCEL_ENTRY = "ui.setting.cancel_reset_to_defaults";

        /// <summary> 確認ダイアログ表示中、フォーカスをダイアログ内へ閉じ込める。 </summary>
        private readonly ModalNavigationScope _dialogNavigationScope = new();

        private readonly IEnvironmentSettingsCommand _environmentSettingsCommand;
        private readonly Button _environmentResetButton;
        private readonly Button _controlResetButton;
        private readonly VisualElement _dialog;
        private readonly Label _messageLabel;
        private readonly Button _confirmButton;
        private readonly Button _cancelButton;
        private readonly LocalizedElementText[] _localizedTexts;
        private IDisposable _environmentResetButtonPreset;
        private IDisposable _controlResetButtonPreset;
        private IDisposable _confirmButtonActivation;
        private IDisposable _cancelButtonActivation;
        private ResetTarget _pendingTarget;
        private bool _isVisible;

        /// <summary>
        ///     環境設定パネルの項目を戻す確認ダイアログを表示する。
        /// </summary>
        private void HandleEnvironmentResetButtonActivated()
        {
            Show(ResetTarget.Environment);
        }

        /// <summary>
        ///     操作設定パネルの項目を戻す確認ダイアログを表示する。
        /// </summary>
        private void HandleControlResetButtonActivated()
        {
            Show(ResetTarget.Control);
        }

        /// <summary>
        ///     確認ダイアログで確定され、対象パネルの項目を既定値へ戻す。
        /// </summary>
        private void HandleConfirmButtonActivated()
        {
            if (!_isVisible)
            {
                return;
            }

            // 戻すのはプレビューだけで、保存は「設定を保存」で行う。
            if (_pendingTarget == ResetTarget.Environment)
            {
                _environmentSettingsCommand.ResetEnvironmentToDefaults();
            }
            else
            {
                _environmentSettingsCommand.ResetControlToDefaults();
            }

            HideAndRestoreFocus();
        }

        /// <summary>
        ///     確認ダイアログをキャンセルして閉じる。
        /// </summary>
        private void HandleCancelButtonActivated()
        {
            TryHide();
        }

        /// <summary>
        ///     ボタンと確認ダイアログのコールバックを登録する。
        /// </summary>
        private void RegisterCallbacks()
        {
            _environmentResetButtonPreset = _environmentResetButton.ApplyBasicButtonPreset(HandleEnvironmentResetButtonActivated);
            _controlResetButtonPreset = _controlResetButton.ApplyBasicButtonPreset(HandleControlResetButtonActivated);

            // ダイアログのボタンはタイトル復帰の確認と同じく、クリックと決定操作を1つの処理へまとめる。
            _confirmButton.MakeNavigable();
            _cancelButton.MakeNavigable();
            _confirmButtonActivation = _confirmButton.RegisterActivation(HandleConfirmButtonActivated);
            _cancelButtonActivation = _cancelButton.RegisterActivation(HandleCancelButtonActivated);
        }

        /// <summary>
        ///     指定したパネルを戻す確認ダイアログを表示する。
        /// </summary>
        private void Show(ResetTarget target)
        {
            if (_isVisible)
            {
                return;
            }

            _pendingTarget = target;
            _isVisible = true;
            _dialog.style.display = DisplayStyle.Flex;

            // 背面の設定項目へフォーカスが抜けないようにする。
            _dialogNavigationScope.Activate(_dialog);

            // 誤操作で戻さないよう、キャンセルボタンを既定のフォーカス先にする。
            _cancelButton.FocusDeferred();
        }

        /// <summary>
        ///     確認ダイアログを閉じ、押されたリセットボタンへフォーカスを戻す。
        /// </summary>
        private void HideAndRestoreFocus()
        {
            Hide();
            Button resetButton = _pendingTarget == ResetTarget.Environment
                ? _environmentResetButton
                : _controlResetButton;
            resetButton.Focus();
        }

        /// <summary>
        ///     必須UI要素を取得する。
        /// </summary>
        private static T Require<T>(VisualElement rootElement, string elementName)
            where T : VisualElement
        {
            return rootElement.Q<T>(elementName)
                ?? throw new InvalidOperationException(
                    $"[{nameof(SettingResetConfirmationView)}] {elementName} が見つかりませんでした。");
        }

        /// <summary>
        ///     既定値へ戻す対象のパネルです。
        /// </summary>
        private enum ResetTarget
        {
            Environment,
            Control,
        }
    }
}
