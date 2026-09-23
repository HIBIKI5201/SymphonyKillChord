using KillChord.Runtime.View.OutGame.Navigation;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     Home設定画面のカテゴリ切り替えとレイアウトを管理するView。
    /// </summary>
    public sealed class SettingCategoryView : IDisposable
    {
        /// <summary>
        ///     カテゴリ表示に必要なUI要素を取得する。
        /// </summary>
        public SettingCategoryView(VisualElement rootElement, HierarchicalNavigationScope hierarchicalNavigationScope)
        {
            rootElement = rootElement
                ?? throw new ArgumentNullException(nameof(rootElement));
            _soundCategoryButton = Require<Button>(rootElement, SOUND_CATEGORY_BUTTON_NAME);
            _systemCategoryButton = Require<Button>(rootElement, SYSTEM_CATEGORY_BUTTON_NAME);
            _soundPanel = Require<VisualElement>(rootElement, SOUND_PANEL_NAME);
            _systemPanel = Require<VisualElement>(rootElement, SYSTEM_PANEL_NAME);
            _settingLayout = Require<VisualElement>(rootElement, SETTING_LAYOUT_NAME);
            _bgmVolumeSlider = Require<SliderInt>(rootElement, BGM_VOLUME_SLIDER_NAME);
            _soundEffectVolumeSlider = Require<SliderInt>(rootElement, SOUND_EFFECT_VOLUME_SLIDER_NAME);
            _voiceVolumeSlider = Require<SliderInt>(rootElement, VOICE_VOLUME_SLIDER_NAME);
            _returnToTitleButton = Require<Button>(rootElement, RETURN_TO_TITLE_BUTTON_NAME);
            _navigationScope = hierarchicalNavigationScope;
            _navigationScope.SetRootLevel(new VisualElement[]
            {
                _soundCategoryButton,
                _systemCategoryButton,
            });
            _navigationScope.AddChildLevel(
                _soundCategoryButton,
                new VisualElement[]
                {
                    _bgmVolumeSlider,
                    _soundEffectVolumeSlider,
                    _voiceVolumeSlider,
                },
                _bgmVolumeSlider);
            _navigationScope.AddChildLevel(
                _systemCategoryButton,
                new VisualElement[]
                {
                    _returnToTitleButton,
                },
                _returnToTitleButton);

            RegisterCallbacks();
            ShowDefaultCategory();
        }

        /// <summary>
        ///     設定画面を開いた直後のサウンドカテゴリへ戻す。
        /// </summary>
        public void ShowDefaultCategory()
        {
            ShowSoundCategory();
            _navigationScope.ResetToRootLevel();
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public void Dispose()
        {
            _soundCategoryButton.clicked -= HandleSoundCategoryClickedHandler;
            _systemCategoryButton.clicked -= HandleSystemCategoryClickedHandler;
            _settingLayout.UnregisterCallback<GeometryChangedEvent>(HandleLayoutGeometryChanged);
            _navigationScope.Dispose();
        }

        private const string SOUND_CATEGORY_BUTTON_NAME = "SoundCategoryButton";
        private const string SYSTEM_CATEGORY_BUTTON_NAME = "SystemCategoryButton";
        private const string SOUND_PANEL_NAME = "SoundPanel";
        private const string SYSTEM_PANEL_NAME = "SystemPanel";
        private const string SETTING_LAYOUT_NAME = "SettingLayout";
        private const string BGM_VOLUME_SLIDER_NAME = "BgmVolumeSlider";
        private const string SOUND_EFFECT_VOLUME_SLIDER_NAME = "SoundEffectVolumeSlider";
        private const string VOICE_VOLUME_SLIDER_NAME = "VoiceVolumeSlider";
        private const string RETURN_TO_TITLE_BUTTON_NAME = "ReturnToTitleButton";
        private const string SELECTED_CATEGORY_CLASS = "setting-category-button--selected";
        private const string NARROW_LAYOUT_CLASS = "setting-layout--narrow";
        private const float NARROW_ASPECT_RATIO = 1.34f;

        private readonly Button _soundCategoryButton;
        private readonly Button _systemCategoryButton;
        private readonly VisualElement _soundPanel;
        private readonly VisualElement _systemPanel;
        private readonly VisualElement _settingLayout;
        private readonly SliderInt _bgmVolumeSlider;
        private readonly SliderInt _soundEffectVolumeSlider;
        private readonly SliderInt _voiceVolumeSlider;
        private readonly Button _returnToTitleButton;
        private readonly HierarchicalNavigationScope _navigationScope;

        /// <summary>
        ///     カテゴリとレイアウトのコールバックを登録する。
        /// </summary>
        private void RegisterCallbacks()
        {
            _soundCategoryButton.clicked += HandleSoundCategoryClickedHandler;
            _systemCategoryButton.clicked += HandleSystemCategoryClickedHandler;
            _settingLayout.RegisterCallback<GeometryChangedEvent>(HandleLayoutGeometryChanged);
        }

        /// <summary>
        ///     サウンドカテゴリを選択し、最初の設定項目へフォーカスを移す。
        /// </summary>
        private void HandleSoundCategoryClickedHandler()
        {
            ShowSoundCategory();
            _navigationScope.EnterLevel(_soundCategoryButton);
        }

        /// <summary>
        ///     システムカテゴリを選択し、最初の設定項目へフォーカスを移す。
        /// </summary>
        private void HandleSystemCategoryClickedHandler()
        {
            ShowSystemCategory();
            _navigationScope.EnterLevel(_systemCategoryButton);
        }

        /// <summary>
        ///     サウンドカテゴリを表示する。
        /// </summary>
        private void ShowSoundCategory()
        {
            _soundPanel.style.display = DisplayStyle.Flex;
            _systemPanel.style.display = DisplayStyle.None;
            _soundCategoryButton.AddToClassList(SELECTED_CATEGORY_CLASS);
            _systemCategoryButton.RemoveFromClassList(SELECTED_CATEGORY_CLASS);
        }

        /// <summary>
        ///     システムカテゴリを表示する。
        /// </summary>
        private void ShowSystemCategory()
        {
            _soundPanel.style.display = DisplayStyle.None;
            _systemPanel.style.display = DisplayStyle.Flex;
            _soundCategoryButton.RemoveFromClassList(SELECTED_CATEGORY_CLASS);
            _systemCategoryButton.AddToClassList(SELECTED_CATEGORY_CLASS);
        }

        /// <summary>
        ///     4:3相当の狭い表示ではカテゴリを上部へ切り替える。
        /// </summary>
        private void HandleLayoutGeometryChanged(GeometryChangedEvent geometryChangedEvent)
        {
            bool isNarrow = geometryChangedEvent.newRect.width
                <= geometryChangedEvent.newRect.height * NARROW_ASPECT_RATIO;
            _settingLayout.EnableInClassList(NARROW_LAYOUT_CLASS, isNarrow);
        }

        /// <summary>
        ///     必須UI要素を取得する。
        /// </summary>
        private static T Require<T>(VisualElement rootElement, string elementName)
            where T : VisualElement
        {
            return rootElement.Q<T>(elementName)
                ?? throw new InvalidOperationException(
                    $"[{nameof(SettingCategoryView)}] {elementName} が見つかりませんでした。");
        }
    }
}
