using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     攻撃定義と攻撃処理のパイプラインを組み合わせて、攻撃処理全体を実行するクラス。
    /// </summary>
    public class AttackExecutor
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="resolver"></param>
        public AttackExecutor(IAttackPipelineResolver resolver)
        {
            _resolver = resolver;
        }

        public AttackResult Execute(CharacterEntity attacker, IHitTarget target, AttackId attackId)
        {
            // 攻撃定義を取得し、攻撃処理のパイプラインを解決する。
            AttackDefinition attackDifinition = attacker.CombatSpec.GetAttackDifinition(attackId);
            AttackPipeline pipeline = _resolver.Resolve(attackDifinition.Id);

            // 攻撃処理の文脈を作成し、パイプラインを実行する。
            AttackContext attackContext = new AttackContext(attacker, target, attackDifinition);
            AttackResult result = pipeline.Execute(attackContext);

            // ダメージをターゲットの体力に適用する。
            float nextHealth = target.Health.CurrentHealth.Value - result.FinalDamage.Value;
            target.Health.ChangeHealth(new Health(nextHealth));

            Debug.Log(
                 $"[Attack] " +
                 $"Attacker:{attacker.Name} " +
                 $"TargetHP:{target.Health.CurrentHealth.Value} " +
                 $"Damage:{result.FinalDamage.Value} " +
                 $"Critical:{result.IsCritical}");

            return result;
        }

        private readonly IAttackPipelineResolver _resolver;
    }
}
