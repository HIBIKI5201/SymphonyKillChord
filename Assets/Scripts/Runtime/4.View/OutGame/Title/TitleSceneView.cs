using KillChord.Runtime.Adaptor.OutGame.Title;
using SymphonyFrameWork.Attribute;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     タイトルシーンの View クラス。
    /// </summary>
    public class TitleSceneView : MonoBehaviour
    {
        /// <summary>
        ///    タイトルシーンの View を初期化する。
        /// </summary>
        public void Initialize(VisualElement root, TitleStartController titleStartController)
        {
            if (root == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneView)}: Root VisualElementがnullです。");
#endif
                return;
            }

            _touchArea = root.Q<VisualElement>(TOUCH_AREA_NAME);
            _titleStartController = titleStartController;

            if (_touchArea == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneView)}: {TOUCH_AREA_NAME}の取得に失敗しました。");
#endif
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            RegisterCallbacks();
        }

        private const string TOUCH_AREA_NAME = "TouchArea";

        [SerializeField, SceneNameSelector, Tooltip("遷移元のシーン名を指定します。")]
        private string _currentSceneName;
        [SerializeField, SceneNameSelector, Tooltip("遷移先のシーン名を指定します。")]
        private string _targetSceneName;

        /// <summary> タッチエリアの VisualElement。 </summary>
        private VisualElement _touchArea;
        private TitleStartController _titleStartController;

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        ///     タッチエリアのクリックイベントを登録する。
        /// </summary>
        private void RegisterCallbacks()
        {
            if(_touchArea == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneView)}: タッチエリアがnullです。");
#endif
                return;
            }

            _touchArea.RegisterCallback<PointerDownEvent>(OnPointDownEvent);
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
                    await _titleStartController.StartGameAsync(_targetSceneName, _cancellationTokenSource.Token);
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

        private void OnDestroy()
        {
            UnRegisterCallbacks();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
