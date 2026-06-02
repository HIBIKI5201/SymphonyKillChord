using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     アニメーション更新データを保持するViewModel。
    ///     DTOを受け取り、PlayableAnimationControllerへ渡す値を管理する。
    /// </summary>
    public class CharacterAnimationViewModel
    {
        /// <summary> アニメーション再生速度（bpm / 60f）。 </summary>
        public float AnimationSpeed { get; private set; }

        /// <summary> StateのInt値順に並んだブレンドウェイト配列。 </summary>
        public float[] Weights { get; private set; }

        /// <summary>
        ///     DTOを受け取って各値を更新する。
        /// </summary>
        /// <param name="dto"> Adaptorから渡されるDTO。 </param>
        public void Apply(in CharacterAnimationDTO dto)
        {
            AnimationSpeed = dto.AnimationSpeed;
            Weights = dto.Weights;
        }
    }
}
