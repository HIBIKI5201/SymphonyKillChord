using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Player
{
    /// <summary>
    ///     プレイヤーの移動や回避といったアクションを管理するアプリケーション層のクラス。
    /// </summary>
    public sealed class PlayerApplication
    {
        public PlayerApplication(PlayerMovementApplication movement, PlayerDodgeMovementApplication dodge)
        {
            _movement = movement;
            _dodge = dodge;
        }

        public bool TryDodge(Vector2 input, float time)
            => _dodge.TryDodge(input, time);
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            if (_dodge.IsDodging)
            {
                _dodge.Update(ref rotation, time, out velocity);
            }
            else
            {
                _movement.Update(ref rotation, input, out velocity);
            }
        }

        private readonly PlayerMovementApplication _movement;
        private readonly PlayerDodgeMovementApplication _dodge;
    }
}
