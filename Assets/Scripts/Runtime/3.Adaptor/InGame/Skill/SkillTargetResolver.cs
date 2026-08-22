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

        /// <summary>
        ///     解決器を初期化します。
        /// </summary>
        /// <param name="targetSystemViewModel"> ターゲットViewModelです。 </param>
        /// <param name="targetEntityRegistry"> ターゲットEntityレジストリです。 </param>
        /// <param name="targetAreaQuery"> 扇形範囲クエリです。 </param>
        /// <param name="playerTransform"> プレイヤーTransformです。 </param>
        /// <param name="areaAttackRangeAddition"> 前方範囲攻撃の追加射程です。 </param>
        public SkillTargetResolver(
            ITargetSystemViewModel targetSystemViewModel,
            TargetEntityRegistry targetEntityRegistry,
            TargetAreaQuery targetAreaQuery,
            Transform playerTransform,
            float areaAttackRangeAddition)
        {
            _targetSystemViewModel = targetSystemViewModel;
            _targetEntityRegistry = targetEntityRegistry;
            _targetAreaQuery = targetAreaQuery;
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

            if (_playerTransform == null || _targetAreaQuery == null)
            {
                return false;
            }

            _targetAreaQuery.QueryFanArea(
                _playerTransform.position,
                _playerTransform.forward,
                _forwardAreaRange,
                FORWARD_AREA_HALF_ANGLE,
                _areaHitBuffer);

            if (_areaHitBuffer.Count == 0)
            {
                return false;
            }

            // クエリは水平距離の昇順で返すため、先頭が最も近い対象になる。
            CharacterEntity[] targetEntities = new CharacterEntity[_areaHitBuffer.Count];
            for (int i = 0; i < _areaHitBuffer.Count; i++)
            {
                targetEntities[i] = _areaHitBuffer[i].Entity;
            }

            result = new SkillTargetResolveResult(targetEntities[0], targetEntities);
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

            // 水平方向のみで判定するため、Y軸方向の差分を無視する。
            targetDirection.y = 0f;

            if (targetDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                result = new SkillTargetResolveResult(currentTargetEntity, new[] { currentTargetEntity });
                return true;
            }

            Vector3 lineDirection = targetDirection.normalized;
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
                toCandidate.y = 0f;

                // 直線上の射影距離を計算し、負の値の場合はプレイヤーの後方にあるためスキップする。
                float projectedLength = Vector3.Dot(lineDirection, toCandidate);

                if (projectedLength < 0f)
                {
                    continue;
                }

                // 射影距離に基づいて直線上の最近接点を計算し、候補との距離を測定する。
                Vector3 closestPoint = lineDirection * projectedLength;
                float distanceToLine = Vector3.Distance(toCandidate, closestPoint);

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
        private readonly TargetAreaQuery _targetAreaQuery;
        private readonly Transform _playerTransform;
        private readonly float _forwardAreaRange;
        private readonly List<TargetAreaHit> _areaHitBuffer = new List<TargetAreaHit>();
    }
}
