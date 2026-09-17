using KillChord.Runtime.Domain.InGame.Battle;
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
            DamageAttackType damageAttackType = DamageAttackType.Normal,
            bool notifyNormalDamage = false
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

            // 計算を行い、ダメージを適用する。
            AttackResult result = AttackCalculator.Calculate(attackDefinition, attacker, defender, isJustHit, baseDamage, isOutOfRange);

            result = DamageExecutor.Execute(attacker, defender, result, damageAttackType, notifyNormalDamage);

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
        /// <param name="notifyNormalDamage"> 通常ダメージを通知するかどうかを示す値です。 </param>
        public static void Execute(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IReadOnlyList<AttackTarget> targets,
            bool isJustHit,
            Damage baseDamage,
            List<AttackResult> results,
            IReadOnlyList<IAttackHitEffect> hitEffects = null,
            DamageAttackType damageAttackType = DamageAttackType.Normal,
            bool notifyNormalDamage = false
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
                    attackDefinition,
                    attacker,
                    target.Defender,
                    isJustHit,
                    baseDamage,
                    target.IsOutOfRange,
                    hitEffects,
                    damageAttackType,
                    notifyNormalDamage));
            }
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
