using KillChord.Runtime.Domain.InGame.Skill;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    ///     スキル効果実行器の解決テーブルです。
    /// </summary>
    public sealed class SkillEffectExecutorResolver : ISkillEffectExecutorResolver
    {
        /// <summary>
        ///     実行器を登録します。
        /// </summary>
        /// <param name="effectType"> 効果種別です。 </param>
        /// <param name="executor"> 実行器です。 </param>
        public void Register(SkillEffectType effectType, ISkillEffectExecutor executor)
        {
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            _executorMap[effectType] = executor;
        }

        /// <summary>
        ///     指定した効果種別に対応する実行器の取得を試みます。
        /// </summary>
        /// <param name="effectType"> 効果種別です。 </param>
        /// <param name="executor"> 取得した実行器です。 </param>
        /// <returns> 取得に成功した場合はtrue。 </returns>
        public bool TryResolve(SkillEffectType effectType, out ISkillEffectExecutor executor)
        {
            return _executorMap.TryGetValue(effectType, out executor);
        }

        private readonly Dictionary<SkillEffectType, ISkillEffectExecutor> _executorMap = new();
    }
}
