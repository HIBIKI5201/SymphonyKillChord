using KillChord.Runtime.Application.InGame.Target;
using KillChord.Runtime.Domain.InGame.Character;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Target
{
    public class TargetRadiusQuery : ITargetRadiusQuery
    {
        public TargetRadiusQuery(
            ITargetSystemViewModel viewModel,
            TargetEntityRegistry targetEntityRegistry)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _targetEntityRegistry = targetEntityRegistry ?? throw new ArgumentNullException(nameof(targetEntityRegistry));
        }

        ///</inheritdoc/>
        public void Query(CharacterEntity center, float range, List<CharacterEntity> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();

            // 引数の検証
            if (center == null || !float.IsFinite(range) || range < 0f)
            {
                return;
            }

            _viewModel.CopyRegisteredTargetsTo(_targets);

            // 中心点の位置を取得する
            if (!TryGetPos(center, out Vector3 centerPos))
            {
                return;
            }

            float sqrRange = range * range;

            // 対象の位置を取得して、範囲内にいるかどうかを判定する
            for (int i = 0; i < _targets.Count; i++)
            {
                ITargetableViewModel target = _targets[i];

                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                if (!_targetEntityRegistry.TryGetEntity(target.TargetId, out CharacterEntity targetEntity))
                {
                    continue;
                }

                if (targetEntity == null || targetEntity.IsDead)
                {
                    continue;
                }

                Vector3 offset = target.Position - centerPos;
                // 高さ方向の差を無視する
                offset.y = 0f;

                if (offset.sqrMagnitude <= sqrRange)
                {
                    results.Add(targetEntity);
                }
            }
        }

        /// <summary>
        ///     対象の位置を取得する。
        /// </summary>
        /// <param name="entity"> 位置を取得する対象のキャラクターエンティティです。 </param>
        /// <param name="position"> 取得した位置を格納する変数です。 </param>
        /// <returns> 位置の取得に成功した場合は true、失敗した場合は false を返します。 </returns>
        private bool TryGetPos(CharacterEntity entity, out Vector3 position)
        {
            for (int i = 0; i < _targets.Count; i++)
            {
                ITargetableViewModel target = _targets[i];

                if (target == null || target.TargetId != entity.Id)
                {
                    continue;
                }

                position = target.Position;
                return true;
            }

            position = default;
            return false;
        }

        private readonly ITargetSystemViewModel _viewModel;
        private readonly TargetEntityRegistry _targetEntityRegistry;
        private readonly List<ITargetableViewModel> _targets = new();
    }
}
