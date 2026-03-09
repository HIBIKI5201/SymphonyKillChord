using System.Collections.Generic;
namespace Research.Chou.OutGame
{
    /// <summary>
    ///     セーブデータクラス
    /// </summary>
    public class KillChordSaveData
    {
        public KillChordSaveData()
        {
            // TODO 仮の初期値。
            PlayerStatus = new PlayerStatus(1, 0, 0, 100, 10, 0.05f, 1.5f, new int[] {0, 0, 0 }, new int[] { 0, 0, 0 });
            MissionProgress = new HashSet<int>();
            EquipmentUnlock = new HashSet<int>();
            SkillUnlock = new HashSet<int>();
            MissionUnlock = new HashSet<int>();
        }

        /// <summary>プレイヤーステータス</summary>
        public PlayerStatus PlayerStatus { get; set; }
        /// <summary>ミッション進捗</summary>
        public HashSet<int> MissionProgress { get; set; }
        /// <summary>装備解放状況</summary>
        public HashSet<int> EquipmentUnlock { get; set; }
        /// <summary>スキル解放状況</summary>
        public HashSet<int> SkillUnlock { get; set; }
        /// <summary>ミッション解放状況</summary>
        public HashSet<int> MissionUnlock { get; set; }
    }

    /// <summary>
    ///     プイレイヤーステータス
    /// </summary>
    public struct PlayerStatus
    {
        /// <summary>レベル</summary>
        public int Lv;
        /// <summary>経験値</summary>
        public long Exp;
        /// <summary>お金</summary>
        public long Gold;
        /// <summary>HP最大値</summary>
        public float HpMax;
        /// <summary>基礎攻撃力</summary>
        public float Attack;
        /// <summary>クリティカル確率</summary>
        public float CritRate;
        /// <summary>クリティカル倍率</summary>
        public float CritScale;
        /// <summary>装備中の装備ID</summary>
        public int[] Equipments;
        /// <summary>装備中のスキルID</summary>
        public int[] Skills;

        public PlayerStatus(int lv, long exp, long gold, float hpMax, float attack, float critRate, float critScale, int[] equipments, int[] skills)
        {
            Lv = lv;
            Exp = exp;
            Gold = gold;
            HpMax = hpMax;
            Attack = attack;
            CritRate = critRate;
            CritScale = critScale;
            Equipments = equipments;
            Skills = skills;
        }
    }
}