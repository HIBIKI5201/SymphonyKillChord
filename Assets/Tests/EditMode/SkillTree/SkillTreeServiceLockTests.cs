using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace KillChord.Tests.EditMode.SkillTree
{
    /// <summary>
    ///     解放を禁止したスキルノードの扱いを検証します。
    /// </summary>
    public sealed class SkillTreeServiceLockTests
    {
        /// <summary>
        ///     禁止したノードは解放コストを求められず、経路も空になることを確認します。
        /// </summary>
        [Test]
        public void TryGetTotalCost_ReturnsMinusOneForLockedNode()
        {
            SkillTreeService service = CreateService(out _, LOCKED_NODE_ID);
            HashSet<SkillNodeEntity> nodesOnPath = new HashSet<SkillNodeEntity>();

            int cost = service.TryGetTotalCost(new SkillNodeId(LOCKED_NODE_ID), nodesOnPath);

            Assert.That(cost, Is.EqualTo(-1));
            Assert.That(nodesOnPath, Is.Empty);
        }

        /// <summary>
        ///     禁止したノードを経由する子ノードも、まとめて解放できないことを確認します。
        /// </summary>
        [Test]
        public void TryGetTotalCost_ReturnsMinusOneForChildOfLockedNode()
        {
            SkillTreeService service = CreateService(out _, LOCKED_NODE_ID);
            HashSet<SkillNodeEntity> nodesOnPath = new HashSet<SkillNodeEntity>();

            int cost = service.TryGetTotalCost(new SkillNodeId(LOCKED_CHILD_NODE_ID), nodesOnPath);

            Assert.That(cost, Is.EqualTo(-1));
            Assert.That(nodesOnPath, Is.Empty);
        }

        /// <summary>
        ///     禁止に関係しないノードは、通常どおり経路の合計コストを求められることを確認します。
        /// </summary>
        [Test]
        public void TryGetTotalCost_ReturnsCostForOpenNode()
        {
            SkillTreeService service = CreateService(out _, LOCKED_NODE_ID);
            HashSet<SkillNodeEntity> nodesOnPath = new HashSet<SkillNodeEntity>();

            int cost = service.TryGetTotalCost(new SkillNodeId(OPEN_NODE_ID), nodesOnPath);

            Assert.That(cost, Is.EqualTo(NODE_COST * 2));
            Assert.That(nodesOnPath.Count, Is.EqualTo(2));
        }

        /// <summary>
        ///     禁止が無い場合は、子ノードから親ノードまでまとめて解放できることを確認します。
        /// </summary>
        [Test]
        public void TryGetTotalCost_WithoutPolicy_ReturnsPathCost()
        {
            SkillTreeService service = CreateService(out _);
            HashSet<SkillNodeEntity> nodesOnPath = new HashSet<SkillNodeEntity>();

            int cost = service.TryGetTotalCost(new SkillNodeId(LOCKED_CHILD_NODE_ID), nodesOnPath);

            Assert.That(cost, Is.EqualTo(NODE_COST * 3));
        }

        /// <summary>
        ///     禁止したノードとその先を除き、残りが解放済みなら全ノード解放とみなすことを確認します。
        /// </summary>
        [Test]
        public void AreAllNodesUnlocked_IgnoresLockedPath()
        {
            SkillTreeService service = CreateService(
                out Dictionary<SkillNodeId, SkillNodeEntity> nodes,
                LOCKED_NODE_ID);
            nodes[new SkillNodeId(ROOT_NODE_ID)].Unlock();
            nodes[new SkillNodeId(OPEN_NODE_ID)].Unlock();

            Assert.That(service.AreAllNodesUnlocked(), Is.True);

            HashSet<SkillNodeEntity> nodesOnPath = new HashSet<SkillNodeEntity>();
            int finalCost = service.TryGetTotalCost(new SkillNodeId(FINAL_NODE_ID), nodesOnPath);
            Assert.That(finalCost, Is.EqualTo(NODE_COST));
        }

        private const int ROOT_NODE_ID = 1;
        private const int LOCKED_NODE_ID = 2;
        private const int LOCKED_CHILD_NODE_ID = 3;
        private const int OPEN_NODE_ID = 4;
        private const int FINAL_NODE_ID = 5;
        private const int NODE_COST = 10;

        /// <summary>
        ///     起点→禁止ノード→子ノード、起点→通常ノード、全解放条件ノードの木を作ります。
        /// </summary>
        /// <param name="nodes"> 作成したノード一覧です。 </param>
        /// <param name="lockedNodeIds"> 解放を禁止するノードIDです。空の場合は禁止しません。 </param>
        /// <returns> 作成したサービスです。 </returns>
        private static SkillTreeService CreateService(
            out Dictionary<SkillNodeId, SkillNodeEntity> nodes,
            params int[] lockedNodeIds)
        {
            SkillNodeEntity root = CreateNode(ROOT_NODE_ID);
            SkillNodeEntity locked = CreateNode(LOCKED_NODE_ID);
            SkillNodeEntity lockedChild = CreateNode(LOCKED_CHILD_NODE_ID);
            SkillNodeEntity open = CreateNode(OPEN_NODE_ID);
            SkillNodeEntity final = CreateNode(FINAL_NODE_ID, true);
            locked.SetParent(new[] { root });
            lockedChild.SetParent(new[] { locked });
            open.SetParent(new[] { root });
            final.SetParent(new[] { open });

            nodes = new Dictionary<SkillNodeId, SkillNodeEntity>
            {
                { root.SkillNodeIdVO, root },
                { locked.SkillNodeIdVO, locked },
                { lockedChild.SkillNodeIdVO, lockedChild },
                { open.SkillNodeIdVO, open },
                { final.SkillNodeIdVO, final },
            };

            ISkillNodeLockPolicy policy = lockedNodeIds.Length == 0
                ? null
                : new FixedLockPolicy(lockedNodeIds);
            return new SkillTreeService(nodes, policy);
        }

        /// <summary>
        ///     テスト用のノードを作ります。
        /// </summary>
        /// <param name="id"> ノードIDです。 </param>
        /// <param name="requiresAllNodesUnlocked"> 全ノード解放を条件にする場合はtrueです。 </param>
        /// <returns> 作成したノードです。 </returns>
        private static SkillNodeEntity CreateNode(int id, bool requiresAllNodesUnlocked = false)
        {
            return new SkillNodeEntity(
                new SkillNodeId(id),
                NODE_COST,
                string.Empty,
                Array.Empty<SkillId>(),
                requiresAllNodesUnlocked: requiresAllNodesUnlocked);
        }

        /// <summary>
        ///     指定IDだけを禁止するテスト用ポリシーです。
        /// </summary>
        private sealed class FixedLockPolicy : ISkillNodeLockPolicy
        {
            /// <summary>
            ///     ポリシーを生成します。
            /// </summary>
            /// <param name="lockedNodeIds"> 禁止するノードIDです。 </param>
            public FixedLockPolicy(int[] lockedNodeIds)
            {
                _lockedNodeIds = new HashSet<int>(lockedNodeIds);
            }

            /// <summary>
            ///     指定ノードの解放を禁止するか判定します。
            /// </summary>
            /// <param name="nodeId"> 判定するノードIDです。 </param>
            /// <returns> 禁止する場合はtrueです。 </returns>
            public bool IsLocked(SkillNodeId nodeId)
            {
                return _lockedNodeIds.Contains(nodeId.Id);
            }

            private readonly HashSet<int> _lockedNodeIds;
        }
    }
}
