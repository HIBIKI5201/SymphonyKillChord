using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     攻撃の計算を行う静的クラス。
    /// </summary>
    public static class AttackCalculator
    {
        /// <summary>
        ///     計算処理を行い、攻撃の結果を返す。
        /// </summary>
        /// <param name="attackDefinition"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <param name="baseDamage"> 攻撃の基礎ダメージ。 </param>
        /// <param name="isOutOfRange"> 対象が射程外にいる場合はtrue。ダメージ減衰の判定に使う。 </param>
        /// <param name="isCriticalForced"> クリティカルヒットが強制されている場合はtrue。 </param>
        /// <param name="criticalDamageMultiplierOverride"> この攻撃特有のクリティカルダメージ倍率のオーバーライド。 </param>
        /// <returns> 攻撃結果。 </returns>
        public static AttackResult Calculate(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender,
            bool isJustHit,
            Damage baseDamage,
            bool isOutOfRange = false,
            bool isCriticalForced = false,
            float? criticalDamageMultiplierOverride = null,
            bool applyAttackerModifiers = true,
            bool applyWeaponDamageMultiplier = true
            )
        {
            Damage modifiedDamage = baseDamage;

            if(attacker != null && applyAttackerModifiers)
            {
                modifiedDamage = attacker.StatusEffectSystem.ApplyAttackPowerModifiers(
                    attacker, defender, baseDamage);
            }

            AttackStepContext stepContext = new AttackStepContext(
                attackDefinition,
                attacker,
                defender,
                isJustHit,
                modifiedDamage,
                isOutOfRange,
                isCriticalForced,
                criticalDamageMultiplierOverride,
                applyWeaponDamageMultiplier);
            return attackDefinition.AttackPipeline.Execute(stepContext);
        }
    }
}
