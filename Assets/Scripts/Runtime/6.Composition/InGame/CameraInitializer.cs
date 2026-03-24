using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.View;
using KillChord.Structure;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public sealed class CameraInitializer : MonoBehaviour
    {
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private CameraConfigs _cameraConfigs;
        [SerializeField] private Transform _followTarget;
        [SerializeField] private Transform _initialLockTarget;

        private void Awake()
        {
            CameraCollisionResolver collisionResolver = new();
            CameraApplication application = new(_cameraConfigs.ToDomain(), collisionResolver.TryResolve);
            CameraController controller = new(application);

            _cameraManager.Init(controller, _followTarget);
            _cameraManager.SetLockTarget(_initialLockTarget);
        }
    }
}
