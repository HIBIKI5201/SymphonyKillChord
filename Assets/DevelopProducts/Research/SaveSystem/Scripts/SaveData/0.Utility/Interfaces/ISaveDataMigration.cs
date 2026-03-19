namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータのバージョン移行インターフェース
    /// </summary>
    public interface ISaveDataMigration
    {
        /// <summary>移行前バージョン</summary>
        public string FromVersion { get; }
        /// <summary>移行後バージョン</summary>
        public string ToVersion { get; }
        /// <summary>
        ///     移行を行う。
        /// </summary>
        /// <param name="saveData"></param>
        public void Migrate(KillChordGameData saveData);
    }
}