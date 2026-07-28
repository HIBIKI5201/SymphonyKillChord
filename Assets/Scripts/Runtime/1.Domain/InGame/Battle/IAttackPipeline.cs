using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Battle
{
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
