using UnityEngine;

namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータのバージョン移行インターフェース。
    /// </summary>
    public abstract class SaveDataMigrationBase<TSaveType>
    {
        /// <summary>移行前バージョン</summary>
        public string FromVersion;
        /// <summary>移行後バージョン</summary>
        public string ToVersion;
        /// <summary>
        ///     移行を行う。
        /// </summary>
        /// <param name="saveData"></param>
        public abstract Awaitable Migrate(TSaveType saveData);
    }
}