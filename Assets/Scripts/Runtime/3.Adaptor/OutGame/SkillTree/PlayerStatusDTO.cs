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
            float areaAttackRangeMultiplier,
            float previewPlayerHealth,
            float previewPlayerAttack,
            float previewCriticalChance,
            float previewCriticalDamage,
            float previewAreaAttackRangeMultiplier)
        {
            PlayerHealth = playerHealth;
            PlayerAttack = playerAttack;
            CriticalChance = criticalChance;
            CriticalDamage = criticalDamage;
            AreaAttackRangeMultiplier = areaAttackRangeMultiplier;
            PreviewPlayerHealth = previewPlayerHealth;
            PreviewPlayerAttack = previewPlayerAttack;
            PreviewCriticalChance = previewCriticalChance;
            PreviewCriticalDamage = previewCriticalDamage;
            PreviewAreaAttackRangeMultiplier = previewAreaAttackRangeMultiplier;
        }
        public readonly float PlayerHealth;
        public readonly float PlayerAttack;
        public readonly float CriticalChance;
        public readonly float CriticalDamage;
        public readonly float AreaAttackRangeMultiplier;
        /// <summary> 選択中ノードを解放した場合のHP(変化しない場合は PlayerHealth と同値)。 </summary>
        public readonly float PreviewPlayerHealth;
        /// <summary> 選択中ノードを解放した場合の攻撃力(変化しない場合は PlayerAttack と同値)。 </summary>
        public readonly float PreviewPlayerAttack;
        /// <summary> 選択中ノードを解放した場合の会心率(変化しない場合は CriticalChance と同値)。 </summary>
        public readonly float PreviewCriticalChance;
        /// <summary> 選択中ノードを解放した場合の会心ダメージ(変化しない場合は CriticalDamage と同値)。 </summary>
        public readonly float PreviewCriticalDamage;
        /// <summary> 選択中ノードを解放した場合の射程範囲倍率(変化しない場合は AreaAttackRangeMultiplier と同値)。 </summary>
        public readonly float PreviewAreaAttackRangeMultiplier;
    }
}
