using KillChord.Runtime.Domain.InGame.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Player
{
    /// <summary>
    ///     プレイヤーの通常移動を更新するクラス。
    /// </summary>
    public sealed class PlayerMovementApplication
    {
        private readonly PlayerMoveParameter _parameter;

        /// <summary> 通常移動アプリケーションを初期化する。 </summary>
        public PlayerMovementApplication(PlayerMoveParameter parameter)
        {
            _parameter = parameter;
        }

        /// <summary> 入力方向に応じて向きと速度を更新する。 </summary>
        public void Update(ref Quaternion rotation, Vector2 input, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            if (input == Vector2.zero)
            {
                return;
            }

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            Vector3 direction = new Vector3(input.x, 0, input.y);
            velocity = direction * _parameter.MoveSpeed.Value;
            rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
