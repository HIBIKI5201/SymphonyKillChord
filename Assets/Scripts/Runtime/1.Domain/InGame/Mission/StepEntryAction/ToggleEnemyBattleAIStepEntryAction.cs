namespace KillChord.Runtime.Domain.InGame.Mission.StepEntryAction
{
    /// <summary>
    ///     敵の戦闘AIが有効か否かを表す。
    /// </summary>
    public class ToggleEnemyBattleAIStepEntryAction : IMissionStepEntryAction
    {
        /// <summary>
        ///     敵の戦闘 AI を有効にするかを指定して生成する。
        /// </summary>
        public ToggleEnemyBattleAIStepEntryAction(bool battleAiActivated)
        {
            IsBattleAiActivated = battleAiActivated;
        }

        /// <summary> 敵の戦闘AIが有効か否か </summary>
        public bool IsBattleAiActivated { get; private set; }
    }
}
