using UnityEngine;

namespace KillChord.Runtime.Domain
{
    [System.Serializable]
    public sealed class CameraMovementParameter
    {
        public CameraMovementParameter(float rotateLerpSpeed, float followLerpSpeed, float followOffsetPower)
        {
            _rotateLerpSpeed = rotateLerpSpeed;
            _followLerpSpeed = followLerpSpeed;
            _followOffsetPower = followOffsetPower;
        }

        public float RotateLerpSpeed => _rotateLerpSpeed;
        public float FollowLerpSpeed => _followLerpSpeed;
        public float FollowOffsetPower => _followOffsetPower;

        [SerializeField] private float _rotateLerpSpeed;
        [SerializeField] private float _followLerpSpeed;
        [SerializeField] private float _followOffsetPower;
    }
}
