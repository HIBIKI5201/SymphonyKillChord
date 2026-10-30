using KillChord.DebugWorld.Common;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.DebugWorld.Hub
{
    /// <summary>
    ///     裏世界のハブシーンを初期化し、ハブ画面とシーン遷移をつなぐクラスです。
    /// </summary>
    public sealed class DebugHubInitializer : MonoBehaviour
    {
        [SerializeField, Tooltip("ハブ画面を表示するUIDocument。")]
        private UIDocument _uiDocument;

        private DebugHubView _view;

        private void Start()
        {
            bool isSuccess = false;
            try
            {
                isSuccess = TryCreateView();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                CompleteSceneInitialization(isSuccess);
            }
        }

        private void OnDestroy()
        {
            if (_view == null)
            {
                return;
            }

            _view.OnReturnRequested -= HandleReturnRequested;
            _view.Dispose();
            _view = null;
        }

        /// <summary>
        ///     タイトルへ戻るよう求められたら、ハブシーンを閉じてタイトルシーンを開きます。
        /// </summary>
        private async void HandleReturnRequested()
        {
            _view.Lock();
            try
            {
                bool isSuccess = await DebugWorldSceneChanger.ChangeSceneAsync(
                    gameObject.scene.name,
                    DebugWorldSceneNames.TITLE);
                if (!isSuccess && _view != null)
                {
                    _view.Unlock();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                _view?.Unlock();
            }
        }

        /// <summary>
        ///     ハブ画面を組み立てます。
        /// </summary>
        /// <returns> 組み立てられた場合はtrueです。 </returns>
        private bool TryCreateView()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError($"[{nameof(DebugHubInitializer)}] UIDocumentが設定されていません。", this);
                return false;
            }

            _view = new DebugHubView(_uiDocument.rootVisualElement);
            _view.OnReturnRequested += HandleReturnRequested;
            return true;
        }

        /// <summary>
        ///     ロード画面を閉じられるよう、ハブシーンの初期化結果を通知します。
        /// </summary>
        /// <param name="isSuccess"> 初期化に成功した場合はtrueです。 </param>
        private void CompleteSceneInitialization(bool isSuccess)
        {
            if (!ServiceLocator.TryGetInstance(out ISceneInitializationReadiness readiness))
            {
                Debug.LogWarning(
                    $"[{nameof(DebugHubInitializer)}] {nameof(ISceneInitializationReadiness)}が取得できません。"
                    + "常駐シーンを通さずに開いた場合は、タイトルへ戻れません。",
                    this);
                return;
            }

            readiness.Complete(gameObject.scene.name, isSuccess);
        }
    }
}
