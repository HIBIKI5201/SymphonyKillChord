using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Structure.InGame.Camera
{
    /// <summary>
    ///     カメラシステムの各種パラメータを設定するためのScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CameraSystemConfig), menuName = "KillChord/InGame/CameraSystemConfig")]
    public sealed class CameraSystemConfig : ScriptableObject
    {
        public CameraSystemParameter ToDomain()
            => new(
                _cameraOffset,
                _distance,
                _followOffsetPower,
                _followLerpSpeed,
                _boneRotateSpeed,
                _lockOnAngleMargin,
                _followRotationSpeed,
                _lockOnLookAtRatio,
                _lockOnRotationSpeed,
                _collisionRadius,
                _pitchRange);

        public LayerMask CollisionMask => _collisionMask;

        [Header("Main")]
        [SerializeField] private Vector3 _cameraOffset;
        [SerializeField] private float _distance = 5f;

        [Header("Follow")]
        [SerializeField] private float _followOffsetPower = 2f;
        [SerializeField] private float _followLerpSpeed = 1.0f;

        [Header("Bone Rotation")]
        [SerializeField] private float _boneRotateSpeed = 1.2f;
        [SerializeField] private float _lockOnAngleMargin = 10f;
        [SerializeField] private float _followRotationSpeed = 1.5f;

        [Header("Camera Rotation")]
        [Range(0f, 1f)]
        [SerializeField] private float _lockOnLookAtRatio = 0.5f;
        [SerializeField] private float _lockOnRotationSpeed = 2.0f;

        [Header("Collision")]
        [SerializeField] private float _collisionRadius = 0.2f;
        [SerializeField] private LayerMask _collisionMask;

        [Header("Limits")]
        [SerializeField] private Vector2 _pitchRange = new Vector2(-45f, 75f);
    }
}
