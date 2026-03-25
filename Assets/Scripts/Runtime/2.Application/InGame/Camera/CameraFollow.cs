using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class CameraFollow
    {
        public CameraFollow(CameraMovementParameter parameter)
        {
            _parameter = parameter;
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
            if (targetFollowCenterOffset.sqrMagnitude >= _parameter.FollowOffsetPower * _parameter.FollowOffsetPower)
            {
                targetFollowCenterOffset.Normalize();
                targetFollowCenterOffset *= _parameter.FollowOffsetPower;
            }

            cameraCenterPosition = Vector3.Lerp(cameraCenterPosition, targetFollowCenterOffset, _parameter.FollowLerpSpeed * deltaTime);
        }

        private readonly CameraMovementParameter _parameter;
        private CameraFollowVelocityApplication _followVelocity;
    }
}
