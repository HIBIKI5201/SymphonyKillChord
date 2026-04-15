namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃処理の結果を表す構造体。
    /// </summary>
    public readonly struct AttackResult
    {
        /// <summary>
        ///     攻撃結果のインスタンスを初期化するコンストラクタ。
        ///     値を直接指定して初期化するためのコンストラクタ。
        /// </summary>
        /// <param name="finalDamage"></param>
        /// <param name="isCritical"></param>
        public AttackResult(Damage finalDamage, bool isCritical)
        {
            FinalDamage = finalDamage;
            IsCritical = isCritical;
        }

        /// <summary>
        ///     攻撃結果のインスタンスを初期化するコンストラクタ。
        ///     攻撃処理の文脈から値を抽出して初期化するためのコンストラクタ。
        /// </summary>
        /// <param name="attackStepContext"></param>
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
