using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.View.InGame.Skill.Effect;
using System.Collections.Generic;

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
        /// <param name="skillEffectSpawner"> スキルエフェクトのSpawnerです。 </param>
        public SkillEffectModuleContainer(ISkillEffectSpawner skillEffectSpawner)
        {
            _skillEffectSpawner = skillEffectSpawner;
        }

        /// <summary> スキルエフェクト再生用のPlayerです。 </summary>
        public ISkillEffectPlayer SkillEffectPlayer => _skillEffectSpawner;

        /// <summary>
        ///     指定した装備スキル一覧でエフェクトのプールを作り直します。
        /// </summary>
        /// <param name="equippedSkillIds"> 装備中スキルのID一覧です。 </param>
        public void Prewarm(IReadOnlyList<int> equippedSkillIds)
        {
            _skillEffectSpawner?.Prewarm(equippedSkillIds);
        }

        private readonly ISkillEffectSpawner _skillEffectSpawner;
    }
}
