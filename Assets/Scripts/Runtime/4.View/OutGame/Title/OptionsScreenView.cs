using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Threading;
using System.Threading.Tasks;
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
        /// <param name="rootElement"> オプション画面のルート要素です。 </param>
        /// <param name="outGameUIEvent"> アウトゲームの UI イベントです。 </param>
        /// <param name="hierarchicalNavigationScope"> オプション画面の階層ごとにフォーカスを管理するクラスです。 </param>
        public OptionsScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent,
            HierarchicalNavigationScope hierarchicalNavigationScope)
            : base(rootElement, outGameUIEvent)
        {
            Initialize(rootElement, hierarchicalNavigationScope);
            RegisterButtonCallbacks();
        }

        /// <summary>
        ///     タブ選択状態へ戻してオプション画面を表示する。
        /// </summary>
        public override ValueTask Show(CancellationToken cancellationToken = default)
        {
            NormalizeActiveTab();
            _navigationScope.ResetToRootLevel();
            return base.Show(cancellationToken);
        }

        /// <summary>
        ///   オプション画面の View のリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            UnregisterButtonCallbacks();
            _navigationScope.Dispose();
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
        private const string SOUND_EFFECT_VOLUME_SLIDER_NAME = "SEVolumeSlider";
        private const string DATA_RESET_BUTTON_NAME = "DataResetButton";
        private const int VOLUME_STEP = 1;

        private Button _backButton;
        private VisualElement _backGround;
        private TabView _tabView;
        private Tab _volumeSettingsTab;
        private Tab _dataResetTab;
        private SliderInt _bgmVolumeSlider;
        private SliderInt _soundEffectVolumeSlider;
        private Button _dataResetButton;
        private HierarchicalNavigationScope _navigationScope;
        private IDisposable _backButtonActivation;

        /// <summary>
        ///     オプション画面の UI 要素を初期化します。
        /// </summary>
        /// <param name="rootElement"> オプション画面のルート要素です。 </param>
        /// <param name="hierarchicalNavigationScope"> オプション画面の階層ごとにフォーカスを管理するクラスです。 </param>
        /// <exception cref="NullReferenceException"> 必要な UI 要素が見つからない場合に発生します。 </exception>
        private void Initialize(VisualElement rootElement, HierarchicalNavigationScope hierarchicalNavigationScope)
        {
            if (rootElement == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(OptionsScreenView)}: Root VisualElementがnullです。");
#endif
                return;
            }
            if (hierarchicalNavigationScope == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(OptionsScreenView)}: HierarchicalNavigationScopeがnullです。");
