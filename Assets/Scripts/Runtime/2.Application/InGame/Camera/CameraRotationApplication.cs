using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラの回転（注視点制御など）を担当するクラス。
    /// </summary>
    public sealed class CameraRotationApplication
    {
        /// <summary>
        ///     カメラシステムの設定パラメータを受け取り、カメラ回転制御を初期化するコンストラクタ。
        /// </summary>
        /// <param name="parameter"> カメラシステムの設定パラメータ。</param>
        public CameraRotationApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        ///     ロックオン状態に応じてカメラの注視点回転を更新する。
        ///     ロックオン中はプレイヤーと対象の中間点を注視するよう補間する。
        /// </summary>
        /// <param name="isLockOn"> ロックオン中かどうか。</param>
        /// <param name="rotation"> 更新対象のカメラ回転。参照渡しで更新される。</param>
        /// <param name="boneRotation"> 現在のカメラボーン回転。</param>
        /// <param name="cameraPosition"> 現在のカメラ位置。</param>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        public void Update(
            bool isLockOn,
            ref Quaternion rotation,
            in Quaternion boneRotation,
            in Vector3 cameraPosition,
            in CameraSystemContext context,
            in Vector3 targetPosition
        )
        {
            Quaternion target = Quaternion.identity;
            if (isLockOn)
            {
                // プレイヤーと対象の中間点を求める
                Vector3 lerpPosition = Vector3.Lerp(context.FollowPosition, targetPosition, _parameter.LockOnLookAtRatio);
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
