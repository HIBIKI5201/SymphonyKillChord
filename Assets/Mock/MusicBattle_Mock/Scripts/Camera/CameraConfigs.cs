using Mock.MusicBattle.Basis;
using Unity.Cinemachine;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    /// <summary>
    ///     カメラの設定データ。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(CameraConfigs),
        menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(CameraConfigs))]
    public class CameraConfigs : ScriptableObject
    {
        #region パブリックプロパティ
        /// <summary> カメラの追従位置補正を取得します。 </summary>
        public Vector3 CameraOffset => _cameraOffset;
        /// <summary> カメラの注視位置補正を取得します。 </summary>
        public Vector3 CameraLookAtOffset => _cameraLookAtOffset;
        /// <summary> 自由カメラの追従速度の減衰率を取得します。 </summary>
        public float CameraPlayerFollowDamping => _cameraPlayerFollowDamping;
        /// <summary> 自由カメラの注視速度の減衰率を取得します。 </summary>
        public float CameraPlayerLookAtDamping => _cameraPlayLookAtDamping;
        /// <summary> ロックオンカメラの追従速度の減衰率を取得します。 </summary>
        public float CameraLockOnFollowDamping => _cameraLockOnFollowDamping;
        /// <summary> ロックオンカメラの注視速度の減衰率を取得します。 </summary>
        public float CameraLockOnLookAtDamping => _cameraLockOnLookAtDamping;
        /// <summary> カメラの回転速度を取得します。 </summary>
        public float CameraRotationSpeed => _cameraRotationSpeed;
        /// <summary> ピッチ角度の最小範囲を取得します（オイラー角度）。 </summary>
        public float PitchRangeMin => _pitchRange.x;
        /// <summary> ピッチ角度の最大範囲を取得します（オイラー角度）。 </summary>
        public float PitchRangeMax => _pitchRange.y;
        /// <summary> ロックオン解除の操作待機時間を取得します。 </summary>
        public float UnlockWaitingTime => _unlockWaitingTime;
        /// <summary> カメラのX回転が反転するかどうかを取得します。 </summary>
        public bool IsCameraFlipX => _isCameraFlipX;
        /// <summary> カメラの障害物回避範囲を取得します。 </summary>
        public float CameraCollisionRadius => _cameraCollisionRadius;
        /// <summary> カメラの障害物回避の位置補正を取得します。 </summary>
        public Vector3 CameraCollisionOffset => _cameraCollisionOffset;
        #endregion

        #region インスペクター表示フィールド
        /// <summary> カメラの追従位置補正。 </summary>
        [SerializeField, Tooltip("カメラの追従位置補正。")]
        private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f);
        /// <summary> カメラの注視位置補正。 </summary>
        [SerializeField, Tooltip("カメラの注視位置補正。")]
        private Vector3 _cameraLookAtOffset = new Vector3(0f, 1.5f, 0f);
        /// <summary> 自由カメラの追従速度の減衰率。 </summary>
        [SerializeField, Tooltip("自由カメラ追従速度の減衰率。")]
        private float _cameraPlayerFollowDamping = 0f;
        /// <summary> 自由カメラの注視速度の減衰率。 </summary>
        [SerializeField, Tooltip("自由カメラ注視速度の減衰率。")]
        private float _cameraPlayLookAtDamping = 0f;
        /// <summary> ロックオンカメラの追従速度の減衰率。 </summary>
        [SerializeField, Tooltip("ロックオンカメラ追従速度の減衰率。")]
        private float _cameraLockOnFollowDamping = 0f;
        /// <summary> ロックオンカメラの注視速度の減衰率。 </summary>
        [SerializeField, Tooltip("ロックオンカメラ注視速度の減衰率。")]
        private float _cameraLockOnLookAtDamping = 0f;
        /// <summary> カメラの回転速度。 </summary>
        [SerializeField, Tooltip("カメラ感度。")]
        private float _cameraRotationSpeed = 3f;
        /// <summary> ピッチ角度の範囲（オイラー角度）。 </summary>
        [SerializeField, Tooltip("ピッチ角度の範囲（オイラー角度）。"), MinMaxRangeSlider(-90, 90)]
        private Vector2 _pitchRange = new Vector2(-30f, 60f);
        /// <summary> ロックオン解除の操作待機時間。 </summary>
        [SerializeField, Tooltip("ロックオン解除の操作待機時間。"), Min(0)]
        private float _unlockWaitingTime = 0.3f;
        /// <summary> カメラのX回転を反転するかどうか。 </summary>
        [SerializeField, Tooltip("カメラのX回転を反転。")]
        private bool _isCameraFlipX;
        /// <summary> カメラの障害物回避範囲。 </summary>
        [SerializeField, Tooltip("カメラの障害物回避範囲。")]
        private float _cameraCollisionRadius = 0.3f;
        /// <summary> カメラの障害物回避の位置補正。 </summary>
        [SerializeField, Tooltip("カメラの障害物回避の位置補正。")]
        private Vector3 _cameraCollisionOffset = new Vector3(0f, 1f, 0f);
        #endregion
    }
}
