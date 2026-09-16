using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.Persistent.Localization;
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
            _rhythmOffsetSlider = Require<SliderInt>(rootElement, RHYTHM_OFFSET_SLIDER_NAME);
            _rhythmOffsetValueLabel = Require<Label>(rootElement, RHYTHM_OFFSET_VALUE_LABEL_NAME);
            _languagePrevButton = Require<Button>(rootElement, LANGUAGE_PREV_BUTTON_NAME);
            _languageNextButton = Require<Button>(rootElement, LANGUAGE_NEXT_BUTTON_NAME);
            _languageValueLabel = Require<Label>(rootElement, LANGUAGE_VALUE_LABEL_NAME);
            _vibrationPrevButton = Require<Button>(rootElement, VIBRATION_PREV_BUTTON_NAME);
            _vibrationNextButton = Require<Button>(rootElement, VIBRATION_NEXT_BUTTON_NAME);
            _vibrationValueLabel = Require<Label>(rootElement, VIBRATION_VALUE_LABEL_NAME);
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
            _languagePrevButtonPreset.Dispose();
            _languageNextButtonPreset.Dispose();
            _vibrationPrevButtonPreset.Dispose();
            _vibrationNextButtonPreset.Dispose();
            _brightnessSlider.UnregisterValueChangedCallback(HandleBrightnessChanged);
            _rhythmOffsetSlider.UnregisterValueChangedCallback(HandleRhythmOffsetChanged);
            _saveButton.clicked -= HandleSaveButtonClicked;
            _screenModeLocalizedText?.Dispose();
            _languageLocalizedText?.Dispose();
            _vibrationLocalizedText?.Dispose();
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
        private const string RHYTHM_OFFSET_SLIDER_NAME = "RhythmOffsetSlider";
        private const string RHYTHM_OFFSET_VALUE_LABEL_NAME = "RhythmOffsetValueLabel";
        private const string LANGUAGE_PREV_BUTTON_NAME = "LanguagePrevButton";
        private const string LANGUAGE_NEXT_BUTTON_NAME = "LanguageNextButton";
        private const string LANGUAGE_VALUE_LABEL_NAME = "LanguageValueLabel";
        private const string VIBRATION_PREV_BUTTON_NAME = "VibrationPrevButton";
        private const string VIBRATION_NEXT_BUTTON_NAME = "VibrationNextButton";
        private const string VIBRATION_VALUE_LABEL_NAME = "VibrationValueLabel";
        private const string SAVE_BUTTON_NAME = "EnvironmentPanelSaveButton";
        private const string UI_COMMON_TABLE = "UICommon";
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
        private readonly SliderInt _rhythmOffsetSlider;
        private readonly Label _rhythmOffsetValueLabel;
        private readonly Button _languagePrevButton;
        private readonly Button _languageNextButton;
        private readonly Label _languageValueLabel;
        private readonly Button _vibrationPrevButton;
        private readonly Button _vibrationNextButton;
        private readonly Label _vibrationValueLabel;
        private readonly Button _saveButton;
        private readonly CompositeDisposable _subscriptions;
        private IDisposable _screenModePrevButtonPreset;
        private IDisposable _screenModeNextButtonPreset;
        private IDisposable _resolutionPrevButtonPreset;
        private IDisposable _resolutionNextButtonPreset;
        private IDisposable _qualityLevelPrevButtonPreset;
        private IDisposable _qualityLevelNextButtonPreset;
        private IDisposable _languagePrevButtonPreset;
        private IDisposable _languageNextButtonPreset;
        private IDisposable _vibrationPrevButtonPreset;
        private IDisposable _vibrationNextButtonPreset;
        private LocalizedElementText _screenModeLocalizedText;
        private LocalizedElementText _languageLocalizedText;
        private LocalizedElementText _vibrationLocalizedText;

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
            _languagePrevButtonPreset = _languagePrevButton.ApplyBasicButtonPreset(HandleLanguagePrevButtonClicked);
            _languageNextButtonPreset = _languageNextButton.ApplyBasicButtonPreset(HandleLanguageNextButtonClicked);
            _vibrationPrevButtonPreset = _vibrationPrevButton.ApplyBasicButtonPreset(HandleVibrationPrevButtonClicked);
            _vibrationNextButtonPreset = _vibrationNextButton.ApplyBasicButtonPreset(HandleVibrationNextButtonClicked);
            _brightnessSlider.RegisterValueChangedCallback(HandleBrightnessChanged);
            _rhythmOffsetSlider.RegisterValueChangedCallback(HandleRhythmOffsetChanged);
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
            _environmentSettingsViewModel.LanguageLabel
                .Subscribe(HandleLanguageLabelPublished)
                .AddTo(_subscriptions);
            _environmentSettingsViewModel.VibrationStrengthLabel
                .Subscribe(HandleVibrationStrengthLabelPublished)
                .AddTo(_subscriptions);
            _environmentSettingsViewModel.RhythmOffsetStep
                .Subscribe(HandleRhythmOffsetStepPublished)
                .AddTo(_subscriptions);
            _environmentSettingsViewModel.RhythmOffsetLabel
                .Subscribe(HandleRhythmOffsetLabelPublished)
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
        ///     リズム判定オフセットゲージの変更を環境設定へ渡す。
        /// </summary>
        private void HandleRhythmOffsetChanged(ChangeEvent<int> changeEvent)
        {
            _environmentSettingsCommand.SetRhythmOffsetStep(changeEvent.newValue);
        }

        /// <summary>
        ///     表示言語を前へ切り替える。
        /// </summary>
        private void HandleLanguagePrevButtonClicked()
        {
            _environmentSettingsCommand.CycleLanguage(CYCLE_PREVIOUS_DIRECTION);
        }

        /// <summary>
        ///     表示言語を次へ切り替える。
        /// </summary>
        private void HandleLanguageNextButtonClicked()
        {
            _environmentSettingsCommand.CycleLanguage(CYCLE_NEXT_DIRECTION);
        }

        /// <summary>
        ///     ゲームパッド振動の強さを前へ切り替える。
        /// </summary>
        private void HandleVibrationPrevButtonClicked()
        {
            _environmentSettingsCommand.CycleVibrationStrength(CYCLE_PREVIOUS_DIRECTION);
        }

        /// <summary>
        ///     ゲームパッド振動の強さを次へ切り替える。
        /// </summary>
        private void HandleVibrationNextButtonClicked()
        {
            _environmentSettingsCommand.CycleVibrationStrength(CYCLE_NEXT_DIRECTION);
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
        /// <param name="labelKey"> UICommonテーブルのローカライズキーです。 </param>
        private void HandleScreenModeLabelPublished(string labelKey)
        {
            _screenModeLocalizedText?.Dispose();
            _screenModeLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE,
                labelKey,
                text => _screenModeValueLabel.text = text);
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
        ///     表示言語を表示へ反映する。
        /// </summary>
        /// <param name="labelKey"> UICommonテーブルのローカライズキーです。 </param>
        private void HandleLanguageLabelPublished(string labelKey)
        {
            _languageLocalizedText?.Dispose();
            _languageLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE,
                labelKey,
                text => _languageValueLabel.text = text);
        }

        /// <summary>
        ///     ゲームパッド振動の強さを表示へ反映する。
        /// </summary>
        /// <param name="labelKey"> UICommonテーブルのローカライズキーです。 </param>
        private void HandleVibrationStrengthLabelPublished(string labelKey)
        {
            _vibrationLocalizedText?.Dispose();
            _vibrationLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE,
                labelKey,
                text => _vibrationValueLabel.text = text);
        }

        /// <summary>
        ///     リズム判定オフセットの段階をゲージへ反映する。
        /// </summary>
        private void HandleRhythmOffsetStepPublished(int step)
        {
            _rhythmOffsetSlider.SetValueWithoutNotify(step);
        }

        /// <summary>
        ///     リズム判定オフセットの表示ラベルを反映する。
        /// </summary>
        private void HandleRhythmOffsetLabelPublished(string label)
        {
            _rhythmOffsetValueLabel.text = label;
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