#endif
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
            _soundEffectVolumeSlider = rootElement.Q<SliderInt>(SOUND_EFFECT_VOLUME_SLIDER_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {SOUND_EFFECT_VOLUME_SLIDER_NAME}が見つかりません。");
            _dataResetButton = rootElement.Q<Button>(DATA_RESET_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(OptionsScreenView)}: {DATA_RESET_BUTTON_NAME}が見つかりません。");
            _navigationScope = hierarchicalNavigationScope;
            _navigationScope.SetRootLevel(new VisualElement[]
            {
                _volumeSettingsTab.tabHeader,
                _dataResetTab.tabHeader,
            });
            _navigationScope.AddChildLevel(
                _volumeSettingsTab.tabHeader,
                new VisualElement[]
                {
                    _bgmVolumeSlider,
                    _soundEffectVolumeSlider,
                },
                _bgmVolumeSlider);
            _navigationScope.AddChildLevel(
                _dataResetTab.tabHeader,
                new VisualElement[]
                {
                    _dataResetButton,
                },
                _dataResetButton);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);

            _backGround.RegisterCallback<PointerDownEvent>(OnPointDownEvent);
            _volumeSettingsTab.tabHeader.RegisterCallback<ClickEvent>(HandleVolumeTabClickedHandler);
            _volumeSettingsTab.tabHeader.RegisterCallback<NavigationSubmitEvent>(HandleVolumeTabSubmittedHandler);
            _dataResetTab.tabHeader.RegisterCallback<ClickEvent>(HandleDataResetTabClickedHandler);
            _dataResetTab.tabHeader.RegisterCallback<NavigationSubmitEvent>(HandleDataResetTabSubmittedHandler);
            RootElement.RegisterCallback<NavigationMoveEvent>(
                HandleVolumeSliderNavigationHandler,
                TrickleDown.TrickleDown);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _backButtonActivation?.Dispose();
            _backGround.UnregisterCallback<PointerDownEvent>(OnPointDownEvent);
            _volumeSettingsTab.tabHeader.UnregisterCallback<ClickEvent>(HandleVolumeTabClickedHandler);
            _volumeSettingsTab.tabHeader.UnregisterCallback<NavigationSubmitEvent>(HandleVolumeTabSubmittedHandler);
            _dataResetTab.tabHeader.UnregisterCallback<ClickEvent>(HandleDataResetTabClickedHandler);
            _dataResetTab.tabHeader.UnregisterCallback<NavigationSubmitEvent>(HandleDataResetTabSubmittedHandler);
            RootElement.UnregisterCallback<NavigationMoveEvent>(
                HandleVolumeSliderNavigationHandler,
                TrickleDown.TrickleDown);
        }

        /// <summary>
        ///     音量設定タブがクリックされたとき、タブ内の先頭項目へ移動する。
        /// </summary>
        private void HandleVolumeTabClickedHandler(ClickEvent clickEvent)
        {
            SelectTab(_volumeSettingsTab);
        }

        /// <summary>
        ///     音量設定タブが決定されたとき、タブ内の先頭項目へ移動する。
        /// </summary>
        private void HandleVolumeTabSubmittedHandler(NavigationSubmitEvent navigationEvent)
        {
            SelectTab(_volumeSettingsTab);
            navigationEvent.StopPropagation();
        }

        /// <summary>
        ///     データリセットタブがクリックされたとき、タブ内の先頭項目へ移動する。
        /// </summary>
        private void HandleDataResetTabClickedHandler(ClickEvent clickEvent)
        {
            SelectTab(_dataResetTab);
        }

        /// <summary>
        ///     データリセットタブが決定されたとき、タブ内の先頭項目へ移動する。
        /// </summary>
        private void HandleDataResetTabSubmittedHandler(NavigationSubmitEvent navigationEvent)
        {
            SelectTab(_dataResetTab);
            navigationEvent.StopPropagation();
        }

        /// <summary>
        ///     音量スライダー間の上下フォーカス移動を明示的に処理する。
        /// </summary>
        /// <param name="navigationEvent"> ナビゲーション移動イベント。 </param>
        private void HandleVolumeSliderNavigationHandler(NavigationMoveEvent navigationEvent)
        {
            VisualElement focusedElement = FocusedElement;
            if (focusedElement == null)
            {
                return;
            }

            bool isBgmFocused = ReferenceEquals(focusedElement, _bgmVolumeSlider) ||
                _bgmVolumeSlider.Contains(focusedElement);
            bool isSoundEffectFocused = ReferenceEquals(focusedElement, _soundEffectVolumeSlider) ||
                _soundEffectVolumeSlider.Contains(focusedElement);

            if (!isBgmFocused && !isSoundEffectFocused)
            {
                return;
            }

            SliderInt focusedSlider = isBgmFocused
                ? _bgmVolumeSlider
                : _soundEffectVolumeSlider;

            switch (navigationEvent.direction)
            {
                case NavigationMoveEvent.Direction.Left:
                    ChangeVolume(focusedSlider, -VOLUME_STEP);
                    break;
                case NavigationMoveEvent.Direction.Right:
                    ChangeVolume(focusedSlider, VOLUME_STEP);
                    break;
                case NavigationMoveEvent.Direction.Up when isSoundEffectFocused:
                    _bgmVolumeSlider.Focus();
                    break;
                case NavigationMoveEvent.Direction.Down when isBgmFocused:
                    _soundEffectVolumeSlider.Focus();
                    break;
                case NavigationMoveEvent.Direction.Up:
                case NavigationMoveEvent.Direction.Down:
                    break;
                default:
                    return;
            }

            // スライダー操作中は TabView の内部要素へフォーカスを逃がさない。
            RootElement.panel?.focusController?.IgnoreEvent(navigationEvent);
            navigationEvent.StopImmediatePropagation();
        }


        /// <summary>
        ///     戻るボタンが押されたときの処理。
        /// </summary>
        private void HandleBackButtonActivationHandler()
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     バックグラウンドが押されたときの処理。
        /// </summary>
        /// <param name="evt"> ポインター押下イベント。 </param>
        private void OnPointDownEvent(PointerDownEvent evt)
        {
            // バックグラウンドの子要素が押された場合は処理を行わない
            if (evt.target != evt.currentTarget) { return; }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     指定タブを選択し、そのタブの内容操作へ切り替える。
        /// </summary>
        /// <param name="tab"> 選択するタブ。 </param>
        private void SelectTab(Tab tab)
        {
            _tabView.activeTab = tab;
            _navigationScope.EnterLevel(tab.tabHeader);
        }

        /// <summary>
        ///     指定した音量スライダーの値を範囲内で増減する。
        /// </summary>
        /// <param name="slider"> 操作対象の音量スライダー。 </param>
        /// <param name="delta"> 音量へ加算する値。 </param>
        private static void ChangeVolume(SliderInt slider, int delta)
        {
            int minimum = Mathf.Min(slider.lowValue, slider.highValue);
            int maximum = Mathf.Max(slider.lowValue, slider.highValue);
            slider.value = Mathf.Clamp(slider.value + delta, minimum, maximum);
        }

        /// <summary>
        ///     現在表示中のタブを既知のタブへ正規化する。
        /// </summary>
        private void NormalizeActiveTab()
        {
            if (ReferenceEquals(_tabView.activeTab, _dataResetTab))
            {
                return;
            }

            _tabView.activeTab = _volumeSettingsTab;
        }
    }
}
