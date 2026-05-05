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
                _collisionMask,
                _pitchRange,
#if UNITY_STANDALONE_WIN
                _invertVertical,
                _invertHorizontal
#endif
                );

        [Header("Main")]
        [Tooltip("追従先を中心としたカメラの基本的オフセット位置")]
        [SerializeField] private Vector3 _cameraOffset;
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
        [Tooltip("カメラのピッチ角度(x角度)の最小値と最大値")]
        [SerializeField] private Vector2 _pitchRange = new Vector2(-45f, 75f);

#if UNITY_STANDALONE_WIN
        [Header("Input Invert")]
        [SerializeField, Tooltip("垂直方向の入力を反転するフラグ。")]
        private bool _invertVertical = false;

        [SerializeField, Tooltip("水平方向の入力を反転するフラグ。")]
        private bool _invertHorizontal = false;
#endif
    }
}
