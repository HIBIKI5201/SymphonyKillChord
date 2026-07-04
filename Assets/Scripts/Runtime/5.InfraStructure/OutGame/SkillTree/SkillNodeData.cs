using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.Player;
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

        [Header("ノードを解放した時の報酬")]
        //[SerializeReference, SubclassSelector] private IParameterUpgradeEffect[] _nodeUnlockEffets;
        [SerializeField, Tooltip("ノードを解放した時に解放されるスキル")]
        private SkillDataAsset[] _unlockSkills;

        /// <summary>
        ///     保持するデータよりEntityを生成する。
        /// </summary>
        /// <returns></returns>
        public SkillNodeEntity ToDomain()
        {
            // UnlockSkillId の配列の長さを取得する
            // _unlockSkills が null の場合は 0 を返す
            int unlockSkills = _unlockSkills == null ? 0 : _unlockSkills.Length;

            // UnlockSkillId の配列を生成する
            var skillIds = new UnlockSkillId[unlockSkills];
            for (var i = 0; i < unlockSkills; i++)
            {
                skillIds[i] = new UnlockSkillId(_unlockSkills[i].Id);
            }

            return new SkillNodeEntity(NodeId, UnlockCost, SkillDetail, skillIds);
        }
    }
}