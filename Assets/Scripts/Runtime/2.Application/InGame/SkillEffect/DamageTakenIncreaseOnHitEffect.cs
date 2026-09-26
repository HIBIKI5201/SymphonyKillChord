using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.InGame.SkillEffect
{
    /// <summary>
    ///     攻撃が命中した際に、対象の受けるダメージを増加させる状態効果を付与する攻撃ヒット効果。
    /// </summary>
    public class DamageTakenIncreaseOnHitEffect : IAttackHitEffect
    {
        public DamageTakenIncreaseOnHitEffect(float increaseRate, float durationSeconds, StatusEffectReapplyPolicy reapplyPolicy)
        {
            _increaseRate = increaseRate;
            _durationSeconds = durationSeconds;
            _reapplyPolicy = reapplyPolicy;
        }

        /// <inheritdoc />
        public void Apply(IAttacker attacker, IDefender defender, in AttackResult attackResult)
        {
            // 同じdefenderに対しては、同じ攻撃で複数回適用されないようにする
            if (!_appliedDefeners.Add(defender))
            {
                return;
            }

            defender.StatusEffectSystem.Add(
                new DamageTakenIncreaseDebuff(_increaseRate, _durationSeconds, _reapplyPolicy));
        }

        private readonly float _increaseRate;
        private readonly float _durationSeconds;
        private readonly StatusEffectReapplyPolicy _reapplyPolicy;

        private readonly HashSet<IDefender> _appliedDefeners = new();
    }
}
