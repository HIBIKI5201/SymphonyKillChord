using LitMotion;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIのアニメーション設定。
    /// </summary>
    public sealed class SkillInputProgressAnimationSetting
    {
        /// <summary>
        ///     スキル入力進行UIのアニメーション設定を生成する。
        /// </summary>
        /// <param name="inputSuccessScaleMultiplier"> 入力成功時の拡大倍率。 </param>
        /// <param name="inputSuccessRotationAngle"> 入力成功時の回転角度。 </param>
        /// <param name="inputSuccessDuration"> 入力成功アニメーションの長さ。 </param>
        /// <param name="inputSuccessEase"> 入力成功アニメーションのイージング。 </param>
        /// <param name="inputSuccessRotationFrequency"> 入力成功時の回転振動数。 </param>
        /// <param name="inputSuccessRotationDampingRatio"> 入力成功時の回転減衰率。 </param>
        /// <param name="resetShakeDistance"> リセット時の横揺れ距離。 </param>
        /// <param name="resetShakeDuration"> リセット時の横揺れ時間。 </param>
        /// <param name="resetShakeEase"> リセット時の横揺れイージング。 </param>
        /// <param name="resetShakeFrequency"> リセット時の横揺れ振動数。 </param>
        /// <param name="resetShakeDampingRatio"> リセット時の横揺れ減衰率。 </param>
        public SkillInputProgressAnimationSetting(
            float inputSuccessScaleMultiplier,
            float inputSuccessRotationAngle,
            float inputSuccessDuration,
            Ease inputSuccessEase,
            int inputSuccessRotationFrequency,
            float inputSuccessRotationDampingRatio,
            float resetShakeDistance,
            float resetShakeDuration,
            Ease resetShakeEase,
            int resetShakeFrequency,
            float resetShakeDampingRatio)
        {
            InputSuccessScaleMultiplier = Mathf.Max(1f, inputSuccessScaleMultiplier);
            InputSuccessRotationAngle = Mathf.Max(0f, inputSuccessRotationAngle);
            InputSuccessDuration = Mathf.Max(0.01f, inputSuccessDuration);
            InputSuccessEase = inputSuccessEase;
            InputSuccessRotationFrequency = Mathf.Max(1, inputSuccessRotationFrequency);
            InputSuccessRotationDampingRatio = Mathf.Max(0f, inputSuccessRotationDampingRatio);
            ResetShakeDistance = Mathf.Max(0f, resetShakeDistance);
            ResetShakeDuration = Mathf.Max(0.01f, resetShakeDuration);
            ResetShakeEase = resetShakeEase;
            ResetShakeFrequency = Mathf.Max(1, resetShakeFrequency);
            ResetShakeDampingRatio = Mathf.Max(0f, resetShakeDampingRatio);
        }

        /// <summary> 入力成功時の拡大倍率。 </summary>
        public float InputSuccessScaleMultiplier { get; }

        /// <summary> 入力成功時の回転角度。 </summary>
        public float InputSuccessRotationAngle { get; }

        /// <summary> 入力成功アニメーションの長さ。 </summary>
        public float InputSuccessDuration { get; }

        /// <summary> 入力成功アニメーションのイージング。 </summary>
        public Ease InputSuccessEase { get; }

        /// <summary> 入力成功時の回転振動数。 </summary>
        public int InputSuccessRotationFrequency { get; }

        /// <summary> 入力成功時の回転減衰率。 </summary>
        public float InputSuccessRotationDampingRatio { get; }

        /// <summary> リセット時の横揺れ距離。 </summary>
        public float ResetShakeDistance { get; }

        /// <summary> リセット時の横揺れ時間。 </summary>
        public float ResetShakeDuration { get; }

        /// <summary> リセット時の横揺れイージング。 </summary>
        public Ease ResetShakeEase { get; }

        /// <summary> リセット時の横揺れ振動数。 </summary>
        public int ResetShakeFrequency { get; }

        /// <summary> リセット時の横揺れ減衰率。 </summary>
        public float ResetShakeDampingRatio { get; }
    }
}
