using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.Utility.InGame;
using KillChord.Runtime.Utility.Persistent;
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
        /// <summary> 初回カメラ更新が完了している場合はtrueです。 </summary>
        public bool HasCompletedInitialUpdate => _hasCompletedInitialUpdate;

        /// <summary> カメラの Transform。 </summary>
        public Transform CameraTransform => _cameraT;

        /// <summary> 外部から制御されている場合はtrueです。 </summary>
        public bool IsExternallyControlled => _isExternallyControlled;

        /// <summary>
        ///     依存オブジェクトを受け取り、カメラシステム View を初期化する。
        /// </summary>
        /// <param name="changeTargetAction"> ターゲット切り替え処理。</param>
        /// <param name="clearTargetAction"> ターゲット解除処理。</param>
        /// <param name="getCurrentTargetPositionFunc"> 現在ターゲット位置の取得処理。</param>
        /// <param name="updateCandidateAction"> 候補ターゲット更新処理。</param>
        /// <param name="trySwitchTargetFunc"> 別ターゲットへの切り替え処理。</param>
        /// <param name="trySetTargetByIdFunc"> 指定IDのターゲットを現在ターゲットへ設定する処理。</param>
        /// <param name="followCalculator"> 追従移動計算クラス。</param>
        /// <param name="lockOnRotationCalculator"> ロックオン回転計算クラス。</param>
        /// <param name="freeLookRotationCalculator"> フリールック回転計算クラス。</param>
        /// <param name="lookAtRotationCalculator"> カメラ回転計算クラス。</param>
        /// <param name="lockOnRangeChecker"> 自動ロックオン対象の視野内判定クラス。</param>
        /// <param name="lockOnBreakTracker"> 強い視点操作によるロックオン解除判定クラス。</param>
        /// <param name="viewSettings"> View が利用するカメラ設定値。</param>
        /// <param name="playerT"> プレイヤーの Transform。</param>
        /// <param name="playerInputView"> プレイヤー入力の View クラス。</param>
        public void Initialize(
            Action<Vector3, Vector3> changeTargetAction,
            Action clearTargetAction,
            Func<(bool HasTarget, Vector3 TargetPosition)> getCurrentTargetPositionFunc,
            Action<Vector3, Vector3> updateCandidateAction,
            Func<Vector3, Vector3, bool> trySwitchTargetFunc,
            Func<Guid, bool> trySetTargetByIdFunc,
            CameraFollowCalculator followCalculator,
            CameraLockOnRotationCalculator lockOnRotationCalculator,
            CameraFreeLookRotationCalculator freeLookRotationCalculator,
            CameraLookAtRotationCalculator lookAtRotationCalculator,
            CameraLockOnRangeChecker lockOnRangeChecker,
            CameraLockOnBreakTracker lockOnBreakTracker,
            CameraConfig viewSettings,
            Transform playerT,
            PlayerInputView playerInputView)
        {
            _changeTargetAction = changeTargetAction;
            _clearTargetAction = clearTargetAction;
            _getCurrentTargetPositionFunc = getCurrentTargetPositionFunc;
            _updateCandidateAction = updateCandidateAction;
            _trySwitchTargetFunc = trySwitchTargetFunc;
            _trySetTargetByIdFunc = trySetTargetByIdFunc;
            _followCalculator = followCalculator;
            _lockOnRotationCalculator = lockOnRotationCalculator;
            _freeLookRotationCalculator = freeLookRotationCalculator;
            _lookAtRotationCalculator = lookAtRotationCalculator;
            _lockOnRangeChecker = lockOnRangeChecker;
            _lockOnBreakTracker = lockOnBreakTracker;
            _viewSettings = viewSettings;
            _playerT = playerT;
            _inputView = playerInputView;
            _camera = _cameraT != null
                ? _cameraT.GetComponent<UnityEngine.Camera>() ?? UnityEngine.Camera.main
                : UnityEngine.Camera.main;
            _currentDistance = viewSettings.Distance;
            _hasCompletedInitialUpdate = false;
            _isExternallyControlled = false;

#if UNITY_ANDROID
            _inputView.OnMobileLookInput += LookHandlerMobile;
            _inputView.OnMobileLockOnSelectInput += LockOnSelectHandlerMobile;
#else
            _inputView.OnLookMouseInput += LookHandlerMouse;
            _inputView.OnLookGamepadInput += LookHandlerGamepad;
            _inputView.OnLockOnSelectInput += LockOnSelectHandler;
#endif
            _inputView.OnMoveInput += MoveHandler;
            _inputView.OnLockOnInput += LockOnHandler;
            _inputView.OnAttackInput += OnAttack;
            EventBus<EOnTakeDamage>.Register(OnTakeDamage);
        }

        /// <summary>
        ///     現在のプレイヤー位置・カメラ前方方向を基に、カメラの状態を即時更新する。
        /// </summary>
        /// <returns> 更新が成功したかどうかを示す値。 </returns>
        public bool RefreshImmediate()
        {
            if (_isExternallyControlled || _playerT == null || _cameraT == null || _viewSettings == null)
            {
                return false;
            }

            Tick(0f);
            return _hasCompletedInitialUpdate;
        }

        /// <summary>
        ///     ステージ演出などへカメラTransformの制御を委譲するため、外部制御モードへ切り替える。
        /// </summary>
        public void BeginExternalControl()
        {
            ClearInputState();
            _isExternallyControlled = true;
        }

        /// <summary>
        ///     外部制御モードを終了し、カメラシステムの制御へ戻す。
        /// </summary>
        public void EndExternalControl()
        {
            ClearInputState();
            _isExternallyControlled = false;
        }

        /// <summary>
        ///     カメラの向きを指定した前方へ向け直し、ロックオンや入力状態を初期化する。
        ///     プレイヤーをスタート地点へ戻す際などに、カメラの向きも合わせて戻すために使用する。
        /// </summary>
        /// <param name="forward"> カメラを向けたい前方(ワールド空間)。プレイヤーのスタート時の前方などを指定する。</param>
        public void ResetOrientation(Vector3 forward)
        {
            // ロックオン中の場合は解除する。
            if (IsLockOn())
            {
                ClearLockOn();
            }

            ClearInputState();

            // 水平成分からヨーを求め、ピッチは水平(0)へ戻す。
            Vector3 flatForward = forward;
            flatForward.y = 0f;
            if (flatForward.sqrMagnitude > float.Epsilon)
            {
                float yaw = Quaternion.LookRotation(flatForward.normalized, Vector3.up).eulerAngles.y;
                _cameraBoneRotation = Quaternion.Euler(0f, yaw, 0f);
            }
            else
            {
                _cameraBoneRotation = Quaternion.identity;
            }

            _cameraRotation = Quaternion.identity;

            if (_viewSettings != null)
            {
                _currentDistance = _viewSettings.Distance;
            }

            // 反映を即時に行い、カメラ位置・回転を更新する。
            RefreshImmediate();
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
        private UnityEngine.Camera _camera;
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
        private CameraLockOnRangeChecker _lockOnRangeChecker;
        private CameraLockOnBreakTracker _lockOnBreakTracker;
        private Action<Vector3, Vector3> _changeTargetAction;
        private Action<Vector3, Vector3> _updateCandidateAction;
        private Action _clearTargetAction;
        private Func<(bool HasTarget, Vector3 TargetPosition)> _getCurrentTargetPositionFunc;
        private Func<Vector3, Vector3, bool> _trySwitchTargetFunc;
        private Func<Guid, bool> _trySetTargetByIdFunc;
        private CameraLockOnState _lockOnState;
        private bool _hasCompletedInitialUpdate;
        private float _autoLockOnIdleTimer;
        private float _autoLockOnViewportGraceTimer;
        private bool _isExternallyControlled;

        /// <summary>
        ///     FixedUpdate タイミングでカメラを更新する。
        /// </summary>
        private void FixedUpdate()
        {
            if (_isExternallyControlled || _playerT == null) { return; }
            if (_updateMode != UpdateModeEnum.FixedUpdate) { return; }

            Tick(Time.fixedDeltaTime);
        }

        /// <summary>
        ///     Update タイミングでカメラを更新する。
        /// </summary>
        private void Update()
        {
            if (_isExternallyControlled || _playerT == null) { return; }

            if (_updateMode != UpdateModeEnum.Update)
            { return; }
            Tick(Time.deltaTime);
        }

        /// <summary>
        ///     LateUpdate タイミングでカメラを更新する。
        /// </summary>
        private void LateUpdate()
        {
            if (_isExternallyControlled || _playerT == null) { return; }

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
            _inputView.OnMobileLockOnSelectInput -= LockOnSelectHandlerMobile;
#else
            _inputView.OnLookMouseInput -= LookHandlerMouse;
            _inputView.OnLookGamepadInput -= LookHandlerGamepad;
            _inputView.OnLockOnSelectInput -= LockOnSelectHandler;
#endif
            _inputView.OnMoveInput -= MoveHandler;
            _inputView.OnLockOnInput -= LockOnHandler;
            _inputView.OnAttackInput -= OnAttack;
            EventBus<EOnTakeDamage>.Unregister(OnTakeDamage);
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

        /// <summary>
        ///     モバイルのロックオン対象切り替え入力を受け取り、手動ロックオン対象の切り替えを試みる。
        /// </summary>
        /// <param name="direction"> 左右方向を表す入力値。 </param>
        private void LockOnSelectHandlerMobile(float direction)
        {
            TrySelectAdjacentTarget(direction);
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

        /// <summary>
        ///     ロックオン対象切り替え入力を受け取り、手動ロックオン対象の切り替えを試みる。
        /// </summary>
        /// <param name="context"> ロックオン対象切り替えの入力コンテキスト。</param>
        private void LockOnSelectHandler(InputContext<float> context)
        {
            if (context.Phase != InputActionPhase.Started)
            {
                return;
            }

            TrySelectAdjacentTarget(context.Value);
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
        ///     プレイヤーの攻撃命中イベントを受け取り、オートロックオン解除タイマーを延長する。
        /// </summary>
        /// <param name="eventData"> ダメージイベント。 </param>
        private void OnTakeDamage(EOnTakeDamage eventData)
        {
            if (_lockOnState == CameraLockOnState.LockOnManual)
            {
                return;
            }

            if (_trySetTargetByIdFunc == null || !_trySetTargetByIdFunc.Invoke(eventData.DefenderId))
            {
                return;
            }

            _lockOnState = CameraLockOnState.LockOnAuto;
            _lockOnBreakTracker.Reset();
            _autoLockOnIdleTimer = 0f;
            _autoLockOnViewportGraceTimer = _viewSettings.AutoLockOnViewportGraceDuration;
        }

        /// <summary>
        ///     カメラの追従・回転を計算し、カメラの Transform を更新する。
        /// </summary>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        private void Tick(float deltaTime)
        {
            if (_changeTargetAction == null || _clearTargetAction == null || _getCurrentTargetPositionFunc == null
                || _updateCandidateAction == null || _trySetTargetByIdFunc == null || _followCalculator == null || _lockOnRotationCalculator == null
                || _freeLookRotationCalculator == null || _lookAtRotationCalculator == null
                || _lockOnRangeChecker == null || _lockOnBreakTracker == null
                || _playerT == null || _cameraT == null)
            {
                return;
            }

            if (_camera == null)
            {
                _camera = UnityEngine.Camera.main;
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
            _hasCompletedInitialUpdate = true;
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
            _lockOnBreakTracker.Reset();
            _autoLockOnIdleTimer = 0f;
            _autoLockOnViewportGraceTimer = 0f;
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
                _lockOnBreakTracker.Reset();
                _autoLockOnIdleTimer = 0f;
                _autoLockOnViewportGraceTimer = 0f;
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
                _lockOnRotationCalculator.Update(
                    ref _cameraBoneRotation,
                    frame.Context,
                    frame.TargetPosition);
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
            CameraUpdateContext context = new(_playerT.position, _playerT.forward, input, _moveInput, deltaTime);

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
                    if (_lockOnState == CameraLockOnState.LockOnAuto)
                    {
                        _autoLockOnIdleTimer += deltaTime;
                        _autoLockOnViewportGraceTimer = Mathf.Max(0f, _autoLockOnViewportGraceTimer - deltaTime);
                        if (ShouldClearAutoLockOn(context, targetPosition))
                        {
                            ClearLockOn();
                            targetPosition = Vector3.zero;
                        }
                    }
                }
            }

            if (!IsLockOn())
            {
                _updateCandidateAction.Invoke(_playerT.position, GetCurrentForward());
            }

            return new CameraUpdateFrame(context, targetPosition, IsLockOn());
        }

        /// <summary>
        ///     ロックオン状態を解除する。
        /// </summary>
        private void ClearLockOn()
        {
            _lockOnState = CameraLockOnState.Free;
            _lockOnBreakTracker.Reset();
            _autoLockOnIdleTimer = 0f;
            _autoLockOnViewportGraceTimer = 0f;
            _clearTargetAction.Invoke();
        }

        /// <summary>
        ///     ロックオン対象の左右切り替えを試みる。
        /// </summary>
        /// <param name="direction"> 左右方向を表す入力値。 </param>
        private void TrySelectAdjacentTarget(float direction)
        {
            if (_lockOnState != CameraLockOnState.LockOnManual
                || _trySwitchTargetFunc == null
                || _playerT == null
                || _cameraT == null)
            {
                return;
            }

            float selectDirection = Mathf.Sign(direction);
            if (Mathf.Approximately(selectDirection, 0f))
            {
                return;
            }

            Vector3 candidateDirection = GetCurrentForward() + (_cameraT.right * selectDirection);
            if (candidateDirection.sqrMagnitude <= float.Epsilon)
            {
                return;
            }

            _trySwitchTargetFunc.Invoke(_playerT.position, candidateDirection.normalized);
        }

        /// <summary>
        ///     オートロックオンを解除するべきかを判定する。
        /// </summary>
        /// <param name="context"> 今フレームの更新コンテキスト。 </param>
        /// <param name="targetPosition"> 現在のロックオン対象座標。 </param>
        /// <returns> 解除するべき場合は true。 </returns>
        private bool ShouldClearAutoLockOn(in CameraUpdateContext context, in Vector3 targetPosition)
        {
            if (_autoLockOnIdleTimer >= _viewSettings.AutoLockOnReleaseDelay)
            {
                return true;
            }

            if (_autoLockOnViewportGraceTimer <= 0f
                && _camera != null
                && !_lockOnRangeChecker.IsWithinRange(_camera, targetPosition))
            {
                return true;
            }

            return _lockOnBreakTracker.Update(context);
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
        ///     カメラ操作に使用する入力値を初期化します。
        /// </summary>
        private void ClearInputState()
        {
            _input = Vector2.zero;
            _moveInput = Vector2.zero;
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
