using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     被ダメージを一定割合軽減し続ける永続バフ。
    /// </summary>
    public class DamageReductionBuff : BuffBase
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="reductionRate"> 軽減割合。1で被ダメージ0(無敵相当)。 </param>
        public DamageReductionBuff(float reductionRate)
            : base(new BuffMetaData(BuffExecuteTiming.Defense_Logic_Before, isPersistent: true))
        {
            _reductionRate = Mathf.Clamp01(reductionRate);
        }

        public override BuffContext ExecuteInstance(BuffContext context)
        {
            Damage reducedDamage = context.AttackResult.FinalDamage * (1f - _reductionRate);
            AttackResult reducedResult = new AttackResult(reducedDamage, context.AttackResult.IsCritical);
            return new BuffContext(context.Attacker, context.Target, reducedResult);
        }

        private readonly float _reductionRate;
    }
}
