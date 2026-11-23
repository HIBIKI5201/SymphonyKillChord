using System;
using SymphonyFramework.SaveData;

namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータの親クラス。
    ///     SaveStoreが扱えるようにSymphonyFrameworkのSaveDataContentを継承する。
    /// </summary>
    [Serializable]
    public abstract class SaveDataBase : SaveDataContent
    {
        /// <summary>バージョン番号</summary>
        public string Version { get; set; }
    }
}