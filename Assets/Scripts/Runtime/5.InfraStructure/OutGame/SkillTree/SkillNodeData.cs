using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.Attribute;
using System.Collections.Generic;
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
        /// <summary> ノードのIDを取得する。 </summary>
        public SkillNodeId NodeId => new SkillNodeId(_nodeId.Id);

        /// <summary> 親ノード数を取得する。 </summary>
        public int ParentNodeCount => _parentNodeIds == null ? 0 : _parentNodeIds.Length;
        /// <summary> 解放に必要なポイント </summary>
        public int UnlockCost;
        /// <summary> スキル説明文 </summary>
        [TextArea] public string SkillDetail;
        /// <summary> スキルプレビュー動画 </summary>
        public VideoClip PreviewVideoClip;

        [Header("ノードを解放した時の報酬")]
        [SerializeReference, SubclassSelector, Tooltip("ノードを解放した時に獲得するステータスボーナス効果。")]
        private IStatusBonusEffect[] _statusBonusEffects;
        [SerializeField, Tooltip("ノードを解放した時に解放されるスキル")]
        [SourceDataCollection("Skill")]
        private DataID[] _unlockSkills;

        /// <summary>
        ///     保持するデータよりEntityを生成する。
        /// </summary>
        /// <returns></returns>
        public SkillNodeEntity ToDomain()
        {
            int unlockSkills = _unlockSkills == null ? 0 : _unlockSkills.Length;
            List<SkillId> skillIds = new List<SkillId>(unlockSkills);
            for (var i = 0; i < unlockSkills; i++)
            {
                if (_unlockSkills[i].Id == 0)
                {
                    continue;
                }

                skillIds.Add(new SkillId(_unlockSkills[i].Id));
            }

            return new SkillNodeEntity(
                NodeId,
                UnlockCost,
                SkillDetail,
                skillIds.ToArray(),
                _statusBonusEffects);
        }

        /// <summary>
        ///     指定した位置の親ノードIDを取得する。
        /// </summary>
        /// <param name="index"> 親ノード配列の位置。 </param>
        /// <returns> 親ノードID。 </returns>
        public SkillNodeId GetParentNodeId(int index)
        {
            return new SkillNodeId(_parentNodeIds[index].Id);
        }

        [SerializeField, Tooltip("ノードを一意に識別するID。")]
        [SourceDataCollection("SkillNode")]
        private DataID _nodeId;

        [SerializeField, Tooltip("親ノードのID一覧。")]
        [SourceDataCollection("SkillNode")]
        private DataID[] _parentNodeIds;
    }
}
