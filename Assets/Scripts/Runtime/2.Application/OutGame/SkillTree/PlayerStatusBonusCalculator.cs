using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.SkillTree
{
    /// <summary>
    ///     解放済みスキルノードからプレイヤーステータスボーナスを集計する。
    /// </summary>
    public sealed class PlayerStatusBonusCalculator
    {
        /// <summary>
        ///     集計対象となるスキルノードを設定する。
        /// </summary>
        /// <param name="skillNodes"> スキルノード一覧。 </param>
        public PlayerStatusBonusCalculator(IReadOnlyCollection<SkillNodeEntity> skillNodes)
        {
            if (skillNodes == null)
            {
                throw new ArgumentNullException(nameof(skillNodes));
            }

            _skillNodes = new Dictionary<SkillNodeId, SkillNodeEntity>(skillNodes.Count);
            foreach (SkillNodeEntity skillNode in skillNodes)
            {
                if (skillNode == null)
                {
                    continue;
                }

                _skillNodes[skillNode.SkillNodeIdVO] = skillNode;
            }
        }

        /// <summary>
        ///     解放済みノードIDに対応するステータスボーナス効果を集計する。
        ///     同じIDが複数回含まれる場合は強化回数として扱い、ノードの強化上限までの回数分を適用する。
        /// </summary>
        /// <param name="unlockedNodeIds"> 解放済みノードID。強化回数分だけ同じIDを含む。 </param>
        /// <returns> 集計済みのプレイヤーステータスボーナス。 </returns>
        public PlayerStatusBonus Calculate(IEnumerable<SkillNodeId> unlockedNodeIds)
        {
            if (unlockedNodeIds == null)
            {
                throw new ArgumentNullException(nameof(unlockedNodeIds));
            }

            // ノードごとの強化回数を数える。
            Dictionary<SkillNodeId, int> levelByNodeId = new Dictionary<SkillNodeId, int>();
            foreach (SkillNodeId nodeId in unlockedNodeIds)
            {
                levelByNodeId.TryGetValue(nodeId, out int level);
                levelByNodeId[nodeId] = level + 1;
            }

            // 強化上限までの回数分、効果を適用する。
            PlayerStatusBonusBuilder builder = new PlayerStatusBonusBuilder();
            foreach (KeyValuePair<SkillNodeId, int> pair in levelByNodeId)
            {
                if (!_skillNodes.TryGetValue(pair.Key, out SkillNodeEntity skillNode))
                {
                    continue;
                }

                int appliedLevel = skillNode.ClampLevel(pair.Value);
                for (int i = 0; i < appliedLevel; i++)
                {
                    skillNode.ApplyStatusBonusEffects(builder);
                }
            }

            return builder.Build();
        }

        private readonly Dictionary<SkillNodeId, SkillNodeEntity> _skillNodes;
    }
}
