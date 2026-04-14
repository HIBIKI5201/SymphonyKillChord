using KillChord.Runtime.Domain.InGame.Player;
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

        public bool IsDodhing => _isDodging;

        public bool TryDodge(Vector2 input, float currentTime)
        {
            if (input.sqrMagnitude <= float.Epsilon)
                return false;

            if (currentTime - _previousDodgedTime < _parameter.DodgeCooldown)
                return false;

            _previousDodgedTime = currentTime;
            _direction = new Vector3(input.x, 0, input.y).normalized;
            _isDodging = true;

            return true;
        }
        public void Update(ref Quaternion rotaition, float time, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            if (!_isDodging)
                return;

            if (time > _previousDodgedTime + _parameter.DodgeDuration)
            {
                _isDodging = false;
                return;
            }

            velocity = (float)_parameter.DodgeSpeed * _direction;
            rotaition = Quaternion.LookRotation(_direction, Vector3.up);
        }

        private readonly PlayerMoveParameter _parameter;

        private Vector3 _direction;
        private float _previousDodgedTime;
        private bool _isDodging;
    }
}
