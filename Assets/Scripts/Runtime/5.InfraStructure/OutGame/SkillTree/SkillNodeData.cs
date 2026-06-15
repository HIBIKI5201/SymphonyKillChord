using KillChord.Runtime.Domain.OutGame.SkillTree;
using SymphonyFrameWork.Attribute;
using UnityEngine;
using UnityEngine.Video;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードに関する情報を格納する。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeData", menuName = "SymphonyDev/SkillTree/SkillNodeData")]
    public class SkillNodeData : ScriptableObject
    {
        /// <summary> ノードのID </summary>
        public int NodeId;
        /// <summary> 親ノードのID </summary>
        public int[] ParentNodeIds;
        /// <summary> 解放に必要なポイント </summary>
        public int UnlockCost;
        /// <summary> スキル説明文 </summary>
        [TextArea] public string SkillDetail;
        /// <summary> スキルプレビュー動画 </summary>
        public VideoClip PreviewVideoClip;

        //[Header("ノードを解放した時の報酬")]
        //[SerializeReference, SubclassSelector] private IParameterUpgradeEffect[] _nodeUnlockEffets;
        //[SerializeReference, SubclassSelector] private ISkillUnlockEffect[] _skillUnlockEffets;

        /// <summary>
        ///     保持するデータよりEntityを生成する。
        /// </summary>
        /// <returns></returns>
        public SkillNodeEntity ToDomain()
        {
            return new SkillNodeEntity(NodeId, UnlockCost, SkillDetail);
        }
    }
}