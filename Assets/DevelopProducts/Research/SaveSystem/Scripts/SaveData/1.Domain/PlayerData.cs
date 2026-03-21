using System;
using System.Collections.Generic;
namespace Research.SaveSystem
{

    /// <summary>
    ///     セーブデータ：プレイヤー情報
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public PlayerData()
        {
            Equipment = new();
            Skill = new();
        }

        /// <summary>装備している装備品</summary>
        public List<int> Equipment { get; set; }
        /// <summary>装備しているスキル</summary>
        public List<int> Skill { get; set; }
    }
}