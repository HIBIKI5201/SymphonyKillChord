using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     攻撃命中後に発動する効果を表すインターフェース。
    /// </summary>
    public interface IAttackHitEffect
    {
        /// <summary>
        ///     攻撃がヒットした際の効果を適用する。
        /// </summary>
        /// <param name="attacker"> 攻撃者。 </param>
        /// <param name="defender"> 防御者。 </param>
        /// <param name="attackResult"> 攻撃の結果。 </param>
        void Apply(IAttacker attacker, IDefender defender, in AttackResult attackResult);
    }
}
