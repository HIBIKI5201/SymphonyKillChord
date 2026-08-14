namespace KillChord.Runtime.Domain.InGame.Battle
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
        /// <param name="finalDamage"> 最終的なダメージ量。 </param>
        /// <param name="isCritical"> クリティカルヒットかどうかを示すフラグ。 </param>
        /// <param name="appliedDamage"> 実際に適用されたダメージ量。 </param>
        /// <param name="barrierDamage"> バリアに吸収されたダメージ量。 </param>
        public AttackResult(
            Damage finalDamage,
            bool isCritical,
            Damage appliedDamage = default,
            Damage barrierDamage = default)
        {
            FinalDamage = finalDamage;
            AppliedDamage = appliedDamage;
            IsCritical = isCritical;
            BarrierDamage = barrierDamage;
        }

        /// <summary>
        ///     攻撃結果のインスタンスを初期化するコンストラクタ。
        ///     攻撃処理の文脈から値を抽出して初期化するためのコンストラクタ。
        /// </summary>
        /// <param name="attackStepContext"> 攻撃処理の文脈情報。 </param>
        public AttackResult(in AttackStepContext attackStepContext)
        {
            FinalDamage = attackStepContext.Damage;
            AppliedDamage = default;
            BarrierDamage = default;
            IsCritical = attackStepContext.CriticalCount > 0;
        }

        /// <summary> 最終的なダメージ量。 </summary>
        public Damage FinalDamage { get; }

        /// <summary> 実際に適用されたダメージ量。 </summary>
        public Damage AppliedDamage { get; }

        /// <summary> バリアに吸収されたダメージ量。 </summary>
        public Damage BarrierDamage { get; }

        /// <summary> クリティカルヒットかどうかを示すフラグを取得する。 </summary>
        public bool IsCritical { get; }

        /// <summary>
        ///     最終ダメージを変更した新しいAttackResultを返す。
        ///　 </summary>
        public AttackResult WithFinalDamage(Damage finalDamage)
        {
            return new AttackResult(finalDamage, IsCritical, AppliedDamage, BarrierDamage);
        }

        /// <summary>
        ///     実適用ダメージを設定した新しいAttackResultを返す。
        /// </summary>
        /// <param name="appliedDamage"> 実際に適用されたダメージ量。 </param>
        /// <returns> 新しいAttackResultのインスタンス。 </returns>
        public AttackResult WithAppliedDamage(Damage appliedDamage)
        {
            return new AttackResult(FinalDamage, IsCritical, appliedDamage, BarrierDamage);
        }

        /// <summary>
        ///     バリアで吸収されたダメージを設定した新しいAttackResultを返す。
        /// </summary>
        /// <param name="barrierDamage"> バリアで吸収されたダメージ量。 </param>
        /// <returns> 新しいAttackResultのインスタンス。 </returns>
        public AttackResult WithBarrierDamage(Damage barrierDamage)
        {
            return new AttackResult(FinalDamage, IsCritical, AppliedDamage, barrierDamage);
        }
    }
}
