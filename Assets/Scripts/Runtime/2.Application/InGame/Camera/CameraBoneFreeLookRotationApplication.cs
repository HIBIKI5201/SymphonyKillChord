using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラのフリー視点での回転制御を担当するクラス。
    /// </summary>
    public sealed class CameraBoneFreeLookRotationApplication
    {
        /// <summary>
        ///     カメラシステムの設定パラメータを受け取り、フリー視点回転制御を初期化するコンストラクタ。
        /// </summary>
        /// <param name="parameter"> カメラシステムの設定パラメータ。</param>
        public CameraBoneFreeLookRotationApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        ///     入力値をもとにカメラボーンのヨー・ピッチ回転を更新する。
        /// </summary>
        /// <param name="cameraBoneRotation"> 更新対象のカメラボーン回転。参照渡しで更新される。</param>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        public void Update(ref Quaternion cameraBoneRotation, in CameraSystemContext context)
        {
            Vector3 euler = cameraBoneRotation.eulerAngles;

            // Unity のオイラー X 軸は0〜360で返るため、負方向（仰角）を正規化する
            if (euler.x > EULER_ANGLE_HALF)
            {
                euler.x -= EULER_ANGLE_FULL;
            }
            float yaw = euler.y + context.Input.x * _parameter.FollowRotationSpeed * context.DeltaTime;
            float pitch = euler.x - context.Input.y * _parameter.FollowRotationSpeed * context.DeltaTime;

            // ピッチ角の制限
            pitch = Mathf.Clamp(pitch, _parameter.PitchRange.x, _parameter.PitchRange.y);

            cameraBoneRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        /// <summary> オイラー角の半周（負方向ピッチの正規化に使用する閾値）。 </summary>
        private const float EULER_ANGLE_HALF = 180f;

        /// <summary> オイラー角の全周（負方向ピッチの正規化に使用するオフセット）。 </summary>
        private const float EULER_ANGLE_FULL = 360f;

        private readonly CameraSystemParameter _parameter;
    }
}
