using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    public static class VisualElementUtility
    {
        public static async Task ChangeBarAsync(
            this VisualElement bar,
            float value, float duration,
            CancellationToken token = default)
        {
            float current = bar.resolvedStyle.width / bar.parent.resolvedStyle.width;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float newValue = Mathf.Lerp(current, value, t);
                bar.style.width = Length.Percent(newValue * 100);

                await Awaitable.NextFrameAsync(token);
            }

            bar.style.width = Length.Percent(value * 100);
        }
    }
}
