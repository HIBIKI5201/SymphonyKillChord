using System; // OperationCanceledExceptionのために追加
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     UI ToolkitのVisualElementを拡張するユーティリティクラス。
    /// </summary>
    public static class VisualElementUtility
    {
        /// <summary>
        ///     VisualElementの横幅を割合で変更する非同期メソッド。
        /// </summary>
        /// <param name="bar">対象のVisualElement。</param>
        /// <param name="value">目標とする横幅の割合（0～1の範囲）。</param>
        /// <param name="duration">変化にかける時間。</param>
        /// <param name="token">非同期処理のキャンセルトークン。</param>
        /// <returns>非同期操作を表すTask。</returns>
        public static async Task ChangeBarAsync(
            this VisualElement bar,
            float value, float duration,
            CancellationToken token = default)
        {
            // 現在の割合を取得。
            float current = bar.resolvedStyle.width / bar.parent.resolvedStyle.width;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float newValue = Mathf.Lerp(current, value, t);
                bar.style.width = Length.Percent(newValue * 100); // パーセンテージで指定。
                // OperationCanceledException は意図的に発生させ、呼び出し側で処理することが多いため、
                // ここではtry-catchせずに次のフレームを待機する。
                await Awaitable.NextFrameAsync(token);
            }

            // 最終的な値を設定。
            bar.style.width = Length.Percent(value * 100);
        }
    }
}
