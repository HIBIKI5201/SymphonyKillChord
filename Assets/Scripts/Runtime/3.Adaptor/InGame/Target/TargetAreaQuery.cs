using KillChord.Runtime.Domain.InGame.Character;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Target
{
    /// <summary>
    ///     登録済みターゲットの中から、扇形範囲に含まれる対象を検索するクエリ。
    ///     判定はXZ平面（水平角・水平距離）で行い、上下方向の差は無視する。
    /// </summary>
    public sealed class TargetAreaQuery
    {
        /// <summary>
        ///     クエリを生成します。
        /// </summary>
        /// <param name="targetSystemViewModel"> ターゲットViewModelです。 </param>
        /// <param name="targetEntityRegistry"> ターゲットEntityレジストリです。 </param>
        public TargetAreaQuery(
            ITargetSystemViewModel targetSystemViewModel,
            TargetEntityRegistry targetEntityRegistry)
        {
            _targetSystemViewModel = targetSystemViewModel;
            _targetEntityRegistry = targetEntityRegistry;
        }

        /// <summary> 指定できる半角の最小値（度）。 </summary>
        public const float MIN_HALF_ANGLE_DEGREES = 0f;

        /// <summary> 指定できる半角の最大値（度）。全方位を表す。 </summary>
        public const float MAX_HALF_ANGLE_DEGREES = 180f;

        /// <summary>
        ///     扇形範囲に含まれる対象を、原点からの水平距離の昇順で取得します。
        /// </summary>
        /// <param name="origin"> 扇形の原点です。 </param>
        /// <param name="direction"> 扇形の中心軸となる方向です。水平成分のみを使用します。 </param>
        /// <param name="range"> 射程です。 </param>
        /// <param name="halfAngleDegrees"> 中心軸からの半角（度）です。 </param>
        /// <param name="results"> 検出結果の格納先です。呼び出し時に内容がクリアされます。 </param>
        public void QueryFanArea(
            in Vector3 origin,
            in Vector3 direction,
            float range,
            float halfAngleDegrees,
            List<TargetAreaHit> results)
        {
            if (results == null)
            {
                Debug.LogError($"[{nameof(TargetAreaQuery)}] {nameof(results)} が null です。");
                return;
            }

            results.Clear();

            if (_targetSystemViewModel == null || _targetEntityRegistry == null)
            {
                Debug.LogError($"[{nameof(TargetAreaQuery)}] 依存が解決されていません。");
                return;
            }

            if (range <= 0f)
            {
                return;
            }

            // 扇形の中心軸を水平化する。真上・真下だけを向いている場合は軸を決められないため検索しない。
            Vector3 axis = new Vector3(direction.x, 0f, direction.z);
            float axisLength = axis.magnitude;
            if (axisLength <= Mathf.Epsilon)
            {
                return;
            }

            axis /= axisLength;

            float clampedHalfAngle = Mathf.Clamp(halfAngleDegrees, MIN_HALF_ANGLE_DEGREES, MAX_HALF_ANGLE_DEGREES);
            float cosThreshold = Mathf.Cos(clampedHalfAngle * Mathf.Deg2Rad);
            float sqrRange = range * range;

            ITargetableViewModel[] targets = _targetSystemViewModel.GetRegisteredTargetsSnapshot();
            for (int i = 0; i < targets.Length; i++)
            {
                ITargetableViewModel target = targets[i];
                if (!TryResolveEntity(target, out CharacterEntity entity))
                {
                    continue;
                }

                // 高低差を無視して水平面上のベクトルへ落とす。
                Vector3 toTarget = target.Position - origin;
                toTarget.y = 0f;

                float sqrDistance = toTarget.sqrMagnitude;
                if (sqrDistance > sqrRange)
                {
                    continue;
                }

                float distance = Mathf.Sqrt(sqrDistance);
                if (distance <= Mathf.Epsilon)
                {
                    // 原点と水平位置が一致する対象は角度を定義できないため、常に範囲内として扱う。
                    results.Add(new TargetAreaHit(target, entity, 0f));
                    continue;
                }

                if (Vector3.Dot(axis, toTarget / distance) < cosThreshold)
                {
                    continue;
                }

                results.Add(new TargetAreaHit(target, entity, distance));
            }

            results.Sort(DISTANCE_ASCENDING_COMPARISON);
        }

        /// <summary> 水平距離の昇順で比較する比較子。 </summary>
        private static readonly Comparison<TargetAreaHit> DISTANCE_ASCENDING_COMPARISON =
            static (left, right) => left.Distance.CompareTo(right.Distance);

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
    }
}
