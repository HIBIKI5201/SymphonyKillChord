using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Skill;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     ターゲットViewModel群からスキル対象を解決するアダプターです。
    /// </summary>
    public sealed class SkillTargetResolver : ISkillTargetResolver
    {
        private const float FORWARD_AREA_RANGE = 12f;
        private const float FORWARD_AREA_HALF_ANGLE = 30f;
        private const float FORWARD_LINE_HALF_WIDTH = 1.5f;
        private const float FORWARD_LINE_LENGTH_MARGIN = 0.5f;

        /// <summary>
        ///     解決器を初期化します。
        /// </summary>
        /// <param name="targetSystemViewModel"> ターゲットViewModelです。 </param>
        /// <param name="targetEntityRegistry"> ターゲットEntityレジストリです。 </param>
        /// <param name="playerTransform"> プレイヤーTransformです。 </param>
        /// <param name="areaAttackRangeAddition"> 前方範囲攻撃の追加射程です。 </param>
        public SkillTargetResolver(
            ITargetSystemViewModel targetSystemViewModel,
            TargetEntityRegistry targetEntityRegistry,
            Transform playerTransform,
            float areaAttackRangeAddition)
        {
            _targetSystemViewModel = targetSystemViewModel;
            _targetEntityRegistry = targetEntityRegistry;
            _playerTransform = playerTransform;
            _forwardAreaRange = Mathf.Max(0f, FORWARD_AREA_RANGE + areaAttackRangeAddition);
        }

        /// <summary>
        ///     スキル対象の解決を試みます。
        /// </summary>
        /// <param name="targetingType"> 対象解決ルールです。 </param>
        /// <param name="result"> 解決結果です。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        public bool TryResolveTargets(SkillTargetingType targetingType, out SkillTargetResolveResult result)
        {
            result = default;

            switch (targetingType)
            {
                case SkillTargetingType.None:
                case SkillTargetingType.Self:
                    result = new SkillTargetResolveResult(null, Array.Empty<CharacterEntity>());
                    return true;
                case SkillTargetingType.CurrentTarget:
                    return TryResolveCurrentTarget(out result);
                case SkillTargetingType.ForwardArea:
                    return TryResolveForwardArea(out result);
                case SkillTargetingType.CurrentTargetForwardLine:
                    return TryResolveCurrentTargetForwardLine(out result);
                default:
                    return false;
            }
        }

        /// <summary>
        ///     現在ターゲットのみを解決します。
        /// </summary>
        /// <param name="result"> 解決結果です。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        private bool TryResolveCurrentTarget(out SkillTargetResolveResult result)
        {
            result = default;
            if (!TryGetCurrentTargetEntity(out CharacterEntity targetEntity))
            {
                return false;
            }

            result = new SkillTargetResolveResult(targetEntity, new[] { targetEntity });
            return true;
        }

        /// <summary>
        ///     前方範囲のターゲット群を解決します。
        /// </summary>
        /// <param name="result"> 解決結果です。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        private bool TryResolveForwardArea(out SkillTargetResolveResult result)
        {
            result = default;

            if (_playerTransform == null)
            {
                return false;
            }

            List<CharacterEntity> targetEntities = new List<CharacterEntity>();
            ITargetableViewModel[] targets = _targetSystemViewModel.GetRegisteredTargetsSnapshot();
            Vector3 origin = _playerTransform.position;
            Vector3 forward = _playerTransform.forward;
            float cosThreshold = Mathf.Cos(FORWARD_AREA_HALF_ANGLE * Mathf.Deg2Rad);

            for (int i = 0; i < targets.Length; i++)
            {
                ITargetableViewModel target = targets[i];
                if (!TryResolveEntity(target, out CharacterEntity entity))
                {
                    continue;
                }

                Vector3 toTarget = target.Position - origin;
                float sqrDistance = toTarget.sqrMagnitude;
                if (sqrDistance > _forwardAreaRange * _forwardAreaRange)
                {
                    continue;
                }

                float distance = Mathf.Sqrt(sqrDistance);
                if (distance <= Mathf.Epsilon)
                {
                    continue;
                }

                float dot = Vector3.Dot(forward, toTarget / distance);
                if (dot < cosThreshold)
                {
                    continue;
                }

                targetEntities.Add(entity);
            }

            if (targetEntities.Count == 0)
            {
                return false;
            }

            result = new SkillTargetResolveResult(targetEntities[0], targetEntities.ToArray());
            return true;
        }

        /// <summary>
        ///     現在ターゲットを軸にした前方直線対象を解決します。
        /// </summary>
        /// <param name="result"> 解決結果です。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        private bool TryResolveCurrentTargetForwardLine(out SkillTargetResolveResult result)
        {
            result = default;

            if (_playerTransform == null)
            {
                return false;
            }

            if (!_targetSystemViewModel.TryGetCurrentTarget(out ITargetableViewModel currentTarget) ||
                !TryResolveEntity(currentTarget, out CharacterEntity currentTargetEntity))
            {
                return false;
            }

            Vector3 origin = _playerTransform.position;
            Vector3 targetDirection = currentTarget.Position - origin;
            float targetDistance = targetDirection.magnitude;
            if (targetDistance <= Mathf.Epsilon)
            {
                result = new SkillTargetResolveResult(currentTargetEntity, new[] { currentTargetEntity });
                return true;
            }

            Vector3 lineDirection = targetDirection / targetDistance;
            List<CharacterEntity> targetEntities = new List<CharacterEntity> { currentTargetEntity };
            ITargetableViewModel[] targets = _targetSystemViewModel.GetRegisteredTargetsSnapshot();

            for (int i = 0; i < targets.Length; i++)
            {
                ITargetableViewModel target = targets[i];
                if (target == null || ReferenceEquals(target, currentTarget))
                {
                    continue;
                }

                if (!TryResolveEntity(target, out CharacterEntity entity))
                {
                    continue;
                }

                Vector3 toCandidate = target.Position - origin;
                float projectedLength = Vector3.Dot(lineDirection, toCandidate);
                if (projectedLength < 0f || projectedLength > targetDistance + FORWARD_LINE_LENGTH_MARGIN)
                {
                    continue;
                }

                Vector3 closestPoint = origin + lineDirection * projectedLength;
                float distanceToLine = Vector3.Distance(target.Position, closestPoint);
                if (distanceToLine > FORWARD_LINE_HALF_WIDTH)
                {
                    continue;
                }

                targetEntities.Add(entity);
            }

            result = new SkillTargetResolveResult(currentTargetEntity, targetEntities.ToArray());
            return true;
        }

        /// <summary>
        ///     現在ターゲットのEntity取得を試みます。
        /// </summary>
        /// <param name="entity"> 取得したEntityです。 </param>
        /// <returns> 取得に成功した場合はtrue。 </returns>
        private bool TryGetCurrentTargetEntity(out CharacterEntity entity)
        {
            entity = null;

            if (!_targetSystemViewModel.TryGetCurrentTargetId(out Guid targetId))
            {
                return false;
            }

            return _targetEntityRegistry.TryGetEntity(targetId, out entity);
        }

        /// <summary>
        ///     ターゲットViewModelからEntity解決を試みます。
        /// </summary>
        /// <param name="target"> ターゲットViewModelです。 </param>
        /// <param name="entity"> 解決したEntityです。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        private bool TryResolveEntity(ITargetableViewModel target, out CharacterEntity entity)
        {
            entity = null;
            if (target == null || !target.IsAlive)
            {
                return false;
            }

            return _targetEntityRegistry.TryGetEntity(target.TargetId, out entity);
        }

        private readonly ITargetSystemViewModel _targetSystemViewModel;
        private readonly TargetEntityRegistry _targetEntityRegistry;
        private readonly Transform _playerTransform;
        private readonly float _forwardAreaRange;
    }
}
