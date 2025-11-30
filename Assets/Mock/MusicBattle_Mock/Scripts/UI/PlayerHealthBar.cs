using Mock.MusicBattle.Character;
using System;
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

            Debug.Assert(_greenBar != null, $"Failed to find element: {ELEMENT_NAME_GREEN_BAR}");
            Debug.Assert(_redBar != null, $"Failed to find element: {ELEMENT_NAME_RED_BAR}");
        }

        /// <summary>
        ///     データをバインドする。
        /// </summary>
        /// <param name="healthEntity"></param>
        /// <param name="token"></param>
        public void BindData(HealthEntity healthEntity, CancellationToken token = default)
        {
            _token = token;

            healthEntity.OnHealthChanged += ChangeHealthBarHandler;
            healthEntity.OnDeath += () =>
            {
                healthEntity.OnHealthChanged -= ChangeHealthBarHandler;
            };
        }

        private const string UXML_RESOURCES_PATH = "PlayerHealthBar";
        private const string ELEMENT_NAME_GREEN_BAR = "green-guage";
        private const string ELEMENT_NAME_RED_BAR = "red-guage";

        private CancellationToken _token;
        private VisualElement _greenBar;
        private VisualElement _redBar;

        /// <summary>
        ///     イベント登録用のヘルスバー変更ハンドラー。
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        private void ChangeHealthBarHandler(float current, float max) => ChangeHealthBar(current, max, _token);

        /// <summary>
        ///     ヘルスバーの割合を変更する。
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async void ChangeHealthBar(float current, float max, CancellationToken token = default)
        {
            float proportion = Mathf.Clamp01(current / max);

            await _greenBar.ChangeBarAsync(proportion, 0.6f, token);
            await Awaitable.WaitForSecondsAsync(1f, token);
            await _redBar.ChangeBarAsync(proportion, 0.6f, token);
        }
    }
}
