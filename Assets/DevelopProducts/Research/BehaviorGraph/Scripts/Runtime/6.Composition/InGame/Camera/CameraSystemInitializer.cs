using DevelopProducts.BehaviorGraph.Runtime.Adaptor;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Camera;
using DevelopProducts.BehaviorGraph.Runtime.Application;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Camera;
using DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Camera;
using KillChord.Runtime.Structure.InGame.Camera;
using DevelopProducts.BehaviorGraph.Runtime.Utility;
using DevelopProducts.BehaviorGraph.Runtime.View.InGame.Camera;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

#if UNITY_EDITOR
using DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Debugger;
#endif

namespace DevelopProducts.BehaviorGraph.Runtime.Composition
{
    /// <summary>
    ///     カメラシステムに関するクラスの生成と依存関係の解決を行う初期化クラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class CameraSystemInitializer : MonoBehaviour
    {
        [SerializeField] private CameraSystemView _cameraSystem;

        [SerializeField] private CameraSystemConfig _config;

        [SerializeField] private EnemyTestSpawnerBG _enemyTestSpawner;

        [SerializeField] private Transform _playerTransform;

        public void Initialize(TargetManager targetManager, TargetEntityRegistry targetEntityRegistry)
        {
            //CameraSystemParameter parameter = _config.ToDomain();
            CameraSystemParameter parameter = new(new Vector3(0, 1, 0), 5, 1, 2, 10, 30, 1.5f, 1, 10, 0.2f, LayerMask.GetMask("Default", "TransparentFX", "Water", "UI"), new Vector2(-45, 75));

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

            //var stageSceneObj = ServiceLocator.GetInstance<IStageSceneInstance>();
            _cameraSystem.Init(controller, _playerTransform);

#if UNITY_EDITOR
            _cameraSystem.gameObject
                .AddComponent<CameraSystemParameterDebug>()
                .SetCameraParameter(parameter);
#endif
        }
    }
}