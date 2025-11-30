using Mock.MusicBattle.Character;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     敵の体力バーの実体クラス。
    /// </summary>
    [UxmlElement]
    public partial class EnemyHealthBar : VisualElement
    {
        public EnemyHealthBar()
        {
            style.visibility = Visibility.Hidden;

            // UXMLを読み込んで要素を取得する。
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"Failed to load UXML at path: {UXML_RESOURCES_PATH}");
                return;
            }

            treeAsset.CloneTree(this);

            _base = this.Q<VisualElement>(ELEMENT_NAME_BASE);
            _greenBar = this.Q<VisualElement>(ELEMENT_NAME_GREEN_BAR);
            _redBar = this.Q<VisualElement>(ELEMENT_NAME_RED_BAR);

            Debug.Assert(_base != null, $"Failed to find element: {ELEMENT_NAME_BASE}");
            Debug.Assert(_greenBar != null, $"Failed to find element: {ELEMENT_NAME_GREEN_BAR}");
            Debug.Assert(_redBar != null, $"Failed to find element: {ELEMENT_NAME_RED_BAR}");

            if (_greenBar == null || _redBar == null) { return; }

            // 初期状態では満タンにする。
            _greenBar.style.width = Length.Percent(100);
            _redBar.style.width = Length.Percent(100);
        }

        /// <summary>
        ///     データをバインドする。
        /// </summary>
        /// <param name="healthEntity"></param>
        /// <param name="transform"></param>
        /// <param name="token"></param>
        public void BindData(HealthEntity healthEntity, Transform transform, CancellationToken token = default)
        {
            _disposeCTS = CancellationTokenSource.CreateLinkedTokenSource(token);

            healthEntity.OnHealthChanged += ChangeHealthBarHandler;
            healthEntity.OnDeath += () =>
            {
                // ヘルスバーの更新処理を止めて非同期タスクをキャンセルする。
                healthEntity.OnHealthChanged -= ChangeHealthBarHandler;
                _disposeCTS.Cancel();
                _disposeCTS.Dispose();
            };

            Update(transform);
            style.visibility = Visibility.Visible;
        }

        private const string UXML_RESOURCES_PATH = "EnemyHealthBar";

        private const string ELEMENT_NAME_BASE = "base";
        private const string ELEMENT_NAME_GREEN_BAR = "green-guage";
        private const string ELEMENT_NAME_RED_BAR = "red-guage";

        /// <summary> 左下基準で位置を大きさの割合で調整する。 </summary>
        private readonly Vector2 _offset = new Vector2(-0.5f, 0.2f);

        private CancellationTokenSource _disposeCTS;
        private VisualElement _base;
        private VisualElement _greenBar;
        private VisualElement _redBar;

        private async void Update(Transform transform)
        {
            CancellationToken token = _disposeCTS.Token;

            // 毎フレーム位置を更新する。
            while (transform != null && !token.IsCancellationRequested)
            {
                MovePosition(transform.position);
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

        private void MovePosition(Vector3 worldPosition)
        {
            // カメラからワールド座標をスクリーン座標に変換する。
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            Vector2 screenPosition = camera.WorldToScreenPoint(worldPosition);

            // オフセットを適用してUI Toolkitの座標系に変換する。
            Vector2 offset = new Vector2(
                _offset.x * _base.resolvedStyle.width,
                _offset.y * _base.resolvedStyle.height);

            Vector2 uitkPosition = new Vector2(
                screenPosition.x + offset.x,
                Screen.height - screenPosition.y + offset.y);

            // ベースの位置を更新する。
            _base.style.left = uitkPosition.x;
            _base.style.top = uitkPosition.y;
        }
    }
}
