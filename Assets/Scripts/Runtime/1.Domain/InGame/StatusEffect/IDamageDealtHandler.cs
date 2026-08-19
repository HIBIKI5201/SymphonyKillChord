using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     ダメージを与えた際に処理を行う状態効果。
    /// </summary>
    public interface IDamageDealtHandler
    {
        /// <summary>
        ///     ダメージを与えた際の処理を行う。
        /// </summary>
        /// <param name="context"> ダメージ情報。 </param>
        void OnDamageDealt(in DamageDealtContext context);
    }
}
