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
        public SkillEffectPose(Vector3 position, Quaternion rotation)
        {
            _position = position;
            _rotation = rotation;
        }

        /// <summary> ワールド座標です。 </summary>
        public Vector3 Position => _position;

        /// <summary> ワールド回転です。 </summary>
        public Quaternion Rotation => _rotation;

        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
    }
}
