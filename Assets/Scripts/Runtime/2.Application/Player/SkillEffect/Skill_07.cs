using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 07 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_07 : SkillBase
    {
        /// <summary>
        ///     スキル効果を初期化します。
        /// </summary>
        /// <param name="attackController"> 攻撃実行器です。 </param>
        public Skill_07(IAttackController attackController)
        {
            _attackController = attackController ?? throw new ArgumentNullException(nameof(attackController));
        }

        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public override void Execute(in SkillEffectContext context)
        {
            ReadOnlySpan<CharacterEntity> targets =
                context.TargetEntities.Span;

            if (targets.Length == 0)
            {
                return;
            }

            // スキル効果パラメータを取得
            int attackCount =
                (int)context.EffectSpec.GetRequiredValue(
                    SkillEffectParameterId.HitCount);

            float reductionRate =
                (float)context.EffectSpec.GetRequiredValue(
                    SkillEffectParameterId.AttackPowerReductionRate);

            float reductionCap =
                (float)context.EffectSpec.GetRequiredValue(
                    SkillEffectParameterId.AttackPowerReductionCap);

            float durationSeconds =
                (float)context.EffectSpec.GetRequiredValue(
                    SkillEffectParameterId.DurationSeconds);

            // 攻撃回数を初期化して攻撃を実行
            _hitCounts.Clear();
            ExecuteAttacks(targets, attackCount, context.IsJustHit);

            // 攻撃力減少デバフを適用し、プレイヤーの攻撃力増加量を計算
            float playerIncreaseAmount = ApplyDebuffs(
                reductionRate,
                reductionCap,
                durationSeconds);

            context.PlayerEntity.StatusEffectSystem.Add(
                new AttackPowerIncreaseBuff(
                    playerIncreaseAmount,
                    durationSeconds));

            Debug.Log($"[Skill07]発動。" +
                $"攻撃回数:{attackCount}、" +
                $"減少率:{reductionRate}、" +
                $"持続時間:{durationSeconds}秒、" +
                $"プレイヤー増加量:{playerIncreaseAmount}");
        }

        private readonly IAttackController _attackController;
        private readonly Dictionary<CharacterEntity, int> _hitCounts = new();
        private readonly List<int> _candidateIndices = new();

        /// <summary>
        ///     対象数に応じて攻撃を実行します。
        /// </summary>
        /// <param name="targets"> 攻撃対象のキャラクターエンティティのスパンです。 </param>
        /// <param name="attackCount"> 実行する攻撃回数です。 </param>
        /// <param name="isJustHit"> ジャストヒットかどうか </param>
        private void ExecuteAttacks(ReadOnlySpan<CharacterEntity> targets, int attackCount, bool isJustHit)
        {
            if (attackCount <= 0)
            {
                return;
            }

            // 対象数が攻撃回数以上の場合は、対象を重複させずに攻撃を実行
            if (targets.Length >= attackCount)
            {
                ExecuteUniqueTargets(targets, attackCount, isJustHit);
                return;
            }

            int executedCount = 0;

            // 対象数が攻撃回数未満の場合は、対象を重複させて攻撃を実行
            for (int i = 0; i < targets.Length && executedCount < attackCount; i++)
            {
                ExecuteAttack(targets[i], isJustHit);
                executedCount++;
            }

            // 残りの攻撃回数分、最も体力が高い対象に攻撃を実行
            while (executedCount < attackCount)
            {
                CharacterEntity target = FindHighestHealthTarget(targets);

                if (target == null)
                {
                    break;
                }

                ExecuteAttack(target, isJustHit);
                executedCount++;
            }
        }

        /// <summary>
        ///     対象を重複させずに攻撃を実行します。
        /// </summary>
        /// <param name="targets"> 攻撃対象のキャラクターエンティティのスパンです。 </param>
        /// <param name="attackCount"> 実行する攻撃回数です。 </param>
        /// <param name="isJustHit"> ジャストヒットかどうか </param>
        private void ExecuteUniqueTargets(ReadOnlySpan<CharacterEntity> targets, int attackCount, bool isJustHit)
        {
            _candidateIndices.Clear();

            for (int i = 0; i < targets.Length; i++)
            {
                _candidateIndices.Add(i);
            }

            // ランダムに対象をシャッフルして攻撃を実行
            for (int i = 0; i < attackCount; i++)
            {
                int randomIndex = Random.Range(i, _candidateIndices.Count);

                (_candidateIndices[i], _candidateIndices[randomIndex]) =
                    (_candidateIndices[randomIndex], _candidateIndices[i]);

                ExecuteAttack(targets[_candidateIndices[i]], isJustHit);
            }
        }

        /// <summary>
        ///     指定された対象に攻撃を実行します。
        /// </summary>
        /// <param name="target"> 攻撃対象のキャラクターエンティティです。 </param>
        /// <param name="isJustHit"> ジャストヒットかどうか </param>
        private void ExecuteAttack(CharacterEntity target, bool isJustHit)
        {
            if (target == null)
            {
                return;
            }

            _attackController.Execute((int)BeatType.Four, target, isJustHit);

            if (_hitCounts.TryGetValue(target, out int currentCount))
            {
                _hitCounts[target] = currentCount + 1;
                return;
            }

            _hitCounts.Add(target, 1);
        }

        /// <summary>
        ///     攻撃力減少デバフを適用し、プレイヤーの攻撃力増加量を計算します。
        /// </summary>
        /// <param name="reductionRate"> 攻撃力減少率です。 </param>
        /// <param name="reductionCap"> 攻撃力減少量の上限です。 </param>
        /// <param name="durationSeconds"> デバフの持続時間（秒）です。 </param>
        /// <returns> プレイヤーの攻撃力増加量です。 </returns>
        private float ApplyDebuffs(
            float reductionRate,
            float reductionCap,
            float durationSeconds)
        {
            float totalReductionAmount = 0f;

            foreach (var kvp in _hitCounts)
            {
                CharacterEntity target = kvp.Key;
                float baseAttackPower = target.BaseDamage.Value;
                float maxReductionAmount = Mathf.Min(reductionCap, baseAttackPower);
                float reductionPerHit = baseAttackPower * reductionRate;
                float requestedReductionAmount = reductionPerHit * kvp.Value;
                float reductionAmount = Mathf.Min(requestedReductionAmount, maxReductionAmount);

                if (maxReductionAmount > 0f)
                {
                    target.StatusEffectSystem.Add(
                        new AttackPowerReductionDebuff(
                            reductionAmount,
                            durationSeconds));
                }

                totalReductionAmount += reductionAmount;

                Debug.Log($"[Skill07]対象:{target.Name}、" +
                    $"攻撃回数:{kvp.Value}、" +
                    $"基礎攻撃力:{baseAttackPower}、" +
                    $"減少率:{reductionRate}、" +
                    $"減少上限:{maxReductionAmount}、" +
                    $"持続時間:{durationSeconds}秒、" +
                    $"追加減少量:{reductionAmount}");
            }
            return totalReductionAmount;
        }

        /// <summary>
        ///     HPの最も高い対象を見つけます。
        /// </summary>
        /// <param name="targets"> 攻撃対象のキャラクターエンティティのスパンです。 </param>
        /// <returns> HPの最も高いキャラクターエンティティです。 </returns>
        private static CharacterEntity FindHighestHealthTarget(ReadOnlySpan<CharacterEntity> targets)
        {
            CharacterEntity highestHealthTarget = null;
            float highestHealth = float.MinValue;

            foreach (CharacterEntity target in targets)
            {
                if (target == null || target.IsDead)
                {
                    continue;
                }

                if (target.CurrentHealth.Value <= highestHealth)
                {
                    continue;
                }

                highestHealth = target.CurrentHealth.Value;
                highestHealthTarget = target;
            }

            return highestHealthTarget;
        }
    }
}