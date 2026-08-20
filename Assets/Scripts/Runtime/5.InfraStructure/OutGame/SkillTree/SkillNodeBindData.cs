using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードと、対応するUI Toolkit側の要素名、
    ///     来ている方向の接続線要素名、行く方向の接続線要素名の紐づきを格納する。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeBindData", menuName = "SymphonyDev/SkillTree/SkillNodeBindData")]
    public class SkillNodeBindData : ScriptableObject
    {
        /// <summary> UI Toolkit上の要素名 </summary>
        public string NodeName;
        /// <summary> TODO 将来的ここは本番のスキルSOを入れる </summary>
        public SkillNodeData SkillNodeData;
        /// <summary> 来ている方向の接続線の要素名 </summary>
        public string[] FromConnNames;
        /// <summary> 次に行ける接続線の要素名 </summary>
        public string[] ToConnNames;
    }
}