using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    public interface IAttackPipeline
    {
        public AttackResult Execute(in AttackStepContext context);
    }
}
