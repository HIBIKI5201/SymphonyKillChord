using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラの回転（注視点制御など）を担当するクラス。
    ///     ロックオン状態や対象の変更に対して注視回転を滑らかに追従させる。
    /// </summary>
    public sealed class CameraLookAtRotationCalculator
    {
        /// <summary>
        ///     Camera View 用パラメータを受け取り、カメラ回転制御を初期化するコンストラクタ。
        /// </summary>
        /// <param name="parameter"> Camera View 用パラメータ。</param>
        public CameraLookAtRotationCalculator(CameraConfig parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        ///     ロックオン状態に応じてカメラの注視点回転を更新する。
        ///     ロックオン中はプレイヤーのモデル中心と対象の中間点を注視するよう補間する。
        ///     状態や対象が切り替わった場合も現在回転から目標回転まで継続的に補間する。
        /// </summary>
        /// <param name="isLockOn"> ロックオン中かどうか。</param>
        /// <param name="rotation"> 更新対象のカメラ回転。参照渡しで更新される。</param>
        /// <param name="boneTargetRotation"> ボーンの目標回転。収束途中の値ではなく最終目標を渡す。</param>
        /// <param name="cameraPosition"> 現在のカメラ位置。</param>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        public void Update(
            bool isLockOn,
            ref Quaternion rotation,
            in Quaternion boneTargetRotation,
            in Vector3 cameraPosition,
            in CameraUpdateContext context,
            in Vector3 targetPosition
        )
        {
            Quaternion target = Quaternion.identity;
            if (isLockOn)
            {
                // プレイヤーのモデル中心位置を求める
                Vector3 playerCenter = context.FollowPosition + _parameter.CharacterCenterOffset;

                // プレイヤーのモデル中心と対象の中間点を求める
                Vector3 lerpPosition = Vector3.Lerp(playerCenter, targetPosition, _parameter.LockOnLookAtRatio);
                Vector3 dir = lerpPosition - cameraPosition;
                if (dir.sqrMagnitude > float.Epsilon)
                {
                    // bone の目標回転の Inverse を使うことで、bone 収束中に target が変動しない
                    target = Quaternion.Inverse(boneTargetRotation) * Quaternion.LookRotation(dir);
                }
            }

            float interpolationRatio = 1f - Mathf.Exp(
                -Mathf.Max(0f, _parameter.LockOnRotationSpeed) * Mathf.Max(0f, context.DeltaTime));
            rotation = Quaternion.Slerp(rotation, target, interpolationRatio);
        }

        private readonly CameraConfig _parameter;
    }
}
