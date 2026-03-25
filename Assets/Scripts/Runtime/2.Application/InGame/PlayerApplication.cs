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
        public void Update(ref Vector3 position, ref Quaternion rotation, Vector2 input, float time, float deltaTime)
        {
            if (_dodge.IsDodhing)
            {
                _dodge.Update(ref position, ref rotation, time, deltaTime);
            }
            else
            {
                _movement.Update(ref position, ref rotation, input, deltaTime);
            }
        }

        private readonly PlayerMovement _movement;
        private readonly PlayerDodgeMovementApplication _dodge;
    }
}
