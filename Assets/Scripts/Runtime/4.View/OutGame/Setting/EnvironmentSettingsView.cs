using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.View.OutGame.Common;
using R3;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     Home設定画面の環境設定項目を管理するView。
    /// </summary>
    public sealed class EnvironmentSettingsView : IDisposable
    {
        /// <summary>
        ///     UI要素を取得し、環境設定へバインドする。
        /// </summary>
        public EnvironmentSettingsView(
            VisualElement rootElement,
            IEnvironmentSettingsViewModel environmentSettingsViewModel,
            IEnvironmentSettingsCommand environmentSettingsCommand)
        {
            _environmentSettingsViewModel = environmentSettingsViewModel
                ?? throw new ArgumentNullException(nameof(environmentSettingsViewModel));
            _environmentSettingsCommand = environmentSettingsCommand
                ?? throw new ArgumentNullException(nameof(environmentSettingsCommand));
            _screenModePrevButton = Require<Button>(rootElement, SCREEN_MODE_PREV_BUTTON_NAME);
            _screenModeNextButton = Require<Button>(rootElement, SCREEN_MODE_NEXT_BUTTON_NAME);
            _screenModeValueLabel = Require<Label>(rootElement, SCREEN_MODE_VALUE_LABEL_NAME);
            _resolutionPrevButton = Require<Button>(rootElement, RESOLUTION_PREV_BUTTON_NAME);
            _resolutionNextButton = Require<Button>(rootElement, RESOLUTION_NEXT_BUTTON_NAME);
            _resolutionValueLabel = Require<Label>(rootElement, RESOLUTION_VALUE_LABEL_NAME);
            _qualityLevelPrevButton = Require<Button>(rootElement, QUALITY_LEVEL_PREV_BUTTON_NAME);
            _qualityLevelNextButton = Require<Button>(rootElement, QUALITY_LEVEL_NEXT_BUTTON_NAME);
            _qualityLevelValueLabel = Require<Label>(rootElement, QUALITY_LEVEL_VALUE_LABEL_NAME);
            _brightnessSlider = Require<SliderInt>(rootElement, BRIGHTNESS_SLIDER_NAME);
            _brightnessValueLabel = Require<Label>(rootElement, BRIGHTNESS_VALUE_LABEL_NAME);
            _saveButton = Require<Button>(rootElement, SAVE_BUTTON_NAME);
            _subscriptions = new CompositeDisposable();

            RegisterCallbacks();
            SubscribeViewModel();
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public void Dispose()
        {
            _screenModePrevButtonPreset.Dispose();
            _screenModeNextButtonPreset.Dispose();
            _resolutionPrevButtonPreset.Dispose();
            _resolutionNextButtonPreset.Dispose();
            _qualityLevelPrevButtonPreset.Dispose();
            _qualityLevelNextButtonPreset.Dispose();
            _brightnessSlider.UnregisterValueChangedCallback(HandleBrightnessChanged);
            _saveButton.clicked -= HandleSaveButtonClicked;
            _subscriptions.Dispose();
        }

        /// <summary>
        ///     プレビュー中の変更を破棄する。
        ///     パネルを保存せずに離れる際(ウィンドウ外クリック/Bボタン)に呼び出される。
        /// </summary>
        public void CancelPendingChanges()
        {
            _environmentSettingsCommand.CancelChanges();
        }

        private const string SCREEN_MODE_PREV_BUTTON_NAME = "ScreenModePrevButton";
        private const string SCREEN_MODE_NEXT_BUTTON_NAME = "ScreenModeNextButton";
        private const string SCREEN_MODE_VALUE_LABEL_NAME = "ScreenModeValueLabel";
        private const string RESOLUTION_PREV_BUTTON_NAME = "ResolutionPrevButton";
        private const string RESOLUTION_NEXT_BUTTON_NAME = "ResolutionNextButton";
        private const string RESOLUTION_VALUE_LABEL_NAME = "ResolutionValueLabel";
        private const string QUALITY_LEVEL_PREV_BUTTON_NAME = "QualityLevelPrevButton";
        private const string QUALITY_LEVEL_NEXT_BUTTON_NAME = "QualityLevelNextButton";
        private const string QUALITY_LEVEL_VALUE_LABEL_NAME = "QualityLevelValueLabel";
        private const string BRIGHTNESS_SLIDER_NAME = "BrightnessSlider";
        private const string BRIGHTNESS_VALUE_LABEL_NAME = "BrightnessValueLabel";
        private const string SAVE_BUTTON_NAME = "EnvironmentPanelSaveButton";
        private const int CYCLE_PREVIOUS_DIRECTION = -1;
        private const int CYCLE_NEXT_DIRECTION = 1;

        private readonly IEnvironmentSettingsViewModel _environmentSettingsViewModel;
        private readonly IEnvironmentSettingsCommand _environmentSettingsCommand;
        private readonly Button _screenModePrevButton;
        private readonly Button _screenModeNextButton;
        private readonly Label _screenModeValueLabel;
        private readonly Button _resolutionPrevButton;
        private readonly Button _resolutionNextButton;
        private readonly Label _resolutionValueLabel;
        private readonly Button _qualityLevelPrevButton;
        private readonly Button _qualityLevelNextButton;
        private readonly Label _qualityLevelValueLabel;
        private readonly SliderInt _brightnessSlider;
        private readonly Label _brightnessValueLabel;
        private readonly Button _saveButton;
        private readonly CompositeDisposable _subscriptions;
        private IDisposable _screenModePrevButtonPreset;
        private IDisposable _screenModeNextButtonPreset;
        private IDisposable _resolutionPrevButtonPreset;
        private IDisposable _resolutionNextButtonPreset;
        private IDisposable _qualityLevelPrevButtonPreset;
        private IDisposable _qualityLevelNextButtonPreset;

        /// <summary>
        ///     UIのコールバックを登録する。
        /// </summary>
        private void RegisterCallbacks()
        {
            _screenModePrevButtonPreset = _screenModePrevButton.ApplyBasicButtonPreset(HandleScreenModePrevButtonClicked);
            _screenModeNextButtonPreset = _screenModeNextButton.ApplyBasicButtonPreset(HandleScreenModeNextButtonClicked);
            _resolutionPrevButtonPreset = _resolutionPrevButton.ApplyBasicButtonPreset(HandleResolutionPrevButtonClicked);
            _resolutionNextButtonPreset = _resolutionNextButton.ApplyBasicButtonPreset(HandleResolutionNextButtonClicked);
            _qualityLevelPrevButtonPreset = _qualityLevelPrevButton.ApplyBasicButtonPreset(HandleQualityLevelPrevButtonClicked);
            _qualityLevelNextButtonPreset = _qualityLevelNextButton.ApplyBasicButtonPreset(HandleQualityLevelNextButtonClicked);
            _brightnessSlider.RegisterValueChangedCallback(HandleBrightnessChanged);
            _saveButton.clicked += HandleSaveButtonClicked;
        }

        /// <summary>
        ///     環境設定の変更を購読する。
        /// </summary>
        private void SubscribeViewModel()
        {
            _environmentSettingsViewModel.ScreenModeLabel
                .Subscribe(HandleScreenModeLabelPublished)
                .AddTo(_subscriptions);
            _environmentSettingsViewModel.ResolutionLabel
                .Subscribe(HandleResolutionLabelPublished)
                .AddTo(_subscriptions);
            _environmentSettingsViewModel.QualityLevelLabel
                .Subscribe(HandleQualityLevelLabelPublished)
                .AddTo(_subscriptions);
            _environmentSettingsViewModel.Brightness
                .Subscribe(HandleBrightnessPublished)
                .AddTo(_subscriptions);
        }

        /// <summary>
        ///     画面モードを前へ切り替える。
        /// </summary>
        private void HandleScreenModePrevButtonClicked()
        {
            _environmentSettingsCommand.ToggleScreenMode();
        }

        /// <summary>
        ///     画面モードを次へ切り替える。
        /// </summary>
        private void HandleScreenModeNextButtonClicked()
        {
            _environmentSettingsCommand.ToggleScreenMode();
        }

        /// <summary>
        ///     解像度を前へ切り替える。
        /// </summary>
        private void HandleResolutionPrevButtonClicked()
        {
            _environmentSettingsCommand.CycleResolution(CYCLE_PREVIOUS_DIRECTION);
        }

        /// <summary>
        ///     解像度を次へ切り替える。
        /// </summary>
        private void HandleResolutionNextButtonClicked()
        {
            _environmentSettingsCommand.CycleResolution(CYCLE_NEXT_DIRECTION);
        }

        /// <summary>
        ///     画質プリセットを前へ切り替える。
        /// </summary>
        private void HandleQualityLevelPrevButtonClicked()
        {
            _environmentSettingsCommand.CycleQualityLevel(CYCLE_PREVIOUS_DIRECTION);
        }

        /// <summary>
        ///     画質プリセットを次へ切り替える。
        /// </summary>
        private void HandleQualityLevelNextButtonClicked()
        {
            _environmentSettingsCommand.CycleQualityLevel(CYCLE_NEXT_DIRECTION);
        }

        /// <summary>
        ///     明るさゲージの変更を環境設定へ渡す。
        /// </summary>
        private void HandleBrightnessChanged(ChangeEvent<int> changeEvent)
        {
            _environmentSettingsCommand.SetBrightness(changeEvent.newValue);
        }

        /// <summary>
        ///     プレビュー中の変更を保存として確定する。
        /// </summary>
        private void HandleSaveButtonClicked()
        {
            _environmentSettingsCommand.ConfirmChanges();
        }

        /// <summary>
        ///     画面モードを表示へ反映する。
        /// </summary>
        private void HandleScreenModeLabelPublished(string label)
        {
            _screenModeValueLabel.text = label;
        }

        /// <summary>
        ///     解像度を表示へ反映する。
        /// </summary>
        private void HandleResolutionLabelPublished(string label)
        {
            _resolutionValueLabel.text = label;
        }

        /// <summary>
        ///     画質プリセットを表示へ反映する。
        /// </summary>
        private void HandleQualityLevelLabelPublished(string label)
        {
            _qualityLevelValueLabel.text = label;
        }

        /// <summary>
        ///     明るさをゲージと数値表示へ反映する。
        /// </summary>
        private void HandleBrightnessPublished(int brightness)
        {
            _brightnessSlider.SetValueWithoutNotify(brightness);
            _brightnessValueLabel.text = brightness.ToString();
        }

        /// <summary>
        ///     必須UI要素を取得する。
        /// </summary>
        private static T Require<T>(VisualElement rootElement, string elementName)
            where T : VisualElement
        {
            return rootElement.Q<T>(elementName)
                ?? throw new InvalidOperationException(
                    $"[{nameof(EnvironmentSettingsView)}] {elementName} が見つかりませんでした。");
        }
    }
}
