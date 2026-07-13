using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
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
        /// <param name="buff"> 付与バフです。 </param>
        /// <param name="attackController"> 攻撃実行器です。 </param>
        public Skill_07(IBuff buff, IAttackController attackController) : base(buff)
        {
            _attackController = attackController;
        }

        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public override void Execute(in SkillEffectContext context)
        {
            ReadOnlySpan<CharacterEntity> targets = context.TargetEntities.Span;
            if (targets.Length == 0)
            {
                return;
            }

            float beforeTargetsTotalDamageValue = 0f;
            for (int i = 0; i < targets.Length; i++)
            {
                beforeTargetsTotalDamageValue += targets[i].BaseDamage.Value;
            }

            Damage beforeTargetsTotalDamage = new Damage(beforeTargetsTotalDamageValue);

            if (_attackCount <= targets.Length)
            {
                int targetNumber = Random.Range(0, targets.Length);
                for (int i = 0; i < _attackCount; i++)
                {
                    _attackController.Execute((int)_beatType, targets[targetNumber]);
                    targets[targetNumber].BuffSystem.Add(_buff);
                    BuffContext buffcontext = new BuffContext(context.PlayerEntity, context.TargetEntity);
                    targets[targetNumber].BuffSystem.Execute(buffcontext, BuffExecuteTiming.Skill);
                }
            }
            else
            {
                ResetHitNumbers(targets.Length);

                for (int i = 0; i < targets.Length; i++)
                {
                    int targetNumber = Random.Range(0, targets.Length);
                    _attackController.Execute((int)_beatType, targets[targetNumber]);
                    targets[targetNumber].BuffSystem.Add(_buff);
                    BuffContext buffcontext = new BuffContext(context.PlayerEntity, context.TargetEntity);
                    targets[targetNumber].BuffSystem.Execute(buffcontext, BuffExecuteTiming.Skill);
                    _hitNumbers[targetNumber]++;

                    bool isAllhit = true;
                    for (int j = 0; j < _hitNumbers.Count; j++)
                    {
                        if (_hitNumbers[j] != 0)
                        {
                            continue;
                        }

                        isAllhit = false;
                        break;
                    }

                    if (isAllhit)
                    {
                        float maxValue = float.MinValue;
                        int maxHealthTarget = 0;
                        for (int k = 0; k < targets.Length; k++)
                        {
                            if (targets[k].CurrentHealth.Value <= maxValue)
                            {
                                continue;
                            }

                            maxValue = targets[k].CurrentHealth.Value;
                            maxHealthTarget = k;
                        }

                        CharacterEntity targetCharacter = targets[maxHealthTarget];
                        _attackController.Execute((int)_beatType, targetCharacter);
                        targetCharacter.BuffSystem.Execute(buffcontext, BuffExecuteTiming.Skill);
                    }
                }
            }

            float afterTargetsTotalDamageValue = 0f;
            for (int i = 0; i < targets.Length; i++)
            {
                afterTargetsTotalDamageValue += targets[i].BaseDamage.Value;
            }

            Damage afterTargetsTotalDamage = new Damage(afterTargetsTotalDamageValue);
            Damage targetsDownBaseDamage = beforeTargetsTotalDamage - afterTargetsTotalDamage.Value;
            context.PlayerEntity.ChangeBaseDamage(targetsDownBaseDamage);
        }

        private readonly IAttackController _attackController;
        private readonly BeatType _beatType = BeatType.Four;
        private readonly int _attackCount = 3;
        private readonly List<int> _hitNumbers = new();

        /// <summary>
        ///     対象数に合わせて命中回数を初期化します。
        /// </summary>
        /// <param name="targetCount"> 対象数です。 </param>
        private void ResetHitNumbers(int targetCount)
        {
            _hitNumbers.Clear();
            for (int i = 0; i < targetCount; i++)
            {
                _hitNumbers.Add(0);
            }
        }
    }
}
