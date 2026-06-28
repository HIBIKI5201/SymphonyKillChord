using KillChord.Runtime.Adaptor.OutGame.Title;
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
        }

        /// <summary>
        ///     遷移先のシーン名を設定する。
        ///     初回起動時とそれ以外で遷移先のシーンが異なるため、外部から設定できるようにする。
        /// </summary>
        /// <param name="targetSceneName"></param>
        public void SetTargetSceneName(string targetSceneName)
        {
            _targetSceneName = targetSceneName;
        }

        private const string TOUCH_AREA_NAME = "TouchArea";
        private const string OPTION_BUTTON_NAME = "OptionButton";

        private string _currentSceneName;
        private string _targetSceneName;

        /// <summary> タッチエリアの VisualElement。 </summary>
        private VisualElement _touchArea;
        private Button _optionButton;

        private TitleStartController _titleStartController;

        private CancellationTokenSource _cancellationTokenSource;

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
            _optionButton.clicked -= OnClickOptionButton;
        }

        /// <summary>
        ///     タッチエリアがクリックされたときの処理。
        ///     アウトゲームシーンに遷移する。
        /// </summary>
        /// <param name="evt"></param>
        private async void OnPointDownEvent(PointerDownEvent evt)
        {
            bool succes = false;

            try
            {
                succes =
                    await _titleStartController.StartGameAsync(_currentSceneName, _targetSceneName, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{_currentSceneName} -> {_targetSceneName} への遷移がキャンセルされました。");
#endif
                return;
            }

            if (succes)
            {
#if UNITY_EDITOR
                Debug.Log($"{_currentSceneName} -> {_targetSceneName} への遷移に成功しました。");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"{_currentSceneName} -> {_targetSceneName} への遷移に失敗しました。");
#endif
            }
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
