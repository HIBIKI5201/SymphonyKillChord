using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     UITKの便利クラス。
    /// </summary>
    public static class VisualElementUtility
    {
        /// <summary>
        ///     VisualElementの横幅を割合で変更する非同期メソッド。
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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

                await Awaitable.NextFrameAsync(token);
            }

            // 最終的な値を設定。
            bar.style.width = Length.Percent(value * 100);
        }
    }
}
