using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Target;
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
    public sealed class CameraSystemInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(CameraSystemInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 600;

        /// <summary>
        ///     他モジュールへ結合してカメラシステムを初期化する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (_config == null || _cameraSystem == null)
            {
                Debug.LogError($"[{nameof(CameraSystemInitializer)}] カメラ初期化参照が不足しています。", this);
                return false;
            }

            TargetSystemModuleContainer targetContainer = ServiceLocator.GetInstance<TargetSystemModuleContainer>();
            if (targetContainer == null)
            {
                Debug.LogError($"[{nameof(CameraSystemInitializer)}] {nameof(TargetSystemModuleContainer)} が見つかりません。", this);
                return false;
            }

            Initialize(targetContainer.TargetSystemViewModel);

#if UNITY_ANDROID
            MobileInput mobileInput = FindFirstObjectByType<MobileInput>();
            PlayerInputView playerInputView = ServiceLocator.GetInstance<PlayerInputView>();
            if (mobileInput != null && playerInputView != null)
            {
                mobileInput.Initialize(playerInputView);
            }
#else
            Cursor.lockState = CursorLockMode.Locked;
#endif
            return true;
        }

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
            CameraLockOnRangeChecker lockOnRangeChecker = new(_config);
            CameraLockOnBreakTracker lockOnBreakTracker = new(_config);
            CameraShakeCalculator shakeCalculator = new();

            PlayerModuleContainer playerModuleContainer = ServiceLocator.GetInstance<PlayerModuleContainer>();
            if (playerModuleContainer == null || playerModuleContainer.PlayerView == null)
            {
                Debug.LogError($"{nameof(PlayerModuleContainer)} が見つかりません。");
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
                (playerPosition, direction) => targetingSystem.UpdateCandidate(playerPosition, direction),
                (playerPosition, direction) => targetingSystem.TrySwitchTarget(playerPosition, direction),
                targetId => targetingSystem.TrySetCurrentTarget(targetId),
                followCalculator, lockOnRotationCalculator,
                freeLookRotationCalculator, lookAtRotationCalculator, lockOnRangeChecker, lockOnBreakTracker,
                shakeCalculator, _config, playerModuleContainer.PlayerView.transform,
                ServiceLocator.GetInstance<PlayerInputView>());
        }

        [SerializeField, Tooltip("カメラシステムの挙動を管理する View コンポーネント。")]
        private CameraSystemView _cameraSystem;

        [SerializeField, Tooltip("カメラシステムのパラメータを定義するコンフィグ。")]
        private CameraConfig _config;
    }
}
