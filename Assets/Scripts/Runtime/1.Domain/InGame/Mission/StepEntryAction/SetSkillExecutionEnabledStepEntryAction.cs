namespace KillChord.Runtime.Domain.InGame.Mission.StepEntryAction
{
    /// <summary>
    ///     目標ステップへの進入時に、スキル発動の可否を切り替えるアクションです。
    /// </summary>
    public sealed class SetSkillExecutionEnabledStepEntryAction : IMissionStepEntryAction
    {
        /// <summary>
        ///     SetSkillExecutionEnabledStepEntryAction クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="isSkillExecutionEnabled">スキル発動を許可する場合はtrueです。</param>
        public SetSkillExecutionEnabledStepEntryAction(bool isSkillExecutionEnabled)
        {
            IsSkillExecutionEnabled = isSkillExecutionEnabled;
        }

        /// <summary> スキル発動を許可する場合はtrueです。 </summary>
        public bool IsSkillExecutionEnabled { get; }
    }
}
