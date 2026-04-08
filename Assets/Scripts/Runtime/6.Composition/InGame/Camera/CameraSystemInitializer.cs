using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Application.InGame.Camera;
using KillChord.Runtime.Composition.InGame.Camera;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Camera;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.Structure.InGame.Camera;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     カメラシステムに関するクラスの生成と依存関係の解決を行う初期化クラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class CameraSystemInitializer : MonoBehaviour
    {
        [SerializeField] private CameraSystemView _cameraSystem;

        [SerializeField] private CameraSystemConfig _config;

        [SerializeField] private EnemyTestSpawner _enemyTestSpawner;

        public void Initialize()
        {
            CameraSystemParameter parameter = _config.ToDomain();

            CameraBoneLockOnRotationApplication boneRotationSystem = new(parameter);
            CameraBoneFreeLookRotationApplication freeLookRotationSystem = new(parameter);
            CameraRotationApplication rotationSystem = new(parameter);
            CameraFollowApplication followSystem = new(parameter);
            CameraSystemApplication application = new(parameter, followSystem, boneRotationSystem,
                freeLookRotationSystem, rotationSystem, _config.CollisionMask);

            TargetManager targetManager = new();
            _enemyTestSpawner.SetTargetManager(targetManager);
            TargetSelector targetSelector = new(targetManager);
            CameraSystemApplication application = new(parameter, followSystem, boneRotationSystem, freeLookRotationSystem, rotationSystem, targetSelector, _config.CollisionMask);

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