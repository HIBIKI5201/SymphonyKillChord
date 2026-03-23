using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class PlayerMovement
    {
        public PlayerMovement(
            MoveSpeed speed,
            MoveSpeed dashSpeed,
            float dashCooldown)
        {
            _speed = speed;
            _dashSpeed = dashSpeed;
            _dashCooldown = dashCooldown;
        }

        public Vector3 GetMovedPostion(Vector3 currentPosition, Vector2 input, float deltaTime)
        {
            return currentPosition + new Vector3(input.x, 0, input.y).normalized * (deltaTime * _speed.Value);
        }
        public bool TryGetDashedPosition(Vector3 currentPosition, Vector2 input, float currentTime, out Vector3 result)
        {
            result = currentPosition;
            if (_previousDashedTime + _dashCooldown >= currentTime)
                return false;

            _previousDashedTime = currentTime;

            result = currentPosition + new Vector3(input.x, 0, input.y).normalized * _dashSpeed.Value;
            return true;
        }


        private MoveSpeed _speed;
        private MoveSpeed _dashSpeed;
        private float _previousDashedTime;
        private float _dashCooldown;
    }
}
