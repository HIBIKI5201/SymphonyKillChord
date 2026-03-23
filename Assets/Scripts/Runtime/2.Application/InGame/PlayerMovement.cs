using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class PlayerMovement
    {
        public PlayerMovement(
            MoveSpeed speed,
            MoveSpeed dodgeSpeed,
            float dodgeCooldown)
        {
            _speed = speed;
            _dodgeSpeed = dodgeSpeed;
            _dodgedCooldown = dodgeCooldown;
        }

        public Vector3 GetMovedPostion(Vector3 currentPosition, Vector2 input, float deltaTime)
        {
            if (input.sqrMagnitude > 1f)
                input.Normalize();

            return currentPosition + new Vector3(input.x, 0, input.y) * (deltaTime * _speed.Value);
        }
        public bool TryGetDodgedPosition(Vector3 currentPosition, Vector2 input, float currentTime, out Vector3 result)
        {
            result = currentPosition;
            if (_previousDodgedTime + _dodgedCooldown >= currentTime)
                return false;

            _previousDodgedTime = currentTime;

            result = currentPosition + new Vector3(input.x, 0, input.y).normalized * _dodgeSpeed.Value;
            return true;
        }


        private MoveSpeed _speed;
        private MoveSpeed _dodgeSpeed;
        private float _previousDodgedTime;
        private float _dodgedCooldown;
    }
}
