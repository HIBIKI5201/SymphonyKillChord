using Mock.MusicBattle.MusicSync;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    /// <summary>
    ///     プレイヤーの移動処理を管理するクラス。
    /// </summary>
    public class PlayerMover
    {
        #region コンストラクタ
        /// <summary>
        ///     <see cref="PlayerMover"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="status">プレイヤーのステータス。</param>
        /// <param name="rb">プレイヤーのRigidbody。</param>
        /// <param name="player">プレイヤーのTransform。</param>
        /// <param name="camera">カメラのTransform。</param>
        public PlayerMover(PlayerStatus status,
            Rigidbody rb, Transform player, Transform camera,
            MusicSyncManager musicSyncManager)
        {
            _status = status;
            _player = player;
            _camera = camera;
            _rb = rb;
            _musicSync = musicSyncManager;
        }
        #endregion

        #region パブリックプロパティ
        /// <summary> 現在の速度。 </summary>
        public Vector3 CurrentVelocity => _currentVelocity;
        /// <summary> 回避中かどうか。 </summary>
        public bool IsDodging => _isDodging;
        /// <summary> 入力ロックが有効か。 </summary>
        public bool IsInputLock => 0 < _inputLockTasks.Count;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     入力方向からプレイヤーの速度を計算します。
        /// </summary>
        /// <param name="inputDirection">入力方向のVector2。</param>
        /// <returns>計算されたプレイヤーの速度ベクトル。</returns>
        public Vector3 CalcPlayerVelocityByInputDirection(Vector2 inputDirection)
        {
            // カメラの向きを基準に移動方向を計算。
            Vector3 cameraForward = (_player.position - _camera.position).normalized;
            cameraForward.y = 0f;
            // カメラの右方向を計算。
            Vector3 cameraRight = new Vector3(cameraForward.z, 0f, -cameraForward.x);
            // 入力方向に基づいて移動ベクトルを計算。
            Vector3 moveDir = cameraForward * inputDirection.y + cameraRight * inputDirection.x;
            moveDir.y = 0f;

            return moveDir.normalized * _status.MoveSpeed;
        }

        /// <summary>
        ///     プレイヤーの目標速度を設定します。
        /// </summary>
        /// <param name="velocity">設定する目標速度。</param>
        public void SetPlayerVelocity(Vector3 velocity)
        {
            if (IsInputLock) { return; }

            _targetVelocity = velocity;
        }

        public void VelocityReset()
        {
            _currentVelocity = Vector3.zero;
            _targetVelocity = Vector3.zero;
        }

        /// <summary>
        ///     プレイヤーが地面に接地している状態を設定します。
        /// </summary>
        /// <param name="isGround">地面に接地している場合はtrue、それ以外はfalse。</param>
        public void SetIsGround(bool isGround)
        {
            _isGround = isGround;
        }

        public async Task Dodge(CancellationToken toknen = default)
        {
            if (!_isGround) { return; }

            Vector3 dir = _player.transform.forward;
            Vector3 cul = _currentVelocity;
            Vector3 tar = _targetVelocity;

            Vector3 vel = dir * _status.DodgeSpeed;
            _currentVelocity = vel;
            _targetVelocity = vel;

            _isDodging = true;

            await Awaitable.WaitForSecondsAsync(
                (float)_status.DodgeDuration.GetLength(_musicSync.MusicBuffer), toknen);

            _isDodging = false;

            _currentVelocity = cul;
            _targetVelocity = tar;
        }

        public async void InputLock(Task moveLockTask)
        {
            if (moveLockTask  == null) { return; }

            _inputLockTasks.AddLast(moveLockTask);
            await moveLockTask;
            _inputLockTasks.Remove(moveLockTask);
        }

        /// <summary>
        ///     プレイヤーの移動を更新します（Updateフェーズで呼び出し）。
        /// </summary>
        public void Update(float delta)
        { 
            float t = CalculateAccelerationLerpT(delta);
            UpdateHorizontalVelocity(t);
            UpdateRotation();
        }
        #endregion

        #region プライベートフィールド
        /// <summary> プレイヤーのステータス。 </summary>
        private readonly PlayerStatus _status;
        /// <summary> プレイヤーのTransform。 </summary>
        private readonly Transform _player;
        /// <summary> カメラのTransform。 </summary>
        private readonly Transform _camera;
        /// <summary> プレイヤーのRigidbody。 </summary>
        private readonly Rigidbody _rb;
        /// <summary> 音楽同期システム。 </summary>
        private readonly MusicSyncManager _musicSync;
        /// <summary> 現在の速度。 </summary>
        private Vector3 _currentVelocity;
        /// <summary> 目標の速度。 </summary>
        private Vector3 _targetVelocity;
        /// <summary> 水平方向の速度。 </summary>
        private Vector3 _horizontalVelocity;
        /// <summary> 地面に接地しているかどうか。 </summary>
        private bool _isGround;
        /// <summary> 回避中かどうか。 </summary>
        private bool _isDodging;
        /// <summary> 入力ロックされているかどうか。 </summary>
        private LinkedList<Task> _inputLockTasks = new();
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     固定フレームレートで呼び出されます。
        ///     Rigidbodyの線形速度を更新します。
        /// </summary>
        public void FixedUpdate()
        {
            _rb.linearVelocity = new Vector3(_currentVelocity.x, _rb.linearVelocity.y, _currentVelocity.z);
        }
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     加速度補間t値を計算します。
        /// </summary>
        /// <returns>加速度補間t値。</returns>
        private float CalculateAccelerationLerpT(float delta)
        {
            float acceleration = _targetVelocity.magnitude > 0f
                ? _status.WalkAccelerationDuration
                : _status.StopAccelerationDuration;

            float damping = Mathf.Max(acceleration, 0.0001f);
            return 1f - Mathf.Exp(-delta / damping);
        }

        /// <summary>
        ///   水平方向の速度ベクトルを更新します。
        /// </summary>
        /// <param name="t">補間係数。</param>
        private void UpdateHorizontalVelocity(float t)
        {
            Vector3 horizontalCurrent = new Vector3(_currentVelocity.x, 0f, _currentVelocity.z);
            Vector3 horizontalTarget = new Vector3(_targetVelocity.x, 0f, _targetVelocity.z);

            _horizontalVelocity = Vector3.Lerp(horizontalCurrent, horizontalTarget, t);

            _currentVelocity = new Vector3(
                _horizontalVelocity.x,
                _currentVelocity.y,
                _horizontalVelocity.z
            );
        }

        /// <summary>
        ///   プレイヤーの回転を更新します。
        /// </summary>
        private void UpdateRotation()
        {
            // 現在の向きを水平に
            Vector3 forward = _player.forward - new Vector3(0f, _player.forward.y, 0f);
            // 水平方向の速度成分
            Vector3 targetDir = new Vector3(_currentVelocity.x, 0f, _currentVelocity.z);
            // Cinemachine式 Damping
            float rotDamping = Mathf.Max(_status.RotationDamping, 0.0001f);
            float rotT = 1f - Mathf.Exp(-Time.deltaTime / rotDamping);
            Vector3 dir = Vector3.Lerp(forward, targetDir, rotT);
            if (dir.sqrMagnitude > 0.0001f)
            {
                _player.LookAt(_player.position + dir.normalized);
            }
        }
        #endregion
    }
}

