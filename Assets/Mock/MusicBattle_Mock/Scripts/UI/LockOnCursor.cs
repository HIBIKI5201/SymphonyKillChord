using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle
{
    [UxmlElement]
    public partial class LockOnCursor : VisualElement
    {
        public LockOnCursor()
        {
            // UXMLを読み込んで要素を取得する。
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"Failed to load UXML at path: {UXML_RESOURCES_PATH}");
                return;
            }

            treeAsset.CloneTree(this);

            _cursor = this.Q<VisualElement>(ELEMENT_CURSOR_NAME);
            style.visibility = Visibility.Hidden;
        }

        public void RegisterTarget(Transform target)
        {
            _target = target;

            // ターゲットの状態による可視状態を確認。
            Visibility visibility = target != null ? Visibility.Visible : Visibility.Hidden;
            if (style.visibility != visibility) { style.visibility = visibility; }
        }
        public void UpdatePosition()
        {
            if (_target == null) { return; }

            // カメラからワールド座標をスクリーン座標に変換する。
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            Vector2 screenPosition = camera.WorldToScreenPoint(_target.position + OFFSET);

            // オフセットを適用してUI Toolkitの座標系に変換する。
            Vector2 offset = new Vector2(
                _cursor.resolvedStyle.width,
                _cursor.resolvedStyle.height);

            Vector2 uitkPosition = new Vector2(
                screenPosition.x + offset.x,
                Screen.height - screenPosition.y + offset.y);

            // ベースの位置を更新する。
            _cursor.style.left = uitkPosition.x;
            _cursor.style.top = uitkPosition.y;
        }

        private const string UXML_RESOURCES_PATH = "LockOnCursor";

        private const string ELEMENT_CURSOR_NAME = "cursor";

        private readonly Vector3 OFFSET = new Vector2(0, 1);

        private readonly VisualElement _cursor;
        private Transform _target;
    }
}
