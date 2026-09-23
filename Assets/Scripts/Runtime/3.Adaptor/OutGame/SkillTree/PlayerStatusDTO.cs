namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     【一時】プレイヤーステータス画面にデータを渡すためのDTO。
    ///     TODO　正式的なデータ構成に変更することが必要。
    /// </summary>
    public readonly ref struct PlayerStatusDTO
    {
        public PlayerStatusDTO(
            float playerHealth,
            float playerAttack,
            float criticalChance,
            float criticalDamage,
            float areaAttackRangeMultiplier)
        {
            PlayerHealth = playerHealth;
            PlayerAttack = playerAttack;
            CriticalChance = criticalChance;
            CriticalDamage = criticalDamage;
            AreaAttackRangeMultiplier = areaAttackRangeMultiplier;
        }
        public readonly float PlayerHealth;
        public readonly float PlayerAttack;
        public readonly float CriticalChance;
        public readonly float CriticalDamage;
        public readonly float AreaAttackRangeMultiplier;
    }
}
