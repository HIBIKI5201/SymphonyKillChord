using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using System.Collections.Generic;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルエフェクトの事前生成と再生を行うSpawnerの契約。
    /// </summary>
    public interface ISkillEffectSpawner : ISkillEffectPlayer
    {
        /// <summary>
        ///     装備スキルに応じたエフェクトのプールを事前生成する。
        /// </summary>
        /// <param name="equippedSkillIds"> 装備中スキルのID一覧です。 </param>
        void Prewarm(IReadOnlyList<int> equippedSkillIds);

        /// <summary>
        ///     生成済みのプールをすべて破棄する。
        /// </summary>
        void Clear();
    }
}
