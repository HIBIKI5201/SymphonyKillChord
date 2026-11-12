namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     自動ビルドのビルドモードです。
    /// </summary>
    internal enum AutoBuildMode
    {
        Development,
        Master,

        /// <summary> 出展イベント用の展示版です。体験版でだけ使い、タイマーとデータリセットを有効にします。 </summary>
        Exhibition,
    }
}
