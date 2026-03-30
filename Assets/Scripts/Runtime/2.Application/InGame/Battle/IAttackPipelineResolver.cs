using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     攻撃IDに基づいて、対応する攻撃処理のパイプラインを解決するインターフェース。
    /// </summary>
    public interface IAttackPipelineResolver
    {
        /// <summary>
        ///     指定した攻撃IDに対応する攻撃処理のパイプラインを解決して返す。
        /// </summary>
        /// <param name="attackId"></param>
        /// <returns></returns>
        AttackPipeline Resolve(AttackId attackId);
    }
}
