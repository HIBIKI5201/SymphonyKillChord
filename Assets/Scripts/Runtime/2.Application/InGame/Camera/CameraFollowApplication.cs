using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラの追従移動の制御を担当するクラス。
    /// </summary>
    public sealed class CameraFollowApplication
    {
        /// <summary>
        ///     カメラシステムの設定パラメータを受け取り、追従移動制御を初期化するコンストラクタ。
        /// </summary>
        /// <param name="parameter"> カメラシステムの設定パラメータ。</param>
        public CameraFollowApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        ///     追従対象の速度に基づいてカメラの中心オフセットを更新する。
        /// </summary>
        /// <param name="cameraCenterPosition"> カメラ中心のオフセット。参照渡しで更新される。</param>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        public void Update(ref Vector3 cameraCenterPosition, in CameraSystemContext context)
        {
            Vector3 targetFollowCenterOffset = -_followVelocity.UpdateFollowVelocity(context.FollowPosition, context.DeltaTime);
            targetFollowCenterOffset.y = 0;

            // オフセットの最大値を制限する
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
