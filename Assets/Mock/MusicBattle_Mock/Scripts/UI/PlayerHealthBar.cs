using Mock.MusicBattle.Character;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     プレイヤーの体力バーの実体クラス。
    /// </summary>
    [UxmlElement]
    public partial class PlayerHealthBar : VisualElement
    {
        /// <summary>
        ///     <see cref="PlayerHealthBar"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        public PlayerHealthBar()
        {
            style.position = Position.Absolute;
            style.width = Length.Percent(100);
            style.height = Length.Percent(100);

            // UXMLを読み込んで初期化する。
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"Failed to load UXML at path: {UXML_RESOURCES_PATH}");
                return;
            }

            treeAsset.CloneTree(this);

            _greenBar = this.Q<VisualElement>(ELEMENT_NAME_GREEN_BAR);
            _redBar = this.Q<VisualElement>(ELEMENT_NAME_RED_BAR);

            Debug.Assert(_greenBar != null, $"Failed to find element: {ELEMENT_NAME_GREEN_BAR}");
            Debug.Assert(_redBar != null, $"Failed to find element: {ELEMENT_NAME_RED_BAR}");

            if (_greenBar == null || _redBar == null) { return; }

            // 初期状態では満タンにしておく。
            _greenBar.style.width = Length.Percent(100);
            _redBar.style.width = Length.Percent(100);
        }

        // PUBLIC_EVENTS
        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        #region Publicメソッド
        /// <summary>
        ///     データをバインドします。
        /// </summary>
        /// <param name="healthEntity">バインドするHealthEntity。</param>
        /// <param name="token">非同期処理のキャンセルトークン。</param>
        public void BindData(HealthEntity healthEntity, CancellationToken token = default)
        {
            _token = token;

            healthEntity.OnHealthChanged += ChangeHealthBarHandler;
            healthEntity.OnDeath += () =>
            {
                // 死亡時にはイベントを解放する。
                healthEntity.OnHealthChanged -= ChangeHealthBarHandler;
            };
        }
        #endregion

        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        #region 定数
        /// <summary> UXMLアセットのリソースパス。 </summary>
        private const string UXML_RESOURCES_PATH = "PlayerHealthBar";
        /// <summary> 緑色のゲージ要素のUXML名。 </summary>
        private const string ELEMENT_NAME_GREEN_BAR = "green-guage";
        /// <summary> 赤色のゲージ要素のUXML名。 </summary>
        private const string ELEMENT_NAME_RED_BAR = "red-guage";
        #endregion

        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> 非同期処理のキャンセルトークン。 </summary>
        private CancellationToken _token;
        /// <summary> 緑色のゲージバー。 </summary>
        private VisualElement _greenBar;
        /// <summary> 赤色のゲージバー（ダメージ表現用）。 </summary>
        private VisualElement _redBar;
        #endregion

        // UNITY_LIFECYCLE_METHODS
        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        #region Privateメソッド
        /// <summary>
        ///     ヘルス変更イベントに応じてヘルスバーを更新するハンドラー。
        /// </summary>
        /// <param name="current">現在の体力。</param>
        /// <param name="max">最大体力。</param>
        private void ChangeHealthBarHandler(float current, float max) => ChangeHealthBar(current, max, _token);

        /// <summary>
        ///     ヘルスバーの表示を指定された値にアニメーション付きで変更します。
        /// </summary>
        /// <param name="current">現在の体力。</param>
        /// <param name="max">最大体力。</param>
        /// <param name="token">非同期処理のキャンセルトークン。</param>
        private async void ChangeHealthBar(float current, float max, CancellationToken token = default)
        {
            float proportion = Mathf.Clamp01(current / max);

            // 緑バーを先に変更。
            await _greenBar.ChangeBarAsync(proportion, 0.3f, token);

            // 少し待ってから赤バーを変更。
            await Awaitable.WaitForSecondsAsync(0.8f, token);
            await _redBar.ChangeBarAsync(proportion, 0.6f, token);
        }
        #endregion
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
