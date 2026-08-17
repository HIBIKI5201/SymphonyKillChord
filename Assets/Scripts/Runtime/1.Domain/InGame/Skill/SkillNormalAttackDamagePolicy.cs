namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキル発動時の通常攻撃ダメージ適用ポリシーを表す列挙型。
    /// </summary>
    public enum SkillNormalAttackDamagePolicy
    {
        /// <summary> 通常攻撃のダメージを適用する。 </summary>
        Apply = 0,

        /// <summary> 通常攻撃のダメージを適用しない。 </summary>
        Skip = 1
    }
}
