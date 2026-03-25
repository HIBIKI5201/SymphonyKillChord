using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class CameraRotation
    {
        public CameraRotation() { }

        public void Update(ref Quaternion rotation, in Vector3 playerPosition, in Vector3 targetPosition, float deltaTime)
        {
            Vector3 dir = targetPosition - playerPosition;
            dir.y = 0;

            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            rotation = Quaternion.Lerp(target, rotation, deltaTime * _speed);
        }

        private readonly float _speed = 0.5f;
    }
}
