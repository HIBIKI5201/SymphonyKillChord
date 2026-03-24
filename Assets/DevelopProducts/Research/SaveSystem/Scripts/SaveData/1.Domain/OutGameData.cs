using System;
using System.Collections.Generic;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータ：アウトゲーム情報
    /// </summary>
    [Serializable]
    public class OutGameData : SaveDataBase
    {
        public OutGameData()
        {
            StageUnlock = new();
            SkillUnlock = new();
            Version = Constants.CURRENT_VERSION;
        }

        /// <summary>ステージ解放状況</summary>
        public HashSet<int> StageUnlock { get; set; }
        /// <summary>スキル解放状況</summary>
        public HashSet<int> SkillUnlock { get; set; }
    }
}