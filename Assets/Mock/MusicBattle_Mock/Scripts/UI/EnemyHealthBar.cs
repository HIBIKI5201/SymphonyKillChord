using Mock.MusicBattle.Character;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    [UxmlElement]
    public partial class EnemyHealthBar : VisualElement
    {
        public EnemyHealthBar()
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
            _greenBar = container.Q<VisualElement>(ELEMENT_NAME_GREEN_BAR);
            _redBar = container.Q<VisualElement>(ELEMENT_NAME_RED_BAR);

            Debug.Assert(_base != null, $"Failed to find element: {ELEMENT_NAME_BASE}");
            Debug.Assert(_greenBar != null, $"Failed to find element: {ELEMENT_NAME_GREEN_BAR}");
            Debug.Assert(_redBar != null, $"Failed to find element: {ELEMENT_NAME_RED_BAR}");
        }

        public void Initialize(HealthEntity healthEntity, Transform transform, CancellationToken token = default)
        {
            _disposeCTS = CancellationTokenSource.CreateLinkedTokenSource(token);

            healthEntity.OnHealthChanged += ChangeHealthBarHandler;
            healthEntity.OnDeath += () =>
            {
                healthEntity.OnHealthChanged -= ChangeHealthBarHandler;
                _disposeCTS.Cancel();
                _disposeCTS.Dispose();
            };

            Update(transform);
        }

        public void SetPosition(Vector3 worldPosition)
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            Vector2 screenPosition = camera.WorldToScreenPoint(worldPosition);

            Vector2 offset = new Vector2(
                _offset.x * _base.resolvedStyle.width,
                _offset.y * _base.resolvedStyle.height);

            Vector2 uitkPosition = new Vector2(
                screenPosition.x + offset.x,
                Screen.height - screenPosition.y + offset.y);

            _base.style.left = uitkPosition.x;
            _base.style.top = uitkPosition.y;
        }

        private const string UXML_RESOURCES_PATH = "EnemyHealthBar";

        private const string ELEMENT_NAME_BASE = "base";
        private const string ELEMENT_NAME_GREEN_BAR = "green-guage";
        private const string ELEMENT_NAME_RED_BAR = "red-guage";

        private readonly Vector2 _offset = new Vector2(-0.5f, 0.2f); // 左下基準。

        private CancellationTokenSource _disposeCTS;
        private VisualElement _base;
        private VisualElement _greenBar;
        private VisualElement _redBar;

        private async void Update(Transform transform)
        {
            CancellationToken token = _disposeCTS.Token;

            while (transform != null && !token.IsCancellationRequested)
            {
                SetPosition(transform.position);
                await Awaitable.NextFrameAsync(token);
            }
        }

        private void ChangeHealthBarHandler(float current, float max) => ChangeHealthBar(current, max, _disposeCTS.Token);

        private async void ChangeHealthBar(float current, float max, CancellationToken token = default)
        {
            float proportion = Mathf.Clamp01(current / max);

            await _greenBar.ChangeBarAsync(proportion, 0.4f, token);
            await Awaitable.WaitForSecondsAsync(0.5f, token);
            await _redBar.ChangeBarAsync(proportion, 0.4f, token);
        }
    }
}
