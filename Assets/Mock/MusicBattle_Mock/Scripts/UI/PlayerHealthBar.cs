using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    [UxmlElement]
    public partial class PlayerHealthBar : VisualElement
    {
        public PlayerHealthBar()
        {
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"Failed to load UXML at path: {UXML_RESOURCES_PATH}");
                return;
            }

            TemplateContainer container = treeAsset.Instantiate();
            hierarchy.Add(container);

            _greenBar = container.Q<VisualElement>(ELEMENT_NAME_GREEN_BAR);
            _redBar = container.Q<VisualElement>(ELEMENT_NAME_RED_BAR);
        }

        /// <summary>
        ///     ヘルスバーの割合を変更する。
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task ChangeHealthBar(float current, float max, CancellationToken token = default)
        {
            float proportion = Mathf.Clamp01(current / max);

            await ChangeBarAsync(_greenBar, proportion, 0.2f, token);
            await Awaitable.WaitForSecondsAsync(1f, token);
            await ChangeBarAsync(_redBar, proportion, 0.6f, token);
        }

        private const string UXML_RESOURCES_PATH = "PlayerHalthBar";
        private const string ELEMENT_NAME_GREEN_BAR = "green-bar";
        private const string ELEMENT_NAME_RED_BAR = "red-bar";

        private VisualElement _greenBar;
        private VisualElement _redBar;

        private async Task ChangeBarAsync(
            VisualElement bar,
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
