using Unity.Cinemachine;
using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     カメラの設定クラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CameraConfig), menuName = "Mock/TPS/" + nameof(CameraConfig))]
    public class CameraConfig : ScriptableObject
    {
        public bool IsCameraFlipX => _isCameraFlipX;

        public Vector3 CameraOffset => _cameraOffset;
        public Vector3 CameraLookAtOffset => _cameraLookAtOffset;

        public float CameraFollowDamping => _cameraFollowDamping;
        public float CameraLookAtDamping => _cameraLookAtDamping;

        public float CameraRotationSpeed => _cameraRotationSpeed;

        public float PitchRangeMin => _pitchRange.x;
        public float PitchRangeMax => _pitchRange.y;
        public float CameraCollisionRadius => _cameraCollisionRadius;


        [SerializeField, Tooltip("カメラのX回転を反転")]
        private bool _isCameraFlipX;

        [SerializeField]
        private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f);
        [SerializeField]
        private Vector3 _cameraLookAtOffset = new Vector3(0f, 1.5f, 0f);

        [SerializeField]
        private float _cameraFollowDamping = 0f;
        [SerializeField]
        private float _cameraLookAtDamping = 0f;

        [SerializeField]
        private float _cameraRotationSpeed = 3f;
        [SerializeField, MinMaxRangeSlider(-90, 90)]
        private Vector2 _pitchRange = new Vector2(-30f, 60f);

        [SerializeField]
        private float _cameraCollisionRadius = 0.3f;
    }
}