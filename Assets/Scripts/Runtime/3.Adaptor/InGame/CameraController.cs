using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public sealed class CameraController
    {
        public CameraController(CameraApplication application)
        {
            _application = application;
        }

        public void Rotate(Vector2 input) => _application.RotateCamera(input);

        public void SetLockTarget(Transform target) => _application.SetLockTarget(target);

        public void Tick(Transform camera, Transform followTarget, float deltaTime)
            => _application.Tick(camera, followTarget, deltaTime);

        public void DrawGizmos(Transform camera) => _application.DrawLockGizmos(camera);

        private readonly CameraApplication _application;
    }
}
