using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    public readonly ref struct CameraSystemContext
    {
        public CameraSystemContext(
            in Vector3 followPosition,
            Vector2 input,
            Vector2 moveInput,
            float deltaTime)
        {
            FollowPosition = followPosition;
            Input = input;
            MoveInput = moveInput;
            DeltaTime = deltaTime;
        }

        public readonly Vector3 FollowPosition;
        public readonly Vector2 Input;
        public readonly Vector2 MoveInput;
        public readonly float DeltaTime;
    }
}
