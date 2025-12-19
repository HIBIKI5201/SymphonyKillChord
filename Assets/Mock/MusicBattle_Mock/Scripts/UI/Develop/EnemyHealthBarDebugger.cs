using Mock.MusicBattle.Character;
using Mock.MusicBattle.UI;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    /// <summary>
    ///     敵のヘルスバーのデバッグ用コンポーネント。
    /// </summary>
    public class EnemyHealthBarDebugger : MonoBehaviour
    {
        // CONSTRUCTOR
        // PUBLIC_EVENTS
        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        // PUBLIC_METHODS
        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        #region インスペクター表示フィールド
        /// <summary> 最大ヘルス量。 </summary>
        [SerializeField, Tooltip("最大ヘルス量。")]
        private float _maxHealth = 100f;
        /// <summary> ダメージ量。 </summary>
        [SerializeField, Tooltip("ダメージ量。")]
        private float _damage = 10;
        /// <summary> HUDマネージャーの参照。 </summary>
        [SerializeField, Tooltip("HUDマネージャーの参照。")]
        private IngameHUDManager _hud;
        #endregion

        #region プライベートフィールド
        /// <summary> ヘルスエンティティ。 </summary>
        private HealthEntity _healthEntity;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     最初のフレームアップデートの前に呼び出されます。
        ///     HealthEntityを初期化し、敵のヘルスバーをHUDに追加します。
        /// </summary>
        private void Start()
        {
            _healthEntity = new HealthEntity(_maxHealth);
            _ = _hud.AddEnemyHealthBar(_healthEntity, transform);
        }
        #endregion

        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        #region Privateメソッド
        /// <summary>
        ///     コンテキストメニューからヘルス値を適用します。
        ///     ダメージを与え、ダメージテキストを表示します。
        /// </summary>
        [ContextMenu(nameof(ApplyHealthValue))]
        private void ApplyHealthValue()
        {
            _healthEntity.TakeDamage(_damage);
            _hud.ShowDamageText(_damage, transform.position);
        }
        #endregion
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
