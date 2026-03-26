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
    public sealed class TestCameraInitializer : MonoBehaviour
    {
        [SerializeField] private TestCameraSystem _cameraSystem;

        [SerializeField] private CameraMovementConfig _config;

        private void Awake()
        {
            CameraMovementParameter parameter = _config.ToDomain();

            CameraBoneRotation boneRotationSystem = new(parameter);
            CameraRotation rotationSystem = new(parameter);
            CameraFollow followSystem = new(parameter);
            TestCameraApplication application = new(parameter, followSystem, boneRotationSystem, rotationSystem);

            TestCameraController controller = new(application);
            _cameraSystem.Init(controller);

#if UNITY_EDITOR
            _cameraSystem.gameObject
                .AddComponent<TestCameraParameterDebug>()
                .SetCameraParameter(parameter);
#endif
        }
    }
}
