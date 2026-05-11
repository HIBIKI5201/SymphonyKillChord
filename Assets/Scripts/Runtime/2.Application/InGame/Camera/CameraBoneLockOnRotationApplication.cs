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
            if (!TryCalcTargetRotation(context.FollowPosition, targetPosition, cameraBoneRotation, out Quaternion target))
            {
                return;
            }

            cameraBoneRotation = Quaternion.Slerp(cameraBoneRotation, target, 1f - Mathf.Exp(-_parameter.BoneRotateSpeed * context.DeltaTime));
        }

        /// <summary>
        ///     ロックオン時のボーンの目標回転を取得する。
        ///     マージン以内の場合は false を返し、<paramref name="targetRotation"/> は現在の回転をそのまま返す。
        /// </summary>
        /// <param name="followPosition"> 追従対象のワールド座標。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        /// <param name="currentBoneRotation"> 現在のカメラボーン回転。</param>
        /// <param name="targetRotation"> 計算した目標回転。</param>
        /// <returns> マージンを超えている場合は true、以内の場合は false。</returns>
        public bool TryGetTargetRotation(in Vector3 followPosition, in Vector3 targetPosition, in Quaternion currentBoneRotation, out Quaternion targetRotation)
        {
            return TryCalcTargetRotation(followPosition, targetPosition, currentBoneRotation, out targetRotation);
        }

        private readonly CameraSystemParameter _parameter;

        /// <summary>
        ///     対象方向へのボーン目標回転を計算する共通ロジック。
        ///     マージン以内の場合は false を返す。
        /// </summary>
        /// <param name="followPosition"> 追従対象のワールド座標。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        /// <param name="currentBoneRotation"> 現在のカメラボーン回転。</param>
        /// <param name="targetRotation"> 計算した目標回転。</param>
        /// <returns> マージンを超えている場合は true、以内の場合は false。</returns>
        private bool TryCalcTargetRotation(in Vector3 followPosition, in Vector3 targetPosition, in Quaternion currentBoneRotation, out Quaternion targetRotation)
        {
            targetRotation = currentBoneRotation;

            Vector3 followDir = targetPosition - followPosition;
            followDir.y = 0;
            if (followDir.sqrMagnitude <= float.Epsilon)
            {
                return false;
            }

            Vector3 cameraDir = currentBoneRotation * Vector3.forward;
            cameraDir.y = 0;
            if (cameraDir.sqrMagnitude <= float.Epsilon)
            {
                return false;
            }

            float angle = Vector3.Angle(followDir, cameraDir);
            float lockOnAngleMargin = _parameter.LockOnAngleMargin;

            // マージン以内の場合は補正不要
            if (angle < lockOnAngleMargin)
            {
                return false;
            }

            float crossY = Vector3.Cross(followDir, cameraDir).y;

            // 外積の符号でマージンオフセットの方向を決定する
            targetRotation = Quaternion.LookRotation(followDir, Vector3.up)
                * Quaternion.Euler(0, (crossY <= 0) ? -lockOnAngleMargin : lockOnAngleMargin, 0);

            return true;
        }
    }
}
