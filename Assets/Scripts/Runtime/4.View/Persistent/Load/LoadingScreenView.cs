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
                ApplyRandomTip();
                SetVisible(true);
                StartMedalRotation();
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
            _medalHandle.TryCancel();
            _visibilityHandle.TryCancel();
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
            ApplyRandomTip();
            SetVisible(true);
            StartMedalRotation();
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
        ///     ロード終了時に画面をフェードアウトして非表示にする。
        /// </summary>
        /// <param name="success"> ロードが成功したかどうか。 </param>
        private void HandleLoadingCompleted(bool success)
        {
            if (success)
            {
                ApplyProgress(1f);
            }

            StopMedalRotation();
            HideWithFade();
        }

        /// <summary>
        ///     Tipsをランダムに選んで表示する。
        /// </summary>
        private void ApplyRandomTip()
        {
            if (_tipsText == null)
            {
                return;
            }

            // Tips設定が存在する場合はランダムにTipsを取得し、存在しない場合は空文字列を設定する。
            string tip = _tipsConfig != null ? _tipsConfig.GetRandomTip() : string.Empty;
            _tipsText.SetText(tip);
        }

        /// <summary>
        ///     勲章の回転を開始する。
        /// </summary>
        private void StartMedalRotation()
        {
            _medalHandle.TryCancel();

            if (_medalHandle == null)
            {
                return;
            }

            _medalInitialEulerAngles = _medalTransform.localEulerAngles;
            PlayMedalRotation();
        }

        /// <summary>
        ///     勲章を1回転させるアニメーションを再帰的に実行する。
        /// </summary>
        private void PlayMedalRotation()
        {
            if (!_isVisible || _medalTransform == null)
            {
                return;
            }

            _medalHandle = LSequence.Create()
                .Append(LMotion.Create(0f, 360f, _medalRotationDuration)
                    .WithEase(_medalRotationEase)
                    .Bind(this, (value, state) =>
                        state.ApplyMedalRotation(value)))
                .AppendInterval(_medalRotationDelay)
                .Run(sequence => sequence
                    .WithOnComplete(() => PlayMedalRotation()));
        }

        /// <summary>
        ///     勲章のY軸回転を適用する。
        /// </summary>
        /// <param name="angle"> 回転させる角度（度） </param>
        private void ApplyMedalRotation(float angle)
        {
            if (_medalTransform == null)
            {
                return;
            }

            Vector3 eulerAngles = _medalInitialEulerAngles;
            eulerAngles.y += angle;
            _medalTransform.localEulerAngles = eulerAngles;
        }

        /// <summary>
        ///     勲章の回転を停止する。
        /// </summary>
        private void StopMedalRotation()
        {
            _medalHandle.TryCancel();

            if (_medalHandle == null)
            {
                return;
            }

            _medalTransform.localEulerAngles = _medalInitialEulerAngles;
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
            _isVisible = isVisible;

            if (_canvasGroup == null)
            {
                return;
            }

            _visibilityHandle.TryCancel();

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

        /// <summary>
        ///     ロード完了時に、ロード画面をアルファフェードで非表示にする。
        /// </summary>
        private void HideWithFade()
        {
            _isVisible = false;

            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _visibilityHandle.TryCancel();

            if (_hideFadeDuration <= 0f)
            {
                _canvasGroup.alpha = 0f;
                return;
            }

            _visibilityHandle = LMotion.Create(_canvasGroup.alpha, 0f, _hideFadeDuration)
                .WithEase(_hideFadeEase)
                .BindToAlpha(_canvasGroup);
        }

        [SerializeField, Tooltip("ロード画面のCanvasGroup")]
        private CanvasGroup _canvasGroup;

        [Header("非表示フェード設定")]
        [SerializeField, Min(0f), Tooltip("ロード完了時にロード画面をフェードアウトする時間（秒）。0でフェードなしの瞬時非表示。")]
        private float _hideFadeDuration = 0.25f;

        [SerializeField, Tooltip("ロード完了時のフェードアウトに使用するイージング")]
        private Ease _hideFadeEase = Ease.OutCubic;

        [SerializeField, Tooltip("伸縮するロードゲージのImage（FillAmountに使う）")]
        private Image _progressImage;

        [SerializeField]
        private TMP_Text _progressText;

        [SerializeField]
        private Image _fadeImage;

        [Header("Tips表示設定")]
        [SerializeField, Tooltip("ロード画面に表示するTipsの設定")]
        private LoadingTipsConfig _tipsConfig;

        [SerializeField, Tooltip("ロード画面に表示するTipsのText")]
        private TMP_Text _tipsText;

        [Header("勲章回転設定")]
        [SerializeField, Tooltip("Y軸回転させる勲章")]
        private RectTransform _medalTransform;

        [SerializeField, Min(0.01f), Tooltip("勲章が1回転するのにかかる時間（秒）")]
        private float _medalRotationDuration;

        [SerializeField, Tooltip("勲章が1回転した後に待機する時間（秒）")]
        private float _medalRotationDelay;

        [SerializeField, Tooltip("勲章の回転に使用するイージング")]
        private Ease _medalRotationEase;

        private LoadingScreenController _controller;
        private bool _isSubscribed;
        private bool _isVisible;
        private Vector3 _medalInitialEulerAngles;
        private MotionHandle _handle;
        private MotionHandle _medalHandle;
        private MotionHandle _visibilityHandle;
    }
}
