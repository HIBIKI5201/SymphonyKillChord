using KillChord.Runtime.Domain.InGame.Skill;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     【一時】スキルツリー関連の情報を保持するEntity。
    ///     TODO：将来は正式的なデータ格納クラスと連携する。
    /// </summary>
    public class SkillTreeStatusEntity
    {
        /// <summary>
        ///     スキルツリー状態を初期化する。
        /// </summary>
        /// <param name="currentPoints"> 現在の研究ポイント。 </param>
        /// <param name="unlockedNodes"> 解放済みノード。 </param>
        /// <param name="unlockedSkills"> 解放済みスキル。 </param>
        public SkillTreeStatusEntity(int currentPoints, SkillNodeId[] unlockedNodes, SkillId[] unlockedSkills)
        {
            _currentPoints = currentPoints;
            _unlockedNodes = new();
            if (unlockedNodes != null)
            {
                _unlockedNodes.AddRange(unlockedNodes);
            }

            _unlockedSkillIds = new();
            if (unlockedSkills != null)
            {
                _unlockedSkillIds.AddRange(unlockedSkills);
            }
        }

        /// <summary> 現在の研究ポイントを取得します。 </summary>
        public int CurrentPoints => _currentPoints;
        /// <summary> 解放されたノードの ID リストを取得します。 </summary>
        public List<SkillNodeId> UnlockedNodes => _unlockedNodes;
        /// <summary> 解放されたスキルの ID リストを取得します。 </summary>
        public List<SkillId> UnlockedSkillIds => _unlockedSkillIds;

        /// <summary>
        ///     研究ポイントを増減させます。
        /// </summary>
        /// <param name="amount"> 増減させるポイントの量。 正の値で増加、負の値で減少します。 </param>
        public void ModifyPoint(int amount)
        {
            _currentPoints += amount;
        }

        /// <summary>
        ///    解放されたノードの ID を追加します。
        /// </summary>
        /// <param name="nodeId"> 追加するノードの ID。 </param>
        public void AddUnlockedNode(SkillNodeId nodeId)
        {
            _unlockedNodes.Add(nodeId);
        }

        /// <summary>
        ///    解放されたスキルの ID を追加します。
        /// </summary>
        /// <param name="skillIds"> 追加するスキルの ID 配列。 </param>
        public void AddUnlockedSkillIds(SkillId[] skillIds)
        {
            _unlockedSkillIds.AddRange(skillIds);
        }

        /// <summary>
        ///     スキルツリーの解放状態と所持ポイントをリセット結果で置き換えます。
        /// </summary>
        /// <param name="currentPoints"> リセット後の研究ポイント。 </param>
        /// <param name="unlockedNodes"> リセット後も解放済みとして維持するノード。 </param>
        /// <param name="unlockedSkills"> リセット後も所持するスキル。 </param>
        public void Reset(
            int currentPoints,
            ReadOnlyMemory<SkillNodeId> unlockedNodes,
            ReadOnlyMemory<SkillId> unlockedSkills)
        {
            if (currentPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentPoints));
            }

            _currentPoints = currentPoints;

            _unlockedNodes.Clear();
            ReadOnlySpan<SkillNodeId> unlockedNodeSpan = unlockedNodes.Span;
            for (int i = 0; i < unlockedNodeSpan.Length; i++)
            {
                _unlockedNodes.Add(unlockedNodeSpan[i]);
            }

            _unlockedSkillIds.Clear();
            ReadOnlySpan<SkillId> unlockedSkillSpan = unlockedSkills.Span;
            for (int i = 0; i < unlockedSkillSpan.Length; i++)
            {
                _unlockedSkillIds.Add(unlockedSkillSpan[i]);
            }
        }

        private int _currentPoints;
        private List<SkillNodeId> _unlockedNodes;
        private List<SkillId> _unlockedSkillIds;
    }
}
