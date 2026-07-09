using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.View.InGame.Camera;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace KillChord.Runtime.Composition.InGame.Camera
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
        /// <param name="targetingSystem"> カメラが参照するターゲット選択機能。</param>
        public void Initialize(ITargetSystemViewModel targetingSystem)
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

            CameraLockOnRotationCalculator lockOnRotationCalculator = new(_config);
            CameraFreeLookRotationCalculator freeLookRotationCalculator = new(_config);
            CameraLookAtRotationCalculator lookAtRotationCalculator = new(_config);
            CameraFollowCalculator followCalculator = new(_config);

            var stageSceneObj = ServiceLocator.GetInstance<IStageSceneInstance>();
            if (stageSceneObj == null)
            {
                Debug.LogError($"{nameof(IStageSceneInstance)} が見つかりません。");
                return;
            }

            _cameraSystem.Initialize(
                (playerPosition, direction) => targetingSystem.ChangeTarget(playerPosition, direction),
                () => targetingSystem.ClearTarget(),
                () =>
                {
                    bool hasTarget = targetingSystem.TryGetCurrentTargetPosition(out Vector3 targetPosition);
                    return (hasTarget, targetPosition);
                },
                followCalculator, lockOnRotationCalculator,
                freeLookRotationCalculator, lookAtRotationCalculator, _config, stageSceneObj.PlayerTransform,
                ServiceLocator.GetInstance<PlayerInputView>());
        }

        [SerializeField, Tooltip("カメラシステムの挙動を管理する View コンポーネント。")]
        private CameraSystemView _cameraSystem;

        [SerializeField, Tooltip("カメラシステムのパラメータを定義するコンフィグ。")]
        private CameraConfig _config;
    }
}
