using KillChord.Runtime.Adaptor.InGame.Skill.Effect;

namespace KillChord.Runtime.Composition.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルエフェクトモジュールの公開物を保持するContainer。
    /// </summary>
    public sealed class SkillEffectModuleContainer
    {
        /// <summary>
        ///     Containerを生成する。
        /// </summary>
        /// <param name="skillEffectPlayer"> スキルエフェクト再生用のPlayerです。 </param>
        public SkillEffectModuleContainer(ISkillEffectPlayer skillEffectPlayer)
        {
            SkillEffectPlayer = skillEffectPlayer;
        }

        /// <summary> スキルエフェクト再生用のPlayerです。 </summary>
        public ISkillEffectPlayer SkillEffectPlayer { get; }
    }
}
