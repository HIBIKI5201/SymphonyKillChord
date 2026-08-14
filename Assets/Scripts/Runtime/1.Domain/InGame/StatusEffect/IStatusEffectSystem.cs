using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     キャラクターが保持する状態効果を管理するシステムの共通インターフェース。
    /// </summary>
    public interface IStatusEffectSystem
    {
        /// <summary> 状態効果を追加します。 </summary>
        void Add(IStatusEffect statusEffect);

        /// <summary> 指定した状態効果を削除します。 </summary>
        void Remove(IStatusEffect statusEffect);

        /// <summary> すべての状態効果をクリアします。 </summary>
        void Clear();

        /// <summary> 与ダメージ補正を適用します。 </summary>
        AttackResult ApplyOutgoingDamageModifiers(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult);

        /// <summary> 被ダメージ補正を適用します。 </summary>
        AttackResult ApplyIncomingDamageModifiers(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult);

        /// <summary>
        ///     ダメージを与えたことを通知します。
        /// </summary>
        /// <param name="context"> ダメージが与えられた際のコンテキスト情報。 </param>
        void NotifyDamageDealt(in DamageDealtContext context);

        /// <summary>
        ///     ダメージを受けたことを通知します。
        /// </summary>
        /// <param name="context"> ダメージが受けられた際のコンテキスト情報。 </param>
        void NotifyDamageTaken(in DamageTakenContext context);

        /// <summary>
        ///     指定した状態効果を取得します。
        /// </summary>
        /// <param name="id"> 取得する状態効果のID </param>
        /// <param name="statusEffect"> 取得した状態効果 </param>
        /// <returns> 状態効果が存在する場合はtrue、それ以外の場合はfalse </returns>
        bool TryGet(StatusEffectId id, out IStatusEffect statusEffect);

        /// <summary>
        ///     クリティカルダメージ倍率補正を適用します。
        /// </summary>
        /// <param name="attacker"> 攻撃者です。 </param>
        /// <param name="defender"> 防御者です。 </param>
        /// <param name="criticalDamageMultiplier"> 現在のクリティカルダメージ倍率です。 </param>
        /// <returns> 修正後のクリティカルダメージ倍率です。 </returns>
        float ApplyCriticalDamageMultiplierModifiers(
            IAttacker attacker, IDefender defender, float criticalDamageMultiplier);
    }
}
