using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃に使うパラメータをまとめた構造体。
    /// </summary>
    public readonly struct AttackSpec
    {
        /// <summary>
        ///     攻撃関係のパラメータのインスタンスを初期化するコンストラクタ。
        /// </summary>
        /// <param name="criticalMultiplier"> クリティカルヒットのダメージ倍率。 </param>
        /// <param name="confirmedDamage"> 確定ダメージ量。 </param>
        public AttackSpec(
            CriticalMultiplier criticalMultiplier,
            Damage confirmedDamage
            )
        {
            CriticalMultiplier = criticalMultiplier;
            ConfirmedDamage = confirmedDamage;
        }

        /// <summary> クリティカルヒットのダメージ倍率。 </summary>
        public CriticalMultiplier CriticalMultiplier { get; }

        /// <summary> 確定ダメージ量を取得する。 </summary>
        public Damage ConfirmedDamage { get; }
    }
}

