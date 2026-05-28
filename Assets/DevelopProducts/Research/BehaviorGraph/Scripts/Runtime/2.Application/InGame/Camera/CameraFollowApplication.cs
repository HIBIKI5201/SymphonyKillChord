using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラの追従移動の制御を担当するクラス。
    /// </summary>
    public sealed class CameraFollowApplication
    {
        public CameraFollowApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(ref Vector3 cameraCenterPosition, in CameraSystemContext context)
        {
            Vector3 targetFollowCenterOffset = -_followVelocity.UpdateFollowVelocity(context.FollowPosition, context.DeltaTime);
            targetFollowCenterOffset.y = 0;
            if (targetFollowCenterOffset.sqrMagnitude >= _parameter.FollowOffsetPower * _parameter.FollowOffsetPower)
            {
                targetFollowCenterOffset.Normalize();
                targetFollowCenterOffset *= _parameter.FollowOffsetPower;
            }

            cameraCenterPosition = Vector3.Lerp(cameraCenterPosition, targetFollowCenterOffset, _parameter.FollowLerpSpeed * context.DeltaTime);
        }

        private readonly CameraSystemParameter _parameter;
        private CameraFollowVelocityApplication _followVelocity;
    }
}
