using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラの回転（注視点制御など）を担当するクラス。
    /// </summary>
    public sealed class CameraRotationApplication
    {
        public CameraRotationApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(
            bool isLockOn,
            ref Quaternion rotation,
            in Quaternion boneRotation,
            in Vector3 cameraPosition,
            in CameraSystemContext context,
            in Vector3 TargetPosition
        )
        {
            Quaternion target = Quaternion.identity;
            if (isLockOn)
            {
                Vector3 lerpPosition = Vector3.Lerp(context.FollowPosition, TargetPosition, _parameter.LockOnLookAtRatio);
                Vector3 dir = lerpPosition - cameraPosition;
                if (dir.sqrMagnitude > float.Epsilon)
                {
                    target = Quaternion.Inverse(boneRotation) * Quaternion.LookRotation(dir);
                }
            }
            rotation = Quaternion.Slerp(rotation, target, 1f - Mathf.Exp(-_parameter.LockOnRotationSpeed * context.DeltaTime));
        }

        private readonly CameraSystemParameter _parameter;
    }
}
