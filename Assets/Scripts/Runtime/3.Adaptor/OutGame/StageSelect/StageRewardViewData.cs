namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ詳細画面に表示する報酬1件分のデータです。
    /// </summary>
    public readonly struct StageRewardViewData
    {
        /// <summary>
        ///     StageRewardViewData の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="displayName"> リソースの表示名です。 </param>
        /// <param name="amount"> 付与される数量です。 </param>
        public StageRewardViewData(string displayName, int amount)
        {
            DisplayName = displayName ?? string.Empty;
            Amount = amount;
        }

        /// <summary> リソースの表示名です。 </summary>
        public string DisplayName { get; }

        /// <summary> 付与される数量です。 </summary>
        public int Amount { get; }
    }
}
