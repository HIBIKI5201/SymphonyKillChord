using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Utility.Identity;
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
        /// <summary> 対応するスキルノードのIDを取得する。 </summary>
        public SkillNodeId SkillNodeId => new SkillNodeId(_skillNodeId.Id);

        /// <summary> UI Toolkit上の要素名 </summary>
        public string NodeName;
        /// <summary> 来ている方向の接続線の要素名 </summary>
        public string[] FromConnNames;
        /// <summary> 次に行ける接続線の要素名 </summary>
        public string[] ToConnNames;

        [SerializeField, Tooltip("対応するスキルノードのID。")]
        [SourceDataCollection("SkillNode")]
        private DataID _skillNodeId;
    }
}