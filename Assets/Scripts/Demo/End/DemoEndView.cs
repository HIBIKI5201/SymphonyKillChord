using LitMotion;
using LitMotion.Extensions;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Demo.End
{
    /// <summary>
    ///     体験版終了画面の暗転と案内UIの表示を行うViewです。
    /// </summary>
    public sealed class DemoEndView : MonoBehaviour
    {
        /// <summary> Timelineがアニメートする暗転用CanvasGroupです。 </summary>
        public CanvasGroup BlackoutCanvasGroup => _blackoutCanvasGroup;

        /// <summary>
        ///     暗転を即時に完全な黒へ切り替えます。
        /// </summary>
        public void ShowBlackoutImmediate()
        {
            SetAlpha(_blackoutCanvasGroup, 1.0f);
        }

        /// <summary>
        ///     案内UIをフェードインさせ、完了するまで待機します。
        /// </summary>
        /// <param name="duration"> フェードインにかける秒数です。 </param>
        /// <param name="cancellationToken"> キャンセル用のトークンです。 </param>
        /// <returns> フェードイン完了まで待機するAwaitableです。 </returns>
        public async Awaitable ShowEndUiAsync(
            float duration,
            CancellationToken cancellationToken)
        {
            if (_endUiCanvasGroup == null)
            {
                Debug.LogError(
                    $"[{nameof(DemoEndView)}] {nameof(_endUiCanvasGroup)} が設定されていません。",
                    this);
                return;
            }

            _endUiCanvasGroup.gameObject.SetActive(true);

            if (duration <= 0.0f)
            {
                SetAlpha(_endUiCanvasGroup, 1.0f);
                return;
            }

            CancelEndUiFade();

            bool isCompleted = false;
            _endUiFadeHandle = LMotion.Create(0.0f, 1.0f, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() => isCompleted = true)
                .BindToAlpha(_endUiCanvasGroup)
                .AddTo(gameObject);

            try
            {
                while (!isCompleted)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                CancelEndUiFade();
                throw;
            }

            SetAlpha(_endUiCanvasGroup, 1.0f);
        }

        /// <summary>
        ///     タイトルへ戻る操作を促すプロンプトの表示を切り替えます。
        /// </summary>
        /// <param name="isVisible"> 表示する場合はtrueです。 </param>
        public void SetPromptVisible(bool isVisible)
        {
            if (_promptCanvasGroup == null)
            {
                return;
            }

            SetAlpha(_promptCanvasGroup, isVisible ? 1.0f : 0.0f);
        }

        [SerializeField, Tooltip("Timelineがアニメートする暗転用CanvasGroupです。")]
        private CanvasGroup _blackoutCanvasGroup;

        [SerializeField, Tooltip("タイトルイラストとQRコードを含む案内UIのCanvasGroupです。")]
        private CanvasGroup _endUiCanvasGroup;

        [SerializeField, Tooltip("タイトルへ戻る操作を促すプロンプトのCanvasGroupです。")]
        private CanvasGroup _promptCanvasGroup;

        private MotionHandle _endUiFadeHandle;

        /// <summary>
        ///     シーン遷移直後の1フレーム目から黒画面を表示し、案内UIは隠しておきます。
        ///     暗転の明け方と暗転し直しはTimelineのAnimationトラックが制御します。
        /// </summary>
        private void Awake()
        {
            SetAlpha(_blackoutCanvasGroup, 1.0f);
            SetAlpha(_endUiCanvasGroup, 0.0f);
            SetPromptVisible(false);
        }

        /// <summary>
        ///     破棄時に再生中のモーションを停止します。
        /// </summary>
        private void OnDestroy()
        {
            CancelEndUiFade();
        }

        /// <summary>
        ///     再生中の案内UIフェードを停止します。
        /// </summary>
        private void CancelEndUiFade()
        {
            _endUiFadeHandle.TryCancel();
        }

        /// <summary>
        ///     CanvasGroupの透明度を設定します。
        /// </summary>
        /// <param name="canvasGroup"> 対象のCanvasGroupです。 </param>
        /// <param name="alpha"> 0から1の透明度です。 </param>
        private static void SetAlpha(CanvasGroup canvasGroup, float alpha)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }
    }
}
