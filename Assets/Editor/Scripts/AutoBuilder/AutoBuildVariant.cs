namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     自動ビルドのゲームデータ種別です。CI のビルド種別（release / demo / exhibition）と同じ軸です。
    ///     展示版は体験版のゲームデータを使い、タイマーとデータリセットを有効にします。
    /// </summary>
    internal enum AutoBuildVariant
    {
        Release,
        Demo,

        /// <summary> 出展イベント用の展示版です。体験版のデータを使い、Master だけをビルドします。 </summary>
        Exhibition,
    }
}
