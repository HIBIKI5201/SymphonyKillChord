using KillChord.Runtime.Domain.InGame.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Player
{
    /// <summary>
    ///     プレイヤーの移動計算を行うクラス。
    /// </summary>
    public sealed class PlayerMovementApplication
    {
        public PlayerMovementApplication(PlayerMoveParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(ref Quaternion rotation, Vector2 input, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            if (input == Vector2.zero)
                return;

            if (input.sqrMagnitude > 1f)
                input.Normalize();
            Vector3 direction = new Vector3(input.x, 0, input.y);
            velocity = direction * _parameter.MoveSpeed.Value;
            rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private readonly PlayerMoveParameter _parameter;
    }
}
