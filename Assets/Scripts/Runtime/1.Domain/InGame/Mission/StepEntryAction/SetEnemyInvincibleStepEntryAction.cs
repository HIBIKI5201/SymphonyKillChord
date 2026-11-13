namespace KillChord.Runtime.Domain.InGame.Mission.StepEntryAction
{
    /// <summary>
    ///     敵を無敵状態にするか否かを表す。
    /// </summary>
    public class SetEnemyInvincibleStepEntryAction : IMissionStepEntryAction
    {
        /// <summary>
        ///     敵を無敵状態にするかを指定して生成する。
        /// </summary>
        public SetEnemyInvincibleStepEntryAction(bool isInvincible)
        {
            IsInvincible = isInvincible;
        }

        /// <summary> 敵を無敵状態にするか否か </summary>
        public bool IsInvincible { get; private set; }
    }
}
