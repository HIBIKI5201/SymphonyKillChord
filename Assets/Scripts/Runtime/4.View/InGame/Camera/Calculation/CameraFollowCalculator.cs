using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラの追従移動の制御を担当するクラス。
    /// </summary>
    public sealed class CameraFollowCalculator
    {
        /// <summary>
        ///     Camera View 用パラメータを受け取り、追従移動制御を初期化するコンストラクタ。
        /// </summary>
        /// <param name="parameter"> Camera View 用パラメータ。</param>
        public CameraFollowCalculator(CameraViewSettings parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        ///     追従対象の速度に基づいてカメラの中心オフセットを更新する。
        /// </summary>
        /// <param name="cameraCenterPosition"> カメラ中心のオフセット。参照渡しで更新される。</param>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        public void Update(ref Vector3 cameraCenterPosition, in CameraUpdateContext context)
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

        private readonly CameraViewSettings _parameter;
        private CameraFollowVelocityTracker _followVelocity;
    }
}
