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
        public Vector3 CameraOffset => _cameraOffset;
        public Vector3 CameraLookAtOffset => _cameraLookAtOffset;

        public float CameraFollowSpeed => _cameraFollowSpeed;
        public float CameraLookAtSpeed => _cameraLookAtSpeed;

        public float CameraRotationSpeed => _cameraRotationSpeed;
        public Vector2 PicthRange => _picthRange;

        [SerializeField]
        private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f);
        [SerializeField]
        private Vector3 _cameraLookAtOffset = new Vector3(0f, 1.5f, 0f);

        [SerializeField]
        private float _cameraFollowSpeed = 1f;
        [SerializeField]
        private float _cameraLookAtSpeed = 1f;

        [SerializeField]
        private float _cameraRotationSpeed = 3f;
        [SerializeField, MinMaxRangeSlider(-90, 90)]
        private Vector2 _picthRange = new Vector2(-30f, 60f);
    }
}