using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class PlayerDodgeMovementApplication
    {
        public PlayerDodgeMovementApplication(PlayerMoveParameter parameter)
        {
            _parameter = parameter;

            _previousDodgedTime = -1;
        }

        public bool IsDodhing => _isDodging;

        public bool TryDodge(Vector2 input, float currentTime)
        {
            if (input.sqrMagnitude <= float.Epsilon)
                return false;

            if (currentTime - _previousDodgedTime < _parameter.DodgeCooldown)
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

            if (time > _previousDodgedTime + _parameter.DodgeDuration)
            {
                _isDodging = false;
                return;
            }

            position += (float)_parameter.DodgeSpeed * deltaTime * _direction;
            rotaition = Quaternion.LookRotation(_direction, Vector3.up);
        }

        private readonly PlayerMoveParameter _parameter;

        private Vector3 _direction;
        private float _previousDodgedTime;
        private bool _isDodging;
    }
}
