namespace KillChord.Runtime.Domain.InGame.Mission.StepEntryAction
{
    /// <summary>
    ///     スキル発動の可否を表します。
    /// </summary>
    public sealed class SetSkillExecutionEnabledStepEntryAction : IMissionStepEntryAction
    {
        /// <summary>
        ///     スキル発動を許可するかを指定して生成する。
        /// </summary>
        public SetSkillExecutionEnabledStepEntryAction(bool isSkillExecutionEnabled)
        {
            IsSkillExecutionEnabled = isSkillExecutionEnabled;
        }

        /// <summary> スキル発動を許可するか </summary>
        public bool IsSkillExecutionEnabled { get; }
    }
}
