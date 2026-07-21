using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     ステージ開始時の黒画面とフェードアウトを表示するViewです。
    /// </summary>
    public sealed class StageStartFadeView : MonoBehaviour
    {
        /// <summary>
        ///     黒画面を即時表示します。
        /// </summary>
        public void ShowBlackImmediate()
        {
            CancelFade();
            SetAlpha(1f);
            SetInputBlock(true);
        }

        /// <summary>
        ///     黒画面を即時非表示にします。
        /// </summary>
        public void HideImmediate()
        {
            CancelFade();
            SetAlpha(0f);
            SetInputBlock(false);
        }

        /// <summary>
        ///     黒画面を維持した後、LitMotionでフェードアウトします。
        /// </summary>
        /// <param name="onCompleted"> フェードアウト完了時に実行する処理。 </param>
        public void PlayFadeOut(Action onCompleted = null)
        {
            CancelFade();
            _onCompleted = onCompleted;

            // フェード処理に必要な参照が設定されていない場合は、フェード処理を中止します。
            if (!ValidateReferences())
            {
                CompleteFade();
                return;
            }

            // フェードアウト開始時は黒画面を表示し、UI入力を遮断します。
            SetAlpha(1f);
            SetInputBlock(true);

            // 黒画面の維持時間とフェードアウト時間を取得します。
            float holdDuration =
                Mathf.Max(0f, _config.BlackholdDuration);

            float fadeDuration =
                Mathf.Max(0f, _config.FadeOutDuration);

            if (fadeDuration <= 0f)
            {
                PlayHoldOnly(holdDuration);
                return;
            }

            // LitMotionでフェードアウトを再生します。
            _fadeHandle = LMotion.Create(
                    1f,
                    0f,
                    fadeDuration)
                .WithDelay(holdDuration)
                .WithEase(_config.FadeOutEasing)
                .WithOnComplete(CompleteFade)
                .BindToAlpha(_canvasGroup)
                .AddTo(gameObject);
        }

        /// <summary>
        ///     再生中のフェードアウトを停止します。
        /// </summary>
        public void CancelFade()
        {
            _onCompleted = null;
            _fadeHandle.TryCancel();
        }

        [SerializeField, Tooltip("ステージ開始演出の表示設定。")]
        private StageStartSequenceConfig _config;

        [SerializeField, Tooltip("黒画面全体のCanvasGroup。")]
        private CanvasGroup _canvasGroup;

        private MotionHandle _fadeHandle;
        private Action _onCompleted;

        /// <summary>
        ///     シーンが最初に描画される前から黒画面を表示します。
        /// </summary>
        private void Awake()
        {
            SetAlpha(1f);
            SetInputBlock(true);
        }

        /// <summary>
        ///     オブジェクト破棄時に再生中のモーションを停止します。
        /// </summary>
        private void OnDestroy()
        {
            CancelFade();
        }

        /// <summary>
        ///     フェード処理に必要な参照を検証します。
        /// </summary>
        /// <returns>
        ///     必要な参照が設定されている場合はtrueです。
        /// </returns>
        private bool ValidateReferences()
        {
            if (_config == null)
            {
                Debug.LogError(
                    $"[{nameof(StageStartFadeView)}] "
                    + $"{nameof(_config)} が設定されていません。",
                    this);
                return false;
            }

            if (_canvasGroup == null)
            {
                Debug.LogError(
                    $"[{nameof(StageStartFadeView)}] "
                    + $"{nameof(_canvasGroup)} が設定されていません。",
                    this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     フェード時間が0の場合に黒画面の維持時間だけ再生します。
        /// </summary>
        /// <param name="duration"> 黒画面を維持する時間。 </param>
        private void PlayHoldOnly(float duration)
        {
            if (duration <= 0f)
            {
                CompleteFade();
                return;
            }

            _fadeHandle = LMotion.Create(
                    0f,
                    1f,
                    duration)
                .WithScheduler(
                    MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(CompleteFade)
                .RunWithoutBinding()
                .AddTo(gameObject);
        }

        /// <summary>
        ///     フェードアウト完了時の状態を反映します。
        /// </summary>
        private void CompleteFade()
        {
            SetAlpha(0f);
            SetInputBlock(false);

            Action onCompleted = _onCompleted;
            _onCompleted = null;

            onCompleted?.Invoke();
        }

        /// <summary>
        ///     黒画面の透明度を設定します。
        /// </summary>
        /// <param name="alpha"> 0から1の透明度です。 </param>
        private void SetAlpha(float alpha)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha =
                Mathf.Clamp01(alpha);
        }

        /// <summary>
        ///     黒画面によるUI入力の遮断状態を設定します。
        /// </summary>
        /// <param name="isBlocked"> UI入力を遮断する場合はtrueです。 </param>
        private void SetInputBlock(bool isBlocked)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.interactable = isBlocked;
            _canvasGroup.blocksRaycasts = isBlocked;
        }
    }
}