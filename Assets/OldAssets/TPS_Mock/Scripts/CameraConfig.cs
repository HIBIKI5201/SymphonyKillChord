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

        public float CameraFollowSpeed => _cameraFollowSpeed;
        public float CameraLookAtSpeed => _cameraLookAtSpeed;

        public float CameraRotationSpeed => _cameraRotationSpeed;

        [SerializeField]
        private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f);

        [SerializeField]
        private float _cameraFollowSpeed = 1f;
        [SerializeField]
        private float _cameraLookAtSpeed = 1f;

        [SerializeField]
        private float _cameraRotationSpeed = 3f;
    }
}