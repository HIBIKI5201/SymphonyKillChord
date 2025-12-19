using Mock.MusicBattle.Character;
using Mock.MusicBattle.UI;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    /// <summary>
    ///     プレイヤーのヘルスバーのデバッグ用コンポーネント。
    /// </summary>
    public class PlayerHealthBarDebugger : MonoBehaviour
    {
        #region インスペクター表示フィールド
        /// <summary> 最大ヘルス量。 </summary>
        [SerializeField, Tooltip("最大ヘルス量。")]
        private float _maxHealth = 100f;
        /// <summary> ダメージ量。 </summary>
        [SerializeField, Tooltip("ダメージ量。")]
        private float _damage = 10;
        #endregion

        #region プライベートフィールド
        /// <summary> HUDマネージャーの参照。 </summary>
        private IngameHUDManager _hud;
        /// <summary> ヘルスエンティティ。 </summary>
        private HealthEntity _healthEntity;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     IngameHUDManagerのインスタンスを検索して取得します。
        /// </summary>
        private void Awake()
        {
            _hud = FindAnyObjectByType<IngameHUDManager>();
        }

        /// <summary>
        ///     最初のフレームアップデートの前に呼び出されます。
        ///     HealthEntityを初期化し、プレイヤーのヘルスバーをHUDに初期化します。
        /// </summary>
        private void Start()
        {
            _healthEntity = new HealthEntity(_maxHealth);
            _hud.InitializePlayerHealthBar(_healthEntity);
        }
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     コンテキストメニューからヘルス値を適用します。
        ///     HealthEntityにダメージを与えます。
        /// </summary>
        [ContextMenu(nameof(ApplyHealthValue))]
        private void ApplyHealthValue()
        {
            _healthEntity.TakeDamage(_damage);
        }
        #endregion
    }
}
