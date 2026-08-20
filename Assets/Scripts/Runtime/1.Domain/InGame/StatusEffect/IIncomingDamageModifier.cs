using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     被ダメージを補正する状態効果。
    /// </summary>
    public interface IIncomingDamageModifier
    {
        /// <summary>
        ///     被ダメージを補正する。
        /// </summary>
        /// <param name="attacker"> 攻撃者。 </param>
        /// <param name="defender"> 防御者。 </param>
        /// <param name="attackResult"> 攻撃結果。 </param>
        /// <returns> 補正後の攻撃結果。 </returns>
        AttackResult ModifyIncomingDamage(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult);
    }
}
