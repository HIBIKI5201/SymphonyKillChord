using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Enemy;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle.Battle
{
    public class LockOnManager : IDisposable
    {
        public LockOnManager(
            Transform player, ILockOnTargetContainer container,
            InputBuffer inputBuffer, float unlockWaitingTime = 0.3f)
        {
            _player = player;
            _targetContainer = container;
            _unlockWaitingTime = unlockWaitingTime;
            _inputBuffer = inputBuffer;

            RegisterInput(inputBuffer);
        }

        public event Action<Transform> OnTargetLocked;

        public void Dispose()
        {
            UnregisterInput(_inputBuffer);

            _lockOnCts?.Cancel();
            _lockOnCts.Dispose();
        }

        private readonly Transform _player;
        private readonly ILockOnTargetContainer _targetContainer;
        private readonly float _unlockWaitingTime;
        private readonly InputBuffer _inputBuffer;
        private EnemyManager _currentEnemy; 

        private int _lockingTargetIndex;
        private bool _isUnlockTarget;
        private CancellationTokenSource _lockOnCts;
        private int _lastSelectDir = 0;

        private void HandleLockOnSelectAction(float value)
        {
            Transform target = null;
            int axis = Math.Sign(value);

            // 入力が0でなければ、コンテナから選択する。
            if (!_isUnlockTarget && !Mathf.Approximately(value, 0f))
            {
                (target, _lockingTargetIndex) =
                    GetTargetWithAxis(_player,
                        _targetContainer.Targets.ToArray(), axis,
                        _targetContainer[_lockingTargetIndex]);
            }

            if (target != null)
            {
                if (target.TryGetComponent<EnemyManager>(out var enemy))
                {
                    _currentEnemy = enemy;
                    _currentEnemy.SetLockOn(target);
                }
                else
                {
                    _currentEnemy = null;
                }
            }


            Debug.Log($"{(target == null ? "ロックオン解除" : $"{target.name}をロックオン")}\n入力値:{value}");
            OnTargetLocked?.Invoke(target);

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

        private void RegisterInput(InputBuffer inputBuffer)
        {
            if (inputBuffer != null)
            {
                InputActionEntity<float> lockOnSelectAction = inputBuffer.LockOnSelectAction;
                lockOnSelectAction.Performed += HandleLockOnSelectAction;
                lockOnSelectAction.Canceled += HandleUnlockAction;
            }
        }

        private void UnregisterInput(InputBuffer inputBuffer)
        {
            if (inputBuffer != null)
            {
                InputActionEntity<float> lockOnSelectAction = inputBuffer.LockOnSelectAction;
                lockOnSelectAction.Performed -= HandleLockOnSelectAction;
                lockOnSelectAction.Canceled -= HandleUnlockAction;
            }
        }

        /// <summary>
        /// 待機時間以内に左右が両方入力されるとロックオンを解除する。
        /// </summary>
        private async void CancelLockOn(int dir)
        {
            if (_isUnlockTarget) { return; }

            // 前回と逆方向か？
            bool opposite = (_lastSelectDir != 0) && (_lastSelectDir != dir);

            // 反対方向が来た場合は即判定。
            if (opposite)
            {
                Debug.Log($"ロックオン解除\ndir:{dir}");
                OnTargetLocked?.Invoke(null);
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
                await Awaitable.WaitForSecondsAsync(_unlockWaitingTime, _lockOnCts.Token);
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

        private (Transform transform, int index) GetTargetWithAxis(Transform camera,
            Transform[] targets, int axis,
            params Transform[] ignore)
        {
            Vector3 forward = camera.forward;
            Vector3 up = camera.up;

            float minAngle = float.MaxValue;
            int index = -1;
            Transform closest = null;

            for (int i = 0; i < targets.Length; i++)
            {
                Transform t = targets[i];
                Vector3 dir = (t.position - camera.position).normalized;

                float signed = Vector3.SignedAngle(forward, dir, up);

                float angle = axis < 0f ?
                    signed >= 0 ? 360f - signed : -signed : // 右（時計回り）
                    signed >= 0 ? signed : 360f + signed; // 左（反時計回り）

                if (angle < minAngle &&
                    !ignore.Contains(t)) // 除外リストに含まれていない時。
                {
                    minAngle = angle;
                    closest = t;
                    index = i;
                }
            }

            return (closest, index);
        }
    }
}
