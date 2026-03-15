using System.Collections.Generic;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータクラス
    /// </summary>
    public class KillChordGameData
    {
        public KillChordGameData()
        {
            // TODO 仮の初期値。
            Gold = 0;
            HpMax = 100;
            Attack = 10;
            CritRate = 0.1f;
            CritScale = 1.3f;
            Equipments = new List<int>(new int[] { 0, 0, 0 });
            Skills = new List<int>(new int[] { 0, 0, 0 });
            MissionProgress = new List<int>();
            EquipmentUnlock = new List<int>();
            SkillUnlock = new List<int>();
            MissionUnlock = new List<int>();
        }
        /// <summary>お金</summary>
        public long Gold { get; set; }
        /// <summary>HP最大値</summary>
        public float HpMax { get; set; }
        /// <summary>基礎攻撃力</summary>
        public float Attack { get; set; }
        /// <summary>クリティカル確率</summary>
        public float CritRate { get; set; }
        /// <summary>クリティカル倍率</summary>
        public float CritScale { get; set; }
        /// <summary>装備中の装備ID</summary>
        public List<int> Equipments { get; set; }
        /// <summary>装備中のスキルID</summary>
        public List<int> Skills { get; set; }
        /// <summary>ミッション進捗</summary>
        public List<int> MissionProgress { get; set; }
        /// <summary>装備解放状況</summary>
        public List<int> EquipmentUnlock { get; set; }
        /// <summary>スキル解放状況</summary>
        public List<int> SkillUnlock { get; set; }
        /// <summary>ミッション解放状況</summary>
        public List<int> MissionUnlock { get; set; }
    }
}