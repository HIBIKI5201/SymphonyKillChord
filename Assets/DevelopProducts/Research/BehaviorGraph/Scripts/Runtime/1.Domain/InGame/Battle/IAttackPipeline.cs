using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle
{
    public interface IAttackPipeline
    {
        public AttackResult Execute(in AttackStepContext context);
    }
}
