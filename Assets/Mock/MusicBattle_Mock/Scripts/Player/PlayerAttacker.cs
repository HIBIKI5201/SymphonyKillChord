using Mock.MusicBattle.Character;
using Mock.MusicBattle.Player;
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
        public PlayerAttacker(PlayerStatus status, PlayerConfig Config,
            PlayerManager player)
        {
            _status = status;
            _config = Config;
            _player = player;
        }

        /// <summary>
        /// 　レイキャストの方向を出す。
        /// </summary>
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_origin, _origin + _direction * _status.AttackRange);
        }

        /// <summary>
        ///     プレイヤーの前にいる敵にアタック処理を行う。
        /// </summary>
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
                Debug.Log("currentCharacter is null");
                return;
            }

            float attackPower = _status.AttackPower * 4 / signature;
            target.TakeDamage(attackPower);
        }

        private const float HEIGHT_RAY = 0.7f;

        private readonly PlayerManager _player;
        private readonly PlayerStatus _status;
        private readonly PlayerConfig _config;

        #region デバッグ用
        private Vector3 _direction;
        private Vector3 _origin;
        #endregion

        /// <summary>
        ///     敵を探して発見したか返す。
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private bool TryFindAttackTarget(Vector3 origin, Vector3 direction, out ICharacter character)
        {
            character = FindAttackTarget(origin, direction);
            return character != null;
        }

        /// <summary>
        ///     敵を探して見つけたら返す。
        /// </summary>
        /// <returns> 敵の情報 </returns>
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
    }
}