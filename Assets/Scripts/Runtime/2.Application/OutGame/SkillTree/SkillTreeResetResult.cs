using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;

namespace KillChord.Runtime.Application.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリーのリセット結果を保持する。
    /// </summary>
    public readonly struct SkillTreeResetResult
    {
        /// <summary>
        ///     リセット結果を生成する。
        /// </summary>
        /// <param name="refundedPoints"> 返却した研究ポイント。 </param>
        /// <param name="currentPoints"> リセット後の研究ポイント。 </param>
        /// <param name="unlockedNodeIds"> リセット後の解放済みノード。 </param>
        /// <param name="unlockedSkillIds"> リセット後の所持スキル。 </param>
        public SkillTreeResetResult(
            int refundedPoints,
            int currentPoints,
            SkillNodeId[] unlockedNodeIds,
            SkillId[] unlockedSkillIds)
        {
            RefundedPoints = refundedPoints;
            CurrentPoints = currentPoints;
            UnlockedNodeIds = unlockedNodeIds ?? Array.Empty<SkillNodeId>();
            UnlockedSkillIds = unlockedSkillIds ?? Array.Empty<SkillId>();
        }

        /// <summary> 返却した研究ポイント。 </summary>
        public int RefundedPoints { get; }

        /// <summary> リセット後の研究ポイント。 </summary>
        public int CurrentPoints { get; }

        /// <summary> リセット後の解放済みノード。 </summary>
        public ReadOnlyMemory<SkillNodeId> UnlockedNodeIds { get; }

        /// <summary> リセット後の所持スキル。 </summary>
        public ReadOnlyMemory<SkillId> UnlockedSkillIds { get; }
    }
}
