using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Camera
{
    /// <summary>
    ///     カメラシステムの各種パラメータを設定するためのScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CameraSystemConfig), menuName = "KillChord/InGame/CameraSystemConfig")]
    public sealed class CameraSystemConfig : ScriptableObject
    {
        /// <summary> カメラの基本オフセット。 </summary>
        public Vector3 Offset => _cameraOffset;

        /// <summary> キャラクターモデルの中心オフセット。 </summary>
        public Vector3 CharacterCenterOffset => _characterCenterOffset;

        /// <summary> 通常時のカメラ距離。 </summary>
        public float Distance => _distance;

        /// <summary> 移動追従オフセットの強さ。 </summary>
        public float FollowOffsetPower => _followOffsetPower;

        /// <summary> 移動追従オフセットの補間速度。 </summary>
        public float FollowLerpSpeed => _followLerpSpeed;

        /// <summary> ロックオン時のボーン回転速度。 </summary>
        public float BoneRotateSpeed => _boneRotateSpeed;

        /// <summary> ロックオン時の角度許容範囲。 </summary>
        public float LockOnAngleMargin => _lockOnAngleMargin;

        /// <summary> フリールック時の回転速度。 </summary>
        public float FollowRotationSpeed => _followRotationSpeed;

        /// <summary> ロックオン注視点の補間比率。 </summary>
        public float LockOnLookAtRatio => _lockOnLookAtRatio;

        /// <summary> ロックオン時のカメラ回転速度。 </summary>
        public float LockOnRotationSpeed => _lockOnRotationSpeed;

        /// <summary> 衝突判定半径。 </summary>
        public float CollisionRadius => _collisionRadius;

        /// <summary> 衝突判定レイヤー。 </summary>
        public int CollisionMask => _collisionMask;

        /// <summary> ピッチ角度の制限範囲。 </summary>
        public Vector2 PitchRange => _pitchRange;

        /// <summary> 垂直方向の入力反転フラグ。 </summary>
        public bool IsInvertVertical => _invertVertical;

        /// <summary> 水平方向の入力反転フラグ。 </summary>
        public bool IsInvertHorizontal => _invertHorizontal;

        [Header("Main")]
        [Tooltip("追従先を中心としたカメラの基本的オフセット位置")]
        [SerializeField] private Vector3 _cameraOffset;
        [Tooltip("キャラクターモデルの中心オフセット")]
        [SerializeField] private Vector3 _characterCenterOffset;
        [Tooltip("追従先からカメラまでの距離")]
        [SerializeField] private float _distance = 5f;

        [Header("Follow")]
        [Tooltip("プレイヤー移動中のカメラの追従オフセットの強さ")]
        [SerializeField] private float _followOffsetPower = 2f;
        [Tooltip("プレイヤー移動中のカメラの追従オフセットの補間速度")]
        [SerializeField] private float _followLerpSpeed = 1.0f;

        [Header("Bone Rotation")]
        [Tooltip("ロックオン時のカメラボーンの回転速度")]
        [SerializeField] private float _boneRotateSpeed = 1.2f;
        [Tooltip("ロックオン状態でのカメラとターゲットの角度差の許容範囲")]
        [SerializeField] private float _lockOnAngleMargin = 10f;
        [Tooltip("非ロックオン時のカメラボーンの回転速度")]
        [SerializeField] private float _followRotationSpeed = 1.5f;

        [Header("Camera Rotation")]
        [Tooltip("ロックオン時のカメラが向けるプレイヤー位置とターゲット位置の補間比率")]
        [Range(0f, 1f)]
        [SerializeField] private float _lockOnLookAtRatio = 0.5f;
        [Tooltip("ロックオン時のカメラの回転速度")]
        [SerializeField] private float _lockOnRotationSpeed = 2.0f;

        [Header("Collision")]
        [Tooltip("カメラの衝突判定に使用する球の半径")]
        [SerializeField] private float _collisionRadius = 0.2f;
        [Tooltip("カメラの衝突判定に使用するレイヤーマスク")]
        [SerializeField] private LayerMask _collisionMask;

        [Header("Limits")]
        [Tooltip("カメラのピッチ角度( x 角度)の最小値と最大値")]
        [SerializeField] private Vector2 _pitchRange = new Vector2(-45f, 75f);

        [Header("Input Invert")]
        [SerializeField, Tooltip("垂直方向の入力を反転するフラグ。")]
        private bool _invertVertical = false;

        [SerializeField, Tooltip("水平方向の入力を反転するフラグ。")]
        private bool _invertHorizontal = false;
    }
}
