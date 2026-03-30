using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.InGame.Battle
{
    /// <summary>
    ///     攻撃IDに対応する攻撃パイプラインを解決するクラス。
    /// </summary>
    public sealed class AttackPipelineResolver : IAttackPipelineResolver
    {
        public AttackPipelineResolver(IReadOnlyDictionary<AttackId, AttackPipeline> pipelines)
        {
            _pipelines = pipelines ?? throw new ArgumentNullException(nameof(pipelines));
        }

        /// <summary>
        ///     指定した攻撃IDに対応する攻撃パイプラインを解決して返す。
        /// </summary>
        /// <param name="attackId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public AttackPipeline Resolve(AttackId attackId)
        {
            if (_pipelines.TryGetValue(attackId, out AttackPipeline pipeline))
            {
                return pipeline;
            }

            throw new InvalidOperationException($"Pipeline not found. AttackId={attackId}");
        }

        private readonly IReadOnlyDictionary<AttackId, AttackPipeline> _pipelines;
    }
}