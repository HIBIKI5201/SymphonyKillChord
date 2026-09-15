using System;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノード解放確認ダイアログにデータを渡すためのDTO。
    /// </summary>
    public readonly ref struct UnlockConfirmDTO
    {
        public UnlockConfirmDTO(
            string[] skillNames,
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
            SkillNames = skillNames ?? Array.Empty<string>();
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

        /// <summary> 解放時に併せて解放される全ノード分のスキル名一覧。スキルを解放するノードが無ければ空配列。 </summary>
        public readonly string[] SkillNames;
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
