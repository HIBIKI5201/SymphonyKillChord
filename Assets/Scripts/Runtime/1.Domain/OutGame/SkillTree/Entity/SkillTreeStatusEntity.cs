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
        public SkillTreeStatusEntity(int currentPoints, int[] unlockedNodes)
        {
            _currentPoints = currentPoints;
            _unlockedNodes = new();
            if(unlockedNodes != null)
            {
                _unlockedNodes.AddRange(unlockedNodes);
            }
        }

        public int CurrentPoints => _currentPoints;
        public List<int> UnlockedNodes => _unlockedNodes;

        public void ModifyPoint(int amount)
        {
            _currentPoints += amount;
        }

        public void AddUnlockedNodes(int[] nodeIds)
        {
            _unlockedNodes.AddRange(nodeIds);
        }

        private int _currentPoints;
        private List<int> _unlockedNodes;
    }
}
