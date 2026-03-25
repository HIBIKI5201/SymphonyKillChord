using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class TestCameraInitializer : MonoBehaviour
    {
        [SerializeField] private TestCameraSystem _cameraSystem;

        //[SerializeField] private Config _config;

        private void Awake()
        {
            CameraMovementParameter parameter = null;

            CameraRotation rotationSystem = new(parameter);
            CameraFollow followSystem = new(parameter);
            CameraCenterOffsetController controller = new(followSystem, rotationSystem);
            _cameraSystem.Init(controller);

#if UNITY_EDITOR
            _cameraSystem.gameObject
                .AddComponent<TestCameraParameterDebug>()
                .SetCameraParameter(parameter);
#endif
        }
    }
}
