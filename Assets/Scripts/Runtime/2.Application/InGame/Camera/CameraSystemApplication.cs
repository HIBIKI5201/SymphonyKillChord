using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Domain.InGame.Camera;
using KillChord.Runtime.Utility;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラシステム全体の更新ロジックを統合して管理するアプリケーション層のクラス。
    /// </summary>
    public sealed class CameraSystemApplication
    {
        /// <summary>
        ///     コンストラクタ。各サブシステムと設定パラメータを受け取り初期化する。
        /// </summary>
        /// <param name="parameter"> カメラシステムの設定パラメータ。</param>
        /// <param name="followSystem"> カメラ追従処理を担うアプリケーション。</param>
        /// <param name="boneRotationSystem"> ロックオン時のボーン回転処理を担うアプリケーション。</param>
        /// <param name="freeLookRotationSystem"> フリールック時のボーン回転処理を担うアプリケーション。</param>
        /// <param name="cameraRotationSystem"> カメラ本体の回転処理を担うアプリケーション。</param>
        /// <param name="targetSelector"> ロックオン対象の選択を担うセレクター。</param>
        public CameraSystemApplication(
            CameraSystemParameter parameter,
            CameraFollowApplication followSystem,
            CameraBoneLockOnRotationApplication boneRotationSystem,
            CameraBoneFreeLookRotationApplication freeLookRotationSystem,
            CameraRotationApplication cameraRotationSystem,
            TargetSelector targetSelector
        )
        {
            _parameter = parameter;
            _followSystem = followSystem;
            _boneRotationSystem = boneRotationSystem;
            _freeLookRotationSystem = freeLookRotationSystem;
            _cameraRotationSystem = cameraRotationSystem;
            _targetSelector = targetSelector;

            _distance = _parameter.Distance;
        }

        /// <summary>
        ///     攻撃時にオートロックオンを発動する。
        ///     マニュアルロックオン中は何もしない。
        /// </summary>
        /// <param name="currentPosition"> プレイヤーの現在位置。</param>
        public void TryActiveAutoLockOn(in Vector3 currentPosition)
        {
            if (_lockOnState == CameraLockOnState.LockOnManual)
            { return; }
            _lockOnState = CameraLockOnState.LockOnAuto;

            Vector3 dir = _cameraBoneRotation * _cameraRotation * Vector3.forward;
            _targetSelector.ChangeTarget(currentPosition, dir);
        }

        /// <summary>
        ///     マニュアルロックオン状態をトグルする。
        ///     ロックオン中であれば解除し、そうでなければ対象を選択してロックオンする。
        /// </summary>
        /// <param name="currentPosition"> プレイヤーの現在位置。</param>
        public void ToggleLockOnState(in Vector3 currentPosition)
        {
            if (!IsLockOn())
            {
                _lockOnState = CameraLockOnState.LockOnManual;

                Vector3 dir = _cameraBoneRotation * _cameraRotation * Vector3.forward;
                _targetSelector.ChangeTarget(currentPosition, dir);
            }
            else
            {
                _lockOnState = CameraLockOnState.Free;
            }
        }

        /// <summary>
        ///     カメラシステムを1フレーム分更新し、結果の回転と位置を返す。
        /// </summary>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        /// <param name="resultRotation"> 計算結果のカメラ回転。</param>
        /// <param name="resultPosition"> 計算結果のカメラ位置。</param>
        public void Update(in CameraSystemContext context, out Quaternion resultRotation, out Vector3 resultPosition)
        {
            // 反転処理済みの Context
            CameraSystemContext resolvedContext = new(
                context.FollowPosition,
                ApplyInvert(context.Input),
                context.MoveInput,
                context.DeltaTime
            );

            // オートロックオン中に入力があった場合はフリーに戻す
            if (_lockOnState == CameraLockOnState.LockOnAuto && (resolvedContext.Input.sqrMagnitude > float.Epsilon
                || resolvedContext.MoveInput.sqrMagnitude > float.Epsilon))
            {
                _lockOnState = CameraLockOnState.Free;
            }

            // ロックオン中は対象の位置を取得し、取得できなければフリーに戻す
            Vector3 targetPosition = Vector3.zero;
            if (IsLockOn())
            {
                Vector3 dir = _cameraBoneRotation * _cameraRotation * Vector3.forward;
                if (!_targetSelector.TryGetTargetPosition(resolvedContext.FollowPosition, dir, out targetPosition))
                {
                    _lockOnState = CameraLockOnState.Free;
                }
            }

            UpdateCameraBone(resolvedContext, targetPosition);
            _followSystem.Update(ref _cameraCenterOffset, resolvedContext);
            _cameraRotationSystem.Update(IsLockOn(), ref _cameraRotation, _cameraBoneRotation, _previousCameraPosition, resolvedContext, targetPosition);


            CalculateCameraPlacement(resolvedContext, out (Vector3 CameraAnchorPosition, Vector3 Direction, float Distance) result);
            UpdateDistance(ref _distance, result.Distance, resolvedContext.DeltaTime);


            resultPosition = result.CameraAnchorPosition + result.Direction * _distance;
            resultRotation = _cameraBoneRotation * _cameraRotation;

            _previousCameraPosition = resultPosition;
        }

        /// <summary> カメラ距離の補間速度。 </summary>
        private const float DISTANCE_LERP_SPEED = 4f;

        /// <summary> 障害物衝突時のカメラ最小距離。 </summary>
        private const float MIN_CAMERA_DISTANCE = 0.1f;

        private readonly CameraSystemParameter _parameter;

        private readonly CameraFollowApplication _followSystem;
        private readonly CameraBoneLockOnRotationApplication _boneRotationSystem;
        private readonly CameraBoneFreeLookRotationApplication _freeLookRotationSystem;
        private readonly CameraRotationApplication _cameraRotationSystem;
        private readonly TargetSelector _targetSelector;

        private float _distance;
        private Vector3 _cameraCenterOffset;
        private Vector3 _previousCameraPosition;
        private Quaternion _cameraRotation = Quaternion.identity;
        private Quaternion _cameraBoneRotation = Quaternion.identity;
        private CameraLockOnState _lockOnState;

        /// <summary>
        ///     現在ロックオン状態であるかを返す。
        /// </summary>
        /// <returns> ロックオン中の場合は true。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsLockOn()
        {
            return _lockOnState != CameraLockOnState.Free;
        }

        /// <summary>
        ///     カメラ距離を目標距離へ向けて補間して更新する。
        /// </summary>
        /// <param name="currentDistance"> 現在のカメラ距離。参照渡しで更新される。</param>
        /// <param name="targetDistance"> 目標のカメラ距離。</param>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateDistance(ref float currentDistance, float targetDistance, float deltaTime)
        {
            currentDistance = Mathf.Lerp(Mathf.Min(currentDistance, targetDistance), targetDistance, deltaTime * DISTANCE_LERP_SPEED);
        }

        /// <summary>
        ///     現在のカメラ状態からカメラの配置情報（アンカー座標・方向・距離）を計算する。
        /// </summary>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        /// <param name="result"> 計算結果のアンカー座標・方向・距離のタプル。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateCameraPlacement(in CameraSystemContext context, out (Vector3 CameraAnchorPosition, Vector3 Direction, float Distance) result)
        {
            result.CameraAnchorPosition = context.FollowPosition + _cameraCenterOffset + _parameter.Offset;

            result.Direction = _cameraBoneRotation * Vector3.back;

            result.Distance = GetDistance(result.CameraAnchorPosition, result.Direction, _parameter.Distance);
        }

        /// <summary>
        ///     ロックオン状態に応じてカメラボーンの回転を更新する。
        /// </summary>
        /// <param name="context"> 今フレームの更新コンテキスト。</param>
        /// <param name="targetPosition"> ロックオン対象のワールド座標。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateCameraBone(in CameraSystemContext context, in Vector3 targetPosition)
        {
            if (_lockOnState != CameraLockOnState.Free)
            {
                _boneRotationSystem.Update(ref _cameraBoneRotation, context, targetPosition);
            }
            else
            {
                _freeLookRotationSystem.Update(ref _cameraBoneRotation, context);
            }
        }

        /// <summary>
        ///     SphereCast による衝突判定を行い、障害物を考慮したカメラ距離を返す。
        /// </summary>
        /// <param name="center"> SphereCast の始点。</param>
        /// <param name="direction"> SphereCast の方向。</param>
        /// <param name="maxDistance"> 衝突がない場合の最大距離。</param>
        /// <returns> 障害物までの距離。衝突がない場合は最大距離を返す。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetDistance(in Vector3 center, in Vector3 direction, float maxDistance)
        {
            if (Physics.SphereCast(center, _parameter.CollisionRadius, direction, out RaycastHit hit, maxDistance, _parameter.CollisionMask))
            {
                return Mathf.Max(MIN_CAMERA_DISTANCE, hit.distance);
            }
            return maxDistance;
        }

        /// <summary>
        ///     設定に基づき入力の垂直・水平反転を適用する。
        /// </summary>
        /// <param name="input"> 反転前の入力値。</param>
        /// <returns> 反転処理後の入力値。</returns>
        private Vector2 ApplyInvert(Vector2 input)
        {
            if (_parameter.IsInvertVertical)
            {
                input.y = -input.y;
            }

            if (_parameter.IsInvertHorizontal)
            {
                input.x = -input.x;
            }

            return input;
        }
    }
}
