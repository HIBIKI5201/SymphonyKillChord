using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃に使うパラメータをまとめた構造体。
    /// </summary>
    public readonly struct AttackParameterSet
    {
        /// <summary>
        ///     攻撃関係のパラメータのインスタンスを初期化するコンストラクタ。
        /// </summary>
        /// <param name="criticalChance"></param>
        /// <param name="criticalDamage"></param>
        /// <param name="confirmedDamage"></param>
        public AttackParameterSet(
            CriticalChance criticalChance,
            CriticalMultiplier criticalMultiplier,
            Damage confirmedDamage
            )
        {
            CriticalChance = criticalChance;
            CriticalMultiplier = criticalMultiplier;
            ConfirmedDamage = confirmedDamage;
        }

        /// <summary> クリティカルヒットの確率。 </summary>
        public CriticalChance CriticalChance { get; }
        /// <summary> クリティカルヒットのダメージ倍率。 </summary>
        public CriticalMultiplier CriticalMultiplier { get; }
        public Damage ConfirmedDamage { get; }
    }
}
