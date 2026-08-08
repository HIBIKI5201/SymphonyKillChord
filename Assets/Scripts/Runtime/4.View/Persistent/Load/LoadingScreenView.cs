using KillChord.Runtime.Adaptor.Persistent.Load;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.Persistent.Load
{
    /// <summary>
    ///     ロード画面をロード進捗を表示するViewクラス。
    /// </summary>
    public sealed class LoadingScreenView : MonoBehaviour
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
            _handle.TryCancel();
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
            progress = Mathf.Clamp01(progress);
            if (_progressImage == null)
            {
                return;
            }
            if (_progressImage.fillAmount == progress)
            {
                return;
            }

            _progressImage.fillAmount = progress;
            _progressText.SetText(((int)(progress * 100f)).ToString("00") + '%');
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

            if (isVisible)
            {
                _handle.TryComplete();
                _handle = LMotion.Create(1f, 0f, 0.3f)
                    .WithEase(Ease.OutCirc)
                    .BindToFillAmount(_fadeImage);
            }
        }

        [SerializeField, Tooltip("ロード画面のCanvasGroup")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("伸縮するロードゲージのImage（FillAmountに使う）")]
        private Image _progressImage;

        [SerializeField]
        private TMP_Text _progressText;

        [SerializeField]
        private Image _fadeImage;

        private LoadingScreenController _controller;
        private bool _isSubscribed;
        private MotionHandle _handle;
    }
}
