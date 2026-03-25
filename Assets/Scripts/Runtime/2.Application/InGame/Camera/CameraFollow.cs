using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class CameraFollow
    {
        public CameraFollow(float offsetPower)
        {
            _offsetPower = offsetPower;
        }

        public void Update(
            ref Vector3 cameraCenterPosition,
            in Vector3 followPostion,
            in Vector3 cameraRight,
            bool isLockOn,
            float deltaTime
            )
        {
            Vector3 targetFollowCenterOffset = -_followVelocity.UpdateFollowVelocity(followPostion, deltaTime);
            targetFollowCenterOffset.y = 0;
            if (targetFollowCenterOffset.sqrMagnitude >= _offsetPower * _offsetPower)
            {
                targetFollowCenterOffset.Normalize();
                targetFollowCenterOffset *= _offsetPower;
            }

            cameraCenterPosition = Vector3.Lerp(cameraCenterPosition, targetFollowCenterOffset, _speed * deltaTime);
        }

        private readonly float _speed = 1.0f;
        private readonly float _offsetPower = 2f;
        private CameraFollowVelocityApplication _followVelocity;
    }
}
