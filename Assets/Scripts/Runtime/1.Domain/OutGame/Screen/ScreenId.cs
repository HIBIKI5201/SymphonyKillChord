namespace KillChord.Runtime.Domain.OutGame.Screen
{
    /// <summary>
    ///     アウトゲームの画面を表す列挙型。
    /// </summary>
    public enum ScreenId
    {
        /// <summary> ホーム画面。 </summary>
        Home,
        /// <summary> 作戦画面。 </summary>
        StageSelect,
        /// <summary> 研究画面。 </summary>
        SkillTree,
        /// <summary> 改造画面。 </summary>
        SkillBuild,
        /// <summary> 設定画面。 </summary>
        Setting,
        /// <summary> 廃止した戦闘準備画面の予約値。後続画面のシリアライズ済み番号を維持します。 </summary>
        BattlePreparation,
        /// <summary> タイトル画面。 </summary>
        Title,
        /// <summary> メニュー画面。 </summary>
        Menu,
        /// <summary> オプション画面。 </summary>
        Options,
        /// <summary> クレジット画面。 </summary>
        Credit,
    }
}
