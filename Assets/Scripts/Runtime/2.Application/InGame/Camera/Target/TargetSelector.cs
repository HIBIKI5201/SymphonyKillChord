using KillChord.Runtime.Domain.InGame.Camera.Target;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera.Target
{
    public sealed class TargetSelector
    {
        public TargetSelector(TargetManager manager)
        {
            _manager = manager;
        }

        public bool TryGetTargetPosition(in Vector3 playerPosition, in Vector3 direction, out Vector3 result)
        {
            result = Vector3.zero;
            if (_manager.Count == 0)
            {
                return false;
            }
            if (_currentTarget is null)
            {
                return false;
            }
            if (!_currentTarget.IsAlive)
            {
                _currentTarget = null;
                return false;
            }
            result = _currentTarget.Position;

            return true;
        }

        public bool TryGetCurrentTarget(out ILockOnTarget result)
        {
            result = _currentTarget;
            
            if(result is null)
            {
                return false;
            }

            if (!result.IsAlive)
            {
                _currentTarget = null;
                result = null;
                return false;
            }

            return true;
        }

        public void ChangeTarget(in Vector3 playerPosition, in Vector3 direction)
        {
            GetTargetPosition(playerPosition, direction, out _currentTarget);
        }

        private readonly TargetManager _manager;
        private ILockOnTarget _currentTarget;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetTargetPosition(in Vector3 center, in Vector3 dir, out ILockOnTarget result)
        {
            //優先順位は
            //1.視界内のカメラベクトルに近い敵
            //2.視界内の敵
            //3.視界外の近くの敵
            //4.視界外の敵

            ILockOnTarget shortestTarget = null;
            ILockOnTarget bestAlignedTarget = null;
            float shortestDist = float.PositiveInfinity;
            float bestDot = -1f;

            float dot;
            float sqrDist;
            Vector3 pos;
            foreach (var item in _manager.GetTargets)
            {
                pos = item.Position;
                dot = NormalizeDot(dir, pos - center);
                if (dot >= bestDot)
                {
                    bestDot = dot;
                    bestAlignedTarget = item;
                }

                sqrDist = Vector3.SqrMagnitude(pos - center);
                if (sqrDist < shortestDist)
                {
                    shortestDist = sqrDist;
                    shortestTarget = item;
                }
            }
            result = (bestDot < 0f) ? shortestTarget : bestAlignedTarget;
        }
    }
}
