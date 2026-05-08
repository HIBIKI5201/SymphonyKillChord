using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラのロックオン時の回転制御を担当するクラス。
    /// </summary>
    public sealed class CameraBoneLockOnRotationApplication
    {
        /// <summary>
        ///     カメラシステムの設定パラメータを受け取り、ロックオン回転制御を初期化するコンストラクタ。
        /// </summary>
        /// <param name="parameter"> カメラシステムの設定パラメータ。</param>
        public CameraBoneLockOnRotationApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        ///     ロックオン対象方向へカメラボーンを滑らかに回転させる。
        ///     対象との角度がマージン以内の場合は更新をスキップする。
        /// </summary>
        /// <param name="cameraBoneRotation"> 更新対象のカメラボーン回転。参照渡しで更新される。</param>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        public void Update(ref Quaternion cameraBoneRotation, in CameraSystemContext context, in Vector3 targetPosition)
        {
            Vector3 followDir = targetPosition - context.FollowPosition;
            followDir.y = 0;
            if (followDir.sqrMagnitude <= float.Epsilon)
            {
                return;
            }

            Vector3 cameraDir = cameraBoneRotation * Vector3.forward;
            cameraDir.y = 0;
            if (cameraDir.sqrMagnitude <= float.Epsilon)
            {
                return;
            }

            float angle = Vector3.Angle(followDir, cameraDir);

            float lockOnAngleMargin = _parameter.LockOnAngleMargin;

            // マージン以内の場合は補正不要
            if (angle < lockOnAngleMargin)
            {
                return;
            }
            float crossY = Vector3.Cross(followDir, cameraDir).y;

            // 外積の符号でマージンオフセットの方向を決定する
            Quaternion target = Quaternion.LookRotation(followDir, Vector3.up) * Quaternion.Euler(0, (crossY <= 0) ? -lockOnAngleMargin : lockOnAngleMargin, 0);

            cameraBoneRotation = Quaternion.Slerp(cameraBoneRotation, target, 1f - Mathf.Exp(-_parameter.BoneRotateSpeed * context.DeltaTime));
        }

        private readonly CameraSystemParameter _parameter;
    }
}
