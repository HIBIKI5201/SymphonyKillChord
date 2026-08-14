using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラのフリー視点での回転制御を担当するクラス。
    /// </summary>
    public sealed class CameraFreeLookRotationCalculator
    {
        /// <summary>
        ///     Camera View 用パラメータを受け取り、フリー視点回転制御を初期化するコンストラクタ。
        /// </summary>
        /// <param name="parameter"> Camera View 用パラメータ。</param>
        public CameraFreeLookRotationCalculator(CameraConfig parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        ///     入力値をもとにカメラボーンのヨー・ピッチ回転を更新する。
        /// </summary>
        /// <param name="cameraBoneRotation"> 更新対象のカメラボーン回転。参照渡しで更新される。</param>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        public void Update(ref Quaternion cameraBoneRotation, in CameraUpdateContext context)
        {
            Vector3 euler = cameraBoneRotation.eulerAngles;

            // Unity のオイラー X 軸は0〜360で返るため、負方向（仰角）を正規化する
            if (euler.x > EULER_ANGLE_HALF)
            {
                euler.x -= EULER_ANGLE_FULL;
            }
            float yaw = euler.y;
            float pitch = euler.x - context.Input.y * _parameter.FollowRotationSpeed * context.DeltaTime;

            if (context.Input.sqrMagnitude >= _parameter.MoveFollowIdleLookThreshold * _parameter.MoveFollowIdleLookThreshold)
            {
                yaw += context.Input.x * _parameter.FollowRotationSpeed * context.DeltaTime;
            }
            else if (context.MoveInput.sqrMagnitude > float.Epsilon && !IsMovingStraightBackward(context.MoveInput))
            {
                Vector3 playerForward = context.PlayerForward;
                playerForward.y = 0f;
                if (playerForward.sqrMagnitude > float.Epsilon)
                {
                    float targetYaw = Quaternion.LookRotation(playerForward.normalized, Vector3.up).eulerAngles.y;
                    float angleDifference = Mathf.Abs(Mathf.DeltaAngle(yaw, targetYaw));
                    if (angleDifference > _parameter.MoveFollowAngleDeadZone)
                    {
                        yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, _parameter.MoveFollowRotationSpeed * context.DeltaTime);
                    }
                }
            }

            // ピッチ角の制限
            pitch = Mathf.Clamp(pitch, _parameter.PitchRange.x, _parameter.PitchRange.y);

            cameraBoneRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        /// <summary> オイラー角の半周（負方向ピッチの正規化に使用する閾値）。 </summary>
        private const float EULER_ANGLE_HALF = 180f;

        /// <summary> オイラー角の全周（負方向ピッチの正規化に使用するオフセット）。 </summary>
        private const float EULER_ANGLE_FULL = 360f;

        /// <summary> 真後ろ入力として許容する、後方成分に対する横方向成分の割合。 </summary>
        private const float STRAIGHT_BACKWARD_HORIZONTAL_RATIO = 0.1f;

        private readonly CameraConfig _parameter;

        /// <summary>
        ///     カメラの自動回転追従を停止する真後ろ入力かどうかを判定する。
        /// </summary>
        /// <param name="moveInput"> 移動操作の入力値。</param>
        /// <returns> 横方向成分が許容範囲内で、後方へ入力されている場合は true。</returns>
        private static bool IsMovingStraightBackward(in Vector2 moveInput)
        {
            return moveInput.y < 0f
                && Mathf.Abs(moveInput.x) <= -moveInput.y * STRAIGHT_BACKWARD_HORIZONTAL_RATIO;
        }
    }
}
