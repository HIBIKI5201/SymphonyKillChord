namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージクリア報酬を表す値型オブジェクト。
    /// </summary>
    public readonly struct StageReward
    {
        /// <summary>
        ///     ステージクリア報酬を初期化する。
        /// </summary>
        /// <param name="skillBuildPoint"> スキル編成・強化に使用するポイント。 </param>
        /// <param name="skillUnlockPoint"> スキル解放・パラメーター強化に使用するポイント。 </param>
        public StageReward(int skillBuildPoint, int skillUnlockPoint)
        {
            _skillBuildPoint = skillBuildPoint;
            _skillUnlockPoint = skillUnlockPoint;
        }

        /// <summary> スキル編成・強化に使用するポイント。 </summary>
        public int SkillBuildPoint => _skillBuildPoint;
        /// <summary> スキル解放・パラメーター強化に使用するポイント。 </summary>
        public int SkillUnlockPoint => _skillUnlockPoint;

        private readonly int _skillBuildPoint;
        private readonly int _skillUnlockPoint;
    }
}
