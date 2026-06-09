namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキル詳細にデータを渡すためのDTO。
    /// </summary>
    public readonly ref struct SkillDetailDTO
    {
        public SkillDetailDTO(int skillnodeId, string skillDetail, int unlockCost, bool canUnlock, bool unlocked)
        {
            SkillNodeId = skillnodeId;
            SkillDetail = skillDetail;
            UnlockCost = unlockCost;
            CanUnlock = canUnlock;
            Unlocked = unlocked;
        }
        /// <summary> スキルノードのID </summary>
        public readonly int SkillNodeId;
        /// <summary> スキルの詳細文 </summary>
        public readonly string SkillDetail;
        /// <summary> 解放するための必要ポイント </summary>
        public readonly int UnlockCost;
        /// <summary> 解放可否 </summary>
        public readonly bool CanUnlock;
        /// <summary> 解放済みか </summary>
        public readonly bool Unlocked;
    }
}
