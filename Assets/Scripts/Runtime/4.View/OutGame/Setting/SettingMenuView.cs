using KillChord.Runtime.View.OutGame.Navigation;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     Home設定画面のメニュー表示とオーディオ設定パネルの切り替えを管理するView。
    /// </summary>
    public sealed class SettingMenuView : IDisposable
    {
        /// <summary>
        ///     メニュー表示に必要なUI要素を取得する。
        /// </summary>
        public SettingMenuView(VisualElement rootElement, HierarchicalNavigationScope hierarchicalNavigationScope)
        {
            rootElement = rootElement
                ?? throw new ArgumentNullException(nameof(rootElement));
            _backGround = Require<VisualElement>(rootElement, BACKGROUND_NAME);
            _environmentSettingButton = Require<Button>(rootElement, ENVIRONMENT_SETTING_BUTTON_NAME);
            _audioSettingButton = Require<Button>(rootElement, AUDIO_SETTING_BUTTON_NAME);
            _returnToTitleButton = Require<Button>(rootElement, RETURN_TO_TITLE_BUTTON_NAME);
            _closeButton = Require<Button>(rootElement, CLOSE_BUTTON_NAME);
            _settingMenu = Require<VisualElement>(rootElement, SETTING_MENU_NAME);
            _soundPanel = Require<VisualElement>(rootElement, SOUND_PANEL_NAME);
            _soundPanelBackButton = Require<Button>(rootElement, SOUND_PANEL_BACK_BUTTON_NAME);
            _bgmVolumeSlider = Require<SliderInt>(rootElement, BGM_VOLUME_SLIDER_NAME);
            _soundEffectVolumeSlider = Require<SliderInt>(rootElement, SOUND_EFFECT_VOLUME_SLIDER_NAME);
            _voiceVolumeSlider = Require<SliderInt>(rootElement, VOICE_VOLUME_SLIDER_NAME);
            _navigationScope = hierarchicalNavigationScope;
            _navigationScope.SetRootLevel(new VisualElement[]
            {
                _environmentSettingButton,
                _audioSettingButton,
                _returnToTitleButton,
                _closeButton,
            });
            _navigationScope.AddChildLevel(
                _audioSettingButton,
                new VisualElement[]
                {
                    _soundPanelBackButton,
                    _bgmVolumeSlider,
                    _soundEffectVolumeSlider,
                    _voiceVolumeSlider,
                },
                _bgmVolumeSlider);

            RegisterCallbacks();
            ShowMenu();
        }

        /// <summary>
        ///     設定画面を開いた直後のメニューへ戻す。
        /// </summary>
        public void ShowMenu()
        {
            _settingMenu.style.display = DisplayStyle.Flex;
            _soundPanel.style.display = DisplayStyle.None;
            _backGround.RemoveFromClassList(AUDIO_BACKGROUND_CLASS);
            _backGround.AddToClassList(MENU_BACKGROUND_CLASS);
            _navigationScope.ResetToRootLevel();
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public void Dispose()
        {
            _audioSettingButton.clicked -= HandleAudioSettingButtonClickedHandler;
            _soundPanelBackButton.clicked -= HandleSoundPanelBackButtonClickedHandler;
            _navigationScope.Dispose();
        }

        private const string BACKGROUND_NAME = "BackGround";
        private const string MENU_BACKGROUND_CLASS = "setting-window--menu";
        private const string AUDIO_BACKGROUND_CLASS = "setting-window--audio";
        private const string ENVIRONMENT_SETTING_BUTTON_NAME = "EnvironmentSettingButton";
        private const string AUDIO_SETTING_BUTTON_NAME = "AudioSettingButton";
        private const string RETURN_TO_TITLE_BUTTON_NAME = "ReturnToTitleButton";
        private const string CLOSE_BUTTON_NAME = "CloseButton";
        private const string SETTING_MENU_NAME = "SettingMenu";
        private const string SOUND_PANEL_NAME = "SoundPanel";
        private const string SOUND_PANEL_BACK_BUTTON_NAME = "SoundPanelBackButton";
        private const string BGM_VOLUME_SLIDER_NAME = "BgmVolumeSlider";
        private const string SOUND_EFFECT_VOLUME_SLIDER_NAME = "SoundEffectVolumeSlider";
        private const string VOICE_VOLUME_SLIDER_NAME = "VoiceVolumeSlider";

        private readonly VisualElement _backGround;
        private readonly Button _environmentSettingButton;
        private readonly Button _audioSettingButton;
        private readonly Button _returnToTitleButton;
        private readonly Button _closeButton;
        private readonly VisualElement _settingMenu;
        private readonly VisualElement _soundPanel;
        private readonly Button _soundPanelBackButton;
        private readonly SliderInt _bgmVolumeSlider;
        private readonly SliderInt _soundEffectVolumeSlider;
        private readonly SliderInt _voiceVolumeSlider;
        private readonly HierarchicalNavigationScope _navigationScope;

        /// <summary>
        ///     オーディオ設定パネルを開き、最初の設定項目へフォーカスを移す。
        /// </summary>
        private void HandleAudioSettingButtonClickedHandler()
        {
            _settingMenu.style.display = DisplayStyle.None;
            _soundPanel.style.display = DisplayStyle.Flex;
            _backGround.RemoveFromClassList(MENU_BACKGROUND_CLASS);
            _backGround.AddToClassList(AUDIO_BACKGROUND_CLASS);
            _navigationScope.EnterLevel(_audioSettingButton);
        }

        /// <summary>
        ///     オーディオ設定パネルを閉じ、メニューへ戻る。
        /// </summary>
        private void HandleSoundPanelBackButtonClickedHandler()
        {
            ShowMenu();
        }

        /// <summary>
        ///     メニューとオーディオ設定パネルのコールバックを登録する。
        /// </summary>
        private void RegisterCallbacks()
        {
            _audioSettingButton.clicked += HandleAudioSettingButtonClickedHandler;
            _soundPanelBackButton.clicked += HandleSoundPanelBackButtonClickedHandler;
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
    }
}
