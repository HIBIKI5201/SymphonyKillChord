using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.Persistent.Localization;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     Home設定画面のメニュー表示と各設定パネルの切り替えを管理するView。
    /// </summary>
    public sealed class SettingMenuView : IDisposable
    {
        /// <summary>
        ///     メニュー表示に必要なUI要素を取得する。
        /// </summary>
        public SettingMenuView(VisualElement rootElement, HierarchicalNavigationScope hierarchicalNavigationScope)
        {
            // 画面の各要素を取得する。
            rootElement = rootElement
                ?? throw new ArgumentNullException(nameof(rootElement));
            _backGround = Require<VisualElement>(rootElement, BACKGROUND_NAME);
            _settingTitleBar = Require<VisualElement>(rootElement, SETTING_TITLE_BAR_NAME);
            _environmentSettingButton = Require<Button>(rootElement, ENVIRONMENT_SETTING_BUTTON_NAME);
            _audioSettingButton = Require<Button>(rootElement, AUDIO_SETTING_BUTTON_NAME);
            _controlSettingButton = Require<Button>(rootElement, CONTROL_SETTING_BUTTON_NAME);
            _returnToTitleButton = Require<Button>(rootElement, RETURN_TO_TITLE_BUTTON_NAME);
            _closeButton = Require<Button>(rootElement, CLOSE_BUTTON_NAME);
            _settingMenu = Require<VisualElement>(rootElement, SETTING_MENU_NAME);
            _soundPanel = Require<VisualElement>(rootElement, SOUND_PANEL_NAME);
            _soundPanelBackButton = Require<Button>(rootElement, SOUND_PANEL_BACK_BUTTON_NAME);
            _environmentPanelBackButton = Require<Button>(rootElement, ENVIRONMENT_PANEL_BACK_BUTTON_NAME);
            _bgmVolumeSlider = Require<SliderInt>(rootElement, BGM_VOLUME_SLIDER_NAME);
            _soundEffectVolumeSlider = Require<SliderInt>(rootElement, SOUND_EFFECT_VOLUME_SLIDER_NAME);
            _voiceVolumeSlider = Require<SliderInt>(rootElement, VOICE_VOLUME_SLIDER_NAME);
            _environmentPanel = Require<VisualElement>(rootElement, ENVIRONMENT_PANEL_NAME);
            _screenModePrevButton = Require<Button>(rootElement, SCREEN_MODE_PREV_BUTTON_NAME);
            _screenModeNextButton = Require<Button>(rootElement, SCREEN_MODE_NEXT_BUTTON_NAME);
            _resolutionPrevButton = Require<Button>(rootElement, RESOLUTION_PREV_BUTTON_NAME);
            _resolutionNextButton = Require<Button>(rootElement, RESOLUTION_NEXT_BUTTON_NAME);
            _qualityLevelPrevButton = Require<Button>(rootElement, QUALITY_LEVEL_PREV_BUTTON_NAME);
            _qualityLevelNextButton = Require<Button>(rootElement, QUALITY_LEVEL_NEXT_BUTTON_NAME);
            _brightnessSlider = Require<SliderInt>(rootElement, BRIGHTNESS_SLIDER_NAME);
            _languagePrevButton = Require<Button>(rootElement, LANGUAGE_PREV_BUTTON_NAME);
            _languageNextButton = Require<Button>(rootElement, LANGUAGE_NEXT_BUTTON_NAME);
            _vibrationPrevButton = Require<Button>(rootElement, VIBRATION_PREV_BUTTON_NAME);
            _vibrationNextButton = Require<Button>(rootElement, VIBRATION_NEXT_BUTTON_NAME);
            _rhythmOffsetSlider = Require<SliderInt>(rootElement, RHYTHM_OFFSET_SLIDER_NAME);
            _environmentPanelSaveButton = Require<Button>(rootElement, ENVIRONMENT_PANEL_SAVE_BUTTON_NAME);
            _controlPanel = Require<VisualElement>(rootElement, CONTROL_PANEL_NAME);
            _controlPanelBackButton = Require<Button>(rootElement, CONTROL_PANEL_BACK_BUTTON_NAME);
            _cameraSensitivitySlider = Require<SliderInt>(rootElement, CAMERA_SENSITIVITY_SLIDER_NAME);
            _cameraInvertPrevButton = Require<Button>(rootElement, CAMERA_INVERT_PREV_BUTTON_NAME);
            _cameraInvertNextButton = Require<Button>(rootElement, CAMERA_INVERT_NEXT_BUTTON_NAME);
            _autoLockOnPrevButton = Require<Button>(rootElement, AUTO_LOCK_ON_PREV_BUTTON_NAME);
            _autoLockOnNextButton = Require<Button>(rootElement, AUTO_LOCK_ON_NEXT_BUTTON_NAME);
            _controlPanelSaveButton = Require<Button>(rootElement, CONTROL_PANEL_SAVE_BUTTON_NAME);
            _buttonLayoutPrevButton = Require<Button>(rootElement, BUTTON_LAYOUT_PREV_BUTTON_NAME);
            _buttonLayoutNextButton = Require<Button>(rootElement, BUTTON_LAYOUT_NEXT_BUTTON_NAME);
            // メニューを第1階層、オーディオ設定・環境設定・操作設定を第2階層とする操作範囲を登録する。
            _navigationScope = hierarchicalNavigationScope;
            _navigationScope.SetRootLevel(new VisualElement[]
            {
                _environmentSettingButton,
                _audioSettingButton,
                _controlSettingButton,
                _returnToTitleButton,
                _closeButton,
            });
            _navigationScope.AddChildLevel(
                _audioSettingButton,
                new VisualElement[]
                {
                    _bgmVolumeSlider,
                    _soundPanelBackButton,
                    _soundEffectVolumeSlider,
                    _voiceVolumeSlider,
                },
                _bgmVolumeSlider);
            _navigationScope.AddChildLevel(
                _environmentSettingButton,
                new VisualElement[]
                {
                    _screenModePrevButton,
                    _screenModeNextButton,
                    _resolutionPrevButton,
                    _resolutionNextButton,
                    _qualityLevelPrevButton,
                    _qualityLevelNextButton,
                    _brightnessSlider,
                    _languagePrevButton,
                    _languageNextButton,
                    _vibrationPrevButton,
                    _vibrationNextButton,
                    _rhythmOffsetSlider,
                    _environmentPanelSaveButton,
                    _environmentPanelBackButton,
                },
                _screenModePrevButton);
            _navigationScope.AddChildLevel(
                _controlSettingButton,
                new VisualElement[]
                {
                    _cameraSensitivitySlider,
                    _cameraInvertPrevButton,
                    _cameraInvertNextButton,
                    _autoLockOnPrevButton,
                    _autoLockOnNextButton,
                    _buttonLayoutPrevButton,
                    _buttonLayoutNextButton,
                    _controlPanelSaveButton,
                    _controlPanelBackButton,
                },
                _cameraSensitivitySlider);

            // 操作を登録し、メニューを表示する。
            RegisterCallbacks();
            ShowMenu();

            // ボタンの文言をローカライズに登録する。
            _audioSettingLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE, "ui.setting.audio", text => _audioSettingButton.text = text);
            _environmentPanelSaveLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE, "ui.setting.environment_save", text => _environmentPanelSaveButton.text = text);
            _controlSettingLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE, "ui.setting.control", text => _controlSettingButton.text = text, "操作設定");
            _controlPanelSaveLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE, "ui.setting.environment_save", text => _controlPanelSaveButton.text = text);
        }

        /// <summary>
        ///     環境設定パネルを保存せずに離れる際に呼び出される処理です。
        /// </summary>
        public Action OnCancelEnvironmentChanges { get; set; }

        /// <summary>
        ///     設定画面を開いた直後のメニューへ戻す。
        /// </summary>
        public void ShowMenu()
        {
            _settingTitleBar.style.display = DisplayStyle.Flex;
            _settingMenu.style.display = DisplayStyle.Flex;
            _soundPanel.style.display = DisplayStyle.None;
            _environmentPanel.style.display = DisplayStyle.None;
            _controlPanel.style.display = DisplayStyle.None;
            _backGround.RemoveFromClassList(AUDIO_BACKGROUND_CLASS);
            _backGround.RemoveFromClassList(ENVIRONMENT_BACKGROUND_CLASS);
            _backGround.RemoveFromClassList(CONTROL_BACKGROUND_CLASS);
            _backGround.AddToClassList(MENU_BACKGROUND_CLASS);
            _navigationScope.ResetToRootLevel();
            _currentState = PanelState.Menu;
        }

        /// <summary>
        ///     サブパネル表示中であれば一段階だけメニューへ戻す。
        ///     環境設定パネルからの場合、未保存の変更を破棄してから戻す。
        /// </summary>
        /// <returns> メニューへ戻した場合はtrue。既にメニュー表示中で戻る先がない場合はfalse。 </returns>
        public bool TryGoBack()
        {
            if (_currentState == PanelState.Menu)
            {
                return false;
            }

            // 環境設定と操作設定は同じ環境設定を編集するため、どちらから離れる場合も未保存の変更を破棄する。
            if (_currentState == PanelState.Environment || _currentState == PanelState.Control)
            {
                OnCancelEnvironmentChanges?.Invoke();
            }

            ShowMenu();
            return true;
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public void Dispose()
        {
            _audioSettingButtonPreset.Dispose();
            _soundPanelBackButtonPreset.Dispose();
            _environmentPanelBackButtonPreset.Dispose();
            _environmentSettingButtonPreset.Dispose();
            _environmentPanelSaveButtonPreset.Dispose();
            _controlSettingButtonPreset.Dispose();
            _controlPanelBackButtonPreset.Dispose();
            _controlPanelSaveButtonPreset.Dispose();
            _audioSettingLocalizedText.Dispose();
            _environmentPanelSaveLocalizedText.Dispose();
            _controlSettingLocalizedText.Dispose();
            _controlPanelSaveLocalizedText.Dispose();
            _navigationScope.Dispose();
        }

        private const string BACKGROUND_NAME = "BackGround";
        private const string SETTING_TITLE_BAR_NAME = "SettingTitleBar";
        private const string MENU_BACKGROUND_CLASS = "setting-window--menu";
        private const string AUDIO_BACKGROUND_CLASS = "setting-window--audio";
        private const string ENVIRONMENT_BACKGROUND_CLASS = "setting-window--environment";
        private const string CONTROL_BACKGROUND_CLASS = "setting-window--control";
        private const string CONTROL_SETTING_BUTTON_NAME = "ControlSettingButton";
        private const string CONTROL_PANEL_NAME = "ControlPanel";
        private const string CONTROL_PANEL_BACK_BUTTON_NAME = "ControlPanelBackButton";
        private const string CONTROL_PANEL_SAVE_BUTTON_NAME = "ControlPanelSaveButton";
        private const string CAMERA_SENSITIVITY_SLIDER_NAME = "CameraSensitivitySlider";
        private const string CAMERA_INVERT_PREV_BUTTON_NAME = "CameraInvertPrevButton";
        private const string CAMERA_INVERT_NEXT_BUTTON_NAME = "CameraInvertNextButton";
        private const string AUTO_LOCK_ON_PREV_BUTTON_NAME = "AutoLockOnPrevButton";
        private const string AUTO_LOCK_ON_NEXT_BUTTON_NAME = "AutoLockOnNextButton";
        private const string BUTTON_LAYOUT_PREV_BUTTON_NAME = "ButtonLayoutPrevButton";
        private const string BUTTON_LAYOUT_NEXT_BUTTON_NAME = "ButtonLayoutNextButton";
        private const string ENVIRONMENT_SETTING_BUTTON_NAME = "EnvironmentSettingButton";
        private const string AUDIO_SETTING_BUTTON_NAME = "AudioSettingButton";
        private const string RETURN_TO_TITLE_BUTTON_NAME = "ReturnToTitleButton";
        private const string CLOSE_BUTTON_NAME = "CloseButton";
        private const string SETTING_MENU_NAME = "SettingMenu";
        private const string SOUND_PANEL_NAME = "SoundPanel";
        private const string SOUND_PANEL_BACK_BUTTON_NAME = "SoundPanelBackButton";
        private const string ENVIRONMENT_PANEL_BACK_BUTTON_NAME = "EnvironmentPanelBackButton";
        private const string BGM_VOLUME_SLIDER_NAME = "BgmVolumeSlider";
        private const string SOUND_EFFECT_VOLUME_SLIDER_NAME = "SoundEffectVolumeSlider";
        private const string VOICE_VOLUME_SLIDER_NAME = "VoiceVolumeSlider";
        private const string ENVIRONMENT_PANEL_NAME = "EnvironmentPanel";
        private const string SCREEN_MODE_PREV_BUTTON_NAME = "ScreenModePrevButton";
        private const string SCREEN_MODE_NEXT_BUTTON_NAME = "ScreenModeNextButton";
        private const string RESOLUTION_PREV_BUTTON_NAME = "ResolutionPrevButton";
        private const string RESOLUTION_NEXT_BUTTON_NAME = "ResolutionNextButton";
        private const string QUALITY_LEVEL_PREV_BUTTON_NAME = "QualityLevelPrevButton";
        private const string QUALITY_LEVEL_NEXT_BUTTON_NAME = "QualityLevelNextButton";
        private const string BRIGHTNESS_SLIDER_NAME = "BrightnessSlider";
        private const string LANGUAGE_PREV_BUTTON_NAME = "LanguagePrevButton";
        private const string LANGUAGE_NEXT_BUTTON_NAME = "LanguageNextButton";
        private const string VIBRATION_PREV_BUTTON_NAME = "VibrationPrevButton";
        private const string VIBRATION_NEXT_BUTTON_NAME = "VibrationNextButton";
        private const string RHYTHM_OFFSET_SLIDER_NAME = "RhythmOffsetSlider";
        private const string ENVIRONMENT_PANEL_SAVE_BUTTON_NAME = "EnvironmentPanelSaveButton";
        private const string UI_COMMON_TABLE = "UICommon";

        private readonly VisualElement _backGround;
        private readonly VisualElement _settingTitleBar;
        private readonly Button _environmentSettingButton;
        private readonly Button _audioSettingButton;
        private readonly Button _returnToTitleButton;
        private readonly Button _closeButton;
        private readonly VisualElement _settingMenu;
        private readonly VisualElement _soundPanel;
        private readonly Button _soundPanelBackButton;
        private readonly Button _environmentPanelBackButton;
        private IDisposable _soundPanelBackButtonPreset;
        private IDisposable _environmentPanelBackButtonPreset;
        private readonly SliderInt _bgmVolumeSlider;
        private readonly SliderInt _soundEffectVolumeSlider;
        private readonly SliderInt _voiceVolumeSlider;
        private readonly VisualElement _environmentPanel;
        private readonly Button _screenModePrevButton;
        private readonly Button _screenModeNextButton;
        private readonly Button _resolutionPrevButton;
        private readonly Button _resolutionNextButton;
        private readonly Button _qualityLevelPrevButton;
        private readonly Button _qualityLevelNextButton;
        private readonly SliderInt _brightnessSlider;
        private readonly Button _languagePrevButton;
        private readonly Button _languageNextButton;
        private readonly Button _vibrationPrevButton;
        private readonly Button _vibrationNextButton;
        private readonly SliderInt _rhythmOffsetSlider;
        private readonly Button _environmentPanelSaveButton;
        private readonly Button _controlSettingButton;
        private readonly VisualElement _controlPanel;
        private readonly Button _controlPanelBackButton;
        private readonly SliderInt _cameraSensitivitySlider;
        private readonly Button _cameraInvertPrevButton;
        private readonly Button _cameraInvertNextButton;
        private readonly Button _autoLockOnPrevButton;
        private readonly Button _autoLockOnNextButton;
        private readonly Button _controlPanelSaveButton;
        private readonly Button _buttonLayoutPrevButton;
        private readonly Button _buttonLayoutNextButton;
        private readonly HierarchicalNavigationScope _navigationScope;
        private IDisposable _audioSettingButtonPreset;
        private IDisposable _environmentSettingButtonPreset;
        private IDisposable _environmentPanelSaveButtonPreset;
        private IDisposable _controlSettingButtonPreset;
        private IDisposable _controlPanelBackButtonPreset;
        private IDisposable _controlPanelSaveButtonPreset;
        private LocalizedElementText _audioSettingLocalizedText;
        private LocalizedElementText _environmentPanelSaveLocalizedText;
        private LocalizedElementText _controlSettingLocalizedText;
        private LocalizedElementText _controlPanelSaveLocalizedText;
        private PanelState _currentState;

        /// <summary>
        ///     オーディオ設定パネルを開き、最初の設定項目へフォーカスを移す。
        /// </summary>
        private void HandleAudioSettingButtonClickedHandler()
        {
            _settingTitleBar.style.display = DisplayStyle.None;
            _settingMenu.style.display = DisplayStyle.None;
            _soundPanel.style.display = DisplayStyle.Flex;
            _backGround.RemoveFromClassList(MENU_BACKGROUND_CLASS);
            _backGround.AddToClassList(AUDIO_BACKGROUND_CLASS);
            _navigationScope.EnterLevel(_audioSettingButton);
            _currentState = PanelState.Sound;
        }

        /// <summary>
        ///     環境設定パネルを開き、最初の設定項目へフォーカスを移す。
        /// </summary>
        private void HandleEnvironmentSettingButtonClickedHandler()
        {
            _settingTitleBar.style.display = DisplayStyle.None;
            _settingMenu.style.display = DisplayStyle.None;
            _environmentPanel.style.display = DisplayStyle.Flex;
            _backGround.RemoveFromClassList(MENU_BACKGROUND_CLASS);
            _backGround.AddToClassList(ENVIRONMENT_BACKGROUND_CLASS);
            _navigationScope.EnterLevel(_environmentSettingButton);
            _currentState = PanelState.Environment;
        }

        /// <summary>
        ///     操作設定パネルを開き、最初の設定項目へフォーカスを移す。
        /// </summary>
        private void HandleControlSettingButtonClickedHandler()
        {
            _settingTitleBar.style.display = DisplayStyle.None;
            _settingMenu.style.display = DisplayStyle.None;
            _controlPanel.style.display = DisplayStyle.Flex;
            _backGround.RemoveFromClassList(MENU_BACKGROUND_CLASS);
            _backGround.AddToClassList(CONTROL_BACKGROUND_CLASS);
            _navigationScope.EnterLevel(_controlSettingButton);
            _currentState = PanelState.Control;
        }

        /// <summary>
        ///     環境設定パネルを閉じ、メニューへ戻る。
        /// </summary>
        private void HandleEnvironmentPanelSaveButtonClickedHandler()
        {
            ShowMenu();
        }

        /// <summary>
        ///     メニューと各設定パネルのコールバックを登録する。
        /// </summary>
        private void RegisterCallbacks()
        {
            _soundPanelBackButtonPreset = _soundPanelBackButton.ApplyBasicButtonPreset(HandlePanelBackButtonClickedHandler);
            _environmentPanelBackButtonPreset = _environmentPanelBackButton.ApplyBasicButtonPreset(HandlePanelBackButtonClickedHandler);
            _audioSettingButtonPreset = _audioSettingButton.ApplyBasicButtonPreset(HandleAudioSettingButtonClickedHandler);
            _environmentSettingButtonPreset = _environmentSettingButton.ApplyBasicButtonPreset(HandleEnvironmentSettingButtonClickedHandler);
            _environmentPanelSaveButtonPreset = _environmentPanelSaveButton.ApplyBasicButtonPreset(HandleEnvironmentPanelSaveButtonClickedHandler);
            _controlSettingButtonPreset = _controlSettingButton.ApplyBasicButtonPreset(HandleControlSettingButtonClickedHandler);
            _controlPanelBackButtonPreset = _controlPanelBackButton.ApplyBasicButtonPreset(HandlePanelBackButtonClickedHandler);
            _controlPanelSaveButtonPreset = _controlPanelSaveButton.ApplyBasicButtonPreset(HandleEnvironmentPanelSaveButtonClickedHandler);
        }

        /// <summary>
        ///     サブパネルから戻り、未保存の環境設定を取り消す。
        /// </summary>
        private void HandlePanelBackButtonClickedHandler()
        {
            TryGoBack();
        }

        /// <summary>
        ///     必須UI要素を取得する。
        /// </summary>
        private static T Require<T>(VisualElement rootElement, string elementName)
            where T : VisualElement
        {
            return rootElement.Q<T>(elementName)
                ?? throw new InvalidOperationException(
                    $"[{nameof(SettingMenuView)}] {elementName} が見つかりませんでした。");
        }

        /// <summary>
        ///     設定画面内で現在表示中の階層です。
        /// </summary>
        private enum PanelState
        {
            Menu,
            Sound,
            Environment,
            Control,
        }
    }
}
