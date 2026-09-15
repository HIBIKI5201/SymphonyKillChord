namespace KillChord.Runtime.Domain.InGame.Mission.StepEntryAction
{
    /// <summary>
    ///     スキル発動の可否を表します。
    /// </summary>
    public sealed class SetSkillExecutionEnabledStepEntryAction : IMissionStepEntryAction
    {
        public SetSkillExecutionEnabledStepEntryAction(bool isSkillExecutionEnabled)
        {
            IsSkillExecutionEnabled = isSkillExecutionEnabled;
        }

        /// <summary> スキル発動を許可するか </summary>
        public bool IsSkillExecutionEnabled { get; }
    }
}
