using Mock.MusicBattle.Character;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    /// <summary>
    ///    プレイヤーの攻撃処理をする。
    /// </summary>
    public class PlayerAttacker
    {
        /// <summary>
        ///    コンストラクタ。
        /// </summary>
        public PlayerAttacker(PlayerStatus status, PlayerConfig config, PlayerManager player)
        {
            _status = status;
            _config = config;
            _player = player;
        }

        // PUBLIC_EVENTS
        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        #region Publicメソッド
        /// <summary>
        ///     ギズモを描画して、レイキャストの方向を視覚化します（デバッグ用）。
        /// </summary>
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_origin, _origin + _direction * _status.AttackRange);
        }

        /// <summary>
        ///     指定されたターゲットに攻撃を行います。
        /// </summary>
        /// <param name="target">攻撃対象。</param>
        /// <param name="signature">攻撃の威力を決定する拍子。</param>
        public void Attack(ICharacter target, float signature)
        {
            if (target == null) { return; }

            Vector3 origin = _player.transform.position + Vector3.up * HEIGHT_RAY;

            #region デバッグ用
            _origin = origin;
            _direction = (target.Pivot - _origin).normalized;
            #endregion

            if (!CanAttackTarget(origin, target))
            {
                Debug.Log("Attack target not found or not reachable.");
                return;
            }

            float attackPower = _status.AttackPower * 4 / signature;
            target.TakeDamage(attackPower);
        }
        #endregion

        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        #region 定数
        /// <summary> レイキャストの高さオフセット。 </summary>
        private const float HEIGHT_RAY = 0.7f;
        #endregion

        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> プレイヤーのマネージャクラス。 </summary>
        private readonly PlayerManager _player;
        /// <summary> プレイヤーのステータス。 </summary>
        private readonly PlayerStatus _status;
        /// <summary> プレイヤーの設定。 </summary>
        private readonly PlayerConfig _config;
        #endregion

        #region デバッグ用プライベートフィールド
        /// <summary> デバッグ用のレイキャスト方向。 </summary>
        private Vector3 _direction;
        /// <summary> デバッグ用のレイキャスト開始点。 </summary>
        private Vector3 _origin;
        #endregion

        // UNITY_LIFECYCLE_METHODS
        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        #region Privateメソッド
        /// <summary>
        ///     指定された方向の攻撃対象を見つけ、その成否を返します。
        /// </summary>
        /// <param name="origin">レイキャストの開始点。</param>
        /// <param name="direction">レイキャストの方向。</param>
        /// <param name="character">発見した攻撃対象。</param>
        /// <returns>対象を発見した場合はtrue、それ以外はfalse。</returns>
        private bool TryFindAttackTarget(Vector3 origin, Vector3 direction, out ICharacter character)
        {
            character = FindAttackTarget(origin, direction);
            return character != null;
        }

        /// <summary>
        ///     指定された方向の攻撃対象を探して返します。
        /// </summary>
        /// <param name="origin">レイキャストの開始点。</param>
        /// <param name="direction">レイキャストの方向。</param>
        /// <returns> 発見した攻撃対象。見つからない場合はnull。 </returns>
        private ICharacter FindAttackTarget(Vector3 origin, Vector3 direction)
        {
            ICharacter character = null;
            if (Physics.Raycast(origin, direction,
                    out RaycastHit hitInfo,
                    _status.AttackRange, ~_config.IgnoreAttackLayer))
            {
                Rigidbody rb = hitInfo.collider.attachedRigidbody;
                Debug.Log($"Hit: {hitInfo.collider.name} {rb?.name}");

                character = rb?.GetComponent<ICharacter>();
            }

            return character;
        }

        /// <summary>
        ///     指定されたターゲットが攻撃可能かどうかを判定します。
        /// </summary>
        /// <param name="origin">レイキャストの開始点。</param>
        /// <param name="target">確認するターゲット。</param>
        /// <returns>攻撃可能な場合はtrue、それ以外はfalse。</returns>
        private bool CanAttackTarget(Vector3 origin, ICharacter target)
        {
            if (Physics.Raycast(origin, (target.Pivot - origin).normalized,
                    out RaycastHit hitInfo,
                    _status.AttackRange, ~_config.IgnoreAttackLayer))
            {
                Rigidbody rb = hitInfo.collider.attachedRigidbody;
                Debug.Log($"Hit: {hitInfo.collider.name} {rb?.name}");

                return (rb?.GetComponent<ICharacter>() ?? null) == target;
            }

            return false;
        }
        #endregion
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}