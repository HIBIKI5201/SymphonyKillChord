using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     Camera View が使用する設定値。
    /// </summary>
    public readonly struct CameraViewSettings
    {
        /// <summary>
        ///     Camera View 用設定値を初期化する。
        /// </summary>
        /// <param name="offset"> カメラの基本オフセット。</param>
        /// <param name="characterCenterOffset"> キャラクターモデルの中心オフセット。</param>
        /// <param name="defaultDistance"> 通常時のカメラ距離。</param>
        /// <param name="followOffsetPower"> 移動追従オフセットの強さ。</param>
        /// <param name="followLerpSpeed"> 移動追従オフセットの補間速度。</param>
        /// <param name="boneRotateSpeed"> ロックオン時のボーン回転速度。</param>
        /// <param name="lockOnAngleMargin"> ロックオン時の角度許容範囲。</param>
        /// <param name="followRotationSpeed"> フリールック時の回転速度。</param>
        /// <param name="lockOnLookAtRatio"> ロックオン注視点の補間比率。</param>
        /// <param name="lockOnRotationSpeed"> ロックオン時のカメラ回転速度。</param>
        /// <param name="collisionRadius"> 衝突判定半径。</param>
        /// <param name="collisionMask"> 衝突判定レイヤー。</param>
        /// <param name="pitchRange"> ピッチ角度の制限範囲。</param>
        /// <param name="invertVertical"> 垂直方向の入力反転フラグ。</param>
        /// <param name="invertHorizontal"> 水平方向の入力反転フラグ。</param>
        public CameraViewSettings(
            in Vector3 offset,
            in Vector3 characterCenterOffset,
            float defaultDistance,
            float followOffsetPower,
            float followLerpSpeed,
            float boneRotateSpeed,
            float lockOnAngleMargin,
            float followRotationSpeed,
            float lockOnLookAtRatio,
            float lockOnRotationSpeed,
            float collisionRadius,
            int collisionMask,
            in Vector2 pitchRange,
            bool invertVertical,
            bool invertHorizontal)
        {
            Offset = offset;
            CharacterCenterOffset = characterCenterOffset;
            DefaultDistance = defaultDistance;
            FollowOffsetPower = followOffsetPower;
            FollowLerpSpeed = followLerpSpeed;
            BoneRotateSpeed = boneRotateSpeed;
            LockOnAngleMargin = lockOnAngleMargin;
            FollowRotationSpeed = followRotationSpeed;
            LockOnLookAtRatio = lockOnLookAtRatio;
            LockOnRotationSpeed = lockOnRotationSpeed;
            CollisionRadius = collisionRadius;
            CollisionMask = collisionMask;
            PitchRange = pitchRange;
            IsInvertVertical = invertVertical;
            IsInvertHorizontal = invertHorizontal;
        }

        /// <summary> カメラの基本オフセット。 </summary>
        public Vector3 Offset { get; }

        /// <summary> キャラクターモデルの中心オフセット。 </summary>
        public Vector3 CharacterCenterOffset { get; }

        /// <summary> 通常時のカメラ距離。 </summary>
        public float DefaultDistance { get; }

        /// <summary> 移動追従オフセットの強さ。 </summary>
        public float FollowOffsetPower { get; }

        /// <summary> 移動追従オフセットの補間速度。 </summary>
        public float FollowLerpSpeed { get; }

        /// <summary> ロックオン時のボーン回転速度。 </summary>
        public float BoneRotateSpeed { get; }

        /// <summary> ロックオン時の角度許容範囲。 </summary>
        public float LockOnAngleMargin { get; }

        /// <summary> フリールック時の回転速度。 </summary>
        public float FollowRotationSpeed { get; }

        /// <summary> ロックオン注視点の補間比率。 </summary>
        public float LockOnLookAtRatio { get; }

        /// <summary> ロックオン時のカメラ回転速度。 </summary>
        public float LockOnRotationSpeed { get; }

        /// <summary> 衝突判定半径。 </summary>
        public float CollisionRadius { get; }

        /// <summary> 衝突判定レイヤー。 </summary>
        public int CollisionMask { get; }

        /// <summary> ピッチ角度の制限範囲。 </summary>
        public Vector2 PitchRange { get; }

        /// <summary> 垂直方向の入力反転フラグ。 </summary>
        public bool IsInvertVertical { get; }

        /// <summary> 水平方向の入力反転フラグ。 </summary>
        public bool IsInvertHorizontal { get; }
    }
}
