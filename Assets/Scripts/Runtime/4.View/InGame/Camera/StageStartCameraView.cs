using KillChord.Runtime.View.InGame.Sequence;
using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     ステージ開始時のカメラ演出を制御するViewクラス。
    /// </summary>
    public class StageStartCameraView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="cameraSystemView"> カメラシステムのビュー。 </param>
        /// <param name="playerTransform"> プレイヤーのTransform。 </param>
        public void Initialize(
            CameraSystemView cameraSystemView,
            Transform playerTransform)
        {
            Cancel();

            _cameraSystemView = cameraSystemView;
            _playerTransform = playerTransform;
        }

        /// <summary>
        ///     カメラ演出の準備を行います。
        /// </summary>
        /// <returns> 準備が成功した場合はtrue、それ以外の場合はfalse。 </returns>
        public bool Prepare()
        {
            Cancel();

            // 参照の設定が正しいか検証する。
            if (!ValidateReferences())
            {
                return false;
            }

            if (!_cameraSystemView.RefreshImmediate())
            {
                Debug.LogError(
                    $"[{nameof(StageStartCameraView)}] "
                    + $"通常カメラのリフレッシュに失敗しました。",
                    this);
                return false;
            }

            // 参照の設定が正しい場合は、カメラ演出を開始する。
            _cameraSystemView.BeginExternalControl();
            _isPrepared = true;

            ApplyOrbit(0f);
            return true;
        }

        /// <summary>
        ///     正面から背面へのカメラ演出を再生します。
        /// </summary>
        /// <param name="onCompleted"> 演出完了時に呼び出されるコールバック。 </param>
        public void Play(Action onCompleted = null)
        {
            if (!_isPrepared)
            {
                onCompleted?.Invoke();
                return;
            }

            _onCompleted = onCompleted;
            StartOrbit();
        }

        /// <summary>
        ///     再生中のカメラ演出をキャンセルし、通常カメラへ復帰する。
        /// </summary>
        public void Cancel()
        {
            _onCompleted = null;
            _motionHandle.TryCancel();

            if (!_isPrepared
                || _cameraSystemView == null)
            {
                _isPrepared = false;
                return;
            }

            _cameraSystemView.EndExternalControl();
            _cameraSystemView.RefreshImmediate();
            _isPrepared = false;
        }

        /// <summary> 方向ベクトルとして扱える最小二乗長。 </summary>
        private const float MIN_DIRECTION_SQR_MAGNITUDE = 0.0001f;

        [SerializeField, Tooltip("ステージ開始時のカメラの設定情報")]
        private StageStartSequenceConfig _config;

        private CameraSystemView _cameraSystemView;
        private Transform _playerTransform;
        private MotionHandle _motionHandle;
        private CameraPose _handoffPose;
        private CameraPose _handoffStartPose;
        private Action _onCompleted;
        private bool _isPrepared;

        private void OnDestroy()
        {
            Cancel();
        }

        /// <summary>
        ///     正面から背面へのカメラ演出を開始します。
        /// </summary>
        private void StartOrbit()
        {
            float duration = _config.OrbitDuration;

            // 正面ショットから背面ショットへのオービットを開始する。
            _motionHandle = LMotion.Create(0f, 1f, duration)
                .WithEase(_config.OrbitEasing)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(StartRearHold)
                .Bind(this, static (progress, view) =>
                {
                    view.ApplyOrbit(progress);
                })
                .AddTo(gameObject);
        }

        /// <summary>
        ///     背面ショットで一時停止し、プレイヤー操作へのハンドオフを開始します。
        /// </summary>
        private void StartRearHold()
        {
            ApplyOrbit(1f);

            float duration = _config.RearHoldDuration;

            // 背面ショットで一時停止し、プレイヤー操作へのハンドオフを開始する。
            _motionHandle = LMotion.Create(0f, 1f, duration)
                .WithEase(_config.HandoffEasing)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(StartHandoff)
                .RunWithoutBinding()
                .AddTo(gameObject);
        }

        /// <summary>
        ///     背面ショットから通常カメラへのハンドオフを開始します。
        /// </summary>
        private void StartHandoff()
        {
            // 背面ショットから通常カメラへのハンドオフを開始する。
            if (!CaptureHandoffPose())
            {
                CompleteWithoutHandoff();
                return;
            }

            float duration = _config.HandoffDuration;

            // ハンドオフの進行度に応じて、背面ショットから通常カメラへの補間を行う。
            _motionHandle = LMotion.Create(0f, 1f, duration)
                .WithEase(_config.HandoffEasing)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(CompleteHandoff)
                .Bind(this, static (progress, view) =>
                {
                    view.ApplyHandoff(progress);
                })
                .AddTo(gameObject);
        }

        /// <summary>
        ///     正面ショットから背面ショットへのオービットを進行度に応じて反映します。
        /// </summary>
        /// <param name="progress"></param>
        private void ApplyOrbit(float progress)
        {
            if (!_isPrepared
                || _cameraSystemView == null
                || _cameraSystemView.CameraTransform == null)
            {
                return;
            }

            // 進行度を0から1の範囲に制限する。
            float clampedProgress = Mathf.Clamp01(progress);

            // 正面ショットから背面ショットへのオービットを進行度に応じて補間する。
            float yawAngle = Mathf.Lerp(
                _config.FrontHorizontalAngle,
                _config.RearHorizontalAngle,
                clampedProgress);

            float elevationAngle = Mathf.Lerp(
                _config.FrontVerticalAngle,
                _config.RearVerticalAngle,
                clampedProgress);

            float distance = Mathf.Lerp(
                _config.FrontDistance,
                _config.RearDistance,
                clampedProgress);

            float rollAngle = Mathf.Lerp(
                _config.FrontRollAngle,
                _config.RearRollAngle,
                clampedProgress);

            Vector3 lookAtPosition = Vector3.Lerp(
                _config.FrontLookAtLocalOffset,
                _config.RearLookAtLocalOffset,
                clampedProgress);

            ApplyShot(_cameraSystemView.CameraTransform,
                lookAtPosition,
                yawAngle,
                elevationAngle,
                distance,
                rollAngle);
        }

        /// <summary>
        ///     指定したパラメータに基づいてカメラの位置と回転を計算し、Transformへ反映します。
        /// </summary>
        /// <param name="cameraTransform"> カメラのTransform。 </param>
        /// <param name="lookAtloacloffset"> 注視点のローカルオフセット。 </param>
        /// <param name="yawAngle"> 水平方向の回転角度。 </param>
        /// <param name="elevationAngle"> 垂直方向の回転角度。 </param>
        /// <param name="distance"> カメラと注視点の距離。 </param>
        /// <param name="rollAngle"> ロール角度。 </param>
        private void ApplyShot(
            Transform cameraTransform,
            in Vector3 lookAtloacloffset,
            float yawAngle,
            float elevationAngle,
            float distance,
            float rollAngle)
        {
            // プレイヤーの上方向と前方向を取得する。
            Vector3 up = _playerTransform.up.normalized;
            Vector3 forward = Vector3.ProjectOnPlane(_playerTransform.forward, up).normalized;

            // プレイヤーの前方向がほぼゼロベクトルの場合、デフォルトの前方向を使用する。
            if (forward.sqrMagnitude <= MIN_DIRECTION_SQR_MAGNITUDE)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward = forward.normalized;
            }

            // 注視点のワールド座標を計算する。
            Vector3 lookAtPosition =
                _playerTransform.TransformPoint(lookAtloacloffset);

            Vector3 horizontalDirection =
                Quaternion.AngleAxis(yawAngle, up) * forward;

            float elevationRad =
                elevationAngle * Mathf.Deg2Rad;

            float horizontalDistance =
                distance * Mathf.Cos(elevationRad);

            float verticalOffset =
                distance * Mathf.Sin(elevationRad);

            Vector3 cameraPosition =
                lookAtPosition + horizontalDirection * horizontalDistance
                + up * verticalOffset;

            Vector3 lookDirection = lookAtPosition - cameraPosition;

            // 注視点とカメラ位置が同一の場合、LookRotationは計算できないため、エラーを出力する。
            if (lookDirection.sqrMagnitude <= MIN_DIRECTION_SQR_MAGNITUDE)
            {
                Debug.LogError($"[{nameof(StageStartCameraView)}] "
                    + $"カメラ位置と注視点が同一であるため、カメラ姿勢を計算できません。",
                    this);
                return;
            }

            // 注視点方向と上方向からカメラの回転を計算する。
            Quaternion lookRotation =
                Quaternion.LookRotation(lookDirection, up);

            Quaternion rollRotation =
                Quaternion.AngleAxis(rollAngle, Vector3.forward);

            cameraTransform.SetPositionAndRotation(
                cameraPosition,
                lookRotation * rollRotation);
        }

        /// <summary>
        ///     現在のプレイヤー状態に対する通常カメラ姿勢を取得します。
        /// </summary>
        /// <returns> 通常カメラ姿勢を取得できた場合はtrueです。 </returns>
        private bool CaptureHandoffPose()
        {
            Transform cameraTransform =
                _cameraSystemView.CameraTransform;

            _handoffStartPose = new CameraPose(
                cameraTransform.position,
                cameraTransform.rotation);

            // 同一フレーム内で通常カメラへ一度制御を戻し、
            // 現在のプレイヤー位置に対する姿勢を計算する。
            _cameraSystemView.EndExternalControl();

            if (!_cameraSystemView.RefreshImmediate())
            {
                _cameraSystemView.BeginExternalControl();
                _handoffStartPose.ApplyTo(cameraTransform);
                return false;
            }

            _handoffPose = new CameraPose(
                cameraTransform.position,
                cameraTransform.rotation);

            // 描画前に背面ショットへ戻すため、
            // 通常カメラ姿勢が途中で画面に映ることはない。
            _cameraSystemView.BeginExternalControl();
            _handoffStartPose.ApplyTo(cameraTransform);
            return true;
        }

        /// <summary>
        ///     背面ショットから通常カメラまでの途中姿勢を反映します。
        /// </summary>
        /// <param name="progress"> 0から1の進行度です。 </param>
        private void ApplyHandoff(float progress)
        {
            if (!_isPrepared
                || _cameraSystemView == null
                || _cameraSystemView.CameraTransform == null)
            {
                return;
            }

            float clampedProgress = Mathf.Clamp01(progress);

            // 位置と回転を線形補間する。
            Vector3 position = Vector3.Lerp(
                _handoffStartPose.Position,
                _handoffPose.Position,
                clampedProgress);

            Quaternion rotation = Quaternion.Slerp(
                _handoffStartPose.Rotation,
                _handoffPose.Rotation,
                clampedProgress);

            // 補間結果を通常カメラへ反映する。
            _cameraSystemView.CameraTransform
                .SetPositionAndRotation(
                    position,
                    rotation);
        }

        /// <summary>
        ///     引き継ぎ先へ姿勢を一致させ、通常カメラへ制御権を戻す。
        /// </summary>
        private void CompleteHandoff()
        {
            if (_cameraSystemView?.CameraTransform != null)
            {
                _handoffPose.ApplyTo(
                    _cameraSystemView.CameraTransform);

                _cameraSystemView.EndExternalControl();
            }

            _isPrepared = false;
            InvokeCompleted();
        }

        /// <summary>
        ///     引き継ぎ姿勢を取得できなかった場合に通常カメラへ制御権を戻す。
        /// </summary>
        private void CompleteWithoutHandoff()
        {
            _cameraSystemView?.EndExternalControl();
            _isPrepared = false;

            InvokeCompleted();
        }

        /// <summary>
        ///     カメラ演出完了処理を一度だけ通知します。
        /// </summary>
        private void InvokeCompleted()
        {
            Action onCompleted = _onCompleted;
            _onCompleted = null;

            onCompleted?.Invoke();
        }

        /// <summary>
        ///     参照の設定が正しいか検証します。
        /// </summary>
        /// <returns> 参照の設定が正しい場合はtrue、それ以外の場合はfalseを返します。 </returns>
        private bool ValidateReferences()
        {
            if (_config == null)
            {
                Debug.LogError(
                    $"[{nameof(StageStartCameraView)}] "
                    + $"{nameof(_config)} が設定されていません。",
                    this);
                return false;
            }

            if (_cameraSystemView == null
                || _cameraSystemView.CameraTransform == null
                || _playerTransform == null)
            {
                Debug.LogError(
                    $"[{nameof(StageStartCameraView)}] "
                    + $"{nameof(_cameraSystemView)} または {nameof(_playerTransform)} が設定されていません。",
                    this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     カメラの位置と回転を保持する構造体。
        /// </summary>
        private readonly struct CameraPose
        {
            /// <summary>
            ///     位置と回転を指定して生成します。
            /// </summary>
            /// <param name="position"> カメラ位置。 </param>
            /// <param name="rotation"> カメラ回転。 </param>
            public CameraPose(
                in Vector3 position,
                in Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }

            /// <summary> カメラ位置です。 </summary>
            public Vector3 Position { get; }

            /// <summary> カメラ回転です。 </summary>
            public Quaternion Rotation { get; }

            /// <summary>
            ///     保持している姿勢をTransformへ反映します。
            /// </summary>
            /// <param name="target"> 姿勢を反映するTransformです。 </param>
            public void ApplyTo(Transform target)
            {
                target.SetPositionAndRotation(Position, Rotation);
            }
        }
    }
}
