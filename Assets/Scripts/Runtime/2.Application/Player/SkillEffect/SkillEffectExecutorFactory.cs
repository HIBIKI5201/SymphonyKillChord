using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Application.InGame.Target;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     既定のスキル効果実行器を登録するファクトリーです。
    /// </summary>
    public static class SkillEffectExecutorFactory
    {
        /// <summary>
        ///     既定の実行器を登録します。
        /// </summary>
        /// <param name="resolver"> 登録先の解決テーブルです。 </param>
        /// <param name="attackController"> スキル用の攻撃実行器です。 </param>
        /// <param name="effectService"> スキル用の保留中効果サービスです。 </param>
        /// <param name="rangeQuery"> スキル用の範囲判定クエリです。 </param>
        /// <param name="targetRadiusQuery"> スキル用の範囲半径判定クエリです。 </param>
        public static void RegisterDefaults(SkillEffectExecutorResolver resolver,
            IAttackController attackController,
            PendingAttackEffectService effectService,
            IPlayerTargetRangeQuery rangeQuery,
            ITargetRadiusQuery targetRadiusQuery)
        {
            resolver.Register(SkillEffectType.Skill00, new Skill_00());
            resolver.Register(SkillEffectType.Skill01, new Skill_01());
            resolver.Register(SkillEffectType.Skill02, new Skill_02(effectService));
            resolver.Register(SkillEffectType.Skill03, new Skill_03());
            resolver.Register(SkillEffectType.Skill04, new Skill_04());
            resolver.Register(SkillEffectType.Skill05, new Skill_05());
            resolver.Register(SkillEffectType.Skill06, new Skill_06());
            resolver.Register(SkillEffectType.Skill07, new Skill_07(attackController));
            resolver.Register(SkillEffectType.Skill08, new Skill_08(effectService, targetRadiusQuery));
            resolver.Register(SkillEffectType.Skill09, new Skill_09(rangeQuery));
            resolver.Register(SkillEffectType.Skill10, new Skill_10());
            resolver.Register(SkillEffectType.Skill13, new Skill_13());
        }
    }
}
