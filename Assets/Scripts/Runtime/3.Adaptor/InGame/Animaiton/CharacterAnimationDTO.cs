using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     AdaptorからViewModelへアニメーション更新データを渡すDTO。
    /// </summary>
    public class CharacterAnimationDTO  
    {
        /// <summary> CharacterAnimationDTOを初期化する。 </summary>
        /// <param name="animationSpeed"> アニメーション再生速度（bpm / 60f）。 </param>
        /// <param name="weights"> StateのInt値順に並んだブレンドウェイト配列。 </param>
        public CharacterAnimationDTO(float animationSpeed, float[] weights)
        {
            AnimationSpeed = animationSpeed;
            Weights = weights;
        }

        /// <summary> アニメーション再生速度（bpm / 60f）。 </summary>
        public float AnimationSpeed { get; }

        /// <summary> StateのInt値順に並んだブレンドウェイト配列。 </summary>
        public float[] Weights { get; }
    }
}
