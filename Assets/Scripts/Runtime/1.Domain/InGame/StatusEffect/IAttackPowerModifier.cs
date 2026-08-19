using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     攻撃力を補正する状態効果のインターフェースです。
    /// </summary>
    public interface IAttackPowerModifier
    {
        /// <summary>
        ///     攻撃力を補正します。
        /// </summary>
        /// <param name="attacker"> 攻撃者 </param>
        /// <param name="defender"> 防御者 </param>
        /// <param name="attackPower"> 元の攻撃力 </param>
        /// <returns> 補正後の攻撃力 </returns>
        Damage ModifyAttackPower(IAttacker attacker, IDefender defender, Damage attackPower);
    }
}
