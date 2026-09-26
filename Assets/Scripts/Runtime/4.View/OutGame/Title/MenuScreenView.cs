using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Localization;
using LitMotion;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     メニュー画面の View クラス。
    /// </summary>
    public class MenuScreenView : ScreenViewBase
    {
        /// <summary>
        ///    メニュー画面の View を初期化します。
        /// </summary>
        public MenuScreenView(
            VisualElement rootElement,
            OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            Initialize(rootElement);
            RegisterButtonCallbacks();
            RegisterLocalizedTexts();
        }

        /// <summary>
        ///   メニュー画面の View のリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            _slideMotionHandle.TryCancel();
            UnbindOptionInput();
            UnregisterButtonCallbacks();
            foreach (LocalizedElementText localizedText in _localizedTexts)
            {
                localizedText.Dispose();
            }

            base.Dispose();
        }

        /// <summary>
        ///     コントローラーのOptionsボタンでメニュー画面を閉じられるようにします。
        /// </summary>
        /// <param name="playerInputView"> 入力Viewです。nullの場合は購読しません。 </param>
        public void BindOptionInput(PlayerInputView playerInputView)
        {
            UnbindOptionInput();

            if (playerInputView == null)
            {
                return;
            }

            _playerInputView = playerInputView;
            _playerInputView.OnOptionInput += OnOptionInput;
        }

        /// <summary>
        ///     ウィンドウを右側の画面外からスライドインさせつつ表示します。
        /// </summary>
        public override ValueTask Show(CancellationToken cancellationToken = default)
        {
            _slideMotionHandle.TryComplete();
            SetWindowTranslateX(SLIDE_OFFSET_X);

            _slideMotionHandle = LMotion.Create(SLIDE_OFFSET_X, 0f, SLIDE_DURATION)
                .WithEase(SLIDE_EASE)
                .Bind(this, static (x, state) => state.SetWindowTranslateX(x));

            return base.Show(cancellationToken);
        }

        /// <summary>
        ///     ウィンドウを右側の画面外へスライドアウトさせつつ非表示にします。
        /// </summary>
        public override ValueTask Hide(CancellationToken cancellationToken = default)
        {
            _slideMotionHandle.TryComplete();

            _slideMotionHandle = LMotion.Create(0f, SLIDE_OFFSET_X, SLIDE_DURATION)
                .WithEase(SLIDE_EASE)
                .Bind(this, static (x, state) => state.SetWindowTranslateX(x));

            return base.Hide(cancellationToken);
        }

        /// <inheritdoc />
        protected override VisualElement InitialFocusElement => _bgmVolumeSlider;

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        private const string BGM_VOLUME_SLIDER_NAME = "BGMVolumeSlider";
        private const string SOUND_EFFECT_VOLUME_SLIDER_NAME = "SEVolumeSlider";
        private const string LANGUAGE_PREV_BUTTON_NAME = "LanguagePrevButton";
        private const string LANGUAGE_NEXT_BUTTON_NAME = "LanguageNextButton";
        private const string CREDIT_BUTTON_NAME = "CreditButton";
        private const string DATA_RESET_BUTTON_NAME = "DataResetButton";
        private const string BACK_BUTTON_NAME = "BackButton";
        private const string BACK_GROUND_NAME = "BackGround";
        private const string DATA_RESET_DIALOG_NAME = "DataResetConfirmDialog";
        private const string DATA_RESET_CONFIRM_BUTTON_NAME = "DataResetConfirmButton";
        private const string DATA_RESET_CANCEL_BUTTON_NAME = "DataResetCancelButton";
        private const string WINDOW_ROOT_NAME = "Root";
        private const string DATA_RESET_MESSAGE_NAME = "DataResetMessage";
        private const string UI_COMMON_TABLE = "UICommon";

        /// <summary> ウィンドウのスライドインにかかる時間(秒)。 </summary>
        private const float SLIDE_DURATION = 0.2f;
        /// <summary> ウィンドウのスライド開始位置(画面左外側へのオフセット、px)。 </summary>
        private const float SLIDE_OFFSET_X = -250f;
        /// <summary> ウィンドウのスライドのイージング。 </summary>
        private const Ease SLIDE_EASE = Ease.OutCirc;

        private SliderInt _bgmVolumeSlider;
        private SliderInt _soundEffectVolumeSlider;
        private Button _languagePrevButton;
        private Button _languageNextButton;
        private Button _creditButton;
        private Button _dataResetButton;

        private Button _backButton;
        private IDisposable _backButtonActivation;
        private VisualElement _backGround;
        private VisualElement _windowRoot;

        private VisualElement _dataResetDialog;
        private Label _dataResetMessageLabel;
        private Button _dataResetConfirmButton;
        private Button _dataResetCancelButton;

        private MotionHandle _slideMotionHandle;
        private LocalizedElementText[] _localizedTexts = Array.Empty<LocalizedElementText>();
        private PlayerInputView _playerInputView;

        private IDisposable _creditButtonActivation;
        private IDisposable _dataResetButtonActivation;
        private IDisposable _dataResetConfirmButtonActivation;
        private IDisposable _dataResetCancelButtonActivation;
        private IManipulator _creditButtonPulse;
        private IManipulator _dataResetButtonPulse;

        /// <summary>
        ///     メニュー画面の UI 要素を初期化します。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void Initialize(VisualElement rootElement)
        {
            if (rootElement == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(MenuScreenView)}: Root VisualElementがnullです。");
#endif
                return;
            }


            _bgmVolumeSlider = rootElement.Q<SliderInt>(BGM_VOLUME_SLIDER_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {BGM_VOLUME_SLIDER_NAME}が見つかりません。");
            _soundEffectVolumeSlider = rootElement.Q<SliderInt>(SOUND_EFFECT_VOLUME_SLIDER_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {SOUND_EFFECT_VOLUME_SLIDER_NAME}が見つかりません。");
            _languagePrevButton = rootElement.Q<Button>(LANGUAGE_PREV_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {LANGUAGE_PREV_BUTTON_NAME}が見つかりません。");
            _languageNextButton = rootElement.Q<Button>(LANGUAGE_NEXT_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {LANGUAGE_NEXT_BUTTON_NAME}が見つかりません。");
            _creditButton = rootElement.Q<Button>(CREDIT_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {CREDIT_BUTTON_NAME}が見つかりません。");
            _dataResetButton = rootElement.Q<Button>(DATA_RESET_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {DATA_RESET_BUTTON_NAME}が見つかりません。");
            _backButton = rootElement.Q<Button>(BACK_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {BACK_BUTTON_NAME}が見つかりません。");
            _backGround = rootElement.Q<VisualElement>(BACK_GROUND_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {BACK_GROUND_NAME}が見つかりません。");
            _windowRoot = rootElement.Q<VisualElement>(WINDOW_ROOT_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {WINDOW_ROOT_NAME}が見つかりません。");
            _dataResetDialog = rootElement.Q<VisualElement>(DATA_RESET_DIALOG_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {DATA_RESET_DIALOG_NAME}が見つかりません。");
            _dataResetMessageLabel = rootElement.Q<Label>(DATA_RESET_MESSAGE_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {DATA_RESET_MESSAGE_NAME}が見つかりません。");
            _dataResetConfirmButton = rootElement.Q<Button>(DATA_RESET_CONFIRM_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {DATA_RESET_CONFIRM_BUTTON_NAME}が見つかりません。");
            _dataResetCancelButton = rootElement.Q<Button>(DATA_RESET_CANCEL_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(MenuScreenView)}: {DATA_RESET_CANCEL_BUTTON_NAME}が見つかりません。");
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            _bgmVolumeSlider.MakeNavigable();
            _soundEffectVolumeSlider.MakeNavigable();
            _creditButton.MakeNavigable();
            _dataResetButton.MakeNavigable();
            _dataResetConfirmButton.MakeNavigable();
            _dataResetCancelButton.MakeNavigable();
            _creditButtonActivation = _creditButton.RegisterActivation(OnCreditButtonClicked);
            _dataResetButtonActivation = _dataResetButton.RegisterActivation(OnDataResetButtonClicked);
            _creditButtonPulse = _creditButton.EnableButtonPulseAnimation();
            _dataResetButtonPulse = _dataResetButton.EnableButtonPulseAnimation();

            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);
            _backGround.RegisterCallback<PointerDownEvent>(OnPointDownEvent);
            // Button.clicked はコントローラーの決定操作(NavigationSubmitEvent)には反応しないため、
            // MakeNavigable() とあわせて RegisterActivation() でクリックと決定操作を1つの処理へ統合する。
            _dataResetConfirmButtonActivation = _dataResetConfirmButton.RegisterActivation(OnDataResetConfirmButtonClicked);
            _dataResetCancelButtonActivation = _dataResetCancelButton.RegisterActivation(OnDataResetCancelButtonClicked);
            _dataResetDialog.RegisterCallback<PointerDownEvent>(OnDataResetDialogPointerDown);
            _dataResetDialog.RegisterCallback<NavigationCancelEvent>(
                HandleDataResetDialogNavigationCancelHandler, TrickleDown.TrickleDown);

            RootElement.RegisterCallback<NavigationMoveEvent>(
                HandleMenuNavigationMoveHandler, TrickleDown.TrickleDown);
            // ScreenViewBase 側のキャンセル処理(フォーカス依存でバブリングする)より
            // 先に確実に捕まえるため、画面ルートにもトリクルダウンで購読しておく。
            RootElement.RegisterCallback<NavigationCancelEvent>(
                HandleRootNavigationCancelHandler, TrickleDown.TrickleDown);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _creditButtonActivation?.Dispose();
            _dataResetButtonActivation?.Dispose();
            _creditButton.RemoveManipulator(_creditButtonPulse);
            _dataResetButton.RemoveManipulator(_dataResetButtonPulse);
            _backButtonActivation?.Dispose();
            _backGround.UnregisterCallback<PointerDownEvent>(OnPointDownEvent);
            _dataResetConfirmButtonActivation?.Dispose();
            _dataResetCancelButtonActivation?.Dispose();
            _dataResetDialog.UnregisterCallback<PointerDownEvent>(OnDataResetDialogPointerDown);
            _dataResetDialog.UnregisterCallback<NavigationCancelEvent>(
                HandleDataResetDialogNavigationCancelHandler, TrickleDown.TrickleDown);
            RootElement.UnregisterCallback<NavigationMoveEvent>(
                HandleMenuNavigationMoveHandler, TrickleDown.TrickleDown);
            RootElement.UnregisterCallback<NavigationCancelEvent>(
                HandleRootNavigationCancelHandler, TrickleDown.TrickleDown);
        }

        /// <summary>
        ///     音量・言語・下段ボタンを配置順に移動し、左右の音量調整はスライダーへ委ねます。
        /// </summary>
        /// <param name="navigationEvent"> ナビゲーション移動イベントです。 </param>
        private void HandleMenuNavigationMoveHandler(NavigationMoveEvent navigationEvent)
        {
            if (!IsShowCompleted || RootElement.panel == null || !RootElement.enabledInHierarchy
                || _windowRoot.resolvedStyle.display == DisplayStyle.None
                || _dataResetDialog.resolvedStyle.display != DisplayStyle.None)
            {
                return;
            }

            VisualElement source = navigationEvent.target as VisualElement;
            if (source == null || !_windowRoot.Contains(source) || !source.enabledInHierarchy)
            {
                return;
            }

            VisualElement destination = null;
            // スライダー内部の入力要素へフォーカスしている場合も、音量行として扱う。
            while (source != null && source != _windowRoot)
            {
                destination = GetMenuNavigationDestination(source, navigationEvent.direction);
                if (destination != null)
                {
                    break;
                }

                source = source.parent;
            }

            if (destination == null || !destination.enabledInHierarchy
                || !destination.canGrabFocus || destination.panel != RootElement.panel
                || destination.resolvedStyle.display == DisplayStyle.None
                || destination.resolvedStyle.visibility != Visibility.Visible)
            {
                return;
            }

            // 標準の位置判定やスライダーによる二重処理を抑え、指定先だけへ移動する。
            navigationEvent.StopPropagation();
            RootElement.panel.focusController?.IgnoreEvent(navigationEvent);
            destination.Focus();
        }

        /// <summary>
        ///     タイトル設定内の移動先を返します。音量の左右入力など、標準処理へ委ねる場合はnullです。
        /// </summary>
        /// <param name="source"> 移動元の要素です。 </param>
        /// <param name="direction"> 入力された移動方向です。 </param>
        private VisualElement GetMenuNavigationDestination(
            VisualElement source,
            NavigationMoveEvent.Direction direction)
        {
            bool isDown = direction == NavigationMoveEvent.Direction.Down;
            if (isDown || direction == NavigationMoveEvent.Direction.Up)
            {
                if (source == _bgmVolumeSlider)
                {
                    return isDown ? _soundEffectVolumeSlider : _bgmVolumeSlider;
                }
                if (source == _soundEffectVolumeSlider)
                {
                    return isDown ? _languagePrevButton : _bgmVolumeSlider;
                }
                if (source == _languagePrevButton)
                {
                    return isDown ? _dataResetButton : _soundEffectVolumeSlider;
                }
                if (source == _languageNextButton)
                {
                    return isDown ? _creditButton : _soundEffectVolumeSlider;
                }
                if (source == _dataResetButton)
                {
                    return isDown ? _dataResetButton : _languagePrevButton;
                }
                if (source == _creditButton)
                {
                    return isDown ? _creditButton : _languageNextButton;
                }
            }
            else if (direction == NavigationMoveEvent.Direction.Left
                || direction == NavigationMoveEvent.Direction.Right)
            {
                bool isRight = direction == NavigationMoveEvent.Direction.Right;
                if (source == _languagePrevButton || source == _languageNextButton)
                {
                    return isRight ? _languageNextButton : _languagePrevButton;
                }
                if (source == _dataResetButton || source == _creditButton)
                {
                    return isRight ? _creditButton : _dataResetButton;
                }
            }

            return null;
        }

        /// <summary>
        ///     Optionsボタンの購読を解除します。
        /// </summary>
        private void UnbindOptionInput()
        {
            if (_playerInputView == null)
            {
                return;
            }

            _playerInputView.OnOptionInput -= OnOptionInput;
            _playerInputView = null;
        }

        /// <summary>
        ///     コントローラーのOptionsボタンでメニュー画面を閉じる。
        ///     <para>
        ///         メニュー画面表示中はタイトル画面側の操作が禁止されるため、
        ///         開く操作と閉じる操作をそれぞれの画面で受け持つことで
        ///         Optionsボタンの開閉トグルを成立させる。
        ///     </para>
        /// </summary>
        /// <param name="inputContext"> 入力情報。 </param>
        private void OnOptionInput(InputContext<float> inputContext)
        {
            // 押した瞬間のみ反応させる。離した際の通知では閉じない。
            // フェードイン中は同一入力で開いた直後の可能性があるため、表示完了まで受け付けない。
            if (!IsShowCompleted || !_backButton.enabledInHierarchy
                || inputContext.Phase != UnityEngine.InputSystem.InputActionPhase.Performed)
            {
                return;
            }

            // データリセット確認ダイアログ表示中は、そちらの操作を優先する。
            if (_dataResetDialog.resolvedStyle.display != DisplayStyle.None)
            {
                return;
            }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     クレジットボタンがクリックされたときの処理。
        /// </summary>
        private void OnCreditButtonClicked()
        {
            OutGameUIEvent.OnShowCreditScreen?.Invoke();
        }

        /// <summary>
        ///     データリセットボタンがクリックされたときの処理。
        ///     即座にリセットせず、確認ダイアログを表示する。
        /// </summary>
        private void OnDataResetButtonClicked()
        {
            _dataResetDialog.style.display = DisplayStyle.Flex;
            _windowRoot.style.display = DisplayStyle.None;
            _dataResetCancelButton.FocusDeferred();
        }

        /// <summary>
        ///     データリセット確認ダイアログの「リセット」が押されたときの処理。
        /// </summary>
        private void OnDataResetConfirmButtonClicked()
        {
            _dataResetDialog.style.display = DisplayStyle.None;
            _windowRoot.style.display = DisplayStyle.Flex;
            SetInitialFocusElement(_dataResetButton);
            RestoreFocus();
            OutGameUIEvent.OnDataResetButtonClicked?.Invoke();
        }

        /// <summary>
        ///     データリセット確認ダイアログの「キャンセル」が押されたときの処理。
        /// </summary>
        private void OnDataResetCancelButtonClicked()
        {
            _dataResetDialog.style.display = DisplayStyle.None;
            _windowRoot.style.display = DisplayStyle.Flex;
            SetInitialFocusElement(_dataResetButton);
            RestoreFocus();
        }

        /// <summary>
        ///     データリセット確認ダイアログの外側(背景)が押されたときの処理。
        ///     キャンセルと同じ扱いでダイアログを閉じる。
        /// </summary>
        /// <param name="evt"></param>
        private void OnDataResetDialogPointerDown(PointerDownEvent evt)
        {
            // ダイアログ内の子要素が押された場合は処理を行わない
            if (evt.target != evt.currentTarget) { return; }

            OnDataResetCancelButtonClicked();
        }

        /// <summary>
        ///     コントローラーのキャンセル操作をキャンセルボタンと同じ動作に変換する。
        ///     画面全体のキャンセル処理(戻る)より先に処理するため、トリクルダウンで購読する。
        /// </summary>
        /// <param name="evt"> ナビゲーションキャンセルイベント。 </param>
        private void HandleDataResetDialogNavigationCancelHandler(NavigationCancelEvent evt)
        {
            if (_dataResetDialog.resolvedStyle.display == DisplayStyle.None)
            {
                return;
            }

            OnDataResetCancelButtonClicked();
            evt.StopPropagation();
        }

        /// <summary>
        ///     戻るボタンが作動したときの処理。
        /// </summary>
        private void HandleBackButtonActivationHandler()
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     メニュー画面ルートでのキャンセル操作を処理する。
        ///     <para>
        ///         <see cref="ScreenViewBase"/> 側のキャンセル処理はフォーカス中の要素から
        ///         バブリングしたイベントに依存するため、フォーカスが正しく当たっていない
        ///         場合に反応しないことがある。ここではフォーカス状態によらず確実に
        ///         画面を閉じられるよう、ルート要素で直接キャンセル操作を受け取る。
        ///     </para>
        /// </summary>
        /// <param name="evt"> ナビゲーションキャンセルイベント。 </param>
        private void HandleRootNavigationCancelHandler(NavigationCancelEvent evt)
        {
            if (RootElement.resolvedStyle.display == DisplayStyle.None)
            {
                return;
            }

            // データリセット確認ダイアログ表示中は、そちら自身のキャンセル処理に任せる。
            if (_dataResetDialog.resolvedStyle.display != DisplayStyle.None)
            {
                return;
            }

            OutGameUIEvent.OnScreenClosed?.Invoke();
            evt.StopPropagation();
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
        ///     ウィンドウの水平方向の移動量を書き込みます。
        /// </summary>
        /// <param name="x"> 右方向への移動量(px)。 </param>
        private void SetWindowTranslateX(float x)
        {
            _windowRoot.style.translate = new Translate(x, 0);
        }

        /// <summary>
        ///     静的なボタン・ラベルのテキストをUICommonテーブルへ連携する。
        /// </summary>
        private void RegisterLocalizedTexts()
        {
            Label soundEffectHeading = RootElement.Q<Label>("SoundEffectHeading");
            Label languageHeading = RootElement.Q<Label>("LanguageHeading");
            Label dataResetWarning = RootElement.Q<Label>("DataResetWarning");
            _localizedTexts = new[]
            {
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.title.menu.sound_effect", text => soundEffectHeading.text = text, "効果音"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.language", text => languageHeading.text = text, "言語"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.title.menu.data_reset_warning", text => dataResetWarning.text = text, "※削除するとデータは戻せません。"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.title.menu.data_reset", text => _dataResetButton.text = text),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.title.menu.credit", text => _creditButton.text = text),
                new LocalizedElementText(
                    UI_COMMON_TABLE,
                    "ui.title.menu.data_reset_confirm_message",
                    text => _dataResetMessageLabel.text = text),
                new LocalizedElementText(
                    UI_COMMON_TABLE,
                    "ui.title.menu.data_reset_confirm",
                    text => _dataResetConfirmButton.text = text),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.close", text => _dataResetCancelButton.text = text),
            };
        }
    }
}
