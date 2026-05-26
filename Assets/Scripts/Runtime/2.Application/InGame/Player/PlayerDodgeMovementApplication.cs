using System;
using KillChord.Runtime.Domain.InGame.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Player
{
    /// <summary>
    ///     プレイヤーの回避移動を更新するクラス。
    /// </summary>
    public sealed class PlayerDodgeMovementApplication
    {
        private readonly PlayerMoveParameter _parameter;
        private Vector3 _direction;
        private float _previousDodgedTime;
        private bool _isDodging;
        private bool _hasNotifiedDodgeEnd;

        /// <summary> 回避移動アプリケーションを初期化する。 </summary>
        public PlayerDodgeMovementApplication(PlayerMoveParameter parameter)
        {
            _parameter = parameter;
            _previousDodgedTime = float.NegativeInfinity;
        }

        /// <summary> 回避開始時に通知するイベント。 </summary>
        public Action<float> OnDodgeStarted;

        /// <summary> 回避終了時に通知するイベント。 </summary>
        public Action OnDodgeEnded;

        /// <summary> 現在回避中かどうか。 </summary>
        public bool IsDodging => _isDodging;

        /// <summary> 回避開始を試行する。 </summary>
        public bool TryDodge(Vector2 input, float currentTime)
        {
            if (input.sqrMagnitude <= float.Epsilon)
            {
                return false;
            }

            if (currentTime - _previousDodgedTime < _parameter.DodgeCooldown.Value)
            {
                return false;
            }

            _previousDodgedTime = currentTime;
            _direction = new Vector3(input.x, 0, input.y).normalized;
            _isDodging = true;
            _hasNotifiedDodgeEnd = false;

            OnDodgeStarted?.Invoke(_parameter.DodgeDuration.Value);
            return true;
        }

        /// <summary> 回避中の向きと速度を更新する。 </summary>
        public void Update(ref Quaternion rotation, float time, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            if (!_isDodging)
            {
                return;
            }

            if (time > _previousDodgedTime + _parameter.DodgeDuration.Value)
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
    }
}
