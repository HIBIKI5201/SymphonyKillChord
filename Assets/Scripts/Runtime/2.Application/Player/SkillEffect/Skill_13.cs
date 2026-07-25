using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 13 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_13 : SkillBase
    {
        public Skill_13(IBuff buff) : base(buff)
        {

        }
        /// <summary>
        ///     スキル効果を実行するメソッド。スキルの効果を対象のキャラクターエンティティに適用する。
        /// </summary>
        /// <param name="context">スキル効果の発動に必要な情報をまとめた構造体。</param>
        public override void Execute(in SkillEffectContext context)
        {
            float damageMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageMultiplier);
            int attackCount = (int)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.HitCount);
            float criticalMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.CriticalMultiplier);
            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);
            //  武器なし攻撃を実装するための箱替え。
            AttackDefinition unbulletDefinition = new AttackDefinition(attackDefinition.AttackName, attackDefinition.AttackSpec, attackDefinition.AttackPipeline);


            for (int i = 0; i < attackCount; i++)
            {
                AttackResult result = AttackCalculator.Calculate(unbulletDefinition, context.PlayerEntity, context.TargetEntity, false, context.PlayerEntity.BaseDamage);
                Damage damage = result.FinalDamage * damageMultiplier;
                if (result.IsCritical)
                {
                    damage /= unbulletDefinition.AttackSpec.CriticalMultiplier.Value; //元の攻撃定義のクリティカル倍率でダメージを補正してから、スキル固有のクリティカル倍率を適用
                    damage *= criticalMultiplier; //クリティカルヒットのダメージを補正
                }
                context.TargetEntity.TakeDamage(damage);
#if UNITY_EDITOR
                Debug.Log($"{i + 1} 回目の、Skill_13 を実行しました:{damage}ダメージです。 ");
#endif
            }

        }
    }
}
