using System.Collections.Generic;
namespace Research.SaveSystem
{

    /// <summary>
    ///     セーブデータ：アウトゲーム情報
    /// </summary>
    public class OutGameData
    {
        public OutGameData()
        {
            StoryProgress = new();
            EquipmentUnlock = new();
            SkillUnlock = new();
        }

        /// <summary>ストーリー進捗</summary>
        public HashSet<int> StoryProgress { get; set; }
        /// <summary>装備解放状況</summary>
        public HashSet<int> EquipmentUnlock { get; set; }
        /// <summary>スキル解放状況</summary>
        public HashSet<int> SkillUnlock { get; set; }
    }
}