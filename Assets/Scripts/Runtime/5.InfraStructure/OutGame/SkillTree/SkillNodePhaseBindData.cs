using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     解放段階と各段階の必要ノードの紐づきを保持する。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodePhaseBindData", menuName = "SymphonyDev/SkillTree/SkillNodePhaseBindData")]
    public class SkillNodePhaseBindData : ScriptableObject
    {
        /// <summary> 段階のインデックス </summary>
        public int PhaseIndex;
        /// <summary> 段階のコンテナーの要素名 </summary>
        public string PhaseName;
        /// <summary> 段階を解放するために必要なノードID </summary>
        public int RequiredSkillNodeId;
    }
}
