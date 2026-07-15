using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Domain.InGame.Character;
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
        /// <param name="targetingSystem"> 現在のターゲット情報を解決するシステム。</param>
        /// <param name="viewModel"> 敵HP HUDのViewModel。</param>
        public HUDEnemyHealthPresenter(
            TargetSystemController targetingSystem,
            IHUDEnemyHealthViewModel viewModel)
        {
            if (targetingSystem == null)
                throw new ArgumentNullException(nameof(targetingSystem), "TargetingSystemがNULL。");
            _targetingSystem = targetingSystem;

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
            if (_targetingSystem.TryGetCurrentTargetEntity(out CharacterEntity entity)
                && _targetingSystem.TryGetCurrentTargetPosition(out Vector3 result))
            {
                _viewModel.Update(new HUDEnemyHealthDTO(
                    entity.CurrentHealth.Value,
                    entity.MaxHealth.Value,
                    LockOnDisplayState.LockedOn,
                    result));
            }
            else if (_targetingSystem.TryGetCurrentCandidateEntity(out _)
                && _targetingSystem.TryGetCurrentCandidatePosition(out Vector3 candidatePosition))
            {
                _viewModel.Update(new HUDEnemyHealthDTO(
                    0f,
                    1f,
                    LockOnDisplayState.Candidate,
                    candidatePosition));
            }
            else
            {
                // 対象なし。ゼロ除算を避けるため MaxHealth には 1 を渡す
                _viewModel.Update(new HUDEnemyHealthDTO(
                    0f,
                    1f,
                    LockOnDisplayState.Hidden,
                    Vector3.zero));
            }
        }

        private readonly TargetSystemController _targetingSystem;
        private readonly IHUDEnemyHealthViewModel _viewModel;
    }
}
