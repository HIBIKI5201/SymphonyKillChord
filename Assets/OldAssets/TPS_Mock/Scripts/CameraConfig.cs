using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     カメラの設定クラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CameraConfig), menuName = "Mock/TPS/" + nameof(CameraConfig))]
    public class CameraConfig : ScriptableObject
    {
        public float CameraRotationSpeed => _cameraRotationSpeed;
        public Vector3 CameraOffset => _cameraOffset;

        [SerializeField]
        private float _cameraRotationSpeed = 3f;
        [SerializeField]
        private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f);
    }
}