using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public class PlayerController
    {
        public PlayerController(PlayerMovement movement)
        {
            _movement = movement;
        }

        public Vector3 GetMovedPosition(Vector3 currentPositon, Vector2 input, float deltaTime)
        {
            return _movement.GetMovedPostion(currentPositon, input, deltaTime);
        }
        public Vector3 GetDodgedPosition(Vector3 currentPosition, Vector2 input, float currentTime)
        {
            _movement.TryGetDodgedPosition(currentPosition, input, currentTime, out Vector3 result);
            return result;
        }


        private readonly PlayerMovement _movement;
    }
}
