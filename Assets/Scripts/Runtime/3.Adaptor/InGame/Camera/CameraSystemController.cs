using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public sealed class CameraSystemController
    {
        public CameraSystemController(CameraSystemApplication application)
        {
            _application = application;
        }
        public void Update(
            in Vector3 followPosition,
            in Vector3 targetPosition,
            bool isLockOn,
            float deltaTime,
            out Quaternion resultRotation,
            out Vector3 resultPosition
            )
        {
            _application.Update(followPosition, targetPosition, isLockOn, deltaTime, out resultRotation, out resultPosition);
        }

        private readonly CameraSystemApplication _application;
    }
}
