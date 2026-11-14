using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace KillChord.Tests.EditMode.SkillTree
{
    /// <summary>
    ///     ステージのクリアを条件にする研究ノード（3・6拍子のスキル）の解放判定を検証します。
    /// </summary>
    public sealed class SkillTreeServiceStageConditionTests
    {
        /// <summary>
        ///     条件のステージが未クリアのノードは解放できないことを確認します。
        /// </summary>
        [Test]
        public void TryGetTotalCost_StageNotCleared_ReturnsMinusOne()
        {
            SkillTreeService service = CreateService(Array.Empty<StageId>(), out _, out SkillNodeEntity locked, out _);
            HashSet<SkillNodeEntity> nodesOnPath = new();

            Assert.That(service.TryGetTotalCost(locked.SkillNodeIdVO, nodesOnPath), Is.EqualTo(-1));
            Assert.That(nodesOnPath, Is.Empty);
        }

        /// <summary>
        ///     条件のノードを経路に含む子ノードも解放できないことを確認します。
        /// </summary>
        [Test]
        public void TryGetTotalCost_ChildOfLockedNode_ReturnsMinusOne()
        {
            SkillTreeService service = CreateService(Array.Empty<StageId>(), out _, out _, out SkillNodeEntity child);

            Assert.That(service.TryGetTotalCost(child.SkillNodeIdVO, new HashSet<SkillNodeEntity>()), Is.EqualTo(-1));
        }

        /// <summary>
        ///     条件のステージをクリア済みなら通常どおり解放コストを返すことを確認します。
        /// </summary>
        [Test]
        public void TryGetTotalCost_StageCleared_ReturnsCost()
        {
            SkillTreeService service = CreateService(new[] { UNLOCK_STAGE_ID }, out _, out SkillNodeEntity locked, out _);

            Assert.That(service.TryGetTotalCost(locked.SkillNodeIdVO, new HashSet<SkillNodeEntity>()), Is.EqualTo(LOCKED_COST));
        }

        private const int LOCKED_COST = 10;
        private static readonly StageId UNLOCK_STAGE_ID = new(-562443898);

        /// <summary>
        ///     解放済みの起点、ステージ条件付きのノード、その子の3つからなるツリーを作ります。
        /// </summary>
        /// <param name="clearedStageIds"> クリア済みのステージIDです。 </param>
        /// <param name="root"> 解放済みの起点ノードです。 </param>
        /// <param name="locked"> ステージ条件付きのノードです。 </param>
        /// <param name="child"> ステージ条件付きのノードの子です。 </param>
        /// <returns> スキルツリーのサービスです。 </returns>
        private static SkillTreeService CreateService(
            IReadOnlyCollection<StageId> clearedStageIds,
            out SkillNodeEntity root,
            out SkillNodeEntity locked,
            out SkillNodeEntity child)
        {
            root = new SkillNodeEntity(new SkillNodeId(1), 0, string.Empty, Array.Empty<SkillId>());
            locked = new SkillNodeEntity(new SkillNodeId(2), LOCKED_COST, string.Empty, Array.Empty<SkillId>(),
                requiredClearedStageId: UNLOCK_STAGE_ID);
            child = new SkillNodeEntity(new SkillNodeId(3), 5, string.Empty, Array.Empty<SkillId>());
            locked.SetParent(new[] { root });
            child.SetParent(new[] { locked });
            root.Unlock();

            Dictionary<SkillNodeId, SkillNodeEntity> nodes = new()
            {
                { root.SkillNodeIdVO, root },
                { locked.SkillNodeIdVO, locked },
                { child.SkillNodeIdVO, child },
            };
            return new SkillTreeService(nodes, clearedStageIds: clearedStageIds);
        }
    }
}
