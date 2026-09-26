using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃の各ステップを順に実行するパイプライン。
    /// </summary>
    public interface IAttackPipeline
    {
        /// <summary>
        ///     攻撃パイプラインを実行する。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public AttackResult Execute(in AttackStepContext context);
    }
}
