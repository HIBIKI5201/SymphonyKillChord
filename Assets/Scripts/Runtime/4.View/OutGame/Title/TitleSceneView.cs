using KillChord.Runtime.Adaptor.OutGame.Title;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Input;
using LitMotion;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     タイトルシーンの View クラス。
    /// </summary>
    public class TitleSceneView : ScreenViewBase
    {
        /// <summary>
        ///    タイトルシーンの View を初期化する。
        /// </summary>
        /// <param name="rootElement"></param>
        /// <param name="outGameUIEvent"></param>
        /// <param name="titleStartController"></param>
        /// <param name="currentSceneName"></param>
        /// <param name="targetSceneName"></param>
        public TitleSceneView(
            VisualElement rootElement,
            OutGameUIEvent outGameUIEvent,
            TitleStartController titleStartController,
            string currentSceneName,
            string targetSceneName) : base(rootElement, outGameUIEvent)
        {
            Initialize(rootElement, titleStartController);
            _currentSceneName = currentSceneName;
            _targetSceneName = targetSceneName;
        }

        /// <summary> タイトルの操作待ち背景演出を再生できる状態です。 </summary>
        public bool IsIdleVideoAllowed => !_isDisposed && !_isStarting && IsShowCompleted
            && _touchArea != null && _touchArea.enabledInHierarchy;

        /// <summary>
        ///    タイトルシーンの View を初期化する。
        /// </summary>
        public void Initialize(VisualElement rootElement, TitleStartController titleStartController)
        {
            if (rootElement == null) { throw new ArgumentNullException(nameof(rootElement)); }

            if (titleStartController == null) { throw new ArgumentNullException(nameof(titleStartController)); }
            _titleStartController = titleStartController;

            _touchArea = rootElement.Q<VisualElement>(TOUCH_AREA_NAME)
                ?? throw new NullReferenceException($"{nameof(TitleSceneView)}: {TOUCH_AREA_NAME}の取得に失敗しました。");
            _optionButton = rootElement.Q<Button>(OPTION_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(TitleSceneView)}: {OPTION_BUTTON_NAME}の取得に失敗しました。");
            _instructionElement = rootElement.Q<VisualElement>(INSTRUCTION_ELEMENT_NAME)
                ?? throw new NullReferenceException($"{nameof(TitleSceneView)}: {INSTRUCTION_ELEMENT_NAME}の取得に失敗しました。");

            ConfigureStartInstruction();
            _cancellationTokenSource = new CancellationTokenSource();

            RegisterCallbacks();
            StartInstructionBreathing();
        }

        /// <summary>
        ///   タイトルシーンの View のリソースを解放する。
        /// </summary>
        public override void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            UnRegisterCallbacks();
            _instructionMotionHandle.TryCancel();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            base.Dispose();
        }

        /// <summary>
        ///     遷移先のシーン名を設定する。
        ///     初回起動時とそれ以外で遷移先のシーンが異なるため、外部から設定できるようにする。
        /// </summary>
        /// <param name="targetSceneName"></param>
        public void SetTargetSceneName(string targetSceneName)
        {
            // targetSceneName が null または空文字の場合は例外をスローする。
            if (string.IsNullOrEmpty(targetSceneName))
            {
                throw new ArgumentException("targetSceneName must not be null or empty.", nameof(targetSceneName));
            }

            _targetSceneName = targetSceneName;
        }

        /// <summary>
        ///     コントローラーのOptionsボタンでオプション画面を開けるようにします。
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

        /// <inheritdoc />
        protected override VisualElement InitialFocusElement => _touchArea;

        private const string TOUCH_AREA_NAME = "TouchArea";
        private const string OPTION_BUTTON_NAME = "OptionButton";
        private const string INSTRUCTION_ELEMENT_NAME = "Instruction";
        private const string MOBILE_INSTRUCTION_NAME = "MobileInstruction";
        private const string CONTROLLER_INSTRUCTION_NAME = "ControllerInstruction";
        private const float INSTRUCTION_FADE_DURATION = 1.8f;

        private string _currentSceneName;
        private string _targetSceneName;

        /// <summary> タッチエリアの VisualElement。 </summary>
        private VisualElement _touchArea;
        private Button _optionButton;
        private VisualElement _instructionElement;
        private MotionHandle _instructionMotionHandle;

        private TitleStartController _titleStartController;
        private PlayerInputView _playerInputView;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isStarting;
        private bool _isDisposed;

        /// <summary>
        ///     タッチエリアのクリックイベントを登録する。
        /// </summary>
        private void RegisterCallbacks()
        {
            if (_touchArea == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneView)}: タッチエリアがnullです。");
#endif
                return;
            }

            _touchArea.RegisterCallback<PointerDownEvent>(OnPointDownEvent);
            _optionButton.clicked += OnClickOptionButton;

            // 決定操作でゲームを開始する。タップ開始と同じ処理へ流す。
            _touchArea.MakeNavigable();
            _touchArea.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);

            // オプションはコントローラーのOptionsボタンから開くため、
            // フォーカス移動の対象からは外す。
            _optionButton.ExcludeFromNavigation();
        }

        /// <summary>
        ///    タッチエリアのクリックイベントを解除する。
        /// </summary>
        private void UnRegisterCallbacks()
        {
            if (_touchArea == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneView)}: タッチエリアがnullです。");
