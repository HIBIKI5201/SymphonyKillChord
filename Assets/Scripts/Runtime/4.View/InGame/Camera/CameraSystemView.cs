using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.Persistent.Input;
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
        /// <param name="controller"> カメラシステムのコントローラー。</param>
        /// <param name="presenter"> カメラシステムのプレゼンター。</param>
        /// <param name="playerT"> プレイヤーの Transform。</param>
        /// <param name="playerInputView"> プレイヤー入力の View クラス。</param>
        public void Initialize(
            CameraSystemController controller,
            CameraSystemPresenter presenter,
            Transform playerT,
            PlayerInputView playerInputView)
        {
            _controller = controller;
            _presenter = presenter;
            _playerT = playerT;
            _inputView = playerInputView;

#if UNITY_ANDROID
            _inputView.OnMobileLookInput += LookHandler;
#else
            _inputView.OnLookInput += LookHandler;
#endif
            _inputView.OnMoveInput += MoveHandler;
            _inputView.OnLockOnInput += LockOnHandler;
            _inputView.OnAttackInput += OnAttack;
        }

        [SerializeField, Tooltip("カメラの Transform")]
        private Transform _cameraT;
        [SerializeField, Tooltip("カメラ更新タイミングの設定")]
        private UpdateModeEnum _updateMode;
        [SerializeField, Tooltip("カメラの感度")]
        private int _cameraSensitivity = 5;

        private CameraSystemController _controller;
        private CameraSystemPresenter _presenter;
        private PlayerInputView _inputView;
        private Transform _playerT;
        private Vector2 _input;
        private Vector2 _moveInput;

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
            if (_controller == null || _playerT == null) { return; }

            if (_updateMode != UpdateModeEnum.Update)
            { return; }
            Tick(Time.deltaTime);
        }

        /// <summary>
        ///     LateUpdate タイミングでカメラを更新する。
        /// </summary>
        private void LateUpdate()
        {
            if (_controller == null || _playerT == null) { return; }

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
            _inputView.OnMobileLookInput -= LookHandler;
#else
            _inputView.OnLookInput -= LookHandler;
#endif
            _inputView.OnMoveInput -= MoveHandler;
            _inputView.OnLockOnInput -= LockOnHandler;
            _inputView.OnAttackInput -= OnAttack;
        }

        /// <summary>
        ///     視点操作入力を受け取り、入力値を更新する。
        /// </summary>
        /// <param name="context"> 視点操作の入力コンテキスト。</param>
        private void LookHandler(InputContext<Vector2> context)
        {
            _input = context.Value;
        }

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
                _controller.ToggleLockOnState(_playerT.position);
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
                _controller.TryActiveAutoLockOn(_playerT.position);
            }
        }

        /// <summary>
        ///     カメラの追従・回転を計算し、カメラの Transform を更新する。
        /// </summary>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        private void Tick(float deltaTime)
        {
            if (_controller == null || _presenter == null||
                _playerT == null || _cameraT == null) { return; }
            Vector2 input = _input * _cameraSensitivity;
            _input = Vector2.zero;

            _presenter.Update(
                _playerT.position,
                input,
                _moveInput,
                deltaTime,
                out Quaternion rotation,
                out Vector3 position
            );
            _cameraT.SetPositionAndRotation(position, rotation);
        }
    }
}