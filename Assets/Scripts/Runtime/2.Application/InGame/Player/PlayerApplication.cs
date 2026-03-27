using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class PlayerApplication
    {
        public PlayerApplication(PlayerMovement movement, PlayerDodgeMovementApplication dodge)
        {
            _movement = movement;
            _dodge = dodge;
        }

        public bool TryDodge(Vector2 input, float time)
            => _dodge.TryDodge(input, time);
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            if (_dodge.IsDodhing)
            {
                _dodge.Update(ref rotation, time, out velocity);
            }
            else
            {
                _movement.Update(ref rotation, input, out velocity);
            }
        }

        private readonly PlayerMovement _movement;
        private readonly PlayerDodgeMovementApplication _dodge;
    }
}
