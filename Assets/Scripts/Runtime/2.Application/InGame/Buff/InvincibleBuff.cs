using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     被ダメージを防御力100%で防ぎ、無敵状態にする永続バフ。
    /// </summary>
    public class InvincibleBuff : StatusEffectBase, IIncomingDamageModifier
    {
        /// <summary>
        ///     再付与時の扱いを指定して生成する。
        /// </summary>
        public InvincibleBuff(StatusEffectReapplyPolicy reapplyPolicy = StatusEffectReapplyPolicy.Ignore)
            : base(
                EFFECT_ID,
                StatusEffectCategory.Buff,
                StatusEffectDuration.UntilRemoved,
                reapplyPolicy)
        {
        }

        /// <inheritdoc />
        public AttackResult ModifyIncomingDamage(
            IAttacker attacker,
            IDefender defender,
            AttackResult result)
        {
            Damage damage = result.FinalDamage * 0f;

            return result.WithFinalDamage(damage);
        }

        /// <summary>
        ///     状態効果の識別子。
        /// </summary>
        private static readonly StatusEffectId EFFECT_ID =
            new(nameof(InvincibleBuff));
    }
}
