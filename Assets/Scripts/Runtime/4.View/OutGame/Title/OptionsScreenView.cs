using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     オプション画面の View クラス。
    /// </summary>
    public class OptionsScreenView : ScreenViewBase
    {
        /// <summary>
        ///    オプション画面の View を初期化します。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <param name="outGameUIEvent"></param>
        public OptionsScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            Initialize(rootElement);
            RegisterButtonCallbacks();
        }

        /// <summary>
        ///   オプション画面の View のリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            UnregisterButtonCallbacks();
            base.Dispose();
        }

        /// <inheritdoc />
        protected override VisualElement InitialFocusElement =>
            _tabView.activeTab?.tabHeader ?? _volumeSettingsTab.tabHeader;

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        private const string BACK_BUTTON_NAME = "BackButton";
        private const string BACK_GROUND_NAME = "BackGround";
        private const string TAB_VIEW_NAME = "SettingsTabView";
        private const string VOLUME_SETTINGS_TAB_NAME = "VolumeSettings";
        private const string DATA_RESET_TAB_NAME = "DetaReset";
        private const string BGM_VOLUME_SLIDER_NAME = "BGMVolumeSlider";
        private const string DATA_RESET_BUTTON_NAME = "DataResetButton";

        private Button _backButton;
        private VisualElement _backGround;
        private TabView _tabView;
        private Tab _volumeSettingsTab;
        private Tab _dataResetTab;
        private SliderInt _bgmVolumeSlider;
        private Button _dataResetButton;

        /// <summary>
        ///     オプション画面の UI 要素を初期化します。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void Initialize(VisualElement rootElement)
        {
            if (rootElement == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(OptionsScreenView)}: Root VisualElementがnullです。");
#endif
                return;
            }

            _backButton = rootElement.Q<Button>(BACK_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {BACK_BUTTON_NAME}が見つかりません。"); 

            _backGround = rootElement.Q<VisualElement>(BACK_GROUND_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {BACK_GROUND_NAME}が見つかりません。");
            _tabView = rootElement.Q<TabView>(TAB_VIEW_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {TAB_VIEW_NAME}が見つかりません。");
            _volumeSettingsTab = rootElement.Q<Tab>(VOLUME_SETTINGS_TAB_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {VOLUME_SETTINGS_TAB_NAME}が見つかりません。");
            _dataResetTab = rootElement.Q<Tab>(DATA_RESET_TAB_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {DATA_RESET_TAB_NAME}が見つかりません。");
            _bgmVolumeSlider = rootElement.Q<SliderInt>(BGM_VOLUME_SLIDER_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {BGM_VOLUME_SLIDER_NAME}が見つかりません。");
            _dataResetButton = rootElement.Q<Button>(DATA_RESET_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {DATA_RESET_BUTTON_NAME}が見つかりません。");

            _volumeSettingsTab.tabHeader.MakeNavigable();
            _dataResetTab.tabHeader.MakeNavigable();
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);

            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();

            _backGround.RegisterCallback<PointerDownEvent>(OnPointDownEvent);
            _volumeSettingsTab.tabHeader.RegisterCallback<ClickEvent>(HandleVolumeTabClickedHandler);
            _volumeSettingsTab.tabHeader.RegisterCallback<NavigationSubmitEvent>(HandleVolumeTabSubmittedHandler);
            _dataResetTab.tabHeader.RegisterCallback<ClickEvent>(HandleDataResetTabClickedHandler);
            _dataResetTab.tabHeader.RegisterCallback<NavigationSubmitEvent>(HandleDataResetTabSubmittedHandler);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _backButton.UnregisterCallback<ClickEvent>(OnBackButtonClicked);
            _backGround.UnregisterCallback<PointerDownEvent>(OnPointDownEvent);
            _volumeSettingsTab.tabHeader.UnregisterCallback<ClickEvent>(HandleVolumeTabClickedHandler);
            _volumeSettingsTab.tabHeader.UnregisterCallback<NavigationSubmitEvent>(HandleVolumeTabSubmittedHandler);
            _dataResetTab.tabHeader.UnregisterCallback<ClickEvent>(HandleDataResetTabClickedHandler);
            _dataResetTab.tabHeader.UnregisterCallback<NavigationSubmitEvent>(HandleDataResetTabSubmittedHandler);
        }

        /// <summary>
        ///     音量設定タブがクリックされたとき、タブ内の先頭項目へ移動する。
        /// </summary>
        private void HandleVolumeTabClickedHandler(ClickEvent clickEvent)
        {
            SelectTab(_volumeSettingsTab, _bgmVolumeSlider);
        }

        /// <summary>
        ///     音量設定タブが決定されたとき、タブ内の先頭項目へ移動する。
        /// </summary>
        private void HandleVolumeTabSubmittedHandler(NavigationSubmitEvent navigationEvent)
        {
            SelectTab(_volumeSettingsTab, _bgmVolumeSlider);
            navigationEvent.StopPropagation();
        }

        /// <summary>
        ///     データリセットタブがクリックされたとき、タブ内の先頭項目へ移動する。
        /// </summary>
        private void HandleDataResetTabClickedHandler(ClickEvent clickEvent)
        {
            SelectTab(_dataResetTab, _dataResetButton);
        }

        /// <summary>
        ///     データリセットタブが決定されたとき、タブ内の先頭項目へ移動する。
        /// </summary>
        private void HandleDataResetTabSubmittedHandler(NavigationSubmitEvent navigationEvent)
        {
            SelectTab(_dataResetTab, _dataResetButton);
            navigationEvent.StopPropagation();
        }


        /// <summary>
        ///     戻るボタンが押されたときの処理。
        /// </summary>
        /// <param name="clickEvent"> クリックイベント。 </param>
        private void OnBackButtonClicked(ClickEvent clickEvent)
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     バックグラウンドが押されたときの処理。
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointDownEvent(PointerDownEvent evt)
        {
            // バックグラウンドの子要素が押された場合は処理を行わない
            if (evt.target != evt.currentTarget) { return; }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     指定タブを選択し、そのタブ内の先頭項目へフォーカスを移す。
        /// </summary>
        /// <param name="tab"> 選択するタブ。 </param>
        /// <param name="firstItem"> タブ内の先頭項目。 </param>
        private void SelectTab(Tab tab, VisualElement firstItem)
        {
            _tabView.activeTab = tab;
            firstItem.FocusDeferred();
        }
    }
}
