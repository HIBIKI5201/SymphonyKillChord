using Unity.Cinemachine;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    /// <summary>
    ///     カメラの設定データ。
    /// </summary>
    /// 
    [CreateAssetMenu(
        fileName = nameof(CameraConfigs),
        menuName = "MusicBattle/" + nameof(CameraConfigs))]
    public class CameraConfigs : ScriptableObject
    {
        public Vector3 CameraOffset => _cameraOffset;
        public Vector3 CameraLookAtOffset => _cameraLookAtOffset;

        public float CameraFollowDamping => _cameraFollowDamping;
        public float CameraLookAtDamping => _cameraLookAtDamping;

        public float CameraRotationSpeed => _cameraRotationSpeed;
        public float PitchRangeMin => _pitchRange.x;
        public float PitchRangeMax => _pitchRange.y;
        public bool IsCameraFlipX => _isCameraFlipX;

        public float CameraCollisionRadius => _cameraCollisionRadius;
        public Vector3 CameraCollisionOffset => _cameraCollisionOffset;

        [Header("追従・注視補正")]
        [SerializeField, Tooltip("カメラの追従位置補正")]
        private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f);
        [SerializeField, Tooltip("カメラの注視位置補正")]
        private Vector3 _cameraLookAtOffset = new Vector3(0f, 1.5f, 0f);

        [SerializeField, Tooltip("カメラ追従速度の減衰率")]
        private float _cameraFollowDamping = 0f;
        [SerializeField, Tooltip("カメラ注視速度の減衰率")]
        private float _cameraLookAtDamping = 0f;

        [Header("カメラ操作")]
        [SerializeField, Tooltip("カメラ感度")]
        private float _cameraRotationSpeed = 3f;
        [SerializeField, Tooltip("ピッチ角度の範囲（オイラー角度）"), MinMaxRangeSlider(-90, 90)]
        private Vector2 _pitchRange = new Vector2(-30f, 60f);
        [Space]
        [SerializeField, Tooltip("カメラのX回転を反転")]
        private bool _isCameraFlipX;

        [Header("障害物回避")]
        [SerializeField, Tooltip("カメラの障害物回避範囲")]
        private float _cameraCollisionRadius = 0.3f;
        [SerializeField, Tooltip("カメラの障害物回避の位置補正")]
        private Vector3 _cameraCollisionOffset = new Vector3(0f, 1f, 0f);
    }
}
