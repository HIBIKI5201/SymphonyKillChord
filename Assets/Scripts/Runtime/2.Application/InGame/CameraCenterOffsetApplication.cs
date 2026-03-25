using System;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public struct CameraCenterOffsetApplication
    {
        public void Update(
            in Vector3 followPostion,
            in Vector3 cameraRight,
            bool isLockOn,
            float deltaTime,
            out Vector3 resultOffset,
            out float screenVelocityX
        )
        {
            resultOffset = _previousOffset;

            Vector3 velocity = _followVelocity.UpdateFollowVelocity(followPostion, deltaTime);

            screenVelocityX = Vector3.Dot(cameraRight, velocity) / _power;


            if (velocity.sqrMagnitude >= _power * _power)
            {
                velocity.Normalize();
                velocity *= _power;
            }

            if (false && isLockOn)
            {
                float sign = Math.Sign(Vector3.Dot(velocity, cameraRight));
                _signX = sign == 0 ? _signX : sign;

                velocity = _signX * _power * cameraRight;
            }
            resultOffset = -velocity;

            _previousOffset = resultOffset;
        }

        private static readonly float _power = 2f;
        private Vector3 _previousOffset;
        private float _signX;
        private CameraFollowVelocityApplication _followVelocity;
    }
}
