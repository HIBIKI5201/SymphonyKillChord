using Mock.MusicBattle.Character;
using Mock.MusicBattle.Player;
using UnityEngine;

namespace Mock.MusicBattle
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
            PlayerManager player, Transform camera)
        {
            _playerstatus = status;
            _config = Config;
            _player = player;
            _camera = camera;
        }

        /// <summary>
        /// 　レイキャストの方向を出す。
        /// </summary>
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_origin, _origin + _direction * GIZMO_RAY_RANGE);
        }

        /// <summary>
        ///     プレイヤーの前にいる敵にアタック処理を行う。
        /// </summary>
        public void Attack()
        {
            _origin = _player.Player.position + Vector3.up * HEIGHT_RAY;
            _direction = _camera.forward;
            if (!TryFindAttackTarget(out ICharacter target))
            {
                Debug.Log("currentCharacter is null");
                return;
            }

            target.TakeDamage(_playerstatus.AttackPower);
        }

        /// <summary>
        ///     敵を探して発見したか返す。
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool TryFindAttackTarget(out ICharacter character)
        {
            character = FindAttackTarget();
            return character != null;
        }

        /// <summary>
        ///     敵を探して見つけたら返す。
        /// </summary>
        /// <returns> 敵の情報 </returns>
        public ICharacter FindAttackTarget()
        {
            ICharacter character = null;
            if (Physics.Raycast(_origin, _direction,
                    out RaycastHit hitInfo,
                    _playerstatus.AttackRange,  _config.IgnoreAttackLayer))
            {
                Rigidbody rb = hitInfo.collider.attachedRigidbody;
                Debug.Log($"Hit: {hitInfo.collider.name} {rb?.name}");

                character = rb?.GetComponent<ICharacter>();
            }

            return character;
        }


        private PlayerManager _player;
        private Transform _camera;
        private PlayerStatus _playerstatus;
        private PlayerConfig _config;

        private const float HEIGHT_RAY = 0.7f;
        private const float GIZMO_RAY_RANGE = 5f;
        private Vector3 _direction;
        private Vector3 _origin;
    }
}