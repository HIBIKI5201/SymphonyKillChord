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
            Gizmos.DrawLine(_origin, _origin + _direction * _gizmoRayRange);
        }

        /// <summary>
        ///     プレイヤーの前にいる敵にアタック処理を行う。
        /// </summary>
        public void Attack()
        {
            _origin = _player.Player.position + Vector3.up * _heightray;
            _direction = _camera.forward;
            _currentCharacter = FindAttackTarget();
            if (_currentCharacter == null)
            {
                Debug.Log("currentCharacter is null");
                return;
            }

            _currentCharacter.TakeDamage(_playerstatus.AttackPower);
        }

        /// <summary>
        ///     敵を探して見つけたら返す。
        /// </summary>
        /// <returns> 敵の情報 </returns>
        public ICharacter FindAttackTarget()
        {
            if (Physics.Raycast(_origin, _direction,
                    out RaycastHit hitInfo, _config.IgnoreAttackLayer))
            {
                _hitTransform = hitInfo.collider.transform;
                Debug.Log($"Hit: {hitInfo.collider.gameObject.name}");

                _character = _hitTransform.GetComponent<ICharacter>();
            }

            return _character != null ? _character : null;
        }


        private PlayerManager _player;
        private Transform _camera;
        private PlayerStatus _playerstatus;
        private PlayerConfig _config;

        private const float _heightray = 0.7f;
        private const float _gizmoRayRange = 5f;
        private Vector3 _direction;
        private Vector3 _origin;
        private Transform _hitTransform;
        private ICharacter _character;
        private ICharacter _currentCharacter;
    }
}