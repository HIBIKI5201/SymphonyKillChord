using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラの回転（注視点制御など）を担当するクラス。
    ///     ロックオンの切り替えを ratio で滑らかに表現し、回転速度と補間を分離する。
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
        ///     ratio をロックオンの有無でスムーズに変化させることで、
        ///     回転速度パラメータと補間処理を分離し target のブレによるバウンスを防ぐ。
        /// </summary>
        /// <param name="isLockOn"> ロックオン中かどうか。</param>
        /// <param name="rotation"> 更新対象のカメラ回転。参照渡しで更新される。</param>
        /// <param name="boneTargetRotation"> ボーンの目標回転。収束途中の値ではなく最終目標を渡す。</param>
        /// <param name="cameraPosition"> 現在のカメラ位置。</param>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        /// <param name="lockOnOffset"> 対象方向へ加算する相対角度。</param>
        public void Update(
            bool isLockOn,
            ref Quaternion rotation,
            in Quaternion boneTargetRotation,
            in Vector3 cameraPosition,
            in CameraUpdateContext context,
            in Vector3 targetPosition,
            in Vector2 lockOnOffset
        )
        {
            // ロックオンの有無に応じて ratio を 0〜1 でスムーズに変化させる
            _ratio = Mathf.Lerp(_ratio, isLockOn ? 1f : 0f, context.DeltaTime * _parameter.LockOnRotationSpeed);

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
                    Quaternion offsetRotation = Quaternion.Euler(lockOnOffset.y, lockOnOffset.x, 0f);
                    target = Quaternion.Inverse(boneTargetRotation) * Quaternion.LookRotation(dir) * offsetRotation;
                }
            }

            // ratio を Slerp の t として使用することで、回転速度と補間処理を分離する
            rotation = Quaternion.Slerp(rotation, target, _ratio);
        }

        private readonly CameraConfig _parameter;

        /// <summary> ロックオン切り替えの補間比率。0: フリー状態, 1: ロックオン完了状態。 </summary>
        private float _ratio;
    }
}
