using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Localization;
using LitMotion;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     設定画面 View です。
    /// </summary>
    public sealed class SettingScreenView : ScreenViewBase
    {

        /// <summary> View を初期化します。 </summary>
        /// <param name="rootElement"> 設定画面のルート要素です。 </param>
        /// <param name="outGameUIEvent"> アウトゲームのUIイベントです。 </param>
        /// <param name="playerInputView"> Cancel入力を購読する入力Viewです。nullの場合はCancel入力での操作を無効にします。 </param>
        public SettingScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent, PlayerInputView playerInputView)
            : base(rootElement, outGameUIEvent)
        {
            _backButton = rootElement.Q<Button>(BACKBUTTON_NAME)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(SettingScreenView)}] {BACKBUTTON_NAME} が見つかりませんでした。");

            _returnToTitleButton = Require<Button>(rootElement, RETURN_TO_TITLE_BUTTON_NAME);
            _environmentSettingButton = Require<Button>(rootElement, ENVIRONMENT_SETTING_BUTTON_NAME);
            _cancelReturnToTitleButton = Require<Button>(rootElement, CANCEL_RETURN_BUTTON_NAME);
            _confirmReturnToTitleButton = Require<Button>(rootElement, CONFIRM_RETURN_BUTTON_NAME);
            _returnToTitleDialog = Require<VisualElement>(rootElement, RETURN_TO_TITLE_DIALOG_NAME);
            _outsideClickArea = Require<VisualElement>(rootElement, OUTSIDE_CLICK_AREA_NAME);
            _backGround = Require<VisualElement>(rootElement, BACK_GROUND_NAME);
            _cancelTarget = Require<VisualElement>(rootElement, CANCEL_TARGET_NAME);

            // UI Toolkit の NavigationCancelEvent はフォーカス状態に依存し不安定なため、
            // OnOptionInput と同様に PlayerInputView の Cancel アクションを直接購読する。
            _playerInputView = playerInputView;
            if (_playerInputView == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(SettingScreenView)}] PlayerInputViewを取得できませんでした。"
                    + "コントローラー/キーボードのCancel入力での設定画面操作は無効になります。");
            }

            RegisterButtonCallback();
            ResetReturnToTitleDialog();
            RegisterLocalizedButtonTexts();
        }

        /// <summary>
        ///     サブパネル表示中のキャンセル操作を横取りする処理です。
        ///     trueを返した場合、設定画面は閉じずにこの処理の結果に委ねます。
        /// </summary>
        public Func<bool> TryNavigateBack { get; set; }

        /// <summary>
        ///     確認状態を初期化し、ウィンドウを左からスライドインさせつつ設定画面を表示する。
        /// </summary>
        public override ValueTask Show(CancellationToken cancellationToken = default)
        {
            _isActive = true;
            _shownFrame = UnityEngine.Time.frameCount;
            ResetReturnToTitleDialog();

            // 背面のホーム画面は表示されたままのため、フォーカスを設定画面内へ閉じ込める。
            _screenNavigationScope.Activate(RootElement);

            _slideMotionHandle.TryComplete();
            SetWindowTranslateX(SLIDE_OFFSET_X);
            _slideMotionHandle = LMotion.Create(SLIDE_OFFSET_X, 0f, SLIDE_DURATION)
                .WithEase(SLIDE_EASE)
                .Bind(this, static (x, state) => state.SetWindowTranslateX(x));

            return base.Show(cancellationToken);
        }

        /// <summary>
        ///     フォーカスの閉じ込めを解除して設定画面を閉じる。
        /// </summary>
        public override ValueTask Hide(CancellationToken cancellationToken = default)
        {
            _isActive = false;
            _dialogNavigationScope.Deactivate();
            _screenNavigationScope.Deactivate();

            _slideMotionHandle.TryComplete();
            _slideMotionHandle = LMotion.Create(0f, SLIDE_OFFSET_X, SLIDE_DURATION)
                .WithEase(SLIDE_EASE)
                .Bind(this, static (x, state) => state.SetWindowTranslateX(x));

            return base.Hide(cancellationToken);
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public override void Dispose()
        {
            _slideMotionHandle.TryCancel();
            base.Dispose();
            UnregisterButtonCallback();
            foreach (LocalizedElementText localizedText in _localizedButtonTexts)
            {
                localizedText.Dispose();
            }
        }

        /// <summary>
        ///     ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallback()
        {
            _backButtonPreset = _backButton.ApplyBasicButtonPreset(HandleBackButtonActivationHandler);
            _cancelTargetActivation = _cancelTarget.RegisterActivation(HandleBackButtonActivationHandler);
            _returnToTitleButtonPreset = _returnToTitleButton.ApplyBasicButtonPreset(ShowReturnToTitleDialog);

            // MakeNavigable() で navigable クラスを付与し、他UIと同じ黄色のフォーカス枠を出す。
            // Button.clicked はコントローラーの決定操作(NavigationSubmitEvent)には反応しないため、
            // RegisterActivation() でクリックと決定操作を1つの処理へ統合する。
            _cancelReturnToTitleButton.MakeNavigable();
            _confirmReturnToTitleButton.MakeNavigable();
            _cancelReturnToTitleButtonActivation =
                _cancelReturnToTitleButton.RegisterActivation(HideReturnToTitleDialog);
            _confirmReturnToTitleButtonActivation =
                _confirmReturnToTitleButton.RegisterActivation(RequestReturnToTitle);
            OutGameUIEvent.OnReturnToTitleRequestCompleted += HandleReturnToTitleRequestCompleted;
            _outsideClickArea.RegisterCallback<PointerDownEvent>(HandleOutsidePointerDown);

            if (_playerInputView != null)
            {
                _playerInputView.OnCancelInput += HandleCancelInputHandler;
            }
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButtonPreset?.Dispose();
            _cancelTargetActivation?.Dispose();
            _returnToTitleButtonPreset?.Dispose();
            _cancelReturnToTitleButtonActivation?.Dispose();
            _confirmReturnToTitleButtonActivation?.Dispose();
            OutGameUIEvent.OnReturnToTitleRequestCompleted -= HandleReturnToTitleRequestCompleted;
            _outsideClickArea.UnregisterCallback<PointerDownEvent>(HandleOutsidePointerDown);

            if (_playerInputView != null)
            {
                _playerInputView.OnCancelInput -= HandleCancelInputHandler;
            }
        }

        /// <summary>
        ///     画面を閉じるボタンが作動したときの処理です。
        /// </summary>
        private void HandleBackButtonActivationHandler()
        {
            if (_isReturnToTitleDialogVisible || _isReturnToTitleRequested)
            {
                return;
            }

            // サブパネル表示中はメニューへ戻すだけに留める。
            if (TryNavigateBack?.Invoke() == true)
            {
                return;
            }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     コントローラー/キーボードのCancel入力を処理します。
        /// </summary>
        private void HandleCancelInputHandler(InputContext<float> inputContext)
        {
            if (!_isActive || inputContext.Phase != InputActionPhase.Performed)
            {
                return;
            }

            // EscはOptionとCancelの両方に割り当てられているため、開いた入力で即座に閉じない。
            if (_shownFrame == UnityEngine.Time.frameCount)
            {
                return;
            }

            // 確認ダイアログ表示中のキャンセル操作は、ダイアログを閉じる動作に割り当てる。
            if (_isReturnToTitleDialogVisible)
            {
                HideReturnToTitleDialog();
                return;
            }

            HandleBackButtonActivationHandler();
        }

        private const string BACKBUTTON_NAME = "CloseButton";
        private const string ENVIRONMENT_SETTING_BUTTON_NAME = "EnvironmentSettingButton";
        private const string RETURN_TO_TITLE_BUTTON_NAME = "ReturnToTitleButton";
        private const string CANCEL_RETURN_BUTTON_NAME = "CancelReturnToTitleButton";
        private const string CONFIRM_RETURN_BUTTON_NAME = "ConfirmReturnToTitleButton";
        private const string RETURN_TO_TITLE_DIALOG_NAME = "ReturnToTitleDialog";
        private const string OUTSIDE_CLICK_AREA_NAME = "Root";
        private const string BACK_GROUND_NAME = "BackGround";
        private const string CANCEL_TARGET_NAME = "CancelTarget";
        private const string UI_COMMON_TABLE = "UICommon";

        /// <summary> ウィンドウのスライドインにかかる時間(秒)。 </summary>
        private const float SLIDE_DURATION = 0.2f;
        /// <summary> ウィンドウのスライド開始位置(画面左外側へのオフセット、px)。 </summary>
        private const float SLIDE_OFFSET_X = -190f;
        /// <summary> ウィンドウのスライドのイージング。 </summary>
        private const Ease SLIDE_EASE = Ease.OutCirc;

        /// <inheritdoc />
        /// <remarks>
        ///     Cancel入力はPlayerInputView.OnCancelInputの直接購読で処理するため、
        ///     基底クラスのNavigationCancelEvent経路は使わず二重処理を避ける。
        /// </remarks>
        protected override VisualElement CancelTargetElement => null;

        /// <inheritdoc />
        protected override VisualElement InitialFocusElement => _environmentSettingButton;

        /// <summary> 設定画面表示中、フォーカスを画面内へ閉じ込める。 </summary>
        private readonly ModalNavigationScope _screenNavigationScope = new();
        /// <summary> 確認ダイアログ表示中、フォーカスをダイアログ内へ閉じ込める。 </summary>
        private readonly ModalNavigationScope _dialogNavigationScope = new();

        private readonly Button _backButton;
        private readonly Button _environmentSettingButton;
        private readonly Button _returnToTitleButton;
        private readonly Button _cancelReturnToTitleButton;
        private readonly Button _confirmReturnToTitleButton;
        private readonly VisualElement _returnToTitleDialog;
        private readonly VisualElement _outsideClickArea;
        private readonly VisualElement _backGround;
        private readonly VisualElement _cancelTarget;
        private readonly PlayerInputView _playerInputView;
        private IDisposable _backButtonPreset;
        private IDisposable _returnToTitleButtonPreset;
        private IDisposable _cancelTargetActivation;
        private IDisposable _cancelReturnToTitleButtonActivation;
        private IDisposable _confirmReturnToTitleButtonActivation;
        private bool _isReturnToTitleDialogVisible;
        private bool _isReturnToTitleRequested;
        private bool _isActive;
        private int _shownFrame = -1;
        private MotionHandle _slideMotionHandle;
        private LocalizedElementText[] _localizedButtonTexts = Array.Empty<LocalizedElementText>();

        /// <summary>
        ///     設定ウィンドウ外が押された場合に設定画面を閉じる。
        /// </summary>
        private void HandleOutsidePointerDown(PointerDownEvent pointerEvent)
        {
            if (!ReferenceEquals(pointerEvent.target, _outsideClickArea)
                || _isReturnToTitleDialogVisible
                || _isReturnToTitleRequested)
            {
                return;
            }

            // サブパネル表示中はメニューへ戻すだけに留める。
            if (TryNavigateBack?.Invoke() == true)
            {
                return;
            }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     タイトル復帰の確認ダイアログを表示する。
        /// </summary>
        private void ShowReturnToTitleDialog()
        {
            if (_isReturnToTitleRequested)
            {
                return;
            }

            _isReturnToTitleDialogVisible = true;
            _returnToTitleDialog.style.display = DisplayStyle.Flex;
            _confirmReturnToTitleButton.SetEnabled(true);

            // 背面の設定項目へフォーカスが抜けないようにする。
            _dialogNavigationScope.Activate(_returnToTitleDialog);

            // Activate() はダイアログ内で最初に見つかった要素へ遅延フォーカスするため、
            // 並び順の先頭にある確定ボタンが既定になってしまう。誤操作でタイトルへ戻らないよう、
            // 後から予約して必ずキャンセルボタンを既定のフォーカス先にする。
            _cancelReturnToTitleButton.FocusDeferred();
        }

        /// <summary>
        ///     タイトル復帰の確認ダイアログを閉じる。
        /// </summary>
        private void HideReturnToTitleDialog()
        {
            if (_isReturnToTitleRequested)
            {
                return;
            }

            _isReturnToTitleDialogVisible = false;
            _dialogNavigationScope.Deactivate();
            _returnToTitleDialog.style.display = DisplayStyle.None;
            _returnToTitleButton.Focus();
        }

        /// <summary>
        ///     タイトル画面への復帰を要求する。
        /// </summary>
        private void RequestReturnToTitle()
        {
            if (!_isReturnToTitleDialogVisible || _isReturnToTitleRequested)
            {
                return;
            }

            _isReturnToTitleRequested = true;
            _confirmReturnToTitleButton.SetEnabled(false);
            _cancelReturnToTitleButton.SetEnabled(false);
            OutGameUIEvent.OnReturnToTitleRequested?.Invoke();
        }

        /// <summary>
        ///     タイトル復帰失敗時に確認UIを再操作可能へ戻す。
        /// </summary>
        private void HandleReturnToTitleRequestCompleted(bool isSucceeded)
        {
            if (isSucceeded)
            {
                return;
            }

            _isReturnToTitleRequested = false;
            _confirmReturnToTitleButton.SetEnabled(true);
            _cancelReturnToTitleButton.SetEnabled(true);
            _confirmReturnToTitleButton.Focus();
        }

        /// <summary>
        ///     タイトル復帰の確認状態と操作可否を初期状態へ戻す。
        /// </summary>
        private void ResetReturnToTitleDialog()
        {
            _isReturnToTitleDialogVisible = false;
            _isReturnToTitleRequested = false;
            _returnToTitleDialog.style.display = DisplayStyle.None;
            _confirmReturnToTitleButton.SetEnabled(true);
            _cancelReturnToTitleButton.SetEnabled(true);
        }

        /// <summary>
        ///     ウィンドウの水平方向の移動量を書き込みます。
        /// </summary>
        /// <param name="x"> 右方向への移動量(px)。 </param>
        private void SetWindowTranslateX(float x)
        {
            _backGround.style.translate = new Translate(x, 0);
        }

        /// <summary>
        ///     必須UI要素を取得する。
        /// </summary>
        private static T Require<T>(VisualElement rootElement, string elementName)
            where T : VisualElement
        {
            return rootElement.Q<T>(elementName)
                ?? throw new System.InvalidOperationException(
                    $"[{nameof(SettingScreenView)}] {elementName} が見つかりませんでした。");
        }

        /// <summary>
        ///     静的なボタン・見出し・確認文をUICommonテーブルへ連携する。
        /// </summary>
        private void RegisterLocalizedButtonTexts()
        {
            // 見出しのラベルを取得する。
            Label titleLabel = Require<Label>(RootElement, "Title");
            Label audioPanelTitle = Require<Label>(RootElement, "AudioPanelTitle");
            Label environmentPanelTitle = Require<Label>(RootElement, "EnvironmentPanelTitle");
            Label screenModeHeading = Require<Label>(RootElement, "ScreenModeHeading");
            Label resolutionHeading = Require<Label>(RootElement, "ResolutionHeading");
            Label qualityHeading = Require<Label>(RootElement, "QualityHeading");
            Label brightnessHeading = Require<Label>(RootElement, "BrightnessHeading");
            Label returnToTitleMessage = Require<Label>(RootElement, "ReturnToTitleMessage");
            Label languageHeading = Require<Label>(RootElement, "LanguageHeading");
            Label vibrationHeading = Require<Label>(RootElement, "VibrationHeading");
            Label rhythmOffsetHeading = Require<Label>(RootElement, "RhythmOffsetHeading");
            // 見出しとボタンの文言をローカライズに登録する。
            _localizedButtonTexts = new[]
            {
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.title", text => titleLabel.text = text, "設定"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.audio", text => audioPanelTitle.text = text, "オーディオ設定"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.environment", text => environmentPanelTitle.text = text, "環境設定"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.screen_mode", text => screenModeHeading.text = text, "画面モード"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.resolution", text => resolutionHeading.text = text, "解像度"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.quality", text => qualityHeading.text = text, "画質"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.brightness", text => brightnessHeading.text = text, "明るさ"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.return_to_title_message", text => returnToTitleMessage.text = text, "タイトル画面に戻りますか？"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.language", text => languageHeading.text = text, "言語"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.vibration", text => vibrationHeading.text = text, "振動"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.rhythm_offset", text => rhythmOffsetHeading.text = text, "リズム判定タイミング"),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.close", text => _backButton.text = text),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.environment", text => _environmentSettingButton.text = text),
                new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.setting.return_to_title", text => _returnToTitleButton.text = text),
                new LocalizedElementText(
                    UI_COMMON_TABLE,
                    "ui.setting.cancel_return_to_title",
                    text => _cancelReturnToTitleButton.text = text),
                new LocalizedElementText(
                    UI_COMMON_TABLE,
                    "ui.setting.confirm_return_to_title",
                    text => _confirmReturnToTitleButton.text = text),
            };
        }
    }
}
