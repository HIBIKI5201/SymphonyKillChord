using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Structure
{
    [CreateAssetMenu(fileName = nameof(CameraMovementConfig), menuName = "KillChord/InGame/CameraMovementConfig")]
    public sealed class CameraMovementConfig : ScriptableObject
    {
        public CameraMovementParameter ToDomain()
            => new(
                _rotateLerpSpeed,
                _followLerpSpeed,
                _followOffsetPower);

        [SerializeField, Tooltip("カメラ回転の補間速度。")]
        private float _rotateLerpSpeed = 1.2f;

        [SerializeField, Tooltip("カメラ追従の補間速度。")]
        private float _followLerpSpeed = 1.0f;

        [SerializeField, Tooltip("カメラ追従のオフセット強度。")]
        private float _followOffsetPower = 2f;
    }
}
