using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public class PlayerController
    {
        public PlayerController(PlayerMovement movement, PlayerDodgeMovementApplication dodgeMovement)
        {
            _movement = movement;
            _dodgeMovement = dodgeMovement;
        }

        public bool TryDodge(Vector2 input, float time)
            => _dodgeMovement.TryDodge(input, time);
        public void Update(ref Vector3 position, ref Quaternion rotation, Vector2 input, float time, float deltaTime)
        {
            if (_dodgeMovement.IsDodhing)
            {
                _dodgeMovement.Update(ref position, ref rotation, time, deltaTime);
            }
            else
            {
                _movement.Update(ref position, ref rotation, input, deltaTime);
            }
        }


        private readonly PlayerMovement _movement;
        private readonly PlayerDodgeMovementApplication _dodgeMovement;
    }
}
