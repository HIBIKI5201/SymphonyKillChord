using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class PlayerDodgeMovementApplication
    {
        public PlayerDodgeMovementApplication(MoveSpeed dodgeSpeed, float duration, float cooldown)
        {
            _dodgeSpeed = dodgeSpeed;
            _dodgeDuration = duration;
            _dodgedCooldown = cooldown;

            _previousDodgedTime = -1;
        }

        public bool IsDodhing => _isDodging;

        public bool TryDodge(Vector2 input, float currentTime)
        {
            if (input == Vector2.zero)
                return false;

            if (currentTime - _previousDodgedTime < _dodgedCooldown)
                return false;

            _previousDodgedTime = currentTime;
            _direction = new Vector3(input.x, 0, input.y).normalized;
            _isDodging = true;

            return true;
        }
        public void Update(ref Vector3 position, ref Quaternion rotaition, float time, float deltaTime)
        {
            if (!_isDodging)
                return;

            if (time > _previousDodgedTime + _dodgeDuration)
            {
                _isDodging = false;
                return;
            }

            position += _dodgeSpeed.Value * deltaTime * _direction;
            rotaition = Quaternion.LookRotation(_direction, Vector3.up);
        }


        private readonly MoveSpeed _dodgeSpeed;
        private readonly float _dodgeDuration;
        private readonly float _dodgedCooldown;

        private Vector3 _direction;
        private float _previousDodgedTime;
        private bool _isDodging;
    }
}
