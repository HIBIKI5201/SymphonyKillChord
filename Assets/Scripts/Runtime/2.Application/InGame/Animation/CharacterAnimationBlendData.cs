using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     アニメーションのブレンド結果。
    /// </summary>
    public readonly struct CharacterAnimationBlendData
    {
        /// <summary>
        ///     ブレンドデータを初期化する。
        /// </summary>
        public CharacterAnimationBlendData(float idleWeight, float walkWeight)
        {
            IdleWeight = idleWeight;
            WalkWeight = walkWeight;
        }

        /// <summary> Idleウェイト。 </summary>
        public float IdleWeight { get; }

        /// <summary> Walkウェイト。 </summary>
        public float WalkWeight { get; }

    }
}
