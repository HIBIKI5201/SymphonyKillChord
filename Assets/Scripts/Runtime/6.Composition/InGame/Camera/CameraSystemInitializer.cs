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
    public sealed class CameraSystemInitializer : MonoBehaviour
    {
        [SerializeField] private CameraSystemView _cameraSystem;

        [SerializeField] private CameraSystemConfig _config;

        private void Awake()
        {
            CameraSystemParameter parameter = _config.ToDomain();

            CameraBoneLockOnRotationApplication boneRotationSystem = new(parameter);
            CameraRotationApplication rotationSystem = new(parameter);
            CameraFollowApplication followSystem = new(parameter);
            CameraSystemApplication application = new(parameter, followSystem, boneRotationSystem, rotationSystem);

            CameraSystemController controller = new(application);
            _cameraSystem.Init(controller);

#if UNITY_EDITOR
            _cameraSystem.gameObject
                .AddComponent<CameraSystemParameterDebug>()
                .SetCameraParameter(parameter);
#endif
        }
    }
}
