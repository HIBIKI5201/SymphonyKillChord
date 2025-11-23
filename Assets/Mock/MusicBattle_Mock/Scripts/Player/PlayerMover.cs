using UnityEngine;

namespace Mock.MusicBattle
{
    public class PlayerMover
    {
        public PlayerMover(PlayerStatus status, Rigidbody rb, Transform player, Transform camera)
        {
            _status = status;
            _player = player;
            _camera = camera;
            _rb = rb;
        }

        private readonly PlayerStatus _status;
        private readonly Transform _player;
        private readonly Transform _camera;
        private readonly Rigidbody _rb;
        private Vector3 _velocity;
        private bool _isGround;
    }
}
