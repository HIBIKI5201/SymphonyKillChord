namespace KillChord.Runtime.Domain.InGame.Mission.StepEntryAction
{
    /// <summary>
    ///     敵の戦闘AIが有効か否かを表す。
    /// </summary>
    public class ToggleEnemyBattleAIStepEntryAction : IMissionStepEntryAction
    {
        public ToggleEnemyBattleAIStepEntryAction(bool battleAiActivated)
        {
            IsBattleAiActivated = battleAiActivated;
        }

        /// <summary> 敵の戦闘AIが有効か否か </summary>
        public bool IsBattleAiActivated { get; private set; }
    }
}
