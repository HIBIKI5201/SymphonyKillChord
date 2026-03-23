namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータの親クラス。
    /// </summary>
    public abstract class SaveDataBase
    {
        /// <summary>バージョン番号</summary>
        public string Version { get; set; }
    }
}