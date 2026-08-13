using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     与えるダメージを一定倍率へ変更し続ける永続バフ。
    /// </summary>
    public class AttackPowerMultiplierBuff : BuffBase
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="multiplier"> ダメージ倍率。0で攻撃力無効化相当。 </param>
        public AttackPowerMultiplierBuff(float multiplier)
            : base(new BuffMetaData(BuffExecuteTiming.Attack_Logic_After, isPersistent: true))
        {
            _multiplier = Mathf.Max(0f, multiplier);
        }

        public override BuffContext ExecuteInstance(BuffContext context)
        {
            Damage multipliedDamage = context.AttackResult.FinalDamage * _multiplier;
            AttackResult multipliedResult = new AttackResult(multipliedDamage, context.AttackResult.IsCritical);
            return new BuffContext(context.Attacker, context.Target, multipliedResult);
        }

        private readonly float _multiplier;
    }
}
