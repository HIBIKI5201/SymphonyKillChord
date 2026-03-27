using UnityEngine;

namespace KillChord.Runtime.View
{
    // MonoBehaviour側が差し替え可能な、interface代替のアダプタクラス。
    public class CameraCollisionResolver
    {
        public virtual bool TryResolve(
            Vector3 origin,
            Vector3 direction,
            float distance,
            float radius,
            Transform ignoreTarget,
            out Vector3 resolvedPosition)
        {
            resolvedPosition = default;

            if (!Physics.SphereCast(origin, radius, direction, out RaycastHit hitInfo, distance)
                || hitInfo.rigidbody?.transform == ignoreTarget)
            {
                return false;
            }

            resolvedPosition = hitInfo.point + hitInfo.normal * radius;
            return true;
        }
    }
}
