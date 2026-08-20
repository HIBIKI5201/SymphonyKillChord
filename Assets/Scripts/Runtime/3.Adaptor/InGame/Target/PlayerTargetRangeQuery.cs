using KillChord.Runtime.Application.InGame.Target;
using KillChord.Runtime.Domain.InGame.Character;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Target
{
    /// <summary>
    ///     プレイヤーを中心としたターゲットの範囲判定を行うクエリです。
    /// </summary>
    public class PlayerTargetRangeQuery : IPlayerTargetRangeQuery
    {
        public PlayerTargetRangeQuery(
            ITargetSystemViewModel targetSystemViewModel,
            Transform playerTransform)
        {
            _targetSystemViewModel = targetSystemViewModel ?? throw new ArgumentNullException(nameof(targetSystemViewModel));
            _playerTransform = playerTransform ?? throw new ArgumentNullException(nameof(playerTransform));
        }

        /// < /inheritdoc>
        public bool IsWithinRange(CharacterEntity target, float range)
        {
            if (target == null || target.IsDead || !float.IsFinite(range) || range < 0f)
            {
                return false;
            }

            _targetSystemViewModel.CopyRegisteredTargetsTo(_targetableViewModels);

            for (int i = 0; i < _targetableViewModels.Count; i++)
            {
                ITargetableViewModel targetableViewModel = _targetableViewModels[i];

                if (targetableViewModel == null ||
                    !targetableViewModel.IsAlive ||
                    targetableViewModel.TargetId != target.Id)
                {
                    continue;
                }

                Vector3 offset = targetableViewModel.Position - _playerTransform.position;

                // 高低差を無視するためにY軸のオフセットを0に設定
                offset.y = 0f;

                return offset.sqrMagnitude <= range * range;
            }

            return false;
        }

        private readonly ITargetSystemViewModel _targetSystemViewModel;
        private readonly Transform _playerTransform;
        private readonly List<ITargetableViewModel> _targetableViewModels = new();
    }
}
