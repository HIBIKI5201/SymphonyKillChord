namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃処理の結果を表す構造体。
    /// </summary>
    public readonly struct AttackResult
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="finalDamage"></param>
        /// <param name="isCritical"></param>
        public AttackResult(Damage finalDamage, bool isCritical)
        {
            FinalDamage = finalDamage;
            IsCritical = isCritical;
        }

        public AttackResult(in AttackStepContext attackStepContext)
        {
            FinalDamage = attackStepContext.Damage;
            IsCritical = attackStepContext.CriticalCount > 0;
        }

        /// <summary> 最終的なダメージ量。 </summary>
        public Damage FinalDamage { get; }
        /// <summary> クリティカルヒットかどうかを示すフラグ。 </summary>
        public bool IsCritical { get; }
    }
}
