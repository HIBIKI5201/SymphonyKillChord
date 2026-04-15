using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Application
{
    public readonly ref struct CameraSystemContext
    {
        public CameraSystemContext(
            in Vector3 followPosition,
            Vector2 input,
            float deltaTime)
        {
            FollowPosition = followPosition;
            Input = input;
            DeltaTime = deltaTime;
        }

        public readonly Vector3 FollowPosition;
        public readonly Vector2 Input;
        public readonly float DeltaTime;
    }
}
