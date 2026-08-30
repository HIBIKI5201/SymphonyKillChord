using KillChord.Runtime.Adaptor.OutGame.Title;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     タイトルシーンの View クラス。
    /// </summary>
    public class TitleSceneView :  ScreenViewBase
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

            _cancellationTokenSource = new CancellationTokenSource();

            RegisterCallbacks();
        }

        /// <summary>
        ///   タイトルシーンの View のリソースを解放する。
        /// </summary>
        public override void Dispose()
        {
            UnRegisterCallbacks();
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

        private string _currentSceneName;
        private string _targetSceneName;

        /// <summary> タッチエリアの VisualElement。 </summary>
        private VisualElement _touchArea;
        private Button _optionButton;

        private TitleStartController _titleStartController;
        private PlayerInputView _playerInputView;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isStarting;

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
            if (_isStarting)
            {
                return;
            }

            _isStarting = true;
            bool isSuccess = false;

            try
            {
                isSuccess =
                    await _titleStartController.StartGameAsync(_currentSceneName, _targetSceneName, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    $"[{nameof(TitleSceneView)}] "
                    + $"{_currentSceneName} -> {_targetSceneName} への遷移がキャンセルされました。");
#endif
                _isStarting = false;
                return;
            }
            catch (Exception exception)
            {
                _isStarting = false;
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
                _isStarting = false;
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(TitleSceneView)}] "
                    + $"{_currentSceneName} -> {_targetSceneName} への遷移に失敗しました。");
#endif
            }
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
            if (inputContext.Phase != UnityEngine.InputSystem.InputActionPhase.Performed)
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
            OutGameUIEvent?.OnShowMenuScreen?.Invoke();
        }
    }
}
