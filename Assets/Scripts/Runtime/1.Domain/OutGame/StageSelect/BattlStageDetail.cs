namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     バトルステージの詳細情報を表すクラス。
    /// </summary>
    public class BattlStageDetail : StageDetailBase
    {
        /// <summary>
        ///     バトルステージの詳細情報を初期化する。
        /// </summary>
        /// <param name="stageName"> ステージ名。 </param>
        /// <param name="flavorText"> フレーバーテキスト。 </param>
        /// <param name="stageClearReward"> ステージクリア報酬。 </param>
        /// <param name="mainMissionDescription"> メインミッションの説明。 </param>
        /// <param name="sub1MissionDescription"> サブミッション1の説明。 </param>
        /// <param name="sub2MissionDescription"> サブミッション2の説明。 </param>
        /// <param name="previousMissionProgress"> 前回のミッションの進行状況。 </param>
        public BattlStageDetail(
            string stageName,
            string flavorText,
            string stageClearReward,
            string mainMissionDescription,
            string sub1MissionDescription,
            string sub2MissionDescription,
            string previousMissionProgress)
            : base(stageName, flavorText)
        {
            StageClearReward = stageClearReward;
            MainMissionDescription = mainMissionDescription;
            Sub1MissionDescription = sub1MissionDescription;
            Sub2MissionDescription = sub2MissionDescription;
            PreviousMissionProgress = previousMissionProgress;
        }

        /// <summary>
        /// ステージクリア報酬。
        /// </summary>
        public string StageClearReward { get; }
        /// <summary>
        /// メインミッションの説明。
        /// </summary>
        public string MainMissionDescription { get; }
        /// <summary>
        /// サブミッション1の説明。
        /// </summary>
        public string Sub1MissionDescription { get; }
        /// <summary>
        /// サブミッション2の説明。
        /// </summary>
        public string Sub2MissionDescription { get; }

        /// <summary>
        /// 前回のミッションの進行状況。
        /// </summary>
        public string PreviousMissionProgress { get; }
    }
}
