using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Utility.Identity;
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
        /// <summary> 段階を解放するために必要なノードID。 </summary>
        public SkillNodeId RequiredSkillNodeId => new SkillNodeId(_requiredSkillNodeId.Id);
        /// <summary>
        ///     全ノードの解放で段階を開くか。trueの場合は <see cref="RequiredSkillNodeId"/> を使わない。
        /// </summary>
        public bool RequiresAllNodesUnlocked => _requiresAllNodesUnlocked;

        [SerializeField, Tooltip("段階を解放するために必要なノードID。")]
        [SourceDataCollection("SkillNode")]
        private DataID _requiredSkillNodeId;

        [SerializeField, Tooltip("全ノード（全ノードの解放が条件のノードを除く）の解放で段階を開くか。trueの場合は必要ノードIDを使わない。")]
        private bool _requiresAllNodesUnlocked;
    }
}
