using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードのEntity。
    /// </summary>
    public class SkillNodeEntity
    {
        public SkillNodeEntity(int nodeId,
            int cost, string skillDetail)
        {
            SkillNodeIdVO = new SkillNodeId(nodeId);
            UnlockCost = new UnlockCost(cost);
            _parents = null;
            SkillDetail = skillDetail;
        }
        /// <summary> ノードのID </summary>
        public SkillNodeId SkillNodeIdVO { get; }
        /// <summary> 解放に必要なコスト </summary>
        public UnlockCost UnlockCost { get; }
        /// <summary> スキルの詳細文 </summary>
        public string SkillDetail { get; }
        /// <summary> 解放されているか </summary>
        public bool IsUnlocked => _isUnlocked;

        /// <summary> 親ノード </summary>
        public SkillNodeEntity[] Parents => _parents;

        /// <summary>
        ///     親ノードを設定する
        /// </summary>
        /// <param name="parents"></param>
        public void SetParent(SkillNodeEntity[] parents)
        {
            _parents = parents;
        }

        /// <summary>
        ///     ノードを解放済みにする。
        /// </summary>
        public void Unlock()
        {
            _isUnlocked = true;
        }

        private bool _isUnlocked = false;
        private SkillNodeEntity[] _parents;
    }
}
