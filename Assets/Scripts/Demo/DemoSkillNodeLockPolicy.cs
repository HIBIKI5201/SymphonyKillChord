using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;
using System.Collections.Generic;

namespace KillChord.Demo
{
    /// <summary>
    ///     体験版設定で指定したスキルノードの解放を禁止します。
    /// </summary>
    public sealed class DemoSkillNodeLockPolicy : ISkillNodeLockPolicy
    {
        /// <summary>
        ///     ポリシーを生成します。
        /// </summary>
        /// <param name="lockedNodeIdValues"> 解放を禁止するノードIDの数値一覧です。 </param>
        public DemoSkillNodeLockPolicy(IReadOnlyList<int> lockedNodeIdValues)
        {
            if (lockedNodeIdValues == null)
            {
                throw new ArgumentNullException(nameof(lockedNodeIdValues));
            }

            _lockedNodeIdValues = new HashSet<int>(lockedNodeIdValues);
        }

        /// <summary> 解放を禁止するノードが1つ以上ある場合はtrueです。 </summary>
        public bool HasLockedNodes => _lockedNodeIdValues.Count > 0;

        /// <summary>
        ///     指定ノードの解放を禁止するか判定します。
        /// </summary>
        /// <param name="nodeId"> 判定するノードIDです。 </param>
        /// <returns> 体験版設定で指定されたノードの場合はtrueです。 </returns>
        public bool IsLocked(SkillNodeId nodeId)
        {
            return _lockedNodeIdValues.Contains(nodeId.Id);
        }

        private readonly HashSet<int> _lockedNodeIdValues;
    }
}
