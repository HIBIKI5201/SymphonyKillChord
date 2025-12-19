using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Character;
using Mock.MusicBattle.Enemy;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle.Battle
{
    /// <summary>
    ///     ロックオンの管理を行うクラス。
    /// </summary>
    public class LockOnManager : IDisposable
    {
        /// <summary>
        ///     <see cref="LockOnManager"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="player">プレイヤーのTransform。</param>
        /// <param name="container">ロックオン可能なターゲットのコンテナ。</param>
        /// <param name="inputBuffer">入力バッファ。</param>
        /// <param name="unlockWaitingTime">ロックオン解除までの待機時間。</param>
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

        #region Publicイベント
        /// <summary> ロックオンターゲットが変更されたときに発火するイベント。 </summary>
        public event Action<Transform> OnTargetLocked;
        #endregion

        #region パブリックプロパティ
        /// <summary> 現在ロックオン中の敵キャラクターを取得します。 </summary>
        public ICharacter LockOnTarget => _currentEnemy;
        #endregion

        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        #region Publicメソッド
        /// <summary>
        ///     現在のロックオンターゲットを変更します。
        /// </summary>
        /// <param name="enemy">新しくロックオンする敵キャラクター。</param>
        public void ChangeCurrentEnemy(EnemyManager enemy)
        {
            if (enemy == null)
            {
                _lockingTargetIndex = 0;
                OnTargetLocked?.Invoke(null);
                _currentEnemy = null;
                return;
            }
            _currentEnemy = enemy;
            OnTargetLocked?.Invoke(enemy.transform);
        }
        #endregion

        #region パブリックインターフェースメソッド
        /// <summary>
        ///     このインスタンスによって使用されているリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            UnregisterInput(_inputBuffer);

            _lockOnCts?.Cancel();
            _lockOnCts?.Dispose(); // nullチェックを追加
        }
        #endregion

        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> プレイヤーのTransform。 </summary>
        private readonly Transform _player;
        /// <summary> ロックオン可能なターゲットのコンテナ。 </summary>
        private readonly ILockOnTargetContainer _targetContainer;
        /// <summary> ロックオン解除までの待機時間。 </summary>
        private readonly float _unlockWaitingTime;
        /// <summary> 入力バッファ。 </summary>
        private readonly InputBuffer _inputBuffer;
        /// <summary> 現在ロックオン中の敵キャラクター。 </summary>
        private EnemyManager _currentEnemy;
        /// <summary> 現在ロックオン中のターゲットのインデックス。 </summary>
        private int _lockingTargetIndex;
        /// <summary> ターゲットがロックオン解除状態かどうかを示すフラグ。 </summary>
        private bool _isUnlockTarget;
        /// <summary> ロックオンキャンセル用のCancellationTokenSource。 </summary>
        private CancellationTokenSource _lockOnCts;
        /// <summary> 最後に選択された方向（-1:左, 1:右, 0:なし）。 </summary>
        private int _lastSelectDir = 0;
        #endregion

        // UNITY_LIFECYCLE_METHODS
        #region イベントハンドラメソッド
        /// <summary>
        ///     ロックオン選択アクションの入力時に呼び出されます。
        /// </summary>
        /// <param name="value">入力値。</param>
        private void HandleLockOnSelectAction(float value)
        {
            Transform target = null;
            int axis = Math.Sign(value);

            // 入力が0でなければ、コンテナから選択する。
            if (!_isUnlockTarget && !Mathf.Approximately(value, 0f))
            {
                (target, _lockingTargetIndex) =
                    GetTargetWithAxis(_player,
                        _targetContainer.NearerTargets.ToArray(), axis,
                        _targetContainer[_lockingTargetIndex]);
            }

            if (target != null)
            {
                if (target.TryGetComponent<EnemyManager>(out _currentEnemy))
                {
                    _currentEnemy.SetLockOn(target);
                }
            }

            Debug.Log($"{(target == null ? "ロックオン解除" : $"{target.name}をロックオン")}しました。\n入力値:{value}");
            OnTargetLocked?.Invoke(target);

            // 同時押しでキャンセルするように。
            CancelLockOn(axis);
        }

        /// <summary>
        ///     ロックオン解除アクションの入力時に呼び出されます。
        /// </summary>
        /// <param name="value">入力値。</param>
        private void HandleUnlockAction(float value)
        {
            _isUnlockTarget = false;
        }
        #endregion

        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        #region Privateメソッド
        /// <summary>
        ///     入力イベントを登録します。
        /// </summary>
        /// <param name="inputBuffer">入力バッファ。</param>
        private void RegisterInput(InputBuffer inputBuffer)
        {
            if (inputBuffer != null)
            {
                InputActionEntity<float> lockOnSelectAction = inputBuffer.LockOnSelectAction;
                lockOnSelectAction.Performed += HandleLockOnSelectAction;
                lockOnSelectAction.Canceled += HandleUnlockAction;
            }
        }

        /// <summary>
        ///     入力イベントの登録を解除します。
        /// </summary>
        /// <param name="inputBuffer">入力バッファ。</param>
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
        ///     待機時間以内に左右が両方入力されるとロックオンを解除します。
        /// </summary>
        /// <param name="dir">現在の入力方向。</param>
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

        /// <summary>
        ///     カメラからの相対的な軸に基づいてターゲットを取得します。
        /// </summary>
        /// <param name="camera">カメラのTransform。</param>
        /// <param name="targets">検索対象のTransform配列。</param>
        /// <param name="axis">軸の方向（-1:左, 1:右）。</param>
        /// <param name="ignore">除外するTransform。</param>
        /// <returns>最も近いターゲットとそのインデックス。</returns>
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
        #endregion
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
