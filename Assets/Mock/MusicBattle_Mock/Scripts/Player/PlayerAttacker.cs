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
            _status = status;
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
            Gizmos.DrawLine(_origin, _origin + _direction * _status.AttackRange);
        }

        /// <summary>
        ///     プレイヤーの前にいる敵にアタック処理を行う。
        /// </summary>
        public void Attack()
        {
            _origin = _player.transform.position + Vector3.up * HEIGHT_RAY;
            _direction = _camera.forward;
            if (!TryFindAttackTarget(_origin, _direction, out ICharacter target))
            {
                Debug.Log("currentCharacter is null");
                return;
            }

            target.TakeDamage(_status.AttackPower);
        }

        /// <summary>
        ///     敵を探して発見したか返す。
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool TryFindAttackTarget(Vector3 origin, Vector3 direction, out ICharacter character)
        {
            character = FindAttackTarget(origin, direction);
            return character != null;
        }

        /// <summary>
        ///     敵を探して見つけたら返す。
        /// </summary>
        /// <returns> 敵の情報 </returns>
        public ICharacter FindAttackTarget(Vector3 origin, Vector3 direction)
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


        private PlayerManager _player;
        private Transform _camera;
        private PlayerStatus _status;
        private PlayerConfig _config;

        private const float HEIGHT_RAY = 0.7f;
        private Vector3 _direction;
        private Vector3 _origin;
    }
}