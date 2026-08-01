using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Result
{
    /// <summary>
    ///     UI要素を左から本来の位置へ右方向にスライドインさせる共通演出です。
    ///     LitMotionでanchoredPositionとCanvasGroup.alphaを同時に動かします。
    /// </summary>
    internal static class ResultTextSlideIn
    {
        /// <summary>
        ///     対象を本来の位置から左へ寄せてから、右方向へスライドインさせます。
        /// </summary>
        /// <param name="target"> 動かすRectTransform。 </param>
        /// <param name="endAnchoredPosition"> スライドインの終点となる本来のanchoredPosition。 </param>
        /// <param name="setting"> 演出設定。 </param>
        /// <param name="delay"> 再生開始までの遅延（秒）。 </param>
        /// <param name="handles"> 生成したモーションハンドルの追加先。 </param>
        public static void Play(
            RectTransform target,
            Vector2 endAnchoredPosition,
            ResultTextSlideInSetting setting,
            float delay,
            List<MotionHandle> handles)
        {
            if (target == null || setting == null || !setting.IsEnabled)
            {
                return;
            }

            CanvasGroup canvasGroup =
                setting.UseFade ? EnsureCanvasGroup(target.gameObject) : null;

            if (setting.Duration <= 0f)
            {
                ApplyEndState(target, endAnchoredPosition, canvasGroup);
                return;
            }

            Vector2 startAnchoredPosition =
                new(endAnchoredPosition.x - setting.Distance, endAnchoredPosition.y);

            // WithDelayの待機中はバインドが走らず終点のまま見えてしまうため、
            // 開始状態を先に反映しておく。
            target.anchoredPosition = startAnchoredPosition;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            MotionHandle positionHandle =
                LMotion.Create(
                        startAnchoredPosition,
                        endAnchoredPosition,
                        setting.Duration)
                    .WithDelay(delay)
                    .WithEase(setting.Ease)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToAnchoredPosition(target)
                    .AddTo(target.gameObject);

            handles?.Add(positionHandle);

            if (canvasGroup == null)
            {
                return;
            }

            MotionHandle alphaHandle =
                LMotion.Create(0f, 1f, setting.Duration)
                    .WithDelay(delay)
                    .WithEase(setting.Ease)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToAlpha(canvasGroup)
                    .AddTo(target.gameObject);

            handles?.Add(alphaHandle);
        }

        /// <summary>
        ///     再生中のスライドインを停止し、ハンドルを破棄します。
        /// </summary>
        /// <param name="handles"> 停止するモーションハンドル一覧。 </param>
        public static void Stop(List<MotionHandle> handles)
        {
            if (handles == null)
            {
                return;
            }

            for (int i = 0; i < handles.Count; i++)
            {
                handles[i].TryCancel();
            }

            handles.Clear();
        }

        /// <summary>
        ///     対象を演出終了後の表示状態へ戻します。
        /// </summary>
        /// <param name="target"> 対象のRectTransform。 </param>
        /// <param name="endAnchoredPosition"> 本来のanchoredPosition。 </param>
        /// <param name="canvasGroup"> フェードに使用したCanvasGroup。未使用ならnull。 </param>
        public static void ApplyEndState(
            RectTransform target,
            Vector2 endAnchoredPosition,
            CanvasGroup canvasGroup)
        {
            if (target == null)
            {
                return;
            }

            target.anchoredPosition = endAnchoredPosition;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        ///     指定GameObjectにCanvasGroupを確保します。
        /// </summary>
        /// <param name="gameObject"> 対象のGameObject。 </param>
        /// <returns> 確保したCanvasGroup。 </returns>
        private static CanvasGroup EnsureCanvasGroup(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(out CanvasGroup canvasGroup))
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            return canvasGroup;
        }
    }
}
