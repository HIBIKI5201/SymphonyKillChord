using KillChord.Runtime.Adaptor.Persistent.Load;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.Persistent.Load
{
    /// <summary>
    ///     ロード画面をロード進捗を表示するViewクラス。
    /// </summary>
    public class LoadingScreenView : MonoBehaviour
    {
        /// <summary>
        ///     ロード画面コントローラーを設定する。
        /// </summary>
        /// <param name="loadingScreenController"></param>
        public void Initialize(LoadingScreenController loadingScreenController)
        {
            Unsubscribe();

            _controller = loadingScreenController;

            Subscribe();

            if (_controller != null && _controller.IsLoading)
            {
                SetVisible(true);
            }
            else
            {
                ApplyProgress(0f);
                SetVisible(false);
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        /// <summary>
        ///     ロード画面イベントを購読する。
        /// </summary>
        private void Subscribe()
        {
            if (_controller == null || _isSubscribed)
            {
                return;
            }

            _controller.LoadingStarted +=
                HandleLoadingStarted;

            _controller.LoadingProgressChanged +=
                HandleLoadingProgressChanged;

            _controller.LoadingCompleted +=
                HandleLoadingCompleted;

            _isSubscribed = true;
        }

        /// <summary>
        ///     ロード画面イベントの購読を解除する。
        /// </summary>
        private void Unsubscribe()
        {
            if (_controller == null || !_isSubscribed)
            {
                return;
            }

            _controller.LoadingStarted -=
                HandleLoadingStarted;

            _controller.LoadingProgressChanged -=
                HandleLoadingProgressChanged;

            _controller.LoadingCompleted -=
                HandleLoadingCompleted;

            _isSubscribed = false;
        }

        /// <summary>
        ///     ロード開始時に画面を表示する。
        /// </summary>
        private void HandleLoadingStarted()
        {
            ApplyProgress(0f);
            SetVisible(true);
        }

        /// <summary>
        ///     ロード進捗をUIへ反映する。
        /// </summary>
        /// <param name="progress"> 0から1の進捗。 </param>
        private void HandleLoadingProgressChanged(float progress)
        {
            ApplyProgress(progress);
        }

        /// <summary>
        ///     ロード終了時に画面を非表示にする。
        /// </summary>
        /// <param name="success"> ロードが成功したかどうか。 </param>
        private void HandleLoadingCompleted(bool success)
        {
            if (success)
            {
                ApplyProgress(1f);
            }

            SetVisible(false);
        }

        /// <summary>
        ///     ロード進捗を表示へ反映する。
        /// </summary>
        /// <param name="progress"> 0から1の進捗。 </param>
        private void ApplyProgress(float progress)
        {
            if (_progressFillArea == null || _progressFillRect == null)
            {
                return;
            }

            float normalizedProgress = Mathf.Clamp01(progress);
            float fillWidth =
                _progressFillArea.rect.width * normalizedProgress;

            _progressFillRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                fillWidth);

            // 幅が0のときにSliced画像の端だけが残るのを防ぐ。
            _progressFillRect.gameObject.SetActive(
                normalizedProgress > 0f);
        }

        /// <summary>
        ///     ロード画面の表示状態を変更する。
        /// </summary>
        /// <param name="isVisible"> 表示する場合はtrue。 </param>
        private void SetVisible(bool isVisible)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
        }

        [SerializeField, Tooltip("ロード画面のCanvasGroup")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("ロードゲージを表示する範囲です。")]
        private RectTransform _progressFillArea;

        [SerializeField, Tooltip("ロード進捗に合わせて横幅を変更するImageです。")]
        private RectTransform _progressFillRect;

        private LoadingScreenController _controller;
        private bool _isSubscribed;
    }
}