#endif
                return;
            }

            _touchArea.UnregisterCallback<PointerDownEvent>(OnPointDownEvent);
            _touchArea.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            _optionButton.clicked -= OnClickOptionButton;
            UnbindOptionInput();
        }

        /// <summary>
        ///     スマートフォンはタップ、PCなどは東ボタン画像付きの開始案内を表示します。
        /// </summary>
        private void ConfigureStartInstruction()
        {
            Label mobileInstruction = _instructionElement.Q<Label>(MOBILE_INSTRUCTION_NAME)
                ?? throw new NullReferenceException($"{nameof(TitleSceneView)}: {MOBILE_INSTRUCTION_NAME}の取得に失敗しました。");
            VisualElement controllerInstruction = _instructionElement.Q<VisualElement>(CONTROLLER_INSTRUCTION_NAME)
                ?? throw new NullReferenceException($"{nameof(TitleSceneView)}: {CONTROLLER_INSTRUCTION_NAME}の取得に失敗しました。");

            bool isMobilePlatform = UnityEngine.Application.isMobilePlatform;
            mobileInstruction.style.display = isMobilePlatform ? DisplayStyle.Flex : DisplayStyle.None;
            controllerInstruction.style.display = isMobilePlatform ? DisplayStyle.None : DisplayStyle.Flex;
        }

        /// <summary>
        ///     開始案内の文字と画像を繰り返しフェードイン/アウトさせます。
        /// </summary>
        private void StartInstructionBreathing()
        {
            _instructionMotionHandle = LMotion.Create(1f, 0f, INSTRUCTION_FADE_DURATION)
                .WithEase(Ease.InOutSine)
                .WithLoops(-1, LoopType.Yoyo)
                .Bind(_instructionElement, static (opacity, element) => element.style.opacity = opacity);
        }

        /// <summary>
        ///     タッチエリアがクリックされたときの処理。
        ///     アウトゲームシーンに遷移する。
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointDownEvent(PointerDownEvent evt)
        {
            StartGame();
        }

        /// <summary>
        ///     アウトゲームシーンへ遷移してゲームを開始する。
        /// </summary>
        private async void StartGame()
        {
            if (_isDisposed || _isStarting || !_touchArea.enabledInHierarchy)
            {
                return;
            }

            _isStarting = true;
            _touchArea.SetEnabled(false);
            _optionButton.SetEnabled(false);
            bool isSuccess = false;

            try
            {
                isSuccess =
                    await _titleStartController.StartGameAsync(_currentSceneName, _targetSceneName, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException operationCanceledException)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    $"[{nameof(TitleSceneView)}] "
                    + $"{_currentSceneName} -> {_targetSceneName} への遷移がキャンセルされました。 {operationCanceledException}");
#endif
                RestoreStartInteraction();
                return;
            }
            catch (Exception exception)
            {
                RestoreStartInteraction();
                Debug.LogException(exception);
                return;
            }

            if (isSuccess)
            {
#if UNITY_EDITOR
                Debug.Log(
                    $"[{nameof(TitleSceneView)}] "
                    + $"{_currentSceneName} -> {_targetSceneName} への遷移に成功しました。");
#endif
            }
            else
            {
                RestoreStartInteraction();
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(TitleSceneView)}] "
                    + $"{_currentSceneName} -> {_targetSceneName} への遷移に失敗しました。");
#endif
            }
        }

        /// <summary>
        ///     失敗とキャンセル時は生存中のViewだけを再操作可能にし、開始領域のフォーカスを復元する。
        /// </summary>
        private void RestoreStartInteraction()
        {
            if (_isDisposed)
            {
                return;
            }

            _isStarting = false;
            _touchArea.SetEnabled(true);
            _optionButton.SetEnabled(true);
            SetInitialFocusElement(_touchArea);
            RestoreFocus();
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
        ///     決定操作でゲームを開始する。
        /// </summary>
        /// <param name="evt"> ナビゲーション決定イベント。 </param>
        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            StartGame();
            evt.StopPropagation();
        }

        /// <summary>
        ///     コントローラーのOptionsボタンでオプション画面を開く。
        /// </summary>
        /// <param name="inputContext"> 入力情報。 </param>
        private void OnOptionInput(InputContext<float> inputContext)
        {
            // 押した瞬間のみ反応させる。離した際の通知では開かない。
            // フェードイン中は画面が操作可能になっていないため、オプション画面を閉じた直後の
            // 同一入力で再度開いてしまわないよう表示完了も条件に加える。
            if (_isDisposed || _isStarting || !IsShowCompleted || !_optionButton.enabledInHierarchy
                || inputContext.Phase != UnityEngine.InputSystem.InputActionPhase.Performed)
            {
                return;
            }

            OnClickOptionButton();
        }

        /// <summary>
        ///     オプションボタンがクリックされたときの処理。
        /// </summary>
        private void OnClickOptionButton()
        {
            if (_isDisposed || _isStarting || !_optionButton.enabledInHierarchy)
            {
                return;
            }

            OutGameUIEvent?.OnShowMenuScreen?.Invoke();
        }
    }
}
