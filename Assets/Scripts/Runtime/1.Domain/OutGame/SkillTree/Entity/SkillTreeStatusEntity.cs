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
        public SkillTreeStatusEntity(int currentPoints, int[] unlockedNodes, int[] unlockedSkills)
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
            if (unlockedNodes != null)
            {
                _unlockedNodes.AddRange(unlockedNodes);
            }
        }

        /// <summary> 現在の研究ポイントを取得します。 </summary>
        public int CurrentPoints => _currentPoints;
        /// <summary> 解放されたノードの ID リストを取得します。 </summary>
        public List<int> UnlockedNodes => _unlockedNodes;
        /// <summary> 解放されたスキルの ID リストを取得します。 </summary>
        public List<int> UnlockedSkillIds => _unlockedSkillIds;

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
        /// <param name="nodeIds"> 追加するノードの ID 配列。 </param>
        public void AddUnlockedNodes(int[] nodeIds)
        {
            _unlockedNodes.AddRange(nodeIds);
        }

        /// <summary>
        ///    解放されたスキルの ID を追加します。
        /// </summary>
        /// <param name="skillIds"> 追加するスキルの ID 配列。 </param>
        public void AddUnlockedSkillIds(int[] skillIds)
        {
            _unlockedSkillIds.AddRange(skillIds);
        }

        private int _currentPoints;
        private List<int> _unlockedNodes;
        private List<int> _unlockedSkillIds;
    }
}
