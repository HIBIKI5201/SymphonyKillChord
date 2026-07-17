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
        public SkillExecutionResult(SkillExecutionResultType resultType, string animationKey = null)
        {
            ResultType = resultType;
            AnimationKey = animationKey;
        }

        /// <summary> 結果種別です。 </summary>
        public SkillExecutionResultType ResultType { get; }

        /// <summary> 再生するアニメーションキーです。 </summary>
        public string AnimationKey { get; }
    }
}
