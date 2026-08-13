using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Skill;
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
        public static void RegisterDefaults(SkillEffectExecutorResolver resolver,
            IAttackController attackController,
            PendingAttackEffectService effectService)
        {
            resolver.Register(SkillEffectType.Skill00, new Skill_00(null));
            resolver.Register(SkillEffectType.Skill01, new Skill_01(null));
            resolver.Register(SkillEffectType.Skill02, new Skill_02(effectService));
            resolver.Register(SkillEffectType.Skill03, new Skill_03(null));
            resolver.Register(SkillEffectType.Skill05, new Skill_05(null));
            resolver.Register(SkillEffectType.Skill06, new Skill_06());
            resolver.Register(SkillEffectType.Skill07, new Skill_07(null, attackController));
            resolver.Register(SkillEffectType.Skill08, new Skill_08(null));
            resolver.Register(SkillEffectType.Skill09, new Skill_09(null));
            resolver.Register(SkillEffectType.Skill10, new Skill_10(null));
            resolver.Register(SkillEffectType.Skill13, new Skill_13(null));
        }
    }
}
