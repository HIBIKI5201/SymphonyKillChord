using KillChord.Runtime.Domain.OutGame.StageSelect;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ詳細画面向けの DTO。
    /// </summary>
    public readonly struct StageDetailDTO
    {
        /// <summary>
        ///     StageDetailDTO を初期化します。
        /// </summary>
        /// <param name="stageName"> ステージ名。</param>
        /// <param name="flavorText"> フレーバーテキスト。</param>
        /// <param name="rewardSkillBuildPoint"> スキル編成・強化に使用するポイント。</param>
        /// <param name="rewardSkillUnlockPoint"> スキル解放・パラメーター強化に使用するポイント。</param>
        /// <param name="mainMissionText">
        ///     メインミッションのテキスト。
        ///     シナリオパートの場合は null。
        /// </param>
        public StageDetailDTO(
            string stageName,
            string flavorText,
            int rewardSkillBuildPoint,
            int rewardSkillUnlockPoint,
            string mainMissionText)
        {
            StageName = stageName;
            FlavorText = flavorText;
            RewardSkillBuildPoint = rewardSkillBuildPoint;
            RewardSkillUnlockPoint = rewardSkillUnlockPoint;
            MainMissionText = mainMissionText;
        }

        /// <summary> ステージ名。 </summary>
        public string StageName { get; }
        /// <summary> フレーバーテキスト。 </summary>
        public string FlavorText { get; }
        /// <summary> スキル編成・強化に使用するポイント。 </summary>
        public int RewardSkillBuildPoint { get; }
        /// <summary> スキル解放・パラメーター強化に使用するポイント。 </summary>
        public int RewardSkillUnlockPoint { get; }
        /// <summary>
        ///     メインミッションのテキスト。
        ///     シナリオパートの場合は null。
        /// </summary>
        public string MainMissionText { get; }

        /// <summary> バトルパートかどうか。 </summary>
        public bool IsBattle => MainMissionText != null;
    }
}
