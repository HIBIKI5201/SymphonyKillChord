using System;
using System.Threading.Tasks; // Awaitable.WaitForSecondsAsyncを使用するため追加
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     ダメージテキストの表示を管理するUI ToolkitのカスタムVisualElement。
    /// </summary>
    [UxmlElement]
    public partial class DamageTextEntity : VisualElement
    {
        /// <summary>
        ///     <see cref="DamageTextEntity"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        public DamageTextEntity()
        {
            // UXMLを読み込んで要素を取得する。
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"UXMLパス: {UXML_RESOURCES_PATH} の読み込みに失敗しました。");
                return;
            }

            treeAsset.CloneTree(this);

            _base = this.Q<VisualElement>(ELEMENT_NAME_BASE);
            _damageText = this.Q<Label>(ELEMENT_NAME_TEXT);
        }

        // PUBLIC_EVENTS
        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        #region Publicメソッド
        /// <summary>
        ///     ダメージテキストエンティティを初期化します。
        /// </summary>
        /// <param name="action">リリース時に呼び出されるアクション。</param>
        /// <param name="lifetime">ダメージテキストの表示時間。</param>
        public void Initialize(Action action, float lifetime)
        {
            _onRelease = action;
            _lifetime = lifetime;
        }

        /// <summary>
        ///     ダメージテキストを表示します。
        /// </summary>
        /// <param name="damage">表示するダメージ量。</param>
        /// <param name="position">ダメージテキストを表示するワールド座標。</param>
        public async void Show(float damage, Vector3 position)
        {
            if (_base == null || _damageText == null)
            {
                Debug.LogError("DamageTextEntityが正しく初期化されていません。");
                return;
            }

            _damageText.text = damage.ToString("0.0");

            // ベースの初期化が完了したら位置を設定する。
            _base.RegisterCallback<GeometryChangedEvent>(MovePosition);

            // 指定された寿命だけ待機する。
            await Awaitable.WaitForSecondsAsync(_lifetime);

            // リリース処理を呼び出す。
            _onRelease?.Invoke();

            /// <summary>
            ///     ダメージテキストの位置を更新するローカル関数。
            /// </summary>
            /// <param name="evt">GeometryChangedEvent。</param>
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
        #endregion

        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        #region 定数
        /// <summary> UXMLアセットのリソースパス。 </summary>
        private const string UXML_RESOURCES_PATH = "DamageTextEntity";
        /// <summary> ベース要素のUXML名。 </summary>
        private const string ELEMENT_NAME_BASE = "base";
        /// <summary> テキスト要素のUXML名。 </summary>
        private const string ELEMENT_NAME_TEXT = "text";
        #endregion

        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> リリース時に呼び出されるアクション。 </summary>
        private Action _onRelease;
        /// <summary> ダメージテキストの表示時間。 </summary>
        private float _lifetime;
        /// <summary> ダメージテキストのベースVisualElement。 </summary>
        private VisualElement _base;
        /// <summary> ダメージテキストを表示するLabel。 </summary>
        private Label _damageText;
        #endregion

        // UNITY_LIFECYCLE_METHODS
        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
