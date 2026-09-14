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
        /// <param name="firstClearSkillBuildPoint"> 初回クリア時に付与するスキル編成・強化ポイント。 </param>
        /// <param name="firstClearSkillUnlockPoint"> 初回クリア時に付与するスキル解放・パラメーター強化ポイント。 </param>
        /// <param name="successSkillBuildPoint"> クリアする度に毎回付与するスキル編成・強化ポイント。 </param>
        /// <param name="successSkillUnlockPoint"> クリアする度に毎回付与するスキル解放・パラメーター強化ポイント。 </param>
        public StageReward(
            int firstClearSkillBuildPoint,
            int firstClearSkillUnlockPoint,
            int successSkillBuildPoint,
            int successSkillUnlockPoint)
        {
            _firstClearSkillBuildPoint = firstClearSkillBuildPoint;
            _firstClearSkillUnlockPoint = firstClearSkillUnlockPoint;
            _successSkillBuildPoint = successSkillBuildPoint;
            _successSkillUnlockPoint = successSkillUnlockPoint;
        }

        /// <summary> 初回クリア時に付与するスキル編成・強化ポイント。 </summary>
        public int FirstClearSkillBuildPoint => _firstClearSkillBuildPoint;
        /// <summary> 初回クリア時に付与するスキル解放・パラメーター強化ポイント。 </summary>
        public int FirstClearSkillUnlockPoint => _firstClearSkillUnlockPoint;
        /// <summary> クリアする度に毎回付与するスキル編成・強化ポイント。 </summary>
        public int SuccessSkillBuildPoint => _successSkillBuildPoint;
        /// <summary> クリアする度に毎回付与するスキル解放・パラメーター強化ポイント。 </summary>
        public int SuccessSkillUnlockPoint => _successSkillUnlockPoint;

        private readonly int _firstClearSkillBuildPoint;
        private readonly int _firstClearSkillUnlockPoint;
        private readonly int _successSkillBuildPoint;
        private readonly int _successSkillUnlockPoint;
    }
}
