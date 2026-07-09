using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Camera;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.View.InGame.Camera;
using KillChord.Runtime.View.InGame.Target;
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
        public void Initialize(TargetingSystem targetingSystem)
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

            CameraViewSettings viewSettings = new(
                _config.Offset,
                _config.CharacterCenterOffset,
                _config.Distance,
                _config.FollowOffsetPower,
                _config.FollowLerpSpeed,
                _config.BoneRotateSpeed,
                _config.LockOnAngleMargin,
                _config.FollowRotationSpeed,
                _config.LockOnLookAtRatio,
                _config.LockOnRotationSpeed,
                _config.CollisionRadius,
                _config.CollisionMask,
                _config.PitchRange,
                _config.IsInvertVertical,
                _config.IsInvertHorizontal);

            CameraLockOnRotationCalculator lockOnRotationCalculator = new(viewSettings);
            CameraFreeLookRotationCalculator freeLookRotationCalculator = new(viewSettings);
            CameraLookAtRotationCalculator lookAtRotationCalculator = new(viewSettings);
            CameraFollowCalculator followCalculator = new(viewSettings);

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
                freeLookRotationCalculator, lookAtRotationCalculator, viewSettings, stageSceneObj.PlayerTransform,
                ServiceLocator.GetInstance<PlayerInputView>());
        }

        [SerializeField, Tooltip("カメラシステムの挙動を管理する View コンポーネント。")]
        private CameraSystemView _cameraSystem;

        [SerializeField, Tooltip("カメラシステムのパラメータを定義するコンフィグ。")]
        private CameraSystemConfig _config;
    }
}
