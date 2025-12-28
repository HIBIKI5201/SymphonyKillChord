using Mock.MusicBattle.Character;
using System;
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
        /// <summary>
        ///     <see cref="EnemyHealthBar"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        public EnemyHealthBar()
        {
            style.visibility = Visibility.Hidden;

            // UXMLを読み込んで要素を取得する。
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"UXMLパス: {UXML_RESOURCES_PATH} の読み込みに失敗しました。");
                return;
            }

            treeAsset.CloneTree(this);

            _base = this.Q<VisualElement>(ELEMENT_NAME_BASE);
            _greenBar = this.Q<VisualElement>(ELEMENT_NAME_GREEN_BAR);
            _redBar = this.Q<VisualElement>(ELEMENT_NAME_RED_BAR);

            Debug.Assert(_base != null, $"要素: {ELEMENT_NAME_BASE} の検索に失敗しました。");
            Debug.Assert(_greenBar != null, $"要素: {ELEMENT_NAME_GREEN_BAR} の検索に失敗しました。");
            Debug.Assert(_redBar != null, $"要素: {ELEMENT_NAME_RED_BAR} の検索に失敗しました。");

            if (_greenBar == null || _redBar == null) { return; }

            // 初期状態では満タンにする。
            _greenBar.style.width = Length.Percent(100);
            _redBar.style.width = Length.Percent(100);
        }

        #region Publicメソッド
        /// <summary>
        ///     敵の体力バーのデータをバインドし、表示位置の追跡を開始します。
        /// </summary>
        /// <param name="healthEntity">敵のHealthEntity。</param>
        /// <param name="transform">敵のTransform。</param>
        /// <param name="token">非同期処理のキャンセルトークン。</param>
        public void BindData(HealthEntity healthEntity, Transform transform, CancellationToken token = default)
        {
            _disposeCTS = CancellationTokenSource.CreateLinkedTokenSource(token);

            healthEntity.OnHealthChanged += ChangeHealthBarHandler;

            Action deathAction = null;
            deathAction = () =>
            {
                // ヘルスバーの更新処理を止めて非同期タスクをキャンセルする。
                healthEntity.OnHealthChanged -= ChangeHealthBarHandler;
                healthEntity.OnDeath -= deathAction;
                _disposeCTS.Cancel();
                _disposeCTS.Dispose();

                RemoveFromHierarchy();
            };

            healthEntity.OnDeath += deathAction;

            UpdatePosition(transform);
            style.visibility = Visibility.Visible;
        }
        #endregion

        #region 定数
        /// <summary> UXMLアセットのリソースパス。 </summary>
        private const string UXML_RESOURCES_PATH = "EnemyHealthBar";
        /// <summary> ベース要素のUXML名。 </summary>
        private const string ELEMENT_NAME_BASE = "base";
        /// <summary> 緑色のゲージ要素のUXML名。 </summary>
        private const string ELEMENT_NAME_GREEN_BAR = "green-guage";
        /// <summary> 赤色のゲージ要素のUXML名。 </summary>
        private const string ELEMENT_NAME_RED_BAR = "red-guage";
        #endregion

        #region プライベートフィールド
        /// <summary> CancellationTokenSource。 </summary>
        private CancellationTokenSource _disposeCTS;
        /// <summary> ヘルスバーのベース要素。 </summary>
        private VisualElement _base;
        /// <summary> 緑色のヘルスバー要素。 </summary>
        private VisualElement _greenBar;
        /// <summary> 赤色のヘルスバー要素（ダメージ表現用）。 </summary>
        private VisualElement _redBar;
        /// <summary> 左下基準で位置を大きさの割合で調整するオフセット。 </summary>
        private readonly Vector2 _offset = new Vector2(-0.5f, 0.2f);
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     敵のTransformを追跡し、体力バーの位置を毎フレーム更新します。
        /// </summary>
        /// <param name="transform">追跡する敵のTransform。</param>
        private async void UpdatePosition(Transform transform)
        {
            CancellationToken token = _disposeCTS.Token;

            // 毎フレーム位置を更新する。
            while (transform != null && !token.IsCancellationRequested)
            {
                MovePosition(transform.position);

                try
                {
                    await Awaitable.NextFrameAsync(token);
                }
                // キャンセルされた場合はループを抜ける。
                catch (OperationCanceledException) { break; }
            }
        }

        /// <summary>
        ///     ヘルス変更イベントに応じてヘルスバーを更新するハンドラー。
        /// </summary>
        /// <param name="current">現在の体力。</param>
        /// <param name="max">最大体力。</param>
        private void ChangeHealthBarHandler(float current, float max) => ChangeHealthBar(current, max, _disposeCTS.Token);

        /// <summary>
        ///     ヘルスバーの割合を変更します。
        /// </summary>
        /// <param name="current">現在の体力。</param>
        /// <param name="max">最大体力。</param>
        /// <param name="token">非同期処理のキャンセルトークン。</param>
        private async void ChangeHealthBar(float current, float max, CancellationToken token = default)
        {
            float proportion = Mathf.Clamp01(current / max);

            await _greenBar.ChangeBarAsync(proportion, 0.1f, token);

            await Awaitable.WaitForSecondsAsync(0.3f, token);
            await _redBar.ChangeBarAsync(proportion, 0.2f, token);
        }

        /// <summary>
        ///     体力バーの位置をワールド座標に基づいて移動させます。
        /// </summary>
        /// <param name="worldPosition">体力バーを配置するワールド座標。</param>
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
        #endregion
    }
}

