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
    }
}

