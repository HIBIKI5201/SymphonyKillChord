using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class CameraRotation
    {
        public void Update(
            bool isLockon,
            ref Quaternion rotation,
            in Quaternion boneRotation,
            in Vector3 targetPosition,
            in Vector3 followPosition,
            in Vector3 cameraPosition,
            float deltaTime
        )
        {
            Quaternion target = Quaternion.identity;
            if (isLockon)
            {
                Vector3 lerpPosition = Vector3.Lerp(followPosition, targetPosition, 0.4f);
                Vector3 dir = lerpPosition - cameraPosition;
                target = Quaternion.Inverse(boneRotation) * Quaternion.LookRotation(dir);
            }
            rotation = Quaternion.Slerp(rotation, target, 1f - Mathf.Exp(-10 * deltaTime));
        }
    }
}
