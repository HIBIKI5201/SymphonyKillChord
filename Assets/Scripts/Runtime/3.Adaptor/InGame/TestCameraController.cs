using KillChord.Runtime.Application;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public sealed class TestCameraController
    {
        public TestCameraController(TestCameraApplication application)
        {
            _application = application;
        }

        [Obsolete("", true)]
        public void Update(
            ref Vector3 cameraCenterPosition,
            ref Quaternion boneRotation,
            ref Quaternion cameraRotation,
            in Vector3 followPostion,
            in Vector3 targetPosition,
            in Vector3 cameraPosition,
            bool isLockOn,
            float deltaTime
            )
        {
        }
        public void Update(
            in Vector3 followPostion,
            in Vector3 targetPosition,
            bool isLockOn,
            float deltaTime,
            out Quaternion resultRotation,
            out Vector3 resultPosition
            )
        {
            _application.Update(followPostion, targetPosition, isLockOn, deltaTime, out resultRotation, out resultPosition);
        }

        private readonly TestCameraApplication _application;
    }
}
