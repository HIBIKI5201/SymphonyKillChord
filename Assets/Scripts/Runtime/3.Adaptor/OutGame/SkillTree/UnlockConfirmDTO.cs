namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノード解放確認ダイアログにデータを渡すためのDTO。
    /// </summary>
    public readonly ref struct UnlockConfirmDTO
    {
        public UnlockConfirmDTO(
            bool hasSkill,
            string skillName,
            int currentPoints,
            int cost,
            float playerHealth,
            float previewPlayerHealth,
            float playerAttack,
            float previewPlayerAttack,
            float criticalChance,
            float previewCriticalChance,
            float criticalDamage,
            float previewCriticalDamage,
            float areaAttackRangeMultiplier,
            float previewAreaAttackRangeMultiplier)
        {
            HasSkill = hasSkill;
            SkillName = skillName == null ? "" : skillName;
            CurrentPoints = currentPoints;
            Cost = cost;
            PlayerHealth = playerHealth;
            PreviewPlayerHealth = previewPlayerHealth;
            PlayerAttack = playerAttack;
            PreviewPlayerAttack = previewPlayerAttack;
            CriticalChance = criticalChance;
            PreviewCriticalChance = previewCriticalChance;
            CriticalDamage = criticalDamage;
            PreviewCriticalDamage = previewCriticalDamage;
            AreaAttackRangeMultiplier = areaAttackRangeMultiplier;
            PreviewAreaAttackRangeMultiplier = previewAreaAttackRangeMultiplier;
        }

        /// <summary> ノードがスキルを解放するか(falseの場合はステータス強化のみのノード)。 </summary>
        public readonly bool HasSkill;
        /// <summary> ノードが解放するスキルの名前。 </summary>
        public readonly string SkillName;
        /// <summary> 解放前の研究ポイント。 </summary>
        public readonly int CurrentPoints;
        /// <summary> 解放に必要な研究ポイント。 </summary>
        public readonly int Cost;
        public readonly float PlayerHealth;
        public readonly float PreviewPlayerHealth;
        public readonly float PlayerAttack;
        public readonly float PreviewPlayerAttack;
        public readonly float CriticalChance;
        public readonly float PreviewCriticalChance;
        public readonly float CriticalDamage;
        public readonly float PreviewCriticalDamage;
        public readonly float AreaAttackRangeMultiplier;
        public readonly float PreviewAreaAttackRangeMultiplier;
    }
}
