using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class CameraFollowApplication
    {
        public CameraFollowApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(
            ref Vector3 cameraCenterPosition,
            in Vector3 followPosition,
            float deltaTime
            )
        {
            Vector3 targetFollowCenterOffset = -_followVelocity.UpdateFollowVelocity(followPosition, deltaTime);
            targetFollowCenterOffset.y = 0;
            if (targetFollowCenterOffset.sqrMagnitude >= _parameter.FollowOffsetPower * _parameter.FollowOffsetPower)
            {
                targetFollowCenterOffset.Normalize();
                targetFollowCenterOffset *= _parameter.FollowOffsetPower;
            }

            cameraCenterPosition = Vector3.Lerp(cameraCenterPosition, targetFollowCenterOffset, _parameter.FollowLerpSpeed * deltaTime);
        }

        private readonly CameraSystemParameter _parameter;
        private CameraFollowVelocityApplication _followVelocity;
    }
}
