using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View;
using KillChord.Structure;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class CameraInitializer : MonoBehaviour
    {
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private CameraConfigs _cameraConfigs;
        [SerializeField] private Transform _followTarget;
        [SerializeField] private Transform _initialLockTarget;

        private void Awake()
        {
            if (_cameraManager == null)
                Debug.LogError($"{nameof(CameraManager)}がNullです", this);
            if (_cameraConfigs == null)
                Debug.LogError($"{nameof(CameraConfigs)}がNullです", this);
            if (_followTarget == null)
                Debug.LogError($"{nameof(_followTarget)}がNullです", this);
            if (_initialLockTarget == null)
                Debug.LogError($"{nameof(_initialLockTarget)}がNullです", this);

            CameraCollisionResolver collisionResolver = new();
            CameraParameter parameter = _cameraConfigs.ToDomain();
            CameraApplication application = new(parameter, collisionResolver.TryResolve);
            CameraController controller = new(application);

            _cameraManager.Init(controller, _followTarget);
            _cameraManager.SetLockTarget(_initialLockTarget);

#if UNITY_EDITOR
            _cameraManager.gameObject
                .AddComponent<CameraParameterDebug>()
                .SetCameraParameter(parameter);
#endif
        }
    }
}
