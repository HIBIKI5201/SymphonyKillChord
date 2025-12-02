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
            // UXMLを読み込んで要素を取得する。
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"Failed to load UXML at path: {UXML_RESOURCES_PATH}");
                return;
            }

            treeAsset.CloneTree(this);

            _base = this.Q<VisualElement>(ELEMENT_NAME_BASE);
            _damageText = this.Q<Label>(ELEMENT_NAME_TEXT);
        }

        /// <summary>
        ///     インスタンス生成時に初期化する。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="lifetime"></param>
        public void Initialize(Action action, float lifetime)
        {
            _onRelease = action;
            _lifetime = lifetime;
        }

        /// <summary>
        ///     ダメージテキストを表示する。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="position"></param>
        public async void Show(float damage, Vector3 position)
        {
            if (_base == null || _damageText == null)
            {
                Debug.LogError("DamageTextEntity is not properly initialized.");
                return;
            }

            _damageText.text = damage.ToString("0.0");

            // ベースの初期化が完了したら位置を設定する。
            _base.RegisterCallback<GeometryChangedEvent>(MovePosition);

            // 指定された寿命だけ待機する。
            await Awaitable.WaitForSecondsAsync(_lifetime);

            // リリース処理を呼び出す。
            _onRelease?.Invoke();

            // 位置を更新するローカル関数。
            void MovePosition(GeometryChangedEvent evt)
            {
                // イベントが重複しないように解約。
                _base.UnregisterCallback<GeometryChangedEvent>(MovePosition);

                // ワールド座標をスクリーン座標に変換する。
                UnityEngine.Camera camera = UnityEngine.Camera.main;
                Vector2 screenPosition = camera.WorldToScreenPoint(position);

                // 大きさの半分だけオフセットをかけて中央上に表示する。
                Vector2 offset = new Vector2(
                    -_base.resolvedStyle.width / 2,
                    _base.resolvedStyle.height / 2);

                // UI Toolkitの座標系に変換する。
                Vector2 uitkPosition = new Vector2(
                    screenPosition.x + offset.x,
                    Screen.height - screenPosition.y + offset.y);

                // ベースの位置を更新する。
                _base.style.left = uitkPosition.x;
                _base.style.top = uitkPosition.y;
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
