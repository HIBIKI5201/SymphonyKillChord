using KillChord.Runtime.Domain.OutGame.StageSelect;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ詳細画面向けの DTO。
    /// </summary>
    public readonly ref struct StageDetailDTO
    {
        /// <summary>
        ///     StageDetailDTO を初期化します。
        /// </summary>
        /// <param name="stageName"> ステージ名。</param>
        /// <param name="flavorText"> フレーバーテキスト。</param>
        /// <param name="currentSkillUnlockPoint"> 現在のスキル解放ポイント。</param>
        /// <param name="currentSkillBuildPoint"> 現在のスキル編成・強化ポイント。</param>
        /// <param name="firstClearRewardSkillUnlockPoint"> 初回報酬で加算されるスキル解放ポイント（研究P）。</param>
        /// <param name="firstClearRewardSkillBuildPoint"> 初回報酬で加算されるスキル編成・強化ポイント（改造P）。</param>
        /// <param name="successRewardSkillUnlockPoint"> 成功報酬で加算されるスキル解放ポイント（研究P）。</param>
        /// <param name="successRewardSkillBuildPoint"> 成功報酬で加算されるスキル編成・強化ポイント（改造P）。</param>
        /// <param name="mainMissionText">
        ///     メインミッションのテキスト。
        ///     シナリオパートの場合は null。
        /// </param>
        public StageDetailDTO(
            string stageName,
            string flavorText,
            int currentSkillUnlockPoint,
            int currentSkillBuildPoint,
            int firstClearRewardSkillUnlockPoint,
            int firstClearRewardSkillBuildPoint,
            int successRewardSkillUnlockPoint,
            int successRewardSkillBuildPoint,
            string mainMissionText,
            string[] subMissionTexts,
            bool[] subMissionCleared)
        {
            StageName = stageName;
            FlavorText = flavorText;
            CurrentSkillUnlockPoint = currentSkillUnlockPoint;
            CurrentSkillBuildPoint = currentSkillBuildPoint;
            FirstClearRewardSkillUnlockPoint = firstClearRewardSkillUnlockPoint;
            FirstClearRewardSkillBuildPoint = firstClearRewardSkillBuildPoint;
            SuccessRewardSkillUnlockPoint = successRewardSkillUnlockPoint;
            SuccessRewardSkillBuildPoint = successRewardSkillBuildPoint;
            MainMissionText = mainMissionText;
            SubMissionTexts = subMissionTexts;
            SubMissionCleared = subMissionCleared;
        }

        /// <summary> ステージ名。 </summary>
        public string StageName { get; }
        /// <summary> フレーバーテキスト。 </summary>
        public string FlavorText { get; }
        /// <summary> 現在のスキル解放ポイント(初回報酬ボックスの矢印左側)。 </summary>
        public int CurrentSkillUnlockPoint { get; }
        /// <summary> 現在のスキル編成・強化ポイント(成功報酬ボックスの矢印左側)。 </summary>
        public int CurrentSkillBuildPoint { get; }
        /// <summary> 初回報酬で加算されるスキル解放ポイント（研究P）。 </summary>
        public int FirstClearRewardSkillUnlockPoint { get; }
        /// <summary> 初回報酬で加算されるスキル編成・強化ポイント（改造P）。 </summary>
        public int FirstClearRewardSkillBuildPoint { get; }
        /// <summary> 成功報酬で加算されるスキル解放ポイント（研究P）。 </summary>
        public int SuccessRewardSkillUnlockPoint { get; }
        /// <summary> 成功報酬で加算されるスキル編成・強化ポイント（改造P）。 </summary>
        public int SuccessRewardSkillBuildPoint { get; }
        /// <summary>
        ///     メインミッションのテキスト。
        ///     シナリオパートの場合は null。
        /// </summary>
        public string MainMissionText { get; }
        /// <summary>
        ///     サブミッションのテキスト。
        ///     シナリオパートの場合は null。
        /// </summary>
        public string[] SubMissionTexts { get; }
        /// <summary>
        ///     サブミッションごとの達成状況。SubMissionTextsと同じ並び順。
        ///     シナリオパートの場合は null。
        /// </summary>
        public bool[] SubMissionCleared { get; }

        /// <summary> バトルパートかどうか。 </summary>
        public bool IsBattle => MainMissionText != null;
    }
}
