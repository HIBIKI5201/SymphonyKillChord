using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラのロックオン時の回転制御を担当するクラス。
    /// </summary>
    public sealed class CameraLockOnRotationCalculator
    {
        /// <summary>
        ///     Camera View 用パラメータを受け取り、ロックオン回転制御を初期化するコンストラクタ。
        /// </summary>
        /// <param name="parameter"> Camera View 用パラメータ。</param>
        public CameraLockOnRotationCalculator(CameraConfig parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        ///     ロックオン対象方向へカメラボーンを滑らかに回転させる。
        ///     対象との角度がマージン内の場合はオフセットなし、マージン外の場合はマージンオフセットを適用し、プレイヤーモデルがフレーム内に収まるよう制御する。
        ///     プレイヤーと対象が同一 XZ 座標の場合、またはカメラの前方向の XZ 成分がゼロの場合は更新をスキップする。
        /// </summary>
        /// <param name="cameraBoneRotation"> 更新対象のカメラボーン回転。参照渡しで更新される。</param>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        /// <param name="lockOnOffset"> 対象方向へ加算する相対角度。</param>
        public void Update(
            ref Quaternion cameraBoneRotation,
            in CameraUpdateContext context,
            in Vector3 targetPosition,
            in Vector2 lockOnOffset)
        {
            if (!TryCalcTargetRotation(context.FollowPosition, targetPosition, cameraBoneRotation, lockOnOffset, out Quaternion target))
            {
                return;
            }

            cameraBoneRotation = Quaternion.Slerp(cameraBoneRotation, target, 1f - Mathf.Exp(-_parameter.BoneRotateSpeed * context.DeltaTime));
        }

        /// <summary>
        ///     ロックオン時のボーンの目標回転を取得する。
        ///     対象との角度がマージン内の場合はオフセットなし、マージン外の場合はマージンオフセットを適用した回転を返すことで、プレイヤーモデルが常にフレーム内に収まることを保証する。
        ///     プレイヤーと対象が同一 XZ 座標の場合、またはカメラの前方向の XZ 成分がゼロの場合は false を返し、<paramref name="targetRotation"/> は現在の回転をそのまま返す。
        /// </summary>
        /// <param name="followPosition"> 追従対象のワールド座標。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        /// <param name="currentBoneRotation"> 現在のカメラボーン回転。</param>
        /// <param name="lockOnOffset"> 対象方向へ加算する相対角度。</param>
        /// <param name="targetRotation"> 計算した目標回転。計算不能な場合は <paramref name="currentBoneRotation"/> をそのまま返す。</param>
        /// <returns> 目標回転を計算できた場合は true、計算不能な場合は false。</returns>
        public bool TryGetTargetRotation(
            in Vector3 followPosition,
            in Vector3 targetPosition,
            in Quaternion currentBoneRotation,
            in Vector2 lockOnOffset,
            out Quaternion targetRotation)
        {
            return TryCalcTargetRotation(followPosition, targetPosition, currentBoneRotation, lockOnOffset, out targetRotation);
        }

        private readonly CameraConfig _parameter;

        /// <summary>
        ///     対象方向へのボーン目標回転を計算する共通ロジック。
        ///     対象との角度がマージン内の場合はオフセットなし、マージン外の場合は外積の Y 成分でカメラが対象の左右どちらにいるかを判定し、マージンオフセットを適用した目標回転を返す。
        ///     プレイヤーと対象が同一 XZ 座標の場合、またはカメラの前方向の XZ 成分がゼロの場合は false を返す。
        /// </summary>
        /// <param name="followPosition"> 追従対象のワールド座標。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        /// <param name="currentBoneRotation"> 現在のカメラボーン回転。</param>
        /// <param name="lockOnOffset"> 対象方向へ加算する相対角度。</param>
        /// <param name="targetRotation"> 計算した目標回転。計算不能な場合は <paramref name="currentBoneRotation"/> をそのまま返す。</param>
        /// <returns> 目標回転を計算できた場合は true、計算不能な場合は false。</returns>
        private bool TryCalcTargetRotation(
            in Vector3 followPosition,
            in Vector3 targetPosition,
            in Quaternion currentBoneRotation,
            in Vector2 lockOnOffset,
            out Quaternion targetRotation)
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
            if (angle <= lockOnAngleMargin)
            {
                // すでにマージン内にいる場合は、マージンオフセットなしの回転を返す
                targetRotation = ApplyOffset(Quaternion.LookRotation(followDir, Vector3.up), lockOnOffset);
                return true;
            }

            float crossY = Vector3.Cross(followDir, cameraDir).y;

            // 外積の Y 成分でカメラが対象の左右どちらにいるかを判定し、マージンオフセットの方向を決定する。
            Quaternion baseRotation = Quaternion.LookRotation(followDir, Vector3.up)
                * Quaternion.Euler(0f, crossY <= 0 ? -lockOnAngleMargin : lockOnAngleMargin, 0f);
            targetRotation = ApplyOffset(baseRotation, lockOnOffset);

            return true;
        }

        /// <summary>
        ///     対象方向の回転へ相対ヨー・ピッチ角を加算します。
        /// </summary>
        /// <param name="baseRotation"> 対象方向を向く基準回転。 </param>
        /// <param name="lockOnOffset"> Xがヨー、Yがピッチの相対角度。 </param>
        /// <returns> 相対角度を加算した回転。 </returns>
        private static Quaternion ApplyOffset(in Quaternion baseRotation, in Vector2 lockOnOffset)
        {
            return baseRotation * Quaternion.Euler(lockOnOffset.y, lockOnOffset.x, 0f);
        }
    }
}
