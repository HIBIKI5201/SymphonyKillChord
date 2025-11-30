using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     ダメージテキストの実体クラス。
    /// </summary>
    [UxmlElement]
    public partial class DamageTextEntity : VisualElement
    {
        public DamageTextEntity()
        {
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"Failed to load UXML at path: {UXML_RESOURCES_PATH}");
                return;
            }
            TemplateContainer container = treeAsset.Instantiate();
            hierarchy.Add(container);

            _base = container.Q<VisualElement>(ELEMENT_NAME_BASE);
            _damageText = container.Q<Label>(ELEMENT_NAME_TEXT);
        }

        public void Initialize(Action action, float lifetime)
        {
            _onRelease = action;
            _lifetime = lifetime;
        }

        public async void Show(float damage, Vector3 position)
        {
            if (_base == null || _damageText == null)
            {
                Debug.LogError("DamageTextEntity is not properly initialized.");
                return;
            }

            _damageText.text = damage.ToString("0.0");

            UnityEngine.Camera camera = UnityEngine.Camera.main;
            Vector2 screenPosition = camera.WorldToScreenPoint(position);

            Vector2 centerPos = screenPosition;

            _base.RegisterCallback<GeometryChangedEvent>(MovePosition);

            await Awaitable.WaitForSecondsAsync(_lifetime);

            _onRelease?.Invoke();

            void MovePosition(GeometryChangedEvent evt)
            {
                Vector2 size = new(_base.resolvedStyle.width, _base.resolvedStyle.height);
                Vector2 offset = size / 2;

                centerPos += new Vector2(offset.x, -offset.y);

                style.left = centerPos.x;
                style.top = Screen.height - centerPos.y;

                Debug.Log($"damage text move to {style.left} {style.top}");
            }
        }

        private const string UXML_RESOURCES_PATH = "DamageTextEntity";
        private const string ELEMENT_NAME_BASE = "base";
        private const string ELEMENT_NAME_TEXT = "text";

        private Action _onRelease;
        private float _lifetime;

        private VisualElement _base;
        private Label _damageText;
    }
}
