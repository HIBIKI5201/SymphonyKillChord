using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Utility.Persistent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     攻撃を実行するクラス。
    ///     攻撃の計算とダメージの適用を行う。
    /// </summary>
    public static class AttackExecutor
    {
        /// <summary>
        ///     攻撃を実行する。
        /// </summary>
        /// <param name="attackDefinition"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <param name="damageAttackType"> ダメージの攻撃タイプ。 </param>
        /// <returns> 攻撃結果。 </returns>
        public static AttackResult Execute(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender,
            bool isJustHit,
            Damage baseDamage,
            bool isOutOfRange = false,
            IReadOnlyList<IAttackHitEffect> hitEffects = null,
            DamageAttackType damageAttackType = DamageAttackType.Normal
               )
        {
            if (attackDefinition == null)
            {
                throw new ArgumentNullException(nameof(attackDefinition));
            }

            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (defender == null)
            {
                throw new ArgumentNullException(nameof(defender));
            }

            ExecuteBuffBeforeAttack(attacker, defender);

            // 計算を行い、ダメージを適用する。
            AttackResult result = AttackCalculator.Calculate(attackDefinition, attacker, defender, isJustHit, baseDamage, isOutOfRange);

            result = ExecuteBuffAfterAttack(attacker, defender, result);
            result = DamageExecutor.Execute(attacker, defender, result, damageAttackType);

            ApplyHitEffects(attacker, defender, result, hitEffects);

            Debug.Log(
                 $"[Attack] " +
                 $"AttackName:{attackDefinition.AttackName} " +
                 $"Damage:{result.FinalDamage.Value} " +
                 $"Critical:{result.IsCritical}");

            return result;
        }

        /// <summary>
        ///     複数の対象へ攻撃を実行する。
        ///     結果は <paramref name="targets"/> と同じ順序で <paramref name="results"/> へ格納する。
        /// </summary>
        /// <param name="attackDefinition"> 攻撃定義。 </param>
        /// <param name="attacker"> 攻撃者。 </param>
        /// <param name="targets"> 攻撃対象の一覧。射程外かどうかを対象ごとに持つ。 </param>
        /// <param name="isJustHit"> ジャスト入力かどうか。 </param>
        /// <param name="baseDamage"> 基礎ダメージ。 </param>
        /// <param name="results"> 攻撃結果の格納先。呼び出し時に内容がクリアされる。 </param>
        /// <param name="damageAttackType"> ダメージの攻撃タイプ。 </param>
        public static void Execute(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IReadOnlyList<AttackTarget> targets,
            bool isJustHit,
            Damage baseDamage,
            List<AttackResult> results,
            IReadOnlyList<IAttackHitEffect> hitEffects = null,
            DamageAttackType damageAttackType = DamageAttackType.Normal
               )
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();

            // 各対象に対して攻撃を実行する。
            for (int i = 0; i < targets.Count; i++)
            {
                AttackTarget target = targets[i];
                results.Add(Execute(
                    attackDefinition, attacker, target.Defender, isJustHit, baseDamage, target.IsOutOfRange, hitEffects, damageAttackType));
            }
        }

        /// <summary>
        ///     攻撃計算前のバフを実行する。
        /// </summary>
        /// <param name="attacker"> 攻撃者。 </param>
        /// <param name="defender"> 攻撃対象。 </param>
        private static void ExecuteBuffBeforeAttack(IAttacker attacker, IDefender defender)
        {
            CharacterEntity attackerEntity = attacker as CharacterEntity;
            CharacterEntity defenderEntity = defender as CharacterEntity;

            attacker.BuffSystem.Execute(
                new BuffContext(attackerEntity, defenderEntity),
                BuffExecuteTiming.Attack_Logic_Before);
        }

        /// <summary>
        ///     攻撃計算後のバフを実行し、補正済みの攻撃結果を返す。
        /// </summary>
        /// <param name="attacker"> 攻撃者。 </param>
        /// <param name="defender"> 攻撃対象。 </param>
        /// <param name="result"> 補正前の攻撃結果。 </param>
        /// <returns> 補正後の攻撃結果。 </returns>
        private static AttackResult ExecuteBuffAfterAttack(IAttacker attacker, IDefender defender, AttackResult result)
        {
            CharacterEntity attackerEntity = attacker as CharacterEntity;
            CharacterEntity defenderEntity = defender as CharacterEntity;

            BuffContext buffContext = new BuffContext(attackerEntity, defenderEntity, result);
            buffContext = attacker.BuffSystem.Execute(buffContext, BuffExecuteTiming.Attack_Logic_After);
            buffContext = defender.BuffSystem.Execute(buffContext, BuffExecuteTiming.Defense_Logic_Before);

            return buffContext.AttackResult;
        }

        /// <summary>
        ///     攻撃命中後の追加効果を適用する。
        /// </summary>
        /// <param name="attacker"> 攻撃者。 </param>
        /// <param name="defender"> 攻撃対象。 </param>
        /// <param name="result"> 攻撃結果。 </param>
        /// <param name="hitEffects"> 攻撃命中後の追加効果の一覧。 </param>
        private static void ApplyHitEffects(IAttacker attacker, IDefender defender, in AttackResult result, IReadOnlyList<IAttackHitEffect> hitEffects)
        {
            if (hitEffects == null)
            {
                return;
            }

            for (int i = 0; i < hitEffects.Count; i++)
            {
                hitEffects[i]?.Apply(attacker, defender, result);
            }
        }
    }
}
