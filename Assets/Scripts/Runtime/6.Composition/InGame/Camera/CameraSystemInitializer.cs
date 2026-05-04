using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame;
using KillChord.Runtime.Application.InGame.Camera;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Camera;
using KillChord.Runtime.Structure.InGame.Camera;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.InGame.Camera;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using KillChord.Runtime.Composition.InGame.Debugger;
#endif

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     カメラシステムに関するクラスの生成と依存関係の解決を行う初期化クラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class CameraSystemInitializer : MonoBehaviour
    {
        public void Initialize(TargetManager targetManager, TargetEntityRegistry targetEntityRegistry)
        {
            CameraSystemParameter parameter = _config.ToDomain();

            CameraBoneLockOnRotationApplication boneRotationSystem = new(parameter);
            CameraBoneFreeLookRotationApplication freeLookRotationSystem = new(parameter);
            CameraRotationApplication rotationSystem = new(parameter);
            CameraFollowApplication followSystem = new(parameter);

            TargetSelector targetSelector = new(targetManager);
            TargetEntityRegistryController targetEntityRegistryController = new(targetEntityRegistry);
            TargetSelectorController targetSelectorController = new(targetSelector, targetEntityRegistryController);
            ServiceLocator.RegisterInstance(targetSelectorController);

            CameraSystemApplication application = new(parameter, followSystem, boneRotationSystem,
                freeLookRotationSystem, rotationSystem, targetSelector);

            CameraSystemController controller = new(application);
            CameraSystemPresenter presenter = new(application);

            var stageSceneObj = ServiceLocator.GetInstance<IStageSceneInstance>();
            _cameraSystem.Initialize(controller, presenter, stageSceneObj.PlayerTransform,
                ServiceLocator.GetInstance<PlayerInputView>());


#if UNITY_EDITOR
            _cameraSystem.gameObject
                .AddComponent<CameraSystemParameterDebug>()
                .SetCameraParameter(parameter);
#endif
        }


        [SerializeField] private CameraSystemView _cameraSystem;

        [SerializeField] private CameraSystemConfig _config;

        [SerializeField] private EnemyInfantryTestSpawner _enemyTestSpawner;
    }
}