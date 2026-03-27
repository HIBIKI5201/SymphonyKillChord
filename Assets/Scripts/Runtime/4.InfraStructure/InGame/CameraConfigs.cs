using KillChord.Runtime.Domain;
using Unity.Cinemachine;
using UnityEngine;

namespace KillChord.Structure
{
    [CreateAssetMenu(fileName = nameof(CameraConfigs), menuName = "KillChord/InGame/CameraConfigs")]
    public class CameraConfigs : ScriptableObject
    {
        public CameraParameter ToDomain()
            => new(
                _cameraOffset,
                _cameraLookAtOffset,
                _cameraPlayerFollowDamping,
                _cameraPlayLookAtDamping,
                _cameraLockOnFollowDamping,
                _cameraLockOnLookAtDamping,
                _cameraRotationSpeed,
                _pitchRange.x,
                _pitchRange.y,
                _isCameraFlipX,
                _cameraCollisionRadius,
                _cameraCollisionOffset);

        [SerializeField, Tooltip("カメラの追従位置補正。")]
        private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f);
        [SerializeField, Tooltip("カメラの注視位置補正。")]
        private Vector3 _cameraLookAtOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField, Tooltip("自由カメラ追従速度の減衰率。")]
        private float _cameraPlayerFollowDamping = 0f;
        [SerializeField, Tooltip("自由カメラ注視速度の減衰率。")]
        private float _cameraPlayLookAtDamping = 0f;
        [SerializeField, Tooltip("ロックオンカメラ追従速度の減衰率。")]
        private float _cameraLockOnFollowDamping = 0f;
        [SerializeField, Tooltip("ロックオンカメラ注視速度の減衰率。")]
        private float _cameraLockOnLookAtDamping = 0f;
        [SerializeField, Tooltip("カメラ感度。")]
        private float _cameraRotationSpeed = 3f;
        [SerializeField, Tooltip("ピッチ角度の範囲（オイラー角度）。"), MinMaxRangeSlider(-90f, 90f)]
        private Vector2 _pitchRange = new Vector2(-30f, 60f);
        [SerializeField, Tooltip("カメラのX回転を反転。")]
        private bool _isCameraFlipX;
        [SerializeField, Tooltip("カメラの障害物回避範囲。")]
        private float _cameraCollisionRadius = 0.3f;
        [SerializeField, Tooltip("カメラの障害物回避の位置補正。")]
        private Vector3 _cameraCollisionOffset = new Vector3(0f, 1f, 0f);
    }
}

