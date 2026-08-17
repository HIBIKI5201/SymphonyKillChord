using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Target;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.InGame.SkillEffect
{
    /// <summary>
    ///     攻撃が命中した際に、指定した範囲内のターゲットに伝染デバフを付与する攻撃ヒットエフェクトです。
    /// </summary>
    public class InfectionOnHitEffect : IAttackHitEffect
    {
        public InfectionOnHitEffect(
            ITargetRadiusQuery targetRadiusQuery,
            AttackDefinition attackDefinition,
            float range,
            int triggerCount,
            float damageRate,
            StatusEffectReapplyPolicy reapplyPolicy)
        {
            _targetRadiusQuery = targetRadiusQuery ?? throw new System.ArgumentNullException(nameof(targetRadiusQuery));
            _attackDefinition = attackDefinition ?? throw new System.ArgumentNullException(nameof(attackDefinition));
            _range = range;
            _triggerCount = triggerCount;
            _damageRate = damageRate;
            _reapplyPolicy = reapplyPolicy;
        }

        /// <inheritdoc/>
        public void Apply(IAttacker attacker, IDefender defender, in AttackResult attackResult)
        {
            if (_isApplied || defender is not CharacterEntity hitTatget)
            {
                return;
            }

            _isApplied = true;
            _targetRadiusQuery.Query(hitTatget, _range, _targets);

            if (_targets.Count == 0)
            {
                return;
            }

            InfectionGroup group = new InfectionGroup(
                attacker, _attackDefinition, _damageRate, _triggerCount);

            // ターゲットに感染デバフを付与し、グループに追加する
            for (int i = 0; i < _targets.Count; i++)
            {
                CharacterEntity target = _targets[i];

                if (target == null || target.IsDead)
                {
                    continue;
                }

                InfectionDebuff effect = new InfectionDebuff(target, group, _reapplyPolicy);
                target.StatusEffectSystem.Add(effect);
                group.AddMember(target, effect);
            }
        }

        private readonly ITargetRadiusQuery _targetRadiusQuery;
        private readonly AttackDefinition _attackDefinition;
        private readonly float _range;
        private readonly int _triggerCount;
        private readonly float _damageRate;
        private readonly StatusEffectReapplyPolicy _reapplyPolicy;
        private readonly List<CharacterEntity> _targets = new();

        private bool _isApplied;
    }
}
