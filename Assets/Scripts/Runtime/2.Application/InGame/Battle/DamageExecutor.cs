using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Utility.Persistent;
using System;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     ダメージ計算を実行するユーティリティクラス。
    /// </summary>
    // TODO: ダメージ通知(EOnTakeDamage)の発火をここへ集約する。
    //   現状は PlayerAttackController と SkillAttackController だけが個別に発火しており、
    //   DamageExecutor を直接呼ぶ経路(Skill_00/01/03/05/13、InfectionGroup)は
    //   ダメージが入るのに数値表示が出ない。呼び出し側での発火は漏れが起きやすい。
    //   あるべき形は、ダメージを確定させたこのメソッドが唯一の通知元になること。
    //   ただし AttackExecutor が内部で本メソッドを呼ぶため、そのまま発火を足すと
    //   通常攻撃系が二重発火する。移行手順は以下。
    //     1. 本メソッドの末尾で EOnTakeDamage を発火する
    //     2. PlayerAttackController と SkillAttackController の明示的な発火を削除する
    //     3. 発火値を FinalDamage と AppliedDamage のどちらに統一するか決める
    //        (現状は表示側が FinalDamage 前提。軽減後の実ダメージを出すなら表示仕様の変更が要る)
    //     4. 通常攻撃・スキル・感染ダメージの表示回数が変わらないことを確認する
    public static class DamageExecutor
    {
        /// <summary>
        ///     通常のダメージ計算を実行します。
        /// </summary>
        /// <param name="attacker"> 攻撃者です。 </param>
        /// <param name="defender"> 防御者です。 </param>
        /// <param name="attackResult"> 攻撃結果です。 </param>
        /// <param name="attackType"> 攻撃タイプです。 </param>
        /// <returns> 計算結果の攻撃結果です。 </returns>
        public static AttackResult Execute(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult,
            DamageAttackType attackType)
        {
            return ExecuteInternal(attacker, defender, attackResult, attackType, true);
        }

        /// <summary>
        ///     派生ダメージ計算を実行します。
        ///     攻撃者のステータス効果によるダメージ修正は適用されません。
        /// </summary>
        /// <param name="attacker"> 攻撃者です。 </param>
        /// <param name="defender"> 防御者です。 </param>
        /// <param name="attackResult"> 攻撃結果です。 </param>
        /// <param name="attackType"> 攻撃タイプです。 </param>
        /// <returns> 計算結果の攻撃結果です。 </returns>
        public static AttackResult ExecuteDerived(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult,
            DamageAttackType attackType)
        {
            return ExecuteInternal(attacker, defender, attackResult, attackType, false);
        }

        /// <summary>
        ///     ダメージ計算を実行します。
        /// </summary>
        /// <param name="attacker"> 攻撃者です。 </param>
        /// <param name="defender"> 防御者です。 </param>
        /// <param name="attackResult"> 攻撃結果です。 </param>
        /// <param name="attackType"> 攻撃タイプです。 </param>
        /// <param name="applyOutgoingModifiers"> 攻撃者のステータス効果によるダメージ修正を適用するかどうかを示す値です。 </param>
        /// <returns> 計算結果の攻撃結果です。 </returns>
        public static AttackResult ExecuteInternal(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult,
            DamageAttackType attackType,
            bool applyOutgoingModifiers)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (defender == null)
            {
                throw new ArgumentNullException(nameof(defender));
            }

            var result = attackResult;

            // キャラクターのステータス効果によるダメージ修正を適用する
            if (applyOutgoingModifiers)
            {
                result = attacker.StatusEffectSystem.ApplyOutgoingDamageModifiers(
                    attacker, defender, result);
            }

            result = defender.StatusEffectSystem.ApplyIncomingDamageModifiers(
                attacker, defender, result);

            // バリアを持つ防御者の場合、バリアでダメージを吸収する
            Damage damageToHealth = result.FinalDamage;
            Damage barrierDamage = default;

            if (defender.CanTakeDamage &&
                defender is IBarrierHolder barrierHolder)
            {
                damageToHealth = barrierHolder.AbsorbBarrier(result.FinalDamage, out barrierDamage);
            }

            Damage appliedDamage = default;

            // 防御者がダメージを受けることができる場合、またはダメージが0より大きい場合にのみ、ダメージを適用する
            if (damageToHealth.Value > 0f ||
                !defender.CanTakeDamage)
            {
                appliedDamage = defender.TakeDamage(damageToHealth);
            }

            // 攻撃結果にバリアダメージと適用されたダメージを設定する
            result = result
                .WithBarrierDamage(barrierDamage)
                .WithAppliedDamage(appliedDamage);

            // 攻撃者と防御者のステータス効果システムにダメージを通知する
            defender.StatusEffectSystem.NotifyDamageTaken(
                new DamageTakenContext(
                    attacker,
                    defender,
                    result,
                    attackType));
            attacker.StatusEffectSystem.NotifyDamageDealt(
                new DamageDealtContext(
                    attacker,
                    defender,
                    result,
                    attackType));

            return result;
        }
    }
}
