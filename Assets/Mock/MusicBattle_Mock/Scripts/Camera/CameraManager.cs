using Mock.MusicBattle.Basis;
using System;
using System.Linq;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    /// <summary>
    ///     カメラのマネージャークラス。
    ///     カメラの各モジュールを実行する。
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraManager : MonoBehaviour, IDisposable
    {
        /// <summary>
        ///     カメラを初期化する。
        /// </summary>
        /// <returns> 成功したかどうか </returns>
        public bool Init(
            InputBuffer inputBuffer,
            ILockOnTargetContainer lockOnTargetContainer)
        {
            #region バリデーションチェック
            if (inputBuffer == null)
            {
                Debug.LogError($"{nameof(InputBuffer)} is null");
                return false;
            }
            if (lockOnTargetContainer == null)
            {
                Debug.LogError($"{nameof(ILockOnTargetContainer)} is null");
                return false;
            }
            if (_cameraConfigs == null)
            {
                Debug.LogError($"{nameof(CameraConfigs)} is null");
                return false;
            }
            #endregion

            CinemachineCamera cam = GetComponent<CinemachineCamera>();

            // イベント登録。
            inputBuffer.LookAction.Performed += HandleLookAction;
            inputBuffer.LookAction.Canceled += HandleLookAction;

            inputBuffer.LockOnSelectAction.Performed += HandleLockOnSelectAction;
            inputBuffer.LockOnSelectAction.Canceled += HandleUnlockAction;

            _mover = new(_cameraConfigs, transform, cam.Follow);

            _camera = cam;
            _targetContainer = lockOnTargetContainer;
            _inputBuffer = inputBuffer;

            return true;
        }

        /// <summary>
        ///     アップデートモードを変更する。
        /// </summary>
        /// <param name="mode"></param>
        public void ChangeUpdateMode(CameraUpdateModeEnum mode)
        {
            _mode = mode;
        }

        /// <summary>
        ///     イベント登録解除をする。
        /// </summary>
        public void Dispose()
        {
            if (_inputBuffer != null)
            {
                _inputBuffer.LookAction.Performed -= HandleLookAction;
                _inputBuffer.LookAction.Canceled -= HandleLookAction;

                _inputBuffer.LockOnSelectAction.Started -= HandleLockOnSelectAction;
            }
        }

        [SerializeField]
        private CameraConfigs _cameraConfigs;

        private ILockOnTargetContainer _targetContainer;
        private InputBuffer _inputBuffer;
        private CinemachineCamera _camera;

        private CameraMover _mover;

        private CameraUpdateModeEnum _mode = CameraUpdateModeEnum.Update;
        private int _lockingTargetIndex;
        private bool _isUnlockTarget;
        private CancellationTokenSource _lockOnCts;
        private int _lastSelectDir = 0;

        private void Update()
        {
            if (_mode != CameraUpdateModeEnum.Update) { return; }
            Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_mode != CameraUpdateModeEnum.FixedUpdate) { return; }
            Tick(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            if (_mode != CameraUpdateModeEnum.LateUpdate) { return; }
            Tick(Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            _mover?.OnDrawGizmos();
        }

        /// <summary>
        ///     1フレームごとの更新を行う。
        /// </summary>
        /// <param name="deltaTime"> デルタタイム </param>
        private void Tick(float deltaTime)
        {
            // 移動モジュールを更新。
            _mover?.UpdatePitch(deltaTime);
            _mover?.UpdateYaw(deltaTime);
        }

        /// <summary>
        ///     Lookアクションの入力を受ける。
        /// </summary>
        /// <param name="value"></param>
        private void HandleLookAction(Vector2 value)
        {
            _mover?.RotateCamera(value);
        }

        /// <summary>
        ///     LockOnSelectアクションの入力を受ける。
        /// </summary>
        /// <param name="value"></param>
        private void HandleLockOnSelectAction(float value)
        {
            Transform target = null;
            int axis = Math.Sign(value);

            // 入力が0でなければ、コンテナから選択する。
            if (!_isUnlockTarget && !Mathf.Approximately(value, 0f))
            {
                (target, _lockingTargetIndex) =
                    transform.GetTargetWithAxis(
                    _targetContainer.Targets.ToArray(), axis,
                    _targetContainer.Targets[_lockingTargetIndex]);
            }

            Debug.Log(target == null ? "ロックオン解除" : $"{target.name}をロックオン\n入力値:{value}");
            _mover?.SetLockTarget(target);

            // 同時押しでキャンセルするように。
            CancelLockOn(axis);
        }

        /// <summary>
        ///     LockOnSelectアクションのキャンセルを受ける。
        /// </summary>
        /// <param name="value"></param>
        private void HandleUnlockAction(float value)
        {
            _isUnlockTarget = false;
        }

        /// <summary>
        /// 0.5秒以内に左右が両方入力されるとログを出力する。
        /// </summary>
        private async void CancelLockOn(int dir)
        {
            // 前回と逆方向か？
            bool opposite = (_lastSelectDir != 0) && (_lastSelectDir != dir);

            // 反対方向が来た場合は即判定。
            if (opposite)
            {
                Debug.Log($"ロックオン解除\ndir:{dir}");
                _mover?.SetLockTarget(null);
                _isUnlockTarget = true;

                _lastSelectDir = 0;

                // タイマーをキャンセルしておく。
                CancelCts(_lockOnCts);
                _lockOnCts = null;
                return;
            }

            // 新しく方向を記録。
            _lastSelectDir = dir;

            // 新規タイマー開始。
            CancelCts(_lockOnCts);
            _lockOnCts = new CancellationTokenSource();

            try
            {
                // 待機時間まで逆方向入力が来なければリセット。
                await Awaitable.WaitForSecondsAsync(0.2f, _lockOnCts.Token);
            }
            catch (Exception) { return; }

            // 待機時間内に反対方向入力がなかった。
            _lastSelectDir = 0;

            void CancelCts(CancellationTokenSource cts)
            {
                if (cts != null)
                {
                    cts.Cancel();
                    cts.Dispose();
                }
            }
        }
    }
}

