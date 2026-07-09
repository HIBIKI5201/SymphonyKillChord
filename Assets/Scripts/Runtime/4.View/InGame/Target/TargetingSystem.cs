using System;
using KillChord.Runtime.Adaptor.InGame.Target;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Target
{
    /// <summary>
    ///     ターゲットの登録、選択、位置取得を一元管理するクラス。
    /// </summary>
    public sealed class TargetingSystem : ITargetSystemViewModel
    {
        /// <summary>
        ///     ターゲットを登録する。
        /// </summary>
        /// <param name="targetable"> 登録するターゲット。</param>
        public void RegisterTarget(ITargetableViewModel targetable)
        {
            if (targetable == null)
            {
                Debug.LogError("targetable が null です。");
                return;
            }

            _targets.Add(targetable);
        }

        /// <summary>
        ///     ターゲットの登録を解除する。
        /// </summary>
        /// <param name="targetable"> 解除するターゲット。</param>
        public void UnregisterTarget(ITargetableViewModel targetable)
        {
            if (targetable == null)
            {
                return;
            }

            _targets.Remove(targetable);
            if (ReferenceEquals(_currentTarget, targetable))
            {
                _currentTarget = null;
            }
        }

        /// <summary>
        ///     現在のターゲットIDの取得を試みる。
        /// </summary>
        /// <param name="targetId"> 取得したターゲットID。取得失敗時は <see cref="Guid.Empty"/>。</param>
        /// <returns> 取得に成功した場合は true。</returns>
        public bool TryGetCurrentTargetId(out Guid targetId)
        {
            targetId = Guid.Empty;

            if (!TryGetCurrentTarget(out ITargetableViewModel targetable))
            {
                return false;
            }

            targetId = targetable.TargetId;
            return true;
        }

        /// <summary>
        ///     現在のターゲットの取得を試みる。
        /// </summary>
        /// <param name="targetable"> 取得したターゲット。取得失敗時は null。</param>
        /// <returns> 取得に成功した場合は true。</returns>
        public bool TryGetCurrentTarget(out ITargetableViewModel targetable)
        {
            targetable = _currentTarget;

            if (targetable == null)
            {
                return false;
            }

            if (!IsSelectableTarget(targetable))
            {
                _currentTarget = null;
                targetable = null;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     現在のターゲット位置の取得を試みる。
        /// </summary>
        /// <param name="result"> 取得した位置。取得失敗時は <see cref="Vector3.zero"/>。</param>
        /// <returns> 取得に成功した場合は true。</returns>
        public bool TryGetCurrentTargetPosition(out Vector3 result)
        {
            result = Vector3.zero;

            if (!TryGetCurrentTarget(out ITargetableViewModel targetable))
            {
                return false;
            }

            result = targetable.Position;
            return true;
        }

        /// <summary>
        ///     プレイヤー位置と方向をもとに最適なターゲットへ切り替える。
        /// </summary>
        /// <param name="playerPosition"> プレイヤーの現在位置。</param>
        /// <param name="direction"> 選択基準に使用する方向。</param>
        public void ChangeTarget(in Vector3 playerPosition, in Vector3 direction)
        {
            _currentTarget = SelectTarget(playerPosition, direction);
        }

        /// <summary>
        ///     現在のターゲット選択を解除する。
        /// </summary>
        public void ClearTarget()
        {
            _currentTarget = null;
        }

        /// <summary> NormalizeDot で使用するゼロ除算回避の下限閾値。 </summary>
        private const float NORMALIZE_DOT_EPSILON = 1E-15f;

        /// <summary> 内積探索の初期値。 </summary>
        private const float DOT_INITIAL_VALUE = -1f;

        /// <summary> 正面判定に使用する閾値。 </summary>
        private const float DOT_FORWARD_THRESHOLD = 0f;

        private readonly HashSet<ITargetableViewModel> _targets = new();
        private ITargetableViewModel _currentTarget;

        /// <summary>
        ///     ターゲットが選択可能な状態かを判定する。
        /// </summary>
        /// <param name="targetable"> 判定対象のターゲット。</param>
        /// <returns> 選択可能な場合は true。</returns>
        private bool IsSelectableTarget(ITargetableViewModel targetable)
        {
            if (targetable == null)
            {
                return false;
            }

            return targetable.IsAlive && _targets.Contains(targetable);
        }

        /// <summary>
        ///     優先順位に従って最適なターゲットを選択する。
        /// </summary>
        /// <param name="center"> 基準位置。</param>
        /// <param name="direction"> 基準方向。</param>
        /// <returns> 選択されたターゲット。候補がない場合は null。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ITargetableViewModel SelectTarget(in Vector3 center, in Vector3 direction)
        {
            ITargetableViewModel shortestTarget = null;
            ITargetableViewModel bestAlignedTarget = null;
            float shortestDistance = float.PositiveInfinity;
            float bestDot = DOT_INITIAL_VALUE;

            foreach (ITargetableViewModel targetable in _targets)
            {
                if (!IsSelectableTarget(targetable))
                {
                    continue;
                }

                Vector3 position = targetable.Position;
                float dot = NormalizeDot(direction, position - center);
                if (dot >= bestDot)
                {
                    bestDot = dot;
                    bestAlignedTarget = targetable;
                }

                float sqrDistance = Vector3.SqrMagnitude(position - center);
                if (sqrDistance < shortestDistance)
                {
                    shortestDistance = sqrDistance;
                    shortestTarget = targetable;
                }
            }

            return bestDot < DOT_FORWARD_THRESHOLD ? shortestTarget : bestAlignedTarget;
        }

        /// <summary>
        ///     二つのベクトルの正規化内積を返す。
        /// </summary>
        /// <param name="from"> 基準ベクトル。</param>
        /// <param name="to"> 比較ベクトル。</param>
        /// <returns> 正規化された内積。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float NormalizeDot(in Vector3 from, in Vector3 to)
        {
            float denominator = Mathf.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denominator < NORMALIZE_DOT_EPSILON)
            {
                return 0f;
            }

            return Mathf.Clamp(Vector3.Dot(from, to) / denominator, -1f, 1f);
        }
    }
}
