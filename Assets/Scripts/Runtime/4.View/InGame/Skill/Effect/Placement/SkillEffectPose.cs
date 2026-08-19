using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     スキルエフェクトの配置結果を表す値オブジェクト。
    /// </summary>
    public readonly struct SkillEffectPose
    {
        /// <summary>
        ///     配置結果を生成する。
        /// </summary>
        /// <param name="position"> ワールド座標です。 </param>
        /// <param name="rotation"> ワールド回転です。 </param>
        /// <param name="followTransform"> 追従対象のTransformです。設置型の場合はnull。 </param>
        public SkillEffectPose(Vector3 position, Quaternion rotation, Transform followTransform = null)
        {
            _position = position;
            _rotation = rotation;
            _followTransform = followTransform;
        }

        /// <summary> ワールド座標です。 </summary>
        public Vector3 Position => _position;

        /// <summary> ワールド回転です。 </summary>
        public Quaternion Rotation => _rotation;

        /// <summary> 追従対象のTransformです。 </summary>
        public Transform FollowTransform => _followTransform;

        /// <summary> 追従型かどうかです。 </summary>
        public bool IsFollow => _followTransform != null;

        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly Transform _followTransform;
    }
}
