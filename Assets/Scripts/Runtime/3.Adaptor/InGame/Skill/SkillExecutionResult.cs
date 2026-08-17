using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル実行結果です。
    /// </summary>
    public readonly struct SkillExecutionResult
    {
        /// <summary>
        ///     結果を初期化します。
        /// </summary>
        /// <param name="resultType"> 結果種別です。 </param>
        /// <param name="animationKey"> 再生するアニメーションキーです。 </param>
        /// <param name="skillNormalAttackDamagePolicy"> 通常攻撃のダメージ計算ポリシーです。 </param>
        public SkillExecutionResult(SkillExecutionResultType resultType, string animationKey = null, SkillNormalAttackDamagePolicy skillNormalAttackDamagePolicy = SkillNormalAttackDamagePolicy.Apply)
        {
            ResultType = resultType;
            AnimationKey = animationKey;
            SkillNormalAttackDamagePolicy = skillNormalAttackDamagePolicy;
        }

        /// <summary> 結果種別です。 </summary>
        public SkillExecutionResultType ResultType { get; }

        /// <summary> 再生するアニメーションキーです。 </summary>
        public string AnimationKey { get; }

        /// <summary> 通常攻撃のダメージ計算ポリシーです。 </summary>
        public SkillNormalAttackDamagePolicy SkillNormalAttackDamagePolicy { get; }
    }
}
