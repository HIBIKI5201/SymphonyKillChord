using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.SkillEffect;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///   スキルID 02 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_02 : SkillBase
    {
        /// <summary>
        ///     スキル効果を初期化します。
        /// </summary>
        /// <param name="effectService"> 攻撃実行サービス。 </param>
        public Skill_02(PendingAttackEffectService effectService)
        {
            _pendingAttackEffectService = effectService ?? throw new ArgumentNullException(nameof(effectService));
        }

        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public override void Execute(in SkillEffectContext context)
        {
            float increaseRate = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageTakenIncreaseRate);

            float durationSeconds = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DurationSeconds);

            Debug.Log($"[Skill_02] 発動: ダメージ増加率={increaseRate}, 効果時間={durationSeconds}秒");

            // 攻撃ヒット時にダメージ増加効果を付与する処理を登録します。
            _pendingAttackEffectService.Register(
                new DamageTakenIncreaseOnHitEffect(
                    increaseRate,
                    durationSeconds,
                    context.EffectSpec.ReapplyPolicy));
        }

        private readonly PendingAttackEffectService _pendingAttackEffectService;
    }
}
