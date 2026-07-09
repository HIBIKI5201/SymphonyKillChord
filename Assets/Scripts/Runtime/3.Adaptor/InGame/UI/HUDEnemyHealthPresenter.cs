using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Utility.InGame;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     ロックオン対象の敵情報とロックオン有無をDTOへ変換し、ViewModelへ渡すPresenter。
    /// </summary>
    public sealed class HUDEnemyHealthPresenter
    {
        /// <summary>
        ///     ロックオン状態・対象解決のコントローラーとViewModelを受け取り、Presenterを初期化するコンストラクタ。
        /// </summary>
        /// <param name="cameraSystemController"> カメラのロックオン状態を提供するコントローラー。</param>
        /// <param name="targetSelectorController"> 現在のロックオン対象エンティティを解決するコントローラー。</param>
        /// <param name="viewModel"> 敵HP HUDのViewModel。</param>
        public HUDEnemyHealthPresenter(
            CameraSystemController cameraSystemController,
            TargetSelectorController targetSelectorController,
            IHUDEnemyHealthViewModel viewModel,
            TargetSelector targetSelector)
        {
            if (cameraSystemController == null)
                throw new ArgumentNullException(nameof(cameraSystemController), "CameraSystemControllerがNULL。");
            _cameraSystemController = cameraSystemController;

            if (targetSelectorController == null)
                throw new ArgumentNullException(nameof(targetSelectorController), "TargetSelectorControllerがNULL。");
            _targetSelectorController = targetSelectorController;

            if (targetSelector == null)
                throw new ArgumentNullException(nameof(targetSelector), "TargetSelectorがNULL。");
            _targetSelector = targetSelector;

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel), "敵HPのViewModelがNULL。");
            _viewModel = viewModel;
        }

        /// <summary>
        ///     現在のロックオン状態と対象のHPをViewModelへ反映する。
        ///     状態変化を通知するイベントが存在しないため、毎フレーム呼び出すこと。
        /// </summary>
        public void Update()
        {
            if (_cameraSystemController.LockOnState != CameraLockOnState.Free
                && _targetSelectorController.TryGetCurrentTargetEntity(out CharacterEntity entity))
            {
                _targetSelector.TryGetTargetPosition(default, default, out Vector3 result);
                _viewModel.Update(new HUDEnemyHealthDTO(
                    entity.CurrentHealth.Value,
                    entity.MaxHealth.Value,
                    true,
                    result));
            }
            else
            {
                // 対象なし。ゼロ除算を避けるため MaxHealth には 1 を渡す
                _viewModel.Update(new HUDEnemyHealthDTO(0f, 1f, false, Vector3.zero));
            }
        }

        private readonly CameraSystemController _cameraSystemController;
        private readonly TargetSelectorController _targetSelectorController;
        private readonly TargetSelector _targetSelector;
        private readonly IHUDEnemyHealthViewModel _viewModel;
    }
}
