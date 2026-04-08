using KillChord.Runtime.Domain.InGame;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    public sealed class TargetSelector
    {
        public TargetSelector(ITargetPositionsProvider provider)
        {
            _targetPositionsProvider = provider;
        }

        public bool TryGetTargetPosition(in Vector3 playerPosition, in Vector3 direction, out Vector3 result)
        {
            result = Vector3.zero;
            _targetPositionsProvider.UpdatePositions();
            if (_targetPositionsProvider.TargetPositions.Count <= 0)
                return false;

            GetTargetPosition(playerPosition, direction, out result);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetTargetPosition(in Vector3 center, in Vector3 dir, out Vector3 result)
        {
            //優先順位は
            //1.視界内のカメラベクトルに近い敵
            //2.視界内の敵
            //3.視界外の近くの敵
            //4.視界外の敵
            IReadOnlyList<Vector3> targets = _targetPositionsProvider.TargetPositions;

            Vector3 shortestPos = Vector3.positiveInfinity;
            float shortestDist = float.PositiveInfinity;
            Vector3 bestAlignedPos = Vector3.positiveInfinity;
            float bestDot = -1f;

            float dot;
            float sqrDist;
            foreach (var item in targets)
            {
                dot = NormalizeDot(dir, item - center);
                if (dot >= bestDot)
                {
                    bestDot = dot;
                    bestAlignedPos = item;
                }

                sqrDist = Vector3.SqrMagnitude(item - center);
                if (sqrDist < shortestDist)
                {
                    shortestDist = sqrDist;
                    shortestPos = item;
                }
            }
            result = (bestDot < 0f) ? shortestPos : bestAlignedPos;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float NormalizeDot(in Vector3 from, in Vector3 to)
        {
            float num = Mathf.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (num < 1E-15f)
            {
                return 0f;
            }

            return Mathf.Clamp(Vector3.Dot(from, to) / num, -1f, 1f);
        }
        private readonly ITargetPositionsProvider _targetPositionsProvider;
    }
}
