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

        public bool TryDodge(Vector2 input, float currentTime)
        {
            if (input.sqrMagnitude <= float.Epsilon)
                return false;

            if (currentTime - _previousDodgedTime < _parameter.DodgeCooldown)
                return false;

            _previousDodgedTime = currentTime;
            _direction = new Vector3(input.x, 0, input.y).normalized;
            _isDodging = true;
            _hasNotifiedDodgeEnd = false;

            OnDodgeStarted?.Invoke(_parameter.DodgeDuration);
            return true;
        }
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
