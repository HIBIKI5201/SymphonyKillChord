using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Application.InGame.Camera;
using KillChord.Runtime.Domain.InGame.Camera;
using KillChord.Runtime.Structure.InGame.Camera;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.InGame.Camera;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

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
        /// <summary>
        ///     カメラシステムを構成する各クラスを生成し、依存関係を解決して初期化する。
        /// </summary>
        /// <param name="targetManager"> ロックオン対象の一覧を管理するマネージャー。</param>
        /// <param name="targetEntityRegistry"> ロックオン対象とキャラクターエンティティの対応を管理するレジストリ。</param>
        public void Initialize(TargetManager targetManager, TargetEntityRegistry targetEntityRegistry)
        {
            if (_config == null)
            {
                Debug.LogError($"{nameof(_config)} がアサインされていません。");
                return;
            }

            if (_cameraSystem == null)
            {
                Debug.LogError($"{nameof(_cameraSystem)} がアサインされていません。");
                return;
            }

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
            if (stageSceneObj == null)
            {
                Debug.LogError($"{nameof(IStageSceneInstance)} が見つかりません。");
                return;
            }

            _cameraSystem.Initialize(controller, presenter, stageSceneObj.PlayerTransform,
                ServiceLocator.GetInstance<PlayerInputView>());

#if UNITY_EDITOR
            _cameraSystem.gameObject
                .AddComponent<CameraSystemParameterDebug>()
                .SetCameraParameter(parameter);
#endif
        }

        [SerializeField, Tooltip("カメラシステムの挙動を管理する View コンポーネント。")]
        private CameraSystemView _cameraSystem;

        [SerializeField, Tooltip("カメラシステムのパラメータを定義するコンフィグ。")]
        private CameraSystemConfig _config;
    }
}