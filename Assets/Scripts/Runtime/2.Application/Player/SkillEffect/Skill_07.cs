using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.Player;
using UnityEngine;
using KillChord.Runtime.Domain.InGame.Buff;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 07 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_07 : SkillBase
    {
        public Skill_07(IBuff buff) : base(buff)
        {

        }
        public override void Execute(SkillEffectContext context)
        {
            //自身の前方60度（仮）射程12m内にいる敵に対して4拍子の通常攻撃を3回行う。
            var targets = context.Repository.FindByRule();
            float BeforeTargetsTotalDamageValue = 0;
            foreach(var target in targets) BeforeTargetsTotalDamageValue += target.BaseDamage.Value;
            Damage BeforeTargetsTotalDamage = new(BeforeTargetsTotalDamageValue);
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
                //範囲内に敵が2体以下の場合、
                //通常攻撃2回目以降の攻撃対象選択時に範囲内のすべての敵に攻撃が的中してた場合はターゲット内にいる敵に残りHPが高い順で通常攻撃が再度Hitする。
                _hitNumbers = new int[targets.Length];

                for (int i = 0; i < targets.Length; i++)
                {
                    int targetNumber = Random.Range(0, targets.Length);
                    _attackController.Execute((int)_beatType, targets[targetNumber]);
                    targets[targetNumber].BuffSystem.Add(_buff);
                    BuffContext buffcontext = new BuffContext(context.PlayerEntity, context.TargetEntity);
                    targets[targetNumber].BuffSystem.Execute(buffcontext, BuffExecuteTiming.Skill);
                    _hitNumbers[targetNumber]++;
                    bool isAllhit = true;
                    foreach (var hit in _hitNumbers) if (hit == 0)
                    {
                        isAllhit = false;
                        break;
                    }

                    if (isAllhit)
                    {
                        float maxValue = float.MinValue;
                        int maxHealthTarget = 0;
                        CharacterEntity targetCharacter;
                        for (int k = 0; k < targets.Length; k++)
                        {
                            if (targets[i].CurrentHealth.Value > maxValue)
                            {
                                maxValue = targets[i].CurrentHealth.Value;
                                maxHealthTarget = i;
                            }
                        }
                        
                        targetCharacter =  targets[maxHealthTarget];
                        _attackController.Execute((int)_beatType, targetCharacter);
                        targetCharacter.BuffSystem.Execute(buffcontext, BuffExecuteTiming.Skill);
                    }
                }
            }

            float AfterTargetsTotalDamageValue = 0;
            foreach(var target in targets) AfterTargetsTotalDamageValue += target.BaseDamage.Value;
            Damage AfterTargetsTotalDamage = new(BeforeTargetsTotalDamageValue);
            
            Damage targetsDownBaseDamage = BeforeTargetsTotalDamage - AfterTargetsTotalDamage.Value;
            context.PlayerEntity.ChangeBaseDamage(targetsDownBaseDamage);

        }

        private IAttackController _attackController;
        private BeatType _beatType = BeatType.Four;
        private int _attackCount = 3;
        private int[] _hitNumbers;
    }
}
