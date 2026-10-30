using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     【一時】動作確認用の、画面表示時初期データ。
    ///     TODO　今後はこれを捨て、セーブデータから取得するようにする。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTreeTestInputData", menuName = "SymphonyDev/SkillTree/SkillTreeTestInputData")]
    public class SkillTreeTestInputData : ScriptableObject
    {
        public int currentPoints;
        public int[] UnlockedSkillNodeIds;
        public int[] UnlockedSkillIds;
        public float PlayerHp;
        public float PlayerAttack;
        public float PlayerCriticalChance;
    }
}