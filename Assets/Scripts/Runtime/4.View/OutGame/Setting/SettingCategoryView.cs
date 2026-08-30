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
        public SettingCategoryView(VisualElement rootElement)
        {
            _soundCategoryButton = Require<Button>(rootElement, SOUND_CATEGORY_BUTTON_NAME);
            _systemCategoryButton = Require<Button>(rootElement, SYSTEM_CATEGORY_BUTTON_NAME);
            _soundPanel = Require<VisualElement>(rootElement, SOUND_PANEL_NAME);
            _systemPanel = Require<VisualElement>(rootElement, SYSTEM_PANEL_NAME);
            _settingLayout = Require<VisualElement>(rootElement, SETTING_LAYOUT_NAME);
            _bgmVolumeSlider = Require<SliderInt>(rootElement, BGM_VOLUME_SLIDER_NAME);
            _returnToTitleButton = Require<Button>(rootElement, RETURN_TO_TITLE_BUTTON_NAME);

            RegisterCallbacks();
            ShowDefaultCategory();
        }

        /// <summary>
        ///     設定画面を開いた直後のサウンドカテゴリへ戻す。
        /// </summary>
        public void ShowDefaultCategory()
        {
            ShowSoundCategory();
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public void Dispose()
        {
            _soundCategoryButton.clicked -= HandleSoundCategoryClickedHandler;
            _systemCategoryButton.clicked -= HandleSystemCategoryClickedHandler;
            _settingLayout.UnregisterCallback<GeometryChangedEvent>(HandleLayoutGeometryChanged);
        }

        private const string SOUND_CATEGORY_BUTTON_NAME = "SoundCategoryButton";
        private const string SYSTEM_CATEGORY_BUTTON_NAME = "SystemCategoryButton";
        private const string SOUND_PANEL_NAME = "SoundPanel";
        private const string SYSTEM_PANEL_NAME = "SystemPanel";
        private const string SETTING_LAYOUT_NAME = "SettingLayout";
        private const string BGM_VOLUME_SLIDER_NAME = "BgmVolumeSlider";
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
        private readonly Button _returnToTitleButton;

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
            _bgmVolumeSlider.FocusDeferred();
        }

        /// <summary>
        ///     システムカテゴリを選択し、最初の設定項目へフォーカスを移す。
        /// </summary>
        private void HandleSystemCategoryClickedHandler()
        {
            ShowSystemCategory();
            _returnToTitleButton.FocusDeferred();
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
