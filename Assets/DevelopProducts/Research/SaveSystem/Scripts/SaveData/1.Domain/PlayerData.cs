using System;
using System.Collections.Generic;
namespace Research.SaveSystem
{

    /// <summary>
    ///     セーブデータ：プレイヤー情報
    /// </summary>
    [Serializable]
    public class PlayerData : SaveDataBase
    {
        public PlayerData()
        {
            EquippedSkills = new();
            Version = Constants.CURRENT_VERSION;
        }

        /// <summary>装備しているスキル</summary>
        public List<int> EquippedSkills { get; set; }
    }
}