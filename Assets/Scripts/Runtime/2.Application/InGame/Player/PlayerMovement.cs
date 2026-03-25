using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class PlayerMovement
    {
        public PlayerMovement(PlayerMoveParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(ref Vector3 position, ref Quaternion rotation, Vector2 input, float deltaTime)
        {
            if (input == Vector2.zero)
                return;

            if (input.sqrMagnitude > 1f)
                input.Normalize();
            Vector3 direction = new Vector3(input.x, 0, input.y);
            position += direction * (deltaTime * _parameter.MoveSpeed.Value);
            rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private readonly PlayerMoveParameter _parameter;
    }
}
