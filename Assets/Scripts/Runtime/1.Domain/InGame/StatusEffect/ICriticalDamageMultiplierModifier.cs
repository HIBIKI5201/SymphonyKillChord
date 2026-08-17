using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     クリティカルダメージ倍率を修正する状態効果を表すインターフェース。
    /// </summary>
    public interface ICriticalDamageMultiplierModifier
    {
        /// <summary>
        ///    クリティカルダメージ倍率を修正する。
        /// </summary>
        /// <param name="attacker"> 攻撃者です。 </param>
        /// <param name="defender"> 防御者です。 </param>
        /// <param name="criticalDamageMultiplier"> 現在のクリティカルダメージ倍率です。 </param>
        /// <returns> 修正後のクリティカルダメージ倍率です。 </returns>
        float ModifyCriticalDamageMultiplier(
            IAttacker attacker, IDefender defender, float criticalDamageMultiplier);
    }
}
