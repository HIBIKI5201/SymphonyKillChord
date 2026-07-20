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
        public PlayerStatusBonusCalculator(IEnumerable<SkillNodeEntity> skillNodes)
        {
            if (skillNodes == null)
            {
                throw new ArgumentNullException(nameof(skillNodes));
            }

            _skillNodes = new Dictionary<SkillNodeId, SkillNodeEntity>();
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
        /// </summary>
        /// <param name="unlockedNodeIds"> 解放済みノードID。 </param>
        /// <returns> 集計済みのプレイヤーステータスボーナス。 </returns>
        public PlayerStatusBonus Calculate(IEnumerable<SkillNodeId> unlockedNodeIds)
        {
            if (unlockedNodeIds == null)
            {
                throw new ArgumentNullException(nameof(unlockedNodeIds));
            }

            PlayerStatusBonusBuilder builder = new PlayerStatusBonusBuilder();
            HashSet<SkillNodeId> appliedNodeIds = new HashSet<SkillNodeId>();
            foreach (SkillNodeId nodeId in unlockedNodeIds)
            {
                if (!appliedNodeIds.Add(nodeId)
                    || !_skillNodes.TryGetValue(nodeId, out SkillNodeEntity skillNode))
                {
                    continue;
                }

                skillNode.ApplyStatusBonusEffects(builder);
            }

            return builder.Build();
        }

        private readonly Dictionary<SkillNodeId, SkillNodeEntity> _skillNodes;
    }
}
