using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.Persistent.Localization;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     環境設定・操作設定・オーディオ設定パネルの「既定値に戻す」ボタンと確認ダイアログを管理するView。
    ///     確定すると表示中のパネルの項目だけを既定値へ戻す。
    ///     環境設定・操作設定は各パネルの「設定を保存」で保存し、オーディオ設定はその場で保存する。
    /// </summary>
    public sealed class SettingResetConfirmationView : IDisposable
    {
        /// <summary>
        ///     ボタンと確認ダイアログのUI要素を取得し、操作を登録する。
        /// </summary>
        public SettingResetConfirmationView(
            VisualElement rootElement,
            IEnvironmentSettingsCommand environmentSettingsCommand,
            IAudioSettingsCommand audioSettingsCommand)
        {
            // コマンドと画面の各要素を取得する。
            rootElement = rootElement
                ?? throw new ArgumentNullException(nameof(rootElement));
            _environmentSettingsCommand = environmentSettingsCommand
                ?? throw new ArgumentNullException(nameof(environmentSettingsCommand));
            _audioSettingsCommand = audioSettingsCommand
                ?? throw new ArgumentNullException(nameof(audioSettingsCommand));
            _environmentResetButton = Require<Button>(rootElement, ENVIRONMENT_RESET_BUTTON_NAME);
            _controlResetButton = Require<Button>(rootElement, CONTROL_RESET_BUTTON_NAME);
            _audioResetButton = Require<Button>(rootElement, AUDIO_RESET_BUTTON_NAME);
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
                    UI_COMMON_TABLE, RESET_BUTTON_ENTRY, text => _audioResetButton.text = text, "既定値に戻す"),
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
            _audioResetButtonPreset?.Dispose();
            _confirmButtonActivation?.Dispose();
            _cancelButtonActivation?.Dispose();
            foreach (LocalizedElementText localizedText in _localizedTexts)
            {
                localizedText.Dispose();
            }
        }

        private const string ENVIRONMENT_RESET_BUTTON_NAME = "EnvironmentPanelResetButton";
        private const string CONTROL_RESET_BUTTON_NAME = "ControlPanelResetButton";
        private const string AUDIO_RESET_BUTTON_NAME = "SoundPanelResetButton";
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
        private readonly IAudioSettingsCommand _audioSettingsCommand;
        private readonly Button _environmentResetButton;
        private readonly Button _controlResetButton;
        private readonly Button _audioResetButton;
        private readonly VisualElement _dialog;
        private readonly Label _messageLabel;
        private readonly Button _confirmButton;
        private readonly Button _cancelButton;
        private readonly LocalizedElementText[] _localizedTexts;
        private IDisposable _environmentResetButtonPreset;
        private IDisposable _controlResetButtonPreset;
        private IDisposable _audioResetButtonPreset;
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
        ///     オーディオ設定パネルの項目を戻す確認ダイアログを表示する。
        /// </summary>
        private void HandleAudioResetButtonActivated()
        {
            Show(ResetTarget.Audio);
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

            // 環境設定・操作設定はプレビューだけ戻し、保存は「設定を保存」で行う。オーディオ設定はその場で保存する。
            switch (_pendingTarget)
            {
                case ResetTarget.Environment:
                    _environmentSettingsCommand.ResetEnvironmentToDefaults();
                    break;
                case ResetTarget.Control:
                    _environmentSettingsCommand.ResetControlToDefaults();
                    break;
                case ResetTarget.Audio:
                    _audioSettingsCommand.ResetToDefaults();
                    break;
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
            _audioResetButtonPreset = _audioResetButton.ApplyBasicButtonPreset(HandleAudioResetButtonActivated);

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
            GetResetButton(_pendingTarget).Focus();
        }

        /// <summary>
        ///     対象パネルの「既定値に戻す」ボタンを取得する。
        /// </summary>
        private Button GetResetButton(ResetTarget target)
        {
            return target switch
            {
                ResetTarget.Environment => _environmentResetButton,
                ResetTarget.Control => _controlResetButton,
                _ => _audioResetButton,
            };
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
            Audio,
        }
    }
}
