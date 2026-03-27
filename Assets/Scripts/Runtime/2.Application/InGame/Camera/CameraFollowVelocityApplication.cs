using UnityEngine;

namespace KillChord.Runtime.Application
{
    public struct CameraFollowVelocityApplication
    {
        public Vector3 UpdateFollowVelocity(in Vector3 currentFollowPosition, float deltaTime)
        {
            if (!_isInitilized || deltaTime <= 0)
            {
                _previousFollowPosition = currentFollowPosition;
                _isInitilized = true;
                return Vector3.zero;
            }

            Vector3 velocity = currentFollowPosition - _previousFollowPosition;
            _previousFollowPosition = currentFollowPosition;
            return velocity / deltaTime;
        }

        private Vector3 _previousFollowPosition;
        private bool _isInitilized;
    }
}
