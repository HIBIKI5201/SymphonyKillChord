using KillChord.Runtime.Domain.InGame.Player;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Player
{
    /// <summary>
    ///     プレイヤーの回避行動の移動計算を担当するクラス。
    /// </summary>
    public sealed class PlayerDodgeMovementApplication
    {
        public PlayerDodgeMovementApplication(PlayerMoveParameter parameter)
        {
            _parameter = parameter;

            _previousDodgedTime = float.NegativeInfinity;
        }

        /// <summary>
        ///     回避開始時のイベント。
        /// </summary>
        public Action<float> OnDodgeStarted;
        /// <summary>
        ///     回避終了時のイベント。
        /// </summary>
        public Action OnDodgeEnded;

        public bool IsDodging => _isDodging;

        /// <summary>
        ///     回避を試みる。
        ///     入力がない場合やクールダウン中の場合は失敗する。
        /// </summary>
        /// <param name="input">移動入力方向（XZ平面）。</param>
        /// <param name="currentTime">現在の時刻（Time.time）。</param>
        /// <returns>回避に成功した場合は true 、失敗した場合は false 。</returns>
        public bool TryDodge(Vector2 input, float currentTime)
        {
            // 入力がない場合は回避不可。
            if (input.sqrMagnitude <= float.Epsilon)
                return false;
            // クールダウン中は回避不可。
            if (currentTime - _previousDodgedTime < _parameter.DodgeCooldown)
                return false;

            _previousDodgedTime = currentTime;
            _direction = new Vector3(input.x, 0, input.y).normalized;
            _isDodging = true;
            _hasNotifiedDodgeEnd = false;

            OnDodgeStarted?.Invoke(_parameter.DodgeDuration);
            return true;
        }

        /// <summary>
        ///     毎フレーム呼び出し、回避中の移動速度と回転を計算する。
        ///     回避時間が終了したら OnDodgeEnded を発火して回避状態を解除する。
        /// </summary>
        /// <param name="rotation">キャラクターの回転（ref）。回避方向に更新される。</param>
        /// <param name="time">現在の時刻（Time.time）。</param>
        /// <param name="velocity">計算された移動速度。回避中でない場合は Vector3.zero 。</param>
        public void Update(ref Quaternion rotation, float time, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            if (!_isDodging)
                return;

            if (time > _previousDodgedTime + _parameter.DodgeDuration)
            {
                _isDodging = false;
                if (!_hasNotifiedDodgeEnd)
                {
                    _hasNotifiedDodgeEnd = true;
                    OnDodgeEnded?.Invoke();
                }
                return;
            }

            velocity = (float)_parameter.DodgeSpeed * _direction;
            rotation = Quaternion.LookRotation(_direction, Vector3.up);
        }

        private readonly PlayerMoveParameter _parameter;

        private Vector3 _direction;
        private float _previousDodgedTime;
        private bool _isDodging;
        // 回避終了イベントが複数回呼び出されないようにするフラグ。
        private bool _hasNotifiedDodgeEnd;
    }
}
