using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using KillChord.Runtime.Utility.Persistent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.SkillEffect
{
    /// <summary>
    ///     伝染ダメージのグループを表すクラスです。
    /// </summary>
    internal class InfectionGroup
    {
        public InfectionGroup(
            IAttacker attacker,
            AttackDefinition attackDefinition,
            float damageRate,
            int triggerCount)
        {
            _attacker = attacker ?? throw new ArgumentNullException(nameof(attacker));

            if (attackDefinition == null)
            {
                throw new ArgumentNullException(nameof(attackDefinition));
            }

            if (!float.IsFinite(damageRate) || damageRate <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damageRate));
            }

            if (triggerCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(triggerCount));
            }

            _infectionAttackDefinition = CreateInfectionAttackDefinition(attackDefinition);
            _damageRate = damageRate;
            _remainingTriggerCount = triggerCount;
        }

        /// <summary> 伝染回数を消費しきったかどうかを示す値です。 </summary>
        public bool IsConsumed => _remainingTriggerCount <= 0;

        /// <summary>
        ///     伝染対象のメンバーを追加します。
        /// </summary>
        /// <param name="character"> 伝染対象のキャラクター </param>
        /// <param name="effect"> 伝染デバフ </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddMember(CharacterEntity character, InfectionDebuff effect)
        {
            if (character == null || effect == null)
            {
                throw new ArgumentNullException(character == null ? nameof(character) : nameof(effect));
            }

            _members.Add(new InfectionMember(character, effect));
        }

        /// <summary>
        ///     伝染ダメージを発生させます。
        /// </summary>
        /// <param name="context"> ダメージを受けた際のコンテキスト情報 </param>
        public void Trigger(in DamageTakenContext context)
        {
            if (IsConsumed || _isTransmitting ||
                context.AttackType == DamageAttackType.Infection)
            {
                return;
            }

            // 無敵などでダメージが発生しなかった場合は伝染しない
            float landedDamage =
                context.AttackResult.AppliedDamage.Value +
                context.AttackResult.BarrierDamage.Value;

            if (landedDamage <= 0)
            {
                return;
            }

            Damage infectionBaseDamage = new Damage(
                context.AttackResult.FinalDamage.Value * _damageRate);

            if (infectionBaseDamage.Value <= 0)
            {
                return;
            }

            _isTransmitting = true;

            try
            {
                // 最初に攻撃をウケた対象自身も伝染対象に含める
                for (int i = 0; i < _members.Count; i++)
                {
                    InfectionMember member = _members[i];

                    if (!IsActiveMember(member))
                    {
                        continue;
                    }

                    ApplyInfectionDamage(
                        member.Character, infectionBaseDamage, context.AttackResult.IsJustHit);
                }
            }
            finally
            {
                _isTransmitting = false;
            }

            // 伝染回数を消費する
            _remainingTriggerCount--;

            Debug.Log($"[Skill08] 伝染ダメージを適用しました。残り伝染回数: {_remainingTriggerCount}");

            if (IsConsumed)
            {
                RemoveOtherEffects(context.Defender);
            }
        }

        private readonly IAttacker _attacker;
        private readonly AttackDefinition _infectionAttackDefinition;
        private readonly float _damageRate;
        private readonly List<InfectionMember> _members = new();

        private int _remainingTriggerCount;
        private bool _isTransmitting;

        private readonly struct InfectionMember
        {
            public InfectionMember(CharacterEntity character, InfectionDebuff effect)
            {
                Character = character;
                Effect = effect;
            }

            public CharacterEntity Character { get; }
            public InfectionDebuff Effect { get; }
        }

        /// <summary>
        ///     指定した対象に伝染ダメージを適用します。
        /// </summary>
        /// <param name="target"> 伝染ダメージを適用する対象のキャラクター </param>
        /// <param name="baseDamage"> 伝染ダメージの基礎値 </param>
        /// <param name="isJustHit"> ジャストヒットかどうかを示すフラグ </param>
        private void ApplyInfectionDamage(CharacterEntity target, Damage baseDamage, bool isJustHit)
        {
            AttackResult result =
                AttackCalculator.Calculate(
                    _infectionAttackDefinition,
                    _attacker,
                    target,
                    isJustHit,
                    baseDamage,
                    applyAttackerModifiers: false);

            DamageExecutor.ExecuteDerived(
                _attacker,
                target,
                result,
                DamageAttackType.Infection);
        }

        /// <summary>
        ///     グループ内の全てのメンバーから伝染デバフを削除します。
        /// </summary>
        private void RemoveOtherEffects(IDefender notifyDefender)
        {
            for (int i = 0; i < _members.Count; i++)
            {
                InfectionMember member = _members[i];

                if (ReferenceEquals(member.Character, notifyDefender))
                {
                    continue;
                }

                member.Character.StatusEffectSystem.Remove(member.Effect);
            }
        }

        /// <summary>
        ///     伝染ダメージ用のAttackDefinitionを作成します。
        /// </summary>
        /// <param name="attackDefinition"> 元となるAttackDefinition </param>
        /// <returns> 伝染ダメージ用のAttackDefinition </returns>
        private static AttackDefinition CreateInfectionAttackDefinition(AttackDefinition attackDefinition)
        {
            AttackPipeline attackPipeline = new AttackPipeline(new IAttackStep[]
            {
                new WeaponDamageStep(),
                new CriticalStep()
            });

            return new AttackDefinition(
                "Skill08.Infection",
                attackDefinition.AttackSpec,
                attackPipeline,
                attackDefinition.BeatType,
                attackDefinition.JustDamageMultiplier,
                1f);
        }

        /// <summary>
        ///     伝染対象のメンバーが有効かどうかを判定します。
        /// </summary>
        /// <param name="member"> 判定対象の伝染メンバー </param>
        /// <returns> メンバーが有効であればtrue、それ以外はfalse </returns>
        private static bool IsActiveMember(in InfectionMember member)
        {
            if (member.Character == null || member.Character.IsDead)
            {
                return false;
            }

            if (!member.Character.StatusEffectSystem.TryGet(
                InfectionDebuff.EffectId, out IStatusEffect effect))
            {
                return false;
            }

            return ReferenceEquals(effect, member.Effect);
        }
    }
}
