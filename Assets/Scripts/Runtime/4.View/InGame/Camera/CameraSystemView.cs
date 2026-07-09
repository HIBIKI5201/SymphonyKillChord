using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.Utility.InGame;
using KillChord.Runtime.View.Persistent.Input;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラシステムの挙動を管理するViewクラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]
    public sealed class CameraSystemView : MonoBehaviour
    {
        /// <summary>
        ///     依存オブジェクトを受け取り、カメラシステム View を初期化する。
        /// </summary>
        /// <param name="changeTargetAction"> ターゲット切り替え処理。</param>
        /// <param name="clearTargetAction"> ターゲット解除処理。</param>
        /// <param name="getCurrentTargetPositionFunc"> 現在ターゲット位置の取得処理。</param>
        /// <param name="followCalculator"> 追従移動計算クラス。</param>
        /// <param name="lockOnRotationCalculator"> ロックオン回転計算クラス。</param>
        /// <param name="freeLookRotationCalculator"> フリールック回転計算クラス。</param>
        /// <param name="lookAtRotationCalculator"> カメラ回転計算クラス。</param>
        /// <param name="viewSettings"> View が利用するカメラ設定値。</param>
        /// <param name="playerT"> プレイヤーの Transform。</param>
        /// <param name="playerInputView"> プレイヤー入力の View クラス。</param>
        public void Initialize(
            Action<Vector3, Vector3> changeTargetAction,
            Action clearTargetAction,
            Func<(bool HasTarget, Vector3 TargetPosition)> getCurrentTargetPositionFunc,
            CameraFollowCalculator followCalculator,
            CameraLockOnRotationCalculator lockOnRotationCalculator,
            CameraFreeLookRotationCalculator freeLookRotationCalculator,
            CameraLookAtRotationCalculator lookAtRotationCalculator,
            CameraConfig viewSettings,
            Transform playerT,
            PlayerInputView playerInputView)
        {
            _changeTargetAction = changeTargetAction;
            _clearTargetAction = clearTargetAction;
            _getCurrentTargetPositionFunc = getCurrentTargetPositionFunc;
            _followCalculator = followCalculator;
            _lockOnRotationCalculator = lockOnRotationCalculator;
            _freeLookRotationCalculator = freeLookRotationCalculator;
            _lookAtRotationCalculator = lookAtRotationCalculator;
            _viewSettings = viewSettings;
            _playerT = playerT;
            _inputView = playerInputView;
            _currentDistance = viewSettings.Distance;

#if UNITY_ANDROID
            _inputView.OnMobileLookInput += LookHandlerMobile;
#else
            _inputView.OnLookMouseInput += LookHandlerMouse;
            _inputView.OnLookGamepadInput += LookHandlerGamepad;
#endif
            _inputView.OnMoveInput += MoveHandler;
            _inputView.OnLockOnInput += LockOnHandler;
            _inputView.OnAttackInput += OnAttack;
        }

        /// <summary> カメラ距離の補間速度。 </summary>
        private const float DISTANCE_LERP_SPEED = 4f;

        /// <summary> 障害物衝突時のカメラ最小距離。 </summary>
        private const float MIN_CAMERA_DISTANCE = 0.1f;

        [SerializeField, Tooltip("カメラの Transform")]
        private Transform _cameraT;

        [SerializeField, Tooltip("カメラ更新タイミングの設定")]
        private UpdateModeEnum _updateMode;

        [SerializeField, Tooltip("カメラの感度")]
        private int _mouseLookSensitivity = 5;

        [SerializeField, Tooltip("コントローラーのカメラ感度")]
        private int _gamepadLookSensitivity = 20;

        [SerializeField, Tooltip("モバイルのカメラ感度")]
        private int _mobileLookSensitivity = 10;

        private PlayerInputView _inputView;
        private Transform _playerT;
        private Vector2 _input;
        private Vector2 _moveInput;
        private float _lookSensitivity = 1f;
        private float _currentDistance;
        private Vector3 _cameraCenterOffset;
        private Quaternion _cameraRotation = Quaternion.identity;
        private Quaternion _cameraBoneRotation = Quaternion.identity;
        private CameraConfig _viewSettings;
        private CameraFollowCalculator _followCalculator;
        private CameraLockOnRotationCalculator _lockOnRotationCalculator;
        private CameraFreeLookRotationCalculator _freeLookRotationCalculator;
        private CameraLookAtRotationCalculator _lookAtRotationCalculator;
        private Action<Vector3, Vector3> _changeTargetAction;
        private Action _clearTargetAction;
        private Func<(bool HasTarget, Vector3 TargetPosition)> _getCurrentTargetPositionFunc;
        private CameraLockOnState _lockOnState;

        /// <summary>
        ///     FixedUpdate タイミングでカメラを更新する。
        /// </summary>
        private void FixedUpdate()
        {
            if (_updateMode != UpdateModeEnum.FixedUpdate) { return; }
            Tick(Time.fixedDeltaTime);
        }

        /// <summary>
        ///     Update タイミングでカメラを更新する。
        /// </summary>
        private void Update()
        {
            if (_playerT == null) { return; }

            if (_updateMode != UpdateModeEnum.Update)
            { return; }
            Tick(Time.deltaTime);
        }

        /// <summary>
        ///     LateUpdate タイミングでカメラを更新する。
        /// </summary>
        private void LateUpdate()
        {
            if (_playerT == null) { return; }

            if (_updateMode != UpdateModeEnum.LateUpdate)
            { return; }
            Tick(Time.deltaTime);
        }

        /// <summary>
        ///     入力イベントの購読解除を行う。
        /// </summary>
        private void OnDestroy()
        {
            if (_inputView == null) { return; }

#if UNITY_ANDROID
            _inputView.OnMobileLookInput -= LookHandlerMobile;
#else
            _inputView.OnLookMouseInput -= LookHandlerMouse;
            _inputView.OnLookGamepadInput -= LookHandlerGamepad;
#endif
            _inputView.OnMoveInput -= MoveHandler;
            _inputView.OnLockOnInput -= LockOnHandler;
            _inputView.OnAttackInput -= OnAttack;
        }

        /// <summary>
        ///     視点操作入力を受け取り、入力値を更新する。
        /// </summary>
        /// <param name="context"> 視点操作の入力コンテキスト。</param>
#if UNITY_ANDROID
        private void LookHandlerMobile(InputContext<Vector2> context)
        {
            _input = context.Value * _mobileLookSensitivity;
        }
#else
        private void LookHandlerMouse(InputContext<Vector2> context)
        {
            _input = context.Value * _mouseLookSensitivity;
        }

        /// <summary>
        ///     ゲームパッドの視点操作入力を受け取り、入力値を更新する。
        /// </summary>
        /// <param name="context"> 視点操作の入力コンテキスト。</param>
        private void LookHandlerGamepad(InputContext<Vector2> context)
        {
            _input = context.Value * _gamepadLookSensitivity;
        }
#endif

        /// <summary>
        ///     移動入力を受け取り、入力値を更新する。
        /// </summary>
        /// <param name="context"> 移動操作の入力コンテキスト。</param>
        private void MoveHandler(InputContext<Vector2> context)
        {
            _moveInput = context.Value;
        }

        /// <summary>
        ///     ロックオン入力を受け取り、マニュアルロックオン状態をトグルする。
        /// </summary>
        /// <param name="context"> ロックオン操作の入力コンテキスト。</param>
        private void LockOnHandler(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Started)
            {
                ToggleLockOnState(_playerT.position, GetCurrentForward());
            }
        }

        /// <summary>
        ///     攻撃入力を受け取り、オートロックオンの発動を試みる。
        /// </summary>
        /// <param name="context"> 攻撃操作の入力コンテキスト。</param>
        private void OnAttack(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Started)
            {
                TryActiveAutoLockOn(_playerT.position, GetCurrentForward());
            }
        }

        /// <summary>
        ///     カメラの追従・回転を計算し、カメラの Transform を更新する。
        /// </summary>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        private void Tick(float deltaTime)
        {
            if (_changeTargetAction == null || _clearTargetAction == null || _getCurrentTargetPositionFunc == null
                || _playerT == null || _cameraT == null)
            {
                return;
            }

            CameraUpdateFrame frame = BuildFrame(deltaTime);

            UpdateCameraBone(frame);
            _followCalculator.Update(ref _cameraCenterOffset, frame.Context);

            Quaternion boneTargetRotation = _cameraBoneRotation;
            if (frame.IsLockOn)
            {
                _lockOnRotationCalculator.TryGetTargetRotation(
                    frame.Context.FollowPosition,
                    frame.TargetPosition,
                    _cameraBoneRotation,
                    out boneTargetRotation);
            }

            Vector3 cameraAnchorPosition = frame.Context.FollowPosition + _cameraCenterOffset + _viewSettings.Offset;
            Vector3 cameraDirection = _cameraBoneRotation * Vector3.back;
            float targetDistance = ResolveDistance(cameraAnchorPosition, cameraDirection, _viewSettings.Distance);
            _currentDistance = Mathf.Lerp(Mathf.Min(_currentDistance, targetDistance), targetDistance, deltaTime * DISTANCE_LERP_SPEED);
            Vector3 position = cameraAnchorPosition + cameraDirection * _currentDistance;
            Vector3 cameraPositionForRotation = frame.IsLockOn
                ? cameraAnchorPosition - _cameraCenterOffset + cameraDirection * _currentDistance
                : position;

            _lookAtRotationCalculator.Update(
                frame.IsLockOn,
                ref _cameraRotation,
                boneTargetRotation,
                cameraPositionForRotation,
                frame.Context,
                frame.TargetPosition);

            Quaternion rotation = _cameraBoneRotation * _cameraRotation;
            _cameraT.SetPositionAndRotation(position, rotation);
        }

        /// <summary>
        ///     現在のカメラ前方ベクトルを返す。
        /// </summary>
        /// <returns> 現在のカメラ前方ベクトル。</returns>
        private Vector3 GetCurrentForward()
        {
            return _cameraBoneRotation * _cameraRotation * Vector3.forward;
        }

        /// <summary>
        ///     攻撃時のオートロックオン発動を試みる。
        /// </summary>
        /// <param name="currentPosition"> プレイヤーの現在位置。</param>
        /// <param name="direction"> 現在のカメラ前方方向。</param>
        private void TryActiveAutoLockOn(in Vector3 currentPosition, in Vector3 direction)
        {
            if (_lockOnState == CameraLockOnState.LockOnManual)
            {
                return;
            }

            _lockOnState = CameraLockOnState.LockOnAuto;
            _changeTargetAction.Invoke(currentPosition, direction);
        }

        /// <summary>
        ///     マニュアルロックオン状態をトグルする。
        /// </summary>
        /// <param name="currentPosition"> プレイヤーの現在位置。</param>
        /// <param name="direction"> 現在のカメラ前方方向。</param>
        private void ToggleLockOnState(in Vector3 currentPosition, in Vector3 direction)
        {
            if (!IsLockOn())
            {
                _lockOnState = CameraLockOnState.LockOnManual;
                _changeTargetAction.Invoke(currentPosition, direction);
                return;
            }

            ClearLockOn();
        }

        /// <summary>
        ///     障害物を考慮したカメラ距離を解決する。
        /// </summary>
        /// <param name="cameraAnchorPosition"> カメラ配置の基準位置。</param>
        /// <param name="direction"> カメラ配置方向。</param>
        /// <param name="defaultDistance"> 通常時のカメラ距離。</param>
        /// <returns> 解決したカメラ距離。</returns>
        private float ResolveDistance(in Vector3 cameraAnchorPosition, in Vector3 direction, float defaultDistance)
        {
            if (Physics.SphereCast(cameraAnchorPosition, _viewSettings.CollisionRadius, direction, out RaycastHit hit, defaultDistance, _viewSettings.CollisionMask))
            {
                return Mathf.Max(MIN_CAMERA_DISTANCE, hit.distance);
            }

            return defaultDistance;
        }

        /// <summary>
        ///     ロックオン状態に応じてカメラボーンの回転を更新する。
        /// </summary>
        /// <param name="frame"> 1フレーム分のカメラ状態。</param>
        private void UpdateCameraBone(in CameraUpdateFrame frame)
        {
            if (frame.IsLockOn)
            {
                _lockOnRotationCalculator.Update(ref _cameraBoneRotation, frame.Context, frame.TargetPosition);
            }
            else
            {
                _freeLookRotationCalculator.Update(ref _cameraBoneRotation, frame.Context);
            }
        }

        /// <summary>
        ///     今フレームの計算状態を構築する。
        /// </summary>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        /// <returns> 1フレーム分の計算状態。</returns>
        private CameraUpdateFrame BuildFrame(float deltaTime)
        {
            Vector2 input = ApplyInvert(_input * _lookSensitivity);
            CameraUpdateContext context = new(_playerT.position, input, _moveInput, deltaTime);

            if (_lockOnState == CameraLockOnState.LockOnAuto
                && (context.Input.sqrMagnitude > float.Epsilon || context.MoveInput.sqrMagnitude > float.Epsilon))
            {
                ClearLockOn();
            }

            Vector3 targetPosition = Vector3.zero;
            if (IsLockOn())
            {
                var targetResult = _getCurrentTargetPositionFunc.Invoke();
                if (!targetResult.HasTarget)
                {
                    ClearLockOn();
                }
                else
                {
                    targetPosition = targetResult.TargetPosition;
                }
            }

            return new CameraUpdateFrame(context, targetPosition, IsLockOn());
        }

        /// <summary>
        ///     ロックオン状態を解除する。
        /// </summary>
        private void ClearLockOn()
        {
            _lockOnState = CameraLockOnState.Free;
            _clearTargetAction.Invoke();
        }

        /// <summary>
        ///     現在ロックオン状態であるかを返す。
        /// </summary>
        /// <returns> ロックオン中の場合は true。</returns>
        private bool IsLockOn()
        {
            return _lockOnState != CameraLockOnState.Free;
        }

        /// <summary>
        ///     設定に基づき入力の垂直・水平反転を適用する。
        /// </summary>
        /// <param name="input"> 反転前の入力値。</param>
        /// <returns> 反転処理後の入力値。</returns>
        private Vector2 ApplyInvert(Vector2 input)
        {
            if (_viewSettings.IsInvertVertical)
            {
                input.y = -input.y;
            }

            if (_viewSettings.IsInvertHorizontal)
            {
                input.x = -input.x;
            }

            return input;
        }

        /// <summary>
        ///     カメラのロックオン状態を表す enum。
        /// </summary>
        private enum CameraLockOnState : byte
        {
            Free,
            LockOnAuto,
            LockOnManual,
        }
    }
}
