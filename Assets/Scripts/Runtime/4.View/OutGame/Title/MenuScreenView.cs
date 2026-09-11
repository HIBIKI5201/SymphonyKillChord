using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
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
        }

        /// <summary>
        ///   メニュー画面の View のリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            _slideMotionHandle.TryCancel();
            UnregisterButtonCallbacks();
            base.Dispose();
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
        private const string CREDIT_BUTTON_NAME = "CreditButton";
        private const string DATA_RESET_BUTTON_NAME = "DataResetButton";
        private const string BACK_BUTTON_NAME = "BackButton";
        private const string BACK_GROUND_NAME = "BackGround";
        private const string DATA_RESET_DIALOG_NAME = "DataResetConfirmDialog";
        private const string DATA_RESET_CONFIRM_BUTTON_NAME = "DataResetConfirmButton";
        private const string DATA_RESET_CANCEL_BUTTON_NAME = "DataResetCancelButton";
        private const string WINDOW_ROOT_NAME = "Root";

        /// <summary> ウィンドウのスライドインにかかる時間(秒)。 </summary>
        private const float SLIDE_DURATION = 0.2f;
        /// <summary> ウィンドウのスライド開始位置(画面左外側へのオフセット、px)。 </summary>
        private const float SLIDE_OFFSET_X = -250f;
        /// <summary> ウィンドウのスライドのイージング。 </summary>
        private const Ease SLIDE_EASE = Ease.OutCirc;

        private SliderInt _bgmVolumeSlider;
        private Button _creditButton;
        private Button _dataResetButton;

        private Button _backButton;
        private IDisposable _backButtonActivation;
        private VisualElement _backGround;
        private VisualElement _windowRoot;

        private VisualElement _dataResetDialog;
        private Button _dataResetConfirmButton;
        private Button _dataResetCancelButton;

        private MotionHandle _slideMotionHandle;

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
            _creditButton.MakeNavigable();
            _dataResetButton.MakeNavigable();
            _dataResetConfirmButton.MakeNavigable();
            _dataResetCancelButton.MakeNavigable();
            _creditButton.clicked += OnCreditButtonClicked;
            _dataResetButton.clicked += OnDataResetButtonClicked;

            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);
            _backGround.RegisterCallback<PointerDownEvent>(OnPointDownEvent);
            _dataResetConfirmButton.clicked += OnDataResetConfirmButtonClicked;
            _dataResetCancelButton.clicked += OnDataResetCancelButtonClicked;
            _dataResetDialog.RegisterCallback<PointerDownEvent>(OnDataResetDialogPointerDown);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _creditButton.clicked -= OnCreditButtonClicked;
            _dataResetButton.clicked -= OnDataResetButtonClicked;
            _backButtonActivation?.Dispose();
            _backGround.UnregisterCallback<PointerDownEvent>(OnPointDownEvent);
            _dataResetConfirmButton.clicked -= OnDataResetConfirmButtonClicked;
            _dataResetCancelButton.clicked -= OnDataResetCancelButtonClicked;
            _dataResetDialog.UnregisterCallback<PointerDownEvent>(OnDataResetDialogPointerDown);
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
        }

        /// <summary>
        ///     データリセット確認ダイアログの「リセット」が押されたときの処理。
        /// </summary>
        private void OnDataResetConfirmButtonClicked()
        {
            _dataResetDialog.style.display = DisplayStyle.None;
            _windowRoot.style.display = DisplayStyle.Flex;
            OutGameUIEvent.OnDataResetButtonClicked?.Invoke();
        }

        /// <summary>
        ///     データリセット確認ダイアログの「キャンセル」が押されたときの処理。
        /// </summary>
        private void OnDataResetCancelButtonClicked()
        {
            _dataResetDialog.style.display = DisplayStyle.None;
            _windowRoot.style.display = DisplayStyle.Flex;
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
        ///     戻るボタンが作動したときの処理。
        /// </summary>
        private void HandleBackButtonActivationHandler()
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
        ///     ウィンドウの水平方向の移動量を書き込みます。
        /// </summary>
        /// <param name="x"> 右方向への移動量(px)。 </param>
        private void SetWindowTranslateX(float x)
        {
            _windowRoot.style.translate = new Translate(x, 0);
        }
    }
}
